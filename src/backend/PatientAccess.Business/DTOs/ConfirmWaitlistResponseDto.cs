namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for waitlist slot confirmation (US_041/FR-026, AC-2).
/// Indicates success/failure, provides message, and returns AppointmentId if booked.
/// </summary>
public class ConfirmWaitlistResponseDto
{
    /// <summary>
    /// Whether the confirmation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// User-facing message explaining the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Created appointment ID if booking succeeded.
    /// </summary>
    public Guid? AppointmentId { get; set; }
}
