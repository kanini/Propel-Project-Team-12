# Task - task_003_be_reminder_scheduling_delivery

## Requirement Reference

- User Story: us_037
- Story Location: .propel/context/tasks/EP-005/us_037/us_037.md
- Acceptance Criteria:
  - AC-1: System sends a reminder via both SMS and Email with appointment date, time, provider, and location at configured intervals (48h, 24h, 2h)
  - AC-2: Delivery occurs within 30 seconds of the scheduled trigger time (NFR-017)
  - AC-3: On delivery failure, system retries with exponential backoff (max 3 retries: 1min, 4min, 16min) and logs the failure
  - AC-4: Admin changes reminder timing; future appointments use new intervals, already-scheduled reminders unchanged
- Edge Cases:
  - Patient has no phone number: Only email reminders are sent; SMS notification is skipped with log entry
  - Appointment cancellation after reminders are scheduled: Pending reminders are cancelled and no further notifications are sent

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
| Database | PostgreSQL with pgvector | PostgreSQL 16, pgvector 0.5+ |
| Library | Hangfire | Hangfire 1.8.x |

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

Implement the core reminder scheduling engine, delivery background jobs, cancellation logic, and Admin settings API. This task creates `IReminderService` / `ReminderService` which reads configurable intervals from `SystemSettings`, creates `Notification` records on appointment booking, and cancels pending notifications on appointment cancellation. A recurring `ReminderSchedulerJob` (Hangfire, every 30 seconds) scans for due notifications and dispatches them. A `ReminderDeliveryJob` processes individual notifications through `ISmsService` and `IEmailService` with exponential backoff retry (1min, 4min, 16min per AC-3). An Admin settings API (`GET/PUT /api/admin/settings`) enables interval configuration (AC-4). The appointment booking and cancellation flows in `AppointmentService` are integrated with `IReminderService`.

## Dependent Tasks

