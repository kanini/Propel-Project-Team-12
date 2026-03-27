namespace PatientAccess.Data.Models;

/// <summary>
/// Quality metric for AI performance tracking (AIR-Q01, AIR-Q03).
/// Tracks AI-Human Agreement Rate, schema validity, and confidence accuracy.
/// </summary>
public class QualityMetric
{
    public Guid QualityMetricId { get; set; }

    /// <summary>
    /// Type of metric being tracked.
    /// Examples: "AIHumanAgreement", "SchemaValidity", "ConfidenceAccuracy"
    /// </summary>
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Metric value as percentage (0-100).
    /// </summary>
    public decimal MetricValue { get; set; }

    /// <summary>
    /// Number of records evaluated for this metric.
    /// </summary>
    public int SampleSize { get; set; }

    /// <summary>
    /// Measurement period granularity.
    /// Examples: "Daily", "Weekly", "Monthly", "Custom"
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
    /// Example: 98.0 for 98% AI-Human Agreement Rate.
    /// </summary>
    public decimal Target { get; set; }

    /// <summary>
    /// Status compared to target threshold.
    /// Values: "BelowTarget", "MeetsTarget", "ExceedsTarget"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Optional notes about the metric measurement.
    /// </summary>
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
