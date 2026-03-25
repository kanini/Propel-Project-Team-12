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
    /// Sends password reset email with reset link.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="toName">Recipient name</param>
    /// <param name="resetToken">Unique password reset token</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken);
}
