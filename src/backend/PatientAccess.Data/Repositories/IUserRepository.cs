using PatientAccess.Data.Models;

namespace PatientAccess.Data.Repositories;

/// <summary>
/// Repository interface for User entity operations (FR-001).
/// Defines data access methods for user management.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves user by email address.
    /// </summary>
    /// <param name="email">User email (unique identifier)</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Retrieves user by ID.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetByIdAsync(Guid userId);

    /// <summary>
    /// Creates a new user in the database.
    /// </summary>
    /// <param name="user">User entity to create</param>
    /// <returns>Created user with generated ID</returns>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Updates existing user in the database.
    /// </summary>
    /// <param name="user">User entity with updated values</param>
    /// <returns>Updated user entity</returns>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Retrieves user by verification token.
    /// </summary>
    /// <param name="token">Verification token</param>
    /// <returns>User entity or null if not found</returns>
    Task<User?> GetByVerificationTokenAsync(string token);

    /// <summary>
    /// Retrieves all users with optional filtering and sorting (US_021, AC4).
    /// </summary>
    /// <param name="search">Optional search term for name or email filtering</param>
    /// <param name="sortBy">Optional column name for sorting (e.g., "Name", "Email", "CreatedAt")</param>
    /// <param name="ascending">Sort direction (true = ascending, false = descending)</param>
    /// <returns>List of users matching filters</returns>
    Task<IEnumerable<User>> GetAllAsync(string? search = null, string? sortBy = null, bool ascending = true);

    /// <summary>
    /// Gets count of active Admin users (US_021, Edge Case).
    /// Used to prevent deactivation of last Admin account.
    /// </summary>
    /// <returns>Number of active Admin users</returns>
    Task<int> GetActiveAdminCountAsync();

    /// <summary>
    /// Updates user status (Active/Inactive/Suspended) (US_021, AC3).
    /// </summary>
    /// <param name="userId">User ID to update</param>
    /// <param name="status">New status value</param>
    /// <returns>Updated user entity</returns>
    Task<User> UpdateStatusAsync(Guid userId, UserStatus status);
}