- EP-005/us_037/task_001_db_reminder_configuration_schema — Provides SystemSettings table and NotificationStatus.Cancelled enum value
- EP-005/us_037/task_002_be_sms_email_reminder_services — Provides ISmsService and IEmailService.SendAppointmentReminderAsync for delivery

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/Interfaces/IReminderService.cs` — Interface for reminder scheduling and cancellation
- **NEW** `src/backend/PatientAccess.Business/Services/ReminderService.cs` — Implementation reading SystemSettings, creating/cancelling Notification records
- **NEW** `src/backend/PatientAccess.Business/BackgroundJobs/ReminderSchedulerJob.cs` — Recurring Hangfire job scanning for due reminders (every 30s)
- **NEW** `src/backend/PatientAccess.Business/BackgroundJobs/ReminderDeliveryJob.cs` — Hangfire job delivering individual reminders with exponential backoff
- **NEW** `src/backend/PatientAccess.Business/DTOs/SystemSettingDto.cs` — DTO for Admin settings API
- **NEW** `src/backend/PatientAccess.Business/DTOs/UpdateSystemSettingsRequestDto.cs` — Request DTO for updating settings
- **MODIFY** `src/backend/PatientAccess.Web/Controllers/AdminController.cs` — Add GET/PUT /api/admin/settings endpoints
- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Register IReminderService, ReminderSchedulerJob, ReminderDeliveryJob, and Hangfire recurring job
- **MODIFY** `src/backend/PatientAccess.Business/Services/AppointmentService.cs` — Integrate reminder scheduling on booking and cancellation on cancel

## Implementation Plan

1. **Define IReminderService interface**:
   ```csharp
   namespace PatientAccess.Business.Interfaces;

   public interface IReminderService
   {
       Task ScheduleRemindersAsync(Guid appointmentId);
       Task CancelRemindersAsync(Guid appointmentId);
       Task<int> ProcessDueRemindersAsync();
   }
   ```
   - `ScheduleRemindersAsync`: creates Notification records for each interval × channel combination
   - `CancelRemindersAsync`: sets all Pending notifications for the appointment to Cancelled
   - `ProcessDueRemindersAsync`: finds and enqueues due notifications, returns count processed

2. **Implement ReminderService**:
   - Inject `PatientAccessDbContext`, `ILogger<ReminderService>`
   - **ScheduleRemindersAsync**:
     1. Load appointment with Patient and Provider navigation properties
     2. Read `Reminder.Intervals` from `SystemSettings` table (e.g., `[48, 24, 2]`), parse as JSON int array
     3. Read `Reminder.SmsEnabled` and `Reminder.EmailEnabled` from `SystemSettings`
     4. For each interval, calculate `ScheduledTime = appointment.ScheduledDateTime - TimeSpan.FromHours(interval)`
     5. Skip intervals where `ScheduledTime` is already in the past
     6. Create Notification records:
        - If SMS enabled AND patient has phone: one `Notification` with `ChannelType.SMS`
        - If Email enabled: one `Notification` with `ChannelType.Email`
     7. Set `Status = Pending`, `TemplateName = "appointment_reminder"`, `RetryCount = 0`
     8. Bulk insert via `AddRange` and `SaveChangesAsync`
   - **CancelRemindersAsync** (edge case: appointment cancelled after reminders scheduled):
     1. Query `Notifications` where `AppointmentId == appointmentId AND Status == Pending`
     2. Set each to `Status = Cancelled`, `UpdatedAt = DateTime.UtcNow`
     3. `SaveChangesAsync`
     4. Log count of cancelled notifications
   - **ProcessDueRemindersAsync**:
     1. Query `Notifications` where `Status == Pending AND ScheduledTime <= DateTime.UtcNow`
     2. Order by `ScheduledTime` ascending (oldest first)
     3. Take batch of 50 (configurable) to prevent long-running transactions
     4. For each notification: enqueue `ReminderDeliveryJob.DeliverAsync(notificationId)` via Hangfire `BackgroundJob.Enqueue`
     5. Return count enqueued

3. **Create ReminderSchedulerJob** (recurring Hangfire job):
   ```csharp
   public class ReminderSchedulerJob
   {
       private readonly IReminderService _reminderService;
       private readonly ILogger<ReminderSchedulerJob> _logger;

       public async Task RunAsync()
       {
           var count = await _reminderService.ProcessDueRemindersAsync();
           _logger.LogInformation("ReminderSchedulerJob processed {Count} due reminders", count);
       }
   }
   ```
   Registered as Hangfire recurring job with 30-second interval (meets NFR-017: delivery within 30s of trigger time).

4. **Create ReminderDeliveryJob** with exponential backoff:
   ```csharp
   public class ReminderDeliveryJob
   {
       [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })]
       public async Task DeliverAsync(Guid notificationId)
       {
           // 1. Load notification with Appointment, Recipient navigation
           // 2. Verify notification still Pending (race condition check)
           // 3. Verify appointment not cancelled (check Appointment.Status)
           // 4. Based on ChannelType:
           //    SMS: call ISmsService.SendAppointmentReminderSmsAsync
           //    Email: call IEmailService.SendAppointmentReminderAsync
           // 5. On success: set Status = Sent, SentTime = DateTime.UtcNow
           // 6. On failure: increment RetryCount, set LastErrorMessage, throw to trigger Hangfire retry
       }
   }
   ```
   Retry delays: 60s (1min), 240s (4min), 960s (16min) per AC-3 specification.

5. **Create DTOs for Admin settings API**:
   ```csharp
   public class SystemSettingDto
   {
       public string Key { get; set; } = string.Empty;
       public string Value { get; set; } = string.Empty;
       public string? Description { get; set; }
   }

   public class UpdateSystemSettingsRequestDto
   {
       [Required]
       public List<SystemSettingDto> Settings { get; set; } = new();
   }
   ```

6. **Add Admin settings endpoints to AdminController**:
   ```csharp
   [HttpGet("settings")]
   public async Task<ActionResult<List<SystemSettingDto>>> GetSettings()
   // Returns all SystemSettings as list of key-value DTOs

   [HttpPut("settings")]
   public async Task<IActionResult> UpdateSettings([FromBody] UpdateSystemSettingsRequestDto request)
   // Updates specified settings; only modifies provided keys, preserves others (AC-4: new intervals for future only)
   ```
   Both endpoints require `AdminOnly` policy (already applied at controller level).

7. **Register services and recurring job in Program.cs**:
   ```csharp
   builder.Services.AddScoped<IReminderService, ReminderService>();
   builder.Services.AddScoped<ReminderSchedulerJob>();
   builder.Services.AddScoped<ReminderDeliveryJob>();
   ```
   After `app.UseHangfireDashboard`:
   ```csharp
   RecurringJob.AddOrUpdate<ReminderSchedulerJob>(
       "reminder-scheduler",
       job => job.RunAsync(),
       "*/30 * * * * *"); // Every 30 seconds (cron with seconds)
   ```

8. **Integrate with AppointmentService**:
   - Inject `IReminderService` into `AppointmentService` constructor
   - After successful appointment creation (`SaveChangesAsync`), call `await _reminderService.ScheduleRemindersAsync(appointment.AppointmentId)`
   - In cancellation flow (where `TODO: Send cancellation confirmation notification` exists), call `await _reminderService.CancelRemindersAsync(appointment.AppointmentId)`

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── BackgroundJobs/
│   │   ├── ConfirmationEmailJob.cs      # EXISTS — pattern reference for Hangfire jobs
│   │   └── SlotAvailabilityMonitor.cs   # EXISTS — pattern reference for recurring jobs
│   ├── Interfaces/
│   │   ├── IAppointmentService.cs       # EXISTS
│   │   └── ISmsService.cs              # FROM task_002
│   ├── Services/
│   │   ├── AppointmentService.cs        # EXISTS — booking/cancellation flows, has TODO for notification
│   │   ├── IEmailService.cs             # EXISTS — FROM task_002 (updated with reminder method)
│   │   ├── EmailService.cs              # EXISTS — FROM task_002 (updated with reminder method)
│   │   └── SmsService.cs               # FROM task_002
│   └── DTOs/
│       └── ...
├── PatientAccess.Web/
│   ├── Controllers/
│   │   └── AdminController.cs           # EXISTS — AdminOnly policy, health endpoint
│   └── Program.cs                       # EXISTS — Hangfire configured, DI registrations
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Interfaces/IReminderService.cs | Interface with Schedule, Cancel, ProcessDue methods |
| CREATE | src/backend/PatientAccess.Business/Services/ReminderService.cs | Core scheduling logic reading SystemSettings, creating/cancelling Notifications |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/ReminderSchedulerJob.cs | Recurring job scanning for due reminders every 30s |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/ReminderDeliveryJob.cs | Delivery job with exponential backoff retry (1min, 4min, 16min) |
| CREATE | src/backend/PatientAccess.Business/DTOs/SystemSettingDto.cs | DTO for settings key-value pair |
| CREATE | src/backend/PatientAccess.Business/DTOs/UpdateSystemSettingsRequestDto.cs | Request DTO for bulk settings update |
| MODIFY | src/backend/PatientAccess.Web/Controllers/AdminController.cs | Add GET/PUT /api/admin/settings endpoints |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register IReminderService, jobs, and Hangfire recurring schedule |
| MODIFY | src/backend/PatientAccess.Business/Services/AppointmentService.cs | Call ScheduleRemindersAsync on booking, CancelRemindersAsync on cancellation |

## External References

- Hangfire Recurring Jobs: https://docs.hangfire.io/en/latest/background-methods/performing-recurrent-tasks.html
- Hangfire AutomaticRetry: https://docs.hangfire.io/en/latest/background-methods/dealing-with-exceptions.html
- ASP.NET Core Background Services: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
- EF Core Bulk Operations: https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.sln
```

