using PatientAccess.Data.Models;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for waitlist entry with queue position (FR-009, AC-2, AC-4).
/// </summary>
public class WaitlistEntryDto
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public DateOnly PreferredStartDate { get; set; }
    public DateOnly PreferredEndDate { get; set; }
    public NotificationPreference NotificationPreference { get; set; }
    public WaitlistStatus Status { get; set; }
    public int QueuePosition { get; set; }
    public DateTime CreatedAt { get; set; }
}
