namespace PatientAccess.Business.DTOs;

/// <summary>
/// Request DTO for batch document submission (EP006-EP008).
/// Accepts multiple upload session IDs to finalize and trigger RAG pipeline for each.
/// </summary>
public class SubmitAllDocumentsRequestDto
{
    /// <summary>
    /// Upload session IDs to finalize and process.
    /// </summary>
    public List<Guid> UploadSessionIds { get; set; } = new();
}

/// <summary>
/// Response DTO for batch document submission.
/// Reports success/failure for each individual document.
/// </summary>
public class SubmitAllDocumentsResponseDto
{
    public List<SubmitDocumentResultDto> Results { get; set; } = new();
    public int TotalSubmitted { get; set; }
    public int TotalFailed { get; set; }
}

/// <summary>
/// Result for a single document in the batch submission.
/// </summary>
public class SubmitDocumentResultDto
{
    public string SessionId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public Guid? DocumentId { get; set; }
    public string? FileName { get; set; }
    public string? Error { get; set; }
}
