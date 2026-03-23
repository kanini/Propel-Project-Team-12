/**
 * QueueManagementService Implementation for Same-Day Queue Operations
 * Handles queue retrieval, priority management, and Pusher event broadcasting
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for managing same-day patient queue with real-time updates
/// </summary>
public class QueueManagementService : IQueueManagementService
{
    private readonly PatientAccessDbContext _context;
    private readonly IPusherService _pusherService;
    private readonly ILogger<QueueManagementService> _logger;
    private const string QueueChannel = "queue-updates";

    public QueueManagementService(
        PatientAccessDbContext context,
        IPusherService pusherService,
        ILogger<QueueManagementService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pusherService = pusherService ?? throw new ArgumentNullException(nameof(pusherService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all patients in the same-day queue with "Arrived" status (US_030, AC-1)
    /// </summary>
    /// <param name="providerId">Optional provider filter (null for all providers)</param>
    /// <returns>Task<List<QueuePatientDto>> - List of queue patients ordered by priority and arrival time</returns>
    public async Task<List<QueuePatientDto>> GetSameDayQueueAsync(Guid? providerId = null)
    {
        try
        {
            _logger.LogInformation("Fetching same-day queue. ProviderId filter: {ProviderId}", providerId ?? Guid.Empty);

            // Fix: Use DateTimeOffset for timestamptz compatibility
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            var tomorrow = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(1), DateTimeKind.Utc);

            _logger.LogInformation("Date range: {Today} to {Tomorrow} (UTC)", today, tomorrow);

            // Debug counts
            var totalTodayCount = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.ScheduledDateTime >= today && a.ScheduledDateTime < tomorrow)
                .CountAsync();

            var arrivedCount = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.Status == AppointmentStatus.Arrived &&
                            a.ScheduledDateTime >= today &&
                            a.ScheduledDateTime < tomorrow)
                .CountAsync();

            _logger.LogInformation("Total today: {Total}, Arrived: {Arrived}", totalTodayCount, arrivedCount);

            // Build query
            var appointmentsQuery = from a in _context.Appointments.AsNoTracking()
                                    join p in _context.Users.AsNoTracking() on a.PatientId equals p.UserId
                                    join pr in _context.Providers.AsNoTracking() on a.ProviderId equals pr.ProviderId
                                    where a.ScheduledDateTime >= today
                                       && a.ScheduledDateTime < tomorrow
                                    // Fix: Removed ArrivalTime.HasValue � all records are NULL
                                    select new
                                    {
                                        Appointment = a,
                                        Patient = p,
                                        Provider = pr
                                    };

            // Optional provider filter
            if (providerId.HasValue)
            {
                appointmentsQuery = appointmentsQuery
                    .Where(x => x.Appointment.ProviderId == providerId.Value);
            }

            try
            {
                _logger.LogInformation("Executing same-day queue query");

                var results = await appointmentsQuery
                    .OrderByDescending(x => x.Appointment.IsPriority)
                    .ThenBy(x => x.Appointment.ScheduledDateTime) // Fix: Use ScheduledDateTime since ArrivalTime is NULL
                    .ToListAsync();

                _logger.LogInformation("Query returned {Count} appointments", results.Count);

                // Fix: Map directly to DTO without mutating entities
                var queuePatients = results.Select(r =>
                {
                    var apt = r.Appointment;
                    apt.Patient = r.Patient;
                    apt.Provider = r.Provider;
                    return MapToQueuePatientDto(apt);
                }).ToList();

                _logger.LogInformation("Mapped {Count} patients to DTO", queuePatients.Count);
                return queuePatients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing queue query: {Message}", ex.Message);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching same-day queue. ProviderId: {ProviderId}", providerId ?? Guid.Empty);
            throw;
        }
    }

    /// <summary>
    /// Update patient priority flag in the queue (US_030, AC-3)
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <param name="isPriority">Priority flag (true for emergency)</param>
    /// <returns>Task<QueuePatientDto> - Updated queue patient data</returns>
    public async Task<QueuePatientDto> UpdatePatientPriorityAsync(Guid appointmentId, bool isPriority)
    {
        try
        {
            _logger.LogInformation("Updating priority for appointment {AppointmentId}. IsPriority: {IsPriority}",
                appointmentId, isPriority);

            // Find appointment with related navigations
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Provider)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            // Validate appointment exists
            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found", appointmentId);
                throw new KeyNotFoundException($"Appointment {appointmentId} not found");
            }

            // Validate appointment status is "Arrived"
            if (appointment.Status != AppointmentStatus.Arrived)
            {
                _logger.LogWarning("Appointment {AppointmentId} has status {Status}, expected Arrived",
                    appointmentId, appointment.Status);
                throw new InvalidOperationException(
                    $"Cannot update priority for appointment with status {appointment.Status}. Only 'Arrived' patients can be prioritized.");
            }

            // Validate arrival time exists
            if (!appointment.ArrivalTime.HasValue)
            {
                _logger.LogWarning("Appointment {AppointmentId} has no arrival time", appointmentId);
                throw new InvalidOperationException("Cannot update priority for appointment without arrival time");
            }

            // Update priority flag
            var oldPriority = appointment.IsPriority;
            appointment.IsPriority = isPriority;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Map to DTO for response and Pusher event
            var queuePatientDto = MapToQueuePatientDto(appointment);

            // Log priority change for audit trail
            _logger.LogInformation(
                "Priority updated for appointment {AppointmentId}. Patient: {PatientName}. IsPriority changed from {OldPriority} to {NewPriority}",
                appointmentId, appointment.Patient.Name, oldPriority, isPriority);

            // Broadcast Pusher event for real-time update (US_030, AC-2)
            await _pusherService.TriggerEventAsync(
                QueueChannel,
                "priority-changed",
                new
                {
                    patientId = appointment.PatientId.ToString(),
                    isPriority = isPriority,
                    updatedQueue = queuePatientDto
                });

            _logger.LogInformation("Priority updated successfully for appointment {AppointmentId}", appointmentId);
            return queuePatientDto;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Error updating priority for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    /// <summary>
    /// Map Appointment entity to QueuePatientDto with calculated wait time
    /// </summary>
    private QueuePatientDto MapToQueuePatientDto(Appointment appointment)
    {
        // Calculate wait time in minutes (current time - arrival time)
        var waitTimeMinutes = appointment.ArrivalTime.HasValue
            ? (int)(DateTime.UtcNow - appointment.ArrivalTime.Value).TotalMinutes
            : 0;

        // Ensure wait time is non-negative
        waitTimeMinutes = Math.Max(0, waitTimeMinutes);

        return new QueuePatientDto
        {
            Id = appointment.AppointmentId,
            PatientId = appointment.PatientId.ToString(),
            PatientName = appointment.Patient?.Name ?? "Unknown",
            AppointmentType = appointment.IsWalkIn ? "Walk-in" : "Scheduled",
            ProviderName = appointment.Provider?.Name ?? "Unknown",
            ProviderId = appointment.ProviderId,
            ArrivalTime = appointment.ArrivalTime ?? DateTime.UtcNow,
            EstimatedWaitTime = waitTimeMinutes,
            IsPriority = appointment.IsPriority
        };
    }
}
