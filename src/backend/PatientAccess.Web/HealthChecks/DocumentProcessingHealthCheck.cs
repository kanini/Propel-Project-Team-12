using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Web.HealthChecks;

/// <summary>
/// Health check for document processing backlog monitoring (US_043).
/// Monitors documents stuck in Uploaded/Processing status and failed documents.
/// </summary>
public class DocumentProcessingHealthCheck : IHealthCheck
{
    private readonly PatientAccessDbContext _context;
    private readonly IConfiguration _configuration;

    public DocumentProcessingHealthCheck(
        PatientAccessDbContext context,
        IConfiguration configuration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get thresholds from configuration
            var maxBacklogMinutes = _configuration.GetValue<int>("HealthChecks:DocumentProcessing:MaxBacklogMinutes", 5);
            var maxFailedJobs = _configuration.GetValue<int>("HealthChecks:DocumentProcessing:MaxFailedJobs", 10);

            var now = DateTime.UtcNow;
            var backlogThreshold = now.AddMinutes(-maxBacklogMinutes);

            // Query documents in Uploaded status older than threshold
            var backloggedDocuments = await _context.ClinicalDocuments
                .Where(d => d.ProcessingStatus == ProcessingStatus.Uploaded
                         && d.UploadedAt < backlogThreshold)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var backlogCount = backloggedDocuments.Count;

            // Find oldest backlog age
            var oldestBacklogAge = backloggedDocuments.Any()
                ? (int)(now - backloggedDocuments.Min(d => d.UploadedAt)).TotalMinutes
                : 0;

            // Query documents in Failed status
            var failedCount = await _context.ClinicalDocuments
                .Where(d => d.ProcessingStatus == ProcessingStatus.Failed)
                .AsNoTracking()
                .CountAsync(cancellationToken);

            // Build health check data
            var data = new Dictionary<string, object>
            {
                ["backlogCount"] = backlogCount,
                ["oldestBacklogAgeMinutes"] = oldestBacklogAge,
                ["failedCount"] = failedCount,
                ["maxBacklogMinutes"] = maxBacklogMinutes,
                ["maxFailedJobs"] = maxFailedJobs,
                ["timestamp"] = DateTime.UtcNow
            };

            // Determine health status based on thresholds

            // Unhealthy: backlog >= 50 or failed >= 20
            if (backlogCount >= 50 || failedCount >= 20)
            {
                return HealthCheckResult.Unhealthy(
                    $"Document Processing: Critical backlog ({backlogCount} documents) or failures ({failedCount} documents)",
                    data: data);
            }

            // Degraded: backlog >= 10 and < 50, or failed >= maxFailedJobs and < 20
            if (backlogCount >= 10 || failedCount >= maxFailedJobs)
            {
                return HealthCheckResult.Degraded(
                    $"Document Processing: Elevated backlog ({backlogCount} documents) or failures ({failedCount} documents)",
                    data: data);
            }

            // Healthy
            return HealthCheckResult.Healthy(
                "Document Processing: Operating normally",
                data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"Document Processing: Health check failed - {ex.Message}",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["timestamp"] = DateTime.UtcNow
                });
        }
    }
}
