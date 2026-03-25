using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PatientAccess.Business.Services;

/// <summary>
/// Email service implementation for sending transactional emails.
/// Uses Brevo (formerly Sendinblue) API for email delivery.
/// Implements FR-001 verification email and US_028 appointment confirmation email.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _frontendUrl;
    private readonly string _brevoApiKey;
    private readonly string _brevoApiUrl;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public EmailService(
        IConfiguration configuration, 
        ILogger<EmailService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
        
        // Load Brevo settings from configuration
        _brevoApiKey = _configuration["BrevoSettings:ApiKey"] ?? throw new InvalidOperationException("Brevo API key not configured");
        _brevoApiUrl = _configuration["BrevoSettings:ApiUrl"] ?? "https://api.brevo.com/v3/smtp/email";
        _senderEmail = _configuration["BrevoSettings:SenderEmail"] ?? "madhave.susheel@gmail.com";
        _senderName = _configuration["BrevoSettings:SenderName"] ?? "Patient Access Platform";
    }

    /// <summary>
    /// Sends verification email with activation link (FR-001).
    /// Uses Brevo transactional email API.
    /// </summary>
    public async Task<bool> SendVerificationEmailAsync(string toEmail, string toName, string verificationToken)
    {
        try
        {
            var verificationLink = $"{_frontendUrl}/verify-email?token={Uri.EscapeDataString(verificationToken)}";

            // Create email payload for Brevo API
            var emailPayload = new
            {
                sender = new
                {
                    name = _senderName,
                    email = _senderEmail
                },
                to = new[]
                {
                    new
                    {
                        email = toEmail,
                        name = toName
                    }
                },
                subject = "Verify Your Patient Access Account",
                htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4F46E5; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #4F46E5; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Patient Access</h1>
        </div>
        <div class='content'>
            <p>Dear {toName},</p>
            <p>Thank you for registering with Patient Access Platform. To complete your registration and activate your account, please verify your email address by clicking the button below:</p>
            <div style='text-align: center;'>
                <a href='{verificationLink}' class='button'>Verify Email Address</a>
            </div>
            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all;'>{verificationLink}</p>
            <p><strong>This verification link will expire in 24 hours.</strong></p>
            <p>If you did not create this account, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.UtcNow.Year} Patient Access Platform. All rights reserved.</p>
            <p>Contact us: support@patientaccess.com | (555) 123-4567</p>
        </div>
    </div>
</body>
</html>"
            };

            // Send email via Brevo API
            var result = await SendBrevoEmailAsync(emailPayload);

            if (result)
            {
                _logger.LogInformation("Verification email sent successfully to {Email}", toEmail);
            }
            else
            {
                _logger.LogWarning("Failed to send verification email to {Email}", toEmail);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending verification email to {Email}", toEmail);
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
    /// Sends password reset email with reset link.
    /// </summary>
    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken)
    {
        try
        {
            var resetLink = $"{_frontendUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

            // Create email payload for Brevo API
            var emailPayload = new
            {
                sender = new
                {
                    name = _senderName,
                    email = _senderEmail
                },
                replyTo = new
                {
                    email = _senderEmail,
                    name = _senderName
                },
                to = new[]
                {
                    new
                    {
                        email = toEmail,
                        name = toName
                    }
                },
                subject = "Reset Your Password - Patient Access",
                htmlContent = $@"
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
                <p><strong>⚠️ Important Security Information:</strong></p>
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
</html>"
            };

            // Send email via Brevo API
            var result = await SendBrevoEmailAsync(emailPayload);

            if (result)
            {
                _logger.LogInformation("Password reset email sent successfully to {Email}", toEmail);
            }
            else
            {
                _logger.LogWarning("Failed to send password reset email to {Email}", toEmail);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// Sends appointment confirmation email with PDF attachment (US_028 - FR-012, AC-2, AC-3).
    /// Uses Brevo transactional email API with attachment support.
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
            // Convert PDF to Base64 for Brevo API
            var pdfBase64 = Convert.ToBase64String(pdfBytes);

            // Create email payload with attachment
            var emailPayload = new
            {
                sender = new
                {
                    name = _senderName,
                    email = _senderEmail
                },
                replyTo = new
                {
                    email = _senderEmail,
                    name = _senderName
                },
                to = new[]
                {
                    new
                    {
                        email = toEmail,
                        name = toName
                    }
                },
                subject = $"Appointment Confirmation - {confirmationNumber}",
                htmlContent = $@"
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
            <h1>✓ Appointment Confirmed</h1>
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
</html>",
                attachment = new[]
                {
                    new
                    {
                        content = pdfBase64,
                        name = pdfFileName
                    }
                }
            };

            // Send email via Brevo API
            var result = await SendBrevoEmailAsync(emailPayload);

            if (result)
            {
                _logger.LogInformation(
                    "Appointment confirmation email sent to {Email} for appointment {ConfirmationNumber}",
                    toEmail, confirmationNumber);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to send appointment confirmation email to {Email}",
                    toEmail);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending appointment confirmation email to {Email}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// Helper method to send email via Brevo API.
    /// </summary>
    /// <param name="emailPayload">Email payload object</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    private async Task<bool> SendBrevoEmailAsync(object emailPayload)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            // Set Brevo API key in request headers
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("api-key", _brevoApiKey);

            // Serialize payload to JSON
            var jsonContent = JsonSerializer.Serialize(emailPayload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            _logger.LogInformation("Sending email via Brevo API. Sender: {SenderEmail}, API URL: {ApiUrl}", 
                _senderEmail, _brevoApiUrl);
            _logger.LogDebug("Brevo API Request Payload: {Payload}", jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Send POST request to Brevo API
            var response = await httpClient.PostAsync(_brevoApiUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✓ Email sent successfully via Brevo. Response: {Response}", responseBody);
                return true;
            }
            else
            {
                _logger.LogError(
                    "✗ Brevo API error. Status: {StatusCode}, Response: {Response}\n" +
                    "IMPORTANT: If you see 'sender not verified' or similar errors, you must:\n" +
                    "1. Log into your Brevo account at https://app.brevo.com\n" +
                    "2. Go to Senders & IP > Senders\n" +
                    "3. Add and verify the sender email: {SenderEmail}\n" +
                    "4. Check the verification email in your inbox and click the verification link",
                    response.StatusCode, responseBody, _senderEmail);
                
                // Check for common Brevo error codes
                if (responseBody.Contains("unauthorized_sender") || responseBody.Contains("sender") && responseBody.Contains("not verified"))
                {
                    _logger.LogError(
                        "⚠️ SENDER NOT VERIFIED ERROR DETECTED!\n" +
                        "The sender email '{SenderEmail}' is not verified in your Brevo account.\n" +
                        "Please verify it at: https://app.brevo.com/settings/senders", 
                        _senderEmail);
                }
                
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calling Brevo API. Check your API key and network connection.");
            return false;
        }
    }
}
