namespace PatientAccess.Business.Models;

/// <summary>
/// Result of entity matching comparison, indicating match type, conflicts, and manual review needs (FR-030, FR-031).
/// </summary>
public class EntityMatchResult
{
    /// <summary>
    /// Whether the entities are considered a match (duplicate or similar).
    /// </summary>
    public bool IsMatch { get; set; }

    /// <summary>
    /// Type of match detected.
    /// </summary>
    public MatchType MatchType { get; set; }

    /// <summary>
    /// Similarity score (0-100): 100 = identical, 0 = completely different.
    /// </summary>
    public int SimilarityScore { get; set; }

    /// <summary>
    /// Indicates if same entity has conflicting critical attributes (e.g., different medication doses).
    /// </summary>
    public bool HasConflict { get; set; }

    /// <summary>
    /// Description of detected conflicts requiring staff verification.
    /// </summary>
    public string? ConflictDetails { get; set; }

    /// <summary>
    /// Indicates if match is ambiguous and requires manual staff review (AIR-005).
    /// </summary>
    public bool RequiresManualReview { get; set; }

    /// <summary>
    /// IDs of source entities being compared.
    /// </summary>
    public List<Guid> SourceEntityIds { get; set; } = new();
}

/// <summary>
/// Type of entity match detected during resolution.
/// </summary>
public enum MatchType
{
    /// <summary>
    /// No match detected (similarity < 70%).
    /// </summary>
    NoMatch = 0,

    /// <summary>
    /// Exact match after normalization (100% similarity).
    /// </summary>
    ExactMatch = 1,

    /// <summary>
    /// High similarity match (>= 90% similarity).
    /// </summary>
    HighSimilarity = 2,

    /// <summary>
    /// Potential match with ambiguity (70-89% similarity), requires manual review.
    /// </summary>
    PotentialMatch = 3
}
