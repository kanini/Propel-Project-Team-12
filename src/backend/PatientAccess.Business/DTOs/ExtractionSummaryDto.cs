namespace PatientAccess.Business.DTOs;

/// <summary>
/// Summary DTO for Pusher event payload (task_003).
/// </summary>
public class ExtractionSummaryDto
{
    public Guid DocumentId { get; set; }
    public int TotalDataPoints { get; set; }
    public int FlaggedForReview { get; set; }
    public Dictionary<string, int> DataTypeBreakdown { get; set; } = new();
    public bool RequiresManualReview { get; set; }
    public DateTime ExtractedAt { get; set; }
}
