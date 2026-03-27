namespace PatientAccess.Data.Models;

/// <summary>
/// De-duplicated patient encounter/visit entity from multiple clinical documents (FR-030).
/// Tracks healthcare visit details with encounter type classification.
/// </summary>
public class ConsolidatedEncounter
{
    public Guid Id { get; set; }

    public int PatientProfileId { get; set; }

    public DateTime EncounterDate { get; set; }

    /// <summary>
    /// Encounter type: Inpatient, Outpatient, Emergency, Telehealth
    /// </summary>
    public string EncounterType { get; set; } = string.Empty;

    public string? Provider { get; set; }

    public string? Facility { get; set; }

    public string? ChiefComplaint { get; set; }

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

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public PatientProfile PatientProfile { get; set; } = null!;
}
