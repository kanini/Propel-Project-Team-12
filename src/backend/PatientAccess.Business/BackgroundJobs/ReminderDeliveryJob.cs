using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.Services;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Background job for delivering individual reminder notifications via SMS/Email (US_037).
/// Implements exponential backoff retry: 60s, 240s, 960s (AC-3: 1min, 4min, 16min per TR-023).
/// </summary>
public class ReminderDeliveryJob
{
    private readonly PatientAccessDbContext _context;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ReminderDeliveryJob> _logger;

    public ReminderDeliveryJob(
        PatientAccessDbContext context,
        ISmsService smsService,
        IEmailService emailService,
        ILogger<ReminderDeliveryJob> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Delivers a reminder notification via the specified channel (SMS or Email).
    /// Hangfire automatically retries on exception with exponential backoff delays.
    /// </summary>
    /// <param name="notificationId">Notification ID to deliver</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })] // US_037 - AC-3: 1min, 4min, 16min
    public async Task DeliverAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing reminder notification {NotificationId}", notificationId);

            // Load notification with navigation properties
            var notification = await _context.Notifications
                .Include(n => n.Recipient)
                .Include(n => n.Appointment)
                    .ThenInclude(a => a!.Provider)
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId, cancellationToken);

            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found - skipping delivery", notificationId);
                return; // Don't retry - notification doesn't exist
            }

            // Race condition check: verify notification still pending
            if (notification.Status != NotificationStatus.Pending)
            {
                _logger.LogWarning(
                    "Notification {NotificationId} status is {Status} (expected Pending) - skipping delivery",
                    notificationId, notification.Status);
                return; // Don't retry - already processed or cancelled
            }

            // Edge case: Appointment cancelled after notification scheduled
            if (notification.Appointment == null || notification.Appointment.Status == AppointmentStatus.Cancelled)
            {
                notification.Status = NotificationStatus.Cancelled;
                notification.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Notification {NotificationId} cancelled - appointment no longer exists or was cancelled",
                    notificationId);
                return; // Don't retry - appointment cancelled
            }

            // Deliver via appropriate channel
            bool deliverySuccess;
            var appointment = notification.Appointment;
            var provider = appointment.Provider;
            var location = provider.Specialty; // Use specialty as location placeholder

            if (notification.ChannelType == ChannelType.SMS)
            {
                deliverySuccess = await _smsService.SendAppointmentReminderSmsAsync(
                    notification.Recipient.Phone ?? string.Empty,
                    notification.Recipient.Name,
                    provider.Name,
                    appointment.ScheduledDateTime,
                    location
                );
            }
            else // Email
            {
                deliverySuccess = await _emailService.SendAppointmentReminderAsync(
                    notification.Recipient.Email ?? string.Empty,
                    notification.Recipient.Name,
                    provider.Name,
                    appointment.ScheduledDateTime,
                    location
                );
            }

            if (deliverySuccess)
            {
                // Success: Update notification status
                notification.Status = NotificationStatus.Sent;
                notification.SentTime = DateTime.UtcNow;
                notification.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Reminder notification {NotificationId} delivered successfully via {Channel}",
                    notificationId, notification.ChannelType);
            }
            else
            {
                // Failure: Increment retry count and throw to trigger Hangfire retry
                notification.RetryCount++;
                notification.LastErrorMessage = $"Delivery failed via {notification.ChannelType} at {DateTime.UtcNow}";
                notification.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Reminder notification {NotificationId} delivery failed via {Channel}. Retry count: {RetryCount}",
                    notificationId, notification.ChannelType, notification.RetryCount);

                // Throw to trigger Hangfire automatic retry with exponential backoff
                throw new InvalidOperationException(
                    $"Failed to deliver reminder notification {notificationId} via {notification.ChannelType}");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Reminder delivery job for notification {NotificationId} was cancelled", notificationId);
            throw; // Rethrow to signal Hangfire
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error delivering reminder notification {NotificationId}",
                notificationId);
            throw; // Rethrow to trigger Hangfire retry
        }
    }
}
