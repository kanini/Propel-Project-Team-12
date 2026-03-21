using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for user login endpoint (FR-002).
/// Contains authentication credentials for validation.
/// </summary>
public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}