## Implementation Validation Strategy

- [ ] `IReminderService` interface compiles with all three methods
- [ ] `ReminderService.ScheduleRemindersAsync` creates correct number of Notification records (intervals × channels)
- [ ] `ReminderService.CancelRemindersAsync` sets Pending notifications to Cancelled status
- [ ] `ReminderSchedulerJob` runs without errors and enqueues due notifications
- [ ] `ReminderDeliveryJob` retries with correct delays (60s, 240s, 960s) on failure
- [ ] `GET /api/admin/settings` returns current system settings (Admin role required)
- [ ] `PUT /api/admin/settings` updates settings values
- [ ] Appointment booking triggers reminder scheduling
- [ ] Appointment cancellation cancels pending reminders
- [ ] Solution builds without warnings

## Implementation Checklist

- [x] Create `IReminderService` interface with `ScheduleRemindersAsync`, `CancelRemindersAsync`, `ProcessDueRemindersAsync`
- [x] Implement `ReminderService` reading intervals from SystemSettings and creating Notification records per channel
- [x] Implement `CancelRemindersAsync` setting pending notifications to Cancelled on appointment cancellation
- [x] Create `ReminderSchedulerJob` as recurring Hangfire job running every 30 seconds
- [x] Create `ReminderDeliveryJob` with `AutomaticRetry(Attempts=3, DelaysInSeconds=[60, 240, 960])`
- [x] Add `GET/PUT /api/admin/settings` endpoints to `AdminController`
- [x] Register services and recurring job in `Program.cs` DI container
- [x] Integrate `IReminderService` into `AppointmentService` booking and cancellation flows
