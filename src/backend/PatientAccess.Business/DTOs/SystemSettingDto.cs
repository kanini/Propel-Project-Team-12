namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for system setting key-value pair (US_037 - Admin settings API).
/// </summary>
public class SystemSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
