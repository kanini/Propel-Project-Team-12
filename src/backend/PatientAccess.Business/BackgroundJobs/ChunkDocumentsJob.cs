using Hangfire;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for chunking medical coding documents (AIR-R01, AIR-R04).
/// Processes ICD-10, CPT, or clinical terminology documents into 512-token chunks with 64-token overlap.
/// Executes with automatic retry policy: 3 attempts with exponential backoff (1min, 5min, 15min).
/// </summary>
public class ChunkDocumentsJob
{
    private readonly IDocumentChunkingService _chunkingService;
    private readonly ILogger<ChunkDocumentsJob> _logger;

    public ChunkDocumentsJob(
        IDocumentChunkingService chunkingService,
        ILogger<ChunkDocumentsJob> logger)
    {
        _chunkingService = chunkingService ?? throw new ArgumentNullException(nameof(chunkingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes document chunking job for specified code system.
    /// Retry policy: 3 attempts with exponential backoff (1min, 5min, 15min).
    /// </summary>
    /// <param name="codeSystem">Code system type: "ICD10", "CPT", or "ClinicalTerminology"</param>
    /// <param name="sourceDocumentPath">File path or blob URI to source document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 }, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [Queue("document-chunking")] // Dedicated queue for chunking jobs
    public async Task ExecuteAsync(string codeSystem, string sourceDocumentPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting chunking job for {CodeSystem} from {Path}", codeSystem, sourceDocumentPath);

        try
        {
            // 1. Load document text from file or blob storage
            var documentText = await LoadDocumentAsync(sourceDocumentPath, cancellationToken);

            if (string.IsNullOrWhiteSpace(documentText))
            {
                _logger.LogWarning("Source document {Path} is empty, skipping chunking", sourceDocumentPath);
                return;
            }

            _logger.LogInformation("Loaded document: {Length} characters", documentText.Length);

            // 2. Chunk based on code system
            var chunks = codeSystem switch
            {
                "ICD10" => await _chunkingService.ChunkICD10DocumentAsync(documentText, cancellationToken),
                "CPT" => await _chunkingService.ChunkCPTDocumentAsync(documentText, cancellationToken),
                "ClinicalTerminology" => await _chunkingService.ChunkClinicalTerminologyAsync(documentText, cancellationToken),
                _ => throw new ArgumentException($"Invalid code system: {codeSystem}. Must be 'ICD10', 'CPT', or 'ClinicalTerminology'.")
            };

            _logger.LogInformation("Chunking job completed successfully: {ChunkCount} chunks created for {CodeSystem}",
                chunks.Count, codeSystem);
        }
        catch (ArgumentException ex)
        {
            // Invalid input - do not retry
            _logger.LogError(ex, "Chunking job failed due to invalid arguments for {CodeSystem}", codeSystem);
            throw;
        }
        catch (FileNotFoundException ex)
        {
            // File not found - do not retry
            _logger.LogError(ex, "Source document not found: {Path}", sourceDocumentPath);
            throw;
        }
        catch (Exception ex)
        {
            // Transient failures - retry via Hangfire
            _logger.LogError(ex, "Chunking job failed for {CodeSystem} from {Path}. Error: {ErrorMessage}",
                codeSystem, sourceDocumentPath, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Loads document text from file system or blob storage.
    /// Supports local file paths and blob URIs (future: integrate with ISupabaseStorageService).
    /// </summary>
    private async Task<string> LoadDocumentAsync(string sourceDocumentPath, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading document from {Path}", sourceDocumentPath);

        // Check if path is a URI (blob storage) or local file path
        if (Uri.TryCreate(sourceDocumentPath, UriKind.Absolute, out var uri) && 
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            // TODO: Implement blob storage loading via ISupabaseStorageService
            _logger.LogWarning("Blob storage loading not yet implemented, treating as file path");
        }

        // Load from local file system
        if (!File.Exists(sourceDocumentPath))
        {
            throw new FileNotFoundException($"Source document not found: {sourceDocumentPath}", sourceDocumentPath);
        }

        var documentText = await File.ReadAllTextAsync(sourceDocumentPath, cancellationToken);

        _logger.LogDebug("Document loaded successfully: {Length} characters", documentText.Length);

        return documentText;
    }
}
