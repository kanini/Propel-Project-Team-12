# Task - task_002_be_waitlist_notification_service

## Requirement Reference

- User Story: us_041
- Story Location: .propel/context/tasks/EP-005/us_041/us_041.md
- Acceptance Criteria:
  - AC-1: System sends notification via preferred channel (SMS/Email) with slot details and confirm/decline options when preferred slot becomes available
  - AC-2: Confirm books patient into available slot, removes from waitlist, sends booking confirmation
  - AC-3: Decline keeps patient on waitlist and offers slot to next eligible patient
  - AC-4: Timeout (e.g., 30 minutes) treated as decline, offers slot to next patient
- Edge Cases:
  - Multiple patients on same slot: First patient by priority timestamp notified; if decline/timeout, next patient notified sequentially (EC-1)
  - Slot re-booked before notification delivered: Availability check on confirm — if gone, patient notified and kept on waitlist (EC-2)

## Design References (Frontend Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Status** | N/A |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | N/A |
| **Screen Spec** | N/A |
| **UXR Requirements** | N/A |
| **Design Tokens** | N/A |

## Applicable Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| Backend | .NET 8 ASP.NET Core Web API | .NET 8.0 |
| Background Jobs | Hangfire | 1.8.x |
| Database | PostgreSQL with pgvector | PostgreSQL 16, pgvector 0.5+ |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

## Mobile References (Mobile Tasks Only)

| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview

Implement the core waitlist notification service that detects slot availability for waitlisted patients, sends notifications via preferred channel (SMS/Email/Both), and processes confirm/decline/timeout responses. Creates `IWaitlistNotificationService` and `WaitlistNotificationService` with methods for slot detection, patient notification with secure confirm/decline tokens, confirmation booking, decline cascading to next patient, and timeout processing. Integrates with existing `IEmailService` (adding waitlist notification method) and `ISmsService` (from US_037/task_002) for multi-channel delivery. Uses `Notification` entity for delivery tracking and `WaitlistEntry` notification fields (from task_001) for lifecycle management.

## Dependent Tasks

