using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Exceptions;

namespace PatientAccess.Business.Services;

/// <summary>
/// SMS service implementation using Twilio SMS gateway (US_037 - TR-010).
/// Implements appointment reminder SMS delivery with development mode fallback.
/// </summary>
public class SmsService : PatientAccess.Business.Interfaces.ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _isEnabled;
    private readonly string? _fromPhoneNumber;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Read Twilio configuration
        _isEnabled = _configuration.GetValue<bool>("TwilioSettings:Enabled", false);
        var accountSid = _configuration["TwilioSettings:AccountSid"];
        var authToken = _configuration["TwilioSettings:AuthToken"];
        _fromPhoneNumber = _configuration["TwilioSettings:FromPhoneNumber"];

        // Initialize Twilio client if enabled and credentials configured
        if (_isEnabled && !string.IsNullOrWhiteSpace(accountSid) && !string.IsNullOrWhiteSpace(authToken))
        {
            TwilioClient.Init(accountSid, authToken);
            _logger.LogInformation("Twilio SMS service initialized. From: {FromPhone}", _fromPhoneNumber);
        }
        else
        {
            _logger.LogWarning(
                "Twilio SMS service running in DEVELOPMENT MODE - messages will be logged but not sent. " +
                "Set TwilioSettings:Enabled=true and configure credentials to enable SMS delivery.");
        }
    }

    public async Task<bool> SendSmsAsync(string toPhoneNumber, string messageBody)
    {
        // Edge case: No phone number provided
        if (string.IsNullOrWhiteSpace(toPhoneNumber))
        {
            _logger.LogWarning("SMS send requested with null/empty phone number - skipping send");
            return true; // Return success (no-op)
        }

        // Development mode: log message instead of sending
        if (!_isEnabled)
        {
            _logger.LogInformation(
                "[DEV MODE] SMS would be sent to {ToPhone}: {Message}",
                toPhoneNumber, messageBody);
            return true;
        }

        try
        {
            var message = await MessageResource.CreateAsync(
                to: toPhoneNumber,
                from: _fromPhoneNumber,
                body: messageBody
            );

            _logger.LogInformation(
                "SMS sent successfully to {ToPhone}. SID: {MessageSid}, Status: {Status}",
                toPhoneNumber, message.Sid, message.Status);

            return true;
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex,
                "Twilio API error sending SMS to {ToPhone}. Code: {ErrorCode}, Message: {ErrorMessage}",
                toPhoneNumber, ex.Code, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending SMS to {ToPhone}", toPhoneNumber);
            return false;
        }
    }

    public async Task<bool> SendAppointmentReminderSmsAsync(
        string toPhoneNumber,
        string patientName,
        string providerName,
        DateTime scheduledDateTime,
        string location)
    {
        // Edge case: No phone number provided (US_037 edge case)
        if (string.IsNullOrWhiteSpace(toPhoneNumber))
        {
            _logger.LogWarning(
                "SMS reminder skipped for patient {PatientName} - no phone number configured",
                patientName);
            return true; // Return success (skip with log entry per edge case requirement)
        }

        // Format appointment reminder message (US_037 - AC-1)
        var messageBody = $"Reminder: You have an appointment with {providerName} " +
                         $"on {scheduledDateTime:MMMM dd, yyyy 'at' h:mm tt}. " +
                         $"Location: {location}. " +
                         $"Please arrive 15 minutes early.";

        _logger.LogInformation(
            "Sending appointment reminder SMS to {ToPhone} for appointment on {DateTime}",
            toPhoneNumber, scheduledDateTime);

        return await SendSmsAsync(toPhoneNumber, messageBody);
    }

    public async Task<bool> SendWaitlistSlotNotificationSmsAsync(
        string toPhoneNumber,
        string patientName,
        string providerName,
        DateTime slotStartTime,
        string confirmUrl,
        string declineUrl,
        int timeoutMinutes)
    {
        // Edge case: No phone number provided
        if (string.IsNullOrWhiteSpace(toPhoneNumber))
        {
            _logger.LogWarning(
                "Waitlist SMS notification skipped for patient {PatientName} - no phone number configured",
                patientName);
            return true; // Return success (skip with log entry)
        }

        // Format waitlist slot notification message (US_041 - AC-1)
        // SMS character limit consideration - keep concise
        var messageBody = $"🎉 {patientName}, a slot with {providerName} is available on " +
                         $"{slotStartTime:MMM dd 'at' h:mm tt}! " +
                         $"Confirm: {confirmUrl} or Decline: {declineUrl}. " +
                         $"Respond within {timeoutMinutes} min.";

        _logger.LogInformation(
            "Sending waitlist slot notification SMS to {ToPhone} for slot starting {DateTime}",
            toPhoneNumber, slotStartTime);

        return await SendSmsAsync(toPhoneNumber, messageBody);
    }
}
