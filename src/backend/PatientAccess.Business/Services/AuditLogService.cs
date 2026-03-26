using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Implements audit logging for authentication and authorization events.
/// Logs are written asynchronously to prevent blocking auth operations.
/// Failures are logged to app logger but do not disrupt auth flows (AC edge case).
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        PatientAccessDbContext context,
        ILogger<AuditLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Logs an authentication event asynchronously.
    /// US_022 AC1: Logs userId, timestamp, action type, IP, user agent.
    /// DR-005: Creates immutable audit log entry.
    /// </summary>
    public async Task LogAuthEventAsync(
        Guid? userId,
        AuditActionType actionType,
        string? ipAddress,
        string? userAgent,
        string? metadata = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                ActionType = actionType.ToString(), // Convert enum to string
                ResourceType = "Authentication",
                IpAddress = ipAddress ?? "Unknown",
                UserAgent = userAgent ?? "Unknown",
                ActionDetails = metadata ?? "{}"
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Audit log created: UserId={UserId}, Action={Action}, IP={IP}",
                userId,
                actionType,
                ipAddress);
        }
        catch (Exception ex)
        {
            // Edge case: Audit log failure should not block auth events (non-blocking)
            _logger.LogError(
                ex,
                "Failed to create audit log: UserId={UserId}, Action={Action}",
                userId,
                actionType);
        }
    }

    /// <summary>
    /// Logs a failed login attempt with hashed email for GDPR/privacy compliance.
    /// US_022 AC2: Logs hashed email, timestamp, IP, failure reason.
    /// </summary>
    public async Task LogFailedLoginAsync(
        string email,
        string? ipAddress,
        string? userAgent,
        string failureReason)
    {
        try
        {
            var hashedEmail = HashEmail(email);

            var metadata = JsonSerializer.Serialize(new
            {
                hashedEmail,
                failureReason,
                timestamp = DateTime.UtcNow
            });

            var auditLog = new AuditLog
            {
                UserId = null, // No valid user for failed login
                Timestamp = DateTime.UtcNow,
                ActionType = AuditActionType.FailedLogin.ToString(), // Convert enum to string
                ResourceType = "Authentication",
                IpAddress = ipAddress ?? "Unknown",
                UserAgent = userAgent ?? "Unknown",
                ActionDetails = metadata
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogWarning(
                "Failed login attempt logged: HashedEmail={HashedEmail}, IP={IP}, Reason={Reason}",
                hashedEmail,
                ipAddress,
                failureReason);
        }
        catch (Exception ex)
        {
            // Edge case: Audit log failure should not block auth events
            _logger.LogError(
                ex,
                "Failed to log failed login attempt for email hash");
        }
    }

    /// <summary>
    /// Logs a session timeout event (US_022, AC3).
    /// Called when a user's 15-minute session TTL expires.
    /// </summary>
    public async Task LogSessionTimeoutAsync(
        Guid userId,
        string? ipAddress,
        string? userAgent,
        DateTime? lastActivityTimestamp = null)
    {
        try
        {
            var metadata = JsonSerializer.Serialize(new
            {
                lastActivity = lastActivityTimestamp ?? DateTime.UtcNow,
                reason = "Session expired due to 15-minute inactivity timeout"
            });

            var auditLog = new AuditLog
            {
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                ActionType = AuditActionType.SessionTimeout.ToString(),
                ResourceType = "Authentication",
                IpAddress = ipAddress ?? "Unknown",
                UserAgent = userAgent ?? "Unknown",
                ActionDetails = metadata
            };

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Session timeout logged: UserId={UserId}, LastActivity={LastActivity}",
                userId,
                lastActivityTimestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to log session timeout: UserId={UserId}",
                userId);
        }
    }

    /// <summary>
    /// Queries audit logs with filtering and pagination for admin review.
    /// </summary>
    public async Task<AuditLogQueryResult> GetAuditLogsAsync(
        Guid? userId = null,
        string? actionType = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 25)
    {
        var query = _context.AuditLogs
            .Include(a => a.User)
            .AsNoTracking()
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        if (!string.IsNullOrWhiteSpace(actionType))
        {
            query = query.Where(a => a.ActionType == actionType);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= endDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                AuditLogId = a.AuditLogId,
                UserId = a.UserId,
                UserName = a.User != null ? a.User.Name : null,
                UserEmail = a.User != null ? a.User.Email : null,
                Timestamp = a.Timestamp,
                ActionType = a.ActionType,
                ResourceType = a.ResourceType,
                ActionDetails = a.ActionDetails,
                IpAddress = a.IpAddress
            })
            .ToListAsync();

        return new AuditLogQueryResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Hashes email using SHA256 for privacy compliance (GDPR).
    /// </summary>
    private string HashEmail(string email)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        return Convert.ToBase64String(hashBytes);
    }
}
