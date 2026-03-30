using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Background job for detecting available slots and notifying waitlist patients (US_041 - FR-010).
/// Runs periodically (recommended every 2 minutes) to detect when slots matching waitlist preferences become available.
/// Notifies highest-priority patients via preferred notification channel (Email/SMS/Both).
/// </summary>
public class WaitlistSlotDetectionJob
{
    private readonly IWaitlistNotificationService _notificationService;
    private readonly ILogger<WaitlistSlotDetectionJob> _logger;

    public WaitlistSlotDetectionJob(
        IWaitlistNotificationService notificationService,
        ILogger<WaitlistSlotDetectionJob> logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the waitlist slot detection and notification job.
    /// Should be called by background job scheduler (Hangfire).
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            _logger.LogInformation("WaitlistSlotDetectionJob started - detecting available slots for waitlist patients");

            // Detect all available slots that match active waitlist entries
            var availableSlots = await _notificationService.DetectAvailableSlotsAsync();

            if (availableSlots.Count == 0)
            {
                _logger.LogInformation("WaitlistSlotDetectionJob completed - no available slots found for active waitlist entries");
                return;
            }

            _logger.LogInformation(
                "WaitlistSlotDetectionJob detected {Count} available slot(s) matching waitlist preferences",
                availableSlots.Count);

            // Process each slot - notify highest-priority patient for each
            int successCount = 0;
            foreach (var (waitlistEntryId, timeSlotId) in availableSlots)
            {
                try
                {
                    var notified = await _notificationService.NotifyNextPatientAsync(timeSlotId);
                    if (notified)
                    {
                        successCount++;
                        _logger.LogInformation(
                            "Notified patient for waitlist entry {WaitlistEntryId} about slot {TimeSlotId}",
                            waitlistEntryId, timeSlotId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to notify patient for slot {TimeSlotId} (waitlist entry {WaitlistEntryId})",
                        timeSlotId, waitlistEntryId);
                    // Continue with next slot - don't fail entire job
                }
            }

            _logger.LogInformation(
                "WaitlistSlotDetectionJob completed - successfully notified {SuccessCount}/{TotalCount} patients",
                successCount, availableSlots.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WaitlistSlotDetectionJob failed with unexpected error");
            // Don't throw - background job should not fail the application
            // Logging and monitoring should alert on errors
        }
    }
}
