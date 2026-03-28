namespace PatientAccess.Business.DTOs;

public class HealthDashboard360Dto
{
    public PatientDemographicsDto Demographics { get; set; } = new();
    public List<ClinicalItemDto> Conditions { get; set; } = new();
    public List<ClinicalItemDto> Medications { get; set; } = new();
    public List<ClinicalItemDto> Allergies { get; set; } = new();
    public List<ClinicalItemDto> Vitals { get; set; } = new();
    public List<ClinicalItemDto> LabResults { get; set; } = new();
    public List<EncounterDto> Encounters { get; set; } = new();
    public List<MedicalCodeDto> MedicalCodes { get; set; } = new();
    public DashboardStatsOverviewDto Stats { get; set; } = new();
}

public class PatientDemographicsDto
{
    public string Name { get; set; } = string.Empty;
    public string? DateOfBirth { get; set; }
    public string? Mrn { get; set; }
    public string? BloodType { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Insurance { get; set; }
}

public class ClinicalItemDto
{
    public Guid ExtractedDataId { get; set; }
    public string DataType { get; set; } = string.Empty;
    public string DataKey { get; set; } = string.Empty;
    public string DataValue { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public string VerificationStatus { get; set; } = "AISuggested";
    public string? Source { get; set; }
    public string? Onset { get; set; }
    public int? SourcePageNumber { get; set; }
    public string? SourceTextExcerpt { get; set; }
    public Dictionary<string, object>? StructuredData { get; set; }
    public List<MedicalCodeDto> MedicalCodes { get; set; } = new();
}

public class EncounterDto
{
    public string Date { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class DashboardStatsOverviewDto
{
    public int TotalExtractedItems { get; set; }
    public int VerifiedItems { get; set; }
    public int PendingItems { get; set; }
    public int TotalDocuments { get; set; }
    public int TotalMedicalCodes { get; set; }
}
