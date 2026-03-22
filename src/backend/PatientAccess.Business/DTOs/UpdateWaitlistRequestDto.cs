using System.ComponentModel.DataAnnotations;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for updating waitlist preferences (FR-009).
/// </summary>
public class UpdateWaitlistRequestDto
{
    [Required]
    public DateOnly PreferredStartDate { get; set; }

    [Required]
    public DateOnly PreferredEndDate { get; set; }

    [Required]
    public NotificationPreference NotificationPreference { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}
