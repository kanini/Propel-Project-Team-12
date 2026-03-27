using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// API endpoints for quality metrics dashboard and monitoring.
/// US_051 Task 3 - Quality Metrics Tracking.
/// Provides access to AI-Human Agreement Rate (AIR-Q01: >98%) and Schema Validity (AIR-Q03: >99%) metrics.
/// </summary>
[ApiController]
[Route("api/quality-metrics")]
[Authorize(Roles = "Admin")]
public class QualityMetricsController : ControllerBase
{
    private readonly ILogger<QualityMetricsController> _logger;
    private readonly IQualityMetricsService _qualityMetricsService;

    public QualityMetricsController(
        ILogger<QualityMetricsController> logger,
        IQualityMetricsService qualityMetricsService)
    {
        _logger = logger;
        _qualityMetricsService = qualityMetricsService;
    }

    /// <summary>
    /// Gets daily quality metrics summary including last 7 days history and rolling averages.
    /// Defaults to current date if not specified.
    /// </summary>
    /// <param name="date">Target date (UTC) for summary (default: today)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Quality metrics summary with agreement rate, schema validity, and trend data</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(QualityMetricsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? date = null,
        CancellationToken cancellationToken = default)
    {
        var targetDate = date ?? DateTime.UtcNow.Date;

        _logger.LogInformation(
            "Retrieving quality metrics summary for {Date} by user {User}",
            targetDate.ToString("yyyy-MM-dd"),
            User.Identity?.Name);

        var summary = await _qualityMetricsService.GetDailySummaryAsync(targetDate, cancellationToken);

        return Ok(summary);
    }

    /// <summary>
    /// Gets historical quality metrics for a specified metric type and time range.
    /// Useful for trend analysis and charting.
    /// </summary>
    /// <param name="metricType">Metric type: "AIHumanAgreement" or "SchemaValidity"</param>
    /// <param name="days">Number of days of history to retrieve (default: 30, max: 365)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of quality metrics ordered by period start date (descending)</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<QualityMetricDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string metricType,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(metricType))
        {
            return BadRequest(new { Error = "MetricType query parameter is required" });
        }

        if (metricType != "AIHumanAgreement" && metricType != "SchemaValidity" && metricType != "SchemaValidityAggregate")
        {
            return BadRequest(new
            {
                Error = $"Invalid MetricType: {metricType}. Must be 'AIHumanAgreement', 'SchemaValidity', or 'SchemaValidityAggregate'"
            });
        }

        if (days < 1 || days > 365)
        {
            return BadRequest(new { Error = "Days must be between 1 and 365" });
        }

        _logger.LogInformation(
            "Retrieving {Days} days of {MetricType} history for user {User}",
            days, metricType, User.Identity?.Name);

        var history = await _qualityMetricsService.GetMetricHistoryAsync(metricType, days, cancellationToken);

        return Ok(history);
    }

    /// <summary>
    /// Gets rolling average for a specified metric type over N days.
    /// Useful for smoothing daily fluctuations and identifying trends.
    /// </summary>
    /// <param name="metricType">Metric type: "AIHumanAgreement" or "SchemaValidity"</param>
    /// <param name="days">Number of days for rolling average window (default: 7, options: 7, 14, 30)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rolling average percentage</returns>
    [HttpGet("rolling-average")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRollingAverage(
        [FromQuery] string metricType,
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(metricType))
        {
            return BadRequest(new { Error = "MetricType query parameter is required" });
        }

        if (metricType != "AIHumanAgreement" && metricType != "SchemaValidity" && metricType != "SchemaValidityAggregate")
        {
            return BadRequest(new
            {
                Error = $"Invalid MetricType: {metricType}. Must be 'AIHumanAgreement' or 'SchemaValidity'"
            });
        }

        if (days < 1 || days > 90)
        {
            return BadRequest(new { Error = "Days must be between 1 and 90" });
        }

        _logger.LogInformation(
            "Calculating {Days}-day rolling average for {MetricType} by user {User}",
            days, metricType, User.Identity?.Name);

        var average = await _qualityMetricsService.CalculateRollingAverageAsync(metricType, days, cancellationToken);

        return Ok(new
        {
            MetricType = metricType,
            Days = days,
            RollingAverage = average,
            CalculatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets weekly quality metrics summary for a specified week.
    /// Week starts on Monday and ends on Sunday.
    /// </summary>
    /// <param name="weekStart">Start date of the week (should be a Monday, default: last Monday)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Weekly quality metrics summary</returns>
    [HttpGet("weekly-summary")]
    [ProducesResponseType(typeof(QualityMetricsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWeeklySummary(
        [FromQuery] DateTime? weekStart = null,
        CancellationToken cancellationToken = default)
    {
        // Calculate last Monday if not specified
        DateTime targetWeekStart;
        if (weekStart.HasValue)
        {
            targetWeekStart = weekStart.Value.Date;
        }
        else
        {
            var today = DateTime.UtcNow.Date;
            var daysFromMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            targetWeekStart = today.AddDays(-daysFromMonday - 7); // Last Monday
        }

        _logger.LogInformation(
            "Retrieving weekly quality metrics summary for week starting {WeekStart} by user {User}",
            targetWeekStart.ToString("yyyy-MM-dd"),
            User.Identity?.Name);

        var summary = await _qualityMetricsService.GetWeeklySummaryAsync(targetWeekStart, cancellationToken);

        return Ok(summary);
    }

    /// <summary>
    /// Triggers manual calculation of quality metrics for a specified date.
    /// Useful for backfilling metrics or recalculating after data corrections.
    /// </summary>
    /// <param name="date">Target date for metric calculation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Calculated metrics</returns>
    [HttpPost("calculate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CalculateMetrics(
        [FromQuery] DateTime date,
        CancellationToken cancellationToken = default)
    {
        var periodStart = date.Date;
        var periodEnd = periodStart.AddDays(1);

        _logger.LogInformation(
            "Manual quality metrics calculation triggered for {Date} by user {User}",
            date.ToString("yyyy-MM-dd"),
            User.Identity?.Name);

        try
        {
            // Calculate both metrics
            var agreementMetric = await _qualityMetricsService
                .CalculateAIHumanAgreementRateAsync(periodStart, periodEnd, cancellationToken);

            var schemaMetric = await _qualityMetricsService
                .CalculateSchemaValidityRateAsync(periodStart, periodEnd, cancellationToken);

            return Ok(new
            {
                Message = "Quality metrics calculated successfully",
                Date = date.ToString("yyyy-MM-dd"),
                AgreementRate = agreementMetric,
                SchemaValidity = schemaMetric
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Manual quality metrics calculation failed for {Date}",
                date.ToString("yyyy-MM-dd"));

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Error = "Quality metrics calculation failed",
                Details = ex.Message
            });
        }
    }
}
