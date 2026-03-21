using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Services;

/// <summary>
/// User service interface for user management operations (FR-001).
/// Handles user registration, email verification, and authentication.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Registers a new patient user with email verification.
    /// Generates verification token and sends email.
    /// </summary>
    /// <param name="request">Registration request with user details</param>
    /// <returns>Registration response with user ID and message</returns>
    /// <exception cref="InvalidOperationException">If email already registered</exception>
    Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request);

    /// <summary>
    /// Verifies user email using verification token.
    /// Activates user account if token valid and not expired.
    /// </summary>
    /// <param name="request">Verification request with token</param>
    /// <returns>True if verification successful, false otherwise</returns>
    Task<bool> VerifyEmailAsync(VerifyEmailRequest request);

    /// <summary>
    /// Authenticates user with email and password (FR-002).
    /// Implements account lockout after 5 failed attempts.
    /// Returns null if authentication fails.
    /// </summary>
    /// <param name="request">Login request with email and password</param>
    /// <returns>Login response with JWT token and role, or null if authentication fails</returns>
    Task<LoginResponse?> AuthenticateUserAsync(LoginRequest request);

    /// <summary>
    /// Creates a new Staff or Admin user account (US_021, AC1).
    /// Generates random password, hashes it, and sends activation email.
    /// </summary>
    /// <param name="request">Create user request with name, email, and role</param>
    /// <param name="adminUserId">ID of admin creating the user (for audit logging)</param>
    /// <returns>UserDto of created user</returns>
    /// <exception cref="InvalidOperationException">If email already exists (409 Conflict)</exception>
    Task<UserDto> CreateUserAsync(CreateUserRequest request, Guid adminUserId);

    /// <summary>
    /// Updates existing user's name and/or role (US_021, AC2).
    /// Creates audit log entry for the modification.
    /// </summary>
    /// <param name="userId">ID of user to update</param>
    /// <param name="request">Update request with new name and role</param>
    /// <param name="adminUserId">ID of admin performing the update (for audit logging)</param>
    /// <returns>Updated UserDto</returns>
    /// <exception cref="InvalidOperationException">If user not found</exception>
    Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, Guid adminUserId);

    /// <summary>
    /// Deactivates user account (soft delete) (US_021, AC3).
    /// Prevents self-deactivation and last Admin deactivation.
    /// Terminates all active sessions for the user.
    /// </summary>
    /// <param name="userId">ID of user to deactivate</param>
    /// <param name="currentUserId">ID of admin performing deactivation</param>
    /// <returns>Task completion</returns>
    /// <exception cref="InvalidOperationException">If attempting self-deactivation or last Admin deactivation</exception>
    Task DeactivateUserAsync(Guid userId, Guid currentUserId);

    /// <summary>
    /// Gets all Staff and Admin users with optional filtering and sorting (US_021, AC4).
    /// </summary>
    /// <param name="search">Optional search term for name or email</param>
    /// <param name="sortBy">Optional column name for sorting</param>
    /// <param name="ascending">Sort direction</param>
    /// <returns>List of UserDto objects</returns>
    Task<IEnumerable<UserDto>> GetAllUsersAsync(string? search = null, string? sortBy = null, bool ascending = true);

    /// <summary>
    /// Gets count of active Admin users (US_021, Edge Case validation).
    /// </summary>
    /// <returns>Number of active Admins</returns>
    Task<int> GetActiveAdminCountAsync();

    /// <summary>
    /// Refreshes user session by extending Redis TTL to 15 minutes (UXR-604, FR-005).
    /// Creates audit log entry for session extension event.
    /// Called by POST /api/auth/refresh-session endpoint when user clicks "Extend Session" button.
    /// </summary>
    /// <param name="userId">User ID from JWT claims</param>
    /// <returns>True if session extended successfully, false if session not found</returns>
    Task<bool> RefreshSessionAsync(Guid userId);
}
