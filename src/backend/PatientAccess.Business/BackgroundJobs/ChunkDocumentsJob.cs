using Hangfire;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for processing document chunking asynchronously.
/// Implements AIR-R01 (512-token chunks with 64-token overlap).
/// Supports independent re-indexing without affecting live queries.
/// </summary>
public class ChunkDocumentsJob
{
    private readonly ILogger<ChunkDocumentsJob> _logger;
    private readonly IDocumentChunkingService _chunkingService;

    public ChunkDocumentsJob(
        ILogger<ChunkDocumentsJob> logger,
        IDocumentChunkingService chunkingService)
    {
        _logger = logger;
        _chunkingService = chunkingService;
    }

    /// <summary>
    /// Executes chunking job for specified code system.
    /// </summary>
    /// <param name="codeSystem">Code system: "ICD10", "CPT", or "ClinicalTerminology"</param>
    /// <param name="documentText">Document text to chunk</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task ExecuteAsync(string codeSystem, string documentText, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting chunking job for {CodeSystem} ({Length} chars)",
            codeSystem,
            documentText.Length);

        try
        {
            var chunks = codeSystem switch
            {
                "ICD10" => await _chunkingService.ChunkICD10DocumentAsync(documentText, cancellationToken),
                "CPT" => await _chunkingService.ChunkCPTDocumentAsync(documentText, cancellationToken),
                "ClinicalTerminology" => await _chunkingService.ChunkClinicalTerminologyAsync(documentText, cancellationToken),
                _ => throw new ArgumentException($"Invalid code system: {codeSystem}", nameof(codeSystem))
            };

            _logger.LogInformation("Chunking job completed successfully: {ChunkCount} chunks created for {CodeSystem}",
                chunks.Count,
                codeSystem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chunking job failed for {CodeSystem}", codeSystem);
            throw;
        }
    }

    /// <summary>
    /// Executes chunking job from file path (for file-based document sources).
    /// </summary>
    /// <param name="codeSystem">Code system: "ICD10", "CPT", or "ClinicalTerminology"</param>
    /// <param name="sourceFilePath">File path to document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
    public async Task ExecuteFromFileAsync(string codeSystem, string sourceFilePath, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting chunking job for {CodeSystem} from file: {FilePath}",
            codeSystem,
            sourceFilePath);

        try
        {
            // Load document text from file
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException($"Source document file not found: {sourceFilePath}");
            }

            var documentText = await File.ReadAllTextAsync(sourceFilePath, cancellationToken);

            await ExecuteAsync(codeSystem, documentText, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chunking job failed for {CodeSystem} from file {FilePath}",
                codeSystem,
                sourceFilePath);
            throw;
        }
    }
}
