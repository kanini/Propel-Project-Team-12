namespace PatientAccess.Business.DTOs;

/// <summary>
/// Individual code retrieval result with similarity scores.
/// </summary>
public class CodeRetrievalResultDto
{
    /// <summary>
    /// Medical code (e.g., "E11.9" for ICD-10, "99213" for CPT)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Code description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Code category classification
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Cosine similarity score (0-1, semantic search)
    /// </summary>
    public double SimilarityScore { get; set; }

    /// <summary>
    /// Keyword matching score (0-1, FTS rank normalized)
    /// </summary>
    public double KeywordScore { get; set; }

    /// <summary>
    /// Final hybrid score: 0.7 * SimilarityScore + 0.3 * KeywordScore
    /// </summary>
    public double FinalScore { get; set; }

    /// <summary>
    /// Match type: "Semantic", "Keyword", or "Hybrid"
    /// </summary>
    public string MatchType { get; set; } = string.Empty;

    /// <summary>
    /// JSONB metadata (additional contextual information)
    /// </summary>
    public string Metadata { get; set; } = "{}";
}

/// <summary>
/// Response DTO for code search results.
/// Implements AIR-R02 (top-5 retrieval) and AIR-R03 (hybrid retrieval).
/// </summary>
public class CodeSearchResponseDto
{
    /// <summary>
    /// Original search query
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Code system searched
    /// </summary>
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// Number of results returned
    /// </summary>
    public int ResultCount { get; set; }

    /// <summary>
    /// List of retrieved code results
    /// </summary>
    public List<CodeRetrievalResultDto> Results { get; set; } = new();

    /// <summary>
    /// Optional message (e.g., fallback notification)
    /// </summary>
    public string? Message { get; set; }
}
