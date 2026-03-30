namespace PatientAccess.Data.Models;

/// <summary>
/// Uploaded patient clinical document with processing status and AI metadata (DR-003, US_058).
/// </summary>
public class ClinicalDocument
{
    public Guid DocumentId { get; set; }

    public Guid PatientId { get; set; }

    public Guid? UploadedBy { get; set; }

    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string FileType { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public ProcessingStatus ProcessingStatus { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Document-level manual review flag (AC2 - US_058, AIR-S02).
    /// Set if any extraction has low confidence or processing errors occurred.
    /// </summary>
    public bool RequiresManualReview { get; set; }

    /// <summary>
    /// AI model version used for document processing (AC5 - US_058, NFR-015).
    /// Aggregated from extraction operations for traceability.
    /// </summary>
    public string? ModelVersion { get; set; }

    /// <summary>
    /// Total prompt tokens consumed for this document (AC4 - US_058, AIR-O05).
    /// Aggregated from all extraction operations.
    /// </summary>
    public int TotalPromptTokens { get; set; }

    /// <summary>
    /// Total completion tokens consumed for this document (AC4 - US_058, AIR-O05).
    /// Aggregated from all extraction operations.
    /// </summary>
    public int TotalCompletionTokens { get; set; }

    /// <summary>
    /// Total tokens consumed for this document (AC4 - US_058, AIR-O05).
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// Total estimated cost in USD for this document (AC4 - US_058, AIR-O05).
    /// Aggregated from all extraction operations.
    /// </summary>
    public decimal TotalEstimatedCost { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User Patient { get; set; } = null!;
    public User? UploadedByUser { get; set; }
    public ICollection<ExtractedClinicalData> ExtractedData { get; set; } = new List<ExtractedClinicalData>();
}
