namespace PatientAccess.Data.Models;

/// <summary>
/// Authentication and authorization action types for audit logging (US_022).
/// Used to track user authentication events for security and compliance.
/// </summary>
public enum AuditActionType
{
    /// <summary>
    /// User successfully logged in (FR-002, AC1).
    /// </summary>
    Login = 0,

    /// <summary>
    /// User explicitly logged out.
    /// </summary>
    Logout = 1,

    /// <summary>
    /// Failed login attempt (invalid credentials or locked account) (AC4).
    /// </summary>
    FailedLogin = 2,

    /// <summary>
    /// Session expired due to 15-minute inactivity timeout (NFR-005, AC2).
    /// </summary>
    SessionTimeout = 3,

    /// <summary>
    /// New user account registered (FR-001, AC1).
    /// </summary>
    Registration = 4,

    /// <summary>
    /// User account deactivated by admin or self.
    /// </summary>
    AccountDeactivated = 5,

    /// <summary>
    /// Email verification completed (FR-001, AC2).
    /// </summary>
    EmailVerified = 6,

    /// <summary>
    /// Account locked due to excessive failed login attempts (FR-002, AC4).
    /// </summary>
    AccountLocked = 7,

    /// <summary>
    /// Password changed by user.
    /// </summary>
    PasswordChanged = 8,

    /// <summary>
    /// Password reset initiated (forgot password flow).
    /// </summary>
    PasswordResetRequested = 9,

    /// <summary>
    /// Password reset completed successfully.
    /// </summary>
    PasswordResetCompleted = 10,

    /// <summary>
    /// Password reset attempted for non-existent account.
    /// </summary>
    PasswordResetAttempt = 11,

    /// <summary>
    /// Password reset failed (invalid or expired token).
    /// </summary>
    PasswordResetFailed = 12
}
