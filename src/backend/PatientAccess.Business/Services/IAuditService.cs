namespace PatientAccess.Business.Services;

/// <summary>
/// Service interface for audit logging operations (NFR-007, FR-005, NFR-014).
/// Provides business logic for creating audit trail entries.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an unauthorized access attempt (403 Forbidden) for minimum necessary access violations (NFR-014).
    /// </summary>
    /// <param name="userId">User who attempted unauthorized access</param>
    /// <param name="resourceType">Type of resource accessed (e.g., "Patient", "Appointment")</param>
    /// <param name="resourceId">ID of the resource</param>
    /// <param name="action">Action attempted (e.g., "Read", "Update", "Delete")</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="userAgent">User agent string</param>
    /// <returns>Task representing the async operation</returns>
    Task LogUnauthorizedAccessAsync(Guid userId, string resourceType, Guid resourceId, string action, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Logs successful data access for PHI (Protected Health Information) tracking (FR-005).
    /// </summary>
    /// <param name="userId">User who accessed the data</param>
    /// <param name="resourceType">Type of resource accessed</param>
    /// <param name="resourceId">ID of the resource</param>
    /// <param name="action">Action performed (e.g., "Read", "Update", "Create", "Delete")</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="userAgent">User agent string</param>
    /// <returns>Task representing the async operation</returns>
    Task LogDataAccessAsync(Guid userId, string resourceType, Guid resourceId, string action, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Logs authentication events (login, logout, failed attempts) (FR-005).
    /// </summary>
    /// <param name="userId">User ID (for successful login/logout) or Guid.Empty for failed attempt</param>
    /// <param name="action">Authentication action (e.g., "Login", "Logout", "FailedLogin")</param>
    /// <param name="email">Email address used for authentication</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="userAgent">User agent string</param>
    /// <returns>Task representing the async operation</returns>
    Task LogAuthenticationEventAsync(Guid userId, string action, string email, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Logs successful login with user ID, timestamp, IP address, and user agent (FR-005 AC1).
    /// NON-BLOCKING: Wrapped in try-catch to prevent authentication flow disruption.
    /// </summary>
    /// <param name="userId">Authenticated user ID</param>
    /// <param name="ipAddress">Client IP address from X-Forwarded-For or RemoteIpAddress</param>
    /// <param name="userAgent">Client user agent string</param>
    /// <returns>Task representing the async operation</returns>
    Task LogSuccessfulLoginAsync(Guid userId, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Logs failed login attempt with hashed email, timestamp, IP address, user agent, and failure reason (FR-005 AC2).
    /// Email is hashed using SHA256 for privacy compliance.
    /// NON-BLOCKING: Wrapped in try-catch to prevent authentication flow disruption.
    /// </summary>
    /// <param name="emailHash">SHA256 hash of attempted email address</param>
    /// <param name="ipAddress">Client IP address</param>
    /// <param name="userAgent">Client user agent string</param>
    /// <param name="failureReason">Reason for login failure (e.g., "Invalid credentials", "Account locked")</param>
    /// <returns>Task representing the async operation</returns>
    Task LogFailedLoginAsync(string emailHash, string? ipAddress, string? userAgent, string failureReason);

    /// <summary>
    /// Logs session timeout event with user ID and last activity timestamp (FR-005 AC3).
    /// Triggered when Redis session expires after 15 minutes of inactivity.
    /// NON-BLOCKING: Wrapped in try-catch to prevent session cleanup disruption.
    /// </summary>
    /// <param name="userId">User whose session timed out</param>
    /// <param name="lastActivityTime">Timestamp of last recorded session activity</param>
    /// <returns>Task representing the async operation</returns>
    Task LogSessionTimeoutAsync(Guid userId, DateTime lastActivityTime);

    /// <summary>
    /// Logs session extension event when user refreshes session via /api/auth/refresh-session endpoint.
    /// Records user ID and timestamp of TTL refresh operation.
    /// NON-BLOCKING: Wrapped in try-catch to prevent session refresh disruption.
    /// </summary>
    /// <param name="userId">User who extended session</param>
    /// <returns>Task representing the async operation</returns>
    Task LogSessionExtensionAsync(Guid userId);
}
