using Microsoft.Extensions.Logging;
using PatientAccess.Data.Models;
using PatientAccess.Data.Repositories;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PatientAccess.Business.Services;

/// <summary>
/// Audit service implementation for creating audit trail entries (NFR-007, FR-005, NFR-014).
/// Provides business logic layer for audit logging operations.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IAuditLogRepository auditLogRepository, ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task LogUnauthorizedAccessAsync(Guid userId, string resourceType, Guid resourceId, string action, string? ipAddress = null, string? userAgent = null)
    {
        var details = new
        {
            Action = action,
            Reason = "Insufficient permissions or minimum necessary access violation (NFR-014)"
        };

        var auditLog = new AuditLog
        {
            UserId = userId,
            ActionType = "UnauthorizedAccess",
            ResourceType = resourceType,
            ResourceId = resourceId,
            ActionDetails = JsonSerializer.Serialize(details),
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _auditLogRepository.CreateAsync(auditLog);

        _logger.LogWarning("Unauthorized access attempt: User {UserId} attempted {Action} on {ResourceType} {ResourceId}",
            userId, action, resourceType, resourceId);
    }

    /// <inheritdoc />
    public async Task LogDataAccessAsync(Guid userId, string resourceType, Guid resourceId, string action, string? ipAddress = null, string? userAgent = null)
    {
        var details = new
        {
            Action = action,
            Purpose = "PHI access tracking (FR-005)"
        };

        var auditLog = new AuditLog
        {
            UserId = userId,
            ActionType = action, // Read, Update, Create, Delete
            ResourceType = resourceType,
            ResourceId = resourceId,
            ActionDetails = JsonSerializer.Serialize(details),
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _auditLogRepository.CreateAsync(auditLog);

        _logger.LogInformation("Data access logged: User {UserId} performed {Action} on {ResourceType} {ResourceId}",
            userId, action, resourceType, resourceId);
    }

    /// <inheritdoc />
    public async Task LogAuthenticationEventAsync(Guid userId, string action, string email, string? ipAddress = null, string? userAgent = null)
    {
        var details = new
        {
            Email = email,
            EventType = action
        };

        var auditLog = new AuditLog
        {
            UserId = userId,
            ActionType = action, // Login, Logout, FailedLogin
            ResourceType = "User",
            ResourceId = userId != Guid.Empty ? userId : null,
            ActionDetails = JsonSerializer.Serialize(details),
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _auditLogRepository.CreateAsync(auditLog);

        if (action == "FailedLogin")
        {
            _logger.LogWarning("Failed login attempt for email {Email} from IP {IpAddress}", email, ipAddress);
        }
        else
        {
            _logger.LogInformation("Authentication event: User {UserId} - {Action}", userId, action);
        }
    }

    /// <inheritdoc />
    public async Task LogSuccessfulLoginAsync(Guid userId, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var details = new
            {
                EventType = "Login",
                Timestamp = DateTime.UtcNow
            };

            var auditLog = new AuditLog
            {
                UserId = userId,
                ActionType = "Login",
                ResourceType = "User",
                ResourceId = userId,
                ActionDetails = JsonSerializer.Serialize(details),
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _auditLogRepository.CreateAsync(auditLog);

            _logger.LogInformation("Successful login: User {UserId} from IP {IpAddress}", userId, ipAddress);
        }
        catch (Exception ex)
        {
            // NON-BLOCKING: Log audit failure but don't throw to prevent disrupting authentication
            _logger.LogError(ex, "Failed to create audit log for successful login. User: {UserId}", userId);
        }
    }

    /// <inheritdoc />
    public async Task LogFailedLoginAsync(string emailHash, string? ipAddress, string? userAgent, string failureReason)
    {
        try
        {
            var details = new
            {
                EventType = "FailedLogin",
                EmailHash = emailHash,
                FailureReason = failureReason,
                Timestamp = DateTime.UtcNow
            };

            var auditLog = new AuditLog
            {
                UserId = Guid.Empty,
                ActionType = "FailedLogin",
                ResourceType = "User",
                ResourceId = null,
                ActionDetails = JsonSerializer.Serialize(details),
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _auditLogRepository.CreateAsync(auditLog);

            _logger.LogWarning("Failed login attempt: EmailHash={EmailHash}, IP={IpAddress}, Reason={Reason}",
                emailHash, ipAddress, failureReason);
        }
        catch (Exception ex)
        {
            // NON-BLOCKING: Log audit failure but don't throw
            _logger.LogError(ex, "Failed to create audit log for failed login attempt");
        }
    }

    /// <inheritdoc />
    public async Task LogSessionTimeoutAsync(Guid userId, DateTime lastActivityTime)
    {
        try
        {
            var details = new
            {
                EventType = "SessionTimeout",
                LastActivityTime = lastActivityTime,
                TimeoutDuration = "15 minutes",
                Timestamp = DateTime.UtcNow
            };

            var auditLog = new AuditLog
            {
                UserId = userId,
                ActionType = "SessionTimeout",
                ResourceType = "User",
                ResourceId = userId,
                ActionDetails = JsonSerializer.Serialize(details),
                IpAddress = null, // Not available during timeout event
                UserAgent = null
            };

            await _auditLogRepository.CreateAsync(auditLog);

            _logger.LogInformation("Session timeout: User {UserId}, LastActivity={LastActivity}",
                userId, lastActivityTime);
        }
        catch (Exception ex)
        {
            // NON-BLOCKING: Log audit failure but don't throw
            _logger.LogError(ex, "Failed to create audit log for session timeout. User: {UserId}", userId);
        }
    }

    /// <inheritdoc />
    public async Task LogSessionExtensionAsync(Guid userId)
    {
        try
        {
            var details = new
            {
                EventType = "SessionExtended",
                Timestamp = DateTime.UtcNow
            };

            var auditLog = new AuditLog
            {
                UserId = userId,
                ActionType = "SessionExtended",
                ResourceType = "User",
                ResourceId = userId,
                ActionDetails = JsonSerializer.Serialize(details),
                IpAddress = null,
                UserAgent = null
            };

            await _auditLogRepository.CreateAsync(auditLog);

            _logger.LogInformation("Session extended: User {UserId}", userId);
        }
        catch (Exception ex)
        {
            // NON-BLOCKING: Log audit failure but don't throw
            _logger.LogError(ex, "Failed to create audit log for session extension. User: {UserId}", userId);
        }
    }
}
