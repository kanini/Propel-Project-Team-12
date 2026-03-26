using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using PatientAccess.Business.Interfaces;
using System.Text.Json;
using Hangfire;

namespace PatientAccess.Business.Services;

/// <summary>
/// Reminder service implementation for appointment reminder scheduling (US_037 - FR-014).
/// Reads configurable intervals from SystemSettings and creates Notification records.
/// </summary>
public class ReminderService : IReminderService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(
        PatientAccessDbContext context,
        ILogger<ReminderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Schedules reminder notifications for an appointment (AC-1, AC-4).
    /// Creates Notification records for each interval × channel combination.
    /// Skips intervals that are already in the past.
    /// </summary>
    public async Task ScheduleRemindersAsync(Guid appointmentId)
    {
        try
        {
            // Load appointment with Patient and Provider navigation properties
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Provider)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                _logger.LogWarning("Cannot schedule reminders: Appointment {AppointmentId} not found", appointmentId);
                return;
            }

            // Read reminder configuration from SystemSettings
            var intervalsSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "Reminder.Intervals");
            var smsEnabledSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "Reminder.SmsEnabled");
            var emailEnabledSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "Reminder.EmailEnabled");

            // Parse reminder intervals (JSON array: [48, 24, 2])
            var intervals = intervalsSetting != null
                ? JsonSerializer.Deserialize<int[]>(intervalsSetting.Value) ?? new[] { 48, 24, 2 }
                : new[] { 48, 24, 2 };

            var smsEnabled = smsEnabledSetting != null && bool.Parse(smsEnabledSetting.Value);
            var emailEnabled = emailEnabledSetting != null && bool.Parse(emailEnabledSetting.Value);

            var notifications = new List<Notification>();
            var now = DateTime.UtcNow;

            foreach (var hoursBeforeAppointment in intervals)
            {
                var scheduledTime = appointment.ScheduledDateTime.AddHours(-hoursBeforeAppointment);

                // Skip intervals already in the past
                if (scheduledTime <= now)
                {
                    _logger.LogDebug(
                        "Skipping reminder interval {Hours}h for appointment {AppointmentId} - scheduled time {ScheduledTime} is in the past",
                        hoursBeforeAppointment, appointmentId, scheduledTime);
                    continue;
                }

                // Create SMS notification if enabled and patient has phone
                if (smsEnabled && !string.IsNullOrWhiteSpace(appointment.Patient.Phone))
                {
                    notifications.Add(new Notification
                    {
                        RecipientId = appointment.PatientId,
                        AppointmentId = appointmentId,
                        ChannelType = ChannelType.SMS,
                        TemplateName = "appointment_reminder",
                        Status = NotificationStatus.Pending,
                        ScheduledTime = scheduledTime,
                        RetryCount = 0,
                        CreatedAt = now
                    });
                }

                // Create Email notification if enabled
                if (emailEnabled)
                {
                    notifications.Add(new Notification
                    {
                        RecipientId = appointment.PatientId,
                        AppointmentId = appointmentId,
                        ChannelType = ChannelType.Email,
                        TemplateName = "appointment_reminder",
                        Status = NotificationStatus.Pending,
                        ScheduledTime = scheduledTime,
                        RetryCount = 0,
                        CreatedAt = now
                    });
                }
            }

            if (notifications.Any())
            {
                await _context.Notifications.AddRangeAsync(notifications);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Scheduled {Count} reminder notifications for appointment {AppointmentId} at intervals {Intervals}",
                    notifications.Count, appointmentId, string.Join(", ", intervals.Select(h => $"{h}h")));
            }
            else
            {
                _logger.LogWarning(
                    "No reminders scheduled for appointment {AppointmentId} - all intervals in the past or channels disabled",
                    appointmentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule reminders for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    /// <summary>
    /// Cancels all pending reminder notifications for an appointment (Edge case: appointment cancelled).
    /// Sets notification status to Cancelled.
    /// </summary>
    public async Task CancelRemindersAsync(Guid appointmentId)
    {
        try
        {
            var pendingNotifications = await _context.Notifications
                .Where(n => n.AppointmentId == appointmentId && n.Status == NotificationStatus.Pending)
                .ToListAsync();

            if (!pendingNotifications.Any())
            {
                _logger.LogDebug("No pending reminders to cancel for appointment {AppointmentId}", appointmentId);
                return;
            }

            var now = DateTime.UtcNow;
            foreach (var notification in pendingNotifications)
            {
                notification.Status = NotificationStatus.Cancelled;
                notification.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Cancelled {Count} pending reminders for appointment {AppointmentId}",
                pendingNotifications.Count, appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel reminders for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    /// <summary>
    /// Processes due reminder notifications by enqueuing delivery jobs (AC-2: within 30s).
    /// Called by ReminderSchedulerJob every 30 seconds.
    /// </summary>
    public async Task<int> ProcessDueRemindersAsync()
    {
        try
        {
            var now = DateTime.UtcNow;

            // Query notifications due for delivery (using composite index on Status, ScheduledTime)
            var dueNotifications = await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Pending && n.ScheduledTime <= now)
                .OrderBy(n => n.ScheduledTime)
                .Take(50) // Batch limit to prevent long-running transactions
                .ToListAsync();

            if (!dueNotifications.Any())
            {
                return 0;
            }

            // Enqueue each notification for delivery via Hangfire
            foreach (var notification in dueNotifications)
            {
                BackgroundJob.Enqueue<Business.BackgroundJobs.ReminderDeliveryJob>(
                    job => job.DeliverAsync(notification.NotificationId));
            }

            _logger.LogInformation(
                "Enqueued {Count} due reminders for delivery",
                dueNotifications.Count);

            return dueNotifications.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process due reminders");
            return 0;
        }
    }
}
