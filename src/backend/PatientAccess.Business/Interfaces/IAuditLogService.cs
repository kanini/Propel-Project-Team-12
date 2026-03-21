using PatientAccess.Data.Models;

namespace PatientAccess.Business.Interfaces;

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
}
