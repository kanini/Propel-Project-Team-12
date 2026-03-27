using Hangfire;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for patient profile aggregation (EP-007, US_047).
/// Used for manual re-aggregation (admin fixes, algorithm updates, conflict resolution).
/// Invoked on-demand via admin API or Hangfire dashboard.
/// </summary>
public class PatientProfileAggregationJob
{
    private readonly IDataAggregationService _aggregationService;
    private readonly ILogger<PatientProfileAggregationJob> _logger;

    public PatientProfileAggregationJob(
        IDataAggregationService aggregationService,
        ILogger<PatientProfileAggregationJob> logger)
    {
        _aggregationService = aggregationService ?? throw new ArgumentNullException(nameof(aggregationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes full patient profile re-aggregation (EP-007, Task 003).
    /// Use cases:
    /// - Admin fixes for data issues
    /// - Algorithm update rollouts
    /// - Post-conflict resolution refresh
    /// Retry policy: 2 attempts with 2-minute delay.
    /// </summary>
    /// <param name="patientId">Patient unique identifier</param>
    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 120, 300 }, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    [Queue("profile-aggregation")] // Dedicated queue for profile aggregation
    public async Task Execute(Guid patientId)
    {
        _logger.LogInformation("Executing patient profile re-aggregation job for Patient {PatientId}", patientId);

        try
        {
            var result = await _aggregationService.ReaggregatePatientProfileAsync(patientId);

            _logger.LogInformation(
                "Patient profile re-aggregation completed for Patient {PatientId}. " +
                "Profile Completeness: {ProfileCompleteness}%, " +
                "Documents Processed: {TotalDocuments}, " +
                "Conflicts Detected: {ConflictsCount}, " +
                "Processing Time: {ProcessingTimeMs}ms",
                patientId,
                result.ProfileCompleteness,
                result.TotalDocumentsProcessed,
                result.ConflictsDetected,
                result.ProcessingDurationMs);

            if (result.HasUnresolvedConflicts)
            {
                _logger.LogWarning("Patient {PatientId} has {ConflictsCount} unresolved conflicts after re-aggregation",
                    patientId, result.ConflictsDetected);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Patient profile re-aggregation job failed for Patient {PatientId}. Error: {ErrorMessage}",
                patientId, ex.Message);

            // Hangfire will retry automatically (2 attempts)
            throw;
        }
    }

    /// <summary>
    /// Enqueues patient profile re-aggregation job for background execution.
    /// Returns Hangfire job ID for tracking.
    /// </summary>
    /// <param name="patientId">Patient unique identifier</param>
    /// <returns>Hangfire job ID</returns>
    public static string Enqueue(Guid patientId)
    {
        return BackgroundJob.Enqueue<PatientProfileAggregationJob>(job => job.Execute(patientId));
    }
}
