namespace PatientAccess.Data.Models;

/// <summary>
/// AI-extracted data point from clinical documents with confidence and verification tracking (DR-004, US_058).
/// </summary>
public class ExtractedClinicalData
{
    public Guid ExtractedDataId { get; set; }

    public Guid DocumentId { get; set; }

    public Guid PatientId { get; set; }

    public ClinicalDataType DataType { get; set; }

    public string DataKey { get; set; } = string.Empty;

    public string DataValue { get; set; } = string.Empty;

    /// <summary>
    /// AI-suggested flag for human-in-the-loop (AC1 - US_058, AIR-S01).
    /// True = AI-generated, requires staff verification before commit.
    /// </summary>
    public bool IsAISuggested { get; set; } = true;

    /// <summary>
    /// AI extraction confidence score (0.0 - 1.0) (AC2 - US_058, AIR-S02).
    /// Below threshold (e.g., 0.7) triggers mandatory manual review.
    /// </summary>
    public decimal ConfidenceScore { get; set; }

    /// <summary>
    /// Auto-flagged for mandatory manual review (AC2 - US_058, AIR-S02).
    /// Set to true when ConfidenceScore &lt; threshold.
    /// </summary>
    public bool RequiresManualReview { get; set; } = false;

    /// <summary>
    /// Verification status lifecycle (AC1 - US_058, AIR-S01).
    /// Default: Pending (requires staff verification).
    /// </summary>
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

    /// <summary>
    /// AI model version used for extraction (AC5 - US_058, NFR-015).
    /// Format: "gpt-4o-2024-05-13" or "prebuilt-layout-2024-02-29".
    /// Captured per-extraction for traceability.
    /// </summary>
    public string? ModelVersion { get; set; }

    /// <summary>
    /// Prompt tokens consumed (AC4 - US_058, AIR-O05).
    /// Used for cost tracking and monitoring.
    /// </summary>
    public int PromptTokens { get; set; }

    /// <summary>
    /// Completion tokens consumed (AC4 - US_058, AIR-O05).
    /// Used for cost tracking and monitoring.
    /// </summary>
    public int CompletionTokens { get; set; }

    /// <summary>
    /// Total tokens consumed (AC4 - US_058, AIR-O05).
    /// Total = PromptTokens + CompletionTokens.
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// Estimated cost in USD for this AI request (AC4 - US_058, AIR-O05).
    /// Calculated based on token usage and model pricing.
    /// </summary>
    public decimal EstimatedCost { get; set; }

    public int? SourcePageNumber { get; set; }

    public string? SourceTextExcerpt { get; set; }

    /// <summary>
    /// Staff member who verified the AI extraction (AC1 - US_058, AIR-S01).
    /// Null = not yet verified.
    /// </summary>
    public Guid? VerifiedBy { get; set; }

    /// <summary>
    /// Timestamp when staff verified the extraction (AC1 - US_058, AIR-S01).
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    public DateTime ExtractedAt { get; set; }

    public string? StructuredData { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ClinicalDocument Document { get; set; } = null!;
    public User Patient { get; set; } = null!;
    public User? Verifier { get; set; }
    public ICollection<MedicalCode> MedicalCodes { get; set; } = new List<MedicalCode>();
}
