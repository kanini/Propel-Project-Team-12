namespace PatientAccess.Business.Interfaces;

/// <summary>
/// SMS service interface for sending text message notifications (US_037).
/// Supports appointment reminders via Twilio SMS gateway (TR-010).
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends a generic SMS message to a recipient.
    /// </summary>
    /// <param name="toPhoneNumber">Recipient phone number in E.164 format (e.g., +15551234567)</param>
    /// <param name="messageBody">Message content (max 160 characters for single SMS)</param>
    /// <returns>True if SMS sent successfully, false otherwise</returns>
    Task<bool> SendSmsAsync(string toPhoneNumber, string messageBody);

    /// <summary>
    /// Sends appointment reminder SMS with appointment details (US_037 - FR-022, AC-1).
    /// Handles Edge Case: Returns true with log entry if phone number is null/empty (no-op).
    /// </summary>
    /// <param name="toPhoneNumber">Patient phone number in E.164 format</param>
    /// <param name="patientName">Patient full name</param>
    /// <param name="providerName">Provider full name</param>
    /// <param name="scheduledDateTime">Appointment date and time</param>
    /// <param name="location">Appointment location/address</param>
    /// <returns>True if SMS sent successfully or skipped (no phone), false on delivery failure</returns>
    Task<bool> SendAppointmentReminderSmsAsync(
        string toPhoneNumber,
        string patientName,
        string providerName,
        DateTime scheduledDateTime,
        string location);

    /// <summary>
    /// Sends waitlist slot availability notification SMS with confirm/decline links (US_041 - AC-1).
    /// </summary>
    /// <param name="toPhoneNumber">Patient phone number in E.164 format</param>
    /// <param name="patientName">Patient full name</param>
    /// <param name="providerName">Provider full name</param>
    /// <param name="slotStartTime">Available slot start time</param>
    /// <param name="confirmUrl">URL to confirm booking</param>
    /// <param name="declineUrl">URL to decline offer</param>
    /// <param name="timeoutMinutes">Response timeout in minutes</param>
    /// <returns>True if SMS sent successfully or skipped (no phone), false on delivery failure</returns>
    Task<bool> SendWaitlistSlotNotificationSmsAsync(
        string toPhoneNumber,
        string patientName,
        string providerName,
        DateTime slotStartTime,
        string confirmUrl,
        string declineUrl,
        int timeoutMinutes);
}
