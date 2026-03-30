using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Recurring background job for scanning and enqueuing due appointment reminders (US_037 - AC-2).
/// Runs every 30 seconds to meet NFR-017: reminders delivered within 30 seconds of scheduled trigger time.
/// </summary>
public class ReminderSchedulerJob
{
    private readonly IReminderService _reminderService;
    private readonly ILogger<ReminderSchedulerJob> _logger;

    public ReminderSchedulerJob(
        IReminderService reminderService,
        ILogger<ReminderSchedulerJob> logger)
    {
        _reminderService = reminderService ?? throw new ArgumentNullException(nameof(reminderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the reminder scheduler job - scans for due reminders and enqueues delivery jobs.
    /// Called by Hangfire recurring job scheduler every 30 seconds.
    /// </summary>
    public async Task RunAsync()
    {
        try
        {
            _logger.LogDebug("ReminderSchedulerJob started");

            var count = await _reminderService.ProcessDueRemindersAsync();

            if (count > 0)
            {
                _logger.LogInformation(
                    "ReminderSchedulerJob completed. Enqueued {Count} due reminders for delivery",
                    count);
            }
            else
            {
                _logger.LogDebug("ReminderSchedulerJob completed. No due reminders found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReminderSchedulerJob failed");
            // Don't throw - background job should not fail the application
        }
    }
}
