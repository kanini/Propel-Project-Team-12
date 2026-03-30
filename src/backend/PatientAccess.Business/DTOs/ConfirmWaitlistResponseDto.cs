namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for waitlist slot confirmation (US_041 - AC-2).
/// </summary>
public class ConfirmWaitlistResponseDto
{
    /// <summary>
    /// Whether the confirmation was successful.
    /// False if slot was re-booked before confirmation (EC-2).
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable message about the confirmation result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Appointment ID if booking was successful. Null if slot unavailable.
    /// </summary>
    public Guid? AppointmentId { get; set; }
}
