namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for displaying notifications on patient dashboard (US_067).
/// Contains notification details with action links.
/// </summary>
public class NotificationDto
{
    /// <summary>
    /// Unique notification identifier.
    /// </summary>
    public Guid NotificationId { get; set; }

    /// <summary>
    /// Notification title (e.g., "Slot Swap Available", "Appointment Reminder").
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message body.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when notification was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Optional action link for navigation (e.g., "/appointments/123").
    /// </summary>
    public string? ActionLink { get; set; }

    /// <summary>
    /// Action button label (e.g., "View Appointment", "Upload Document").
    /// </summary>
    public string? ActionLabel { get; set; }

    /// <summary>
    /// Indicates if notification has been read by user.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Notification type/channel for display purposes.
    /// </summary>
    public string NotificationType { get; set; } = string.Empty;
}
