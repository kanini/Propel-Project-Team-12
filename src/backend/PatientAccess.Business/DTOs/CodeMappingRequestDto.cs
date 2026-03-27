using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for code mapping service.
/// Used to map clinical text to ICD-10 or CPT codes using RAG pattern.
/// </summary>
public class CodeMappingRequestDto
{
    /// <summary>
    /// ID of the extracted clinical data to map.
    /// </summary>
    [Required]
    public Guid ExtractedClinicalDataId { get; set; }

    /// <summary>
    /// Clinical text to map to medical codes.
    /// Example: "Type 2 Diabetes Mellitus without complications"
    /// </summary>
    [Required]
    [StringLength(5000, MinimumLength = 5, ErrorMessage = "Clinical text must be between 5 and 5000 characters")]
    public string ClinicalText { get; set; } = string.Empty;

    /// <summary>
    /// Code system to map to: "ICD10" or "CPT".
    /// </summary>
    [Required]
    [RegularExpression("^(ICD10|CPT)$", ErrorMessage = "CodeSystem must be 'ICD10' or 'CPT'")]
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of code suggestions to return.
    /// Default: 5 (per AIR-R02)
    /// </summary>
    [Range(1, 10, ErrorMessage = "MaxSuggestions must be between 1 and 10")]
    public int MaxSuggestions { get; set; } = 5;
}
