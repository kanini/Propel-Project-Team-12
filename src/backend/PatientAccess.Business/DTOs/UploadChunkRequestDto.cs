namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for uploading a single chunk (US_042, AC2).
/// Supports resumable uploads with chunk sequencing.
/// Note: ChunkData is handled as Stream in service layer.
/// </summary>
public class UploadChunkRequestDto
{
    /// <summary>
    /// Upload session identifier
    /// </summary>
    public Guid UploadSessionId { get; set; }

    /// <summary>
    /// Zero-based chunk index
    /// </summary>
    public int ChunkIndex { get; set; }
}
