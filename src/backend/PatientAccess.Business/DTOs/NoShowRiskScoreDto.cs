namespace PatientAccess.Business.DTOs;

/// <summary>
/// US_038 AC-1: No-show risk score (0-100) with risk level classification and factor breakdown for audit/debugging.
/// </summary>
public class NoShowRiskScoreDto
{
    /// <summary>
    /// Final weighted risk score (0-100).
    /// </summary>
    public decimal Score { get; set; }

    /// <summary>
    /// Risk level classification: "Low" (&lt;40), "Medium" (40-70), "High" (&gt;70).
    /// </summary>
    public string RiskLevel { get; set; } = string.Empty;

    /// <summary>
    /// Lead time raw factor (0-100) before weight application.
    /// </summary>
    public decimal LeadTimeFactor { get; set; }

    /// <summary>
    /// No-show history raw factor (0-100) before weight application.
    /// </summary>
    public decimal HistoryFactor { get; set; }

    /// <summary>
    /// Confirmation response rate raw factor (0-100) before weight application.
    /// </summary>
    public decimal ConfirmationFactor { get; set; }
}
