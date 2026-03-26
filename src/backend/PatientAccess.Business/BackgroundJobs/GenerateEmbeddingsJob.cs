using Hangfire;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for generating embeddings for document chunks (AIR-R04, DR-010).
/// Processes pending chunks for a specified code system using Azure OpenAI text-embedding-3-small.
/// Executes with automatic retry policy: 3 attempts with exponential backoff (5min, 15min, 30min).
/// </summary>
public class GenerateEmbeddingsJob
{
    private readonly IEmbeddingGenerationService _embeddingService;
    private readonly ILogger<GenerateEmbeddingsJob> _logger;

    public GenerateEmbeddingsJob(
        IEmbeddingGenerationService embeddingService,
        ILogger<GenerateEmbeddingsJob> logger)
    {
        _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes embedding generation job for specified code system.
    /// Processes all pending DocumentChunk records and persists embeddings to appropriate table.
    /// Retry policy: 3 attempts with exponential backoff (5min, 15min, 30min).
    /// </summary>
    /// <param name="codeSystem">Code system type: "ICD10", "CPT", or "ClinicalTerminology"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 300, 900, 1800 }, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [Queue("embedding-generation")] // Dedicated queue for embedding jobs
    public async Task ExecuteAsync(string codeSystem, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting embedding generation job for {CodeSystem}", codeSystem);

        var startTime = DateTime.UtcNow;

        try
        {
            // Validate code system
            var validCodeSystems = new[] { "ICD10", "CPT", "ClinicalTerminology" };
            if (!validCodeSystems.Contains(codeSystem, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Invalid code system: {codeSystem}. Must be 'ICD10', 'CPT', or 'ClinicalTerminology'.");
            }

            // Process all pending chunks for this code system
            await _embeddingService.ProcessPendingChunksAsync(codeSystem, cancellationToken);

            var duration = (DateTime.UtcNow - startTime).TotalSeconds;
            _logger.LogInformation("Embedding generation job completed successfully for {CodeSystem} in {Duration}s",
                codeSystem, duration);
        }
        catch (ArgumentException ex)
        {
            // Invalid input - do not retry
            _logger.LogError(ex, "Embedding generation job failed due to invalid arguments for {CodeSystem}", codeSystem);
            throw;
        }
        catch (OperationCanceledException)
        {
            // Job cancelled - do not retry
            _logger.LogWarning("Embedding generation job cancelled for {CodeSystem}", codeSystem);
            throw;
        }
        catch (Exception ex)
        {
            // Transient failures - retry via Hangfire
            var duration = (DateTime.UtcNow - startTime).TotalSeconds;
            _logger.LogError(ex, "Embedding generation job failed for {CodeSystem} after {Duration}s. Error: {ErrorMessage}",
                codeSystem, duration, ex.Message);
            throw;
        }
    }
}
