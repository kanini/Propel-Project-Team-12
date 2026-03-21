using PatientAccess.Data.Models;

namespace PatientAccess.Data.Repositories;

/// <summary>
/// Repository interface for immutable audit log entries (NFR-007, FR-005).
/// Supports append-only operations for compliance with audit trail requirements.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Creates a new audit log entry (append-only operation).
    /// </summary>
    /// <param name="auditLog">The audit log entry to create</param>
    /// <returns>The created audit log with generated ID</returns>
    Task<AuditLog> CreateAsync(AuditLog auditLog);

    /// <summary>
    /// Retrieves audit logs for a specific user within a time range.
    /// </summary>
    /// <param name="userId">The user ID to query</param>
    /// <param name="hours">Number of hours to look back (default: 24)</param>
    /// <returns>List of audit logs for the user</returns>
    Task<List<AuditLog>> GetByUserIdAsync(Guid userId, int hours = 24);

    /// <summary>
    /// Retrieves audit logs for a specific resource.
    /// </summary>
    /// <param name="resourceType">Type of resource (e.g., "Patient", "Appointment")</param>
    /// <param name="resourceId">ID of the resource</param>
    /// <returns>List of audit logs for the resource</returns>
    Task<List<AuditLog>> GetByResourceAsync(string resourceType, Guid resourceId);

    /// <summary>
    /// Retrieves unauthorized access attempts within a time range (NFR-014).
    /// </summary>
    /// <param name="hours">Number of hours to look back (default: 24)</param>
    /// <returns>List of unauthorized access audit logs</returns>
    Task<List<AuditLog>> GetUnauthorizedAccessAttemptsAsync(int hours = 24);
}
