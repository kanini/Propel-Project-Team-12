using System.Net;

namespace PatientAccess.Web.Middleware;

/// <summary>
/// Middleware for capturing IP address and User-Agent for audit logging (FR-005, NFR-007).
/// Stores client metadata in HttpContext.Items for access in controllers.
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract Client IP Address (X-Forwarded-For takes precedence for proxy scenarios)
        var ipAddress = GetClientIpAddress(context);
        context.Items["ClientIpAddress"] = ipAddress;

        // Extract User-Agent
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        context.Items["UserAgent"] = userAgent ?? "Unknown";

        _logger.LogDebug("Audit context set: IP={IpAddress}, UserAgent={UserAgent}", ipAddress, userAgent);

        await _next(context);
    }

    /// <summary>
    /// Extracts client IP address from request, handling proxy scenarios.
    /// Prioritizes X-Forwarded-For header (for reverse proxy/load balancer scenarios).
    /// Falls back to RemoteIpAddress if X-Forwarded-For is not present.
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>Client IP address as string</returns>
    private string GetClientIpAddress(HttpContext context)
    {
        // Check X-Forwarded-For header (set by reverse proxies/load balancers)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs (client, proxy1, proxy2, ...)
            // First IP is the original client IP
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                var clientIp = ips[0].Trim();
                
                // Validate IP address format
                if (IPAddress.TryParse(clientIp, out _))
                {
                    return clientIp;
                }
            }
        }

        // Fallback to direct connection IP
        var remoteIpAddress = context.Connection.RemoteIpAddress;
        
        if (remoteIpAddress != null)
        {
            // Handle IPv6 loopback (::1) and map to IPv4
            if (remoteIpAddress.Equals(IPAddress.IPv6Loopback))
            {
                return "127.0.0.1";
            }

            // Handle IPv4-mapped IPv6 addresses (::ffff:192.168.1.1)
            if (remoteIpAddress.IsIPv4MappedToIPv6)
            {
                return remoteIpAddress.MapToIPv4().ToString();
            }

            return remoteIpAddress.ToString();
        }

        return "Unknown";
    }
}

/// <summary>
/// Extension methods for registering AuditLoggingMiddleware in the pipeline.
/// </summary>
public static class AuditLoggingMiddlewareExtensions
{
    /// <summary>
    /// Adds AuditLoggingMiddleware to the application pipeline.
    /// MUST be registered after authentication middleware to access user context.
    /// </summary>
    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuditLoggingMiddleware>();
    }
}
