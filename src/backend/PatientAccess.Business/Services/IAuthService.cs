using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Services;

/// <summary>
/// Authentication service interface for user registration and verification (FR-001).
/// Handles account creation, email verification, and account activation workflows.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account with email verification workflow (FR-001, AC1).
    /// Creates user with 'Pending' status and sends verification email.
    /// </summary>
    /// <param name="request">User registration data</param>
    /// <param name="ipAddress">Client IP address for audit logging</param>
    /// <param name="userAgent">Client user agent for audit logging</param>
    /// <returns>Registration response with userId and status</returns>
    /// <exception cref="InvalidOperationException">Thrown when email already exists</exception>
    Task<RegisterUserResponseDto> RegisterUserAsync(RegisterUserRequestDto request, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Verifies user email using verification token (FR-001, AC2).
    /// Activates account by changing status from 'Pending' to 'Active'.
    /// </summary>
    /// <param name="token">Verification token from email link</param>
    /// <param name="ipAddress">Client IP address for audit logging</param>
    /// <param name="userAgent">Client user agent for audit logging</param>
    /// <returns>True if verification successful, false otherwise</returns>
    Task<bool> VerifyEmailAsync(string token, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Checks if email address is already registered.
    /// </summary>
    /// <param name="email">Email address to check</param>
    /// <returns>True if email exists, false otherwise</returns>
    Task<bool> IsEmailRegisteredAsync(string email);

    /// <summary>
    /// Authenticates user credentials and generates JWT session token (FR-002, AC1).
    /// Validates credentials, checks account status and lockout, generates JWT with role claims,
    /// stores session in Redis with 15-minute TTL.
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="ipAddress">Client IP address for audit logging</param>
    /// <param name="userAgent">Client user agent for audit logging</param>
    /// <returns>Login response with JWT token and user information</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid or account is locked</exception>
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Refreshes the session TTL for an authenticated user (US_022, AC5).
    /// Resets the 15-minute Redis TTL and returns a new token expiration time.
    /// </summary>
    /// <param name="userId">Authenticated user's ID.</param>
    /// <param name="ipAddress">Client IP address for audit context.</param>
    /// <param name="userAgent">Client user agent for audit context.</param>
    /// <returns>New token expiration timestamp.</returns>
    Task<SessionRefreshResponseDto> RefreshSessionAsync(string userId, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Logs a session timeout event when the frontend detects auto-logout (US_022, AC3).
    /// </summary>
    /// <param name="userId">User whose session timed out.</param>
    /// <param name="ipAddress">Client IP address.</param>
    /// <param name="userAgent">Client user agent.</param>
    /// <param name="lastActivityTimestamp">Last recorded activity timestamp from the client.</param>
    Task LogSessionTimeoutAsync(string userId, string? ipAddress = null, string? userAgent = null, DateTime? lastActivityTimestamp = null);
}
