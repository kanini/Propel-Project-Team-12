using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using System.Diagnostics;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for orchestrating clinical document processing (US_043).
/// Manages document status lifecycle and AI extraction coordination.
/// </summary>
public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly PatientAccessDbContext _context;
    private readonly IPusherService _pusherService;
    private readonly ILogger<DocumentProcessingService> _logger;

    private const int ProcessingTimeoutSeconds = 60; // NFR-010: 60-second target for 10MB files

    public DocumentProcessingService(
        PatientAccessDbContext context,
        IPusherService pusherService,
        ILogger<DocumentProcessingService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pusherService = pusherService ?? throw new ArgumentNullException(nameof(pusherService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes a clinical document asynchronously (AC2, AC3).
    /// Updates status: Uploaded → Processing → Completed/Failed.
    /// Broadcasts Pusher events for real-time status tracking.
    /// </summary>
    public async Task ProcessDocumentAsync(Guid documentId)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting document processing for {DocumentId}", documentId);

            // Load document entity
            var document = await _context.ClinicalDocuments
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);

            if (document == null)
            {
                _logger.LogError("Document {DocumentId} not found", documentId);
                throw new InvalidOperationException($"Document {documentId} not found");
            }

            // Update status to Processing (AC2)
            document.ProcessingStatus = ProcessingStatus.Processing;
            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Document {DocumentId} status updated to Processing", documentId);

            // Trigger Pusher event: processing-started (AC2)
            await TriggerProcessingStartedEventAsync(documentId, document.PatientId, document.FileName);

            // Execute placeholder processing logic (EP-006-II will add AI extraction)
            // Simulate processing delay for testing
            await Task.Delay(100); // Simulate minimal processing time

            // Placeholder: In EP-006-II, this will invoke AI extraction service
            _logger.LogInformation("Document {DocumentId} processing placeholder executed (AI extraction pending EP-006-II)", documentId);

            // Update status to Completed (AC3)
            document.ProcessingStatus = ProcessingStatus.Completed;
            document.ProcessedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            stopwatch.Stop();
            var processingTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("Document {DocumentId} processing completed in {ProcessingTimeMs}ms", documentId, processingTimeMs);

            // Log warning if processing exceeds 60-second target (NFR-010)
            if (processingTimeMs > ProcessingTimeoutSeconds * 1000)
            {
                _logger.LogWarning("Document {DocumentId} processing exceeded {TimeoutSeconds}s target: {ProcessingTimeMs}ms",
                    documentId, ProcessingTimeoutSeconds, processingTimeMs);
            }

            // Trigger Pusher event: processing-completed (AC3)
            await TriggerProcessingCompletedEventAsync(documentId, document.PatientId, document.FileName, processingTimeMs);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "Document {DocumentId} processing failed after {ProcessingTimeMs}ms. Error: {ErrorMessage}",
                documentId, stopwatch.ElapsedMilliseconds, ex.Message);

            // Update status to Failed (AC4)
            await UpdateDocumentStatusAsync(documentId, "Failed", ex.Message);

            // Trigger Pusher event: processing-failed (AC4)
            var document = await _context.ClinicalDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);

            if (document != null)
            {
                await TriggerProcessingFailedEventAsync(documentId, document.PatientId, document.FileName, ex.Message);
            }

            throw; // Re-throw to trigger Hangfire retry
        }
    }

    /// <summary>
    /// Updates document status atomically.
    /// </summary>
    public async Task UpdateDocumentStatusAsync(Guid documentId, string status, string? errorMessage = null)
    {
        var document = await _context.ClinicalDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            _logger.LogError("Document {DocumentId} not found for status update", documentId);
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        // Parse status string to enum
        if (!Enum.TryParse<ProcessingStatus>(status, true, out var statusEnum))
        {
            throw new ArgumentException($"Invalid processing status: {status}", nameof(status));
        }

        document.ProcessingStatus = statusEnum;
        document.ErrorMessage = errorMessage;
        document.UpdatedAt = DateTime.UtcNow;

        if (statusEnum == ProcessingStatus.Completed || statusEnum == ProcessingStatus.Failed)
        {
            document.ProcessedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Document {DocumentId} status updated to {Status}", documentId, status);
    }

    /// <summary>
    /// Retries processing for a failed document (US_044).
    /// </summary>
    public async Task RetryProcessingAsync(Guid documentId)
    {
        var document = await _context.ClinicalDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            throw new InvalidOperationException($"Document {documentId} not found");
        }

        if (document.ProcessingStatus != ProcessingStatus.Failed)
        {
            throw new InvalidOperationException($"Document {documentId} is not in Failed status (current: {document.ProcessingStatus})");
        }

        // Reset status to Uploaded for retry
        document.ProcessingStatus = ProcessingStatus.Uploaded;
        document.ErrorMessage = null;
        document.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Document {DocumentId} reset to Uploaded status for retry", documentId);

        // Re-enqueue background job
        Hangfire.BackgroundJob.Enqueue<BackgroundJobs.DocumentProcessingJob>(job => job.Execute(documentId));

        _logger.LogInformation("Document {DocumentId} retry job enqueued in Hangfire", documentId);
    }

    /// <summary>
    /// Triggers Pusher event for processing started.
    /// </summary>
    private async Task TriggerProcessingStartedEventAsync(Guid documentId, Guid patientId, string fileName)
    {
        var channel = $"patient-{patientId}-documents";
        var eventName = "processing-started";
        var data = new
        {
            documentId,
            fileName,
            status = "Processing",
            timestamp = DateTime.UtcNow
        };

        var success = await _pusherService.TriggerEventAsync(channel, eventName, data);

        if (success)
        {
            _logger.LogInformation("Pusher event {EventName} triggered for document {DocumentId}", eventName, documentId);
        }
        else
        {
            _logger.LogWarning("Failed to trigger Pusher event {EventName} for document {DocumentId}", eventName, documentId);
        }
    }

    /// <summary>
    /// Triggers Pusher event for processing completed.
    /// </summary>
    private async Task TriggerProcessingCompletedEventAsync(Guid documentId, Guid patientId, string fileName, long processingTimeMs)
    {
        var channel = $"patient-{patientId}-documents";
        var eventName = "processing-completed";
        var data = new
        {
            documentId,
            fileName,
            status = "Completed",
            processingTimeMs,
            timestamp = DateTime.UtcNow
        };

        var success = await _pusherService.TriggerEventAsync(channel, eventName, data);

        if (success)
        {
            _logger.LogInformation("Pusher event {EventName} triggered for document {DocumentId}", eventName, documentId);
        }
        else
        {
            _logger.LogWarning("Failed to trigger Pusher event {EventName} for document {DocumentId}", eventName, documentId);
        }
    }

    /// <summary>
    /// Triggers Pusher event for processing failed.
    /// </summary>
    private async Task TriggerProcessingFailedEventAsync(Guid documentId, Guid patientId, string fileName, string errorMessage)
    {
        var channel = $"patient-{patientId}-documents";
        var eventName = "processing-failed";
        var data = new
        {
            documentId,
            fileName,
            status = "Failed",
            errorMessage,
            timestamp = DateTime.UtcNow
        };

        var success = await _pusherService.TriggerEventAsync(channel, eventName, data);

        if (success)
        {
            _logger.LogInformation("Pusher event {EventName} triggered for document {DocumentId}", eventName, documentId);
        }
        else
        {
            _logger.LogWarning("Failed to trigger Pusher event {EventName} for document {DocumentId}", eventName, documentId);
        }
    }
}
