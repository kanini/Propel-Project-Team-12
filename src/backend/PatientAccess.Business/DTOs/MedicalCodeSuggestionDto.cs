namespace PatientAccess.Business.DTOs;

/// <summary>
/// Represents a single medical code suggestion (ICD-10 or CPT) with confidence score and rationale.
/// US_051 Task 1 - Code Mapping Service.
/// Implements FR-034 (ICD-10 suggestions with confidence) and FR-035 (CPT suggestions with confidence).
/// </summary>
public class MedicalCodeSuggestionDto
{
    /// <summary>
    /// Medical code value (e.g., "E11.9" for ICD-10, "99213" for CPT).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Full code description from knowledge base.
    /// Example: "Type 2 diabetes mellitus without complications"
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// LLM-generated confidence score (0-100%) indicating certainty of code mapping.
    /// Higher scores indicate stronger semantic match between clinical text and code.
    /// </summary>
    public decimal ConfidenceScore { get; set; }

    /// <summary>
    /// LLM-generated rationale explaining why this code was selected.
    /// Provides transparency for staff reviewers (e.g., "Clinical text explicitly mentions 'Type 2 Diabetes'").
    /// </summary>
    public string Rationale { get; set; } = string.Empty;

    /// <summary>
    /// Rank of this suggestion (1 = top suggestion, 2-5 = alternatives).
    /// Sorted by ConfidenceScore descending when multiple codes are suggested.
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// Convenience flag indicating if this is the top suggestion (Rank == 1).
    /// Used for highlighting primary recommendation in UI.
    /// </summary>
    public bool IsTopSuggestion { get; set; }
}
