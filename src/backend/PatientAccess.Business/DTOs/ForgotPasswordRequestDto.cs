using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for initiating password reset workflow.
/// </summary>
public class ForgotPasswordRequestDto
{
    /// <summary>
    /// Email address of the account to reset password.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}
