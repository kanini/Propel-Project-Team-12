using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using System.Net.Http.Headers;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for uploading and downloading documents via Supabase Storage REST API (US_042, task_002).
/// Uses HttpClient with Bearer token auth against the Supabase Storage v1 REST API.
/// </summary>
public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly PatientAccessDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SupabaseStorageService> _logger;
    private readonly string _bucketName;
    private readonly string _tempDownloadPath;
    private readonly string _supabaseUrl;

    public SupabaseStorageService(
        PatientAccessDbContext context,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SupabaseStorageService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _httpClient = httpClientFactory.CreateClient();
        _supabaseUrl = configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase:Url not configured");
        _bucketName = configuration["Supabase:StorageBucket"] ?? "propelIq";
        _tempDownloadPath = configuration["FileStorage:TempUploadPath"] ?? "./uploads/temp";

        var apiKey = configuration["Supabase:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
        }

        Directory.CreateDirectory(_tempDownloadPath);
    }

    public async Task<string> UploadDocumentAsync(string localFilePath, Guid patientId, Guid documentId, string contentType)
    {
        var extension = Path.GetExtension(localFilePath);
        var objectPath = $"documents/{patientId}/{documentId}{extension}";

        _logger.LogInformation("Uploading document {DocumentId} to Supabase bucket {Bucket} path {Path}",
            documentId, _bucketName, objectPath);

        using var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        // Supabase Storage REST API: POST /storage/v1/object/{bucket}/{path}
        var url = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{objectPath}";

        var response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Supabase upload failed. Status: {Status}, Body: {Body}", response.StatusCode, errorBody);
            throw new InvalidOperationException($"Supabase upload failed ({response.StatusCode}): {errorBody}");
        }

        _logger.LogInformation("Document {DocumentId} uploaded to Supabase successfully. Path: {Path}", documentId, objectPath);
        return objectPath;
    }

    public async Task<string> DownloadDocumentAsync(Guid documentId)
    {
        _logger.LogInformation("Downloading document {DocumentId} from Supabase storage", documentId);

        var document = await _context.ClinicalDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        var storagePath = document.StoragePath;

        // If StoragePath is a Supabase object path (starts with "documents/"), download from Supabase
        if (!string.IsNullOrEmpty(storagePath) && storagePath.StartsWith("documents/", StringComparison.OrdinalIgnoreCase))
        {
            var tempFilePath = Path.Combine(_tempDownloadPath, $"{documentId}.pdf");

            // Supabase Storage REST API: GET /storage/v1/object/{bucket}/{path}
            var url = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{storagePath}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Supabase download failed for {DocumentId}. Status: {Status}, Body: {Body}",
                    documentId, response.StatusCode, errorBody);
                throw new InvalidOperationException($"Supabase download failed ({response.StatusCode}): {errorBody}");
            }

            using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);

            _logger.LogInformation("Document {DocumentId} downloaded from Supabase path {Path} to {TempPath}",
                documentId, storagePath, tempFilePath);
            return tempFilePath;
        }

        // Fallback: try local file path for documents uploaded before Supabase migration
        _logger.LogWarning("Document {DocumentId} has local StoragePath {StoragePath}, attempting local read",
            documentId, storagePath);

        if (!string.IsNullOrEmpty(storagePath))
        {
            var normalizedPath = storagePath.Replace("/", "\\");
            if (File.Exists(normalizedPath))
            {
                var tempFilePath = Path.Combine(_tempDownloadPath, $"{documentId}.pdf");
                File.Copy(normalizedPath, tempFilePath, overwrite: true);
                return tempFilePath;
            }
        }

        throw new FileNotFoundException($"Document file not found in Supabase or locally. StoragePath: {storagePath}");
    }

    public async Task<Stream> GetDocumentStreamAsync(Guid documentId)
    {
        _logger.LogInformation("Getting document {DocumentId} stream", documentId);

        var document = await _context.ClinicalDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        var storagePath = document.StoragePath;

        // If StoragePath is a Supabase object path, stream from Supabase
        if (!string.IsNullOrEmpty(storagePath) && storagePath.StartsWith("documents/", StringComparison.OrdinalIgnoreCase))
        {
            var url = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{storagePath}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Supabase download failed ({response.StatusCode}): {errorBody}");
            }

            var memoryStream = new MemoryStream();
            await response.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        // Fallback: local file for pre-Supabase documents
        if (!string.IsNullOrEmpty(storagePath))
        {
            var normalizedPath = storagePath.Replace("/", "\\");
            if (File.Exists(normalizedPath))
            {
                return new FileStream(normalizedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        }

        throw new FileNotFoundException($"Document file not found in Supabase or locally. StoragePath: {storagePath}");
    }
}
