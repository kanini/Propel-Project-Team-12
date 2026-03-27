using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for staff verification workflow (FR-036).
/// Used to verify or reject AI-suggested medical codes.
/// </summary>
public class VerifyCodeRequestDto
{
    /// <summary>
    /// Verification status: "StaffVerified" (accept) or "StaffRejected" (reject).
    /// </summary>
    [Required]
    [RegularExpression("^(StaffVerified|StaffRejected)$", ErrorMessage = "VerificationStatus must be 'StaffVerified' or 'StaffRejected'")]
    public string VerificationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Optional notes from staff about the verification decision.
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}
