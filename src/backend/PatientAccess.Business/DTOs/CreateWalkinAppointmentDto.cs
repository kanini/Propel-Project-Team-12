using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for creating walk-in appointments (US_029, AC-3).
/// Staff can immediately book appointments for walk-in patients.
/// IsWalkin flag defaults to true, appointment status defaults to Arrived.
/// </summary>
public class CreateWalkinAppointmentDto
{
    /// <summary>
    /// Patient unique identifier (User ID).
    /// </summary>
    [Required(ErrorMessage = "Patient ID is required")]
    public Guid PatientId { get; set; }

    /// <summary>
    /// Provider unique identifier.
    /// </summary>
    [Required(ErrorMessage = "Provider ID is required")]
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Time slot unique identifier for the appointment.
    /// Must be available (not booked) at the time of booking.
    /// </summary>
    [Required(ErrorMessage = "Time slot ID is required")]
    public Guid TimeSlotId { get; set; }

    /// <summary>
    /// Reason for walk-in visit (max 500 characters).
    /// </summary>
    [Required(ErrorMessage = "Visit reason is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Visit reason must be between 1 and 500 characters")]
    public string VisitReason { get; set; } = string.Empty;
}
