namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for code mapping service.
/// Contains list of code suggestions ranked by confidence.
/// </summary>
public class CodeMappingResponseDto
{
    /// <summary>
    /// ID of the extracted clinical data that was mapped.
    /// </summary>
    public Guid ExtractedClinicalDataId { get; set; }

    /// <summary>
    /// Code system used for mapping: "ICD10" or "CPT".
    /// </summary>
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// List of code suggestions ranked by confidence (descending).
    /// </summary>
    public List<MedicalCodeSuggestionDto> Suggestions { get; set; } = new();

    /// <summary>
    /// Number of suggestions returned.
    /// </summary>
    public int SuggestionCount { get; set; }

    /// <summary>
    /// Optional message for edge cases.
    /// Example: "No matching code found" when clinical text is unmappable.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// True if multiple codes have similar confidence scores (ambiguous mapping).
    /// Ambiguous = top 2 suggestions have confidence difference < 10%.
    /// </summary>
    public bool IsAmbiguous { get; set; }
}
