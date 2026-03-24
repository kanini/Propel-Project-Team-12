namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for displaying upcoming appointments on dashboard (US_067).
/// Contains essential appointment details for list view.
/// </summary>
public class UpcomingAppointmentDto
{
    /// <summary>
    /// Unique appointment identifier.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Provider full name.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Provider specialty (e.g., Cardiology, Dermatology).
    /// </summary>
    public string ProviderSpecialty { get; set; } = string.Empty;

    /// <summary>
    /// Scheduled appointment date and time in UTC.
    /// </summary>
    public DateTime ScheduledDateTime { get; set; }

    /// <summary>
    /// Appointment status: Confirmed, Waitlist, Pending.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Unique confirmation number for appointment.
    /// </summary>
    public string ConfirmationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Visit reason for the appointment.
    /// </summary>
    public string VisitReason { get; set; } = string.Empty;
}
