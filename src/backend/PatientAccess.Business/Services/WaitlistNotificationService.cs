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
/// Service for managing waitlist slot notification lifecycle (US_041/FR-026).
/// Detects available slots, notifies patients via preferred channel, processes confirm/decline/timeout responses.
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
        _context = context;
        _emailService = emailService;
        _smsService = smsService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<(Guid WaitlistEntryId, Guid TimeSlotId)>> DetectAvailableSlotsAsync()
    {
        _logger.LogInformation("Detecting available slots for active waitlist entries");

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
        var result = matches
            .GroupBy(m => m.TimeSlotId)
            .Select(g => (g.First().WaitlistEntryId, g.Key))
            .ToList();

        _logger.LogInformation("Found {Count} available slot matches", result.Count);
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> NotifyNextPatientAsync(Guid timeSlotId)
    {
        _logger.LogInformation("Attempting to notify next patient for slot {SlotId}", timeSlotId);

        // Find highest-priority Active waitlist entry for this slot's provider and date
        var slot = await _context.TimeSlots
            .Include(ts => ts.Provider)
            .FirstOrDefaultAsync(ts => ts.TimeSlotId == timeSlotId);

        if (slot == null || slot.IsBooked)
        {
            _logger.LogWarning("Slot {SlotId} not found or already booked", timeSlotId);
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
            _logger.LogWarning("No active waitlist entry found for slot {SlotId}", timeSlotId);
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

        // Build confirm/decline URLs
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
        var confirmUrl = $"{baseUrl}/api/waitlist/{responseToken}/confirm";
        var declineUrl = $"{baseUrl}/api/waitlist/{responseToken}/decline";

        var notificationsSent = false;

        // Send via preferred channel(s) (AC-1)
        if (entry.NotificationPreference is NotificationPreference.Email or NotificationPreference.Both)
        {
            try
            {
                await _emailService.SendWaitlistSlotNotificationAsync(
                    entry.Patient.Email, entry.Patient.Name,
                    slot.Provider.Name, slot.StartTime,
                    confirmUrl, declineUrl, timeoutMinutes);

                await CreateNotificationRecord(entry, ChannelType.Email);
                notificationsSent = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send waitlist email notification to {Email}", entry.Patient.Email);
            }
        }

        if (entry.NotificationPreference is NotificationPreference.SMS or NotificationPreference.Both)
        {
            if (!string.IsNullOrWhiteSpace(entry.Patient.Phone))
            {
                try
                {
                    // Build SMS message
                    var smsMessage = $"Hi {entry.Patient.Name}, a slot with {slot.Provider.Name} is available on {slot.StartTime:g}. " +
                                     $"Confirm: {confirmUrl} | Decline: {declineUrl}. Offer expires in {timeoutMinutes} minutes.";

                    await _smsService.SendSmsAsync(entry.Patient.Phone, smsMessage);
                    await CreateNotificationRecord(entry, ChannelType.SMS);
                    notificationsSent = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send waitlist SMS notification to {Phone}", entry.Patient.Phone);
                }
            }
        }

        if (!notificationsSent)
        {
            _logger.LogError("No notifications sent for waitlist entry {EntryId}", entry.WaitlistEntryId);
            // Rollback status change
            entry.Status = WaitlistStatus.Active;
            entry.NotifiedAt = null;
            entry.ResponseToken = null;
            entry.ResponseDeadline = null;
            entry.NotifiedSlotId = null;
            await _context.SaveChangesAsync();
            return false;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Notified patient {PatientId} for slot {SlotId}", entry.PatientId, timeSlotId);
        return true;
    }

    /// <inheritdoc />
    public async Task<ConfirmWaitlistResponseDto> ProcessConfirmAsync(string responseToken)
    {
        _logger.LogInformation("Processing confirm for token {Token}", responseToken);

        var entry = await _context.WaitlistEntries
            .Include(w => w.Patient)
            .Include(w => w.NotifiedSlot)
            .FirstOrDefaultAsync(w => w.ResponseToken == responseToken);

        if (entry == null)
        {
            _logger.LogWarning("Invalid response token: {Token}", responseToken);
            throw new KeyNotFoundException("Invalid or expired notification token");
        }

        if (entry.Status != WaitlistStatus.Notified)
        {
            _logger.LogWarning("Entry {EntryId} already responded (status: {Status})", entry.WaitlistEntryId, entry.Status);
            throw new InvalidOperationException("This notification has already been responded to");
        }

        if (entry.ResponseDeadline.HasValue && entry.ResponseDeadline < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired token used for entry {EntryId}", entry.WaitlistEntryId);
            throw new InvalidOperationException("This notification has expired");
        }

        // EC-2: Real-time availability check — slot may have been re-booked
        var slot = await _context.TimeSlots.FindAsync(entry.NotifiedSlotId);
        if (slot == null || slot.IsBooked)
        {
            _logger.LogWarning("Slot {SlotId} no longer available for entry {EntryId}", entry.NotifiedSlotId, entry.WaitlistEntryId);

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Appointments.Add(appointment);

        // Mark waitlist entry as fulfilled (AC-2)
        entry.Status = WaitlistStatus.Fulfilled;
        entry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Booked appointment {AppointmentId} for patient {PatientId}", appointment.AppointmentId, entry.PatientId);

        // Send booking confirmation via existing background job
        try
        {
            BackgroundJob.Enqueue<ConfirmationEmailJob>(
                job => job.SendConfirmationAsync(appointment.AppointmentId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue confirmation email job for appointment {AppointmentId}", appointment.AppointmentId);
        }

        return new ConfirmWaitlistResponseDto
        {
            Success = true,
            Message = "Appointment booked successfully!",
            AppointmentId = appointment.AppointmentId
        };
    }

    /// <inheritdoc />
    public async Task<bool> ProcessDeclineAsync(string responseToken)
    {
        _logger.LogInformation("Processing decline for token {Token}", responseToken);

        var entry = await _context.WaitlistEntries
            .FirstOrDefaultAsync(w => w.ResponseToken == responseToken);

        if (entry == null)
        {
            _logger.LogWarning("Invalid response token: {Token}", responseToken);
            throw new KeyNotFoundException("Invalid or expired notification token");
        }

        if (entry.Status != WaitlistStatus.Notified)
        {
            _logger.LogWarning("Entry {EntryId} already responded (status: {Status})", entry.WaitlistEntryId, entry.Status);
            throw new InvalidOperationException("This notification has already been responded to");
        }

        var slotId = entry.NotifiedSlotId;

        // Reset entry to Active — patient stays on waitlist (AC-3)
        entry.Status = WaitlistStatus.Active;
        entry.NotifiedAt = null;
        entry.ResponseToken = null;
        entry.ResponseDeadline = null;
        entry.NotifiedSlotId = null;
        entry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Patient {PatientId} declined slot {SlotId}", entry.PatientId, slotId);

        // Offer slot to next eligible patient (EC-1)
        if (slotId.HasValue)
        {
            try
            {
                BackgroundJob.Enqueue<IWaitlistNotificationService>(
                    svc => svc.NotifyNextPatientAsync(slotId.Value));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue notification for next patient on slot {SlotId}", slotId);
            }
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<int> ProcessTimeoutsAsync()
    {
        _logger.LogInformation("Processing expired waitlist notifications");

        var expiredEntries = await _context.WaitlistEntries
            .Where(w => w.Status == WaitlistStatus.Notified &&
                        w.ResponseDeadline.HasValue &&
                        w.ResponseDeadline < DateTime.UtcNow)
            .ToListAsync();

        _logger.LogInformation("Found {Count} expired notifications", expiredEntries.Count);

        var processedCount = 0;

        foreach (var entry in expiredEntries)
        {
            var slotId = entry.NotifiedSlotId;

            // Treat as decline (AC-4)
            entry.Status = WaitlistStatus.Active;
            entry.NotifiedAt = null;
            entry.ResponseToken = null;
            entry.ResponseDeadline = null;
            entry.NotifiedSlotId = null;
            entry.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Entry {EntryId} timed out, reset to Active", entry.WaitlistEntryId);

            // Offer to next patient (EC-1)
            if (slotId.HasValue)
            {
                try
                {
                    BackgroundJob.Enqueue<IWaitlistNotificationService>(
                        svc => svc.NotifyNextPatientAsync(slotId.Value));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to enqueue notification for next patient on slot {SlotId}", slotId);
                }
            }

            processedCount++;
        }

        _logger.LogInformation("Processed {Count} expired notifications", processedCount);
        return processedCount;
    }

    private async Task CreateNotificationRecord(WaitlistEntry entry, ChannelType channel)
    {
        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            AppointmentId = null, // No appointment yet
            RecipientId = entry.PatientId,
            ChannelType = channel,
            TemplateName = "WaitlistSlotAvailable",
            Status = NotificationStatus.Sent,
            ScheduledTime = DateTime.UtcNow,
            SentTime = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
    }
}
