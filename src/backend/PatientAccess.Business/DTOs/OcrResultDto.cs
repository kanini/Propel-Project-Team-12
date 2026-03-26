namespace PatientAccess.Business.DTOs;

/// <summary>
/// Represents OCR extraction result from a single PDF page.
/// </summary>
public class OcrResultDto
{
    public int PageNumber { get; set; }
    public string ExtractedText { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public string Language { get; set; } = "eng";
}
