namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for chunk upload status (US_042, AC2).
/// Provides real-time progress information.
/// </summary>
public class ChunkUploadResponseDto
{
    /// <summary>
    /// Number of chunks successfully received
    /// </summary>
    public int ChunksReceived { get; set; }

    /// <summary>
    /// Total chunks expected
    /// </summary>
    public int TotalChunks { get; set; }

    /// <summary>
    /// Upload progress percentage (0-100)
    /// </summary>
    public int PercentComplete { get; set; }

    /// <summary>
    /// Upload status: "Uploading", "Complete", "Failed"
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
