using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for hybrid retrieval combining semantic similarity with keyword matching (AIR-R02, AIR-R03).
/// Implements top-5 retrieval with cosine similarity above 0.75 threshold across separate code system indices (AIR-R04).
/// </summary>
public interface IHybridRetrievalService
{
    /// <summary>
    /// Executes hybrid search (semantic + keyword) with fallback logic.
    /// If no semantic matches above threshold, falls back to keyword-only search.
    /// </summary>
    /// <param name="request">Search request with query, code system, topK, and threshold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search response with results and optional fallback message</returns>
    Task<CodeSearchResponseDto> SearchAsync(CodeSearchRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes semantic search using pgvector cosine similarity.
    /// Generates query embedding and retrieves top-K results above similarity threshold.
    /// </summary>
    /// <param name="query">Search query text</param>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="topK">Number of results to return</param>
    /// <param name="minThreshold">Minimum cosine similarity threshold (0-1)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of results ordered by semantic similarity score (descending)</returns>
    Task<List<CodeRetrievalResultDto>> SemanticSearchAsync(
        string query, 
        string codeSystem, 
        int topK, 
        double minThreshold, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes keyword search using PostgreSQL full-text search (FTS).
    /// Returns top-K results ranked by FTS relevance score.
    /// </summary>
    /// <param name="query">Search query text</param>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="topK">Number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of results ordered by keyword relevance score (descending)</returns>
    Task<List<CodeRetrievalResultDto>> KeywordSearchAsync(
        string query, 
        string codeSystem, 
        int topK, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes hybrid search combining semantic and keyword signals.
    /// Final score = 0.7 * semantic_score + 0.3 * keyword_score.
    /// </summary>
    /// <param name="query">Search query text</param>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="topK">Number of results to return</param>
    /// <param name="minThreshold">Minimum semantic similarity threshold (0-1)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of results ordered by hybrid score (descending)</returns>
    Task<List<CodeRetrievalResultDto>> HybridSearchAsync(
        string query, 
        string codeSystem, 
        int topK, 
        double minThreshold, 
        CancellationToken cancellationToken = default);
}
