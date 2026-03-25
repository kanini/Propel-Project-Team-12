namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for intake appointment selection (US_037).
/// Contains appointment details with intake status for display in appointment selection screen.
/// </summary>
public class IntakeAppointmentDto
{
    /// <summary>
    /// Unique identifier for the appointment.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Provider unique identifier.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Full name of the provider (e.g., "Dr. Sarah Johnson").
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Provider's specialty (e.g., "Family Medicine", "Cardiology").
    /// </summary>
    public string ProviderSpecialty { get; set; } = string.Empty;

    /// <summary>
    /// Appointment date in ISO format (YYYY-MM-DD).
    /// </summary>
    public string AppointmentDate { get; set; } = string.Empty;

    /// <summary>
    /// Appointment time in 24-hour format (HH:mm).
    /// </summary>
    public string AppointmentTime { get; set; } = string.Empty;

    /// <summary>
    /// Current intake status: "Pending", "InProgress", or "Completed".
    /// </summary>
    public string IntakeStatus { get; set; } = "Pending";

    /// <summary>
    /// Intake session ID if an intake record exists (null if pending).
    /// </summary>
    public Guid? IntakeSessionId { get; set; }
}
