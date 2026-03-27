using Hangfire;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for daily quality metrics calculation (AIR-Q01, AIR-Q03).
/// Runs daily at 2:00 AM UTC to calculate AI-Human Agreement Rate and Schema Validity
/// for the previous day.
/// </summary>
public class DailyQualityMetricsJob
{
    private readonly IQualityMetricsService _qualityMetricsService;
    private readonly ILogger<DailyQualityMetricsJob> _logger;

    public DailyQualityMetricsJob(
        IQualityMetricsService qualityMetricsService,
        ILogger<DailyQualityMetricsJob> logger)
    {
        _qualityMetricsService = qualityMetricsService ?? throw new ArgumentNullException(nameof(qualityMetricsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes daily quality metrics calculation job.
    /// Calculates metrics for previous day (yesterday 00:00 to today 00:00).
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 300, 600, 1800 })]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var periodStart = yesterday;
        var periodEnd = yesterday.AddDays(1);

        _logger.LogInformation("Running Daily Quality Metrics Job for {Date}", yesterday.ToString("yyyy-MM-dd"));

        try
        {
            // Calculate AI-Human Agreement Rate (AIR-Q01)
            _logger.LogInformation("Calculating AI-Human Agreement Rate for {Date}", yesterday.ToString("yyyy-MM-dd"));
            var agreementMetric = await _qualityMetricsService
                .CalculateAIHumanAgreementRateAsync(periodStart, periodEnd, cancellationToken);

            _logger.LogInformation("AI-Human Agreement Rate calculated: {Rate:F2}% (Sample: {Count}, Status: {Status})",
                agreementMetric.MetricValue, agreementMetric.SampleSize, agreementMetric.Status);

            // Calculate Schema Validity Rate (AIR-Q03)
            _logger.LogInformation("Calculating Schema Validity Rate for {Date}", yesterday.ToString("yyyy-MM-dd"));
            var schemaMetric = await _qualityMetricsService
                .CalculateSchemaValidityRateAsync(periodStart, periodEnd, cancellationToken);

            _logger.LogInformation("Schema Validity Rate calculated: {Rate:F2}% (Sample: {Count}, Status: {Status})",
                schemaMetric.MetricValue, schemaMetric.SampleSize, schemaMetric.Status);

            _logger.LogInformation("Daily Quality Metrics Job completed successfully for {Date}",
                yesterday.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Daily Quality Metrics Job failed for {Date}", yesterday.ToString("yyyy-MM-dd"));
            throw; // Rethrow for Hangfire retry mechanism
        }
    }
}
