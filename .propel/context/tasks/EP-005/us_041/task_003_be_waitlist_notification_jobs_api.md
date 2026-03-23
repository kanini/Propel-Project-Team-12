# Task - task_003_be_waitlist_notification_jobs_api

## Requirement Reference

- User Story: us_041
- Story Location: .propel/context/tasks/EP-005/us_041/us_041.md
- Acceptance Criteria:
  - AC-1: Notification sent when preferred slot becomes available (background job detects availability)
  - AC-2: Confirm books patient into slot (API endpoint)
  - AC-3: Decline keeps on waitlist, offers to next (API endpoint)
  - AC-4: Timeout treated as decline after configured period (background job)
- Edge Cases:
  - Multiple patients on same slot: Sequential notification by priority timestamp (EC-1)
  - Slot re-booked before delivery: Availability check on confirm (EC-2)

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

Create Hangfire background jobs for automated waitlist slot detection and timeout processing, plus REST API endpoints for patient confirm/decline responses. The `WaitlistSlotDetectionJob` runs every 2 minutes to detect newly available slots and trigger notifications. The `WaitlistTimeoutJob` runs every minute to expire unanswered notifications (AC-4). Two public API endpoints (`POST /api/waitlist/{token}/confirm` and `POST /api/waitlist/{token}/decline`) handle patient responses from SMS/Email links — these are `[AllowAnonymous]` since patients click links outside the authenticated app. Integrates with `IWaitlistNotificationService` (from task_002) and existing `AppointmentService` cancellation flow to detect newly freed slots.

## Dependent Tasks

