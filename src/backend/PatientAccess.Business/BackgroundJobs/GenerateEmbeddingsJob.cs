using Hangfire;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for generating embeddings for document chunks.
/// Processes pending DocumentChunk records and populates vector tables.
/// Implements AIR-R04 (separate processing per code system).
/// </summary>
public class GenerateEmbeddingsJob
{
    private readonly ILogger<GenerateEmbeddingsJob> _logger;
    private readonly IEmbeddingGenerationService _embeddingService;

    public GenerateEmbeddingsJob(
        ILogger<GenerateEmbeddingsJob> logger,
        IEmbeddingGenerationService embeddingService)
    {
        _logger = logger;
        _embeddingService = embeddingService;
    }

    /// <summary>
    /// Executes embedding generation job for specified code system.
    /// </summary>
    /// <param name="codeSystem">Code system: "ICD10", "CPT", or "ClinicalTerminology"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 300, 900, 1800 })] // 5min, 15min, 30min
    public async Task ExecuteAsync(string codeSystem, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting embedding generation job for {CodeSystem}", codeSystem);

        try
        {
            await _embeddingService.ProcessPendingChunksAsync(codeSystem, cancellationToken);

            _logger.LogInformation("Embedding generation job completed successfully for {CodeSystem}", codeSystem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedding generation job failed for {CodeSystem}", codeSystem);
            throw;
        }
    }
}
