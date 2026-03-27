namespace PatientAccess.Data.Models;

/// <summary>
/// De-duplicated patient medication entity from multiple clinical documents (FR-030).
/// Includes conflict detection for dosage mismatches across documents.
/// </summary>
public class ConsolidatedMedication
{
    public Guid Id { get; set; }

    public int PatientProfileId { get; set; }

    public string DrugName { get; set; } = string.Empty;

    public string Dosage { get; set; } = string.Empty;

    public string Frequency { get; set; } = string.Empty;

    public string? RouteOfAdministration { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Status: Active, Discontinued, Historical
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

    /// <summary>
    /// Flag for dosage conflicts requiring staff review (FR-031).
    /// </summary>
    public bool HasConflict { get; set; }

    public DateTime FirstRecordedAt { get; set; }

    public DateTime LastUpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public PatientProfile PatientProfile { get; set; } = null!;
}
