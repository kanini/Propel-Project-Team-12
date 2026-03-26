namespace PatientAccess.Business.Services;

/// <summary>
/// Email service interface for sending transactional emails.
/// Supports verification emails, notifications, and other email communications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a verification email with activation link to newly registered user (FR-001).
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="toName">Recipient name</param>
    /// <param name="verificationToken">Unique verification token for account activation</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendVerificationEmailAsync(string toEmail, string toName, string verificationToken);

    /// <summary>
    /// Resends verification email for expired tokens.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="toName">Recipient name</param>
    /// <param name="verificationToken">New verification token</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> ResendVerificationEmailAsync(string toEmail, string toName, string verificationToken);

    /// <summary>
    /// Sends appointment confirmation email with PDF attachment (US_028 - FR-012, AC-2).
    /// </summary>
    /// <param name="toEmail">Patient email address</param>
    /// <param name="toName">Patient name</param>
    /// <param name="providerName">Provider name</param>
    /// <param name="scheduledDateTime">Appointment date and time</param>
    /// <param name="confirmationNumber">Unique confirmation number</param>
    /// <param name="pdfBytes">PDF attachment content</param>
    /// <param name="pdfFileName">PDF file name</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendAppointmentConfirmationAsync(
        string toEmail,
        string toName,
        string providerName,
        DateTime scheduledDateTime,
        string confirmationNumber,
        byte[] pdfBytes,
        string pdfFileName);

    /// <summary>
    /// Sends appointment reminder email without PDF attachment (US_037 - FR-014, AC-1).
    /// </summary>
    /// <param name="toEmail">Patient email address</param>
    /// <param name="toName">Patient name</param>
    /// <param name="providerName">Provider name</param>
    /// <param name="scheduledDateTime">Appointment date and time</param>
    /// <param name="location">Appointment location</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken);
    Task<bool> SendAppointmentReminderAsync(
        string toEmail,
        string toName,
        string providerName,
        DateTime scheduledDateTime,
        string location);

    /// <summary>
    /// Sends waitlist slot availability notification with confirm/decline links (US_041 - FR-026, AC-1).
    /// </summary>
    /// <param name="toEmail">Patient email address</param>
    /// <param name="toName">Patient name</param>
    /// <param name="providerName">Provider name</param>
    /// <param name="slotDateTime">Available time slot date and time</param>
    /// <param name="confirmUrl">URL for confirming the slot</param>
    /// <param name="declineUrl">URL for declining the slot</param>
    /// <param name="timeoutMinutes">Number of minutes before the offer expires</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendWaitlistSlotNotificationAsync(
        string toEmail,
        string toName,
        string providerName,
        DateTime slotDateTime,
        string confirmUrl,
        string declineUrl,
        int timeoutMinutes);
}
