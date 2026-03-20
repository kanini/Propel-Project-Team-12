using System.Net;
using System.Text.Json;

namespace PatientAccess.Web.Middleware;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions
/// and returns consistent RFC 7807 problem details responses.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";
        
        var (statusCode, title, detail) = GetErrorDetails(exception);
        context.Response.StatusCode = (int)statusCode;

        var problem = new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title,
            status = (int)statusCode,
            detail,
            instance = context.Request.Path.ToString(),
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode statusCode, string title, string detail) GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentException or ArgumentNullException => 
                (HttpStatusCode.BadRequest, "Bad Request", exception.Message),
            
            UnauthorizedAccessException => 
                (HttpStatusCode.Unauthorized, "Unauthorized", "Authentication is required to access this resource."),
            
            KeyNotFoundException => 
                (HttpStatusCode.NotFound, "Not Found", "The requested resource was not found."),
            
            InvalidOperationException => 
                (HttpStatusCode.Conflict, "Conflict", exception.Message),
            
            _ => 
                (HttpStatusCode.InternalServerError, "Internal Server Error", 
                 "An unexpected error occurred. Please contact support if the problem persists.")
        };
    }
}
