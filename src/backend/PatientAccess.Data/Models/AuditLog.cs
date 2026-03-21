namespace PatientAccess.Data.Models;

/// <summary>
/// Immutable audit trail entry for compliance (DR-005, DR-007).
/// Database triggers prevent UPDATE and DELETE operations.
/// </summary>
public class AuditLog
{
    public Guid AuditLogId { get; set; }

    public Guid UserId { get; set; }

    public DateTime Timestamp { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string ResourceType { get; set; } = string.Empty;

    public Guid? ResourceId { get; set; }

    /// <summary>
    /// JSONB column for detailed action information.
    /// </summary>
    public string ActionDetails { get; set; } = "{}";

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
