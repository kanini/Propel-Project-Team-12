using Hangfire;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for calculating weekly quality metrics.
/// US_051 Task 3 - Quality Metrics Tracking.
/// Runs weekly on Mondays at 3:00 AM UTC to calculate metrics for previous week (Monday to Sunday).
/// </summary>
public class WeeklyQualityMetricsJob
{
    private readonly IQualityMetricsService _qualityMetricsService;
    private readonly ILogger<WeeklyQualityMetricsJob> _logger;

    public WeeklyQualityMetricsJob(
        IQualityMetricsService qualityMetricsService,
        ILogger<WeeklyQualityMetricsJob> logger)
    {
        _qualityMetricsService = qualityMetricsService;
        _logger = logger;
    }

    /// <summary>
    /// Executes weekly quality metrics calculation.
    /// Calculates metrics for the previous week (last Monday 00:00 to last Sunday 23:59).
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 300, 900, 1800 })] // Retry: 5min, 15min, 30min
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // Calculate previous week start (last Monday)
        var today = DateTime.UtcNow.Date;
        var daysFromMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var thisWeekMonday = today.AddDays(-daysFromMonday);
        var lastWeekMonday = thisWeekMonday.AddDays(-7);
        var lastWeekSunday = lastWeekMonday.AddDays(7);

        _logger.LogInformation(
            "Starting Weekly Quality Metrics Job for week {Start} to {End}",
            lastWeekMonday.ToString("yyyy-MM-dd"), lastWeekSunday.ToString("yyyy-MM-dd"));

        try
        {
            // Calculate AI-Human Agreement Rate for previous week
            var agreementMetric = await _qualityMetricsService
                .CalculateAIHumanAgreementRateAsync(lastWeekMonday, lastWeekSunday, cancellationToken);

            _logger.LogInformation(
                "Weekly AI-Human Agreement Rate: {Rate}% (Sample: {Sample}, Status: {Status})",
                agreementMetric.MetricValue, agreementMetric.SampleSize, agreementMetric.Status);

            // Calculate Schema Validity Rate for previous week
            var schemaMetric = await _qualityMetricsService
                .CalculateSchemaValidityRateAsync(lastWeekMonday, lastWeekSunday, cancellationToken);

            _logger.LogInformation(
                "Weekly Schema Validity Rate: {Rate}% (Sample: {Sample}, Status: {Status})",
                schemaMetric.MetricValue, schemaMetric.SampleSize, schemaMetric.Status);

            // Calculate rolling averages for trend analysis
            var sevenDayAvg = await _qualityMetricsService
                .CalculateRollingAverageAsync("AIHumanAgreement", 7, cancellationToken);

            var thirtyDayAvg = await _qualityMetricsService
                .CalculateRollingAverageAsync("AIHumanAgreement", 30, cancellationToken);

            _logger.LogInformation(
                "Rolling Averages: 7-day={SevenDay}%, 30-day={ThirtyDay}%",
                sevenDayAvg, thirtyDayAvg);

            _logger.LogInformation(
                "Weekly Quality Metrics Job completed successfully for week {Start} to {End}",
                lastWeekMonday.ToString("yyyy-MM-dd"), lastWeekSunday.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Weekly Quality Metrics Job failed for week {Start} to {End}. Error: {Message}",
                lastWeekMonday.ToString("yyyy-MM-dd"), lastWeekSunday.ToString("yyyy-MM-dd"), ex.Message);
            throw; // Re-throw to trigger Hangfire automatic retry
        }
    }
}
