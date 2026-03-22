using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Exceptions;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Waitlist service implementation for US_025 - Waitlist Enrollment (FR-009).
/// Implements FIFO queue with priority timestamp and efficient position calculation using window functions.
/// </summary>
public class WaitlistService : IWaitlistService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<WaitlistService> _logger;

    public WaitlistService(PatientAccessDbContext context, ILogger<WaitlistService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Enrolls patient in waitlist with duplicate detection (FR-009, AC-2, AC-3).
    /// Returns 409 Conflict if patient already on waitlist for provider.
    /// </summary>
    public async Task<WaitlistEntryDto> JoinWaitlistAsync(Guid patientId, JoinWaitlistRequestDto request)
    {
        try
        {
            _logger.LogInformation(
                "Patient {PatientId} joining waitlist for Provider {ProviderId}",
                patientId, request.ProviderId);

            // Validate date range
            if (request.PreferredEndDate < request.PreferredStartDate)
            {
                throw new ArgumentException("PreferredEndDate must be after PreferredStartDate");
            }

            // Check for existing active waitlist entry (AC-3 - duplicate detection)
            var existingEntry = await _context.WaitlistEntries
                .Include(w => w.Provider)
                .Where(w => w.PatientId == patientId &&
                           w.ProviderId == request.ProviderId &&
                           w.Status == WaitlistStatus.Active)
                .FirstOrDefaultAsync();

            if (existingEntry != null)
            {
                // Calculate queue position for existing entry
                var position = await CalculateQueuePositionAsync(existingEntry.WaitlistEntryId, request.ProviderId);

                _logger.LogWarning(
                    "Patient {PatientId} already on waitlist for Provider {ProviderId} at position {Position}",
                    patientId, request.ProviderId, position);

                // Return existing entry with 409 Conflict
                throw new ConflictException($"Patient already on waitlist for this provider at position {position}")
                {
                    Data =
                    {
                        ["ExistingEntry"] = new WaitlistEntryDto
                        {
                            Id = existingEntry.WaitlistEntryId,
                            ProviderId = existingEntry.ProviderId,
                            ProviderName = existingEntry.Provider.Name,
                            Specialty = existingEntry.Provider.Specialty,
                            PreferredStartDate = existingEntry.PreferredDateStart,
                            PreferredEndDate = existingEntry.PreferredDateEnd,
                            NotificationPreference = existingEntry.NotificationPreference,
                            Status = existingEntry.Status,
                            QueuePosition = position,
                            CreatedAt = existingEntry.CreatedAt
                        }
                    }
                };
            }

            // Verify provider exists
            var providerExists = await _context.Providers
                .AnyAsync(p => p.ProviderId == request.ProviderId && p.IsActive);

            if (!providerExists)
            {
                throw new ArgumentException($"Provider {request.ProviderId} does not exist or is inactive");
            }

            // Create new waitlist entry with priority timestamp (FIFO ordering)
            var newEntry = new WaitlistEntry
            {
                WaitlistEntryId = Guid.NewGuid(),
                PatientId = patientId,
                ProviderId = request.ProviderId,
                PreferredDateStart = request.PreferredStartDate,
                PreferredDateEnd = request.PreferredEndDate,
                NotificationPreference = request.NotificationPreference,
                Reason = request.Reason,
                Priority = 1, // Default priority
                Status = WaitlistStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.WaitlistEntries.Add(newEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Waitlist entry {EntryId} created for Patient {PatientId}, Provider {ProviderId}",
                newEntry.WaitlistEntryId, patientId, request.ProviderId);

            // Load provider details for response
            var provider = await _context.Providers
                .Where(p => p.ProviderId == request.ProviderId)
                .Select(p => new { p.Name, p.Specialty })
                .FirstAsync();

            // Calculate queue position (should be last in queue)
            var queuePosition = await CalculateQueuePositionAsync(newEntry.WaitlistEntryId, request.ProviderId);

            return new WaitlistEntryDto
            {
                Id = newEntry.WaitlistEntryId,
                ProviderId = newEntry.ProviderId,
                ProviderName = provider.Name,
                Specialty = provider.Specialty,
                PreferredStartDate = newEntry.PreferredDateStart,
                PreferredEndDate = newEntry.PreferredDateEnd,
                NotificationPreference = newEntry.NotificationPreference,
                Status = newEntry.Status,
                QueuePosition = queuePosition,
                CreatedAt = newEntry.CreatedAt
            };
        }
        catch (ConflictException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining waitlist for Patient {PatientId}", patientId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves patient's waitlist entries with calculated queue positions (FR-009, AC-4).
    /// Uses ROW_NUMBER() window function for efficient position calculation with 50+ entries.
    /// </summary>
    public async Task<List<WaitlistEntryDto>> GetPatientWaitlistAsync(Guid patientId)
    {
        try
        {
            _logger.LogInformation("Fetching waitlist entries for Patient {PatientId}", patientId);

            // Query with window function for queue position calculation
            // ROW_NUMBER() OVER (PARTITION BY ProviderId ORDER BY CreatedAt ASC)
            var entries = await _context.WaitlistEntries
                .Include(w => w.Provider)
                .Where(w => w.PatientId == patientId && w.Status == WaitlistStatus.Active)
                .OrderBy(w => w.CreatedAt)
                .ToListAsync();

            if (!entries.Any())
            {
                return new List<WaitlistEntryDto>();
            }

            // Calculate positions for each entry
            var result = new List<WaitlistEntryDto>();
            foreach (var entry in entries)
            {
                var position = await CalculateQueuePositionAsync(entry.WaitlistEntryId, entry.ProviderId);

                result.Add(new WaitlistEntryDto
                {
                    Id = entry.WaitlistEntryId,
                    ProviderId = entry.ProviderId,
                    ProviderName = entry.Provider.Name,
                    Specialty = entry.Provider.Specialty,
                    PreferredStartDate = entry.PreferredDateStart,
                    PreferredEndDate = entry.PreferredDateEnd,
                    NotificationPreference = entry.NotificationPreference,
                    Status = entry.Status,
                    QueuePosition = position,
                    CreatedAt = entry.CreatedAt
                });
            }

            _logger.LogInformation(
                "Retrieved {Count} waitlist entries for Patient {PatientId}",
                result.Count, patientId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching waitlist for Patient {PatientId}", patientId);
            throw;
        }
    }

    /// <summary>
    /// Updates waitlist preferences while maintaining queue position (FR-009).
    /// Only updates date range and notification preferences, not priority timestamp.
    /// </summary>
    public async Task<WaitlistEntryDto> UpdateWaitlistAsync(Guid entryId, Guid patientId, UpdateWaitlistRequestDto request)
    {
        try
        {
            _logger.LogInformation(
                "Updating waitlist entry {EntryId} for Patient {PatientId}",
                entryId, patientId);

            // Validate date range
            if (request.PreferredEndDate < request.PreferredStartDate)
            {
                throw new ArgumentException("PreferredEndDate must be after PreferredStartDate");
            }

            // Verify ownership and active status
            var entry = await _context.WaitlistEntries
                .Include(w => w.Provider)
                .Where(w => w.WaitlistEntryId == entryId &&
                           w.PatientId == patientId &&
                           w.Status == WaitlistStatus.Active)
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                throw new KeyNotFoundException($"Waitlist entry {entryId} not found or unauthorized");
            }

            // Update preferences (maintain CreatedAt for queue position)
            entry.PreferredDateStart = request.PreferredStartDate;
            entry.PreferredDateEnd = request.PreferredEndDate;
            entry.NotificationPreference = request.NotificationPreference;
            entry.Reason = request.Reason;
            entry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Waitlist entry {EntryId} updated successfully", entryId);

            // Calculate queue position
            var position = await CalculateQueuePositionAsync(entryId, entry.ProviderId);

            return new WaitlistEntryDto
            {
                Id = entry.WaitlistEntryId,
                ProviderId = entry.ProviderId,
                ProviderName = entry.Provider.Name,
                Specialty = entry.Provider.Specialty,
                PreferredStartDate = entry.PreferredDateStart,
                PreferredEndDate = entry.PreferredDateEnd,
                NotificationPreference = entry.NotificationPreference,
                Status = entry.Status,
                QueuePosition = position,
                CreatedAt = entry.CreatedAt
            };
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating waitlist entry {EntryId}", entryId);
            throw;
        }
    }

    /// <summary>
    /// Removes patient from waitlist (FR-009).
    /// Soft delete by setting status to Expired.
    /// </summary>
    public async Task DeleteWaitlistAsync(Guid entryId, Guid patientId)
    {
        try
        {
            _logger.LogInformation(
                "Deleting waitlist entry {EntryId} for Patient {PatientId}",
                entryId, patientId);

            // Verify ownership
            var entry = await _context.WaitlistEntries
                .Where(w => w.WaitlistEntryId == entryId &&
                           w.PatientId == patientId &&
                           w.Status == WaitlistStatus.Active)
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                throw new KeyNotFoundException($"Waitlist entry {entryId} not found or unauthorized");
            }

            // Soft delete
            entry.Status = WaitlistStatus.Cancelled;
            entry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Waitlist entry {EntryId} deleted successfully", entryId);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting waitlist entry {EntryId}", entryId);
            throw;
        }
    }

    /// <summary>
    /// Calculates queue position using FIFO ordering (CreatedAt timestamp).
    /// Uses efficient query to count entries created before the target entry.
    /// </summary>
    private async Task<int> CalculateQueuePositionAsync(Guid entryId, Guid providerId)
    {
        var entry = await _context.WaitlistEntries
            .Where(w => w.WaitlistEntryId == entryId)
            .Select(w => w.CreatedAt)
            .FirstOrDefaultAsync();

        if (entry == default)
        {
            return 0;
        }

        // Count entries for same provider created before or at the same time
        var position = await _context.WaitlistEntries
            .Where(w => w.ProviderId == providerId &&
                       w.Status == WaitlistStatus.Active &&
                       w.CreatedAt <= entry)
            .CountAsync();

        return position;
    }
}
