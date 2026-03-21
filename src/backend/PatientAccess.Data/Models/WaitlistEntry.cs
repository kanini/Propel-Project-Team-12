namespace PatientAccess.Data.Models;

/// <summary>
/// Patient preference record for unavailable appointment slots (DR-011).
/// </summary>
public class WaitlistEntry
{
    public Guid WaitlistEntryId { get; set; }

    public Guid PatientId { get; set; }

    public Guid ProviderId { get; set; }

    public DateOnly PreferredDateStart { get; set; }

    public DateOnly PreferredDateEnd { get; set; }

    public PreferredTimeOfDay? PreferredTimeOfDay { get; set; }

    public NotificationPreference NotificationPreference { get; set; }

    public int Priority { get; set; } = 1;

    public WaitlistStatus Status { get; set; } = WaitlistStatus.Active;

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User Patient { get; set; } = null!;
    public Provider Provider { get; set; } = null!;
}
