namespace PatientAccess.Data.Models;

/// <summary>
/// Patient-level aggregated no-show metrics for FR-023 risk assessment (DR-016).
/// </summary>
public class NoShowHistory
{
    public Guid NoShowHistoryId { get; set; }

    public Guid PatientId { get; set; }

    public int TotalAppointments { get; set; }

    public int NoShowCount { get; set; }

    /// <summary>
    /// Percentage (0.00 to 100.00) of confirmations responded to.
    /// </summary>
    public decimal? ConfirmationResponseRate { get; set; }

    /// <summary>
    /// Average lead time in hours for no-show appointments.
    /// </summary>
    public decimal? AverageLeadTimeHours { get; set; }

    public decimal? LastCalculatedRiskScore { get; set; }

    public DateTime? LastCalculatedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User Patient { get; set; } = null!;
}
