using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for modifying an AI-suggested medical code (EP-008-US-052, AC3).
/// Used when staff member wants to change the suggested code to an alternative.
/// </summary>
public class ModifyCodeDto
{
    /// <summary>
    /// New code value to replace the AI suggestion.
    /// Examples: "E11.65" (ICD-10), "99213" (CPT)
    /// </summary>
    [Required(ErrorMessage = "New code is required")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "Code must be between 1 and 20 characters")]
    public string NewCode { get; set; } = string.Empty;

    /// <summary>
    /// Description of the new code.
    /// Example: "Type 2 diabetes mellitus with hyperglycemia"
    /// </summary>
    [Required(ErrorMessage = "Code description is required")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 1000 characters")]
    public string NewDescription { get; set; } = string.Empty;

    /// <summary>
    /// Staff member's rationale for modifying the code.
    /// Required for audit trail and quality improvement.
    /// Example: "AI suggested nonspecific code E11.9; patient has documented hyperglycemia, so E11.65 is more accurate"
    /// </summary>
    [Required(ErrorMessage = "Modification rationale is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Rationale must be between 10 and 2000 characters")]
    public string Rationale { get; set; } = string.Empty;
}
