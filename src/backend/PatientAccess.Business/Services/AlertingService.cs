using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for sending quality metric alerts to admin/QA team.
/// US_051 Task 3 - Quality Metrics Tracking.
/// Currently logs alerts; can be extended with SendGrid/MailKit for email notifications.
/// </summary>
public class AlertingService : IAlertingService
{
    private readonly ILogger<AlertingService> _logger;

    public AlertingService(ILogger<AlertingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Sends quality alert notification.
    /// Currently logs as critical error; production implementation should send emails to admin/QA team.
    /// </summary>
    public async Task SendQualityAlertAsync(
        string subject,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogCritical("QUALITY ALERT: {Subject}\n{Message}", subject, message);

        // TODO: Implement email sending in production using SendGrid or MailKit
        // Example SendGrid implementation:
        // var client = new SendGridClient(_apiKey);
        // var from = new EmailAddress("alerts@patientaccess.com", "Quality Alerts");
        // var to = new EmailAddress("qa-team@patientaccess.com", "QA Team");
        // var email = MailHelper.CreateSingleEmail(from, to, subject, message, message);
        // await client.SendEmailAsync(email, cancellationToken);

        // For now, just log the alert
        await Task.CompletedTask;
    }
}
