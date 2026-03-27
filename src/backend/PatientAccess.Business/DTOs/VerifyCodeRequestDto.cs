using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for staff verification of AI-suggested medical codes.
/// US_051 Task 2 - Code Mapping API.
/// Used to approve or reject code suggestions for quality metrics tracking (AIR-Q01: >98% agreement).
/// </summary>
public class VerifyCodeRequestDto
{
    /// <summary>
    /// Verification status: "Accepted", "Rejected", or "Modified".
    /// Maps to MedicalCodeVerificationStatus enum values.
    /// </summary>
    [Required]
    [RegularExpression("^(Accepted|Rejected|Modified)$", ErrorMessage = "VerificationStatus must be 'Accepted', 'Rejected', or 'Modified'")]
    public string VerificationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Optional staff notes explaining verification decision.
    /// Used for quality review and training data.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
