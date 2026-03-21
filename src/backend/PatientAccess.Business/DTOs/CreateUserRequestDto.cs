using System.ComponentModel.DataAnnotations;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for creating a new user (US_021, AC1).
/// Admin endpoint for creating Staff/Admin accounts.
/// </summary>
public class CreateUserRequestDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? Phone { get; set; }
}
