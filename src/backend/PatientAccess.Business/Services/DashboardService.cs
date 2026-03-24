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
/// Dashboard service implementation for US_067.
/// Provides dashboard statistics with caching for performance optimization.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly PatientAccessDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<DashboardService> _logger;
    private const int CacheDurationMinutes = 5;

    public DashboardService(
        PatientAccessDbContext context,
        IDistributedCache cache,
        ILogger<DashboardService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves dashboard statistics for authenticated patient (US_067, AC1, AC2).
    /// Calculates appointment counts and trends with Redis caching.
    /// </summary>
    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId)
    {
        var cacheKey = $"dashboard_stats_{userId}";

        try
        {
            // Try to get from cache
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("Dashboard stats cache hit for user {UserId}", userId);
                return JsonSerializer.Deserialize<DashboardStatsDto>(cachedData)!;
            }

            _logger.LogInformation("Dashboard stats cache miss for user {UserId}, calculating stats", userId);

            // Calculate stats
            var now = DateTime.UtcNow;
            var sixMonthsAgo = now.AddMonths(-6);
            var twelveMonthsAgo = now.AddMonths(-12);
            var thirtyDaysFromNow = now.AddDays(30);

            // Current period counts
            var totalAppointmentsCurrent = await _context.Appointments
                .Where(a => a.PatientId == userId 
                    && a.ScheduledDateTime >= sixMonthsAgo 
                    && a.ScheduledDateTime <= now
                    && a.Status != AppointmentStatus.Cancelled)
                .CountAsync();

            var upcomingAppointmentsCurrent = await _context.Appointments
                .Where(a => a.PatientId == userId 
                    && a.ScheduledDateTime >= now 
                    && a.ScheduledDateTime <= thirtyDaysFromNow
                    && (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed))
                .CountAsync();

            var waitlistEntriesCurrent = await _context.WaitlistEntries
                .Where(w => w.PatientId == userId 
                    && w.Status == WaitlistStatus.Active)
                .CountAsync();

            // Document counts
            var totalDocuments = await _context.ClinicalDocuments
                .Where(d => d.PatientId == userId)
                .CountAsync();

            var completedDocuments = await _context.ClinicalDocuments
                .Where(d => d.PatientId == userId 
                    && d.ProcessingStatus == ProcessingStatus.Completed)
                .CountAsync();

            // Previous period counts for trend calculation
            var totalAppointmentsPrevious = await _context.Appointments
                .Where(a => a.PatientId == userId 
                    && a.ScheduledDateTime >= twelveMonthsAgo 
                    && a.ScheduledDateTime < sixMonthsAgo
                    && a.Status != AppointmentStatus.Cancelled)
                .CountAsync();

            var stats = new DashboardStatsDto
            {
                TotalAppointments = totalAppointmentsCurrent,
                UpcomingAppointments = upcomingAppointmentsCurrent,
                WaitlistEntries = waitlistEntriesCurrent,
                TotalDocuments = totalDocuments,
                CompletedDocuments = completedDocuments,
                Trends = new TrendsDto
                {
                    TotalAppointmentsTrend = CalculateTrendPercentage(totalAppointmentsCurrent, totalAppointmentsPrevious),
                    UpcomingAppointmentsTrend = 0, // Trend not applicable for future counts
                    WaitlistEntriesTrend = 0 // Trend requires historical tracking
                }
            };

            // Cache for 5 minutes
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes)
            };
            var serializedStats = JsonSerializer.Serialize(stats);
            await _cache.SetStringAsync(cacheKey, serializedStats, cacheOptions);

            _logger.LogInformation("Dashboard stats cached for user {UserId}", userId);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating dashboard stats for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Calculates trend percentage comparing current to previous period.
    /// </summary>
    private double CalculateTrendPercentage(int current, int previous)
    {
        if (previous == 0)
        {
            return current > 0 ? 100.0 : 0.0;
        }
        return Math.Round(((current - previous) / (double)previous) * 100.0, 1);
    }
}
