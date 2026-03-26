using PatientAccess.Data.Models;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for chunking medical coding documents into 512-token segments (AIR-R01).
/// Processes documents separately by code system (ICD-10, CPT, clinical terminology) per AIR-R04.
/// </summary>
public interface IDocumentChunkingService
{
    /// <summary>
    /// Chunks ICD-10 coding document into 512-token segments with 64-token overlap.
    /// Preserves ICD-10 code boundaries to avoid mid-code splits.
    /// </summary>
    /// <param name="documentText">Full ICD-10 document text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of document chunks persisted to staging table</returns>
    Task<List<DocumentChunk>> ChunkICD10DocumentAsync(string documentText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Chunks CPT coding document into 512-token segments with 64-token overlap.
    /// Preserves CPT code boundaries and modifiers to avoid mid-code splits.
    /// </summary>
    /// <param name="documentText">Full CPT document text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of document chunks persisted to staging table</returns>
    Task<List<DocumentChunk>> ChunkCPTDocumentAsync(string documentText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Chunks clinical terminology document into 512-token segments with 64-token overlap.
    /// No special boundary detection required (simple segmentation).
    /// </summary>
    /// <param name="documentText">Full clinical terminology document text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of document chunks persisted to staging table</returns>
    Task<List<DocumentChunk>> ChunkClinicalTerminologyAsync(string documentText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the token count for a given text using cl100k_base encoding.
    /// Used for validation and debugging.
    /// </summary>
    /// <param name="text">Text to tokenize</param>
    /// <returns>Token count</returns>
    Task<int> GetTokenCountAsync(string text);
}
