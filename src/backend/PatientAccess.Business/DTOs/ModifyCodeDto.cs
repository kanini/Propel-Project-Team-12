using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for staff to modify AI-suggested medical codes (US_052 Task 2, AC3).
/// Allows selection of alternative codes with modification rationale for audit trail.
/// </summary>
public class ModifyCodeDto
{
    /// <summary>
    /// The new medical code value (e.g., "E11.65" for ICD-10 or "99214" for CPT).
    /// Must match format: ICD-10 (Letter + 2 digits + . + 1-2 digits) or CPT (5 digits).
    /// </summary>
    [Required(ErrorMessage = "New code is required")]
    public string NewCode { get; set; } = string.Empty;

    /// <summary>
    /// The description of the new code (e.g., "Type 2 diabetes mellitus with hyperglycemia").
    /// Must be between 5 and 1000 characters.
    /// </summary>
    [Required(ErrorMessage = "Code description is required")]
    [StringLength(1000, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 1000 characters")]
    public string NewDescription { get; set; } = string.Empty;

    /// <summary>
    /// Staff rationale explaining why the code is being modified.
    /// Required for audit trail and quality review (AIR-Q01 compliance).
    /// Must be between 10 and 2000 characters.
    /// </summary>
    [Required(ErrorMessage = "Modification rationale is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Rationale must be between 10 and 2000 characters")]
    public string Rationale { get; set; } = string.Empty;
}
