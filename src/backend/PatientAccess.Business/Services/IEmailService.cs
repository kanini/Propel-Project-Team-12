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
}
