/**
 * ArrivalSearchResultDto - DTO for Appointment Search Results in Arrival Management
 * Used for displaying appointments in arrival search
 */

using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for appointment search results in arrival management (US_031)
/// </summary>
public class ArrivalSearchResultDto
{
    /// <summary>
    /// Appointment ID (primary key)
    /// </summary>
    [Required]
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Patient ID (foreign key)
    /// </summary>
    [Required]
    public Guid PatientId { get; set; }

    /// <summary>
    /// Patient full name
    /// </summary>
    [Required]
    public string PatientName { get; set; } = string.Empty;

    /// <summary>
    /// Patient date of birth (YYYY-MM-DD format)
    /// </summary>
    [Required]
    public string DateOfBirth { get; set; } = string.Empty;

    /// <summary>
    /// Scheduled appointment date and time
    /// </summary>
    [Required]
    public DateTime ScheduledDateTime { get; set; }

    /// <summary>
    /// Provider full name
    /// </summary>
    [Required]
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Visit reason or appointment reason
    /// </summary>
    public string VisitReason { get; set; } = string.Empty;

    /// <summary>
    /// Appointment status: Scheduled, Confirmed, Arrived, Cancelled, Completed, NoShow
    /// </summary>
    [Required]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Calculated no-show risk score (0-100). Null for legacy appointments without scoring (US_038 - FR-023).
    /// </summary>
    public decimal? NoShowRiskScore { get; set; }

    /// <summary>
    /// Risk level derived from score: "Low" (< 40), "Medium" (40-70), "High" (> 70).
    /// Null when score is null (US_038 - FR-023).
    /// </summary>
    public string? RiskLevel { get; set; }
}
