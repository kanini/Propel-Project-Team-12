using PatientAccess.Data.Models;

namespace PatientAccess.Business.DTOs;

/// <summary>
/// Represents a single extracted data point from clinical document.
/// </summary>
public class ExtractedDataPointDto
{
    public ClinicalDataType DataType { get; set; }
    public string DataKey { get; set; } = string.Empty;
    public string DataValue { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public int? SourcePageNumber { get; set; }
    public string? SourceTextExcerpt { get; set; }
    public Dictionary<string, object>? StructuredData { get; set; }
}
