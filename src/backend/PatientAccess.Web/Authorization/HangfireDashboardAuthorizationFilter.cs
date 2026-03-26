using Hangfire.Dashboard;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace PatientAccess.Web.Authorization;

/// <summary>
/// Authorization filter for Hangfire dashboard (US_043).
/// In Development: Allows all access for testing.
/// In Production: Restricts dashboard access to Admin role users only.
/// </summary>
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _environment;

    public HangfireDashboardAuthorizationFilter(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Authorizes dashboard access.
    /// Development: Always allows access.
    /// Production: Requires authenticated Admin role.
    /// </summary>
    /// <param name="context">Dashboard context with HTTP context</param>
    /// <returns>True if authorized, false otherwise</returns>
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow unrestricted access in Development environment for testing
        if (_environment.IsDevelopment())
        {
            return true;
        }

        // Production: Check user authentication
        if (httpContext.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // Production: Check Admin role
        var isAdmin = httpContext.User.IsInRole("Admin");

        if (!isAdmin)
        {
            // Log unauthorized access attempt
            var userId = httpContext.User.FindFirst("sub")?.Value ?? "Unknown";
            // Note: Logger not available in filter constructor - consider audit log service if needed
        }

        return isAdmin;
    }
}
