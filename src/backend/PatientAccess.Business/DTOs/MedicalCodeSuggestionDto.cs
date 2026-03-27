namespace PatientAccess.Business.DTOs;

/// <summary>
/// Individual medical code suggestion from AI mapping.
/// Contains code, description, confidence score, and LLM rationale.
/// </summary>
public class MedicalCodeSuggestionDto
{
    /// <summary>
    /// Medical code value (e.g., "E11.9", "99213").
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Full description of the medical code.
    /// Example: "Type 2 diabetes mellitus without complications"
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score from 0 to 100.
    /// Represents LLM certainty in the code mapping.
    /// </summary>
    public decimal ConfidenceScore { get; set; }

    /// <summary>
    /// LLM explanation for selecting this code.
    /// Example: "Clinical text explicitly mentions 'Type 2 Diabetes Mellitus' without mention of complications."
    /// </summary>
    public string Rationale { get; set; } = string.Empty;

    /// <summary>
    /// Rank of this suggestion (1 = top suggestion).
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// True if this is the top-ranked suggestion (Rank == 1).
    /// </summary>
    public bool IsTopSuggestion { get; set; }
}
