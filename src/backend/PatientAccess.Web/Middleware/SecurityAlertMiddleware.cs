using StackExchange.Redis;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Web.Middleware;

/// <summary>
/// Security alert middleware for detecting suspicious activity (AC4 - US_057).
/// Tracks repeated 401/403 errors from same IP and triggers alerts when threshold exceeded.
/// Implements audit logging and optional IP throttling for suspicious actors.
/// </summary>
public class SecurityAlertMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityAlertMiddleware> _logger;

    // Security alert thresholds (AC4 - US_057, configurable via appsettings.json)
    private int _maxAuthFailuresInWindow = 10;
    private TimeSpan _alertWindow = TimeSpan.FromMinutes(5);
    private TimeSpan _throttleDuration = TimeSpan.FromHours(1);

    public SecurityAlertMiddleware(
        RequestDelegate next,
        ILogger<SecurityAlertMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;

        // Load configuration
        var securityAlertSettings = configuration.GetSection("SecurityAlerts");
        _maxAuthFailuresInWindow = securityAlertSettings.GetValue<int>("MaxAuthFailuresInWindow", 10);
        _alertWindow = TimeSpan.FromMinutes(securityAlertSettings.GetValue<int>("AlertWindowMinutes", 5));
        _throttleDuration = TimeSpan.FromHours(securityAlertSettings.GetValue<int>("ThrottleDurationHours", 1));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Execute next middleware
        await _next(context);

        // Check for 401/403 responses (AC4 - US_057)
        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized ||
            context.Response.StatusCode == StatusCodes.Status403Forbidden)
        {
            await HandleAuthorizationFailure(context);
        }

        // Check for 429 responses (rate limit) and log to audit (AC2 - US_057)
        if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
        {
            await LogRateLimitViolation(context);
        }
    }

    private async Task HandleAuthorizationFailure(HttpContext context)
    {
        var ipAddress = GetClientIpAddress(context);
        
        // Get Redis connection if available
        var redis = context.RequestServices.GetService<IConnectionMultiplexer>();
        if (redis == null || !redis.IsConnected)
        {
            _logger.LogWarning("Redis not available for security alert tracking. Skipping auth failure tracking.");
            return;
        }

        var db = redis.GetDatabase();
        var redisKey = $"auth:failures:{ipAddress}";

        try
        {
            // Increment failure counter with sliding window expiry (AC4)
            var failureCount = await db.StringIncrementAsync(redisKey);

            // Set expiry if this is the first failure
            if (failureCount == 1)
            {
                await db.KeyExpireAsync(redisKey, _alertWindow);
            }

            // Check if threshold exceeded (AC4: >10 in 5 minutes)
            if (failureCount > _maxAuthFailuresInWindow)
            {
                await TriggerSecurityAlert(context, ipAddress, (int)failureCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to track authorization failure for IP {IpAddress}. Error: {Message}", 
                ipAddress, ex.Message);
        }
    }

    private async Task TriggerSecurityAlert(HttpContext context, string ipAddress, int failureCount)
    {
        // Log security alert (AC4 - US_057)
        _logger.LogWarning(
            "SECURITY ALERT: Suspicious activity detected from IP {IpAddress}. " +
            "{FailureCount} authorization failures in {Window} minutes. Threshold: {Threshold}.",
            ipAddress, failureCount, _alertWindow.TotalMinutes, _maxAuthFailuresInWindow);

        // Log to audit system (AC4)
        var auditLogService = context.RequestServices.GetService<IAuditLogService>();
        if (auditLogService != null)
        {
            try
            {
                var metadata = $"{{\"failureCount\": {failureCount}, " +
                              $"\"window\": \"{_alertWindow.TotalMinutes}m\", " +
                              $"\"threshold\": {_maxAuthFailuresInWindow}, " +
                              $"\"actionType\": \"SecurityAlert\"}}";

                await auditLogService.LogAuthEventAsync(
                    userId: null,
                    actionType: PatientAccess.Data.Models.AuditActionType.FailedLogin,
                    ipAddress: ipAddress,
                    userAgent: context.Request.Headers["User-Agent"].ToString(),
                    metadata: metadata
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security alert to audit system. Error: {Message}", ex.Message);
            }
        }

        // Optional: Trigger IP throttling (AC4)
        await ThrottleIpAddress(context, ipAddress);
    }

    private async Task ThrottleIpAddress(HttpContext context, string ipAddress)
    {
        var redis = context.RequestServices.GetService<IConnectionMultiplexer>();
        if (redis == null || !redis.IsConnected)
        {
            return;
        }

        try
        {
            var db = redis.GetDatabase();
            var throttleKey = $"throttle:ip:{ipAddress}";
            
            // Store throttled IP in Redis with expiry
            await db.StringSetAsync(throttleKey, "true", _throttleDuration);

            _logger.LogWarning(
                "IP throttling activated for {IpAddress}. Duration: {Duration} hour(s).",
                ipAddress, _throttleDuration.TotalHours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to throttle IP {IpAddress}. Error: {Message}", 
                ipAddress, ex.Message);
        }
    }

    private async Task LogRateLimitViolation(HttpContext context)
    {
        var ipAddress = GetClientIpAddress(context);
        var endpoint = $"{context.Request.Method}:{context.Request.Path}";

        // Log rate limit violation to audit system (AC2 - US_057)
        var auditLogService = context.RequestServices.GetService<IAuditLogService>();
        if (auditLogService != null)
        {
            try
            {
                var metadata = $"{{\"endpoint\": \"{endpoint}\", \"actionType\": \"RateLimitExceeded\"}}";

                await auditLogService.LogAuthEventAsync(
                    userId: null,
                    actionType: PatientAccess.Data.Models.AuditActionType.FailedLogin,
                    ipAddress: ipAddress,
                    userAgent: context.Request.Headers["User-Agent"].ToString(),
                    metadata: metadata
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log rate limit violation to audit system. Error: {Message}", ex.Message);
            }
        }

        _logger.LogWarning(
            "Rate limit exceeded for IP {IpAddress} on endpoint {Endpoint}.",
            ipAddress, endpoint);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Check X-Forwarded-For header first (Railway/Render proxy)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Fallback to RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

/// <summary>
/// Extension method to register SecurityAlertMiddleware.
/// </summary>
public static class SecurityAlertMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityAlerts(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityAlertMiddleware>();
    }
}
