using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace PatientAccess.Web.HealthChecks;

/// <summary>
/// Custom health check response writer that formats health check results as structured JSON.
/// Implements AC-3 requirement for JSON payload with database, Redis connectivity, and overall status.
/// Implements AC-4 requirement for identifying unhealthy dependencies in 503 response.
/// </summary>
public static class CustomHealthCheckResponseWriter
{
    public static Task WriteResponse(HttpContext context, HealthReport healthReport)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var result = new
        {
            status = healthReport.Status.ToString(),
            totalDuration = healthReport.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow.ToString("o"),
            checks = healthReport.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description ?? "No description provided",
                duration = entry.Value.Duration.TotalMilliseconds,
                exception = entry.Value.Exception?.Message,
                data = entry.Value.Data
            })
        };

        var json = JsonSerializer.Serialize(result, options);

        return context.Response.WriteAsync(json);
    }
}
