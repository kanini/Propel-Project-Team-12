using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PatientAccess.Web.HealthChecks;

/// <summary>
/// Health check for Hangfire server availability and job queue monitoring (US_043).
/// Monitors server status, queue depth, and failed job count.
/// </summary>
public class HangfireHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public HangfireHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get thresholds from configuration
            var maxQueueDepth = _configuration.GetValue<int>("HealthChecks:Hangfire:MaxQueueDepth", 50);
            var maxFailedJobs = _configuration.GetValue<int>("HealthChecks:Hangfire:MaxFailedJobs", 10);

            // Get monitoring API
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            // Check active servers using monitoring API
            var servers = monitoringApi.Servers();
            var activeServerCount = servers?.Count ?? 0;

            if (activeServerCount == 0)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy(
                        "Hangfire: No active servers detected",
                        data: new Dictionary<string, object>
                        {
                            ["activeServers"] = activeServerCount,
                            ["timestamp"] = DateTime.UtcNow
                        }));
            }

            // Check enqueued jobs count (queue depth)
            var enqueuedCount = monitoringApi.EnqueuedCount("document-processing");

            // Check failed jobs count
            var failedCount = monitoringApi.FailedCount();

            // Determine health status based on thresholds
            var data = new Dictionary<string, object>
            {
                ["activeServers"] = activeServerCount,
                ["queueDepth"] = enqueuedCount,
                ["failedJobs"] = failedCount,
                ["maxQueueDepth"] = maxQueueDepth,
                ["maxFailedJobs"] = maxFailedJobs,
                ["timestamp"] = DateTime.UtcNow
            };

            // Unhealthy: failed jobs exceed threshold
            if (failedCount >= maxFailedJobs * 2)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy(
                        $"Hangfire: Failed jobs ({failedCount}) exceed critical threshold ({maxFailedJobs * 2})",
                        data: data));
            }

            // Degraded: queue depth or failed jobs exceed warning thresholds
            if (enqueuedCount > maxQueueDepth || failedCount >= maxFailedJobs)
            {
                return Task.FromResult(
                    HealthCheckResult.Degraded(
                        $"Hangfire: Queue depth ({enqueuedCount}) or failed jobs ({failedCount}) above warning threshold",
                        data: data));
            }

            // Healthy
            return Task.FromResult(
                HealthCheckResult.Healthy(
                    "Hangfire: Server operational",
                    data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    $"Hangfire: Health check failed - {ex.Message}",
                    exception: ex,
                    data: new Dictionary<string, object>
                    {
                        ["timestamp"] = DateTime.UtcNow
                    }));
        }
    }
}
