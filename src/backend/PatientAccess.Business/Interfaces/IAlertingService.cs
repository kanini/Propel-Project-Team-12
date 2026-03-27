namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for quality alerting when metrics fall below thresholds.
/// Sends email notifications to admin/QA team for AIR-Q01 and AIR-Q03 violations.
/// </summary>
public interface IAlertingService
{
    /// <summary>
    /// Sends quality alert email when metric falls below threshold.
    /// Examples: Agreement Rate <98%, Schema Validity <99%
    /// </summary>
    /// <param name="subject">Alert subject line</param>
    /// <param name="message">Alert message body with metric details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task SendQualityAlertAsync(
        string subject,
        string message,
        CancellationToken cancellationToken = default);
}
