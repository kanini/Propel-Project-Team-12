using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PatientAccess.Business.Services;

/// <summary>
/// Email service implementation for sending transactional emails.
/// Uses SMTP configuration for email delivery.
/// TODO: Integrate with SendGrid or other email provider for production.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _frontendUrl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
    }

    /// <summary>
    /// Sends verification email with activation link (FR-001).
    /// In development, logs email content instead of sending.
    /// </summary>
    public async Task<bool> SendVerificationEmailAsync(string toEmail, string toName, string verificationToken)
    {
        try
        {
            var verificationLink = $"{_frontendUrl}/verify-email?token={Uri.EscapeDataString(verificationToken)}";

            // TODO: Replace with actual email sending logic (SendGrid/SMTP)
            // For now, log the verification link (development mode)
            _logger.LogInformation(
                "VERIFICATION EMAIL (Development Mode)\n" +
                "To: {Email}\n" +
                "Name: {Name}\n" +
                "Verification Link: {Link}\n" +
                "Token expires in 24 hours.",
                toEmail, toName, verificationLink);

            // Simulate async email sending
            await Task.Delay(100);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// Resends verification email for expired or lost tokens.
    /// </summary>
    public async Task<bool> ResendVerificationEmailAsync(string toEmail, string toName, string verificationToken)
    {
        // Same implementation as SendVerificationEmailAsync
        return await SendVerificationEmailAsync(toEmail, toName, verificationToken);
    }

    /// <summary>
    /// Sends appointment confirmation email with PDF attachment (US_028 - FR-012, AC-2, AC-3).
    /// In development, logs email content instead of sending.
    /// </summary>
    public async Task<bool> SendAppointmentConfirmationAsync(
        string toEmail,
        string toName,
        string providerName,
        DateTime scheduledDateTime,
        string confirmationNumber,
        byte[] pdfBytes,
        string pdfFileName)
    {
        try
        {
            // TODO: Replace with actual email sending logic with attachment support (MailKit/SendGrid)
            // For now, log the email details (development mode)
            _logger.LogInformation(
                "APPOINTMENT CONFIRMATION EMAIL (Development Mode)\n" +
                "To: {Email}\n" +
                "Name: {Name}\n" +
                "Provider: {Provider}\n" +
                "Scheduled: {DateTime}\n" +
                "Confirmation Number: {ConfirmationNumber}\n" +
                "PDF Attachment: {FileName} ({Size} bytes)\n" +
                "Email Body:\n" +
                "Dear {Name}, your appointment with {Provider} is confirmed for {DateTime}.\n" +
                "Confirmation Number: {ConfirmationNumber}\n" +
                "Please find your confirmation PDF attached.\n" +
                "\n" +
                "Important Notes:\n" +
                "- Please arrive 15 minutes early\n" +
                "- Bring valid photo ID and insurance card\n" +
                "- To cancel or reschedule, please provide 24 hours notice\n" +
                "\n" +
                "Contact: (555) 123-4567 or support@patientaccess.com",
                toEmail, toName, providerName, scheduledDateTime.ToString("MMMM dd, yyyy 'at' h:mm tt"),
                confirmationNumber, pdfFileName, pdfBytes.Length);

            // Simulate async email sending
            await Task.Delay(100);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment confirmation email to {Email}", toEmail);
            return false;
        }
    }
}
