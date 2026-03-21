namespace PatientAccess.Business.Services;

/// <summary>
/// Email service interface for sending notifications (FR-001).
/// Supports verification emails and other notifications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends email verification link to user.
    /// Non-blocking operation with configured timeout.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="name">Recipient name for personalization</param>
    /// <param name="verificationToken">Verification token for link</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendVerificationEmailAsync(string email, string name, string verificationToken);

    /// <summary>
    /// Sends account activation email with temporary password (US_021 AC1).
    /// Used when Admin creates Staff/Admin user accounts.
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="name">Recipient name for personalization</param>
    /// <param name="temporaryPassword">Generated temporary password</param>
    /// <returns>True if email sent successfully, false otherwise</returns>
    Task<bool> SendUserActivationEmailAsync(string email, string name, string temporaryPassword);
}
