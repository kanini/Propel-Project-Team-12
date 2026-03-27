using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace PatientAccess.Business.Services;

/// <summary>
/// Email service implementation for sending transactional emails.
/// Uses Resend API for email delivery.
/// Implements FR-001 verification email and US_028 appointment confirmation email.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly string _frontendUrl;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly IResend _resendClient;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:5173";

        var apiKey = configuration["ResendSettings:ApiKey"] ?? throw new InvalidOperationException("Resend API key not configured");
        _senderEmail = configuration["ResendSettings:SenderEmail"] ?? "onboarding@resend.dev";
        _senderName = configuration["ResendSettings:SenderName"] ?? "Patient Access Platform";

        _resendClient = ResendClient.Create(apiKey);
    }

    public async Task<bool> SendVerificationEmailAsync(string toEmail, string toName, string verificationToken)
    {
        try
        {
            var verificationLink = $"{_frontendUrl}/verify-email?token={Uri.EscapeDataString(verificationToken)}";

                    var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Segoe UI', Arial, sans-serif;
            background-color: #eef0f2;
            padding: 40px 20px;
            color: #333;
            line-height: 1.6;
        }}
        .wrapper {{
            max-width: 560px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 12px rgba(0,0,0,0.08);
            padding: 48px 52px 36px;
        }}
        .brand-title {{
            text-align: center;
            font-size: 26px;
            font-weight: 700;
            color: #1f7a5c;
            margin-bottom: 6px;
        }}
        .subtitle {{
            text-align: center;
            font-size: 16px;
            font-weight: 600;
            color: #1f7a5c;
            margin-bottom: 28px;
        }}
        .body-text {{
            font-size: 14.5px;
            color: #444;
            margin-bottom: 16px;
        }}
        .btn-wrap {{
            text-align: center;
            margin: 28px 0;
        }}
        .btn {{
            display: inline-block;
            padding: 14px 44px;
            background-color: #1a1a1a;
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 50px;
            font-size: 15px;
            font-weight: 600;
            letter-spacing: 0.3px;
        }}
        .note {{
            font-size: 13.5px;
            color: #555;
            margin-bottom: 12px;
        }}
        .divider {{
            border: none;
            border-top: 1px solid #e5e5e5;
            margin: 28px 0 16px;
        }}
        .footer {{
            text-align: center;
            font-size: 12px;
            color: #888;
            line-height: 1.8;
        }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='brand-title'>Patient Access</div>
        <div class='subtitle'>Confirm your account</div>

        <p class='body-text'>Hello {toName},</p>

        <p class='body-text'>
            Thank you for creating your account with <strong>PropelIQ</strong>.
        </p>

        <p class='body-text'>
            To complete your registration and start using the platform, please confirm your email
            address by clicking the button below.
        </p>

        <div class='btn-wrap'>
            <a href='{verificationLink}' class='btn'>Confirm My Account</a>
        </div>

        <p class='note'>If you did not create this account, you can safely ignore this email.</p>

        <p class='note'>This confirmation link will expire in <strong>24 hours</strong>.</p>

        <p class='note'>Need help? Contact us at <a href='mailto:support@propeliq.com' style='color:#1f7a5c;'>support@propeliq.com</a></p>

        <hr class='divider' />

        <div class='footer'>
            <p>&copy; {DateTime.UtcNow.Year} PropelIQ. All rights reserved.</p>
            <p>You're receiving this email because you created an account with PropelIQ.</p>
        </div>
    </div>
</body>
</html>";

            var result = await SendEmailAsync(toEmail, "Verify Your Patient Access Account", htmlContent);

            if (result)
                _logger.LogInformation("Verification email sent successfully to {Email}", toEmail);
            else
                _logger.LogWarning("Failed to send verification email to {Email}", toEmail);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification email to {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> ResendVerificationEmailAsync(string toEmail, string toName, string verificationToken)
    {
        return await SendVerificationEmailAsync(toEmail, toName, verificationToken);
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken)
    {
        try
        {
            var resetLink = $"{_frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #EF4444; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #EF4444; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .warning {{ background-color: #FEF3C7; padding: 15px; margin: 15px 0; border-radius: 5px; border-left: 4px solid #F59E0B; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Dear {toName},</p>
            <p>We received a request to reset your password for your Patient Access account. If you made this request, click the button below to reset your password:</p>
            <div style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </div>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all;'>{resetLink}</p>
            
            <div class='warning'>
                <p><strong>Important Security Information:</strong></p>
                <ul>
                    <li>This password reset link will expire in 1 hour</li>
                    <li>For security reasons, the link can only be used once</li>
                    <li>If you did not request a password reset, please ignore this email</li>
                    <li>Your password will remain unchanged until you use this link</li>
                </ul>
            </div>

            <p>If you did not request this password reset or have concerns about your account security, please contact us immediately at support@patientaccess.com or call (555) 123-4567.</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.UtcNow.Year} Patient Access Platform. All rights reserved.</p>
            <p>Contact us: support@patientaccess.com | (555) 123-4567</p>
        </div>
    </div>
</body>
</html>";

            var result = await SendEmailAsync(toEmail, "Reset Your Password - Patient Access", htmlContent);

            if (result)
                _logger.LogInformation("Password reset email sent successfully to {Email}", toEmail);
            else
                _logger.LogWarning("Failed to send password reset email to {Email}", toEmail);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", toEmail);
            return false;
        }
    }

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
            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10B981; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .info-box {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #10B981; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .important {{ background-color: #FEF3C7; padding: 15px; margin: 15px 0; border-radius: 5px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Appointment Confirmed</h1>
        </div>
        <div class='content'>
            <p>Dear {toName},</p>
            <p>Your appointment has been successfully confirmed!</p>
            
            <div class='info-box'>
                <p><strong>Provider:</strong> {providerName}</p>
                <p><strong>Date & Time:</strong> {scheduledDateTime:MMMM dd, yyyy 'at' h:mm tt}</p>
                <p><strong>Confirmation Number:</strong> {confirmationNumber}</p>
            </div>

            <div class='important'>
                <h3>Important Reminders:</h3>
                <ul>
                    <li>Please arrive 15 minutes early for check-in</li>
                    <li>Bring valid photo ID and insurance card</li>
                    <li>To cancel or reschedule, please provide at least 24 hours notice</li>
                </ul>
            </div>

            <p>Your confirmation details are attached as a PDF document.</p>
            
            <p>If you need to make any changes or have questions, please contact us:</p>
            <p><strong>Phone:</strong> (555) 123-4567<br>
            <strong>Email:</strong> support@patientaccess.com</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.UtcNow.Year} Patient Access Platform. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            var message = new EmailMessage
            {
                From = $"{_senderName} <{_senderEmail}>",
                Subject = $"Appointment Confirmation - {confirmationNumber}",
                HtmlBody = htmlContent,
                Attachments = new List<EmailAttachment>
                {
                    new EmailAttachment
                    {
                        Filename = pdfFileName,
                        Content = pdfBytes
                    }
                }
            };
            message.To.Add(toEmail);

            var response = await _resendClient.EmailSendAsync(message);

            if (response != null)
            {
                _logger.LogInformation(
                    "Appointment confirmation email sent to {Email} for appointment {ConfirmationNumber}",
                    toEmail, confirmationNumber);
                return true;
            }

            _logger.LogWarning("Failed to send appointment confirmation email to {Email}", toEmail);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending appointment confirmation email to {Email}", toEmail);
            return false;
        }
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        try
        {
            var message = new EmailMessage
            {
                From = $"{_senderName} <{_senderEmail}>",
                Subject = subject,
                HtmlBody = htmlContent
            };
            message.To.Add(toEmail);

            _logger.LogInformation("Sending email via Resend. To: {ToEmail}, Subject: {Subject}", toEmail, subject);

            var response = await _resendClient.EmailSendAsync(message);

            if (response != null)
            {
                _logger.LogInformation("Email sent successfully via Resend.");
                return true;
            }

            _logger.LogError("Resend API returned null response for email to {Email}", toEmail);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calling Resend API. Check your API key and network connection.");
            return false;
        }
    }
}
