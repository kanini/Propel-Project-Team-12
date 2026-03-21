using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data.Models;
using System.Security.Claims;

namespace PatientAccess.Web.Authorization;

/// <summary>
/// Custom authorization handler that logs unauthorized access attempts to audit log (US_020).
/// Integrates with IAuditLogService to track authorization violations.
/// </summary>
public class AuditingAuthorizationHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly IAuthorizationMiddlewareResultHandler _defaultHandler;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<AuditingAuthorizationHandler> _logger;

    public AuditingAuthorizationHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<AuditingAuthorizationHandler> logger)
    {
        _defaultHandler = new AuthorizationMiddlewareResultHandler();
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        // If authorization failed, log the unauthorized access attempt
        if (!authorizeResult.Succeeded)
        {
            await LogUnauthorizedAccessAsync(context, policy, authorizeResult);
        }

        // Delegate to default handler for standard behavior (returns 403, etc.)
        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    private async Task LogUnauthorizedAccessAsync(
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        try
        {
            // Create a scope to resolve scoped services
            using var scope = _serviceScopeFactory.CreateScope();
            var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();

            // Extract user information from claims
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = null;

            if (Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            // Extract role from claims
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";

            // Extract audit context (IP, User Agent) from middleware
            var ipAddress = context.Items["AuditContext:IpAddress"] as string;
            var userAgent = context.Items["AuditContext:UserAgent"] as string;

            // Determine failure reason
            var failureReason = authorizeResult.AuthorizationFailure?.FailureReasons
                .Select(r => r.Message)
                .FirstOrDefault() ?? "Insufficient permissions";

            // Extract required roles from policy
            var requiredRoles = policy.Requirements
                .OfType<Microsoft.AspNetCore.Authorization.Infrastructure.RolesAuthorizationRequirement>()
                .SelectMany(r => r.AllowedRoles)
                .ToArray();

            var metadata = System.Text.Json.JsonSerializer.Serialize(new
            {
                endpoint = $"{context.Request.Method} {context.Request.Path}",
                requiredRole = requiredRoles.Length > 0 ? requiredRoles : new[] { "Authenticated" },
                userRole = role,
                failureReason,
                timestamp = DateTime.UtcNow
            });

            // Log unauthorized access attempt (US_020, AC1, AC2, AC3)
            await auditLogService.LogAuthEventAsync(
                userId: userId,
                actionType: AuditActionType.FailedLogin, // Reusing for unauthorized access
                ipAddress: ipAddress,
                userAgent: userAgent,
                metadata: metadata);

            _logger.LogWarning(
                "Unauthorized access attempt: UserId={UserId}, Role={Role}, Endpoint={Endpoint}",
                userId,
                role,
                $"{context.Request.Method} {context.Request.Path}");
        }
        catch (Exception ex)
        {
            // Don't let audit logging failures prevent authorization response
            _logger.LogError(ex, "Failed to log unauthorized access attempt");
        }
    }
}
