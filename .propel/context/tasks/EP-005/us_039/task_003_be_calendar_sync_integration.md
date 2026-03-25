# Task - task_003_be_calendar_sync_integration

## Requirement Reference

- User Story: us_039
- Story Location: .propel/context/tasks/EP-005/us_039/us_039.md
- Acceptance Criteria:
  - AC-1: Calendar event created via Google Calendar API when appointment is booked
  - AC-2: Google Calendar event updated when appointment is rescheduled
  - AC-3: Google Calendar event removed when appointment is cancelled
- Edge Cases:
  - Google Calendar API temporarily unavailable: Appointment booking succeeds; calendar sync retried asynchronously with exponential backoff (EC-1)

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

Integrate the `ICalendarService` into the appointment lifecycle and implement asynchronous calendar synchronization via Hangfire background jobs. When an appointment is booked, rescheduled, or cancelled, a background job is enqueued to create, update, or delete the corresponding Google Calendar event respectively. This decouples the calendar sync from the booking flow, ensuring appointment operations are never blocked by Google API availability (EC-1). The `CalendarSyncJob` implements exponential backoff retry (1min, 4min, 16min) for transient failures. On successful event creation, the `GoogleCalendarEventId` is persisted on the `Appointment` entity for use in subsequent update/delete operations.

## Dependent Tasks

- EP-005/us_039/task_001_db_calendar_integration_schema — Provides CalendarIntegration entity and Appointment.GoogleCalendarEventId field
- EP-005/us_039/task_002_be_google_calendar_service — Provides ICalendarService with CRUD and OAuth methods

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/BackgroundJobs/CalendarSyncJob.cs` — Hangfire job for async calendar event CRUD with retry
- **MODIFY** `src/backend/PatientAccess.Business/Services/AppointmentService.cs` — Enqueue CalendarSyncJob on booking, reschedule, cancel
- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Register CalendarSyncJob as Scoped for Hangfire resolution

## Implementation Plan

1. **Create CalendarSyncJob Hangfire background job**:
   ```csharp
   namespace PatientAccess.Business.BackgroundJobs;

   public class CalendarSyncJob
   {
       private readonly ICalendarService _calendarService;
       private readonly PatientAccessDbContext _context;
       private readonly ILogger<CalendarSyncJob> _logger;

       public CalendarSyncJob(
           ICalendarService calendarService,
           PatientAccessDbContext context,
           ILogger<CalendarSyncJob> logger) { ... }

       [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })]
       public async Task CreateCalendarEventAsync(Guid appointmentId) { ... }

       [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })]
       public async Task UpdateCalendarEventAsync(Guid appointmentId) { ... }

       [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })]
       public async Task DeleteCalendarEventAsync(Guid appointmentId, string googleCalendarEventId, Guid patientId) { ... }
   }
   ```
   - Uses `[AutomaticRetry]` attribute with exponential backoff: 1min, 4min, 16min (EC-1)
   - Each method is independently retriable — serialized via appointment ID

2. **Implement CreateCalendarEventAsync**:
   ```csharp
   public async Task CreateCalendarEventAsync(Guid appointmentId)
   {
       var appointment = await _context.Appointments
           .Include(a => a.Provider)
           .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

       if (appointment == null) return;

       // Check if user has Google Calendar connected
       var isConnected = await _calendarService.IsConnectedAsync(appointment.PatientId);
       if (!isConnected) return; // Silently skip — user hasn't connected calendar

       var eventData = new CalendarEventDto
       {
           Title = $"Appointment with {appointment.Provider.Name}",
           StartTime = appointment.ScheduledDateTime,
           EndTime = appointment.ScheduledDateTime.AddMinutes(30),
           Description = $"Visit reason: {appointment.VisitReason}",
           Location = $"{appointment.Provider.Name} - {appointment.Provider.Specialty}"
       };

       var eventId = await _calendarService.CreateEventAsync(appointment.PatientId, eventData);

       // Persist Google Calendar event ID for future update/delete (AC-2, AC-3)
       appointment.GoogleCalendarEventId = eventId;
       appointment.UpdatedAt = DateTime.UtcNow;
       await _context.SaveChangesAsync();
   }
   ```
   - Only creates event if user has connected Google Calendar (IsConnected = true)
   - Stores returned event ID on Appointment for subsequent operations

3. **Implement UpdateCalendarEventAsync**:
   ```csharp
   public async Task UpdateCalendarEventAsync(Guid appointmentId)
   {
       var appointment = await _context.Appointments
           .Include(a => a.Provider)
           .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

       if (appointment?.GoogleCalendarEventId == null) return;

       var isConnected = await _calendarService.IsConnectedAsync(appointment.PatientId);
       if (!isConnected) return;

       var eventData = new CalendarEventDto
       {
           Title = $"Appointment with {appointment.Provider.Name}",
           StartTime = appointment.ScheduledDateTime,
           EndTime = appointment.ScheduledDateTime.AddMinutes(30),
           Description = $"Visit reason: {appointment.VisitReason}",
           Location = $"{appointment.Provider.Name} - {appointment.Provider.Specialty}"
       };

       await _calendarService.UpdateEventAsync(
           appointment.PatientId,
           appointment.GoogleCalendarEventId,
           eventData);
   }
   ```
   - Skips if no GoogleCalendarEventId (appointment was created before calendar connection)

4. **Implement DeleteCalendarEventAsync**:
   ```csharp
   public async Task DeleteCalendarEventAsync(
       Guid appointmentId, string googleCalendarEventId, Guid patientId)
   {
       var isConnected = await _calendarService.IsConnectedAsync(patientId);
       if (!isConnected) return;

       await _calendarService.DeleteEventAsync(patientId, googleCalendarEventId);
   }
   ```
   - Event ID and patient ID passed as parameters since appointment may be in cancelled state
   - Graceful skip if user disconnected calendar between booking and cancellation

5. **Integrate into AppointmentService.CreateAppointmentAsync (AC-1)**:
   - After successful appointment creation and `SaveChangesAsync`:
     ```csharp
     // Enqueue async calendar sync (US_039 - AC-1)
     BackgroundJob.Enqueue<CalendarSyncJob>(
         job => job.CreateCalendarEventAsync(appointment.AppointmentId));
     ```
   - Placed after the transaction commits — booking always succeeds regardless of calendar sync (EC-1)

6. **Integrate into AppointmentService.RescheduleAsync (AC-2)**:
   - After successful reschedule transaction commit:
     ```csharp
     // Enqueue async calendar update (US_039 - AC-2)
     BackgroundJob.Enqueue<CalendarSyncJob>(
         job => job.UpdateCalendarEventAsync(appointment.AppointmentId));
     ```

7. **Integrate into AppointmentService.CancelAsync (AC-3)**:
   - Before setting status to Cancelled, capture the event ID:
     ```csharp
     var googleEventId = appointment.GoogleCalendarEventId;
     ```
   - After successful cancellation and `SaveChangesAsync`:
     ```csharp
     // Enqueue async calendar event deletion (US_039 - AC-3)
     if (!string.IsNullOrEmpty(googleEventId))
     {
         BackgroundJob.Enqueue<CalendarSyncJob>(
             job => job.DeleteCalendarEventAsync(
                 appointment.AppointmentId, googleEventId, appointment.PatientId));
     }
     ```

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── BackgroundJobs/
│   │   ├── ConfirmationEmailJob.cs     # EXISTS — pattern reference for Hangfire jobs
│   │   ├── SlotAvailabilityMonitor.cs  # EXISTS — recurring job pattern
│   │   └── (no CalendarSyncJob)
│   ├── Interfaces/
│   │   └── ICalendarService.cs         # FROM task_002
│   ├── Services/
│   │   ├── AppointmentService.cs       # EXISTS — CreateAppointmentAsync, RescheduleAsync, CancelAsync
│   │   └── GoogleCalendarService.cs    # FROM task_002
│   └── DTOs/
│       └── CalendarEventDto.cs         # FROM task_002
├── PatientAccess.Web/
│   └── Program.cs                      # EXISTS — Hangfire configured, needs CalendarSyncJob registration
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/CalendarSyncJob.cs | Hangfire job with create/update/delete methods and exponential backoff retry |
| MODIFY | src/backend/PatientAccess.Business/Services/AppointmentService.cs | Enqueue CalendarSyncJob after booking (AC-1), reschedule (AC-2), cancel (AC-3) |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register CalendarSyncJob as Scoped for Hangfire DI resolution |

## External References

- Hangfire Automatic Retries: https://docs.hangfire.io/en/latest/background-methods/dealing-with-exceptions.html
- Hangfire Best Practices: https://docs.hangfire.io/en/latest/best-practices.html
- EF Core Async Patterns: https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.sln
```

