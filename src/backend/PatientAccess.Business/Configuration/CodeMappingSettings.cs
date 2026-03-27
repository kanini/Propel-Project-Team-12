namespace PatientAccess.Business.Configuration;

/// <summary>
/// Configuration settings for medical code mapping service.
/// US_051 Task 1 - Code Mapping Service.
/// Configured via appsettings.json under "CodeMapping" section.
/// </summary>
public class CodeMappingSettings
{
    /// <summary>
    /// Use Google Gemini instead of Azure OpenAI for code mapping.
    /// Default: true (using Gemini 1.5 Flash).
    /// </summary>
    public bool UseGemini { get; set; } = true;

    /// <summary>
    /// Azure Open AI GPT-4o deployment name (e.g., "gpt-4o").
    /// Legacy - only used if UseGemini=false.
    /// Used for code mapping inference with HIPAA BAA (TR-015).
    /// </summary>
    public string? Gpt4oDeploymentName { get; set; }

    /// <summary>
    /// Maximum number of code suggestions to return per request.
    /// Default: 5 per AIR-R02 (top-5 retrieval).
    /// </summary>
    public int MaxSuggestions { get; set; } = 5;

    /// <summary>
    /// Ambiguity threshold for flagging cases with similar confidence scores (percentage points).
    /// Default: 10.0 means codes with confidence difference <10% are considered ambiguous.
    /// Example: Code A = 85%, Code B = 80% → |85-80| = 5% < 10% → IsAmbiguous = true
    /// </summary>
    public decimal AmbiguityThreshold { get; set; } = 10.0m;

    /// <summary>
    /// Temperature parameter for LLM inference (0.0 = deterministic, 1.0 = creative).
    /// Default: 0.0 for deterministic medical coding (no randomness).
    /// </summary>
    public float Temperature { get; set; } = 0.0f;

    /// <summary>
    /// Maximum tokens to generate in LLM response.
    /// Default: 1000 tokens (sufficient for 5 code suggestions with rationale).
    /// </summary>
    public int MaxTokens { get; set; } = 1000;
}
