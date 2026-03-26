using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for downloading documents from Supabase Storage (task_002).
/// NOTE: This is a stub implementation. Full Supabase SDK integration requires:
/// - NuGet package: supabase-csharp
/// - Supabase project URL and API key configuration
/// </summary>
public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly PatientAccessDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SupabaseStorageService> _logger;
    private readonly string _tempDownloadPath;

    public SupabaseStorageService(
        PatientAccessDbContext context,
        IConfiguration configuration,
        ILogger<SupabaseStorageService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _tempDownloadPath = _configuration["FileStorage:TempUploadPath"] ?? "./uploads/temp";
        Directory.CreateDirectory(_tempDownloadPath);
    }

    public async Task<string> DownloadDocumentAsync(Guid documentId)
    {
        _logger.LogInformation("Downloading document {DocumentId} from Supabase Storage", documentId);

        var document = await _context.ClinicalDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        // For local storage: check if file exists at StoragePath
        var permanentPath = document.StoragePath;
        
        // Normalize path separators for Windows
        if (!string.IsNullOrEmpty(permanentPath))
        {
            permanentPath = permanentPath.Replace("/", "\\");
        }

        // Try the stored path first (works for new uploads with matching IDs)
        if (!string.IsNullOrEmpty(permanentPath) && File.Exists(permanentPath))
        {
            var tempFilePath = Path.Combine(_tempDownloadPath, $"{documentId}.pdf");
            File.Copy(permanentPath, tempFilePath, overwrite: true);

            _logger.LogInformation("Document {DocumentId} copied from {PermanentPath} to {TempPath}", 
                documentId, permanentPath, tempFilePath);
            return tempFilePath;
        }

        // Fallback for old documents with mismatched IDs: search patient directory
        _logger.LogWarning("Document {DocumentId} not found at {StoragePath}, searching patient directory", 
            documentId, permanentPath);

        var patientId = document.PatientId;
        var patientDir = Path.Combine("./uploads/documents", patientId.ToString());
        
        if (Directory.Exists(patientDir))
        {
            var files = Directory.GetFiles(patientDir, "*.pdf");
            if (files.Length > 0)
            {
                // Use the first PDF found (for old documents, there's typically only one)
                var foundFile = files[0];
                var tempFilePath = Path.Combine(_tempDownloadPath, $"{documentId}.pdf");
                File.Copy(foundFile, tempFilePath, overwrite: true);

                _logger.LogWarning("Document {DocumentId} found at {FoundPath} (different from stored path), copied to {TempPath}", 
                    documentId, foundFile, tempFilePath);
                return tempFilePath;
            }
        }

        throw new FileNotFoundException($"Document file not found. StoragePath: {permanentPath}, Patient directory: {patientDir}");
    }

    public async Task<Stream> GetDocumentStreamAsync(Guid documentId)
    {
        _logger.LogInformation("Getting document {DocumentId} stream from local storage", documentId);

        var document = await _context.ClinicalDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        // Normalize path separators for Windows
        var permanentPath = document.StoragePath;
        if (!string.IsNullOrEmpty(permanentPath))
        {
            permanentPath = permanentPath.Replace("/", "\\");
        }

        // Try stored path first
        if (!string.IsNullOrEmpty(permanentPath) && File.Exists(permanentPath))
        {
            return new FileStream(permanentPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        // Fallback: search patient directory
        var patientId = document.PatientId;
        var patientDir = Path.Combine("./uploads/documents", patientId.ToString());
        
        if (Directory.Exists(patientDir))
        {
            var files = Directory.GetFiles(patientDir, "*.pdf");
            if (files.Length > 0)
            {
                _logger.LogWarning("Document {DocumentId} found at {FoundPath} (fallback)", documentId, files[0]);
                return new FileStream(files[0], FileMode.Open, FileAccess.Read, FileShare.Read);
            }
        }

        throw new FileNotFoundException($"Document file not found. StoragePath: {permanentPath}, Patient directory: {patientDir}");
    }
}
