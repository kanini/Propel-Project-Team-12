namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for staff dashboard queue preview (US_068, AC4).
/// Represents a single patient in today's queue for the dashboard preview widget.
/// </summary>
public class QueuePreviewDto
{
    /// <summary>
    /// Unique appointment identifier.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Patient full name for display.
    /// </summary>
    public string PatientName { get; set; } = string.Empty;

    /// <summary>
    /// Provider name for display.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Scheduled appointment time in ISO 8601 format (e.g., "2026-03-25T09:00:00Z").
    /// </summary>
    public DateTime AppointmentTime { get; set; }

    /// <summary>
    /// Estimated wait time in human-readable format (e.g., "15 mins", "1 hour").
    /// Calculated based on current queue position and average appointment duration.
    /// </summary>
    public string EstimatedWait { get; set; } = string.Empty;

    /// <summary>
    /// No-show risk level for the appointment (FR-023).
    /// Possible values: "low", "medium", "high".
    /// MVP: Placeholder value "low" until FR-023 risk assessment is implemented.
    /// </summary>
    public string RiskLevel { get; set; } = "low";

    /// <summary>
    /// Appointment status (e.g., "Scheduled", "Waiting", "In-Progress", "Completed").
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
