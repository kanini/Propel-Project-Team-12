using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Web.Services;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Health and metrics controller for system monitoring (AC3 - US_057).
/// Provides metrics summary with <1s query time guarantee.
/// </summary>
[ApiController]
[Route("[controller]")]
[AllowAnonymous] // Health endpoints are public for monitoring tools
public class HealthController : ControllerBase
{
    private readonly MetricsService _metricsService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        MetricsService metricsService,
        ILogger<HealthController> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint.
    /// Returns 200 OK if API is responsive.
    /// </summary>
    /// <returns>Health status</returns>
    /// <response code="200">API is healthy</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public ActionResult<object> GetHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Metrics summary endpoint (AC3 - US_057).
    /// Returns response times, error rates, and request volumes within 1 second.
    /// Includes X-Query-Time-Ms header to verify <1s requirement.
    /// </summary>
    /// <returns>Metrics summary with response times, request volume, error rate, status codes, and top endpoints</returns>
    /// <response code="200">Metrics retrieved successfully</response>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(MetricsSummaryDto), StatusCodes.Status200OK)]
    public ActionResult<MetricsSummaryDto> GetMetrics()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var metrics = _metricsService.GetMetricsSummary();

            stopwatch.Stop();

            // Add query time to response header (AC3 verification)
            Response.Headers.Append("X-Query-Time-Ms", stopwatch.ElapsedMilliseconds.ToString());

            // Warn if query exceeds 1s (AC3 - US_057)
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "Metrics query exceeded 1s threshold: {Duration}ms (AC3 requires <1s)",
                    stopwatch.ElapsedMilliseconds);
            }

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "Failed to retrieve metrics. Error: {Message}", ex.Message);

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "InternalServerError",
                message = "Failed to retrieve metrics. Please try again later.",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
