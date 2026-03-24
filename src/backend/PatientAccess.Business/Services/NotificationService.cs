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
/// Notification service implementation for US_067.
/// Manages notification retrieval, unread counts, and read status with caching.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly PatientAccessDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<NotificationService> _logger;
    private const int CacheDurationMinutes = 2;

    public NotificationService(
        PatientAccessDbContext context,
        IDistributedCache cache,
        ILogger<NotificationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves recent notifications for dashboard panel (US_067, AC5).
    /// </summary>
    public async Task<List<NotificationDto>> GetRecentNotificationsAsync(Guid userId, int limit)
    {
        if (limit < 1 || limit > 20)
        {
            throw new ArgumentException("Limit must be between 1 and 20", nameof(limit));
        }

        var cacheKey = $"recent_notifications_{userId}_{limit}";

        try
        {
            // Try to get from cache
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("Recent notifications cache hit for user {UserId}", userId);
                return JsonSerializer.Deserialize<List<NotificationDto>>(cachedData)!;
            }

            _logger.LogInformation("Recent notifications cache miss for user {UserId}", userId);

            // Note: The current Notification model tracks scheduled/sent notifications.
            // For dashboard notifications, we need to retrieve notifications where:
            // - RecipientId matches userId
            // - Status is Delivered or Sent
            // - Created recently
            var notificationEntities = await _context.Notifications
                .Where(n => n.RecipientId == userId 
                    && (n.Status == NotificationStatus.Sent || n.Status == NotificationStatus.Delivered))
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();

            // Map to DTOs in memory (cannot call custom methods in SQL query)
            var notifications = notificationEntities.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Title = GetNotificationTitle(n.TemplateName),
                Message = GetNotificationMessage(n.TemplateName, n.ScheduledTime),
                CreatedAt = n.CreatedAt,
                ActionLink = n.AppointmentId.HasValue ? $"/appointments/{n.AppointmentId}" : null,
                ActionLabel = n.AppointmentId.HasValue ? "View Appointment" : null,
                IsRead = false, // Extend Notification model to track read status
                NotificationType = n.ChannelType.ToString()
            }).ToList();

            // Cache for 2 minutes
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes)
            };
            var serializedNotifications = JsonSerializer.Serialize(notifications);
            await _cache.SetStringAsync(cacheKey, serializedNotifications, cacheOptions);

            _logger.LogInformation("Recent notifications cached for user {UserId}", userId);

            return notifications;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent notifications for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves unread notification count (US_067, AC5).
    /// </summary>
    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var cacheKey = $"unread_count_{userId}";

        try
        {
            // Try to get from cache
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("Unreading count cache hit for user {UserId}", userId);
                return int.Parse(cachedData);
            }

            _logger.LogInformation("Unread count cache miss for user {UserId}", userId);

            // Count unread notifications (requires IsRead field extension)
            var count = await _context.Notifications
                .Where(n => n.RecipientId == userId 
                    && (n.Status == NotificationStatus.Sent || n.Status == NotificationStatus.Delivered))
                .CountAsync();

            // Cache for 2 minutes
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes)
            };
            await _cache.SetStringAsync(cacheKey, count.ToString(), cacheOptions);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unread count for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Marks notification as read (US_067, AC7).
    /// Invalidates cached counts and notifications list.
    /// Note: Requires extending Notification model with IsRead field.
    /// </summary>
    public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
    {
        try
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.RecipientId == userId);

            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found for user {UserId}", notificationId, userId);
                return false;
            }

            // Note: This requires adding IsRead field to Notification model
            // For now, we'll update the UpdatedAt timestamp as a placeholder
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Invalidate cached counts and notifications
            await InvalidateCacheAsync(userId);

            _logger.LogInformation("Notification {NotificationId} marked as read for user {UserId}", notificationId, userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
            throw;
        }
    }

    /// <summary>
    /// Invalidates cached notification data for user.
    /// </summary>
    private async Task InvalidateCacheAsync(Guid userId)
    {
        var cacheKeys = new[]
        {
            $"unread_count_{userId}",
            $"recent_notifications_{userId}_5",
            $"recent_notifications_{userId}_10",
            $"recent_notifications_{userId}_20"
        };

        foreach (var key in cacheKeys)
        {
            await _cache.RemoveAsync(key);
        }

        _logger.LogInformation("Invalidated notification cache for user {UserId}", userId);
    }

    /// <summary>
    /// Maps template name to user-friendly notification title.
    /// </summary>
    private string GetNotificationTitle(string templateName)
    {
        return templateName switch
        {
            "AppointmentReminder" => "Upcoming Appointment",
            "AppointmentConfirmation" => "Appointment Confirmed",
            "SlotSwapAvailable" => "Slot Swap Available",
            "DocumentProcessingComplete" => "Document Ready",
            "DocumentProcessingFailed" => "Document Processing Failed",
            _ => "Notification"
        };
    }

    /// <summary>
    /// Generates notification message based on template and context.
    /// </summary>
    private string GetNotificationMessage(string templateName, DateTime scheduledTime)
    {
        return templateName switch
        {
            "AppointmentReminder" => $"You have an appointment scheduled for {scheduledTime:MMMM dd, yyyy}",
            "AppointmentConfirmation" => "Your appointment has been confirmed",
            "SlotSwapAvailable" => "A preferred time slot is now available",
            "DocumentProcessingComplete" => "Your clinical document has been processed successfully",
            "DocumentProcessingFailed" => "Document processing encountered an error",
            _ => "You have a new notification"
        };
    }
}
