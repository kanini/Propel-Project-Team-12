using System.ComponentModel.DataAnnotations;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for updating user details (US_021, AC2).
/// </summary>
public class UpdateUserRequestDto
{
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string? Name { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    public UserRole? Role { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? Phone { get; set; }

    public UserStatus? Status { get; set; }
}
