namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for successful appointment creation (FR-008).
/// Contains appointment details and confirmation number.
/// </summary>
public class AppointmentResponseDto
{
    /// <summary>
    /// Unique appointment identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Provider ID.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Provider name for display.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Provider specialty for display.
    /// </summary>
    public string ProviderSpecialty { get; set; } = string.Empty;

    /// <summary>
    /// Scheduled appointment date and time in UTC.
    /// </summary>
    public DateTime ScheduledDateTime { get; set; }

    /// <summary>
    /// Visit reason.
    /// </summary>
    public string VisitReason { get; set; } = string.Empty;

    /// <summary>
    /// Appointment status (Scheduled, Confirmed, etc.).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Unique 8-character alphanumeric confirmation number.
    /// </summary>
    public string ConfirmationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Optional preferred time slot ID for dynamic slot swap (FR-010, US_026).
    /// When set, indicates patient has a preferred slot swap preference active.
    /// Null means no swap preference.
    /// </summary>
    public Guid? PreferredSlotId { get; set; }

    /// <summary>
    /// Preferred slot start time (for display purposes).
    /// Only populated when PreferredSlotId is set.
    /// </summary>
    public DateTime? PreferredSlotStartTime { get; set; }
}
