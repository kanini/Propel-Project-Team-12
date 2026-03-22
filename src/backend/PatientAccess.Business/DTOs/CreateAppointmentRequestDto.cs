using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for creating a new appointment (FR-008).
/// Validates required fields for appointment booking.
/// </summary>
public class CreateAppointmentRequestDto
{
    /// <summary>
    /// Provider ID for the appointment.
    /// </summary>
    [Required(ErrorMessage = "ProviderId is required")]
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Time slot ID to book.
    /// </summary>
    [Required(ErrorMessage = "TimeSlotId is required")]
    public Guid TimeSlotId { get; set; }

    /// <summary>
    /// Reason for visit.
    /// </summary>
    [Required(ErrorMessage = "VisitReason is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "VisitReason must be between 1 and 500 characters")]
    public string VisitReason { get; set; } = string.Empty;

    /// <summary>
    /// Optional preferred time slot ID for dynamic slot swap (FR-010).
    /// When specified, system will automatically swap to this slot when available.
    /// </summary>
    public Guid? PreferredSlotId { get; set; }
}
