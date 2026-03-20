using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PatientAccess.Web.Filters;

/// <summary>
/// Swagger operation filter that automatically adds 401/403 responses to endpoints marked with [Authorize] attribute.
/// Adds security requirements to protected endpoints, displaying lock icon in Swagger UI.
/// Implements AC-1 requirement for documenting authentication requirements.
/// </summary>
public class SwaggerAuthorizationOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Check if endpoint has [Authorize] attribute
        var hasAuthorizeAttribute = context.MethodInfo.DeclaringType != null &&
            (context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
             context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any());

        // Check if endpoint has [AllowAnonymous] attribute (overrides [Authorize])
        var hasAllowAnonymousAttribute = context.MethodInfo.DeclaringType != null &&
            (context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ||
             context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any());

        if (!hasAuthorizeAttribute || hasAllowAnonymousAttribute)
        {
            return; // Not a protected endpoint
        }

        // Add security requirement for protected endpoints
        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            }
        };

        // Add 401 Unauthorized response
        operation.Responses.TryAdd("401", new OpenApiResponse
        {
            Description = "Unauthorized - Missing or invalid authentication token"
        });

        // Add 403 Forbidden response
        operation.Responses.TryAdd("403", new OpenApiResponse
        {
            Description = "Forbidden - Valid token but insufficient permissions"
        });
    }
}
