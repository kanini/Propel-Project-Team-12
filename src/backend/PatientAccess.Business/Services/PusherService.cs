/**
 * PusherService Implementation for Real-time Event Broadcasting
 * Wraps Pusher Server SDK client for event publishing to Pusher Channels
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PusherServer;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for broadcasting real-time events via Pusher Channels
/// </summary>
public class PusherService : IPusherService
{
    private readonly Pusher _pusherClient;
    private readonly ILogger<PusherService> _logger;
    private readonly bool _isEnabled;

    /// <summary>
    /// Initialize PusherService with configuration and logger
    /// </summary>
    /// <param name="configuration">Configuration for Pusher credentials</param>
    /// <param name="logger">Logger instance</param>
    public PusherService(IConfiguration configuration, ILogger<PusherService> logger)
    {
        _logger = logger;

        // Read Pusher configuration from appsettings
        var pusherAppId = configuration["Pusher:AppId"];
        var pusherKey = configuration["Pusher:Key"];
        var pusherSecret = configuration["Pusher:Secret"];
        var pusherCluster = configuration["Pusher:Cluster"] ?? "mt1";
        var enabledConfig = configuration["Pusher:Enabled"];

        // Check if Pusher is enabled (default to false if not configured)
        _isEnabled = bool.TryParse(enabledConfig, out var enabled) && enabled;

        if (!_isEnabled)
        {
            _logger.LogInformation("Pusher service is disabled. Events will not be broadcast.");
            _pusherClient = null!; // Service is disabled
            return;
        }

        // Validate required configuration
        if (string.IsNullOrWhiteSpace(pusherAppId) ||
            string.IsNullOrWhiteSpace(pusherKey) ||
            string.IsNullOrWhiteSpace(pusherSecret))
        {
            _logger.LogWarning("Pusher configuration is incomplete. Pusher service will be disabled. " +
                             "Required: Pusher:AppId, Pusher:Key, Pusher:Secret, Pusher:Enabled");
            _isEnabled = false;
            _pusherClient = null!;
            return;
        }

        try
        {
            // Initialize Pusher client
            var options = new PusherOptions
            {
                Cluster = pusherCluster,
                Encrypted = true // Use HTTPS for security
            };

            _pusherClient = new Pusher(pusherAppId, pusherKey, pusherSecret, options);
            _logger.LogInformation("Pusher service initialized successfully. Cluster: {Cluster}", pusherCluster);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Pusher service. Events will not be broadcast.");
            _isEnabled = false;
            _pusherClient = null!;
        }
    }

    /// <summary>
    /// Trigger a Pusher event on a specified channel
    /// Implements graceful degradation - logs error but doesn't throw exceptions
    /// </summary>
    /// <param name="channel">Channel name (e.g., "queue-updates")</param>
    /// <param name="eventName">Event name (e.g., "patient-added", "priority-changed")</param>
    /// <param name="data">Event data object to broadcast</param>
    /// <returns>Task<bool> - True if event triggered successfully, False otherwise</returns>
    public async Task<bool> TriggerEventAsync(string channel, string eventName, object data)
    {
        if (!_isEnabled || _pusherClient == null)
        {
            _logger.LogDebug("Pusher service is disabled. Event {EventName} on channel {Channel} not broadcast.",
                eventName, channel);
            return false;
        }

        try
        {
            _logger.LogInformation("Broadcasting Pusher event: {EventName} on channel: {Channel}", eventName, channel);

            // Trigger the event via Pusher API
            var result = await _pusherClient.TriggerAsync(channel, eventName, data);

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Pusher event {EventName} triggered successfully on channel {Channel}",
                    eventName, channel);
                return true;
            }

            _logger.LogWarning("Pusher event {EventName} failed with status code: {StatusCode}. Body: {Body}",
                eventName, result.StatusCode, result.Body);
            return false;
        }
        catch (Exception ex)
        {
            // Graceful degradation: log error but don't fail the request
            _logger.LogError(ex, "Failed to trigger Pusher event {EventName} on channel {Channel}. " +
                               "Event will not be broadcast.", eventName, channel);
            return false;
        }
    }

    /// <summary>
    /// Send real-time notification for completed patient profile aggregation (EP-007, FR-032).
    /// Channel: private-patient-{patientId}
    /// Event: patient-profile-updated
    /// </summary>
    public async Task<bool> SendAggregationCompleteAsync(Guid patientId, AggregationResultDto result)
    {
        var channel = $"private-patient-{patientId}";
        var eventName = "patient-profile-updated";

        var payload = new
        {
            patientId = result.PatientId,
            profileCompleteness = result.ProfileCompleteness,
            newConditionsCount = result.NewConditionsCount,
            newMedicationsCount = result.NewMedicationsCount,
            newAllergiesCount = result.NewAllergiesCount,
            newVitalsCount = result.NewVitalsCount,
            newEncountersCount = result.NewEncountersCount,
            conflictsDetected = result.ConflictsDetected,
            hasUnresolvedConflicts = result.HasUnresolvedConflicts,
            aggregatedAt = result.AggregatedAt,
            isIncremental = result.IsIncremental
        };

        return await TriggerEventAsync(channel, eventName, payload);
    }

    /// <summary>
    /// Send real-time notification for critical conflict detection (US_048, FR-031).
    /// Channel: private-patient-{patientId}
    /// Event: critical-conflict-detected
    /// Only sends for Critical severity conflicts.
    /// </summary>
    public async Task<bool> SendConflictDetectedAsync(Guid patientId, DataConflictDto conflict)
    {
        // Only send Pusher notifications for Critical conflicts
        if (conflict.Severity != Data.Models.Enums.ConflictSeverity.Critical)
        {
            _logger.LogInformation("Skipping Pusher notification for non-critical conflict {ConflictId}", conflict.Id);
            return true;
        }

        var channel = $"private-patient-{patientId}";
        var eventName = "critical-conflict-detected";

        var payload = new
        {
            conflictId = conflict.Id,
            severity = conflict.Severity.ToString(),
            entityType = conflict.EntityType,
            conflictType = conflict.ConflictType,
            description = conflict.Description,
            detectedAt = conflict.CreatedAt
        };

        _logger.LogInformation("Sending critical conflict notification for patient {PatientId}, conflict {ConflictId}",
            patientId, conflict.Id);

        return await TriggerEventAsync(channel, eventName, payload);
    }
}
