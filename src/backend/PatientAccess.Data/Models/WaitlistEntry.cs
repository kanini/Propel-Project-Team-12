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

    // Notification lifecycle fields (US_041)
    /// <summary>
    /// Timestamp when notification was dispatched to the patient. Null when Status=Active.
    /// </summary>
    public DateTime? NotifiedAt { get; set; }

    /// <summary>
    /// Cryptographically random URL-safe token for securing confirm/decline links.
    /// 32 bytes base64url encoded. Unique index ensures O(1) lookup.
    /// </summary>
    public string? ResponseToken { get; set; }

    /// <summary>
    /// NotifiedAt + configured timeout (e.g., 30 minutes). Null when not notified.
    /// Used by timeout job to detect expired notifications.
    /// </summary>
    public DateTime? ResponseDeadline { get; set; }

    /// <summary>
    /// FK to the specific TimeSlot offered to this patient.
    /// Used for availability re-check on confirm (edge case: slot booked by another patient).
    /// </summary>
    public Guid? NotifiedSlotId { get; set; }

    // Navigation properties
    public User Patient { get; set; } = null!;
    public Provider Provider { get; set; } = null!;

    /// <summary>
    /// Navigation property for the offered slot (US_041).
    /// </summary>
    public TimeSlot? NotifiedSlot { get; set; }
}
