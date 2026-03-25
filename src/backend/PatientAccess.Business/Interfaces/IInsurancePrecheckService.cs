using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Insurance precheck service interface (US_036)
/// </summary>
public interface IInsurancePrecheckService
{
    /// <summary>
    /// Verifies insurance eligibility for an appointment
    /// </summary>
    Task<InsurancePrecheckResponseDto> VerifyInsuranceAsync(InsurancePrecheckRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Gets cached precheck result for an appointment
    /// </summary>
    Task<InsurancePrecheckResponseDto?> GetPrecheckResultAsync(int appointmentId, CancellationToken ct = default);
}
