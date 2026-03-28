using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

public interface IClinicalVerificationService
{
    Task<VerificationQueueResponseDto> GetVerificationQueueAsync(int limit = 10, string? searchTerm = null);
    Task<ClinicalVerificationDashboardDto> GetVerificationDashboardAsync(Guid patientId);
    Task VerifyDataPointAsync(Guid extractedDataId, Guid staffUserId);
    Task RejectDataPointAsync(Guid extractedDataId, Guid staffUserId);
    Task VerifyMedicalCodeAsync(Guid medicalCodeId, Guid staffUserId);
    Task RejectMedicalCodeAsync(Guid medicalCodeId, Guid staffUserId, string reason);
    Task ModifyMedicalCodeAsync(Guid medicalCodeId, string newCodeValue, string newDescription, Guid staffUserId);
}
