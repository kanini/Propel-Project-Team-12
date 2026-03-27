namespace PatientAccess.Data.Models;

/// <summary>
/// De-duplicated patient allergy entity from multiple clinical documents (FR-030).
/// Tracks allergen, reaction, and severity classification.
/// </summary>
public class ConsolidatedAllergy
{
    public Guid Id { get; set; }

    public int PatientProfileId { get; set; }

    public string AllergenName { get; set; } = string.Empty;

    public string? Reaction { get; set; }

    /// <summary>
    /// Severity: Mild, Moderate, Severe, Critical
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    public DateTime? OnsetDate { get; set; }

    /// <summary>
    /// Status: Active, Inactive
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// JSONB array of source document IDs for traceability.
    /// </summary>
    public List<Guid> SourceDocumentIds { get; set; } = new List<Guid>();

    /// <summary>
    /// JSONB array of source extracted data IDs for traceability.
    /// </summary>
    public List<Guid> SourceDataIds { get; set; } = new List<Guid>();

    public bool IsDuplicate { get; set; }

    public int DuplicateCount { get; set; }

    public DateTime FirstRecordedAt { get; set; }

    public DateTime LastUpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public PatientProfile PatientProfile { get; set; } = null!;
}
