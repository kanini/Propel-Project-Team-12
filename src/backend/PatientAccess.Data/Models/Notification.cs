namespace PatientAccess.Data.Models;

/// <summary>
/// Scheduled or delivered notification with delivery tracking (DR-014).
/// </summary>
public class Notification
{
    public Guid NotificationId { get; set; }

    public Guid RecipientId { get; set; }

    public Guid? AppointmentId { get; set; }

    public ChannelType ChannelType { get; set; }

    public string TemplateName { get; set; } = string.Empty;

    public NotificationStatus Status { get; set; }

    public DateTime ScheduledTime { get; set; }

    public DateTime? SentTime { get; set; }

    public DateTime? DeliveryConfirmation { get; set; }

    public int RetryCount { get; set; }

    public string? LastErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User Recipient { get; set; } = null!;
    public Appointment? Appointment { get; set; }
}
