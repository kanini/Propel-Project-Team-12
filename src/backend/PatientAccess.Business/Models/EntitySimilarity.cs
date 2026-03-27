namespace PatientAccess.Business.Models;

/// <summary>
/// Detailed similarity calculation between two entities, tracking matched and conflicting fields.
/// Used for entity resolution analysis and debugging (AIR-005).
/// </summary>
public class EntitySimilarity
{
    /// <summary>
    /// Primary entity ID being compared.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Comparison entity ID.
    /// </summary>
    public Guid ComparisonEntityId { get; set; }

    /// <summary>
    /// Overall similarity score (0-100).
    /// </summary>
    public int SimilarityScore { get; set; }

    /// <summary>
    /// Names of fields that matched between entities.
    /// </summary>
    public List<string> MatchedFields { get; set; } = new();

    /// <summary>
    /// Names of fields that have conflicting values between entities.
    /// </summary>
    public List<string> ConflictingFields { get; set; } = new();

    /// <summary>
    /// Detailed breakdown of field-level similarity scores.
    /// </summary>
    public Dictionary<string, int> FieldSimilarityScores { get; set; } = new();
}
