using Pgvector;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service for generating vector embeddings using Azure OpenAI text-embedding-3-small.
/// Implements DR-010 (1536-dimensional vectors) and AIR-R04 (separate indices per code system).
/// </summary>
public interface IEmbeddingGenerationService
{
    /// <summary>
    /// Generates a single 1536-dimensional vector embedding for the given text.
    /// </summary>
    /// <param name="text">Text to embed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>1536-dimensional vector embedding</returns>
    Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken);

    /// <summary>
    /// Generates embeddings for multiple texts in a single API call (batch processing).
    /// </summary>
    /// <param name="texts">List of texts to embed (max 100 per batch)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary mapping text to vector embedding</returns>
    Task<Dictionary<string, Vector>> GenerateBatchEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken);

    /// <summary>
    /// Processes all pending document chunks for the specified code system.
    /// Generates embeddings and persists to ICD10Codes, CPTCodes, or ClinicalTerminology tables.
    /// </summary>
    /// <param name="codeSystem">Code system: "ICD10", "CPT", or "ClinicalTerminology"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ProcessPendingChunksAsync(string codeSystem, CancellationToken cancellationToken);
}
