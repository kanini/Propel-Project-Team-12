using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Manages staff verification workflow for AI-suggested data (AC1 - US_058, AIR-S01).
/// Handles verification state transitions and tracks staff verification actions.
/// </summary>
public class VerificationWorkflowService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<VerificationWorkflowService> _logger;
    
    public VerificationWorkflowService(
        PatientAccessDbContext context,
        ILogger<VerificationWorkflowService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    /// <summary>
    /// Records staff verification for AI extraction (AC1 - US_058).
    /// Updates verification status, timestamps, and optionally corrects values.
    /// </summary>
    /// <param name="extractionId">ID of the extraction to verify</param>
    /// <param name="staffUserId">Staff member performing verification</param>
    /// <param name="status">Verification status (Verified/ManuallyEdited/Rejected)</param>
    /// <param name="correctedValue">Corrected value if manually edited</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated extraction</returns>
    /// <exception cref="InvalidOperationException">Thrown when extraction not found</exception>
    public async Task<ExtractedClinicalData>RecordVerificationAsync(
        Guid extractionId,
        Guid staffUserId,
        VerificationStatus status,
        string? correctedValue = null,
        CancellationToken cancellationToken = default)
    {
        var extraction = await _context.ExtractedClinicalData
            .FirstOrDefaultAsync(e => e.ExtractedDataId == extractionId, cancellationToken);
        
        if (extraction == null)
        {
            throw new InvalidOperationException($"Extraction {extractionId} not found.");
        }
        
        // Prevent re-verification if already verified
        if (extraction.VerificationStatus != VerificationStatus.Pending)
        {
            _logger.LogWarning(
                "Extraction {ExtractionId} already verified with status {Status}. Skipping re-verification.",
                extractionId, extraction.VerificationStatus);
            return extraction;
        }
        
        // Update verification fields (AC1 - US_058)
        extraction.VerificationStatus = status;
        extraction.VerifiedBy = staffUserId;
        extraction.VerifiedAt = DateTime.UtcNow;
        extraction.UpdatedAt = DateTime.UtcNow;
        
        // If manually edited, update value
        if (status == VerificationStatus.ManuallyEdited && !string.IsNullOrEmpty(correctedValue))
        {
            var originalValue = extraction.DataValue;
            extraction.DataValue = correctedValue;
            
            _logger.LogInformation(
                "Staff user {StaffUserId} manually edited extraction {ExtractionId}: '{OriginalValue}' -> '{CorrectedValue}' (AC1 - US_058).",
                staffUserId, extractionId, originalValue, correctedValue);
        }
        
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation(
            "Staff user {StaffUserId} verified extraction {ExtractionId} with status {Status} (AC1 - US_058).",
            staffUserId, extractionId, status);
        
        return extraction;
    }
    
    /// <summary>
    /// Gets all extractions pending staff verification.
    /// Prioritizes entries requiring manual review and lowest confidence scores.
    /// </summary>
    /// <param name="patientId">Optional patient ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pending verifications, ordered by priority</returns>
    public async Task<List<ExtractedClinicalData>> GetPendingVerificationsAsync(
        Guid? patientId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ExtractedClinicalData
            .Where(e => e.IsAISuggested && e.VerificationStatus == VerificationStatus.Pending);
        
        if (patientId.HasValue)
        {
            query = query.Where(e => e.PatientId == patientId.Value);
        }
        
        return await query
            .Include(e => e.Document)
            .Include(e => e.Patient)
            .OrderBy(e => e.RequiresManualReview ? 0 : 1) // Priority 1: manual review required
            .ThenBy(e => e.ConfidenceScore) // Priority 2: lowest confidence first
            .ThenBy(e => e.ExtractedAt) // Priority 3: oldest first
            .ToListAsync(cancellationToken);
    }
    
    /// <summary>
    /// Gets count of pending verifications for dashboard display.
    /// </summary>
    /// <param name="patientId">Optional patient ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of pending verifications</returns>
    public async Task<int> GetPendingVerificationCountAsync(
        Guid? patientId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ExtractedClinicalData
            .Where(e => e.IsAISuggested && e.VerificationStatus == VerificationStatus.Pending);
        
        if (patientId.HasValue)
        {
            query = query.Where(e => e.PatientId == patientId.Value);
        }
        
        return await query.CountAsync(cancellationToken);
    }
    
    /// <summary>
    /// Gets count of entries requiring manual review (high priority).
    /// </summary>
    /// <param name="patientId">Optional patient ID filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of entries requiring manual review</returns>
    public async Task<int> GetManualReviewCountAsync(
        Guid? patientId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ExtractedClinicalData
            .Where(e => e.RequiresManualReview && e.VerificationStatus == VerificationStatus.Pending);
        
        if (patientId.HasValue)
        {
            query = query.Where(e => e.PatientId == patientId.Value);
        }
        
        return await query.CountAsync(cancellationToken);
    }
}
