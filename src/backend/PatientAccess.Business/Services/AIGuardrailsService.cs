using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// AI guardrails service enforcing human-in-the-loop and confidence thresholds (AC1, AC2 - US_058).
/// Implements AIR-S01 (human-in-the-loop) and AIR-S02 (confidence validation).
/// </summary>
public class AIGuardrailsService : IAIGuardrailsService
{
    private readonly ConfidenceThresholdEvaluator _thresholdEvaluator;
    private readonly VerificationWorkflowService _verificationWorkflow;
    private readonly ILogger<AIGuardrailsService> _logger;
    
    public AIGuardrailsService(
        ConfidenceThresholdEvaluator thresholdEvaluator,
        VerificationWorkflowService verificationWorkflow,
        ILogger<AIGuardrailsService> logger)
    {
        _thresholdEvaluator = thresholdEvaluator;
        _verificationWorkflow = verificationWorkflow;
        _logger = logger;
    }
    
    /// <summary>
    /// Evaluates AI extraction confidence and auto-flags for manual review (AC2 - US_058).
    /// Sets IsAISuggested=true, evaluates ConfidenceScore, and sets RequiresManualReview flag.
    /// </summary>
    /// <param name="extraction">The AI extraction to evaluate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The evaluated extraction with updated guardrail flags</returns>
    public Task<ExtractedClinicalData> EvaluateAndFlagAsync(
        ExtractedClinicalData extraction,
        CancellationToken cancellationToken = default)
    {
        // Set AI-suggested flag (AC1 - AIR-S01)
        extraction.IsAISuggested = true;
        
        // Evaluate confidence threshold (AC2 - AIR-S02)
        var requiresReview = _thresholdEvaluator.RequiresManualReview(
            extraction.DataType,
            extraction.ConfidenceScore);
        
        extraction.RequiresManualReview = requiresReview;
        
        // Set initial verification status to Pending (AC1)
        extraction.VerificationStatus = VerificationStatus.Pending;
        
        // Log guardrail evaluation
        var warningLevel = _thresholdEvaluator.GetWarningLevel(extraction.ConfidenceScore);
        
        _logger.LogInformation(
            "AI extraction evaluated: DataType={DataType}, Confidence={Confidence:F3}, " +
            "RequiresManualReview={RequiresReview}, WarningLevel={WarningLevel} (AC2 - US_058)",
            extraction.DataType,
            extraction.ConfidenceScore,
            requiresReview,
            warningLevel);
        
        return Task.FromResult(extraction);
    }
    
    /// <summary>
    /// Validates that AI-suggested data cannot be committed without verification (AC1 - US_058).
    /// Throws InvalidOperationException if AI-suggested data is unverified.
    /// </summary>
    /// <param name="extraction">The extraction to validate</param>
    /// <exception cref="InvalidOperationException">Thrown when AI-suggested data is unverified</exception>
    public void ValidateBeforeCommit(ExtractedClinicalData extraction)
    {
        // Check if AI-suggested data is unverified (AC1 - AIR-S01)
        if (extraction.IsAISuggested && extraction.VerificationStatus == VerificationStatus.Pending)
        {
            _logger.LogWarning(
                "Validation failed: AI-suggested extraction {ExtractionId} cannot be committed without verification (AC1 - US_058).",
                extraction.ExtractedDataId);
            
            throw new InvalidOperationException(
                $"AI-suggested data (ID: {extraction.ExtractedDataId}) cannot be committed to patient record " +
                $"without explicit staff verification (AC1 - US_058, AIR-S01). " +
                $"Current status: {extraction.VerificationStatus}. " +
                $"Required action: Staff must verify, edit, or reject this extraction before commit.");
        }
        
        _logger.LogDebug(
            "Validation passed: Extraction {ExtractionId} verified by staff {StaffId} with status {Status} (AC1 - US_058).",
            extraction.ExtractedDataId,
            extraction.VerifiedBy,
            extraction.VerificationStatus);
    }
    
    /// <summary>
    /// Records staff verification for AI-suggested data (AC1 - US_058).
    /// Updates verification status, timestamps, and optionally corrects values.
    /// </summary>
    /// <param name="extractionId">ID of the extraction to verify</param>
    /// <param name="staffUserId">Staff member performing verification</param>
    /// <param name="status">Verification status (Verified/ManuallyEdited/Rejected)</param>
    /// <param name="correctedValue">Corrected value if manually edited</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated extraction</returns>
    /// <exception cref="InvalidOperationException">Thrown when extraction not found</exception>
    public async Task<ExtractedClinicalData> RecordVerificationAsync(
        Guid extractionId,
        Guid staffUserId,
        VerificationStatus status,
        string? correctedValue = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Recording verification: ExtractionId={ExtractionId}, StaffUserId={StaffUserId}, Status={Status} (AC1 - US_058)",
            extractionId, staffUserId, status);
        
        return await _verificationWorkflow.RecordVerificationAsync(
            extractionId,
            staffUserId,
            status,
            correctedValue,
            cancellationToken);
    }
}
