using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using System.Diagnostics;
using System.Text.Json;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for orchestrating clinical document processing (US_043, US_045).
/// Manages document status lifecycle, AI extraction coordination, and data persistence.
/// </summary>
public class DocumentProcessingService : IDocumentProcessingService
{
    private readonly PatientAccessDbContext _context;
    private readonly IPusherService _pusherService;
    private readonly IClinicalDataExtractionService _extractionService;
    private readonly ILogger<DocumentProcessingService> _logger;

    private const int ProcessingTimeoutSeconds = 60; // NFR-010: 60-second target for 10MB files
    private const decimal LowConfidenceThreshold = 50.0m;

    public DocumentProcessingService(
        PatientAccessDbContext context,
        IPusherService pusherService,
        IClinicalDataExtractionService extractionService,
        ILogger<DocumentProcessingService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pusherService = pusherService ?? throw new ArgumentNullException(nameof(pusherService));
        _extractionService = extractionService ?? throw new ArgumentNullException(nameof(extractionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes a clinical document asynchronously (US_043, US_045).
    /// Pipeline: Extract → Persist → Flag → Notify
    /// Updates status: Uploaded → Processing → ProcessingComplete/Failed.
    /// </summary>
    public async Task ProcessDocumentAsync(Guid documentId)
    {
        var stopwatch = Stopwatch.StartNew();

        // Use execution strategy to handle retries with transactions (task_003)
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

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

            // Execute AI extraction pipeline (task_002)
            _logger.LogInformation("Invoking ClinicalDataExtractionService for document {DocumentId}", documentId);
            var extractionResult = await _extractionService.ExtractClinicalDataAsync(documentId);

            // Persist extracted data points (task_003)
            _logger.LogInformation("Persisting {DataPointCount} extracted data points", extractionResult.TotalDataPoints);
            await PersistExtractedDataAsync(document, extractionResult);

            // Flag for manual review if low confidence (task_003 AC7)
            document.RequiresManualReview = extractionResult.RequiresManualReview;
            if (extractionResult.RequiresManualReview)
            {
                _logger.LogInformation("Document {DocumentId} flagged for manual review: {Count} data points below {Threshold}% confidence", 
                    documentId, extractionResult.FlaggedForReviewCount, LowConfidenceThreshold);
            }

            // Update status to Completed (task_003 AC6)
            document.ProcessingStatus = ProcessingStatus.Completed;
            document.ProcessedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            stopwatch.Stop();
            var processingTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("Document {DocumentId} processing completed in {ProcessingTimeMs}ms. Extracted {TotalDataPoints} data points, {FlaggedCount} flagged for review",
                documentId, processingTimeMs, extractionResult.TotalDataPoints, extractionResult.FlaggedForReviewCount);

            // Log warning if processing exceeds 30-second target (task_003)
            if (processingTimeMs > 30000)
            {
                _logger.LogWarning("Document {DocumentId} processing exceeded 30s target: {ProcessingTimeMs}ms",
                    documentId, processingTimeMs);
            }

            // Trigger Pusher event: extraction-complete (task_003)
            var summary = new ExtractionSummaryDto
            {
                DocumentId = documentId,
                TotalDataPoints = extractionResult.TotalDataPoints,
                FlaggedForReview = extractionResult.FlaggedForReviewCount,
                DataTypeBreakdown = extractionResult.DataTypeBreakdown,
                RequiresManualReview = extractionResult.RequiresManualReview,
                ExtractedAt = extractionResult.ExtractedAt
            };

            await TriggerExtractionCompleteEventAsync(document.PatientId, documentId, summary);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
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
        }); // End of ExecuteAsync
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
    /// Persists extracted clinical data to database with duplicate detection (task_003).
    /// </summary>
    private async Task PersistExtractedDataAsync(ClinicalDocument document, ExtractionResultDto extractionResult)
    {
        var entitiesToAdd = new List<ExtractedClinicalData>();

        foreach (var dataPoint in extractionResult.DataPoints)
        {
            // Duplicate detection (task_003 edge case)
            var exists = await _context.ExtractedClinicalData
                .AnyAsync(e => e.DocumentId == document.DocumentId
                    && e.DataType == dataPoint.DataType
                    && e.DataValue == dataPoint.DataValue
                    && e.SourcePageNumber == dataPoint.SourcePageNumber);

            if (exists)
            {
                _logger.LogWarning("Duplicate data point detected: {DataType} - {DataValue} on page {PageNumber}",
                    dataPoint.DataType, dataPoint.DataValue, dataPoint.SourcePageNumber);
                continue;
            }

            // Create entity
            var entity = new ExtractedClinicalData
            {
                ExtractedDataId = Guid.NewGuid(),
                DocumentId = document.DocumentId,
                PatientId = document.PatientId,
                DataType = dataPoint.DataType,
                DataKey = dataPoint.DataKey,
                DataValue = dataPoint.DataValue,
                ConfidenceScore = dataPoint.ConfidenceScore,
                VerificationStatus = VerificationStatus.AISuggested,
                SourcePageNumber = dataPoint.SourcePageNumber,
                SourceTextExcerpt = dataPoint.SourceTextExcerpt,
                ExtractedAt = DateTime.UtcNow,
                StructuredData = dataPoint.StructuredData != null ? JsonSerializer.Serialize(dataPoint.StructuredData) : null,
                CreatedAt = DateTime.UtcNow
            };

            entitiesToAdd.Add(entity);
        }

        // Batch insert
        if (entitiesToAdd.Any())
        {
            await _context.ExtractedClinicalData.AddRangeAsync(entitiesToAdd);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Persisted {Count} extracted clinical data entities", entitiesToAdd.Count);
        }
    }

    /// <summary>
    /// Triggers Pusher event for extraction complete with summary (task_003).
    /// </summary>
    private async Task TriggerExtractionCompleteEventAsync(Guid patientId, Guid documentId, ExtractionSummaryDto summary)
    {
        var channel = $"private-patient-{patientId}";
        var eventName = "document-extraction-complete";
        var data = new
        {
            documentId = summary.DocumentId,
            status = "ProcessingComplete",
            totalDataPoints = summary.TotalDataPoints,
            flaggedForReview = summary.FlaggedForReview,
            dataTypeBreakdown = summary.DataTypeBreakdown,
            requiresManualReview = summary.RequiresManualReview,
            extractionTimestamp = summary.ExtractedAt
        };

        var success = await _pusherService.TriggerEventAsync(channel, eventName, data);

        if (success)
        {
            _logger.LogInformation("Pusher extraction-complete event triggered for document {DocumentId}", documentId);
        }
        else
        {
            _logger.LogWarning("Failed to trigger Pusher extraction-complete event for document {DocumentId}", documentId);
        }
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
