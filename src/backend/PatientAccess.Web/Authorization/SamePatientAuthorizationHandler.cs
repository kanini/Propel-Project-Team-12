using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PatientAccess.Web.Authorization;

/// <summary>
/// Authorization requirement to ensure patients can only access their own data (US_020, AC3).
/// Validates that the userId from JWT matches the resource being accessed.
/// </summary>
public class SamePatientRequirement : IAuthorizationRequirement
{
    public string RouteParameterName { get; }

    /// <summary>
    /// Creates a requirement to validate patient data access.
    /// </summary>
    /// <param name="routeParameterName">Name of route parameter containing patient ID (e.g., "patientId")</param>
    public SamePatientRequirement(string routeParameterName = "patientId")
    {
        RouteParameterName = routeParameterName;
    }
}

/// <summary>
/// Handles SamePatientRequirement by comparing JWT userId claim with route parameter.
/// Admin and Staff roles bypass this check (can access any patient data).
/// </summary>
public class SamePatientAuthorizationHandler : AuthorizationHandler<SamePatientRequirement>
{
    private readonly ILogger<SamePatientAuthorizationHandler> _logger;

    public SamePatientAuthorizationHandler(ILogger<SamePatientAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SamePatientRequirement requirement)
    {
        // Admin and Staff can access any patient data
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (role == "Admin" || role == "Staff")
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Extract userId from JWT claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogWarning("SamePatientRequirement failed: No userId claim found");
            return Task.CompletedTask; // Fail requirement
        }

        // Extract patient ID from route (requires HttpContext)
        if (context.Resource is HttpContext httpContext)
        {
            var routePatientId = httpContext.Request.RouteValues[requirement.RouteParameterName]?.ToString();

            if (string.IsNullOrEmpty(routePatientId))
            {
                // No patient ID in route - requirement doesn't apply
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Compare userId from token with patientId from route
            if (userIdClaim.Equals(routePatientId, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
                _logger.LogDebug("SamePatientRequirement succeeded: userId matches patientId");
            }
            else
            {
                _logger.LogWarning(
                    "SamePatientRequirement failed: Cross-patient access attempt. UserId={UserId}, RoutePatientId={RoutePatientId}",
                    userIdClaim,
                    routePatientId);
                // Requirement fails - authorization will return 403
            }
        }

        return Task.CompletedTask;
    }
}
