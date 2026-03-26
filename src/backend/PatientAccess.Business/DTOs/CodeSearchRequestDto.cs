using System.ComponentModel.DataAnnotations;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for hybrid retrieval code search (AIR-R02, AIR-R03).
/// Supports semantic + keyword search across ICD-10, CPT, and clinical terminology.
/// </summary>
public class CodeSearchRequestDto
{
    /// <summary>
    /// Search query (e.g., "Type 2 Diabetes Mellitus", "hypertension", "knee replacement").
    /// Minimum 2 characters, maximum 500 characters.
    /// </summary>
    [Required(ErrorMessage = "Query is required")]
    [StringLength(500, MinimumLength = 2, ErrorMessage = "Query must be 2-500 characters")]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Code system to search: "ICD10", "CPT", or "ClinicalTerminology".
    /// Per AIR-R04, separate indices are maintained for each code system.
    /// </summary>
    [Required(ErrorMessage = "CodeSystem is required")]
    [RegularExpression("^(ICD10|CPT|ClinicalTerminology)$", 
        ErrorMessage = "CodeSystem must be ICD10, CPT, or ClinicalTerminology")]
    public string CodeSystem { get; set; } = string.Empty;

    /// <summary>
    /// Number of results to return (default 5 per AIR-R02).
    /// Range: 1-20 results.
    /// </summary>
    [Range(1, 20, ErrorMessage = "TopK must be between 1 and 20")]
    public int TopK { get; set; } = 5;

    /// <summary>
    /// Minimum similarity threshold for semantic matches (default 0.75 per AIR-R02).
    /// Range: 0.0-1.0 (cosine similarity score).
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "MinSimilarityThreshold must be between 0.0 and 1.0")]
    public double MinSimilarityThreshold { get; set; } = 0.75;
}
