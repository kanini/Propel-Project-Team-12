namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for generating embeddings using Azure OpenAI text-embedding-3-small (AIR-R04, DR-010).
/// Produces 1536-dimensional vectors for medical coding documents and stores in pgvector indices.
/// </summary>
public interface IEmbeddingGenerationService
{
    /// <summary>
    /// Generates a single 1536-dimensional embedding for the given text.
    /// Uses Azure OpenAI text-embedding-3-small model.
    /// </summary>
    /// <param name="text">Text to embed (up to 8192 tokens)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>1536-dimensional embedding vector</returns>
    Task<List<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for multiple texts in a single batch request.
    /// Supports up to 100 texts per batch for efficiency.
    /// </summary>
    /// <param name="texts">List of texts to embed (max 100)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary mapping text to embedding vector</returns>
    Task<Dictionary<string, List<float>>> GenerateBatchEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes all pending DocumentChunk records for a specified code system.
    /// Generates embeddings and persists to appropriate table (ICD10Codes, CPTCodes, ClinicalTerminology).
    /// Updates DocumentChunk.IsProcessed flag after successful processing.
    /// </summary>
    /// <param name="codeSystem">Code system: "ICD10", "CPT", or "ClinicalTerminology"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completing when all pending chunks are processed</returns>
    Task ProcessPendingChunksAsync(string codeSystem, CancellationToken cancellationToken = default);
}
