using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire recurring job for processing expired waitlist notifications (US_041 - FR-026, AC-4).
/// Treats expired notifications as declines and offers slots to next eligible patients.
/// Runs every 1 minute for timely timeout processing.
/// </summary>
public class WaitlistTimeoutJob
{
    private readonly IWaitlistNotificationService _notificationService;
    private readonly ILogger<WaitlistTimeoutJob> _logger;

    public WaitlistTimeoutJob(
        IWaitlistNotificationService notificationService,
        ILogger<WaitlistTimeoutJob> logger)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes expired waitlist notifications (AC-4).
    /// Scheduled as Hangfire recurring job every 1 minute.
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            _logger.LogInformation("WaitlistTimeoutJob started");

            var expiredCount = await _notificationService.ProcessTimeoutsAsync();

            if (expiredCount > 0)
            {
                _logger.LogInformation(
                    "Processed {Count} expired waitlist notifications", expiredCount);
            }
            else
            {
                _logger.LogDebug("No expired waitlist notifications found");
            }

            _logger.LogDebug("WaitlistTimeoutJob completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WaitlistTimeoutJob failed");
            // Don't throw — background job should not crash the app
        }
    }
}
