using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for retrieving 360-Degree Patient View data (FR-032, AIR-007).
/// Implements sub-2-second retrieval (NFR-002) with Redis caching.
/// </summary>
public interface IPatientProfileService
{
    /// <summary>
    /// Retrieves comprehensive 360° patient view with all health sections.
    /// Includes demographics, conditions, medications, allergies, vital trends, and encounters.
    /// </summary>
    /// <param name="patientId">Patient's GUID identifier</param>
    /// <param name="vitalRangeStart">Start date for vital trends (default: 12 months ago)</param>
    /// <param name="vitalRangeEnd">End date for vital trends (default: today)</param>
    /// <returns>Complete 360° patient profile DTO with verification badges (UXR-402)</returns>
    /// <exception cref="KeyNotFoundException">Thrown when patient profile doesn't exist</exception>
    Task<PatientProfile360Dto> Get360ProfileAsync(
        Guid patientId,
        DateTime? vitalRangeStart = null,
        DateTime? vitalRangeEnd = null);

    /// <summary>
    /// Invalidates cached 360° profile for specified patient.
    /// Called after data aggregation or conflict resolution updates.
    /// </summary>
    /// <param name="patientId">Patient's GUID identifier</param>
    Task InvalidateCacheAsync(Guid patientId);
}
