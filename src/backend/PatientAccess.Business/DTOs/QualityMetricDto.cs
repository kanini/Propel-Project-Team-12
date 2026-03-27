namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for quality metric data (AIR-Q01, AIR-Q03).
/// Used for dashboard display and API responses.
/// </summary>
public class QualityMetricDto
{
    /// <summary>
    /// Type of metric: "AIHumanAgreement", "SchemaValidity"
    /// </summary>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Metric value as percentage (0-100).
    /// Example: 98.5 for 98.5% agreement rate
    /// </summary>
    public decimal MetricValue { get; set; }

    /// <summary>
    /// Number of records evaluated for this metric.
    /// </summary>
    public int SampleSize { get; set; }

    /// <summary>
    /// Measurement period granularity: "Daily", "Weekly", "Monthly"
    /// </summary>
    public string MeasurementPeriod { get; set; } = string.Empty;

    /// <summary>
    /// Start of measurement period (UTC).
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// End of measurement period (UTC).
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Target threshold for this metric (percentage).
    /// Example: 98.0 for 98% AI-Human Agreement Rate (AIR-Q01)
    /// Example: 99.0 for 99% schema validity (AIR-Q03)
    /// </summary>
    public decimal Target { get; set; }

    /// <summary>
    /// Status compared to target: "MeetsTarget", "BelowTarget", "ExceedsTarget"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when metric was calculated (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Summary DTO containing current metrics and trend data.
/// Used for quality dashboard overview.
/// </summary>
public class QualityMetricsSummaryDto
{
    /// <summary>
    /// Current AI-Human Agreement Rate metric.
    /// </summary>
    public QualityMetricDto? AgreementRate { get; set; }

    /// <summary>
    /// Current schema validity metric.
    /// </summary>
    public QualityMetricDto? SchemaValidity { get; set; }

    /// <summary>
    /// Last 7 days of metrics for trend analysis.
    /// </summary>
    public List<QualityMetricDto> Last7Days { get; set; } = new();

    /// <summary>
    /// 7-day rolling average for trend detection.
    /// </summary>
    public decimal SevenDayRollingAverage { get; set; }

    /// <summary>
    /// 30-day rolling average for long-term trend.
    /// </summary>
    public decimal ThirtyDayRollingAverage { get; set; }
}
