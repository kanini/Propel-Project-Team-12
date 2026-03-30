using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Background job for processing expired waitlist notification responses (US_041 - FR-010).
/// Runs periodically (recommended every 1 minute) to find and process waitlist entries where
/// patients have not responded within the timeout period (default 30 minutes).
/// Automatically cascades to next patient on the waitlist when timeout occurs.
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
    /// Executes the waitlist timeout processing job.
    /// Should be called by background job scheduler (Hangfire).
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            _logger.LogInformation("WaitlistTimeoutJob started - processing expired waitlist notifications");

            // Process all expired waitlist entries (Status=Notified, past ResponseDeadline)
            var expiredCount = await _notificationService.ProcessTimeoutsAsync();

            if (expiredCount == 0)
            {
                _logger.LogInformation("WaitlistTimeoutJob completed - no expired notifications found");
                return;
            }

            _logger.LogInformation(
                "WaitlistTimeoutJob completed - processed {Count} expired notification(s). Slots cascaded to next patients.",
                expiredCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WaitlistTimeoutJob failed with unexpected error");
            // Don't throw - background job should not fail the application
            // Logging and monitoring should alert on errors
        }
    }
}
