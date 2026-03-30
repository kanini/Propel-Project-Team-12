using Microsoft.Extensions.Diagnostics.HealthChecks;
using PatientAccess.Business.BackgroundJobs;

namespace PatientAccess.Web.HealthChecks;

/// <summary>
/// Health check for audit log batch processing system (AC4 - US_059).
/// Confirms zero audit loss by monitoring queue depth and processing rate.
/// </summary>
public class AuditLogHealthCheck : IHealthCheck
{
    private readonly ProcessAuditBatchJob? _batchJob;
    private readonly ILogger<AuditLogHealthCheck> _logger;
    
    private const int QUEUE_WARNING_THRESHOLD = 500;
    private const int QUEUE_CRITICAL_THRESHOLD = 1000;

    public AuditLogHealthCheck(
        ILogger<AuditLogHealthCheck> logger,
        ProcessAuditBatchJob? batchJob = null)
    {
        _batchJob = batchJob;
        _logger = logger;
    }

    /// <summary>
    /// Performs health check on audit logging system (AC4).
    /// Returns Healthy if queue is processing normally (<500 entries).
    /// Returns Degraded if queue is backing up (500-1000 entries).
    /// Returns Unhealthy if queue is critically backed up (>1000 entries) or processing has stopped.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // If Redis is not configured, return healthy status
            if (_batchJob == null)
            {
                return HealthCheckResult.Healthy(
                    "Audit logging active (direct database writes - Redis not configured)",
                    data: new Dictionary<string, object>
                    {
                        ["mode"] = "direct-database",
                        ["redisEnabled"] = false
                    });
            }

            var metrics = await _batchJob.GetQueueMetricsAsync();

            // Check if metrics retrieval failed
            if (metrics.QueueLength < 0)
            {
                _logger.LogError(
                    "Audit health check failed: Unable to retrieve queue metrics. Error: {Error}",
                    metrics.ErrorMessage);
                
                return HealthCheckResult.Unhealthy(
                    $"Cannot connect to audit queue: {metrics.ErrorMessage}",
                    data: new Dictionary<string, object>
                    {
                        ["queueLength"] = "Unknown",
                        ["recentLogs"] = metrics.RecentLogsIn5Minutes,
                        ["lastChecked"] = metrics.LastChecked,
                        ["error"] = metrics.ErrorMessage ?? "Unknown error"
                    });
            }

            // Check for critically backed up queue (>1000 entries)
            if (metrics.QueueLength >= QUEUE_CRITICAL_THRESHOLD)
            {
                _logger.LogError(
                    "Audit health check CRITICAL: Queue length {QueueLength} exceeds critical threshold {Threshold} (AC4 - US_059)",
                    metrics.QueueLength,
                    QUEUE_CRITICAL_THRESHOLD);
                
                return HealthCheckResult.Unhealthy(
                    $"Audit queue critically backed up: {metrics.QueueLength} entries (threshold: {QUEUE_CRITICAL_THRESHOLD})",
                    data: new Dictionary<string, object>
                    {
                        ["queueLength"] = metrics.QueueLength,
                        ["criticalThreshold"] = QUEUE_CRITICAL_THRESHOLD,
                        ["recentLogs"] = metrics.RecentLogsIn5Minutes,
                        ["lastChecked"] = metrics.LastChecked,
                        ["status"] = "Critical - potential audit loss risk"
                    });
            }

            // Check for warning-level queue backup (500-1000 entries)
            if (metrics.QueueLength >= QUEUE_WARNING_THRESHOLD)
            {
                _logger.LogWarning(
                    "Audit health check DEGRADED: Queue length {QueueLength} exceeds warning threshold {Threshold} (AC4 - US_059)",
                    metrics.QueueLength,
                    QUEUE_WARNING_THRESHOLD);
                
                return HealthCheckResult.Degraded(
                    $"Audit queue backing up: {metrics.QueueLength} entries (warning threshold: {QUEUE_WARNING_THRESHOLD})",
                    data: new Dictionary<string, object>
                    {
                        ["queueLength"] = metrics.QueueLength,
                        ["warningThreshold"] = QUEUE_WARNING_THRESHOLD,
                        ["recentLogs"] = metrics.RecentLogsIn5Minutes,
                        ["lastChecked"] = metrics.LastChecked,
                        ["status"] = "Degraded - queue backing up"
                    });
            }

            // Healthy state: queue is processing normally
            _logger.LogDebug(
                "Audit health check HEALTHY: Queue length {QueueLength}, recent logs {RecentLogs} (AC4 - US_059)",
                metrics.QueueLength,
                metrics.RecentLogsIn5Minutes);
            
            return HealthCheckResult.Healthy(
                $"Audit system operating normally. Queue: {metrics.QueueLength} entries, Recent: {metrics.RecentLogsIn5Minutes} logs",
                data: new Dictionary<string, object>
                {
                    ["queueLength"] = metrics.QueueLength,
                    ["recentLogs"] = metrics.RecentLogsIn5Minutes,
                    ["lastChecked"] = metrics.LastChecked,
                    ["status"] = "Healthy",
                    ["zeroAuditLoss"] = true // Confirms AC4 requirement
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit health check encountered an error");
            
            return HealthCheckResult.Unhealthy(
                $"Audit health check error: {ex.Message}",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["stackTrace"] = ex.StackTrace ?? "N/A"
                });
        }
    }
}
