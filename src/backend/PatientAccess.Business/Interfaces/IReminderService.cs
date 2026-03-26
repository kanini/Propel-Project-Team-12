namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Reminder service interface for scheduling and managing appointment reminders (US_037 - FR-014).
/// Handles reminder creation, cancellation, and due reminder processing.
/// </summary>
public interface IReminderService
{
    /// <summary>
    /// Schedules reminder notifications for an appointment based on system-configured intervals (AC-1).
    /// Creates Notification records for each interval × enabled channel (SMS/Email) combination.
    /// </summary>
    /// <param name="appointmentId">Appointment ID to schedule reminders for</param>
    /// <returns>Task completing when reminders are scheduled</returns>
    Task ScheduleRemindersAsync(Guid appointmentId);

    /// <summary>
    /// Cancels all pending reminder notifications for an appointment (Edge case: appointment cancelled).
    /// </summary>
    /// <param name="appointmentId">Appointment ID to cancel reminders for</param>
    /// <returns>Task completing when reminders are cancelled</returns>
    Task CancelRemindersAsync(Guid appointmentId);

    /// <summary>
    /// Processes all due reminder notifications by enqueuing delivery jobs (AC-2: within 30s).
    /// Called by ReminderSchedulerJob recurring job every 30 seconds.
    /// </summary>
    /// <returns>Count of reminders enqueued for delivery</returns>
    Task<int> ProcessDueRemindersAsync();
}
