namespace PatientAccess.Business.DTOs;

public class VerificationQueueItemDto
{
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int PendingClinicalDataCount { get; set; }
    public int PendingMedicalCodesCount { get; set; }
    public int ConflictCount { get; set; }
    public string Priority { get; set; } = "Low";
    public DateTime? LastUploadDate { get; set; }
}

public class VerificationQueueResponseDto
{
    public List<VerificationQueueItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class ClinicalVerificationDashboardDto
{
    public int PendingCount { get; set; }
    public int VerifiedCount { get; set; }
    public int RejectedCount { get; set; }
    public int ConflictCount { get; set; }
    public List<VerificationItemDto> ClinicalData { get; set; } = new();
    public List<VerificationMedicalCodeDto> MedicalCodes { get; set; } = new();
}

public class VerificationItemDto
{
    public Guid ExtractedDataId { get; set; }
    public string DataType { get; set; } = string.Empty;
    public string DataValue { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public string? SourceDocument { get; set; }
    public int? SourcePageNumber { get; set; }
    public string? SourceTextExcerpt { get; set; }
}

public class VerificationMedicalCodeDto
{
    public Guid MedicalCodeId { get; set; }
    public string CodeSystem { get; set; } = string.Empty;
    public string CodeValue { get; set; } = string.Empty;
    public string CodeDescription { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public string? SourceDataSummary { get; set; }
    public string? VerifiedByName { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? RejectionReason { get; set; }
}

public class VerifyActionDto
{
    public Guid Id { get; set; }
}

public class RejectActionDto
{
    public Guid Id { get; set; }
    public string? Reason { get; set; }
}

public class ModifyCodeDto
{
    public Guid MedicalCodeId { get; set; }
    public string CodeValue { get; set; } = string.Empty;
    public string CodeDescription { get; set; } = string.Empty;
}
