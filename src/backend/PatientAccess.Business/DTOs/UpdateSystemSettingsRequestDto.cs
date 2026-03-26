using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for bulk update of system settings (US_037 - AC-4).
/// Admin can modify reminder intervals and notification channel toggles.
/// </summary>
public class UpdateSystemSettingsRequestDto
{
    [Required]
    public List<SystemSettingDto> Settings { get; set; } = new();
}
