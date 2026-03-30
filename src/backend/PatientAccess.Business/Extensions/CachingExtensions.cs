using StackExchange.Redis;
using System.Text.Json;
using PatientAccess.Business.Enums;
using PatientAccess.Business.Validators;

namespace PatientAccess.Business.Extensions;

/// <summary>
/// Redis caching extensions with zero-PHI policy enforcement (AC4 - US_056).
/// All methods validate against PHI caching violations before write operations.
/// Epic: EP-010 - HIPAA Compliance & Security Hardening
/// Requirement: NFR-004 (zero-PHI caching strategy), AD-005 (design.md)
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Safely sets a cache value with PHI validation.
    /// Automatically validates key prefix and scans value for prohibited PHI fields.
    /// Throws InvalidOperationException if PHI detected or invalid key prefix.
    /// </summary>
    /// <typeparam name="T">Type of value to cache</typeparam>
    /// <param name="redis">Redis database connection</param>
    /// <param name="prefix">Allowed key prefix from RedisKeyPrefix enum</param>
    /// <param name="keySuffix">Key suffix (e.g., userId:sessionId)</param>
    /// <param name="value">Value to cache (must not contain PHI)</param>
    /// <param name="expiry">Cache expiration time (optional)</param>
    /// <returns>Task representing async operation</returns>
    /// <exception cref="InvalidOperationException">Thrown if PHI detected or invalid prefix</exception>
    public static async Task SetSafeAsync<T>(
        this IDatabase redis,
        RedisKeyPrefix prefix,
        string keySuffix,
        T value,
        TimeSpan? expiry = null)
    {
        var cacheKey = BuildCacheKey(prefix, keySuffix);

        // Validate key prefix (AC4)
        CachingPolicyValidator.ValidateKeyPrefix(cacheKey);

        // Validate no PHI in value (AC4)
        CachingPolicyValidator.ValidateNoPHI(value, cacheKey);

        // Serialize and set
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await redis.StringSetAsync(cacheKey, json, expiry, When.Always);
    }

    /// <summary>
    /// Gets a cached value safely.
    /// Validates key prefix to ensure only allowed cache types are accessed.
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    /// <param name="redis">Redis database connection</param>
    /// <param name="prefix">Allowed key prefix from RedisKeyPrefix enum</param>
    /// <param name="keySuffix">Key suffix (e.g., userId:sessionId)</param>
    /// <returns>Cached value or default(T) if not found</returns>
    public static async Task<T?> GetSafeAsync<T>(
        this IDatabase redis,
        RedisKeyPrefix prefix,
        string keySuffix)
    {
        var cacheKey = BuildCacheKey(prefix, keySuffix);

        // Validate key prefix (AC4)
        CachingPolicyValidator.ValidateKeyPrefix(cacheKey);

        var json = await redis.StringGetAsync(cacheKey);
        if (!json.HasValue)
            return default;

        return JsonSerializer.Deserialize<T>(json!, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// Deletes a cached value safely.
    /// Validates key prefix to ensure only allowed cache types are accessed.
    /// </summary>
    /// <param name="redis">Redis database connection</param>
    /// <param name="prefix">Allowed key prefix from RedisKeyPrefix enum</param>
    /// <param name="keySuffix">Key suffix (e.g., userId:sessionId)</param>
    /// <returns>True if key was deleted, false if key did not exist</returns>
    public static async Task<bool> DeleteSafeAsync(
        this IDatabase redis,
        RedisKeyPrefix prefix,
        string keySuffix)
    {
        var cacheKey = BuildCacheKey(prefix, keySuffix);

        // Validate key prefix (AC4)
        CachingPolicyValidator.ValidateKeyPrefix(cacheKey);

        return await redis.KeyDeleteAsync(cacheKey);
    }

    /// <summary>
    /// Checks if a cached value exists.
    /// Validates key prefix to ensure only allowed cache types are accessed.
    /// </summary>
    /// <param name="redis">Redis database connection</param>
    /// <param name="prefix">Allowed key prefix from RedisKeyPrefix enum</param>
    /// <param name="keySuffix">Key suffix (e.g., userId:sessionId)</param>
    /// <returns>True if key exists, false otherwise</returns>
    public static async Task<bool> ExistsSafeAsync(
        this IDatabase redis,
        RedisKeyPrefix prefix,
        string keySuffix)
    {
        var cacheKey = BuildCacheKey(prefix, keySuffix);

        // Validate key prefix (AC4)
        CachingPolicyValidator.ValidateKeyPrefix(cacheKey);

        return await redis.KeyExistsAsync(cacheKey);
    }

    /// <summary>
    /// Sets cache expiration time for an existing key.
    /// Validates key prefix to ensure only allowed cache types are accessed.
    /// </summary>
    /// <param name="redis">Redis database connection</param>
    /// <param name="prefix">Allowed key prefix from RedisKeyPrefix enum</param>
    /// <param name="keySuffix">Key suffix (e.g., userId:sessionId)</param>
    /// <param name="expiry">New expiration time</param>
    /// <returns>True if expiration was set, false if key did not exist</returns>
    public static async Task<bool> ExpireSafeAsync(
        this IDatabase redis,
        RedisKeyPrefix prefix,
        string keySuffix,
        TimeSpan expiry)
    {
        var cacheKey = BuildCacheKey(prefix, keySuffix);

        // Validate key prefix (AC4)
        CachingPolicyValidator.ValidateKeyPrefix(cacheKey);

        return await redis.KeyExpireAsync(cacheKey, expiry);
    }

    /// <summary>
    /// Builds cache key from prefix and suffix.
    /// Format: "prefix:suffix"
    /// </summary>
    /// <param name="prefix">Enum prefix</param>
    /// <param name="keySuffix">Key suffix</param>
    /// <returns>Full cache key</returns>
    private static string BuildCacheKey(RedisKeyPrefix prefix, string keySuffix)
    {
        if (string.IsNullOrWhiteSpace(keySuffix))
        {
            throw new ArgumentException("Key suffix cannot be null or whitespace", nameof(keySuffix));
        }

        // Convert enum to lowercase prefix
        var prefixString = ConvertEnumToPrefix(prefix);
        return $"{prefixString}:{keySuffix}";
    }

    /// <summary>
    /// Converts RedisKeyPrefix enum to lowercase prefix string.
    /// Example: RedisKeyPrefix.Session → "session"
    /// </summary>
    private static string ConvertEnumToPrefix(RedisKeyPrefix prefix)
    {
        var prefixString = prefix.ToString();

        // Handle multi-word prefixes (AggregateAppointments → aggregateappointments)
        return prefixString.ToLowerInvariant();
    }
}
