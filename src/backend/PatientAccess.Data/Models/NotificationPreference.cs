namespace PatientAccess.Data.Models;

/// <summary>
/// Notification preference for waitlist entries (DR-011).
/// Maps to: 1=Email, 2=SMS, 3=Both
/// </summary>
public enum NotificationPreference
{
    Email = 1,
    SMS = 2,
    Both = 3
}
