namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO containing medical code suggestions with confidence scores and ambiguity indicators.
/// US_051 Task 1 - Code Mapping Service.
/// Implements AIR-003 (ICD-10 mapping via RAG) and AIR-004 (CPT mapping via RAG).
/// </summary>
public class CodeMappingResponseDto
{
    /// <summary>
    /// Foreign key to ExtractedClinicalData entity (same as request).
    /// </summary>
    public Guid ExtractedClinicalDataId { get; set; }

    /// <summary>
    /// Code system identifier: "ICD10" or "CPT".
    /// </summary>
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// List of medical code suggestions ranked by confidence (descending).
    /// Empty list if no matching code found.
    /// </summary>
    public List<MedicalCodeSuggestionDto> Suggestions { get; set; } = new();

    /// <summary>
    /// Number of code suggestions returned.
    /// Zero if no matching code found.
    /// </summary>
    public int SuggestionCount { get; set; }

    /// <summary>
    /// Optional message for edge cases (e.g., "No matching code found - recommend manual coding").
    /// Set when clinical text is unmappable to any code.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Indicates if multiple codes have similar confidence scores (difference <10%).
    /// Used to flag ambiguous cases requiring staff review.
    /// True if SuggestionCount > 1 and |ConfidenceScore[0] - ConfidenceScore[1]| < AmbiguityThreshold (default: 10%).
    /// </summary>
    public bool IsAmbiguous { get; set; }
}
