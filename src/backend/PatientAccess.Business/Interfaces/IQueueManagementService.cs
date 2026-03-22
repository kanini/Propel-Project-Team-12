/**
 * IQueueManagementService Interface for Same-Day Queue Operations
 * Provides business logic for queue retrieval and priority management
 */

using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for managing same-day patient queue
/// </summary>
public interface IQueueManagementService
{
    /// <summary>
    /// Get all patients in the same-day queue with "Arrived" status
    /// </summary>
    /// <param name="providerId">Optional provider filter (null for all providers)</param>
    /// <returns>Task<List<QueuePatientDto>> - List of queue patients ordered by priority and arrival time</returns>
    Task<List<QueuePatientDto>> GetSameDayQueueAsync(Guid? providerId = null);

    /// <summary>
    /// Update patient priority flag in the queue
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <param name="isPriority">Priority flag (true for emergency)</param>
    /// <returns>Task<QueuePatientDto> - Updated queue patient data</returns>
    Task<QueuePatientDto> UpdatePatientPriorityAsync(Guid appointmentId, bool isPriority);
}
