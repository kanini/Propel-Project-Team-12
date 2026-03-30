using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Evaluates confidence scores against configurable thresholds (AC2 - US_058, AIR-S02).
/// Supports per-data-type thresholds for fine-grained control.
/// </summary>
public class ConfidenceThresholdEvaluator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfidenceThresholdEvaluator> _logger;
    
    // Default threshold from AIR-S03: 70%
    private const decimal DefaultConfidenceThreshold = 0.7m;
    
    public ConfidenceThresholdEvaluator(
        IConfiguration configuration,
        ILogger<ConfidenceThresholdEvaluator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    /// <summary>
    /// Determines if extraction requires manual review based on confidence threshold (AC2 - US_058).
    /// Returns true if confidence score is below the configured threshold.
    /// </summary>
    /// <param name="dataType">Clinical data type (e.g., Vital, Medication, Allergy)</param>
    /// <param name="confidenceScore">AI confidence score (0.0 - 1.0)</param>
    /// <returns>True if manual review required, false otherwise</returns>
    public bool RequiresManualReview(ClinicalDataType dataType, decimal confidenceScore)
    {
        if (confidenceScore < 0 || confidenceScore > 1)
        {
            _logger.LogWarning(
                "Invalid confidence score {Score} for {DataType}. Expected 0.0-1.0. Flagging for manual review.",
                confidenceScore, dataType);
            return true;
        }
        
        var threshold = GetThresholdForDataType(dataType);
        var requiresReview = confidenceScore < threshold;
        
        if (requiresReview)
        {
            _logger.LogInformation(
                "Confidence score {Score:F3} below threshold {Threshold:F3} for {DataType}. Flagging for manual review (AC2 - US_058).",
                confidenceScore, threshold, dataType);
        }
        
        return requiresReview;
    }
    
    /// <summary>
    /// Gets confidence threshold for specific data type.
    /// Allows per-data-type thresholds (e.g., stricter for medications).
    /// Configuration hierarchy: DataType-specific > Default > Hardcoded default (0.7)
    /// </summary>
    /// <param name="dataType">Clinical data type</param>
    /// <returns>Confidence threshold (0.0 - 1.0)</returns>
    private decimal GetThresholdForDataType(ClinicalDataType dataType)
    {
        // Check for data type-specific threshold
        // Example config: AIGuardrails:ConfidenceThresholds:Medication = 0.85
        var dataTypeName = dataType.ToString();
        var specificThreshold = _configuration.GetValue<decimal?>(
            $"AIGuardrails:ConfidenceThresholds:{dataTypeName}");
        
        if (specificThreshold.HasValue)
        {
            _logger.LogDebug(
                "Using data type-specific threshold {Threshold:F3} for {DataType}.",
                specificThreshold.Value, dataType);
            return specificThreshold.Value;
        }
        
        // Fallback to general threshold
        var generalThreshold = _configuration.GetValue<decimal?>(
            "AIGuardrails:ConfidenceThresholds:Default");
        
        if (generalThreshold.HasValue)
        {
            _logger.LogDebug(
                "Using default threshold {Threshold:F3} for {DataType}.",
                generalThreshold.Value, dataType);
            return generalThreshold.Value;
        }
        
        // Final fallback to hardcoded default per AIR-S03
        _logger.LogDebug(
            "Using hardcoded default threshold {Threshold:F3} for {DataType}.",
            DefaultConfidenceThreshold, dataType);
        return DefaultConfidenceThreshold;
    }
    
    /// <summary>
    /// Gets warning level for confidence score to support UI indicators.
    /// </summary>
    /// <param name="confidenceScore">AI confidence score (0.0 - 1.0)</param>
    /// <returns>Warning level: Critical, Warning, Info, or None</returns>
    public string GetWarningLevel(decimal confidenceScore)
    {
        if (confidenceScore < 0 || confidenceScore > 1)
            return "Critical"; // Invalid score
        
        if (confidenceScore < 0.5m)
            return "Critical"; // < 50%
        
        if (confidenceScore < 0.7m)
            return "Warning"; // 50-70%
        
        if (confidenceScore < 0.9m)
            return "Info"; // 70-90%
        
        return "None"; // >= 90%
    }
}
