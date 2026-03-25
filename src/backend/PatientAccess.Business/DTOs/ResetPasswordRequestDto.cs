using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for resetting password with token.
/// </summary>
public class ResetPasswordRequestDto
{
    /// <summary>
    /// Password reset token from email link.
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// New password (8-100 chars, requires uppercase, lowercase, digit, special char).
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirm new password.
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
