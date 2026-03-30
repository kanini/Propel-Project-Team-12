using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

public interface IGeminiAiService
{
    Task<GeminiExtractionResponseDto> ExtractClinicalDataWithCodesAsync(string contextChunks, string systemPrompt);
}

public class GeminiExtractionResponseDto
{
    public List<ExtractedDataPointDto> DataPoints { get; set; } = new();
    public List<GeminiMedicalCodeDto> MedicalCodes { get; set; } = new();
}

public class GeminiMedicalCodeDto
{
    public string CodeSystem { get; set; } = string.Empty; // "ICD10" or "CPT"
    public string CodeValue { get; set; } = string.Empty;
    public string CodeDescription { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public string SourceDataKey { get; set; } = string.Empty; // Links to ExtractedDataPointDto.DataKey
}