- EP-005/us_041/task_001_db_waitlist_notification_schema — Provides NotifiedAt, ResponseToken, ResponseDeadline, NotifiedSlotId fields on WaitlistEntry
- EP-005/us_037/task_002_be_sms_email_reminder_services — Provides ISmsService for SMS delivery

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/Interfaces/IWaitlistNotificationService.cs` — Interface for waitlist notification lifecycle
- **NEW** `src/backend/PatientAccess.Business/Services/WaitlistNotificationService.cs` — Implementation with detect, notify, confirm, decline, timeout methods
- **MODIFY** `src/backend/PatientAccess.Business/Services/IEmailService.cs` — Add SendWaitlistSlotNotificationAsync method
- **MODIFY** `src/backend/PatientAccess.Business/Services/EmailService.cs` — Implement SendWaitlistSlotNotificationAsync
- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Register IWaitlistNotificationService DI

## Implementation Plan

1. **Define IWaitlistNotificationService interface**:
   ```csharp
   public interface IWaitlistNotificationService
   {
       /// <summary>
       /// Detects unbooked time slots matching active waitlist entries (AC-1).
       /// Returns list of (WaitlistEntryId, TimeSlotId) pairs to notify.
       /// </summary>
       Task<List<(Guid WaitlistEntryId, Guid TimeSlotId)>> DetectAvailableSlotsAsync();

       /// <summary>
       /// Notifies the highest-priority active waitlist patient for a specific slot (AC-1, EC-1).
       /// Generates ResponseToken, sets NotifiedAt/ResponseDeadline, sends SMS/Email.
       /// </summary>
       Task<bool> NotifyNextPatientAsync(Guid timeSlotId);

       /// <summary>
       /// Processes a confirm response from the patient (AC-2).
       /// Validates token, checks slot availability (EC-2), books appointment, sets Fulfilled.
       /// </summary>
       Task<ConfirmWaitlistResponseDto> ProcessConfirmAsync(string responseToken);

       /// <summary>
       /// Processes a decline response from the patient (AC-3).
       /// Resets entry to Active, notifies next eligible patient (EC-1).
       /// </summary>
       Task<bool> ProcessDeclineAsync(string responseToken);

       /// <summary>
       /// Finds and processes expired notifications (AC-4).
       /// Treats timed-out entries as declines, cascades to next patient.
       /// </summary>
       Task<int> ProcessTimeoutsAsync();
   }
   ```

2. **Implement DetectAvailableSlotsAsync**:
   ```csharp
   public async Task<List<(Guid, Guid)>> DetectAvailableSlotsAsync()
   {
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
       return matches
           .GroupBy(m => m.TimeSlotId)
           .Select(g => (g.First().WaitlistEntryId, g.Key))
           .ToList();
   }
   ```
   - Matches Active waitlist entries with unbooked slots by provider and date range
   - Only considers future slots (not past)
   - Groups by TimeSlotId to avoid duplicate notifications for same slot

3. **Implement NotifyNextPatientAsync (AC-1, EC-1)**:
   ```csharp
   public async Task<bool> NotifyNextPatientAsync(Guid timeSlotId)
   {
       // Find highest-priority Active waitlist entry for this slot's provider and date
       var slot = await _context.TimeSlots.FindAsync(timeSlotId);
       if (slot == null || slot.IsBooked) return false;

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

       if (entry == null) return false;

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

       // Create Notification record for tracking
       var provider = await _context.Providers.FindAsync(entry.ProviderId);

       // Send via preferred channel(s) (AC-1)
       var confirmUrl = $"{_configuration["AppSettings:BaseUrl"]}/api/waitlist/{responseToken}/confirm";
       var declineUrl = $"{_configuration["AppSettings:BaseUrl"]}/api/waitlist/{responseToken}/decline";

       if (entry.NotificationPreference is NotificationPreference.Email or NotificationPreference.Both)
       {
           await _emailService.SendWaitlistSlotNotificationAsync(
               entry.Patient.Email, entry.Patient.Name,
               provider?.Name ?? "", slot.StartTime,
               confirmUrl, declineUrl, timeoutMinutes);

           await CreateNotificationRecord(entry, ChannelType.Email);
       }

       if (entry.NotificationPreference is NotificationPreference.SMS or NotificationPreference.Both)
       {
           await _smsService.SendWaitlistSlotNotificationSmsAsync(
               entry.Patient.Phone, entry.Patient.Name,
               provider?.Name ?? "", slot.StartTime,
               confirmUrl, declineUrl, timeoutMinutes);

           await CreateNotificationRecord(entry, ChannelType.SMS);
       }

       await _context.SaveChangesAsync();
       return true;
   }
   ```
   - Selects patient by Priority (ascending) then CreatedAt (FIFO) for EC-1
   - Generates cryptographically random 32-byte token (URL-safe base64)
   - Sends to preferred channel(s): Email, SMS, or Both

4. **Implement ProcessConfirmAsync (AC-2, EC-2)**:
   ```csharp
   public async Task<ConfirmWaitlistResponseDto> ProcessConfirmAsync(string responseToken)
   {
       var entry = await _context.WaitlistEntries
           .Include(w => w.Patient)
           .Include(w => w.NotifiedSlot)
           .FirstOrDefaultAsync(w => w.ResponseToken == responseToken);

       if (entry == null)
           throw new KeyNotFoundException("Invalid or expired notification token");

       if (entry.Status != WaitlistStatus.Notified)
           throw new InvalidOperationException("This notification has already been responded to");

       if (entry.ResponseDeadline.HasValue && entry.ResponseDeadline < DateTime.UtcNow)
           throw new InvalidOperationException("This notification has expired");

       // EC-2: Real-time availability check — slot may have been re-booked
       var slot = await _context.TimeSlots.FindAsync(entry.NotifiedSlotId);
       if (slot == null || slot.IsBooked)
       {
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

       // Send booking confirmation via existing flow
       BackgroundJob.Enqueue<ConfirmationEmailJob>(
           job => job.GenerateAndSendConfirmationAsync(appointment.AppointmentId));

       return new ConfirmWaitlistResponseDto
       {
           Success = true,
           Message = "Appointment booked successfully!",
           AppointmentId = appointment.AppointmentId
       };
   }
   ```
   - Validates token exists, entry is in Notified state, not expired
   - EC-2: Checks slot availability before booking — if gone, patient stays on waitlist
   - Creates Appointment, marks slot as booked, sets entry to Fulfilled
   - Enqueues ConfirmationEmailJob for booking confirmation

5. **Implement ProcessDeclineAsync (AC-3, EC-1)**:
   ```csharp
   public async Task<bool> ProcessDeclineAsync(string responseToken)
   {
       var entry = await _context.WaitlistEntries
           .FirstOrDefaultAsync(w => w.ResponseToken == responseToken);

       if (entry == null)
           throw new KeyNotFoundException("Invalid or expired notification token");

       if (entry.Status != WaitlistStatus.Notified)
           throw new InvalidOperationException("This notification has already been responded to");

       var slotId = entry.NotifiedSlotId;

       // Reset entry to Active — patient stays on waitlist (AC-3)
       entry.Status = WaitlistStatus.Active;
       entry.NotifiedAt = null;
       entry.ResponseToken = null;
       entry.ResponseDeadline = null;
       entry.NotifiedSlotId = null;
       entry.UpdatedAt = DateTime.UtcNow;

       await _context.SaveChangesAsync();

       // Offer slot to next eligible patient (EC-1)
       if (slotId.HasValue)
       {
           BackgroundJob.Enqueue<WaitlistNotificationService>(
               svc => svc.NotifyNextPatientAsync(slotId.Value));
       }

       return true;
   }
   ```
   - Resets entry to Active status and clears notification fields
   - Enqueues Hangfire job to notify next patient for the same slot (EC-1 cascading)

6. **Implement ProcessTimeoutsAsync (AC-4)**:
   ```csharp
   public async Task<int> ProcessTimeoutsAsync()
   {
       var expiredEntries = await _context.WaitlistEntries
           .Where(w => w.Status == WaitlistStatus.Notified &&
                       w.ResponseDeadline.HasValue &&
                       w.ResponseDeadline < DateTime.UtcNow)
           .ToListAsync();

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

           // Offer to next patient (EC-1)
           if (slotId.HasValue)
           {
               BackgroundJob.Enqueue<WaitlistNotificationService>(
                   svc => svc.NotifyNextPatientAsync(slotId.Value));
           }

           processedCount++;
       }

       return processedCount;
   }
   ```
   - Uses the partial index on (Status, ResponseDeadline) from task_001
   - Each expired entry is treated as decline: reset to Active, cascade to next patient

7. **Add SendWaitlistSlotNotificationAsync to IEmailService**:
   ```csharp
   /// <summary>
   /// Sends waitlist slot availability notification with confirm/decline links (FR-026, AC-1).
   /// </summary>
   Task<bool> SendWaitlistSlotNotificationAsync(
       string toEmail, string toName,
       string providerName, DateTime slotDateTime,
       string confirmUrl, string declineUrl, int timeoutMinutes);
   ```
   - New method on existing IEmailService interface
   - EmailService implementation sends via SendGrid with slot details and action links

8. **Create ConfirmWaitlistResponseDto**:
   ```csharp
   public class ConfirmWaitlistResponseDto
   {
       public bool Success { get; set; }
       public string Message { get; set; } = string.Empty;
       public Guid? AppointmentId { get; set; }
   }
   ```

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Interfaces/
│   │   ├── IWaitlistService.cs          # EXISTS — Join, Get, Update, Delete waitlist
│   │   ├── ISlotSwapService.cs          # EXISTS — Automatic swap (US_026, different from US_041)
│   │   └── (no IWaitlistNotificationService)
│   ├── Services/
│   │   ├── WaitlistService.cs           # EXISTS — US_025 waitlist CRUD
│   │   ├── SlotSwapService.cs           # EXISTS — US_026 automatic swap
│   │   ├── IEmailService.cs             # EXISTS — SendVerificationEmailAsync, SendAppointmentConfirmationAsync
│   │   ├── EmailService.cs              # EXISTS — SendGrid implementation
│   │   └── AppointmentService.cs        # EXISTS — CreateAsync, CancelAsync, RescheduleAsync
│   ├── BackgroundJobs/
│   │   ├── SlotAvailabilityMonitor.cs   # EXISTS — calls SlotSwapService.ProcessPendingSwapsAsync
│   │   └── ConfirmationEmailJob.cs      # EXISTS — Hangfire job for confirmation emails
│   └── DTOs/
│       └── WaitlistEntryDto.cs          # EXISTS — response DTO for waitlist entries
├── PatientAccess.Data/
│   └── Models/
│       ├── WaitlistEntry.cs             # MODIFIED by task_001 — has NotifiedAt, ResponseToken, etc.
│       ├── WaitlistStatus.cs            # EXISTS — Active=1, Notified=2, Fulfilled=3, Cancelled=4
│       ├── NotificationPreference.cs    # EXISTS — Email=1, SMS=2, Both=3
│       ├── Notification.cs              # EXISTS — delivery tracking entity
│       └── ChannelType.cs               # EXISTS — SMS=1, Email=2
├── PatientAccess.Web/
│   ├── Program.cs                       # EXISTS — DI registrations
│   └── Controllers/
│       └── WaitlistController.cs        # EXISTS — CRUD endpoints for US_025
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Interfaces/IWaitlistNotificationService.cs | Interface with Detect, Notify, Confirm, Decline, Timeout methods |
| CREATE | src/backend/PatientAccess.Business/Services/WaitlistNotificationService.cs | Full implementation with slot matching, token generation, multi-channel delivery, booking, cascading |
| CREATE | src/backend/PatientAccess.Business/DTOs/ConfirmWaitlistResponseDto.cs | Response DTO for confirm action (success/message/appointmentId) |
| MODIFY | src/backend/PatientAccess.Business/Services/IEmailService.cs | Add SendWaitlistSlotNotificationAsync method signature |
| MODIFY | src/backend/PatientAccess.Business/Services/EmailService.cs | Implement SendWaitlistSlotNotificationAsync via SendGrid |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register IWaitlistNotificationService DI |

## External References

- .NET RandomNumberGenerator: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.randomnumbergenerator
- Hangfire BackgroundJob.Enqueue: https://docs.hangfire.io/en/latest/background-methods/calling-methods-in-background.html
- SendGrid Dynamic Templates: https://docs.sendgrid.com/ui/sending-email/how-to-send-an-email-with-dynamic-transactional-templates

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.sln
```

