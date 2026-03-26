namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for finalized document upload (US_042, AC3).
/// Contains document metadata and confirmation details.
/// </summary>
public class DocumentUploadResponseDto
{
    /// <summary>
    /// Created document identifier
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Processing status: "Uploaded — Processing pending"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Upload timestamp (UTC)
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Patient ID for whom the document was uploaded
    /// </summary>
    public Guid PatientId { get; set; }
}
