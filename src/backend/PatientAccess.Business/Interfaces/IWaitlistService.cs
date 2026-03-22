using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for waitlist enrollment and management (FR-009).
/// </summary>
public interface IWaitlistService
{
    /// <summary>
    /// Enrolls patient in waitlist for provider (FR-009, AC-2).
    /// Implements FIFO ordering with priority timestamp.
    /// </summary>
    /// <exception cref="ConflictException">Thrown when patient already on waitlist for provider</exception>
    Task<WaitlistEntryDto> JoinWaitlistAsync(Guid patientId, JoinWaitlistRequestDto request);

    /// <summary>
    /// Retrieves patient's active waitlist entries with calculated queue positions (FR-009, AC-4).
    /// Uses ROW_NUMBER() window function for efficient position calculation.
    /// </summary>
    Task<List<WaitlistEntryDto>> GetPatientWaitlistAsync(Guid patientId);

    /// <summary>
    /// Updates waitlist preferences while maintaining queue position (FR-009).
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when entry not found or unauthorized</exception>
    Task<WaitlistEntryDto> UpdateWaitlistAsync(Guid entryId, Guid patientId, UpdateWaitlistRequestDto request);

    /// <summary>
    /// Removes patient from waitlist (FR-009).
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when entry not found or unauthorized</exception>
    Task DeleteWaitlistAsync(Guid entryId, Guid patientId);
}
