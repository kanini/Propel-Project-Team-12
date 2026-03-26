namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for calendar event data used in Google Calendar integration (US_039 - FR-024).
/// Contains appointment details formatted for external calendar APIs.
/// </summary>
public class CalendarEventDto
{
    /// <summary>
    /// Event title displayed in calendar, e.g., "Appointment with Dr. Smith".
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Event start date and time (UTC).
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Event end date and time (UTC).
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Event description including visit reason and patient notes.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Event location formatted as "Provider Name - Specialty".
    /// </summary>
    public string Location { get; set; } = string.Empty;
}
