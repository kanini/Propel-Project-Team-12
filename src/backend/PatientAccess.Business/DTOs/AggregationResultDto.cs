namespace PatientAccess.Business.DTOs;

/// <summary>
/// Result of patient data aggregation operation (FR-030, FR-032).
/// Contains statistics, completeness score, and conflict detection results.
/// </summary>
public class AggregationResultDto
{
    /// <summary>
    /// Patient ID for whom aggregation was performed.
    /// </summary>
    public Guid PatientId { get; set; }

    /// <summary>
    /// PatientProfile ID (database primary key).
    /// </summary>
    public int PatientProfileId { get; set; }

    /// <summary>
    /// Profile completeness score (0-100%).
    /// </summary>
    public decimal ProfileCompleteness { get; set; }

    /// <summary>
    /// Total number of documents processed and aggregated.
    /// </summary>
    public int TotalDocumentsProcessed { get; set; }

    /// <summary>
    /// Number of new conditions added in this aggregation.
    /// </summary>
    public int NewConditionsCount { get; set; }

    /// <summary>
    /// Number of new medications added in this aggregation.
    /// </summary>
    public int NewMedicationsCount { get; set; }

    /// <summary>
    /// Number of new allergies added in this aggregation.
    /// </summary>
    public int NewAllergiesCount { get; set; }

    /// <summary>
    /// Number of new vital trends added in this aggregation.
    /// </summary>
    public int NewVitalsCount { get; set; }

    /// <summary>
    /// Number of new encounters added in this aggregation.
    /// </summary>
    public int NewEncountersCount { get; set; }

    /// <summary>
    /// Number of conflicts detected requiring staff review (FR-031).
    /// </summary>
    public int ConflictsDetected { get; set; }

    /// <summary>
    /// Indicates if the profile has unresolved conflicts.
    /// </summary>
    public bool HasUnresolvedConflicts { get; set; }

    /// <summary>
    /// Timestamp when aggregation completed.
    /// </summary>
    public DateTime AggregatedAt { get; set; }

    /// <summary>
    /// Processing duration in milliseconds.
    /// </summary>
    public long ProcessingDurationMs { get; set; }

    /// <summary>
    /// Indicates if this was an incremental update (true) or full re-aggregation (false).
    /// </summary>
    public bool IsIncremental { get; set; }
}
