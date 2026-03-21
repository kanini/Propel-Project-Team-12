namespace PatientAccess.Web.Middleware;

/// <summary>
/// Middleware to capture and store audit context (IP address, User Agent) for the current request.
/// This context is made available to downstream services via HttpContext.Items.
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public const string IpAddressKey = "AuditContext:IpAddress";
    public const string UserAgentKey = "AuditContext:UserAgent";

    public AuditLoggingMiddleware(
        RequestDelegate next,
        ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract IP address from connection (supports both direct and proxied requests)
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        
        // Handle X-Forwarded-For header for proxy/load balancer scenarios
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var firstIp = forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(firstIp))
            {
                ipAddress = firstIp;
            }
        }

        // Extract User Agent from request headers
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        if (string.IsNullOrEmpty(userAgent))
        {
            userAgent = "Unknown";
        }

        // Store in HttpContext.Items for downstream access
        context.Items[IpAddressKey] = ipAddress;
        context.Items[UserAgentKey] = userAgent;

        _logger.LogDebug(
            "Audit context captured: IP={IP}, UserAgent={UserAgent}",
            ipAddress,
            userAgent);

        await _next(context);
    }
}
