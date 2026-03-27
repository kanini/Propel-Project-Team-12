namespace PatientAccess.Data.Models;

using PatientAccess.Data.Models.Enums;

/// <summary>
/// Data conflict tracking entity for mismatches requiring staff verification (FR-031).
/// Enables conflict resolution workflow for inconsistent clinical data.
/// </summary>
public class DataConflict
{
    public Guid Id { get; set; }

    public int PatientProfileId { get; set; }

    /// <summary>
    /// Conflict type: MedicationDosageMismatch, DiagnosisMismatch, AllergyMismatch
    /// </summary>
    public string ConflictType { get; set; } = string.Empty;

    /// <summary>
    /// Entity type: Medication, Condition, Allergy
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Polymorphic FK to consolidated entities (ConsolidatedMedication.Id, ConsolidatedCondition.Id, etc.)
    /// </summary>
    public Guid EntityId { get; set; }

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Severity level for prioritization (Critical, Warning, Info).
    /// </summary>
    public ConflictSeverity Severity { get; set; } = ConflictSeverity.Info;

    /// <summary>
    /// JSONB array of conflicting ExtractedClinicalData IDs.
    /// </summary>
    public List<Guid> SourceDataIds { get; set; } = new List<Guid>();

    /// <summary>
    /// Resolution status: Unresolved, Resolved, Dismissed
    /// </summary>
    public string ResolutionStatus { get; set; } = string.Empty;

    public Guid? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public PatientProfile PatientProfile { get; set; } = null!;
    public User? Resolver { get; set; }
}
