using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for medical code search via hybrid retrieval (AIR-R02, AIR-R03).
/// </summary>
public class CodeSearchRequestDto
{
    /// <summary>
    /// Search query (e.g., "Type 2 Diabetes Mellitus")
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 2, ErrorMessage = "Query must be between 2 and 500 characters")]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Code system to search: "ICD10", "CPT", or "ClinicalTerminology"
    /// </summary>
    [Required]
    [RegularExpression("^(ICD10|CPT|ClinicalTerminology)$", ErrorMessage = "CodeSystem must be ICD10, CPT, or ClinicalTerminology")]
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// Number of top results to return (default: 5 per AIR-R02)
    /// </summary>
    [Range(1, 50, ErrorMessage = "TopK must be between 1 and 50")]
    public int TopK { get; set; } = 5;

    /// <summary>
    /// Minimum cosine similarity threshold (default: 0.75 per AIR-R02)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "MinSimilarityThreshold must be between 0 and 1")]
    public double MinSimilarityThreshold { get; set; } = 0.75;
}
