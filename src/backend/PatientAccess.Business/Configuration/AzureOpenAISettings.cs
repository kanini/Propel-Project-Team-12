namespace PatientAccess.Business.Configuration;

/// <summary>
/// Configuration settings for Azure OpenAI Service (AIR-R04, DR-010).
/// Used for embedding generation with text-embedding-3-small model.
/// </summary>
public class AzureOpenAISettings
{
    /// <summary>
    /// Azure OpenAI resource endpoint.
    /// Example: "https://your-resource.openai.azure.com/"
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure OpenAI API key for authentication.
    /// Should be stored in Azure Key Vault or User Secrets in development.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Deployment name for text-embedding-3-small model.
    /// Configured in Azure OpenAI Studio.
    /// </summary>
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// Maximum tokens per minute (TPM) quota for Azure OpenAI.
    /// Default: 120,000 TPM for standard deployments.
    /// Used for rate limiting to avoid quota exhaustion.
    /// </summary>
    public int MaxTokensPerMinute { get; set; } = 120000;

    /// <summary>
    /// Maximum requests per minute (RPM) quota for Azure OpenAI.
    /// Default: 720 RPM for standard deployments.
    /// Used for rate limiting to avoid quota exhaustion.
    /// </summary>
    public int MaxRequestsPerMinute { get; set; } = 720;

    /// <summary>
    /// Number of chunks to process per API call.
    /// Azure OpenAI supports up to 100 inputs per batch request.
    /// Default: 100 for maximum efficiency.
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
