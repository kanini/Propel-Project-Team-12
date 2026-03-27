namespace PatientAccess.Data.Models;

/// <summary>
/// Aggregated 360-degree patient profile consolidating clinical data from multiple documents (FR-030, FR-032).
/// Provides a unified view of patient health with de-duplicated, verified clinical information.
/// </summary>
public class PatientProfile
{
    public int Id { get; set; }

    public Guid PatientId { get; set; }

    public DateTime LastAggregatedAt { get; set; }

    public int TotalDocumentsProcessed { get; set; }

    public bool HasUnresolvedConflicts { get; set; }

    /// <summary>
    /// Profile completeness percentage (0-100) based on data coverage.
    /// </summary>
    public decimal ProfileCompleteness { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User Patient { get; set; } = null!;
    public ICollection<ConsolidatedCondition> Conditions { get; set; } = new List<ConsolidatedCondition>();
    public ICollection<ConsolidatedMedication> Medications { get; set; } = new List<ConsolidatedMedication>();
    public ICollection<ConsolidatedAllergy> Allergies { get; set; } = new List<ConsolidatedAllergy>();
    public ICollection<VitalTrend> VitalTrends { get; set; } = new List<VitalTrend>();
    public ICollection<ConsolidatedEncounter> Encounters { get; set; } = new List<ConsolidatedEncounter>();
    public ICollection<DataConflict> Conflicts { get; set; } = new List<DataConflict>();
}
