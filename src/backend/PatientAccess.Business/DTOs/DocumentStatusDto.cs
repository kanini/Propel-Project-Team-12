namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for document status retrieval (US_044, AC1).
/// Contains document metadata, processing status, and timing information.
/// </summary>
public class DocumentStatusDto
{
    /// <summary>
    /// Document identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Upload timestamp (UTC)
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Human-readable file size (e.g., "2.4 MB")
    /// </summary>
    public string FileSizeFormatted => FormatFileSize(FileSize);

    /// <summary>
    /// Processing status: Uploaded, Processing, Completed, Failed
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Processing time in milliseconds (nullable if not yet processed)
    /// </summary>
    public long? ProcessingTimeMs { get; set; }

    /// <summary>
    /// Error message if processing failed (nullable)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Indicates if document has been stuck in Processing status for more than 5 minutes
    /// </summary>
    public bool IsStuckProcessing { get; set; }

    /// <summary>
    /// Timestamp when processing was completed or failed (nullable if still processing)
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Formats file size to human-readable string.
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:F1} {units[unitIndex]}";
    }
}
