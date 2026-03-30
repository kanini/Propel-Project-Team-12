namespace PatientAccess.Data.Models;

/// <summary>
/// Immutable audit trail entry for compliance (DR-005, DR-007).
/// Database triggers prevent UPDATE and DELETE operations.
/// Captures all system events including authentication, authorization, and data access.
/// </summary>
public class AuditLog
{
    public Guid AuditLogId { get; set; }

    /// <summary>
    /// User ID associated with the action.
    /// Nullable to support logging of failed login attempts where user may not exist.
    /// </summary>
    public Guid? UserId { get; set; }

    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Type of action performed (e.g., "Login", "Logout", "FailedLogin").
    /// For authentication events, use values from AuditActionType enum.
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    public string ResourceType { get; set; } = string.Empty;

    public Guid? ResourceId { get; set; }

    /// <summary>
    /// JSONB column for detailed action information (metadata).
    /// For authentication events, can include: attempted email, error messages, etc.
    /// </summary>
    public string ActionDetails { get; set; } = "{}";

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    /// <summary>
    /// Result of the action: Success, Failure, PartialSuccess (AC1 - US_059).
    /// Tracks whether the audited action completed successfully.
    /// </summary>
    public string Result { get; set; } = "Success";

    // Navigation properties
    public User? User { get; set; }
}
