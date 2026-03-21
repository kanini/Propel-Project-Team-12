using System.ComponentModel.DataAnnotations;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for updating user details (US_021, AC2).
/// Email is immutable after creation - only name and role can be modified.
/// </summary>
public class UpdateUserRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid role. Must be Patient, Staff, or Admin.")]
    public UserRole Role { get; set; }
}
