using System.ComponentModel.DataAnnotations;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for joining waitlist (FR-009).
/// </summary>
public class JoinWaitlistRequestDto
{
    /// <summary>
    /// Provider ID for waitlist.
    /// </summary>
    [Required(ErrorMessage = "ProviderId is required")]
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Preferred date range start.
    /// </summary>
    [Required(ErrorMessage = "PreferredStartDate is required")]
    public DateOnly PreferredStartDate { get; set; }

    /// <summary>
    /// Preferred date range end.
    /// </summary>
    [Required(ErrorMessage = "PreferredEndDate is required")]
    public DateOnly PreferredEndDate { get; set; }

    /// <summary>
    /// Notification channel preference (SMS, Email, Both).
    /// </summary>
    [Required(ErrorMessage = "NotificationPreference is required")]
    public NotificationPreference NotificationPreference { get; set; }

    /// <summary>
    /// Optional reason for waitlist.
    /// </summary>
    [StringLength(500)]
    public string? Reason { get; set; }
}
