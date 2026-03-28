namespace PatientAccess.Business.Configuration;

/// <summary>
/// Configuration settings for Google Gemini AI service.
/// </summary>
public class GeminiSettings
{
    /// <summary>
    /// Maximum number of texts to process in a single batch.
    /// </summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>
    /// Maximum number of API requests allowed per minute (rate limiting).
    /// </summary>
    public int MaxRequestsPerMinute { get; set; } = 60;
}