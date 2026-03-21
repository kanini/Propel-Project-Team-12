using System.Collections.Concurrent;
using System.Text.Json;

namespace PatientAccess.Web.Middleware;

/// <summary>
/// Rate limiting middleware for registration endpoints (FR-001).
/// Limits registration attempts to 3 requests per 5 minutes per email address.
/// Uses in-memory cache with sliding window algorithm.
/// </summary>
public class RegistrationRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RegistrationRateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitEntry> _rateLimitCache = new();
    private static readonly TimeSpan _windowDuration = TimeSpan.FromMinutes(5);
    private const int _maxRequests = 3;

    public RegistrationRateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RegistrationRateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply rate limiting to registration endpoint
        if (!context.Request.Path.StartsWithSegments("/api/auth/register") ||
            context.Request.Method != HttpMethods.Post)
        {
            await _next(context);
            return;
        }

        // Extract email from request body for rate limiting key
        var email = await ExtractEmailFromRequestAsync(context);

        if (string.IsNullOrWhiteSpace(email))
        {
            await _next(context);
            return;
        }

        // Normalize email for rate limit key
        var rateLimitKey = email.ToLower().Trim();

        // Clean up expired entries periodically
        CleanupExpiredEntries();

        // Check rate limit
        if (!_rateLimitCache.TryGetValue(rateLimitKey, out var entry))
        {
            // First request from this email
            entry = new RateLimitEntry
            {
                Count = 1,
                WindowStart = DateTime.UtcNow
            };
            _rateLimitCache[rateLimitKey] = entry;
        }
        else
        {
            var elapsed = DateTime.UtcNow - entry.WindowStart;

            if (elapsed > _windowDuration)
            {
                // Reset window
                entry.Count = 1;
                entry.WindowStart = DateTime.UtcNow;
            }
            else if (entry.Count >= _maxRequests)
            {
                // Rate limit exceeded
                var retryAfter = (_windowDuration - elapsed).TotalSeconds;

                _logger.LogWarning(
                    "Rate limit exceeded for email {Email}. Requests: {Count}, Window: {Elapsed}s",
                    rateLimitKey, entry.Count, elapsed.TotalSeconds);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Append("Retry-After", ((int)retryAfter).ToString());
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    error = "Rate limit exceeded",
                    message = $"Too many registration attempts. Please try again in {Math.Ceiling(retryAfter)} seconds.",
                    retryAfterSeconds = (int)retryAfter
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                return;
            }
            else
            {
                // Increment request count within window
                entry.Count++;
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Extracts email from request body for rate limiting.
    /// Enables request body buffering to read body multiple times.
    /// </summary>
    private async Task<string?> ExtractEmailFromRequestAsync(HttpContext context)
    {
        try
        {
            // Enable buffering to read body multiple times
            context.Request.EnableBuffering();

            using var reader = new StreamReader(
                context.Request.Body,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();

            // Reset stream position for subsequent middleware
            context.Request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            // Parse JSON to extract email
            var jsonDoc = JsonDocument.Parse(body);
            if (jsonDoc.RootElement.TryGetProperty("email", out var emailElement))
            {
                return emailElement.GetString();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract email from request body for rate limiting");
            return null;
        }
    }

    /// <summary>
    /// Removes expired entries from rate limit cache (older than window duration).
    /// </summary>
    private static void CleanupExpiredEntries()
    {
        var cutoff = DateTime.UtcNow - _windowDuration;

        var expiredKeys = _rateLimitCache
            .Where(kvp => DateTime.UtcNow - kvp.Value.WindowStart > _windowDuration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _rateLimitCache.TryRemove(key, out _);
        }
    }

    private class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
    }
}
