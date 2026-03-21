using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace PatientAccess.Business.Services;

/// <summary>
/// Email service implementation using SMTP (FR-001).
/// Sends verification emails and other notifications.
/// Uses SendGrid SMTP or fallback SMTP server from configuration.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _frontendUrl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Load SMTP configuration
        var emailConfig = _configuration.GetSection("EmailSettings");
        _smtpHost = emailConfig["SmtpHost"] ?? "smtp.sendgrid.net";
        _smtpPort = int.Parse(emailConfig["SmtpPort"] ?? "587");
        _smtpUsername = emailConfig["SmtpUsername"] ?? string.Empty;
        _smtpPassword = emailConfig["SmtpPassword"] ?? string.Empty;
        _fromEmail = emailConfig["FromEmail"] ?? "noreply@patientaccess.com";
        _fromName = emailConfig["FromName"] ?? "Patient Access Platform";
        _frontendUrl = emailConfig["FrontendUrl"] ?? "http://localhost:5173";
    }

    /// <inheritdoc />
    public async Task<bool> SendVerificationEmailAsync(string email, string name, string verificationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(verificationToken))
            {
                _logger.LogWarning("Cannot send verification email: missing required parameters");
                return false;
            }

            var verificationLink = $"{_frontendUrl}/verify-email?token={Uri.EscapeDataString(verificationToken)}";

            var subject = "Verify Your Email Address - Patient Access Platform";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Verify Your Email</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #2563eb;'>Welcome to Patient Access Platform!</h1>
        <p>Hi {name},</p>
        <p>Thank you for registering. Please verify your email address by clicking the button below:</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{verificationLink}' 
               style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                Verify Email Address
            </a>
        </div>
        <p>Or copy and paste this link into your browser:</p>
        <p style='word-break: break-all; color: #666;'>{verificationLink}</p>
        <p><strong>This link will expire in 24 hours.</strong></p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
        <p style='font-size: 12px; color: #666;'>
            If you didn't create an account, please ignore this email.
        </p>
    </div>
</body>
</html>";

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true,
                Timeout = 10000 // 10 second timeout
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(new MailAddress(email, name));

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("Verification email sent successfully to {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendUserActivationEmailAsync(string email, string name, string temporaryPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(temporaryPassword))
            {
                _logger.LogWarning("Cannot send activation email: missing required parameters");
                return false;
            }

            var loginLink = $"{_frontendUrl}/login";

            var subject = "Your New Account - Patient Access Platform";
            var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Account Created</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h1 style='color: #2563eb;'>Welcome to Patient Access Platform!</h1>
        <p>Hi {name},</p>
        <p>An administrator has created an account for you. You can now log in to the platform using the temporary credentials below:</p>
        <div style='background-color: #f3f4f6; padding: 20px; border-radius: 5px; margin: 20px 0;'>
            <p style='margin: 0; font-weight: bold;'>Email:</p>
            <p style='margin: 5px 0 15px 0; color: #666;'>{email}</p>
            <p style='margin: 0; font-weight: bold;'>Temporary Password:</p>
            <p style='margin: 5px 0; font-family: monospace; font-size: 16px; color: #2563eb;'>{temporaryPassword}</p>
        </div>
        <p><strong>Important:</strong> Please change your password immediately after your first login for security purposes.</p>
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{loginLink}' 
               style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                Log In Now
            </a>
        </div>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
        <p style='font-size: 12px; color: #666;'>
            If you didn't expect this email, please contact your system administrator.
        </p>
    </div>
</body>
</html>";

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true,
                Timeout = 10000 // 10 second timeout
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(new MailAddress(email, name));

            await smtpClient.SendMailAsync(mailMessage);

            _logger.LogInformation("User activation email sent successfully to {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send activation email to {Email}", email);
            return false;
        }
    }
}
