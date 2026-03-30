using System.Text.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.BackgroundJobs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Reminder service implementation for appointment reminder scheduling and management (US_037).
/// Reads configurable intervals from SystemSettings and creates Notification records per channel.
/// </summary>
public class ReminderService : IReminderService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<ReminderService> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ReminderService(
        PatientAccessDbContext context,
        ILogger<ReminderService> logger,
        IBackgroundJobClient backgroundJobClient)
    {
        _context = context;
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task ScheduleRemindersAsync(Guid appointmentId)
    {
        try
        {
            // Load appointment with required navigation properties
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Provider)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                _logger.LogWarning("Cannot schedule reminders - appointment {AppointmentId} not found", appointmentId);
                return;
            }

            // Read reminder configuration from SystemSettings
            var intervalsSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "Reminder.Intervals");
            var smsEnabledSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "Reminder.SmsEnabled");
            var emailEnabledSetting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == "Reminder.EmailEnabled");

            if (intervalsSetting == null)
            {
                _logger.LogWarning("Reminder.Intervals setting not found - skipping reminder scheduling");
                return;
            }

            // Parse intervals JSON array (e.g., "[48, 24, 2]")
            int[] intervals;
            try
            {
                intervals = JsonSerializer.Deserialize<int[]>(intervalsSetting.Value) ?? Array.Empty<int>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Reminder.Intervals value: {Value}", intervalsSetting.Value);
                return;
            }

            if (intervals.Length == 0)
            {
                _logger.LogWarning("No reminder intervals configured - skipping reminder scheduling");
                return;
            }

            // Parse channel enable flags
            var smsEnabled = bool.Parse(smsEnabledSetting?.Value ?? "true");
            var emailEnabled = bool.Parse(emailEnabledSetting?.Value ?? "true");

            var notificationsToCreate = new List<Notification>();
            var now = DateTime.UtcNow;

            // Create notification for each interval × channel combination
            foreach (var intervalHours in intervals)
            {
                var scheduledTime = appointment.ScheduledDateTime.AddHours(-intervalHours);

                // Skip intervals in the past (AC-4: only future reminders)
                if (scheduledTime <= now)
                {
                    _logger.LogDebug(
                        "Skipping reminder {Hours}h before appointment {AppointmentId} - scheduled time {ScheduledTime} is in the past",
                        intervalHours, appointmentId, scheduledTime);
                    continue;
                }

                // SMS notification
                if (smsEnabled && !string.IsNullOrWhiteSpace(appointment.Patient.Phone))
                {
                    notificationsToCreate.Add(new Notification
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
                else if (smsEnabled && string.IsNullOrWhiteSpace(appointment.Patient.Phone))
                {
                    _logger.LogDebug(
                        "Skipping SMS reminder for appointment {AppointmentId} - patient has no phone number (Edge case)",
                        appointmentId);
                }

                // Email notification
                if (emailEnabled)
                {
                    notificationsToCreate.Add(new Notification
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

            // Bulk insert notifications
            if (notificationsToCreate.Any())
            {
                await _context.Notifications.AddRangeAsync(notificationsToCreate);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Scheduled {Count} reminder notifications for appointment {AppointmentId} on {DateTime}",
                    notificationsToCreate.Count, appointmentId, appointment.ScheduledDateTime);
            }
            else
            {
                _logger.LogInformation(
                    "No reminders scheduled for appointment {AppointmentId} - all intervals in past or channels disabled",
                    appointmentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling reminders for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task CancelRemindersAsync(Guid appointmentId)
    {
        try
        {
            // Find all pending notifications for the appointment
            var pendingNotifications = await _context.Notifications
                .Where(n => n.AppointmentId == appointmentId && n.Status == NotificationStatus.Pending)
                .ToListAsync();

            if (pendingNotifications.Any())
            {
                var now = DateTime.UtcNow;
                foreach (var notification in pendingNotifications)
                {
                    notification.Status = NotificationStatus.Cancelled;
                    notification.UpdatedAt = now;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cancelled {Count} pending reminder notifications for appointment {AppointmentId} (Edge case: appointment cancelled)",
                    pendingNotifications.Count, appointmentId);
            }
            else
            {
                _logger.LogDebug(
                    "No pending reminders to cancel for appointment {AppointmentId}",
                    appointmentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reminders for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task<int> ProcessDueRemindersAsync()
    {
        try
        {
            var now = DateTime.UtcNow;

            // Query pending notifications where scheduled time has passed (AC-2: 30-second delivery window)
            var dueNotifications = await _context.Notifications
                .Where(n => n.Status == NotificationStatus.Pending && n.ScheduledTime <= now)
                .OrderBy(n => n.ScheduledTime) // Oldest first
                .Take(50) // Batch limit to prevent long-running transactions
                .ToListAsync();

            if (!dueNotifications.Any())
            {
                return 0;
            }

            // Enqueue delivery job for each due notification
            var enqueuedCount = 0;
            foreach (var notification in dueNotifications)
            {
                try
                {
                    _backgroundJobClient.Enqueue<ReminderDeliveryJob>(
                        job => job.DeliverAsync(notification.NotificationId, CancellationToken.None));
                    enqueuedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to enqueue delivery job for notification {NotificationId}",
                        notification.NotificationId);
                }
            }

            _logger.LogInformation(
                "Enqueued {EnqueuedCount} out of {TotalCount} due reminder notifications for delivery",
                enqueuedCount, dueNotifications.Count);

            return enqueuedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing due reminders");
            return 0;
        }
    }
}
