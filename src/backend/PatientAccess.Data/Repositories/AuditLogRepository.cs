using Microsoft.EntityFrameworkCore;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Repositories;

/// <summary>
/// Repository implementation for AuditLog entity using EF Core (NFR-007, FR-005).
/// Implements append-only audit trail operations for compliance.
/// Database triggers prevent UPDATE and DELETE operations on audit_logs table.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly PatientAccessDbContext _context;

    public AuditLogRepository(PatientAccessDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<AuditLog> CreateAsync(AuditLog auditLog)
    {
        if (auditLog == null)
            throw new ArgumentNullException(nameof(auditLog));

        auditLog.AuditLogId = Guid.NewGuid();
        auditLog.Timestamp = DateTime.UtcNow;

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return auditLog;
    }

    /// <inheritdoc />
    public async Task<List<AuditLog>> GetByUserIdAsync(Guid userId, int hours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);

        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Timestamp >= cutoffTime)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<AuditLog>> GetByResourceAsync(string resourceType, Guid resourceId)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.ResourceType == resourceType && a.ResourceId == resourceId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<AuditLog>> GetUnauthorizedAccessAttemptsAsync(int hours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hours);

        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.ActionType == "UnauthorizedAccess" && a.Timestamp >= cutoffTime)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}
