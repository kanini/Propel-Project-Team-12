using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for quality metrics calculation and tracking (AIR-Q01, AIR-Q03).
/// Monitors AI-Human Agreement Rate and output schema validity.
/// </summary>
public interface IQualityMetricsService
{
    /// <summary>
    /// Calculates AI-Human Agreement Rate for specified period (AIR-Q01: >98%).
    /// Agreement = (StaffVerified top suggestions / Total verified) * 100
    /// </summary>
    /// <param name="periodStart">Start of measurement period (UTC)</param>
    /// <param name="periodEnd">End of measurement period (UTC)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Quality metric DTO with agreement rate</returns>
    Task<QualityMetricDto> CalculateAIHumanAgreementRateAsync(
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates schema validity rate for specified period (AIR-Q03: >99%).
    /// Validity = (Valid schema responses / Total responses) * 100
    /// </summary>
    /// <param name="periodStart">Start of measurement period (UTC)</param>
    /// <param name="periodEnd">End of measurement period (UTC)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Quality metric DTO with schema validity rate</returns>
    Task<QualityMetricDto> CalculateSchemaValidityRateAsync(
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves daily quality metrics summary for specified date.
    /// Includes agreement rate, schema validity, and 7-day trend.
    /// </summary>
    /// <param name="date">Target date for summary</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Summary with current metrics and trends</returns>
    Task<QualityMetricsSummaryDto> GetDailySummaryAsync(
        DateTime date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves weekly quality metrics summary starting from specified date.
    /// </summary>
    /// <param name="weekStart">Start of week (Monday)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Summary with weekly metrics and trends</returns>
    Task<QualityMetricsSummaryDto> GetWeeklySummaryAsync(
        DateTime weekStart,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves metric history for specified number of days.
    /// Used for dashboard trend charts.
    /// </summary>
    /// <param name="metricType">Metric type: "AIHumanAgreement" or "SchemaValidity"</param>
    /// <param name="days">Number of days to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of metrics ordered by date descending</returns>
    Task<List<QualityMetricDto>> GetMetricHistoryAsync(
        string metricType,
        int days,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates rolling average for specified metric over N days.
    /// Used for trend detection and alert thresholds.
    /// </summary>
    /// <param name="metricType">Metric type: "AIHumanAgreement" or "SchemaValidity"</param>
    /// <param name="days">Number of days for rolling average (7 or 30 recommended)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rolling average as percentage</returns>
    Task<decimal> CalculateRollingAverageAsync(
        string metricType,
        int days,
        CancellationToken cancellationToken = default);
}
