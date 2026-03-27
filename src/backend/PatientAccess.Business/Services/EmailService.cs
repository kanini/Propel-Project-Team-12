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
            font-family: 'Inter', system-ui, -apple-system, 'Segoe UI', sans-serif;
            background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
            padding: 40px 20px;
            color: #334155;
            line-height: 1.6;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 24px rgba(15, 98, 254, 0.08), 0 8px 48px rgba(0, 0, 0, 0.04);
        }}
        .header {{
            background: linear-gradient(135deg, #0f62fe 0%, #0c50d4 100%);
            padding: 40px 30px;
            text-align: center;
            position: relative;
            overflow: hidden;
        }}
        .header::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -10%;
            width: 300px;
            height: 300px;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 50%;
        }}
        .header::after {{
            content: '';
            position: absolute;
            bottom: -30%;
            left: -5%;
            width: 200px;
            height: 200px;
            background: rgba(255, 255, 255, 0.03);
            border-radius: 50%;
        }}
        .logo-icon {{
            width: 56px;
            height: 56px;
            margin: 0 auto 16px;
            background: rgba(255, 255, 255, 0.95);
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            position: relative;
            z-index: 1;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }}
        .logo-icon svg {{
            width: 32px;
            height: 32px;
        }}
        .brand-title {{
            font-size: 32px;
            font-weight: 700;
            color: #ffffff;
            margin-bottom: 8px;
            position: relative;
            z-index: 1;
            letter-spacing: -0.5px;
        }}
        .brand-subtitle {{
            font-size: 14px;
            font-weight: 500;
            color: rgba(255, 255, 255, 0.9);
            position: relative;
            z-index: 1;
            letter-spacing: 0.5px;
            text-transform: uppercase;
        }}
        .content {{
            padding: 48px 40px;
        }}
        .section-title {{
            font-size: 22px;
            font-weight: 700;
            color: #1e293b;
            margin-bottom: 24px;
            text-align: center;
        }}
        .body-text {{
            font-size: 15px;
            color: #475569;
            margin-bottom: 16px;
            line-height: 1.7;
        }}
        .btn-wrap {{
            text-align: center;
            margin: 32px 0;
        }}
        .btn {{
            display: inline-block;
            padding: 16px 48px;
            background: linear-gradient(135deg, #0f62fe 0%, #0c50d4 100%);
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            letter-spacing: 0.3px;
            box-shadow: 0 4px 12px rgba(15, 98, 254, 0.25);
            transition: all 0.3s ease;
        }}
        .btn:hover {{
            box-shadow: 0 6px 20px rgba(15, 98, 254, 0.35);
            transform: translateY(-2px);
        }}
        .info-card {{
            background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
            border: 1px solid #bae6fd;
            border-radius: 10px;
            padding: 20px;
            margin: 24px 0;
        }}
        .note {{
            font-size: 14px;
            color: #64748b;
            margin-bottom: 12px;
            line-height: 1.6;
        }}
        .note strong {{
            color: #334155;
            font-weight: 600;
        }}
        .divider {{
            border: none;
            border-top: 2px solid #f1f5f9;
            margin: 32px 0;
        }}
        .footer {{
            background-color: #f8fafc;
            padding: 32px 40px;
            text-align: center;
        }}
        .footer-text {{
            font-size: 13px;
            color: #94a3b8;
            line-height: 1.8;
            margin-bottom: 8px;
        }}
        .footer-links {{
            margin-top: 16px;
        }}
        .footer-links a {{
            color: #0f62fe;
            text-decoration: none;
            margin: 0 12px;
            font-size: 13px;
            font-weight: 500;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <div class='logo-icon'>
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <path d='M12 2L2 7L12 12L22 7L12 2Z' fill='#0f62fe' opacity='0.3'/>
                    <path d='M2 17L12 22L22 17V12L12 17L2 12V17Z' fill='#0f62fe'/>
                </svg>
            </div>
            <div class='brand-title'>Patient Access</div>
            <div class='brand-subtitle'>Healthcare Platform</div>
        </div>
        <div class='content'>
            <div class='section-title'>Confirm Your Account</div>

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

        <p class='note'>Need help? Contact us at <a href='mailto:support@propeliq.com' style='color:#0f62fe; font-weight: 600; text-decoration: none;'>support@propeliq.com</a></p>
        </div>

        <div class='footer'>
            <p class='footer-text'>&copy; {DateTime.UtcNow.Year} Patient Access Platform. All rights reserved.</p>
            <p class='footer-text'>You're receiving this email because you created an account with our platform.</p>
            <div class='footer-links'>
                <a href='{_frontendUrl}'>Visit Platform</a>
                <a href='mailto:support@propeliq.com'>Support</a>
                <a href='{_frontendUrl}/privacy'>Privacy Policy</a>
            </div>
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
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Inter', system-ui, -apple-system, 'Segoe UI', sans-serif;
            background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
            padding: 40px 20px;
            color: #334155;
            line-height: 1.6;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 24px rgba(15, 98, 254, 0.08), 0 8px 48px rgba(0, 0, 0, 0.04);
        }}
        .header {{
            background: linear-gradient(135deg, #0f62fe 0%, #0c50d4 100%);
            padding: 40px 30px;
            text-align: center;
            position: relative;
            overflow: hidden;
        }}
        .header::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -10%;
            width: 300px;
            height: 300px;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 50%;
        }}
        .header::after {{
            content: '';
            position: absolute;
            bottom: -30%;
            left: -5%;
            width: 200px;
            height: 200px;
            background: rgba(255, 255, 255, 0.03);
            border-radius: 50%;
        }}
        .logo-icon {{
            width: 56px;
            height: 56px;
            margin: 0 auto 16px;
            background: rgba(255, 255, 255, 0.95);
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            position: relative;
            z-index: 1;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }}
        .logo-icon svg {{
            width: 32px;
            height: 32px;
        }}
        .brand-title {{
            font-size: 32px;
            font-weight: 700;
            color: #ffffff;
            margin-bottom: 8px;
            position: relative;
            z-index: 1;
            letter-spacing: -0.5px;
        }}
        .brand-subtitle {{
            font-size: 14px;
            font-weight: 500;
            color: rgba(255, 255, 255, 0.9);
            position: relative;
            z-index: 1;
            letter-spacing: 0.5px;
            text-transform: uppercase;
        }}
        .content {{
            padding: 48px 40px;
        }}
        .section-title {{
            font-size: 22px;
            font-weight: 700;
            color: #1e293b;
            margin-bottom: 24px;
            text-align: center;
        }}
        .body-text {{
            font-size: 15px;
            color: #475569;
            margin-bottom: 16px;
            line-height: 1.7;
        }}
        .btn-wrap {{
            text-align: center;
            margin: 32px 0;
        }}
        .btn {{
            display: inline-block;
            padding: 16px 48px;
            background: linear-gradient(135deg, #0f62fe 0%, #0c50d4 100%);
            color: #ffffff !important;
            text-decoration: none;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            letter-spacing: 0.3px;
            box-shadow: 0 4px 12px rgba(15, 98, 254, 0.25);
            transition: all 0.3s ease;
        }}
        .btn:hover {{
            box-shadow: 0 6px 20px rgba(15, 98, 254, 0.35);
            transform: translateY(-2px);
        }}
        .warning {{
            background: linear-gradient(135deg, #fffbeb 0%, #fef3c7 100%);
            border: 1px solid #fcd34d;
            border-radius: 10px;
            padding: 24px;
            margin: 24px 0;
        }}
        .warning strong {{
            color: #92400e;
            font-size: 15px;
            font-weight: 700;
            display: block;
            margin-bottom: 12px;
        }}
        .warning ul {{
            margin-left: 20px;
            color: #78350f;
            font-size: 14px;
            line-height: 1.7;
        }}
        .warning li {{
            margin-bottom: 8px;
        }}
        .note {{
            font-size: 14px;
            color: #64748b;
            margin-bottom: 12px;
            line-height: 1.6;
        }}
        .note strong {{
            color: #334155;
            font-weight: 600;
        }}
        .footer {{
            background-color: #f8fafc;
            padding: 32px 40px;
            text-align: center;
        }}
        .footer-text {{
            font-size: 13px;
            color: #94a3b8;
            line-height: 1.8;
            margin-bottom: 8px;
        }}
        .footer-links {{
            margin-top: 16px;
        }}
        .footer-links a {{
            color: #0f62fe;
            text-decoration: none;
            margin: 0 12px;
            font-size: 13px;
            font-weight: 500;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <div class='logo-icon'>
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <path d='M12 2L2 7L12 12L22 7L12 2Z' fill='#0f62fe' opacity='0.3'/>
                    <path d='M2 17L12 22L22 17V12L12 17L2 12V17Z' fill='#0f62fe'/>
                </svg>
            </div>
            <div class='brand-title'>Patient Access</div>
            <div class='brand-subtitle'>Healthcare Platform</div>
        </div>
        <div class='content'>
            <div class='section-title'>Password Reset Request</div>

        <p class='body-text'>Hello {toName},</p>

        <p class='body-text'>
            We received a request to reset your password for your Patient Access account. 
            If you made this request, click the button below to reset your password:
        </p>

        <div class='btn-wrap'>
            <a href='{resetLink}' class='btn'>Reset Password</a>
        </div>

        <div class='warning'>
            <strong>Important Security Information:</strong>
            <ul>
                <li>This password reset link will expire in <strong>1 hour</strong></li>
                <li>For security reasons, the link can only be used once</li>
                <li>If you did not request a password reset, please ignore this email</li>
                <li>Your password will remain unchanged until you use this link</li>
            </ul>
        </div>

        <p class='note'>
            If you did not request this password reset or have concerns about your account security, 
            please contact us immediately at <a href='mailto:support@patientaccess.com' style='color:#0f62fe; font-weight: 600; text-decoration: none;'>support@patientaccess.com</a> 
            or call (555) 123-4567.
        </p>
        </div>

        <div class='footer'>
            <p class='footer-text'>&copy; {DateTime.UtcNow.Year} Patient Access Platform. All rights reserved.</p>
            <p class='footer-text'>Contact us: support@patientaccess.com | (555) 123-4567</p>
            <div class='footer-links'>
                <a href='{_frontendUrl}'>Visit Platform</a>
                <a href='mailto:support@patientaccess.com'>Support</a>
                <a href='{_frontendUrl}/privacy'>Privacy Policy</a>
            </div>
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
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: 'Inter', system-ui, -apple-system, 'Segoe UI', sans-serif;
            background: linear-gradient(135deg, #f8fafc 0%, #e2e8f0 100%);
            padding: 40px 20px;
            color: #334155;
            line-height: 1.6;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 24px rgba(15, 98, 254, 0.08), 0 8px 48px rgba(0, 0, 0, 0.04);
        }}
        .header {{
            background: linear-gradient(135deg, #0f62fe 0%, #0c50d4 100%);
            padding: 40px 30px;
            text-align: center;
            position: relative;
            overflow: hidden;
        }}
        .header::before {{
            content: '';
            position: absolute;
            top: -50%;
            right: -10%;
            width: 300px;
            height: 300px;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 50%;
        }}
        .header::after {{
            content: '';
            position: absolute;
            bottom: -30%;
            left: -5%;
            width: 200px;
            height: 200px;
            background: rgba(255, 255, 255, 0.03);
            border-radius: 50%;
        }}
        .logo-icon {{
            width: 56px;
            height: 56px;
            margin: 0 auto 16px;
            background: rgba(255, 255, 255, 0.95);
            border-radius: 12px;
            display: flex;
            align-items: center;
            justify-content: center;
            position: relative;
            z-index: 1;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }}
        .logo-icon svg {{
            width: 32px;
            height: 32px;
        }}
        .brand-title {{
            font-size: 32px;
            font-weight: 700;
            color: #ffffff;
            margin-bottom: 8px;
            position: relative;
            z-index: 1;
            letter-spacing: -0.5px;
        }}
        .brand-subtitle {{
            font-size: 14px;
            font-weight: 500;
            color: rgba(255, 255, 255, 0.9);
            position: relative;
            z-index: 1;
            letter-spacing: 0.5px;
            text-transform: uppercase;
        }}
        .content {{
            padding: 48px 40px;
        }}
        .success-badge {{
            text-align: center;
            margin-bottom: 24px;
        }}
        .success-badge svg {{
            width: 64px;
            height: 64px;
            margin-bottom: 12px;
        }}
        .section-title {{
            font-size: 22px;
            font-weight: 700;
            color: #16a34a;
            margin-bottom: 24px;
            text-align: center;
        }}
        .body-text {{
            font-size: 15px;
            color: #475569;
            margin-bottom: 16px;
            line-height: 1.7;
        }}
        .info-box {{
            background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
            border: 1px solid #7dd3fc;
            border-radius: 10px;
            padding: 24px;
            margin: 24px 0;
        }}
        .info-box p {{
            font-size: 15px;
            color: #0c4a6e;
            margin-bottom: 12px;
            line-height: 1.6;
        }}
        .info-box p:last-child {{
            margin-bottom: 0;
        }}
        .info-box strong {{
            color: #075985;
            font-weight: 700;
        }}
        .important {{
            background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);
            border: 1px solid #86efac;
            border-radius: 10px;
            padding: 24px;
            margin: 24px 0;
        }}
        .important h3 {{
            color: #166534;
            font-size: 16px;
            font-weight: 700;
            margin-bottom: 14px;
        }}
        .important ul {{
            margin-left: 20px;
            color: #15803d;
            font-size: 14px;
            line-height: 1.7;
        }}
        .important li {{
            margin-bottom: 8px;
        }}
        .note {{
            font-size: 14px;
            color: #64748b;
            margin-bottom: 12px;
            line-height: 1.6;
        }}
        .note strong {{
            color: #334155;
            font-weight: 600;
        }}
        .footer {{
            background-color: #f8fafc;
            padding: 32px 40px;
            text-align: center;
        }}
        .footer-text {{
            font-size: 13px;
            color: #94a3b8;
            line-height: 1.8;
            margin-bottom: 8px;
        }}
        .footer-links {{
            margin-top: 16px;
        }}
        .footer-links a {{
            color: #0f62fe;
            text-decoration: none;
            margin: 0 12px;
            font-size: 13px;
            font-weight: 500;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <div class='logo-icon'>
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <path d='M12 2L2 7L12 12L22 7L12 2Z' fill='#0f62fe' opacity='0.3'/>
                    <path d='M2 17L12 22L22 17V12L12 17L2 12V17Z' fill='#0f62fe'/>
                </svg>
            </div>
            <div class='brand-title'>Patient Access</div>
            <div class='brand-subtitle'>Healthcare Platform</div>
        </div>
        <div class='content'>
            <div class='success-badge'>
                <svg viewBox='0 0 24 24' fill='none' xmlns='http://www.w3.org/2000/svg'>
                    <circle cx='12' cy='12' r='11' fill='#dcfce7'/>
                    <circle cx='12' cy='12' r='9' fill='#16a34a'/>
                    <path d='M7 12L10.5 15.5L17 9' stroke='white' stroke-width='2.5' stroke-linecap='round' stroke-linejoin='round'/>
                </svg>
            </div>
            <div class='section-title'>Appointment Confirmed!</div>

        <p class='body-text'>Hello {toName},</p>

        <p class='body-text'>
            Your appointment has been successfully confirmed! We look forward to seeing you.
        </p>

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

        <p class='note'>Your confirmation details are attached as a PDF document for your records.</p>

        <p class='note'>
            If you need to make any changes or have questions, please contact us at 
            <a href='mailto:support@patientaccess.com' style='color:#0f62fe; font-weight: 600; text-decoration: none;'>support@patientaccess.com</a> 
            or call (555) 123-4567.
        </p>
        </div>

        <div class='footer'>
            <p class='footer-text'>&copy; {DateTime.UtcNow.Year} Patient Access Platform. All rights reserved.</p>
            <p class='footer-text'>Contact us: support@patientaccess.com | (555) 123-4567</p>
            <div class='footer-links'>
                <a href='{_frontendUrl}'>Visit Platform</a>
                <a href='mailto:support@patientaccess.com'>Support</a>
                <a href='{_frontendUrl}/privacy'>Privacy Policy</a>
            </div>
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
