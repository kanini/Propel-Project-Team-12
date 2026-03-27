using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for sending quality alerts when metrics fall below thresholds.
/// Sends email notifications to admin/QA team for AIR-Q01 and AIR-Q03 violations.
/// </summary>
public class AlertingService : IAlertingService
{
    private readonly ILogger<AlertingService> _logger;
    private readonly IEmailService _emailService;

    public AlertingService(
        ILogger<AlertingService> logger,
        IEmailService emailService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    /// <inheritdoc />
    public async Task SendQualityAlertAsync(
        string subject,
        string message,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Quality alert triggered: {Subject}", subject);

        // Log as high-severity event for Application Insights / monitoring
        _logger.LogError("QUALITY ALERT: {Subject}\n{Message}", subject, message);

        // In production, send email to admin/QA team
        // For now, we log the alert (actual email implementation would use IEmailService)
        // Example: await _emailService.SendQualityAlertEmailAsync("admin@example.com", subject, message);

        // TODO: Implement actual email sending in production
        // Options:
        // 1. Use existing IEmailService.SendEmailAsync if it supports arbitrary recipients
        // 2. Add SendQualityAlertEmailAsync method to IEmailService
        // 3. Use SMTP or SendGrid directly for admin notifications

        _logger.LogInformation("Quality alert logged successfully");

        await Task.CompletedTask; // Placeholder for async operation
    }
}
