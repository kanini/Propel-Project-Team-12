using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service implementation for intake appointment operations (US_037).
/// Retrieves appointments requiring intake with caching support.
/// </summary>
public class IntakeAppointmentService : IIntakeAppointmentService
{
    private readonly PatientAccessDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<IntakeAppointmentService> _logger;
    
    /// <summary>
    /// Cache expiration for intake appointments (2 minutes sliding expiration).
    /// </summary>
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);
    
    /// <summary>
    /// Cache key prefix for patient intake appointments.
    /// </summary>
    private const string CacheKeyPrefix = "intake_appointments_";

    public IntakeAppointmentService(
        PatientAccessDbContext context,
        IMemoryCache cache,
        ILogger<IntakeAppointmentService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<List<IntakeAppointmentDto>> GetPatientIntakeAppointmentsAsync(
        int patientId, 
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{patientId}";

        // Check cache first
        if (_cache.TryGetValue(cacheKey, out List<IntakeAppointmentDto>? cachedAppointments) && cachedAppointments != null)
        {
            _logger.LogDebug("Cache hit for intake appointments, PatientId: {PatientId}", patientId);
            return cachedAppointments;
        }

        _logger.LogInformation("Fetching intake appointments for PatientId: {PatientId}", patientId);

        try
        {
            // Convert int patientId to Guid for querying (using user_id from Users table)
            // Since frontend passes int, we need to find the user by their integer ID first
            var patientGuid = await GetPatientGuidByIdAsync(patientId, cancellationToken);
            
            if (patientGuid == null)
            {
                _logger.LogWarning("Patient not found for PatientId: {PatientId}", patientId);
                return new List<IntakeAppointmentDto>();
            }

            // Query future appointments that may require intake
            // Note: In a real implementation, there would be a RequiresIntake flag on appointments
            // For now, we'll assume all future appointments require intake
            var now = DateTime.UtcNow;
            
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Provider)
                .Include(a => a.TimeSlot)
                .Where(a => a.PatientId == patientGuid.Value &&
                           a.ScheduledDateTime > now &&
                           a.Status == AppointmentStatus.Scheduled)
                .OrderBy(a => a.ScheduledDateTime)
                .ToListAsync(cancellationToken);

            // Get intake records for these appointments
            var appointmentIds = appointments.Select(a => a.AppointmentId).ToList();
            var intakeRecords = await _context.IntakeRecords
                .AsNoTracking()
                .Where(ir => appointmentIds.Contains(ir.AppointmentId))
                .ToDictionaryAsync(ir => ir.AppointmentId, ir => ir, cancellationToken);

            // Map to DTOs
            var result = appointments.Select(appointment =>
            {
                var hasIntakeRecord = intakeRecords.TryGetValue(appointment.AppointmentId, out var intakeRecord);
                string intakeStatus;
                Guid? intakeSessionId = null;

                if (!hasIntakeRecord || intakeRecord == null)
                {
                    intakeStatus = "Pending";
                }
                else if (intakeRecord.IsCompleted)
                {
                    intakeStatus = "Completed";
                    intakeSessionId = intakeRecord.IntakeRecordId;
                }
                else
                {
                    intakeStatus = "InProgress";
                    intakeSessionId = intakeRecord.IntakeRecordId;
                }

                return new IntakeAppointmentDto
                {
                    AppointmentId = appointment.AppointmentId,
                    ProviderId = appointment.ProviderId,
                    ProviderName = FormatProviderName(appointment.Provider),
                    ProviderSpecialty = appointment.Provider?.Specialty ?? "General Practice",
                    AppointmentDate = appointment.ScheduledDateTime.ToString("yyyy-MM-dd"),
                    AppointmentTime = appointment.TimeSlot?.StartTime.ToString("HH:mm") ?? 
                                     appointment.ScheduledDateTime.ToString("HH:mm"),
                    IntakeStatus = intakeStatus,
                    IntakeSessionId = intakeSessionId
                };
            }).ToList();

            // Cache the result with sliding expiration
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(CacheDuration);
            
            _cache.Set(cacheKey, result, cacheOptions);
            
            _logger.LogInformation(
                "Retrieved {Count} intake appointments for PatientId: {PatientId}",
                result.Count, patientId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching intake appointments for PatientId: {PatientId}", patientId);
            throw;
        }
    }

    /// <summary>
    /// Get patient GUID from integer ID.
    /// This is a workaround since the frontend uses integer IDs but the database uses GUIDs.
    /// </summary>
    private async Task<Guid?> GetPatientGuidByIdAsync(int patientId, CancellationToken cancellationToken)
    {
        // Query users table to find the patient
        // In a real system, the JWT would contain the GUID directly
        // For now, we'll use a hash-based approach or look up by some mapping
        
        // Option 1: If the patientId is actually a string representation extracted from JWT
        // We'll try to get the user from the context where the claim maps to UserId
        
        // Since we receive an int from the frontend but store Guids,
        // we need to handle this gracefully. The controller will extract
        // the actual Guid from the JWT claims.
        
        // For now, return null and let the controller handle the Guid extraction
        // This method signature will be updated when the controller is implemented
        
        // Alternative: Try parsing the int as part of a deterministic Guid pattern
        // This is a temporary solution - the real fix is to use Guids throughout
        
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.Patient)
            .Take(100) // Limit for safety
            .ToListAsync(cancellationToken);
        
        // Find user with matching ID hash (temporary solution)
        foreach (var user in users)
        {
            if (user.UserId.GetHashCode() == patientId || 
                Math.Abs(user.UserId.GetHashCode()) == patientId)
            {
                return user.UserId;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Overload that accepts Guid directly (for use when controller extracts GUID from JWT).
    /// </summary>
    public async Task<List<IntakeAppointmentDto>> GetPatientIntakeAppointmentsAsync(
        Guid patientGuid,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{patientGuid}";

        if (_cache.TryGetValue(cacheKey, out List<IntakeAppointmentDto>? cachedAppointments) && cachedAppointments != null)
        {
            _logger.LogDebug("Cache hit for intake appointments, PatientGuid: {PatientGuid}", patientGuid);
            return cachedAppointments;
        }

        _logger.LogInformation("Fetching intake appointments for PatientGuid: {PatientGuid}", patientGuid);

        try
        {
            var now = DateTime.UtcNow;
            
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Provider)
                .Include(a => a.TimeSlot)
                .Where(a => a.PatientId == patientGuid &&
                           a.ScheduledDateTime > now &&
                           a.Status == AppointmentStatus.Scheduled)
                .OrderBy(a => a.ScheduledDateTime)
                .ToListAsync(cancellationToken);

            var appointmentIds = appointments.Select(a => a.AppointmentId).ToList();
            var intakeRecords = await _context.IntakeRecords
                .AsNoTracking()
                .Where(ir => appointmentIds.Contains(ir.AppointmentId))
                .ToDictionaryAsync(ir => ir.AppointmentId, ir => ir, cancellationToken);

            var result = appointments.Select(appointment =>
            {
                var hasIntakeRecord = intakeRecords.TryGetValue(appointment.AppointmentId, out var intakeRecord);
                string intakeStatus;
                Guid? intakeSessionId = null;

                if (!hasIntakeRecord || intakeRecord == null)
                {
                    intakeStatus = "Pending";
                }
                else if (intakeRecord.IsCompleted)
                {
                    intakeStatus = "Completed";
                    intakeSessionId = intakeRecord.IntakeRecordId;
                }
                else
                {
                    intakeStatus = "InProgress";
                    intakeSessionId = intakeRecord.IntakeRecordId;
                }

                return new IntakeAppointmentDto
                {
                    AppointmentId = appointment.AppointmentId,
                    ProviderId = appointment.ProviderId,
                    ProviderName = FormatProviderName(appointment.Provider),
                    ProviderSpecialty = appointment.Provider?.Specialty ?? "General Practice",
                    AppointmentDate = appointment.ScheduledDateTime.ToString("yyyy-MM-dd"),
                    AppointmentTime = appointment.TimeSlot?.StartTime.ToString("HH:mm") ?? 
                                     appointment.ScheduledDateTime.ToString("HH:mm"),
                    IntakeStatus = intakeStatus,
                    IntakeSessionId = intakeSessionId
                };
            }).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(CacheDuration);
            
            _cache.Set(cacheKey, result, cacheOptions);
            
            _logger.LogInformation(
                "Retrieved {Count} intake appointments for PatientGuid: {PatientGuid}",
                result.Count, patientGuid);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching intake appointments for PatientGuid: {PatientGuid}", patientGuid);
            throw;
        }
    }

    /// <summary>
    /// Format provider name from Provider entity.
    /// </summary>
    private static string FormatProviderName(Provider? provider)
    {
        if (provider == null) return "Unknown Provider";
        
        // Provider model uses single Name field
        return provider.Name;
    }
}
