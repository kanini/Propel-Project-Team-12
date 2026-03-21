namespace PatientAccess.Data.Models;

/// <summary>
/// Waitlist entry status values (DR-011).
/// Maps to: 1=Active, 2=Notified, 3=Fulfilled, 4=Cancelled
/// </summary>
public enum WaitlistStatus
{
    Active = 1,
    Notified = 2,
    Fulfilled = 3,
    Cancelled = 4
}
