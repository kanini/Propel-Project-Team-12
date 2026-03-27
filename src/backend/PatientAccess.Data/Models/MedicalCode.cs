namespace PatientAccess.Data.Models;

/// <summary>
/// ICD-10 or CPT code suggestion from AI extraction (DR-013).
/// </summary>
public class MedicalCode
{
    public Guid MedicalCodeId { get; set; }

    public Guid ExtractedDataId { get; set; }

    public CodeSystem CodeSystem { get; set; }

    public string CodeValue { get; set; } = string.Empty;

    public string CodeDescription { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score from 0.00 to 100.00.
    /// </summary>
    public decimal ConfidenceScore { get; set; }

    /// <summary>
    /// LLM explanation for code selection (rationale for mapping).
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

    /// <summary>
    /// RAG context chunks used for code mapping (for auditing).
    /// </summary>
    public string? RetrievedContext { get; set; }

    public MedicalCodeVerificationStatus VerificationStatus { get; set; }

    public Guid? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ExtractedClinicalData ExtractedData { get; set; } = null!;
    public User? Verifier { get; set; }
}