- EP-005/us_041/task_002_be_waitlist_notification_service — Provides IWaitlistNotificationService with Detect, Notify, Confirm, Decline, Timeout methods

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/BackgroundJobs/WaitlistSlotDetectionJob.cs` — Hangfire recurring job for slot availability detection
- **NEW** `src/backend/PatientAccess.Business/BackgroundJobs/WaitlistTimeoutJob.cs` — Hangfire recurring job for timeout processing
- **MODIFY** `src/backend/PatientAccess.Web/Controllers/WaitlistController.cs` — Add confirm/decline endpoints with ResponseToken route parameter
- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Register background jobs as Hangfire recurring jobs
- **MODIFY** `src/backend/PatientAccess.Business/Services/AppointmentService.cs` — Trigger waitlist notification check after cancellation frees a slot

## Implementation Plan

1. **Create WaitlistSlotDetectionJob**:
   ```csharp
   public class WaitlistSlotDetectionJob
   {
       private readonly IWaitlistNotificationService _notificationService;
       private readonly ILogger<WaitlistSlotDetectionJob> _logger;

       public WaitlistSlotDetectionJob(
           IWaitlistNotificationService notificationService,
           ILogger<WaitlistSlotDetectionJob> logger)
       {
           _notificationService = notificationService;
           _logger = logger;
       }

       /// <summary>
       /// Detects available slots matching waitlist entries and notifies patients.
       /// Scheduled as Hangfire recurring job every 2 minutes.
       /// </summary>
       public async Task RunAsync()
       {
           try
           {
               _logger.LogInformation("WaitlistSlotDetectionJob started");

               var matches = await _notificationService.DetectAvailableSlotsAsync();

               _logger.LogInformation(
                   "Found {Count} waitlist matches for available slots", matches.Count);

               foreach (var (_, timeSlotId) in matches)
               {
                   // Notify highest-priority patient for each available slot
                   await _notificationService.NotifyNextPatientAsync(timeSlotId);
               }

               _logger.LogInformation("WaitlistSlotDetectionJob completed");
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "WaitlistSlotDetectionJob failed");
               // Don't throw — background job should not crash the app
           }
       }
   }
   ```
   - Follows `SlotAvailabilityMonitor` pattern (existing background job)
   - Runs every 2 minutes to balance responsiveness with DB load
   - Error-safe: logs and swallows exceptions

2. **Create WaitlistTimeoutJob**:
   ```csharp
   public class WaitlistTimeoutJob
   {
       private readonly IWaitlistNotificationService _notificationService;
       private readonly ILogger<WaitlistTimeoutJob> _logger;

       public WaitlistTimeoutJob(
           IWaitlistNotificationService notificationService,
           ILogger<WaitlistTimeoutJob> logger)
       {
           _notificationService = notificationService;
           _logger = logger;
       }

       /// <summary>
       /// Processes expired waitlist notifications (AC-4).
       /// Scheduled as Hangfire recurring job every 1 minute.
       /// </summary>
       public async Task RunAsync()
       {
           try
           {
               _logger.LogInformation("WaitlistTimeoutJob started");

               var expiredCount = await _notificationService.ProcessTimeoutsAsync();

               if (expiredCount > 0)
               {
                   _logger.LogInformation(
                       "Processed {Count} expired waitlist notifications", expiredCount);
               }
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "WaitlistTimeoutJob failed");
           }
       }
   }
   ```
   - Runs every 1 minute for timely timeout processing (AC-4: 30-minute default)
   - Granular logging for expired notification count

3. **Register Hangfire recurring jobs in Program.cs**:
   ```csharp
   // After app.MapControllers()
   app.UseHangfireDashboard();

   // Register recurring jobs
   RecurringJob.AddOrUpdate<WaitlistSlotDetectionJob>(
       "waitlist-slot-detection",
       job => job.RunAsync(),
       "*/2 * * * *"); // Every 2 minutes

   RecurringJob.AddOrUpdate<WaitlistTimeoutJob>(
       "waitlist-timeout-processing",
       job => job.RunAsync(),
       "* * * * *"); // Every 1 minute

   // Register DI for background jobs
   builder.Services.AddScoped<WaitlistSlotDetectionJob>();
   builder.Services.AddScoped<WaitlistTimeoutJob>();
   ```

4. **Add confirm/decline API endpoints to WaitlistController**:
   ```csharp
   /// <summary>
   /// Confirms waitlist slot offer — books appointment (AC-2).
   /// AllowAnonymous because patient clicks link from email/SMS.
   /// Token-based authentication via ResponseToken.
   /// </summary>
   [HttpPost("{token}/confirm")]
   [AllowAnonymous]
   [ProducesResponseType(typeof(ConfirmWaitlistResponseDto), StatusCodes.Status200OK)]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   [ProducesResponseType(StatusCodes.Status409Conflict)]
   [ProducesResponseType(StatusCodes.Status410Gone)]
   public async Task<IActionResult> ConfirmSlot([FromRoute] string token)
   {
       try
       {
           var result = await _notificationService.ProcessConfirmAsync(token);

           if (!result.Success)
           {
               // EC-2: Slot no longer available
               return Conflict(new { message = result.Message });
           }

           return Ok(result);
       }
       catch (KeyNotFoundException)
       {
           return NotFound(new { message = "Invalid or expired notification token" });
       }
       catch (InvalidOperationException ex)
       {
           return Gone(new { message = ex.Message });
       }
   }

   /// <summary>
   /// Declines waitlist slot offer — stays on waitlist, next patient notified (AC-3).
   /// AllowAnonymous for same reason as confirm.
   /// </summary>
   [HttpPost("{token}/decline")]
   [AllowAnonymous]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   public async Task<IActionResult> DeclineSlot([FromRoute] string token)
   {
       try
       {
           await _notificationService.ProcessDeclineAsync(token);
           return Ok(new { message = "You remain on the waitlist. We'll notify you when another slot opens." });
       }
       catch (KeyNotFoundException)
       {
           return NotFound(new { message = "Invalid or expired notification token" });
       }
   }
   ```
   - `[AllowAnonymous]` because patients click links from SMS/Email notifications
   - Token-based authentication: ResponseToken is unique and cryptographically random
   - Confirm returns 409 Conflict when slot was re-booked (EC-2)
   - Both endpoints use human-readable response messages

5. **Inject IWaitlistNotificationService into WaitlistController**:
   ```csharp
   public class WaitlistController : ControllerBase
   {
       private readonly IWaitlistService _waitlistService;
       private readonly IWaitlistNotificationService _notificationService;
       private readonly ILogger<WaitlistController> _logger;

       public WaitlistController(
           IWaitlistService waitlistService,
           IWaitlistNotificationService notificationService,
           ILogger<WaitlistController> logger)
       {
           _waitlistService = waitlistService;
           _notificationService = notificationService;
           _logger = logger;
       }
   }
   ```

6. **Integrate with AppointmentService.CancelAsync**:
   ```csharp
   // After slot is released in CancelAsync:
   // timeSlot.IsBooked = false;
   // await _context.SaveChangesAsync();

   // Trigger waitlist notification check for the freed slot
   BackgroundJob.Enqueue<WaitlistSlotDetectionJob>(job => job.RunAsync());
   ```
   - When a cancellation frees a slot, immediately trigger detection instead of waiting for next 2-minute cycle
   - Uses Hangfire enqueue for async processing (doesn't block cancellation response)

7. **Add WaitlistSettings configuration section**:
   ```json
   "WaitlistSettings": {
       "ResponseTimeoutMinutes": 30,
       "DetectionIntervalCron": "*/2 * * * *",
       "TimeoutCheckIntervalCron": "* * * * *"
   }
   ```
   - Configurable timeout (default 30 minutes per AC-4)
   - Configurable job intervals for operational tuning

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Interfaces/
│   │   └── IWaitlistNotificationService.cs  # FROM task_002 — Detect, Notify, Confirm, Decline, Timeout
│   ├── Services/
│   │   └── WaitlistNotificationService.cs   # FROM task_002 — full implementation
│   ├── BackgroundJobs/
│   │   ├── SlotAvailabilityMonitor.cs       # EXISTS — pattern reference for job structure
│   │   ├── ConfirmationEmailJob.cs          # EXISTS — pattern reference for Hangfire job
│   │   └── (no WaitlistSlotDetectionJob or WaitlistTimeoutJob)
│   └── DTOs/
│       └── ConfirmWaitlistResponseDto.cs    # FROM task_002
├── PatientAccess.Web/
│   ├── Controllers/
│   │   └── WaitlistController.cs            # EXISTS — CRUD endpoints, needs confirm/decline
│   └── Program.cs                           # EXISTS — DI registrations, Hangfire config
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/WaitlistSlotDetectionJob.cs | Recurring Hangfire job for slot availability detection (every 2 min) |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/WaitlistTimeoutJob.cs | Recurring Hangfire job for expired notification processing (every 1 min) |
| MODIFY | src/backend/PatientAccess.Web/Controllers/WaitlistController.cs | Add POST {token}/confirm and POST {token}/decline endpoints (AllowAnonymous) |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register WaitlistSlotDetectionJob, WaitlistTimeoutJob as Hangfire recurring jobs + DI |
| MODIFY | src/backend/PatientAccess.Business/Services/AppointmentService.cs | Enqueue WaitlistSlotDetectionJob after cancellation frees a slot |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add WaitlistSettings configuration section |

## External References

- Hangfire Recurring Jobs: https://docs.hangfire.io/en/latest/background-methods/performing-recurrent-tasks.html
- Hangfire Cron Expressions: https://docs.hangfire.io/en/latest/background-methods/performing-recurrent-tasks.html#cron-expressions
- ASP.NET Core AllowAnonymous: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple#use-the-allowanonymous-attribute

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.sln
```

