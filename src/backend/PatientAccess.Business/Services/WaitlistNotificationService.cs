using System.Security.Cryptography;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PatientAccess.Business.BackgroundJobs;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.Services;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Waitlist notification service implementation for US_041 - Waitlist Slot Availability Notifications.
/// Manages the complete lifecycle: detect available slots, notify patients, process responses (confirm/decline), handle timeouts.
/// </summary>
public class WaitlistNotificationService : IWaitlistNotificationService
{
    private readonly PatientAccessDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WaitlistNotificationService> _logger;

    public WaitlistNotificationService(
        PatientAccessDbContext context,
        IEmailService emailService,
        ISmsService smsService,
        IConfiguration configuration,
        ILogger<WaitlistNotificationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Detects unbooked time slots matching active waitlist entries (AC-1).
    /// Returns list of (WaitlistEntryId, TimeSlotId) pairs to notify.
    /// </summary>
    public async Task<List<(Guid WaitlistEntryId, Guid TimeSlotId)>> DetectAvailableSlotsAsync()
    {
        try
        {
            _logger.LogDebug("Starting slot availability detection for waitlist");

            // Find unbooked slots that match active waitlist entries
            var matches = await _context.WaitlistEntries
                .Where(w => w.Status == WaitlistStatus.Active)
                .Join(
                    _context.TimeSlots.Where(ts => !ts.IsBooked),
                    w => w.ProviderId,
                    ts => ts.ProviderId,
                    (w, ts) => new { WaitlistEntry = w, TimeSlot = ts })
                .Where(x =>
                    DateOnly.FromDateTime(x.TimeSlot.StartTime) >= x.WaitlistEntry.PreferredDateStart &&
                    DateOnly.FromDateTime(x.TimeSlot.StartTime) <= x.WaitlistEntry.PreferredDateEnd &&
                    x.TimeSlot.StartTime > DateTime.UtcNow) // Only future slots
                .Select(x => new { x.WaitlistEntry.WaitlistEntryId, x.TimeSlot.TimeSlotId })
                .Distinct()
                .ToListAsync();

            // Deduplicate: one notification per slot (highest priority patient handles it)
            var deduplicatedMatches = matches
                .GroupBy(m => m.TimeSlotId)
                .Select(g => (g.First().WaitlistEntryId, g.Key))
                .ToList();

            _logger.LogInformation(
                "Detected {Count} available slots matching waitlist entries",
                deduplicatedMatches.Count);

            return deduplicatedMatches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting available slots for waitlist");
            throw;
        }
    }

    /// <summary>
    /// Notifies the highest-priority active waitlist patient for a specific slot (AC-1, EC-1).
    /// </summary>
    public async Task<bool> NotifyNextPatientAsync(Guid timeSlotId)
    {
        try
        {
            _logger.LogDebug("Notifying next patient for TimeSlot {TimeSlotId}", timeSlotId);

            // Find highest-priority Active waitlist entry for this slot's provider and date
            var slot = await _context.TimeSlots.FindAsync(timeSlotId);
            if (slot == null || slot.IsBooked)
            {
                _logger.LogWarning("TimeSlot {TimeSlotId} not found or already booked", timeSlotId);
                return false;
            }

            var slotDate = DateOnly.FromDateTime(slot.StartTime);
            var entry = await _context.WaitlistEntries
                .Include(w => w.Patient)
                .Where(w => w.Status == WaitlistStatus.Active &&
                            w.ProviderId == slot.ProviderId &&
                            slotDate >= w.PreferredDateStart &&
                            slotDate <= w.PreferredDateEnd)
                .OrderBy(w => w.Priority) // Lower = higher priority
                .ThenBy(w => w.CreatedAt) // FIFO for same priority (EC-1)
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                _logger.LogDebug("No eligible waitlist patient found for TimeSlot {TimeSlotId}", timeSlotId);
                return false;
            }

            // Generate secure response token
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var responseToken = Base64UrlEncoder.Encode(tokenBytes);

            // Read configurable timeout (default 30 minutes)
            var timeoutMinutes = _configuration.GetValue<int>("WaitlistSettings:ResponseTimeoutMinutes", 30);

            // Update waitlist entry
            entry.Status = WaitlistStatus.Notified;
            entry.NotifiedAt = DateTime.UtcNow;
            entry.ResponseToken = responseToken;
            entry.ResponseDeadline = DateTime.UtcNow.AddMinutes(timeoutMinutes);
            entry.NotifiedSlotId = timeSlotId;
            entry.UpdatedAt = DateTime.UtcNow;

            // Get provider info
            var provider = await _context.Providers.FindAsync(entry.ProviderId);

            // Build confirm/decline URLs
            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
            var confirmUrl = $"{baseUrl}/api/waitlist/{responseToken}/confirm";
            var declineUrl = $"{baseUrl}/api/waitlist/{responseToken}/decline";

            // Send via preferred channel(s) (AC-1)
            bool emailSent = false;
            bool smsSent = false;

            if (entry.NotificationPreference is NotificationPreference.Email or NotificationPreference.Both)
            {
                emailSent = await _emailService.SendWaitlistSlotNotificationAsync(
                    entry.Patient.Email,
                    entry.Patient.Name,
                    provider?.Name ?? "Unknown Provider",
                    slot.StartTime,
                    confirmUrl,
                    declineUrl,
                    timeoutMinutes);

                if (emailSent)
                {
                    await CreateNotificationRecordAsync(entry, ChannelType.Email);
                }
            }

            if (entry.NotificationPreference is NotificationPreference.SMS or NotificationPreference.Both)
            {
                smsSent = await _smsService.SendWaitlistSlotNotificationSmsAsync(
                    entry.Patient.Phone ?? string.Empty,
                    entry.Patient.Name,
                    provider?.Name ?? "Unknown Provider",
                    slot.StartTime,
                    confirmUrl,
                    declineUrl,
                    timeoutMinutes);

                if (smsSent)
                {
                    await CreateNotificationRecordAsync(entry, ChannelType.SMS);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Notified patient {PatientId} for TimeSlot {TimeSlotId} (Email: {EmailSent}, SMS: {SmsSent})",
                entry.PatientId, timeSlotId, emailSent, smsSent);

            return emailSent || smsSent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying patient for TimeSlot {TimeSlotId}", timeSlotId);
            throw;
        }
    }

    /// <summary>
    /// Processes a confirm response from the patient (AC-2, EC-2).
    /// </summary>
    public async Task<ConfirmWaitlistResponseDto> ProcessConfirmAsync(string responseToken)
    {
        try
        {
            _logger.LogInformation("Processing confirm for token {Token}", responseToken);

            var entry = await _context.WaitlistEntries
                .Include(w => w.Patient)
                .Include(w => w.NotifiedSlot)
                .FirstOrDefaultAsync(w => w.ResponseToken == responseToken);

            if (entry == null)
            {
                _logger.LogWarning("Invalid or expired notification token: {Token}", responseToken);
                throw new KeyNotFoundException("Invalid or expired notification token");
            }

            if (entry.Status != WaitlistStatus.Notified)
            {
                _logger.LogWarning("Notification already responded to for token {Token}", responseToken);
                throw new InvalidOperationException("This notification has already been responded to");
            }

            if (entry.ResponseDeadline.HasValue && entry.ResponseDeadline < DateTime.UtcNow)
            {
                _logger.LogWarning("Notification expired for token {Token}", responseToken);
                throw new InvalidOperationException("This notification has expired");
            }

            // EC-2: Real-time availability check — slot may have been re-booked
            var slot = await _context.TimeSlots.FindAsync(entry.NotifiedSlotId);
            if (slot == null || slot.IsBooked)
            {
                _logger.LogWarning(
                    "Slot {SlotId} no longer available for waitlist entry {EntryId}",
                    entry.NotifiedSlotId, entry.WaitlistEntryId);

                // Slot gone — reset entry to Active, keep on waitlist
                entry.Status = WaitlistStatus.Active;
                entry.NotifiedAt = null;
                entry.ResponseToken = null;
                entry.ResponseDeadline = null;
                entry.NotifiedSlotId = null;
                entry.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new ConfirmWaitlistResponseDto
                {
                    Success = false,
                    Message = "Sorry, this slot is no longer available. You remain on the waitlist."
                };
            }

            // Book the appointment (AC-2)
            slot.IsBooked = true;
            slot.UpdatedAt = DateTime.UtcNow;

            var appointment = new Appointment
            {
                AppointmentId = Guid.NewGuid(),
                PatientId = entry.PatientId,
                ProviderId = entry.ProviderId,
                TimeSlotId = slot.TimeSlotId,
                ScheduledDateTime = slot.StartTime,
                Status = AppointmentStatus.Scheduled,
                VisitReason = entry.Reason ?? "Waitlist booking",
                ConfirmationNumber = GenerateConfirmationNumber(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Appointments.Add(appointment);

            // Mark waitlist entry as fulfilled (AC-2)
            entry.Status = WaitlistStatus.Fulfilled;
            entry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send booking confirmation via existing flow
            BackgroundJob.Enqueue<ConfirmationEmailJob>(
                job => job.SendConfirmationAsync(appointment.AppointmentId));

            _logger.LogInformation(
                "Waitlist entry {EntryId} confirmed, appointment {AppointmentId} created",
                entry.WaitlistEntryId, appointment.AppointmentId);

            return new ConfirmWaitlistResponseDto
            {
                Success = true,
                Message = "Appointment booked successfully!",
                AppointmentId = appointment.AppointmentId
            };
        }
        catch (Exception ex) when (ex is not KeyNotFoundException and not InvalidOperationException)
        {
            _logger.LogError(ex, "Error processing confirm for token {Token}", responseToken);
            throw;
        }
    }

    /// <summary>
    /// Processes a decline response from the patient (AC-3, EC-1).
    /// </summary>
    public async Task<bool> ProcessDeclineAsync(string responseToken)
    {
        try
        {
            _logger.LogInformation("Processing decline for token {Token}", responseToken);

            var entry = await _context.WaitlistEntries
                .FirstOrDefaultAsync(w => w.ResponseToken == responseToken);

            if (entry == null)
            {
                _logger.LogWarning("Invalid notification token: {Token}", responseToken);
                throw new KeyNotFoundException("Invalid notification token");
            }

            if (entry.Status != WaitlistStatus.Notified)
            {
                _logger.LogDebug("Notification already processed for token {Token}", responseToken);
                return true; // Idempotent — already declined/confirmed
            }

            var notifiedSlotId = entry.NotifiedSlotId;

            // Reset entry to Active (AC-3)
            entry.Status = WaitlistStatus.Active;
            entry.NotifiedAt = null;
            entry.ResponseToken = null;
            entry.ResponseDeadline = null;
            entry.NotifiedSlotId = null;
            entry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Waitlist entry {EntryId} declined, reset to Active",
                entry.WaitlistEntryId);

            // Cascade to next patient (EC-1)
            if (notifiedSlotId.HasValue)
            {
                BackgroundJob.Enqueue<WaitlistSlotDetectionJob>(job => job.RunAsync());
            }

            return true;
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            _logger.LogError(ex, "Error processing decline for token {Token}", responseToken);
            throw;
        }
    }

    /// <summary>
    /// Finds and processes expired notifications (AC-4).
    /// </summary>
    public async Task<int> ProcessTimeoutsAsync()
    {
        try
        {
            _logger.LogDebug("Processing waitlist notification timeouts");

            // Find Notified entries past ResponseDeadline (partial index optimized)
            var expiredEntries = await _context.WaitlistEntries
                .Where(w => w.Status == WaitlistStatus.Notified &&
                            w.ResponseDeadline.HasValue &&
                            w.ResponseDeadline < DateTime.UtcNow)
                .ToListAsync();

            if (expiredEntries.Count == 0)
            {
                return 0;
            }

            _logger.LogInformation("Found {Count} expired waitlist notifications", expiredEntries.Count);

            var processedCount = 0;
            foreach (var entry in expiredEntries)
            {
                var notifiedSlotId = entry.NotifiedSlotId;

                // Treat as decline (AC-4)
                entry.Status = WaitlistStatus.Active;
                entry.NotifiedAt = null;
                entry.ResponseToken = null;
                entry.ResponseDeadline = null;
                entry.NotifiedSlotId = null;
                entry.UpdatedAt = DateTime.UtcNow;

                processedCount++;

                // Cascade to next patient (EC-1)
                if (notifiedSlotId.HasValue)
                {
                    BackgroundJob.Enqueue<WaitlistSlotDetectionJob>(job => job.RunAsync());
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Processed {Count} expired waitlist notifications", processedCount);

            return processedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing waitlist timeouts");
            throw;
        }
    }

    /// <summary>
    /// Creates a Notification record for delivery tracking.
    /// </summary>
    private Task CreateNotificationRecordAsync(WaitlistEntry entry, ChannelType channelType)
    {
        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            RecipientId = entry.PatientId,
            AppointmentId = null, // Waitlist notification, not tied to appointment yet
            ChannelType = channelType,
            TemplateName = "WaitlistSlotAvailable",
            Status = NotificationStatus.Sent,
            ScheduledTime = DateTime.UtcNow,
            SentTime = DateTime.UtcNow,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Generates a unique 8-character alphanumeric confirmation number.
    /// </summary>
    private static string GenerateConfirmationNumber()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }
}
