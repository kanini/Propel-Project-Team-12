namespace PatientAccess.Business.DTOs;

/// <summary>
/// DTO for displaying recent clinical documents on dashboard (US_067).
/// Contains document metadata and processing status.
/// </summary>
public class RecentDocumentDto
{
    /// <summary>
    /// Unique document identifier.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Original uploaded file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File type/extension (e.g., "pdf", "jpg").
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Processing status: Processing, Completed, Failed.
    /// </summary>
    public string ProcessingStatus { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when document was uploaded (UTC).
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Timestamp when processing completed (UTC), if applicable.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Error message if processing failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
