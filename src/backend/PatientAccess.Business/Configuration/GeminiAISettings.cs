namespace PatientAccess.Business.Configuration;

/// <summary>
/// Configuration settings for Google Gemini AI Service.
/// Used for: 1) Document OCR (US_045), 2) Medical Code Mapping - EP-008 (US_051).
/// NOTE: Gemini does NOT provide embedding APIs - embeddings still require Azure OpenAI or alternative.
/// </summary>
public class GeminiAISettings
{
    /// <summary>
    /// Gemini API key from Google AI Studio
    /// Free tier: 15 RPM, 1M TPM, 1500 RPD
    /// Should be stored in Azure Key Vault in production
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gemini API endpoint URL
    /// Default: "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent"
    /// </summary>
    public string ApiEndpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent";

    /// <summary>
    /// Gemini model name
    /// Options: "gemini-1.5-flash-latest" (fast), "gemini-1.5-pro-latest" (advanced)
    /// Default: "gemini-1.5-flash-latest"
    /// </summary>
    public string ModelName { get; set; } = "gemini-1.5-flash-latest";

    /// <summary>
    /// Maximum output tokens for Gemini response
    /// Default: 8000 (Gemini 1.5 Flash supports up to 8192 output tokens)
    /// </summary>
    public int MaxTokens { get; set; } = 8000;

    /// <summary>
    /// Temperature for Gemini inference (0.0 = deterministic, 1.0 = creative)
    /// For medical coding: 0.0 (deterministic, consistent results)
    /// Default: 0.0
    /// </summary>
    public float Temperature { get; set; } = 0.0f;

    /// <summary>
    /// Maximum requests per minute (Gemini free tier quota)
    /// Free tier: 15 RPM
    /// Default: 15
    /// </summary>
    public int MaxRequestsPerMinute { get; set; } = 15;

    /// <summary>
    /// HTTP client timeout in seconds
    /// Default: 30 seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
