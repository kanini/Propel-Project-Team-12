using PatientAccess.Data.Models;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service for chunking medical coding documents into 512-token segments.
/// Implements AIR-R01 (512-token chunks with 64-token overlap).
/// Supports ICD-10, CPT, and clinical terminology code systems.
/// </summary>
public interface IDocumentChunkingService
{
    /// <summary>
    /// Chunks ICD-10 document into 512-token segments with code boundary preservation.
    /// </summary>
    /// <param name="documentText">Raw ICD-10 document text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of document chunks persisted to database</returns>
    Task<List<DocumentChunk>> ChunkICD10DocumentAsync(string documentText, CancellationToken cancellationToken);

    /// <summary>
    /// Chunks CPT document into 512-token segments with code boundary preservation.
    /// </summary>
    /// <param name="documentText">Raw CPT document text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of document chunks persisted to database</returns>
    Task<List<DocumentChunk>> ChunkCPTDocumentAsync(string documentText, CancellationToken cancellationToken);

    /// <summary>
    /// Chunks clinical terminology document into 512-token segments.
    /// </summary>
    /// <param name="documentText">Raw clinical terminology text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of document chunks persisted to database</returns>
    Task<List<DocumentChunk>> ChunkClinicalTerminologyAsync(string documentText, CancellationToken cancellationToken);

    /// <summary>
    /// Gets token count for text using cl100k_base encoding (matches text-embedding-3-small).
    /// </summary>
    /// <param name="text">Text to tokenize</param>
    /// <returns>Token count</returns>
    Task<int> GetTokenCountAsync(string text);
}
