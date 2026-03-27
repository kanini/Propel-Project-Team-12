using Hangfire;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for calculating daily quality metrics.
/// US_051 Task 3 - Quality Metrics Tracking.
/// Runs daily at 2:00 AM UTC to calculate AI-Human Agreement and Schema Validity rates for previous day.
/// </summary>
public class DailyQualityMetricsJob
{
    private readonly IQualityMetricsService _qualityMetricsService;
    private readonly ILogger<DailyQualityMetricsJob> _logger;

    public DailyQualityMetricsJob(
        IQualityMetricsService qualityMetricsService,
        ILogger<DailyQualityMetricsJob> logger)
    {
        _qualityMetricsService = qualityMetricsService;
        _logger = logger;
    }

    /// <summary>
    /// Executes daily quality metrics calculation.
    /// Calculates metrics for the previous day (yesterday 00:00 to yesterday 23:59).
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })] // Retry: 1min, 5min, 15min
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var periodStart = yesterday;
        var periodEnd = yesterday.AddDays(1);

        _logger.LogInformation(
            "Starting Daily Quality Metrics Job for {Date} ({Start} to {End})",
            yesterday.ToString("yyyy-MM-dd"), periodStart, periodEnd);

        try
        {
            // Calculate AI-Human Agreement Rate for yesterday
            var agreementMetric = await _qualityMetricsService
                .CalculateAIHumanAgreementRateAsync(periodStart, periodEnd, cancellationToken);

            _logger.LogInformation(
                "AI-Human Agreement Rate calculated: {Rate}% (Sample: {Sample}, Status: {Status})",
                agreementMetric.MetricValue, agreementMetric.SampleSize, agreementMetric.Status);

            // Calculate Schema Validity Rate for yesterday
            var schemaMetric = await _qualityMetricsService
                .CalculateSchemaValidityRateAsync(periodStart, periodEnd, cancellationToken);

            _logger.LogInformation(
                "Schema Validity Rate calculated: {Rate}% (Sample: {Sample}, Status: {Status})",
                schemaMetric.MetricValue, schemaMetric.SampleSize, schemaMetric.Status);

            _logger.LogInformation("Daily Quality Metrics Job completed successfully for {Date}", yesterday.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Daily Quality Metrics Job failed for {Date}. Error: {Message}",
                yesterday.ToString("yyyy-MM-dd"), ex.Message);
            throw; // Re-throw to trigger Hangfire automatic retry
        }
    }
}
