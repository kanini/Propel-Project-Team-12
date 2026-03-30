using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// No-show risk scoring service interface (US_038 - FR-023).
/// Calculates rule-based risk scores using configurable weighted factors.
/// </summary>
public interface INoShowRiskService
{
    /// <summary>
    /// Calculates no-show risk score for an appointment.
    /// </summary>
    /// <param name="patientId">Patient unique identifier.</param>
    /// <param name="scheduledDateTime">Scheduled appointment date and time in UTC.</param>
    /// <param name="isWalkIn">True if walk-in appointment (returns fixed score of 0).</param>
    /// <returns>Risk score with factor breakdown. Meets 100ms performance target (NFR-016).</returns>
    Task<NoShowRiskScoreDto> CalculateRiskScoreAsync(
        Guid patientId,
        DateTime scheduledDateTime,
        bool isWalkIn);
}
