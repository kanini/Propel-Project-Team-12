using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PatientAccess.Business.Services;
using System.Security.Claims;

namespace PatientAccess.Web.Filters;

/// <summary>
/// Action filter enforcing minimum necessary access principle (NFR-014).
/// Validates that Patient role users can only access their own data.
/// Logs unauthorized access attempts via audit service (NFR-007).
/// </summary>
public class MinimumNecessaryAccessFilter : IAsyncActionFilter
{
    private readonly IAuditService _auditService;
    private readonly ILogger<MinimumNecessaryAccessFilter> _logger;

    public MinimumNecessaryAccessFilter(IAuditService auditService, ILogger<MinimumNecessaryAccessFilter> logger)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Extract user claims from authenticated context
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) 
                          ?? context.HttpContext.User.FindFirst("sub");
        var roleClaim = context.HttpContext.User.FindFirst(ClaimTypes.Role) 
                        ?? context.HttpContext.User.FindFirst("role")
                        ?? context.HttpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

        if (userIdClaim == null || roleClaim == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = Guid.Parse(userIdClaim.Value);
        var role = roleClaim.Value;

        // Minimum necessary access enforcement applies only to Patient role
        if (role == "Patient")
        {
            // Extract resource ID from route parameters or query string
            Guid? resourceId = null;
            
            // Try to get from route data (e.g., /api/patients/{id})
            if (context.RouteData.Values.TryGetValue("id", out var routeId))
            {
                // Handle "me" endpoint -> map to current user ID
                if (routeId?.ToString()?.ToLower() == "me")
                {
                    resourceId = userId;
                }
                else if (Guid.TryParse(routeId?.ToString(), out var parsedId))
                {
                    resourceId = parsedId;
                }
            }

            // Try to get from action parameters (e.g., patientId parameter)
            if (resourceId == null && context.ActionArguments.TryGetValue("patientId", out var patientIdArg))
            {
                if (Guid.TryParse(patientIdArg?.ToString(), out var parsedPatientId))
                {
                    resourceId = parsedPatientId;
                }
            }

            // Validate Patient can only access their own data (userId must match resourceId)
            if (resourceId.HasValue && resourceId.Value != userId)
            {
                var resourceType = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
                var action = context.RouteData.Values["action"]?.ToString() ?? "Unknown";
                var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();

                // Log unauthorized access attempt (NFR-014 violation)
                await _auditService.LogUnauthorizedAccessAsync(
                    userId, 
                    resourceType, 
                    resourceId.Value, 
                    action, 
                    ipAddress, 
                    userAgent);

                _logger.LogWarning(
                    "Minimum necessary access violation: Patient {UserId} attempted to access {ResourceType} {ResourceId}",
                    userId, resourceType, resourceId.Value);

                context.Result = new ForbidResult();
                return;
            }
        }
        else if (role == "Staff" || role == "Admin")
        {
            // Staff and Admin can access any patient data (but log access for audit trail)
            var resourceType = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
            var action = context.RouteData.Values["action"]?.ToString() ?? "Unknown";
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();

            // Extract resource ID if available
            Guid? resourceId = null;
            if (context.RouteData.Values.TryGetValue("id", out var routeId) && Guid.TryParse(routeId?.ToString(), out var parsedId))
            {
                resourceId = parsedId;
            }

            if (resourceId.HasValue && (action == "Get" || action == "Update" || action == "Delete"))
            {
                await _auditService.LogDataAccessAsync(
                    userId, 
                    resourceType, 
                    resourceId.Value, 
                    action == "Get" ? "Read" : action, 
                    ipAddress, 
                    userAgent);
            }
        }

        // Continue to action execution
        await next();
    }
}
