using Hangfire;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for processing clinical documents (US_043).
/// Executes with automatic retry policy: 3 attempts with exponential backoff.
/// </summary>
public class DocumentProcessingJob
{
    private readonly IDocumentProcessingService _processingService;
    private readonly ILogger<DocumentProcessingJob> _logger;

    public DocumentProcessingJob(
        IDocumentProcessingService processingService,
        ILogger<DocumentProcessingJob> logger)
    {
        _processingService = processingService ?? throw new ArgumentNullException(nameof(processingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes document processing job with retry policy (AC1, AC2, AC3, AC4).
    /// Retry policy: 3 attempts with exponential backoff (1min, 5min, 15min).
    /// </summary>
    /// <param name="documentId">Document unique identifier</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 }, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [Queue("document-processing")] // Dedicated queue for document processing
    public async Task Execute(Guid documentId)
    {
        _logger.LogInformation("Executing document processing job for {DocumentId}", documentId);

        try
        {
            await _processingService.ProcessDocumentAsync(documentId);

            _logger.LogInformation("Document processing job completed successfully for {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            // Log error with correlation ID
            _logger.LogError(ex, "Document processing job failed for {DocumentId}. Error: {ErrorMessage}",
                documentId, ex.Message);

            // Hangfire will automatically retry based on [AutomaticRetry] attribute
            // On final failure, job is deleted and document remains in Failed status
            throw;
        }
    }
}
