using PatientAccess.Data.Models;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Paged result for audit log queries.
/// </summary>
public class AuditLogQueryResult
{
    public List<AuditLogDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// DTO for audit log entries returned by query API.
/// </summary>
public class AuditLogDto
{
    public Guid AuditLogId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public DateTime Timestamp { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string ActionDetails { get; set; } = "{}";
    public string? IpAddress { get; set; }
}

/// <summary>
/// Service for creating immutable audit log entries for authentication and authorization events.
/// Implements non-blocking async logging to prevent audit failures from disrupting auth flows.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Logs an authentication event asynchronously without blocking the calling thread.
    /// </summary>
    /// <param name="userId">User ID (null for failed login attempts on non-existent users).</param>
    /// <param name="actionType">Type of authentication action (Login, FailedLogin, Registration, etc.).</param>
    /// <param name="ipAddress">IP address of the client making the request.</param>
    /// <param name="userAgent">User agent string from the HTTP request headers.</param>
    /// <param name="metadata">Additional details in JSON format (e.g., failure reason, resource accessed).</param>
    /// <returns>Task representing the async operation.</returns>
    Task LogAuthEventAsync(
        Guid? userId,
        AuditActionType actionType,
        string? ipAddress,
        string? userAgent,
        string? metadata = null);

    /// <summary>
    /// Logs a failed login attempt with hashed email for privacy compliance.
    /// </summary>
    /// <param name="email">Email address of failed login attempt (will be hashed).</param>
    /// <param name="ipAddress">IP address of the client.</param>
    /// <param name="userAgent">User agent string.</param>
    /// <param name="failureReason">Reason for login failure.</param>
    /// <returns>Task representing the async operation.</returns>
    Task LogFailedLoginAsync(
        string email,
        string? ipAddress,
        string? userAgent,
        string failureReason);

    /// <summary>
    /// Logs a session timeout event when the 15-minute TTL expires (US_022, AC3).
    /// </summary>
    /// <param name="userId">User ID whose session timed out.</param>
    /// <param name="ipAddress">Last known IP address of the client.</param>
    /// <param name="userAgent">Last known user agent string.</param>
    /// <param name="lastActivityTimestamp">Timestamp of the user's last activity before timeout.</param>
    /// <returns>Task representing the async operation.</returns>
    Task LogSessionTimeoutAsync(
        Guid userId,
        string? ipAddress,
        string? userAgent,
        DateTime? lastActivityTimestamp = null);

    /// <summary>
    /// Queries audit log entries with optional filtering for admin review.
    /// </summary>
    /// <param name="userId">Optional filter by user ID.</param>
    /// <param name="actionType">Optional filter by action type.</param>
    /// <param name="startDate">Optional start date filter.</param>
    /// <param name="endDate">Optional end date filter.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of results per page.</param>
    /// <returns>Paged audit log entries.</returns>
    Task<AuditLogQueryResult> GetAuditLogsAsync(
        Guid? userId = null,
        string? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 25);
}
