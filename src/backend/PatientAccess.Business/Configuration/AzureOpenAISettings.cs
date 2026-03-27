namespace PatientAccess.Business.Configuration;

/// <summary>
/// Configuration settings for Azure OpenAI Service.
/// Used for text-embedding-3-small model (1536-dimensional vectors per DR-010).
/// </summary>
public class AzureOpenAISettings
{
    /// <summary>
    /// Azure OpenAI service endpoint URL
    /// Example: "https://your-resource.openai.azure.com/"
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure OpenAI API key
    /// Should be stored in Azure Key Vault in production
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Deployment name for text-embedding-3-small model
    /// Default: "text-embedding-3-small"
    /// </summary>
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// Maximum tokens per minute (Azure OpenAI quota)
    /// Default: 120,000 TPM (adjust based on your subscription)
    /// </summary>
    public int MaxTokensPerMinute { get; set; } = 120000;

    /// <summary>
    /// Maximum requests per minute (Azure OpenAI quota)
    /// Default: 720 RPM (adjust based on your subscription)
    /// </summary>
    public int MaxRequestsPerMinute { get; set; } = 720;

    /// <summary>
    /// Number of chunks to process in a single API call
    /// Default: 100 (Azure OpenAI supports up to 2048 texts per batch)
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
