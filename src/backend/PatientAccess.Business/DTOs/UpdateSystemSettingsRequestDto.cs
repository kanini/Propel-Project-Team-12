using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for bulk updating system settings (US_037 - Admin settings API).
/// Used in PUT /api/admin/settings endpoint.
/// </summary>
public class UpdateSystemSettingsRequestDto
{
    [Required]
    public List<SystemSettingDto> Settings { get; set; } = new();
}
