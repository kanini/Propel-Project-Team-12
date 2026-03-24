namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for document processing orchestration (US_043).
/// Handles document status lifecycle and background processing.
/// </summary>
public interface IDocumentProcessingService
{
    /// <summary>
    /// Processes a clinical document asynchronously.
    /// Updates document status through lifecycle: Processing → Completed/Failed.
    /// Broadcasts real-time status changes via Pusher Channels.
    /// </summary>
    /// <param name="documentId">Document unique identifier</param>
    /// <returns>Task completing when processing finishes</returns>
    Task ProcessDocumentAsync(Guid documentId);

    /// <summary>
    /// Updates document processing status atomically.
    /// </summary>
    /// <param name="documentId">Document unique identifier</param>
    /// <param name="status">New processing status</param>
    /// <param name="errorMessage">Optional error message for Failed status</param>
    /// <returns>Task completing when status updated</returns>
    Task UpdateDocumentStatusAsync(Guid documentId, string status, string? errorMessage = null);

    /// <summary>
    /// Retries processing for a failed document.
    /// Re-enqueues the document processing job in Hangfire.
    /// </summary>
    /// <param name="documentId">Document unique identifier</param>
    /// <returns>Task completing when retry job enqueued</returns>
    Task RetryProcessingAsync(Guid documentId);
}
