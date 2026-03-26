namespace PatientAccess.Business.DTOs;

/// <summary>
/// Single code retrieval result with similarity scores and match type (AIR-R02, AIR-R03).
/// Returned by hybrid retrieval combining semantic and keyword search.
/// </summary>
public class CodeRetrievalResultDto
{
    /// <summary>
    /// Medical code (e.g., "E11.9" for ICD-10, "99213" for CPT).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Code description (e.g., "Type 2 diabetes mellitus without complications").
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Code category (e.g., "Endocrine, nutritional and metabolic diseases" for ICD-10).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Semantic similarity score from pgvector cosine distance (0-1).
    /// 0 = no semantic match, 1 = perfect semantic match.
    /// </summary>
    public double SimilarityScore { get; set; }

    /// <summary>
    /// Keyword match score from PostgreSQL full-text search (0-1, normalized).
    /// 0 = no keyword match, 1 = highest keyword relevance.
    /// </summary>
    public double KeywordScore { get; set; }

    /// <summary>
    /// Final hybrid score: 0.7 * SimilarityScore + 0.3 * KeywordScore.
    /// Used to rank results when combining semantic and keyword search.
    /// </summary>
    public double FinalScore { get; set; }

    /// <summary>
    /// Match type: "Semantic", "Keyword", or "Hybrid".
    /// Indicates which retrieval method(s) contributed to this result.
    /// </summary>
    public string MatchType { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata from JSONB column (e.g., synonyms, ICD-10 chapter code).
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Response DTO for code search containing results and metadata (AIR-R02, AIR-R03).
/// Includes fallback message when no confident semantic matches found.
/// </summary>
public class CodeSearchResponseDto
{
    /// <summary>
    /// Original search query.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Code system searched: "ICD10", "CPT", or "ClinicalTerminology".
    /// </summary>
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// Number of results returned (may be less than TopK if insufficient matches).
    /// </summary>
    public int ResultCount { get; set; }

    /// <summary>
    /// Ordered list of code retrieval results (descending by FinalScore).
    /// </summary>
    public List<CodeRetrievalResultDto> Results { get; set; } = new();

    /// <summary>
    /// Optional message for edge cases (e.g., "No confident matches found. Showing keyword-based results with reduced confidence.").
    /// Null when normal retrieval succeeds.
    /// </summary>
    public string? Message { get; set; }
}
