using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for medical code mapping (ICD-10 or CPT) from extracted clinical text.
/// US_051 Task 1 - Code Mapping Service.
/// </summary>
public class CodeMappingRequestDto
{
    /// <summary>
    /// Foreign key to ExtractedClinicalData entity.
    /// References the clinical data extraction record that triggered code mapping.
    /// </summary>
    [Required]
    public Guid ExtractedClinicalDataId { get; set; }

    /// <summary>
    /// Extracted clinical text to map to medical codes.
    /// Example: "Patient presents with Type 2 Diabetes Mellitus, uncontrolled."
    /// </summary>
    [Required]
    [StringLength(5000, MinimumLength = 5, ErrorMessage = "Clinical text must be between 5 and 5000 characters")]
    public string ClinicalText { get; set; } = string.Empty;

    /// <summary>
    /// Code system identifier: "ICD10" or "CPT".
    /// Determines which knowledge base index and prompt template to use.
    /// </summary>
    [Required]
    [RegularExpression("^(ICD10|CPT)$", ErrorMessage = "CodeSystem must be 'ICD10' or 'CPT'")]
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of code suggestions to return (default: 5 per AIR-R02).
    /// Used for limiting results in ambiguous cases.
    /// </summary>
    [Range(1, 10, ErrorMessage = "MaxSuggestions must be between 1 and 10")]
    public int MaxSuggestions { get; set; } = 5;
}
