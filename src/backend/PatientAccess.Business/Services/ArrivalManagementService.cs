/**
 * ArrivalManagementService Implementation for Patient Arrival Operations
 * Handles arrival search, status marking, and Pusher event broadcasting
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Service for managing patient arrivals with real-time queue updates
/// </summary>
public class ArrivalManagementService : IArrivalManagementService
{
    private readonly PatientAccessDbContext _context;
    private readonly IPusherService _pusherService;
    private readonly ILogger<ArrivalManagementService> _logger;
    private const string QueueChannel = "queue-updates";

    public ArrivalManagementService(
        PatientAccessDbContext context,
        IPusherService pusherService,
        ILogger<ArrivalManagementService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pusherService = pusherService ?? throw new ArgumentNullException(nameof(pusherService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Search for appointments scheduled for today matching the query (US_031, AC-1)
    /// If no query provided, returns all appointments for the date
    /// </summary>
    /// <param name="query">Search term (patient name, email, or phone). Empty string returns all appointments.</param>
    /// <param name="date">Date to search for appointments (defaults to today)</param>
    /// <returns>Task<List<ArrivalSearchResultDto>> - List of matching appointments</returns>
    public async Task<List<ArrivalSearchResultDto>> SearchTodayAppointmentsAsync(string? query = null, DateTime? date = null)
    {
        try
        {
            // Use provided date or default to today (UTC)
            // Ensure DateTime.Kind is set to UTC for PostgreSQL compatibility
            var searchDate = date.HasValue
                ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc)
                : DateTime.UtcNow.Date;
            var nextDay = searchDate.AddDays(1);

            _logger.LogInformation("Searching appointments for date: {Date}, query: {Query}", searchDate, query);

            // Build base query for appointments on the specified date
            var appointmentsQuery = _context.Appointments
                .AsNoTracking() // Read-only optimization
                .Include(a => a.Patient)
                .Include(a => a.Provider)
                .Where(a => a.ScheduledDateTime >= searchDate &&
                           a.ScheduledDateTime < nextDay);

            // Apply search filter if query provided and valid
            if (!string.IsNullOrWhiteSpace(query) && query.Trim().Length >= 2)
            {
                var searchTerm = query.Trim().ToLower();
                appointmentsQuery = appointmentsQuery.Where(a =>
                    a.Patient.Name.ToLower().Contains(searchTerm) ||
                    (a.Patient.Email != null && a.Patient.Email.ToLower().Contains(searchTerm)) ||
                    (a.Patient.Phone != null && a.Patient.Phone.Contains(searchTerm)));
            }

            // Execute query with ordering
            var appointments = await appointmentsQuery
                .OrderBy(a => a.ScheduledDateTime) // Order by time
                .ToListAsync();

            // Map to DTOs
            var results = appointments.Select(a => MapToArrivalSearchResultDto(a)).ToList();

            _logger.LogInformation("Found {Count} appointments for query: {Query}", results.Count, query);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching appointments. Query: {Query}", query);
            throw;
        }
    }

    /// <summary>
    /// Mark an appointment as arrived and record arrival time (US_031, AC-2)
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <returns>Task<ArrivalSearchResultDto> - Updated appointment data</returns>
    public async Task<ArrivalSearchResultDto> MarkAppointmentArrivedAsync(Guid appointmentId)
    {
        try
        {
            _logger.LogInformation("Marking appointment {AppointmentId} as arrived", appointmentId);

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

            // Edge case: Check if already arrived (409 Conflict)
            if (appointment.Status == AppointmentStatus.Arrived)
            {
                _logger.LogInformation("Appointment {AppointmentId} already marked as arrived", appointmentId);
                throw new InvalidOperationException("Patient already marked as arrived");
            }

            // Edge case: Check if cancelled (400 Bad Request)
            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                _logger.LogWarning("Attempt to mark cancelled appointment {AppointmentId} as arrived", appointmentId);
                throw new InvalidOperationException("Appointment was cancelled. Cannot mark as arrived.");
            }

            // Validate status is Scheduled or Confirmed
            if (appointment.Status != AppointmentStatus.Scheduled && appointment.Status != AppointmentStatus.Confirmed)
            {
                _logger.LogWarning("Appointment {AppointmentId} has invalid status {Status} for arrival marking",
                    appointmentId, appointment.Status);
                throw new InvalidOperationException(
                    $"Cannot mark appointment with status {appointment.Status} as arrived. Only Scheduled or Confirmed appointments can be marked.");
            }

            // Update status to Arrived and record arrival time
            var oldStatus = appointment.Status;
            appointment.Status = AppointmentStatus.Arrived;
            appointment.ArrivalTime = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Map to DTO for response and Pusher event
            var arrivalDto = MapToArrivalSearchResultDto(appointment);

            // Log arrival marking for audit trail
            _logger.LogInformation(
                "Appointment {AppointmentId} marked as arrived. Patient: {PatientName}. Status changed from {OldStatus} to Arrived. Arrival time: {ArrivalTime}",
                appointmentId, appointment.Patient.Name, oldStatus, appointment.ArrivalTime);

            // Broadcast Pusher event to add patient to queue (US_031, AC-2)
            await _pusherService.TriggerEventAsync(
                QueueChannel,
                "patient-added",
                new
                {
                    appointmentId = appointment.AppointmentId.ToString(),
                    patientId = appointment.PatientId.ToString(),
                    patientName = appointment.Patient.Name,
                    arrivalTime = appointment.ArrivalTime,
                    providerId = appointment.ProviderId.ToString(),
                    providerName = appointment.Provider.Name,
                    appointmentType = appointment.IsWalkIn ? "Walk-in" : "Scheduled"
                });

            _logger.LogInformation("Appointment {AppointmentId} marked as arrived successfully", appointmentId);
            return arrivalDto;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Error marking appointment {AppointmentId} as arrived", appointmentId);
            throw;
        }
    }

    /// <summary>
    /// Map Appointment entity to ArrivalSearchResultDto
    /// </summary>
    private ArrivalSearchResultDto MapToArrivalSearchResultDto(Appointment appointment)
    {
        return new ArrivalSearchResultDto
        {
            AppointmentId = appointment.AppointmentId,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient?.Name ?? "Unknown",
            DateOfBirth = appointment.Patient?.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
            ScheduledDateTime = appointment.ScheduledDateTime,
            ProviderName = appointment.Provider?.Name ?? "Unknown",
            VisitReason = appointment.VisitReason ?? string.Empty,
            Status = appointment.Status.ToString(),
            NoShowRiskScore = appointment.NoShowRiskScore,
            RiskLevel = appointment.NoShowRiskScore.HasValue
                ? DeriveRiskLevel(appointment.NoShowRiskScore.Value)
                : null
        };
    }

    /// <summary>
    /// Derive risk level from score (US_038 - FR-023)
    /// Low: < 40, Medium: 40-70, High: > 70
    /// </summary>
    private static string DeriveRiskLevel(decimal score)
    {
        return score switch
        {
            < 40 => "Low",
            >= 40 and <= 70 => "Medium",
            _ => "High"
        };
    }
}
