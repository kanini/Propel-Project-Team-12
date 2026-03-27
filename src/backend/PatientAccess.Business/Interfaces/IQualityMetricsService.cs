using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for calculating and tracking quality metrics.
/// US_051 Task 3 - Quality Metrics Tracking.
/// Implements AIR-Q01 (AI-Human Agreement Rate >98%) and AIR-Q03 (Schema Validity >99%) tracking.
/// </summary>
public interface IQualityMetricsService
{
    /// <summary>
    /// Calculates AI-Human Agreement Rate for a given time period.
    /// Agreement = (StaffVerified TopSuggestions / TotalVerified TopSuggestions) * 100.
    /// Target: >98% per AIR-Q01.
    /// </summary>
    Task<QualityMetricDto> CalculateAIHumanAgreementRateAsync(
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates Schema Validity Rate for a given time period.
    /// Validity = (ValidResponses / TotalResponses) * 100.
    /// Target: >99% per AIR-Q03.
    /// </summary>
    Task<QualityMetricDto> CalculateSchemaValidityRateAsync(
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets daily quality metrics summary including last 7 days history.
    /// </summary>
    Task<QualityMetricsSummaryDto> GetDailySummaryAsync(
        DateTime date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets weekly quality metrics summary for a given week start date.
    /// </summary>
    Task<QualityMetricsSummaryDto> GetWeeklySummaryAsync(
        DateTime weekStart,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metric history for a specified number of days.
    /// </summary>
    /// <param name="metricType">"AIHumanAgreement" or "SchemaValidity"</param>
    /// <param name="days">Number of days to retrieve (default: 30)</param>
    Task<List<QualityMetricDto>> GetMetricHistoryAsync(
        string metricType,
        int days = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates rolling average for a metric over specified number of days.
    /// Used for trend analysis (e.g., 7-day or 30-day rolling average).
    /// </summary>
    Task<decimal> CalculateRollingAverageAsync(
        string metricType,
        int days = 7,
        CancellationToken cancellationToken = default);
}
