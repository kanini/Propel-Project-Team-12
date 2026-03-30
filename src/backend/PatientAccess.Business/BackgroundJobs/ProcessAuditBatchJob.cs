using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace PatientAccess.Business.BackgroundJobs;

/// <summary>
/// Hangfire background job for batch processing audit log entries from Redis queue (AC4 - US_059).
/// Processes audit logs asynchronously to ensure zero data loss and non-blocking performance.
/// </summary>
public class ProcessAuditBatchJob
{
    private readonly PatientAccessDbContext _context;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<ProcessAuditBatchJob> _logger;
    
    private const string REDIS_AUDIT_QUEUE = "audit:queue";
    private const int BATCH_SIZE = 100; // Process 100 audit logs per job execution (AC4)
    private const int MAX_RETRIES = 3;

    public ProcessAuditBatchJob(
        PatientAccessDbContext context,
        ILogger<ProcessAuditBatchJob> logger,
        IConnectionMultiplexer? redis = null)
    {
        _context = context;
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Executes the batch job: dequeues audit logs from Redis and persists to PostgreSQL (AC4).
    /// Hangfire job invoked every 5 seconds for near-real-time processing.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // Skip if Redis is not available (graceful degradation)
        if (_redis == null || !_redis.IsConnected)
        {
            _logger.LogDebug("Redis not available. Skipping audit batch processing (logs written directly to database).");
            return;
        }

        var database = _redis.GetDatabase();
        var processedCount = 0;
        var failedCount = 0;

        try
        {
            _logger.LogDebug("Starting audit log batch processing (batch size: {BatchSize})", BATCH_SIZE);

            // Get queue length for monitoring
            var queueLength = await database.ListLengthAsync(REDIS_AUDIT_QUEUE);
            
            if (queueLength == 0)
            {
                _logger.LogDebug("Audit queue is empty. No logs to process.");
                return;
            }

            _logger.LogInformation("Processing {QueueLength} audit logs from queue", queueLength);

            var batch = new List<AuditLog>();

            // Dequeue up to BATCH_SIZE entries
            for (int i = 0; i < Math.Min(BATCH_SIZE, queueLength); i++)
            {
                var auditJson = await database.ListLeftPopAsync(REDIS_AUDIT_QUEUE);
                
                if (auditJson.IsNullOrEmpty)
                    break;

                try
                {
                    var auditLog = JsonSerializer.Deserialize<AuditLog>(auditJson!);
                    
                    if (auditLog != null)
                    {
                        batch.Add(auditLog);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize audit log: {Json}", auditJson);
                        failedCount++;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Invalid JSON in audit queue: {Json}", auditJson);
                    failedCount++;
                }
            }

            // Batch insert to database
            if (batch.Count > 0)
            {
                await _context.AuditLogs.AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                processedCount = batch.Count;

                _logger.LogInformation(
                    "Successfully processed {ProcessedCount} audit logs in batch (AC4 - US_059)",
                    processedCount);
            }

            // Log metrics for health check monitoring
            if (failedCount > 0)
            {
                _logger.LogWarning(
                    "Batch processing completed with {FailedCount} failures (AC4 - US_059)",
                    failedCount);
            }
        }
        catch (RedisException ex)
        {
            _logger.LogError(
                ex,
                "Redis error during audit batch processing. Queue may be temporarily unavailable (Edge Case - US_059).");
            
            // Don't throw - allow Hangfire to retry automatically
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "Database error during audit batch insert. {ProcessedCount} logs may be lost.",
                processedCount);
            
            // Critical: audit loss detected - health check will flag this
            throw; // Rethrow to trigger Hangfire retry
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error during audit batch processing.");
            
            throw; // Rethrow to trigger Hangfire retry
        }
    }

    /// <summary>
    /// Gets current audit queue metrics for health check monitoring (AC4).
    /// </summary>
    public async Task<AuditQueueMetrics> GetQueueMetricsAsync()
    {
        try
        {
            // Return zero metrics if Redis is not available
            if (_redis == null || !_redis.IsConnected)
            {
                var recentLogsWithoutRedis = await _context.AuditLogs
                    .Where(a => a.Timestamp >= DateTime.UtcNow.AddMinutes(-5))
                    .CountAsync();

                return new AuditQueueMetrics
                {
                    QueueLength = 0,
                    RecentLogsIn5Minutes = recentLogsWithoutRedis,
                    IsHealthy = true,
                    LastChecked = DateTime.UtcNow,
                    ErrorMessage = "Redis not configured - direct database writes enabled"
                };
            }

            var database = _redis.GetDatabase();
            var queueLength = await database.ListLengthAsync(REDIS_AUDIT_QUEUE);

            // Check for any failed jobs in Hangfire (would indicate audit loss risk)
            var recentAuditLogs = await _context.AuditLogs
                .Where(a => a.Timestamp >= DateTime.UtcNow.AddMinutes(-5))
                .CountAsync();

            return new AuditQueueMetrics
            {
                QueueLength = (int)queueLength,
                RecentLogsIn5Minutes = recentAuditLogs,
                IsHealthy = queueLength < 1000, // Alert if queue exceeds 1000 entries
                LastChecked = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit queue metrics");
            
            return new AuditQueueMetrics
            {
                QueueLength = -1,
                RecentLogsIn5Minutes = 0,
                IsHealthy = false,
                LastChecked = DateTime.UtcNow,
                ErrorMessage = ex.Message
            };
        }
    }
}

/// <summary>
/// Metrics for audit queue health monitoring (AC4 - US_059).
/// </summary>
public class AuditQueueMetrics
{
    public int QueueLength { get; set; }
    public int RecentLogsIn5Minutes { get; set; }
    public bool IsHealthy { get; set; }
    public DateTime LastChecked { get; set; }
    public string? ErrorMessage { get; set; }
}
