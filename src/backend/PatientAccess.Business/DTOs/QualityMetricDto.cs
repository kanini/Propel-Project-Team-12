namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO representing a single quality metric measurement.
/// US_051 Task 3 - Quality Metrics Tracking.
/// Used for tracking AIR-Q01 (AI-Human Agreement >98%) and AIR-Q03 (Schema Validity >99%).
/// </summary>
public class QualityMetricDto
{
    /// <summary>
    /// Metric type identifier: "AIHumanAgreement" or "SchemaValidity".
    /// </summary>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Metric value as percentage (0-100).
    /// For AIHumanAgreement: (StaffVerified / TotalVerified) * 100.
    /// For SchemaValidity: (ValidResponses / TotalResponses) * 100.
    /// </summary>
    public decimal MetricValue { get; set; }

    /// <summary>
    /// Number of records evaluated for this metric.
    /// </summary>
    public int SampleSize { get; set; }

    /// <summary>
    /// Measurement period identifier: "Daily", "Weekly", "Monthly", "Custom".
    /// </summary>
    public string MeasurementPeriod { get; set; } = string.Empty;

    /// <summary>
    /// Start timestamp of measurement period (UTC).
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// End timestamp of measurement period (UTC).
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Target threshold for this metric (e.g., 98.0 for AIHumanAgreement).
    /// </summary>
    public decimal Target { get; set; }

    /// <summary>
    /// Status relative to target: "MeetsTarget", "BelowTarget", "ExceedsTarget".
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Optional notes for context.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Record creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO representing a summary of quality metrics for a time period.
/// Includes current metrics and historical trend data.
/// </summary>
public class QualityMetricsSummaryDto
{
    /// <summary>
    /// Current AI-Human Agreement Rate metric.
    /// </summary>
    public QualityMetricDto? AgreementRate { get; set; }

    /// <summary>
    /// Current Schema Validity metric.
    /// </summary>
    public QualityMetricDto? SchemaValidity { get; set; }

    /// <summary>
    /// Historical metrics for the last 7 days.
    /// </summary>
    public List<QualityMetricDto> Last7Days { get; set; } = new();

    /// <summary>
    /// 7-day rolling average for agreement rate.
    /// </summary>
    public decimal SevenDayRollingAverage { get; set; }

    /// <summary>
    /// 30-day rolling average for long-term trend analysis.
    /// </summary>
    public decimal ThirtyDayRollingAverage { get; set; }

    /// <summary>
    /// Overall status: "Healthy" if both metrics meet targets, "AtRisk" if one below, "Critical" if both below.
    /// </summary>
    public string OverallStatus { get; set; } = string.Empty;
}
