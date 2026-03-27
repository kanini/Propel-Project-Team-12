/**
 * IPusherService Interface for Real-time Event Broadcasting
 * Provides abstraction for Pusher Channels event triggering
 */

using PatientAccess.Business.DTOs;

namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for broadcasting real-time events via Pusher Channels
/// </summary>
public interface IPusherService
{
    /// <summary>
    /// Trigger a Pusher event on a specified channel
    /// </summary>
    /// <param name="channel">Channel name (e.g., "queue-updates")</param>
    /// <param name="eventName">Event name (e.g., "patient-added", "priority-changed")</param>
    /// <param name="data">Event data object to broadcast</param>
    /// <returns>Task<bool> - True if event triggered successfully, False otherwise</returns>
    Task<bool> TriggerEventAsync(string channel, string eventName, object data);

    /// <summary>
    /// Send real-time notification for completed patient profile aggregation (EP-007).
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="result">Aggregation result with statistics</param>
    /// <returns>Task<bool> - True if notification sent successfully</returns>
    Task<bool> SendAggregationCompleteAsync(Guid patientId, AggregationResultDto result);

    /// <summary>
    /// Send real-time notification for critical conflict detection (US_048).
    /// Only sends notifications for Critical severity conflicts.
    /// </summary>
    /// <param name="patientId">Patient ID</param>
    /// <param name="conflict">Conflict details</param>
    /// <returns>Task<bool> - True if notification sent successfully</returns>
    Task<bool> SendConflictDetectedAsync(Guid patientId, DataConflictDto conflict);
}
