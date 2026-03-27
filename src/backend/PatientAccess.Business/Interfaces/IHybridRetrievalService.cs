using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service for hybrid retrieval combining semantic similarity and keyword matching.
/// Implements AIR-R02 (top-5 retrieval, cosine >0.75) and AIR-R03 (hybrid retrieval).
/// </summary>
public interface IHybridRetrievalService
{
    /// <summary>
    /// Executes hybrid search with fallback logic.
    /// Returns top-K results combining semantic and keyword matching.
    /// Falls back to keyword-only if no semantic matches above threshold.
    /// </summary>
    /// <param name="request">Search request with query and parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search response with ranked results</returns>
    Task<CodeSearchResponseDto> SearchAsync(CodeSearchRequestDto request, CancellationToken cancellationToken);

    /// <summary>
    /// Executes semantic search using pgvector cosine similarity.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="topK">Number of top results</param>
    /// <param name="minThreshold">Minimum cosine similarity threshold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of semantically similar codes</returns>
    Task<List<CodeRetrievalResultDto>> SemanticSearchAsync(
        string query, 
        string codeSystem, 
        int topK, 
        double minThreshold, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Executes keyword search using PostgreSQL full-text search.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="topK">Number of top results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of keyword-matched codes</returns>
    Task<List<CodeRetrievalResultDto>> KeywordSearchAsync(
        string query, 
        string codeSystem, 
        int topK, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Executes hybrid search combining semantic and keyword results.
    /// Merges results with weighted scoring: 0.7 * semantic + 0.3 * keyword.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="topK">Number of top results</param>
    /// <param name="minThreshold">Minimum cosine similarity threshold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of hybrid-scored codes</returns>
    Task<List<CodeRetrievalResultDto>> HybridSearchAsync(
        string query, 
        string codeSystem, 
        int topK, 
        double minThreshold, 
        CancellationToken cancellationToken);
}