## Implementation Validation Strategy

- [ ] WaitlistSlotDetectionJob runs as Hangfire recurring job every 2 minutes
- [ ] WaitlistTimeoutJob runs as Hangfire recurring job every 1 minute
- [ ] POST /api/waitlist/{token}/confirm endpoint books appointment when slot available (AC-2)
- [ ] POST /api/waitlist/{token}/confirm returns 409 when slot re-booked (EC-2)
- [ ] POST /api/waitlist/{token}/decline resets entry to Active and cascades (AC-3)
- [ ] Both endpoints are [AllowAnonymous] for SMS/Email link access
- [ ] AppointmentService.CancelAsync triggers immediate waitlist detection on slot release

## Implementation Checklist

- [ ] Create `WaitlistSlotDetectionJob` with RunAsync calling DetectAvailableSlotsAsync + NotifyNextPatientAsync
- [ ] Create `WaitlistTimeoutJob` with RunAsync calling ProcessTimeoutsAsync
- [ ] Register both jobs as Hangfire recurring jobs in Program.cs with configurable cron intervals
- [ ] Add `POST {token}/confirm` endpoint to WaitlistController with [AllowAnonymous] and token validation
- [ ] Add `POST {token}/decline` endpoint to WaitlistController with [AllowAnonymous] and token validation
- [ ] Inject `IWaitlistNotificationService` into WaitlistController constructor
- [ ] Add `BackgroundJob.Enqueue<WaitlistSlotDetectionJob>` in AppointmentService.CancelAsync after slot release
