using PatientAccess.Data.Models;

namespace PatientAccess.Business.DTOs;

public class MedicalCodeDto
{
    public Guid? MedicalCodeId { get; set; }
    public CodeSystem CodeSystem { get; set; }
    public string CodeValue { get; set; } = string.Empty;
    public string CodeDescription { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public string VerificationStatus { get; set; } = "AISuggested";
    public string? SourceDataSummary { get; set; }
    public Guid? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
}
