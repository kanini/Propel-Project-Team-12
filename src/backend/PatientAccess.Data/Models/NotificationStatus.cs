namespace PatientAccess.Data.Models;

/// <summary>
/// Notification delivery status (DR-014, US_037).
/// Maps to: 1=Pending, 2=Sent, 3=Failed, 4=Delivered, 5=Cancelled
/// Cancelled status supports edge case: appointment cancelled after reminders are scheduled.
/// </summary>
public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Delivered = 4,
    Cancelled = 5
}
