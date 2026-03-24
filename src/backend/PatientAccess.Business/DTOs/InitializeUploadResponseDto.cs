namespace PatientAccess.Business.DTOs;

/// <summary>
/// Response DTO for initialized upload session (US_042, AC1).
/// Provides session details for subsequent chunk uploads.
/// </summary>
public class InitializeUploadResponseDto
{
    /// <summary>
    /// Upload session identifier (GUID)
    /// </summary>
    public Guid UploadSessionId { get; set; }

    /// <summary>
    /// Recommended chunk size in bytes (e.g., 1MB = 1048576)
    /// </summary>
    public int ChunkSize { get; set; }

    /// <summary>
    /// Session expiration timestamp (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Pusher channel name for real-time progress updates
    /// </summary>
    public string PusherChannel { get; set; } = string.Empty;
}
