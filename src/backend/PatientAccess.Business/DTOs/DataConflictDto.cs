using PatientAccess.Data.Models.Enums;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Data conflict DTO for API responses (US_048).
/// </summary>
public class DataConflictDto
{
    public Guid Id { get; set; }
    public int PatientProfileId { get; set; }
    public string ConflictType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public ConflictSeverity Severity { get; set; }
    public List<Guid> SourceDataIds { get; set; } = new List<Guid>();
    public string ResolutionStatus { get; set; } = string.Empty;
    public Guid? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Conflict summary DTO for conflict counts and stats (US_048).
/// </summary>
public class ConflictSummaryDto
{
    public int TotalUnresolved { get; set; }
    public int CriticalCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public DateTime? OldestConflictDate { get; set; }
}

/// <summary>
/// Conflict resolution request DTO (US_048).
/// </summary>
public class ResolveConflictRequest
{
    public string Resolution { get; set; } = string.Empty;
    public Guid? ChosenEntityId { get; set; }
}
