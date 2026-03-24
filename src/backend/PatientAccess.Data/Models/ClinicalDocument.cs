namespace PatientAccess.Data.Models;

/// <summary>
/// Uploaded patient clinical document with processing status (DR-003).
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

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User Patient { get; set; } = null!;
    public User? UploadedByUser { get; set; }
    public ICollection<ExtractedClinicalData> ExtractedData { get; set; } = new List<ExtractedClinicalData>();
}
