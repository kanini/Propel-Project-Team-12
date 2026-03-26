namespace PatientAccess.Data.Models;

/// <summary>
/// System-wide configuration key-value store (US_037).
/// Supports admin-configurable reminder intervals and notification toggles.
/// Keys use dot-notation convention (e.g., "Reminder.Intervals", "Reminder.SmsEnabled").
/// </summary>
public class SystemSetting
{
    public Guid SystemSettingId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
