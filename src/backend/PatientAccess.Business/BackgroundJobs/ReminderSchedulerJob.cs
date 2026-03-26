using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Recurring Hangfire job that scans for due reminder notifications every 30 seconds (US_037 - AC-2, NFR-017).
/// Ensures reminders are delivered within 30 seconds of their scheduled trigger time.
/// </summary>
public class ReminderSchedulerJob
{
    private readonly IReminderService _reminderService;
    private readonly ILogger<ReminderSchedulerJob> _logger;

    public ReminderSchedulerJob(
        IReminderService reminderService,
        ILogger<ReminderSchedulerJob> logger)
    {
        _reminderService = reminderService;
        _logger = logger;
    }

    /// <summary>
    /// Processes due reminders and enqueues them for delivery.
    /// Called by Hangfire recurring job scheduler every 30 seconds.
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            _logger.LogDebug("ReminderSchedulerJob started - scanning for due reminders");

            var count = await _reminderService.ProcessDueRemindersAsync();

            if (count > 0)
            {
                _logger.LogInformation("ReminderSchedulerJob processed {Count} due reminders", count);
            }
            else
            {
                _logger.LogDebug("ReminderSchedulerJob completed - no due reminders found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReminderSchedulerJob failed");
            throw;
        }
    }
}
