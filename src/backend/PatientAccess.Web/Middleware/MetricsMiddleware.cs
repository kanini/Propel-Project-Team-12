using Prometheus;
using System.Diagnostics;

namespace PatientAccess.Web.Middleware;

/// <summary>
/// Metrics middleware for tracking response times, error rates, and request volumes (AC3 - US_057).
/// Uses Prometheus.NET for in-memory metrics collection with <1s query time.
/// </summary>
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;

    // Prometheus metrics (AC3 - US_057)
    private static readonly Histogram RequestDuration = Metrics
        .CreateHistogram(
            "http_request_duration_seconds",
            "HTTP request duration in seconds",
            new HistogramConfiguration
            {
                LabelNames = new[] { "method", "endpoint", "status_code" },
                Buckets = new[] { 0.01, 0.05, 0.1, 0.5, 1.0, 2.0, 5.0, 10.0 }
            });

    private static readonly Counter RequestCount = Metrics
        .CreateCounter(
            "http_requests_total",
            "Total HTTP requests",
            new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint", "status_code" }
            });

    private static readonly Counter ErrorCount = Metrics
        .CreateCounter(
            "http_errors_total",
            "Total HTTP errors (4xx + 5xx)",
            new CounterConfiguration
            {
                LabelNames = new[] { "method", "endpoint", "status_code" }
            });

    public MetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip metrics endpoints to avoid self-tracking
        if (context.Request.Path.StartsWithSegments("/metrics") ||
            context.Request.Path.StartsWithSegments("/health/metrics"))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Execute next middleware
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            // Extract labels
            var method = context.Request.Method;
            var endpoint = GetEndpointPattern(context);
            var statusCode = context.Response.StatusCode.ToString();

            // Record response time (AC3 - US_057)
            RequestDuration
                .WithLabels(method, endpoint, statusCode)
                .Observe(stopwatch.Elapsed.TotalSeconds);

            // Increment request count (AC3)
            RequestCount
                .WithLabels(method, endpoint, statusCode)
                .Inc();

            // Increment error count if 4xx or 5xx (AC3, NFR-011)
            if (context.Response.StatusCode >= 400)
            {
                ErrorCount
                    .WithLabels(method, endpoint, statusCode)
                    .Inc();
            }
        }
    }

    private string GetEndpointPattern(HttpContext context)
    {
        // Get endpoint pattern from route data (e.g., /api/patients/{id})
        var endpoint = context.GetEndpoint();
        if (endpoint is Microsoft.AspNetCore.Routing.RouteEndpoint routeEndpoint)
        {
            return routeEndpoint.RoutePattern.RawText ?? context.Request.Path;
        }

        // Fallback to path with route parameter normalization
        var path = context.Request.Path.Value ?? "/";

        // Normalize route parameters (replace numeric IDs with {id})
        if (System.Text.RegularExpressions.Regex.IsMatch(path, @"/\d+"))
        {
            path = System.Text.RegularExpressions.Regex.Replace(path, @"/\d+", "/{id}");
        }

        // Normalize GUIDs in path
        path = System.Text.RegularExpressions.Regex.Replace(
            path,
            @"/[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
            "/{id}");

        return path;
    }
}

/// <summary>
/// Extension method to register MetricsMiddleware.
/// </summary>
public static class MetricsMiddlewareExtensions
{
    public static IApplicationBuilder UseApiMetrics(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MetricsMiddleware>();
    }
}
