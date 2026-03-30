namespace PatientAccess.Data.Models;

/// <summary>
/// Key-value entity for system-wide configuration settings (US_037).
/// Used for admin-configurable reminder intervals and notification preferences.
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
