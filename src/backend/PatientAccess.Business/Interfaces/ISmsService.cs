namespace PatientAccess.Business.Interfaces;

/// <summary>
/// SMS service interface for sending text message notifications (US_037 - FR-014).
/// Supports generic SMS delivery and appointment-specific reminder messages via Twilio.
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Sends a generic SMS message to a phone number.
    /// </summary>
    /// <param name="toPhoneNumber">Recipient phone number in E.164 format (e.g., +15551234567)</param>
    /// <param name="messageBody">SMS message content (max 1600 characters)</param>
    /// <returns>True if SMS sent or logged successfully, false on failure</returns>
    Task<bool> SendSmsAsync(string toPhoneNumber, string messageBody);

    /// <summary>
    /// Sends an appointment reminder SMS with structured details (US_037 - AC-1).
    /// </summary>
    /// <param name="toPhoneNumber">Patient phone number in E.164 format</param>
    /// <param name="patientName">Patient name for personalization</param>
    /// <param name="providerName">Provider name</param>
    /// <param name="scheduledDateTime">Appointment date and time</param>
    /// <param name="location">Appointment location</param>
    /// <returns>True if SMS sent/logged successfully, false on failure. Returns true with log skip when phone is null/empty</returns>
    Task<bool> SendAppointmentReminderSmsAsync(
        string toPhoneNumber,
        string patientName,
        string providerName,
        DateTime scheduledDateTime,
        string location);
}
