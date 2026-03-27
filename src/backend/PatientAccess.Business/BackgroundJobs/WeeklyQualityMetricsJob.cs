using Hangfire;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for weekly quality metrics calculation (AIR-Q01, AIR-Q03).
/// Runs weekly on Mondays at 3:00 AM UTC to calculate AI-Human Agreement Rate and
/// Schema Validity for the previous week (Monday to Sunday).
/// </summary>
public class WeeklyQualityMetricsJob
{
    private readonly IQualityMetricsService _qualityMetricsService;
    private readonly ILogger<WeeklyQualityMetricsJob> _logger;

    public WeeklyQualityMetricsJob(
        IQualityMetricsService qualityMetricsService,
        ILogger<WeeklyQualityMetricsJob> logger)
    {
        _qualityMetricsService = qualityMetricsService ?? throw new ArgumentNullException(nameof(qualityMetricsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes weekly quality metrics calculation job.
    /// Calculates metrics for previous week (last Monday 00:00 to this Monday 00:00).
    /// </summary>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 300, 600, 1800 })]
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        
        // Calculate last Monday (7 days ago if today is Monday, otherwise go back to previous Monday)
        var daysUntilMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var lastMonday = today.AddDays(-daysUntilMonday - 7);
        
        var periodStart = lastMonday;
        var periodEnd = lastMonday.AddDays(7);

        _logger.LogInformation("Running Weekly Quality Metrics Job for week {Start} to {End}",
            periodStart.ToString("yyyy-MM-dd"), periodEnd.ToString("yyyy-MM-dd"));

        try
        {
            // Calculate AI-Human Agreement Rate (AIR-Q01)
            _logger.LogInformation("Calculating weekly AI-Human Agreement Rate");
            var agreementMetric = await _qualityMetricsService
                .CalculateAIHumanAgreementRateAsync(periodStart, periodEnd, cancellationToken);

            _logger.LogInformation("Weekly AI-Human Agreement Rate: {Rate:F2}% (Sample: {Count}, Status: {Status})",
                agreementMetric.MetricValue, agreementMetric.SampleSize, agreementMetric.Status);

            // Calculate Schema Validity Rate (AIR-Q03)
            _logger.LogInformation("Calculating weekly Schema Validity Rate");
            var schemaMetric = await _qualityMetricsService
                .CalculateSchemaValidityRateAsync(periodStart, periodEnd, cancellationToken);

            _logger.LogInformation("Weekly Schema Validity Rate: {Rate:F2}% (Sample: {Count}, Status: {Status})",
                schemaMetric.MetricValue, schemaMetric.SampleSize, schemaMetric.Status);

            _logger.LogInformation("Weekly Quality Metrics Job completed successfully for week {Start} to {End}",
                periodStart.ToString("yyyy-MM-dd"), periodEnd.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Weekly Quality Metrics Job failed for week {Start} to {End}",
                periodStart.ToString("yyyy-MM-dd"), periodEnd.ToString("yyyy-MM-dd"));
            throw; // Rethrow for Hangfire retry mechanism
        }
    }
}
