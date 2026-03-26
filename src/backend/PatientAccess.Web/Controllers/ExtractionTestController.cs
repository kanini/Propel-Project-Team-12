using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Test controller for AI Clinical Data Extraction workflow (US_045).
/// Provides diagnostic endpoints to validate extraction pipeline and troubleshoot issues.
/// WARNING: For testing/development only - remove or secure before production deployment.
/// </summary>
[ApiController]
[Route("api/test/extraction")]
[AllowAnonymous] // For testing only - remove in production
public class ExtractionTestController : ControllerBase
{
    private readonly PatientAccessDbContext _context;
    private readonly IClinicalDataExtractionService _extractionService;
    private readonly IDocumentProcessingService _processingService;
    private readonly ILogger<ExtractionTestController> _logger;

    public ExtractionTestController(
        PatientAccessDbContext context,
        IClinicalDataExtractionService extractionService,
        IDocumentProcessingService processingService,
        ILogger<ExtractionTestController> logger)
    {
        _context = context;
        _extractionService = extractionService;
        _processingService = processingService;
        _logger = logger;
    }

    /// <summary>
    /// Check if document exists and get its current processing status.
    /// </summary>
    [HttpGet("status/{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocumentStatus(Guid documentId)
    {
        var document = await _context.ClinicalDocuments
            .Include(d => d.Patient)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            return NotFound(new { error = "Document not found", documentId });
        }

        var extractedCount = await _context.ExtractedClinicalData
            .CountAsync(e => e.DocumentId == documentId);

        return Ok(new
        {
            documentId = document.DocumentId,
            patientId = document.PatientId,
            patientName = document.Patient?.Name ?? "Unknown",
            status = document.ProcessingStatus.ToString(),
            filePath = document.StoragePath,
            uploadedAt = document.UploadedAt,
            processedAt = document.ProcessedAt,
            extractedDataCount = extractedCount,
            requiresManualReview = document.RequiresManualReview,
            fileExists = System.IO.File.Exists(document.StoragePath)
        });
    }

    /// <summary>
    /// Manually trigger extraction for a document (bypasses Hangfire queue).
    /// </summary>
    [HttpPost("trigger/{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TriggerExtraction(Guid documentId)
    {
        try
        {
            var document = await _context.ClinicalDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == documentId);

            if (document == null)
            {
                return NotFound(new { error = "Document not found", documentId });
            }

            _logger.LogInformation("Manually triggering extraction for document {DocumentId}", documentId);

            // Call the extraction service directly
            var extractionResult = await _extractionService.ExtractClinicalDataAsync(documentId);

            // Process the result (persist data)
            await _processingService.ProcessDocumentAsync(documentId);

            return Ok(new
            {
                documentId,
                totalDataPoints = extractionResult.TotalDataPoints,
                dataTypeBreakdown = extractionResult.DataTypeBreakdown,
                flaggedForReviewCount = extractionResult.FlaggedForReviewCount,
                processingTimeMs = extractionResult.ProcessingTimeMs,
                requiresManualReview = extractionResult.RequiresManualReview,
                extractedAt = extractionResult.ExtractedAt,
                message = "Extraction completed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering extraction for document {DocumentId}", documentId);
            return StatusCode(500, new { error = ex.Message, documentId });
        }
    }

    /// <summary>
    /// Get all extracted data points for a document.
    /// </summary>
    [HttpGet("data/{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExtractedData(Guid documentId)
    {
        var documentExists = await _context.ClinicalDocuments
            .AnyAsync(d => d.DocumentId == documentId);

        if (!documentExists)
        {
            return NotFound(new { error = "Document not found", documentId });
        }

        var extractedData = await _context.ExtractedClinicalData
            .Where(e => e.DocumentId == documentId)
            .OrderBy(e => e.DataType)
            .ThenByDescending(e => e.ConfidenceScore)
            .Select(e => new
            {
                id = e.ExtractedDataId,
                dataType = e.DataType.ToString(),
                value = e.DataValue,
                confidenceScore = e.ConfidenceScore,
                verificationStatus = e.VerificationStatus.ToString(),
                sourcePageNumber = e.SourcePageNumber,
                extractedAt = e.ExtractedAt,
                structuredData = e.StructuredData,
                verifiedByUserId = e.VerifiedBy,
                verifiedAt = e.VerifiedAt
            })
            .ToListAsync();

        return Ok(extractedData);
    }

    /// <summary>
    /// Get low-confidence data points that need manual review.
    /// </summary>
    [HttpGet("low-confidence/{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLowConfidenceData(Guid documentId, [FromQuery] decimal threshold = 50)
    {
        var documentExists = await _context.ClinicalDocuments
            .AnyAsync(d => d.DocumentId == documentId);

        if (!documentExists)
        {
            return NotFound(new { error = "Document not found", documentId });
        }

        var lowConfidenceData = await _context.ExtractedClinicalData
            .Where(e => e.DocumentId == documentId && e.ConfidenceScore < threshold)
            .OrderBy(e => e.ConfidenceScore)
            .Select(e => new
            {
                id = e.ExtractedDataId,
                dataType = e.DataType.ToString(),
                value = e.DataValue,
                confidenceScore = e.ConfidenceScore,
                verificationStatus = e.VerificationStatus.ToString(),
                sourcePageNumber = e.SourcePageNumber,
                structuredData = e.StructuredData,
                needsReview = true
            })
            .ToListAsync();

        return Ok(new
        {
            documentId,
            threshold,
            lowConfidenceCount = lowConfidenceData.Count,
            items = lowConfidenceData
        });
    }

    /// <summary>
    /// Get all documents with their extraction status.
    /// </summary>
    [HttpGet("documents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDocuments([FromQuery] int limit = 50)
    {
        var documents = await _context.ClinicalDocuments
            .Include(d => d.Patient)
            .Include(d => d.ExtractedData)
            .OrderByDescending(d => d.UploadedAt)
            .Take(limit)
            .Select(d => new
            {
                documentId = d.DocumentId,
                patientId = d.PatientId,
                patientName = d.Patient.Name,
                status = d.ProcessingStatus.ToString(),
                uploadedAt = d.UploadedAt,
                processedAt = d.ProcessedAt,
                extractedDataCount = d.ExtractedData.Count(),
                requiresManualReview = d.RequiresManualReview
            })
            .ToListAsync();

        return Ok(documents);
    }

    /// <summary>
    /// Get extraction statistics across all documents.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExtractionStats()
    {
        var totalDocuments = await _context.ClinicalDocuments.CountAsync();
        var processedDocuments = await _context.ClinicalDocuments
            .CountAsync(d => d.ProcessingStatus == ProcessingStatus.Completed);
        var processingDocuments = await _context.ClinicalDocuments
            .CountAsync(d => d.ProcessingStatus == ProcessingStatus.Processing);
        var failedDocuments = await _context.ClinicalDocuments
            .CountAsync(d => d.ProcessingStatus == ProcessingStatus.Failed);
        var totalExtractedData = await _context.ExtractedClinicalData.CountAsync();
        var requiresReviewCount = await _context.ClinicalDocuments
            .CountAsync(d => d.RequiresManualReview == true);

        var avgConfidence = await _context.ExtractedClinicalData
            .AverageAsync(e => (decimal?)e.ConfidenceScore) ?? 0;

        var dataTypeBreakdown = await _context.ExtractedClinicalData
            .GroupBy(e => e.DataType)
            .Select(g => new
            {
                dataType = g.Key.ToString(),
                count = g.Count()
            })
            .ToListAsync();

        return Ok(new
        {
            totalDocuments,
            processedDocuments,
            processingDocuments,
            failedDocuments,
            totalExtractedData,
            requiresReviewCount,
            averageConfidence = Math.Round(avgConfidence, 2),
            dataTypeBreakdown
        });
    }

    /// <summary>
    /// Verify or reject an extracted data point.
    /// </summary>
    [HttpPost("verify/{extractedDataId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyData(Guid extractedDataId, [FromBody] VerifyDataRequest request)
    {
        var data = await _context.ExtractedClinicalData
            .FirstOrDefaultAsync(e => e.ExtractedDataId == extractedDataId);

        if (data == null)
        {
            return NotFound(new { error = "Extracted data not found", extractedDataId });
        }

        data.VerificationStatus = request.IsVerified 
            ? VerificationStatus.StaffVerified 
            : VerificationStatus.Rejected;
        data.VerifiedBy = request.VerifiedById;
        data.VerifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = data.ExtractedDataId,
            verificationStatus = data.VerificationStatus.ToString(),
            verifiedById = data.VerifiedBy,
            verifiedAt = data.VerifiedAt,
            message = request.IsVerified ? "Data verified successfully" : "Data rejected"
        });
    }

    /// <summary>
    /// Delete all extracted data for a document (for re-testing).
    /// </summary>
    [HttpDelete("data/{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExtractedData(Guid documentId)
    {
        var document = await _context.ClinicalDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == documentId);

        if (document == null)
        {
            return NotFound(new { error = "Document not found", documentId });
        }

        var extractedData = await _context.ExtractedClinicalData
            .Where(e => e.DocumentId == documentId)
            .ToListAsync();

        _context.ExtractedClinicalData.RemoveRange(extractedData);

        // Reset document status
        document.ProcessingStatus = ProcessingStatus.Uploaded;
        document.ProcessedAt = null;
        document.RequiresManualReview = false;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            documentId,
            deletedCount = extractedData.Count,
            message = "Extracted data deleted and document reset to Uploaded status"
        });
    }
}

/// <summary>
/// Request model for verifying extracted data.
/// </summary>
public class VerifyDataRequest
{
    public bool IsVerified { get; set; }
    public Guid? VerifiedById { get; set; }
}
