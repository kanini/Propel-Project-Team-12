using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace PatientAccess.Web.Middleware;

/// <summary>
/// Middleware to provide detailed JWT validation error responses.
/// Distinguishes between expired tokens (token_expired) and invalid tokens (invalid_token).
/// Implements AC-5 requirement for 401 Unauthorized responses with no sensitive information.
/// </summary>
public class JwtValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtValidationMiddleware> _logger;

    public JwtValidationMiddleware(RequestDelegate next, ILogger<JwtValidationMiddleware> _logger)
    {
        _next = next;
        this._logger = _logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning(ex, "JWT token expired for request {Path}", context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "token_expired",
                message = "Authentication token has expired. Please login again.",
                timestamp = DateTime.UtcNow.ToString("o")
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Invalid JWT token for request {Path}", context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "invalid_token",
                message = "Authentication token is invalid or malformed.",
                timestamp = DateTime.UtcNow.ToString("o")
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}

/// <summary>
/// Extension method to register JWT validation middleware in pipeline.
/// </summary>
public static class JwtValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtValidationMiddleware>();
    }
}