## Implementation Validation Strategy

- [ ] IWaitlistNotificationService compiles with all 5 methods
- [ ] DetectAvailableSlotsAsync matches Active entries with unbooked slots by provider and date range
- [ ] NotifyNextPatientAsync selects by Priority then CreatedAt (FIFO per EC-1)
- [ ] ResponseToken generated with cryptographic random 32 bytes
- [ ] Notification sent via patient's preferred channel (Email/SMS/Both per AC-1)
- [ ] ProcessConfirmAsync checks slot availability before booking (EC-2)
- [ ] ProcessDeclineAsync resets entry to Active and cascades to next patient (EC-1)
- [ ] ProcessTimeoutsAsync finds expired Notified entries and treats as decline (AC-4)

## Implementation Checklist

- [ ] Create `IWaitlistNotificationService` interface with DetectAvailableSlotsAsync, NotifyNextPatientAsync, ProcessConfirmAsync, ProcessDeclineAsync, ProcessTimeoutsAsync
- [ ] Implement `WaitlistNotificationService` constructor with DbContext, IEmailService, ISmsService, IConfiguration, ILogger injections
- [ ] Implement slot detection logic matching Active waitlist entries with unbooked future slots by provider and date range
- [ ] Implement NotifyNextPatientAsync with priority-based selection, secure token generation, multi-channel delivery, and Notification record creation
- [ ] Implement ProcessConfirmAsync with token validation, real-time slot availability check (EC-2), appointment booking, and ConfirmationEmailJob enqueue
- [ ] Implement ProcessDeclineAsync with entry reset and cascading notification to next patient via Hangfire
- [ ] Implement ProcessTimeoutsAsync scanning expired Notified entries using partial index
- [ ] Add SendWaitlistSlotNotificationAsync to IEmailService and implement in EmailService with SendGrid
