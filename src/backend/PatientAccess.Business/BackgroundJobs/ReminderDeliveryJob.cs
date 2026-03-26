using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Hangfire;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.Services;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire job that delivers individual reminder notifications with exponential backoff retry (US_037 - AC-3).
/// Retry delays: 60s (1min), 240s (4min), 960s (16min) for max 3 attempts.
/// </summary>
public class ReminderDeliveryJob
{
    private readonly PatientAccess.Data.PatientAccessDbContext _context;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ReminderDeliveryJob> _logger;

    public ReminderDeliveryJob(
        PatientAccess.Data.PatientAccessDbContext context,
        ISmsService smsService,
        IEmailService emailService,
        ILogger<ReminderDeliveryJob> logger)
    {
        _context = context;
        _smsService = smsService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Delivers a reminder notification via SMS or Email (AC-1).
    /// Retries with exponential backoff: 1min, 4min, 16min (AC-3).
    /// </summary>
    /// <param name="notificationId">Notification ID to deliver</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })]
    public async Task DeliverAsync(Guid notificationId)
    {
        try
        {
            // Load notification with related data
            var notification = await _context.Notifications
                .Include(n => n.Recipient)
                .Include(n => n.Appointment)
                    .ThenInclude(a => a!.Provider)
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found", notificationId);
                return;
            }

            // Race condition check: verify notification is still Pending
            if (notification.Status != NotificationStatus.Pending)
            {
                _logger.LogWarning(
                    "Notification {NotificationId} status is {Status}, skipping delivery",
                    notificationId, notification.Status);
                return;
            }

            // Verify appointment is not cancelled
            if (notification.Appointment != null &&
                (notification.Appointment.Status == AppointmentStatus.Cancelled ||
                 notification.Appointment.Status == AppointmentStatus.NoShow))
            {
                notification.Status = NotificationStatus.Cancelled;
                notification.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Notification {NotificationId} cancelled - appointment status is {Status}",
                    notificationId, notification.Appointment.Status);
                return;
            }

            if (notification.Appointment == null)
            {
                _logger.LogError("Notification {NotificationId} has no associated appointment", notificationId);
                notification.Status = NotificationStatus.Failed;
                notification.LastErrorMessage = "No associated appointment found";
                notification.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return;
            }

            // Deliver based on channel type
            bool success = false;
            var appointment = notification.Appointment;

            switch (notification.ChannelType)
            {
                case ChannelType.SMS:
                    success = await _smsService.SendAppointmentReminderSmsAsync(
                        notification.Recipient.Phone ?? string.Empty,
                        notification.Recipient.Name,
                        appointment.Provider!.Name,
                        appointment.ScheduledDateTime,
                        "Patient Access Clinic"); // TODO: Get actual location from Provider entity

                    break;

                case ChannelType.Email:
                    success = await _emailService.SendAppointmentReminderAsync(
                        notification.Recipient.Email,
                        notification.Recipient.Name,
                        appointment.Provider!.Name,
                        appointment.ScheduledDateTime,
                        "Patient Access Clinic"); // TODO: Get actual location from Provider entity

                    break;

                default:
                    _logger.LogError(
                        "Notification {NotificationId} has unsupported channel type {ChannelType}",
                        notificationId, notification.ChannelType);
                    notification.Status = NotificationStatus.Failed;
                    notification.LastErrorMessage = $"Unsupported channel type: {notification.ChannelType}";
                    notification.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return;
            }

            if (success)
            {
                // Mark as sent
                notification.Status = NotificationStatus.Sent;
                notification.SentTime = DateTime.UtcNow;
                notification.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Reminder notification {NotificationId} delivered successfully via {Channel}",
                    notificationId, notification.ChannelType);
            }
            else
            {
                // Delivery failed - increment retry count and throw to trigger Hangfire retry
                notification.RetryCount++;
                notification.LastErrorMessage = $"Delivery failed via {notification.ChannelType}";
                notification.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogWarning(
                    "Reminder notification {NotificationId} delivery failed (retry {RetryCount})",
                    notificationId, notification.RetryCount);

                throw new InvalidOperationException($"Failed to deliver notification {notificationId} via {notification.ChannelType}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error delivering reminder notification {NotificationId}",
                notificationId);

            // Update failure status if max retries exceeded (Hangfire will not retry further)
            try
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification != null && notification.RetryCount >= 3)
                {
                    notification.Status = NotificationStatus.Failed;
                    notification.LastErrorMessage = $"Max retries exceeded: {ex.Message}";
                    notification.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update notification status for {NotificationId}", notificationId);
            }

            throw; // Re-throw to trigger Hangfire retry
        }
    }
}
