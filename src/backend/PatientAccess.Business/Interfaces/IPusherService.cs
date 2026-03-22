/**
 * IPusherService Interface for Real-time Event Broadcasting
 * Provides abstraction for Pusher Channels event triggering
 */

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
}
