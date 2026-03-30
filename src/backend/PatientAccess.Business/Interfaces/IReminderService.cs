namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Reminder service interface for scheduling and managing appointment reminders (US_037).
/// Handles reminder scheduling on booking, cancellation, and processing of due reminders.
/// </summary>
public interface IReminderService
{
    /// <summary>
    /// Schedules reminder notifications for an appointment based on system-configured intervals (US_037 - AC-1, AC-4).
    /// Creates Notification records for each interval × channel combination.
    /// </summary>
    /// <param name="appointmentId">Appointment ID to schedule reminders for</param>
    /// <returns>Task representing async operation</returns>
    Task ScheduleRemindersAsync(Guid appointmentId);

    /// <summary>
    /// Cancels all pending reminder notifications for an appointment (US_037 - Edge Case).
    /// Sets Notification.Status = Cancelled for pending reminders.
    /// </summary>
    /// <param name="appointmentId">Appointment ID to cancel reminders for</param>
    /// <returns>Task representing async operation</returns>
    Task CancelRemindersAsync(Guid appointmentId);

    /// <summary>
    /// Processes due reminder notifications by enqueuing delivery jobs (US_037 - AC-2).
    /// Queries for Pending notifications where ScheduledTime &lt;= NOW() and enqueues ReminderDeliveryJob.
    /// </summary>
    /// <returns>Count of reminders enqueued for delivery</returns>
    Task<int> ProcessDueRemindersAsync();
}
