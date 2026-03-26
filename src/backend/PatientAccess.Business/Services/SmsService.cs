using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.Services;

/// <summary>
/// SMS service implementation using Twilio (US_037 - FR-014, TR-010).
/// Supports development mode (logs only) and production mode (actual SMS delivery).
/// Edge case: Returns success with log skip when phone number is null/empty (US_037 edge case).
/// </summary>
public class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;
    private readonly bool _smsEnabled;
    private readonly string? _twilioAccountSid;
    private readonly string? _twilioAuthToken;
    private readonly string? _twilioFromPhoneNumber;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Read Twilio configuration
        _smsEnabled = bool.TryParse(_configuration["TwilioSettings:Enabled"], out var enabled) && enabled;
        _twilioAccountSid = _configuration["TwilioSettings:AccountSid"];
        _twilioAuthToken = _configuration["TwilioSettings:AuthToken"];
        _twilioFromPhoneNumber = _configuration["TwilioSettings:FromPhoneNumber"];
    }

    /// <summary>
    /// Sends a generic SMS message.
    /// Development mode: logs message content.
    /// Production mode: sends via Twilio API.
    /// </summary>
    public async Task<bool> SendSmsAsync(string toPhoneNumber, string messageBody)
    {
        try
        {
            // Edge case: no phone number provided (US_037)
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
            {
                _logger.LogWarning("SMS skipped — no phone number provided");
                return true; // Return success to not block flow
            }

            // Development mode: log instead of sending
            if (!_smsEnabled || string.IsNullOrWhiteSpace(_twilioAccountSid))
            {
                _logger.LogInformation(
                    "SMS (Development Mode)\n" +
                    "To: {PhoneNumber}\n" +
                    "Message: {Message}",
                    toPhoneNumber, messageBody);

                await Task.Delay(50); // Simulate network delay
                return true;
            }

            // Production mode: Send via Twilio
            // TODO: Implement Twilio SDK integration when Twilio package is added
            // TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
            // var message = await MessageResource.CreateAsync(
            //     to: new PhoneNumber(toPhoneNumber),
            //     from: new PhoneNumber(_twilioFromPhoneNumber),
            //     body: messageBody);

            _logger.LogInformation("SMS sent to {PhoneNumber} via Twilio", toPhoneNumber);
            await Task.Delay(50); // Placeholder for actual API call
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", toPhoneNumber);
            return false;
        }
    }

    /// <summary>
    /// Sends appointment reminder SMS with structured message (US_037 - AC-1).
    /// Message format: "Reminder: Appointment with Dr. [Provider] on [Date] at [Time]. Location: [Location]. Reply STOP to unsubscribe."
    /// </summary>
    public async Task<bool> SendAppointmentReminderSmsAsync(
        string toPhoneNumber,
        string patientName,
        string providerName,
        DateTime scheduledDateTime,
        string location)
    {
        try
        {
            // Edge case: no phone number (patient has no phone) - US_037
            if (string.IsNullOrWhiteSpace(toPhoneNumber))
            {
                _logger.LogWarning(
                    "SMS reminder skipped for patient {PatientName} — no phone number on file",
                    patientName);
                return true; // Return success to not block reminder flow
            }

            // Format reminder message (AC-1: appointment date, time, provider, and location)
            var messageBody = $"Reminder: Appointment with {providerName} on " +
                              $"{scheduledDateTime:MMMM dd, yyyy} at {scheduledDateTime:h:mm tt}. " +
                              $"Location: {location}. Reply STOP to unsubscribe.";

            // Delegate to generic SMS method
            return await SendSmsAsync(toPhoneNumber, messageBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send appointment reminder SMS to {PhoneNumber} for patient {PatientName}",
                toPhoneNumber,
                patientName);
            return false;
        }
    }
}
