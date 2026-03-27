namespace PatientAccess.Business.Configuration;

/// <summary>
/// Configuration settings for medical code mapping service.
/// Used for ICD-10 and CPT code mapping with Azure OpenAI GPT-4o (AIR-003, AIR-004).
/// </summary>
public class CodeMappingSettings
{
    /// <summary>
    /// Azure OpenAI GPT-4o deployment name for code mapping inference.
    /// Configured in Azure OpenAI Studio.
    /// Example: "gpt-4o"
    /// </summary>
    public string Gpt4oDeploymentName { get; set; } = "gpt-4o";

    /// <summary>
    /// Maximum number of code suggestions to return per request.
    /// Default: 5 (per AIR-R02)
    /// </summary>
    public int MaxSuggestions { get; set; } = 5;

    /// <summary>
    /// Confidence score difference threshold for ambiguity detection (percentage).
    /// If top 2 suggestions differ by less than this threshold, mapping is considered ambiguous.
    /// Default: 10.0% (difference < 10% indicates ambiguity)
    /// </summary>
    public decimal AmbiguityThreshold { get; set; } = 10.0m;

    /// <summary>
    /// Temperature for GPT-4o code mapping inference.
    /// Default: 0.0 (deterministic) for medical coding accuracy.
    /// </summary>
    public float Temperature { get; set; } = 0.0f;

    /// <summary>
    /// Maximum tokens for GPT-4o response.
    /// Default: 1000 (sufficient for JSON response with 5 code suggestions)
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Retry count for Azure OpenAI API failures.
    /// Default: 3 retries with exponential backoff (2s, 4s, 8s)
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Circuit breaker failure threshold.
    /// Default: 5 consecutive failures before opening circuit (1-minute break)
    /// </summary>
    public int CircuitBreakerThreshold { get; set; } = 5;
}