## Implementation Validation Strategy

- [ ] CalendarSyncJob compiles with all three async methods
- [ ] `[AutomaticRetry]` configured with 3 attempts and 1min/4min/16min delays
- [ ] CreateCalendarEventAsync persists GoogleCalendarEventId on Appointment after creation
- [ ] CreateCalendarEventAsync silently skips when user has no calendar connection
- [ ] UpdateCalendarEventAsync skips when no GoogleCalendarEventId present
- [ ] AppointmentService.CreateAppointmentAsync enqueues calendar sync job after commit
- [ ] AppointmentService.RescheduleAsync enqueues calendar update job after commit
- [ ] AppointmentService.CancelAsync enqueues calendar delete job with captured event ID

## Implementation Checklist

- [ ] Create `CalendarSyncJob` with `CreateCalendarEventAsync`, `UpdateCalendarEventAsync`, `DeleteCalendarEventAsync` methods
- [ ] Configure `[AutomaticRetry]` with exponential backoff (1min, 4min, 16min) on all job methods
- [ ] Implement event creation logic with GoogleCalendarEventId persistence and connection check
- [ ] Integrate `BackgroundJob.Enqueue<CalendarSyncJob>` into `CreateAppointmentAsync` after transaction commit
- [ ] Integrate `BackgroundJob.Enqueue<CalendarSyncJob>` into `RescheduleAsync` after transaction commit
- [ ] Integrate `BackgroundJob.Enqueue<CalendarSyncJob>` into `CancelAsync` after cancellation with captured event ID
- [ ] Register `CalendarSyncJob` as Scoped in `Program.cs` for Hangfire DI resolution
