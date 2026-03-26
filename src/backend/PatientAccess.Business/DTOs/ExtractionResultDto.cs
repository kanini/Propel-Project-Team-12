namespace PatientAccess.Business.DTOs;

/// <summary>
/// Represents the complete extraction result for a clinical document.
/// </summary>
public class ExtractionResultDto
{
    public Guid DocumentId { get; set; }
    public List<ExtractedDataPointDto> DataPoints { get; set; } = new();
    public int TotalDataPoints { get; set; }
    public int FlaggedForReviewCount { get; set; }
    public Dictionary<string, int> DataTypeBreakdown { get; set; } = new();
    public bool RequiresManualReview { get; set; }
    public DateTime ExtractedAt { get; set; }
    public long ProcessingTimeMs { get; set; }
}
