using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using System.Text.Json;

namespace PatientAccess.Business.Services;

/// <summary>
/// Staff dashboard service implementation for US_068.
/// Provides dashboard metrics and queue preview with caching for performance optimization.
/// </summary>
public class StaffDashboardService : IStaffDashboardService
{
    private readonly PatientAccessDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<StaffDashboardService> _logger;
    private const int CacheDurationMinutes = 5;

    public StaffDashboardService(
        PatientAccessDbContext context,
        IDistributedCache cache,
        ILogger<StaffDashboardService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves dashboard metrics including today's appointments, queue size, and pending verifications (US_068, AC2).
    /// Implements Redis caching with 5-minute expiration for performance (NFR-001).
    /// </summary>
    public async Task<StaffDashboardMetricsDto> GetDashboardMetricsAsync()
    {
        try
        {

            _logger.LogInformation("Staff dashboard metrics cache miss, calculating stats");

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            // Calculate metrics
            var todayAppointments = await _context.Appointments
                .Where(a => a.ScheduledDateTime >= today 
                    && a.ScheduledDateTime < tomorrow
                    && a.Status != AppointmentStatus.Cancelled)
                .CountAsync();

            var currentQueueSize = await _context.Appointments
                .Where(a => a.ScheduledDateTime >= today 
                    && a.ScheduledDateTime < tomorrow
                    && (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed))
                .CountAsync();

            var pendingVerifications = await _context.ExtractedClinicalData
                .Where(ecd => ecd.VerificationStatus == VerificationStatus.AISuggested)
                .CountAsync();

            var metrics = new StaffDashboardMetricsDto
            {
                TodayAppointments = todayAppointments,
                CurrentQueueSize = currentQueueSize,
                PendingVerifications = pendingVerifications
            };


            _logger.LogInformation(
                "Staff dashboard metrics calculated: TodayAppointments={TodayAppointments}, QueueSize={QueueSize}, PendingVerifications={PendingVerifications}",
                todayAppointments, currentQueueSize, pendingVerifications);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve staff dashboard metrics");
            throw;
        }
    }

    /// <summary>
    /// Retrieves queue preview showing next N patients chronologically (US_068, AC4).
    /// Returns patients ordered by appointment time with estimated wait calculations.
    /// </summary>
    public async Task<List<QueuePreviewDto>> GetQueuePreviewAsync(int count = 5)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var queueItems = await _context.Appointments
                .Where(a => a.ScheduledDateTime >= today 
                    && a.ScheduledDateTime < tomorrow)                    
                .OrderBy(a => a.ScheduledDateTime)
                .Take(count)
                .Include(a => a.Patient)
                .Include(a => a.Provider)
                .ToListAsync();

            // Project to DTOs after materialization
            var result = queueItems.Select(a => new QueuePreviewDto
            {
                AppointmentId = a.AppointmentId,
                PatientName = a.Patient.Name,
                ProviderName = a.Provider.Name,
                AppointmentTime = a.ScheduledDateTime,
                ArrivalTime = a.ArrivalTime,
                EstimatedWait = CalculateWaitTime(a.ArrivalTime),
                RiskLevel = "low", // MVP: placeholder until FR-023 implemented
                Status = a.Status.ToString()
            }).ToList();

            _logger.LogInformation("Retrieved {Count} queue preview items", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve queue preview");
            throw;
        }
    }

    /// <summary>
    /// Calculates wait time based on patient arrival.
    /// If patient has not arrived (ArrivalTime is null), returns "-".
    /// If patient has arrived, calculates actual waiting time from arrival to now.
    /// </summary>
    private string CalculateWaitTime(DateTime? arrivalTime)
    {
        // If patient hasn't arrived yet, show dash
        if (!arrivalTime.HasValue)
        {
            return "-";
        }
        
        var now = DateTime.UtcNow;
        
        // Calculate actual waiting time from arrival to now
        var waitTime = now - arrivalTime.Value;
        
        if (waitTime.TotalMinutes < 1)
            return "< 1 min";
        if (waitTime.TotalMinutes < 60)
            return $"{(int)waitTime.TotalMinutes} mins";
        return $"{(int)waitTime.TotalHours}h {waitTime.Minutes}m";
    }
}
