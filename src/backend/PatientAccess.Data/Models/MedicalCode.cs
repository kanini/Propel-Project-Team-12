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
    /// LLM-generated rationale explaining why this code was selected.
    /// Provides transparency for staff reviewers and quality audits (US_051 Task 1).
    /// </summary>
    public string? Rationale { get; set; }

    /// <summary>
    /// Rank of this suggestion (1 = top suggestion, 2-5 = alternatives).
    /// Sorted by ConfidenceScore descending when multiple codes are suggested (US_051 Task 1).
    /// </summary>
    public int Rank { get; set; } = 1;

    /// <summary>
    /// Convenience flag indicating if this is the top suggestion (Rank == 1).
    /// Used for filtering primary recommendations in UI and metrics (US_051 Task 1).
    /// </summary>
    public bool IsTopSuggestion { get; set; } = true;

    /// <summary>
    /// RAG-retrieved context chunks from knowledge base used for code mapping.
    /// Stored for audit trail and reproducibility (US_051 Task 1).
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
