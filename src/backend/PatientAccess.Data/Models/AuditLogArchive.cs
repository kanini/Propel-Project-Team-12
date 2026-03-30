namespace PatientAccess.Data.Models;

/// <summary>
/// Archive table for audit logs older than retention period (DR-007, US_059 Edge Case).
/// Supports 7-year HIPAA retention requirement without deleting data.
/// Original logs can be moved to cold storage while maintaining immutability guarantees.
/// </summary>
public class AuditLogArchive
{
    /// <summary>
    /// Original audit log ID from AuditLog table (immutable).
    /// Preserves referential integrity for compliance reporting.
    /// </summary>
    public Guid AuditLogId { get; set; }

    /// <summary>
    /// User ID who performed the action (nullable for system actions).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Timestamp when the action occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Type of action performed (e.g., "Login", "Logout", "DataAccess").
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Resource type accessed (e.g., "Patient", "ClinicalDocument").
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// Resource ID (nullable for non-resource actions like Login).
    /// </summary>
    public Guid? ResourceId { get; set; }

    /// <summary>
    /// JSONB column for detailed action information.
    /// </summary>
    public string ActionDetails { get; set; } = "{}";

    /// <summary>
    /// IP address of the client.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string from HTTP request headers.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Timestamp when this entry was archived to cold storage (UTC).
    /// Tracks when the entry was moved from AuditLog to AuditLogArchive.
    /// </summary>
    public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Reason for archiving (e.g., "Retention policy: >90 days").
    /// </summary>
    public string ArchiveReason { get; set; } = "Retention policy";

    // Navigation properties
    public User? User { get; set; }
}
