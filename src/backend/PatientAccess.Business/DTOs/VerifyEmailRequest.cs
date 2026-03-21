using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for email verification (FR-001).
/// Contains verification token from email link.
/// </summary>
public class VerifyEmailRequest
{
    [Required(ErrorMessage = "Verification token is required")]
    public string Token { get; set; } = string.Empty;
}
