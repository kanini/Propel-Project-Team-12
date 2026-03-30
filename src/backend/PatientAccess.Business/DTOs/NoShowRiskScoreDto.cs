namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for no-show risk score calculation (US_038 - FR-023).
/// Contains calculated risk score (0-100), derived risk level, and individual factor breakdown.
/// </summary>
public class NoShowRiskScoreDto
{
    /// <summary>
    /// Final calculated risk score (0-100).
    /// Weighted combination of lead time, history, and confirmation factors.
    /// </summary>
    public decimal Score { get; set; }

    /// <summary>
    /// Risk level derived from score.
    /// "Low" (< 40), "Medium" (40-70), "High" (> 70).
    /// </summary>
    public string RiskLevel { get; set; } = string.Empty;

    /// <summary>
    /// Lead time factor contribution (0-100 raw factor before weighting).
    /// Based on time until scheduled appointment.
    /// </summary>
    public decimal LeadTimeFactor { get; set; }

    /// <summary>
    /// No-show history factor contribution (0-100 raw factor before weighting).
    /// Based on patient's previous no-show rate.
    /// </summary>
    public decimal HistoryFactor { get; set; }

    /// <summary>
    /// Confirmation response rate factor contribution (0-100 raw factor before weighting).
    /// Based on patient's historical confirmation responsiveness.
    /// </summary>
    public decimal ConfirmationFactor { get; set; }
}
