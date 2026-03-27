namespace PatientAccess.Business.Interfaces;

/// <summary>
/// Service interface for sending quality metric alerts.
/// US_051 Task 3 - Quality Metrics Tracking.
/// Used to notify admin/QA team when metrics fall below thresholds (AIR-Q01: <98%, AIR-Q03: <99%).
/// </summary>
public interface IAlertingService
{
    /// <summary>
    /// Sends quality alert notification to admin/QA team.
    /// </summary>
    /// <param name="subject">Alert subject line</param>
    /// <param name="message">Alert message body with metric details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendQualityAlertAsync(string subject, string message, CancellationToken cancellationToken = default);
}
