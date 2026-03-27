using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientAccess.Data.Models;

/// <summary>
/// Tracks quality metrics for AI code mapping to ensure AI-Human Agreement Rate >98% (AIR-Q01)
/// and output schema validity >99% (AIR-Q03).
/// US_051 Task 1 - Quality metrics tracking.
/// </summary>
[Table("QualityMetrics")]
public class QualityMetric
{
    /// <summary>
    /// Primary key (UUID).
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Metric type identifier: "AIHumanAgreement", "SchemaValidity", "ConfidenceAccuracy".
    /// Used for filtering and reporting different quality dimensions.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string MetricType { get; set; } = string.Empty;

    /// <summary>
    /// Metric value as percentage (0-100).
    /// For AI-Human Agreement: percentage of top suggestions verified by staff.
    /// For Schema Validity: percentage of responses passing validation.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal MetricValue { get; set; }

    /// <summary>
    /// Number of records evaluated for this metric calculation.
    /// Used for statistical significance assessment.
    /// </summary>
    [Required]
    public int SampleSize { get; set; }

    /// <summary>
    /// Measurement period identifier: "Daily", "Weekly", "Monthly", "Custom".
    /// Used for time-series analysis and trend reporting.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string MeasurementPeriod { get; set; } = string.Empty;

    /// <summary>
    /// Start timestamp of measurement period (UTC).
    /// </summary>
    [Required]
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// End timestamp of measurement period (UTC).
    /// </summary>
    [Required]
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Target threshold for this metric (e.g., 98.0 for AI-Human Agreement per AIR-Q01).
    /// Used for automated alerting when metrics fall below target.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Target { get; set; }

    /// <summary>
    /// Status relative to target: "BelowTarget", "MeetsTarget", "ExceedsTarget".
    /// Used for dashboard visualization and alerting workflows.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Optional notes for context (e.g., reasons for metric drop, mitigation actions).
    /// Max 2000 characters for detailed explanations.
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Record creation timestamp (UTC).
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
