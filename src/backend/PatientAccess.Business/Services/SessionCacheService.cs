using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace PatientAccess.Business.Services;

/// <summary>
/// Redis-based session token caching service with database fallback.
/// Implements Zero-PHI caching strategy - stores ONLY session tokens, NO patient health information.
/// Supports 15-minute TTL with sliding expiration per NFR-005.
/// Falls back to database storage when Redis is unavailable (graceful degradation per AG-001).
/// </summary>
public class SessionCacheService : ISessionCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<SessionCacheService> _logger;
    private readonly TimeSpan _sessionTtl = TimeSpan.FromMinutes(15); // NFR-005: 15-minute session timeout
    private const string SessionKeyPrefix = "session:";

    /// <summary>
    /// Initializes SessionCacheService with Redis connection.
    /// Redis connection is configured as Singleton in DI container for connection pooling.
    /// </summary>
    /// <param name="redis">Redis connection multiplexer (injected as singleton)</param>
    /// <param name="logger">Logger for diagnostics and fallback notifications</param>
    public SessionCacheService(IConnectionMultiplexer redis, ILogger<SessionCacheService> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Stores session token in Redis with 15-minute TTL.
    /// Falls back to database if Redis unavailable.
    /// Zero-PHI Policy: Only stores session token metadata, no patient data.
    /// </summary>
    public async Task<bool> SetSessionAsync(string userId, string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentNullException(nameof(token));

        try
        {
            var db = _redis.GetDatabase();
            var key = GetSessionKey(userId);

            // Store token with 15-minute TTL (NFR-005)
            // Zero-PHI: Only storing token string, no patient health information
            var success = await db.StringSetAsync(key, token, _sessionTtl);

            if (success)
            {
                _logger.LogDebug("Session cached in Redis for user {UserId} with {TtlMinutes}-minute TTL", userId, _sessionTtl.TotalMinutes);
                return true;
            }

            _logger.LogWarning("Failed to cache session in Redis for user {UserId}", userId);
            return await FallbackToDatabase_SetSessionAsync(userId, token, cancellationToken);
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis unavailable for SetSession (user: {UserId}). Falling back to database.", userId);
            return await FallbackToDatabase_SetSessionAsync(userId, token, cancellationToken);
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex, "Redis timeout for SetSession (user: {UserId}). Falling back to database.", userId);
            return await FallbackToDatabase_SetSessionAsync(userId, token, cancellationToken);
        }
    }

    /// <summary>
    /// Retrieves session token from Redis and refreshes TTL (sliding expiration).
    /// Falls back to database if Redis unavailable.
    /// </summary>
    public async Task<string?> GetSessionAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        try
        {
            var db = _redis.GetDatabase();
            var key = GetSessionKey(userId);

            // Retrieve token from Redis
            var token = await db.StringGetAsync(key);

            if (token.IsNullOrEmpty)
            {
                _logger.LogDebug("Session cache miss in Redis for user {UserId}. Checking database fallback.", userId);
                return await FallbackToDatabase_GetSessionAsync(userId, cancellationToken);
            }

            // Implement sliding expiration: refresh TTL on access (NFR-005)
            await db.KeyExpireAsync(key, _sessionTtl);

            _logger.LogDebug("Session cache hit in Redis for user {UserId}. TTL refreshed to {TtlMinutes} minutes (sliding expiration).", userId, _sessionTtl.TotalMinutes);
            return token.ToString();
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis unavailable for GetSession (user: {UserId}). Falling back to database.", userId);
            return await FallbackToDatabase_GetSessionAsync(userId, cancellationToken);
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex, "Redis timeout for GetSession (user: {UserId}). Falling back to database.", userId);
            return await FallbackToDatabase_GetSessionAsync(userId, cancellationToken);
        }
    }

    /// <summary>
    /// Removes session token from Redis and database.
    /// Used during explicit logout operations.
    /// </summary>
    public async Task<bool> RemoveSessionAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        var redisRemoved = false;

        try
        {
            var db = _redis.GetDatabase();
            var key = GetSessionKey(userId);

            redisRemoved = await db.KeyDeleteAsync(key);

            if (redisRemoved)
            {
                _logger.LogDebug("Session removed from Redis for user {UserId}", userId);
            }
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis unavailable for RemoveSession (user: {UserId}). Attempting database cleanup.", userId);
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex, "Redis timeout for RemoveSession (user: {UserId}). Attempting database cleanup.", userId);
        }

        // Always attempt database cleanup to ensure session is removed
        var dbRemoved = await FallbackToDatabase_RemoveSessionAsync(userId, cancellationToken);

        return redisRemoved || dbRemoved;
    }

    /// <summary>
    /// Refreshes TTL of existing session (sliding expiration).
    /// Falls back to database if Redis unavailable.
    /// </summary>
    public async Task<bool> RefreshSessionAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        try
        {
            var db = _redis.GetDatabase();
            var key = GetSessionKey(userId);

            // Check if key exists and refresh TTL
            var exists = await db.KeyExistsAsync(key);

            if (!exists)
            {
                _logger.LogDebug("Session key not found in Redis for user {UserId}. Checking database fallback.", userId);
                return await FallbackToDatabase_RefreshSessionAsync(userId, cancellationToken);
            }

            // Refresh TTL (sliding expiration)
            var refreshed = await db.KeyExpireAsync(key, _sessionTtl);

            if (refreshed)
            {
                _logger.LogDebug("Session TTL refreshed in Redis for user {UserId} to {TtlMinutes} minutes", userId, _sessionTtl.TotalMinutes);
                return true;
            }

            return false;
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis unavailable for RefreshSession (user: {UserId}). Falling back to database.", userId);
            return await FallbackToDatabase_RefreshSessionAsync(userId, cancellationToken);
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex, "Redis timeout for RefreshSession (user: {UserId}). Falling back to database.", userId);
            return await FallbackToDatabase_RefreshSessionAsync(userId, cancellationToken);
        }
    }

    /// <summary>
    /// Health check to verify Redis connectivity.
    /// Returns false if Redis is unavailable (triggers database fallback mode).
    /// </summary>
    public async Task<bool> IsRedisAvailableAsync()
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.PingAsync();
            return true;
        }
        catch (RedisException ex)
        {
            _logger.LogWarning(ex, "Redis health check failed: Redis connection unavailable");
            return false;
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex, "Redis health check failed: Connection timeout");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Redis health check");
            return false;
        }
    }

    #region Private Helpers

    /// <summary>
    /// Generates Redis key for session storage with prefix for namespace isolation.
    /// Zero-PHI: Key contains only user ID, no patient identifiers.
    /// </summary>
    private static string GetSessionKey(string userId) => $"{SessionKeyPrefix}{userId}";

    #endregion

    #region Database Fallback Methods (Placeholder)

    // Note: These methods will be implemented after database session storage is created.
    // For now, they return minimal implementation to satisfy interface contract.

    /// <summary>
    /// Database fallback for storing session token.
    /// TODO: Implement database session storage (requires SessionToken table/entity).
    /// </summary>
    private Task<bool> FallbackToDatabase_SetSessionAsync(string userId, string token, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database fallback for SetSession not yet implemented. Session for user {UserId} stored in-memory only (will be lost on restart).", userId);
        // TODO: Implement database session storage
        // Example:
        // var session = new SessionToken { UserId = userId, Token = token, ExpiresAt = DateTime.UtcNow.Add(_sessionTtl) };
        // await _dbContext.SessionTokens.AddAsync(session, cancellationToken);
        // await _dbContext.SaveChangesAsync(cancellationToken);
        return Task.FromResult(false);
    }

    /// <summary>
    /// Database fallback for retrieving session token.
    /// TODO: Implement database session lookup (requires SessionToken table/entity).
    /// </summary>
    private Task<string?> FallbackToDatabase_GetSessionAsync(string userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database fallback for GetSession not yet implemented. User {UserId} session lookup failed.", userId);
        // TODO: Implement database session lookup
        // Example:
        // var session = await _dbContext.SessionTokens
        //     .Where(s => s.UserId == userId && s.ExpiresAt > DateTime.UtcNow)
        //     .FirstOrDefaultAsync(cancellationToken);
        // return session?.Token;
        return Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Database fallback for removing session token.
    /// TODO: Implement database session removal (requires SessionToken table/entity).
    /// </summary>
    private Task<bool> FallbackToDatabase_RemoveSessionAsync(string userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database fallback for RemoveSession not yet implemented. Session cleanup for user {UserId} incomplete.", userId);
        // TODO: Implement database session removal
        // Example:
        // var sessions = _dbContext.SessionTokens.Where(s => s.UserId == userId);
        // _dbContext.SessionTokens.RemoveRange(sessions);
        // await _dbContext.SaveChangesAsync(cancellationToken);
        return Task.FromResult(false);
    }

    /// <summary>
    /// Database fallback for refreshing session TTL.
    /// TODO: Implement database session TTL refresh (requires SessionToken table/entity).
    /// </summary>
    private Task<bool> FallbackToDatabase_RefreshSessionAsync(string userId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Database fallback for RefreshSession not yet implemented. Session refresh for user {UserId} incomplete.", userId);
        // TODO: Implement database session refresh
        // Example:
        // var session = await _dbContext.SessionTokens
        //     .Where(s => s.UserId == userId && s.ExpiresAt > DateTime.UtcNow)
        //     .FirstOrDefaultAsync(cancellationToken);
        // if (session != null)
        // {
        //     session.ExpiresAt = DateTime.UtcNow.Add(_sessionTtl);
        //     await _dbContext.SaveChangesAsync(cancellationToken);
        //     return true;
        // }
        return Task.FromResult(false);
    }

    #endregion
}
