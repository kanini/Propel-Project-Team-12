using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// US_038 AC-1: Rule-based no-show risk scoring service (TR-020).
/// Calculates deterministic risk score (0-100) using configurable weighted factors.
/// </summary>
public interface INoShowRiskService
{
    /// <summary>
    /// Calculate no-show risk score for a patient's appointment.
    /// </summary>
    /// <param name="patientId">Patient unique identifier.</param>
    /// <param name="scheduledDateTime">Appointment scheduled date and time (UTC).</param>
    /// <param name="isWalkIn">True if walk-in appointment (EC-2: fixed score of 0).</param>
    /// <returns>Risk score DTO with score, risk level, and factor breakdown.</returns>
    Task<NoShowRiskScoreDto> CalculateRiskScoreAsync(Guid patientId, DateTime scheduledDateTime, bool isWalkIn);
}
