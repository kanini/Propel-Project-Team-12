namespace PatientAccess.Data.Models;

/// <summary>
/// De-duplicated patient condition/diagnosis entity from multiple clinical documents (FR-030).
/// Supports temporal tracking to preserve older data without overwriting newer records.
/// </summary>
public class ConsolidatedCondition
{
    public Guid Id { get; set; }

    public int PatientProfileId { get; set; }

    public string ConditionName { get; set; } = string.Empty;

    public string? ICD10Code { get; set; }

    public DateTime? DiagnosisDate { get; set; }

    /// <summary>
    /// Status: Active, Resolved, Historical
    /// </summary>
    public string Status { get; set; } = string.Empty;

    public string? Severity { get; set; }

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
