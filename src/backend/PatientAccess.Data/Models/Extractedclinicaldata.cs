using Pgvector;

namespace PatientAccess.Data.Models;

/// <summary>
/// AI-extracted data point from clinical documents with confidence and verification tracking (DR-004).
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
    /// Confidence score from 0.00 to 100.00.
    /// </summary>
    public decimal ConfidenceScore { get; set; }

    public VerificationStatus VerificationStatus { get; set; }

    public int? SourcePageNumber { get; set; }

    public string? SourceTextExcerpt { get; set; }

    public Guid? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Optional 1536-dimensional vector embedding for semantic similarity search (DR-010).
    /// </summary>
    public Vector? Embedding { get; set; }

    // Navigation properties
    public ClinicalDocument Document { get; set; } = null!;
    public User Patient { get; set; } = null!;
    public User? Verifier { get; set; }
    public ICollection<MedicalCode> MedicalCodes { get; set; } = new List<MedicalCode>();
}
