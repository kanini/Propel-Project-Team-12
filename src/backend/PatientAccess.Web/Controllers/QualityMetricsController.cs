using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Controller for quality metrics operations (AIR-Q01, AIR-Q03).
/// Admin-only endpoints for monitoring AI-Human Agreement Rate and Schema Validity.
/// </summary>
[ApiController]
[Route("api/quality-metrics")]
[Authorize(Roles = "Admin")]
public class QualityMetricsController : ControllerBase
{
    private readonly IQualityMetricsService _qualityMetricsService;
    private readonly ILogger<QualityMetricsController> _logger;

    public QualityMetricsController(
        IQualityMetricsService qualityMetricsService,
        ILogger<QualityMetricsController> logger)
    {
        _qualityMetricsService = qualityMetricsService ?? throw new ArgumentNullException(nameof(qualityMetricsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get daily quality metrics summary for specified period.
    /// Returns AI-Human Agreement Rate (AIR-Q01) and Schema Validity (AIR-Q03).
    /// </summary>
    /// <param name="periodType">Period type: 'daily' or 'weekly'. Default: 'daily'</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Daily or weekly quality metrics summary</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(QualityMetricsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<QualityMetricsSummaryDto>> GetSummary(
        [FromQuery] string periodType = "daily",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching quality metrics summary for period type: {PeriodType}", periodType);

        try
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var lastMonday = DateTime.UtcNow.Date.AddDays(-((int)DateTime.UtcNow.DayOfWeek - 1 + 7) % 7 - 7);

            QualityMetricsSummaryDto summary = periodType.ToLowerInvariant() switch
            {
                "daily" => await _qualityMetricsService.GetDailySummaryAsync(yesterday, cancellationToken),
                "weekly" => await _qualityMetricsService.GetWeeklySummaryAsync(lastMonday, cancellationToken),
                _ => throw new ArgumentException($"Invalid period type: {periodType}. Must be 'daily' or 'weekly'.", nameof(periodType))
            };

            _logger.LogInformation("Quality metrics summary retrieved: Agreement={AgreementRate:F2}%, Validity={ValidityRate:F2}%",
                summary.AgreementRate?.MetricValue ?? 0, summary.SchemaValidity?.MetricValue ?? 0);

            return Ok(summary);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid period type: {PeriodType}", periodType);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quality metrics summary for period type: {PeriodType}", periodType);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while retrieving quality metrics summary" });
        }
    }

    /// <summary>
    /// Get quality metric history for specified metric type and date range.
    /// Returns time-series data for trend analysis.
    /// </summary>
    /// <param name="metricType">Metric type: 'agreement' or 'validity'</param>
    /// <param name="startDate">Start date (ISO 8601). Default: 30 days ago</param>
    /// <param name="endDate">End date (ISO 8601). Default: today</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of historical quality metrics</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<QualityMetricDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<QualityMetricDto>>> GetHistory(
        [FromQuery] string metricType,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(metricType))
        {
            return BadRequest(new { error = "Metric type is required" });
        }

        var metricTypeLower = metricType.ToLowerInvariant();
        if (metricTypeLower != "agreement" && metricTypeLower != "validity")
        {
            return BadRequest(new { error = "Invalid metric type. Must be 'agreement' or 'validity'" });
        }

        var start = startDate ?? DateTime.UtcNow.Date.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow.Date;

        if (start > end)
        {
            return BadRequest(new { error = "Start date must be before end date" });
        }

        _logger.LogInformation("Fetching quality metric history for {MetricType} from {StartDate} to {EndDate}",
            metricType, start.ToString("yyyy-MM-dd"), end.ToString("yyyy-MM-dd"));

        try
        {
            var normalizedMetricType = metricTypeLower == "agreement"
                ? "AIHumanAgreement"
                : "SchemaValidity";

            var daysDiff = (int)(end - start).TotalDays + 1; // Include end date
            var history = await _qualityMetricsService.GetMetricHistoryAsync(
                normalizedMetricType, daysDiff, cancellationToken);

            _logger.LogInformation("Retrieved {Count} historical records for {MetricType}", 
                history.Count, metricType);

            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metric history for {MetricType}", metricType);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving metric history" });
        }
    }

    /// <summary>
    /// Get rolling average for specified metric type.
    /// Calculates N-day rolling average for trend smoothing.
    /// </summary>
    /// <param name="metricType">Metric type: 'agreement' or 'validity'</param>
    /// <param name="days">Number of days for rolling average. Default: 7</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rolling average value</returns>
    [HttpGet("rolling-average")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetRollingAverage(
        [FromQuery] string metricType,
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(metricType))
        {
            return BadRequest(new { error = "Metric type is required" });
        }

        var metricTypeLower = metricType.ToLowerInvariant();
        if (metricTypeLower != "agreement" && metricTypeLower != "validity")
        {
            return BadRequest(new { error = "Invalid metric type. Must be 'agreement' or 'validity'" });
        }

        if (days < 1 || days > 90)
        {
            return BadRequest(new { error = "Days must be between 1 and 90" });
        }

        _logger.LogInformation("Calculating {Days}-day rolling average for {MetricType}", days, metricType);

        try
        {
            var normalizedMetricType = metricTypeLower == "agreement"
                ? "AIHumanAgreement"
                : "SchemaValidity";

            var rollingAverage = await _qualityMetricsService.CalculateRollingAverageAsync(
                normalizedMetricType, days, cancellationToken);

            _logger.LogInformation("{Days}-day rolling average for {MetricType}: {Value:F2}%",
                days, metricType, rollingAverage);

            return Ok(new
            {
                metricType = metricType,
                days = days,
                rollingAverage = Math.Round(rollingAverage, 2),
                calculatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating rolling average for {MetricType}", metricType);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while calculating rolling average" });
        }
    }
}
