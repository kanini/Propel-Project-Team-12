using PatientAccess.Data.Models;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// AI guardrails service for confidence threshold evaluation and human-in-the-loop enforcement (AC1, AC2 - US_058).
/// Implements AIR-S01 (human-in-the-loop) and AIR-S02 (confidence thresholds).
/// </summary>
public interface IAIGuardrailsService
{
    /// <summary>
    /// Evaluates AI extraction confidence and auto-flags for manual review if below threshold (AC2 - US_058).
    /// Sets IsAISuggested=true, evaluates ConfidenceScore, and sets RequiresManualReview flag.
    /// </summary>
    /// <param name="extraction">The AI extraction to evaluate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The evaluated extraction with updated flags</returns>
    Task<ExtractedClinicalData> EvaluateAndFlagAsync(
        ExtractedClinicalData extraction,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates that AI-suggested data cannot be committed without staff verification (AC1 - US_058).
    /// Throws InvalidOperationException if validation fails.
    /// </summary>
    /// <param name="extraction">The extraction to validate</param>
    /// <exception cref="InvalidOperationException">Thrown when AI-suggested data is unverified</exception>
    void ValidateBeforeCommit(ExtractedClinicalData extraction);
    
    /// <summary>
    /// Records staff verification for AI-suggested data (AC1 - US_058).
    /// Updates VerificationStatus, VerifiedBy, and VerifiedAt fields.
    /// </summary>
    /// <param name="extractionId">ID of the extraction to verify</param>
    /// <param name="staffUserId">Staff member performing verification</param>
    /// <param name="status">Verification status (Verified/ManuallyEdited/Rejected)</param>
    /// <param name="correctedValue">Corrected value if manually edited</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated extraction</returns>
    Task<ExtractedClinicalData> RecordVerificationAsync(
        Guid extractionId,
        Guid staffUserId,
        VerificationStatus status,
        string? correctedValue = null,
        CancellationToken cancellationToken = default);
}
