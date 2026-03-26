namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for system setting key-value pair (US_037 - AC-4).
/// Used for admin configuration of reminder intervals and notification toggles.
/// </summary>
public class SystemSettingDto
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string? Description { get; set; }
}
