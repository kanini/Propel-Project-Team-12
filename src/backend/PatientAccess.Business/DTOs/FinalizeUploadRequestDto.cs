namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for finalizing a chunked upload (US_042, AC3).
/// Triggers chunk assembly and database record creation.
/// </summary>
public class FinalizeUploadRequestDto
{
    /// <summary>
    /// Upload session identifier
    /// </summary>
    public Guid UploadSessionId { get; set; }
}
