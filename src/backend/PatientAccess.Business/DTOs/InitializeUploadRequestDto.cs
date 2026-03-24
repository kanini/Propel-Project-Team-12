namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for initializing a chunked upload session (US_042, AC1).
/// Validates file metadata before accepting upload.
/// </summary>
public class InitializeUploadRequestDto
{
    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Total file size in bytes (max 10MB = 10485760 bytes)
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME type (must be "application/pdf")
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Total number of chunks to be uploaded
    /// </summary>
    public int TotalChunks { get; set; }

    /// <summary>
    /// Patient ID for whom the document is being uploaded
    /// </summary>
    public Guid PatientId { get; set; }
}
