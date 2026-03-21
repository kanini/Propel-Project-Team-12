using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for admin user management operations (US_021).
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Creates a new user account (AC1).
    /// Sends activation email to new user.
    /// </summary>
    /// <param name="request">User creation data</param>
    /// <param name="ipAddress">IP address for audit logging</param>
    /// <param name="userAgent">User agent for audit logging</param>
    /// <returns>Created user information</returns>
    /// <exception cref="InvalidOperationException">Thrown when email already exists</exception>
    Task<UserDto> CreateUserAsync(CreateUserRequestDto request, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Updates user details (AC2).
    /// Creates audit log entry for changes.
    /// </summary>
    /// <param name="userId">User ID to update</param>
    /// <param name="request">Updated user data</param>
    /// <param name="ipAddress">IP address for audit logging</param>
    /// <param name="userAgent">User agent for audit logging</param>
    /// <returns>Updated user information</returns>
    /// <exception cref="KeyNotFoundException">Thrown when user not found</exception>
    Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequestDto request, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Deactivates user account (AC3).
    /// Sets status to Inactive, terminates sessions, blocks future logins.
    /// </summary>
    /// <param name="userId">User ID to deactivate</param>
    /// <param name="currentUserId">Current admin user ID (to prevent self-deactivation)</param>
    /// <param name="ipAddress">IP address for audit logging</param>
    /// <param name="userAgent">User agent for audit logging</param>
    /// <exception cref="InvalidOperationException">Thrown when attempting self-deactivation or last admin</exception>
    /// <exception cref="KeyNotFoundException">Thrown when user not found</exception>
    Task DeactivateUserAsync(Guid userId, Guid currentUserId, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Gets all users with optional filtering.
    /// </summary>
    /// <param name="searchTerm">Optional search term for name/email filtering</param>
    /// <param name="role">Optional role filter</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of users matching criteria</returns>
    Task<List<UserDto>> GetAllUsersAsync(string? searchTerm = null, string? role = null, string? status = null);

    /// <summary>
    /// Gets a single user by ID.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User information</returns>
    /// <exception cref="KeyNotFoundException">Thrown when user not found</exception>
    Task<UserDto> GetUserByIdAsync(Guid userId);
}
