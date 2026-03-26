using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire recurring job for detecting available time slots matching waitlist entries (US_041 - FR-026, AC-1).
/// Runs every 2 minutes to balance responsiveness with database load.
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
    /// Detects available slots matching waitlist entries and notifies highest-priority patients.
    /// Scheduled as Hangfire recurring job every 2 minutes.
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            _logger.LogInformation("WaitlistSlotDetectionJob started");

            var matches = await _notificationService.DetectAvailableSlotsAsync();

            _logger.LogInformation(
                "Found {Count} waitlist matches for available slots", matches.Count);

            foreach (var (_, timeSlotId) in matches)
            {
                // Notify highest-priority patient for each available slot
                await _notificationService.NotifyNextPatientAsync(timeSlotId);
            }

            _logger.LogInformation("WaitlistSlotDetectionJob completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WaitlistSlotDetectionJob failed");
            // Don't throw — background job should not crash the app
        }
    }
}
