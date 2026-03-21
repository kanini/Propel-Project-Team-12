namespace PatientAccess.Business.Services;

/// <summary>
/// Service interface for distributed session token caching using Redis with database fallback.
/// Implements Zero-PHI caching strategy - stores ONLY session tokens, NO patient health information.
/// Supports 15-minute TTL with sliding expiration per NFR-005.
/// </summary>
public interface ISessionCacheService
{
    /// <summary>
    /// Stores a session token in Redis cache with 15-minute TTL and sliding expiration.
    /// Falls back to database storage if Redis is unavailable.
    /// </summary>
    /// <param name="userId">Unique user identifier</param>
    /// <param name="token">JWT session token to cache</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if cached successfully, false otherwise</returns>
    Task<bool> SetSessionAsync(string userId, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a session token from Redis cache and refreshes TTL if found (sliding expiration).
    /// Falls back to database lookup if Redis is unavailable.
    /// </summary>
    /// <param name="userId">Unique user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cached session token if found and valid, null otherwise</returns>
    Task<string?> GetSessionAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a session token from Redis cache and database.
    /// Used during explicit logout operations.
    /// </summary>
    /// <param name="userId">Unique user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if removed successfully, false otherwise</returns>
    Task<bool> RemoveSessionAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the TTL of an existing session token (sliding expiration).
    /// Falls back to database update if Redis is unavailable.
    /// </summary>
    /// <param name="userId">Unique user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if refreshed successfully, false if session not found</returns>
    Task<bool> RefreshSessionAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extends the TTL of an existing session token to 15 minutes (900 seconds).
    /// Used by POST /api/auth/refresh-session endpoint to reset session timeout.
    /// Falls back to database update if Redis is unavailable.
    /// </summary>
    /// <param name="userId">Unique user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if extended successfully, false if session not found</returns>
    Task<bool> ExtendSessionAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Health check to verify Redis connectivity.
    /// Returns false if Redis is unavailable (triggers database fallback mode).
    /// </summary>
    /// <returns>True if Redis is available, false otherwise</returns>
    Task<bool> IsRedisAvailableAsync();

    /// <summary>
    /// Invalidates all active sessions for a specific user (US_021 AC3).
    /// Uses Redis SCAN to find all session keys matching pattern "session:{userId}:*"
    /// and deletes them. Used when deactivating user accounts.
    /// </summary>
    /// <param name="userId">User ID whose sessions should be terminated</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of sessions invalidated</returns>
    Task<int> InvalidateUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
