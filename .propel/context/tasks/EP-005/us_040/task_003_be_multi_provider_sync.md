# Task - task_003_be_multi_provider_sync

## Requirement Reference

- User Story: us_040
- Story Location: .propel/context/tasks/EP-005/us_040/us_040.md
- Acceptance Criteria:
  - AC-1: Outlook Calendar event created via Microsoft Graph API when appointment is booked
  - AC-2: Outlook Calendar event updated when appointment is rescheduled
  - AC-3: Outlook Calendar event removed when appointment is cancelled
- Edge Cases:
  - Both Google and Outlook connected: Events are created in both calendars independently (EC-2)
  - Microsoft Graph API rate limits exceeded: Requests queued and retried after rate limit window resets (EC-1)

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

Refactor the calendar synchronization infrastructure from US_039 to support multiple calendar providers simultaneously. This converts the single-provider `ICalendarService` DI registration to .NET 8 keyed services, resolving `GoogleCalendarService` via key "Google" and `OutlookCalendarService` via key "Outlook". The `CalendarSyncJob` is refactored to iterate all connected providers per user, creating/updating/deleting events in each connected calendar independently (EC-2). The `CalendarController` is updated with a `{provider}` route parameter to support provider-specific OAuth flows. The `Appointment` entity's `OutlookCalendarEventId` is persisted alongside `GoogleCalendarEventId` during event creation.

## Dependent Tasks

- EP-005/us_039/task_002_be_google_calendar_service — Provides ICalendarService interface, GoogleCalendarService, CalendarController
- EP-005/us_039/task_003_be_calendar_sync_integration — Provides CalendarSyncJob and AppointmentService integration
- EP-005/us_040/task_001_db_outlook_event_id_schema — Provides Appointment.OutlookCalendarEventId field
- EP-005/us_040/task_002_be_outlook_calendar_service — Provides OutlookCalendarService implementing ICalendarService

## Impacted Components

- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Switch to keyed services: register Google and Outlook calendar services with provider keys
- **MODIFY** `src/backend/PatientAccess.Business/BackgroundJobs/CalendarSyncJob.cs` — Refactor to iterate all connected providers per user
- **MODIFY** `src/backend/PatientAccess.Web/Controllers/CalendarController.cs` — Add `{provider}` route parameter for provider-specific OAuth
- **MODIFY** `src/backend/PatientAccess.Business/Services/AppointmentService.cs` — Update CancelAsync to pass both GoogleCalendarEventId and OutlookCalendarEventId to the delete job

## Implementation Plan

1. **Refactor DI to use .NET 8 keyed services**:
   ```csharp
   // In Program.cs — replace single ICalendarService registration from US_039
   // Old (US_039):
   // builder.Services.AddScoped<ICalendarService, GoogleCalendarService>();

   // New (US_040 — multi-provider):
   builder.Services.AddKeyedScoped<ICalendarService, GoogleCalendarService>("Google");
   builder.Services.AddKeyedScoped<ICalendarService, OutlookCalendarService>("Outlook");
   ```
   - .NET 8 natively supports keyed services via `AddKeyedScoped`
   - Each provider resolved by its string key matching CalendarIntegration.Provider value
   - CalendarSyncJob and CalendarController inject `IServiceProvider` to resolve by key at runtime

2. **Refactor CalendarSyncJob to multi-provider**:
   ```csharp
   public class CalendarSyncJob
   {
       private readonly IServiceProvider _serviceProvider;
       private readonly PatientAccessDbContext _context;
       private readonly ILogger<CalendarSyncJob> _logger;

       // Constructor: inject IServiceProvider instead of single ICalendarService
   }
   ```
   - Replace single `ICalendarService` injection with `IServiceProvider` for keyed resolution

3. **Refactor CalendarSyncJob.CreateCalendarEventAsync for all providers**:
   ```csharp
   [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })]
   public async Task CreateCalendarEventAsync(Guid appointmentId)
   {
       var appointment = await _context.Appointments
           .Include(a => a.Provider)
           .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
       if (appointment == null) return;

       // Find all connected calendar providers for this patient
       var connectedProviders = await _context.CalendarIntegrations
           .Where(c => c.UserId == appointment.PatientId && c.IsConnected)
           .Select(c => c.Provider)
           .ToListAsync();

       foreach (var provider in connectedProviders)
       {
           var calendarService = _serviceProvider.GetRequiredKeyedService<ICalendarService>(provider);
           var eventData = BuildCalendarEventDto(appointment);

           var eventId = await calendarService.CreateEventAsync(appointment.PatientId, eventData);

           // Store event ID on the correct field per provider
           if (provider == "Google")
               appointment.GoogleCalendarEventId = eventId;
           else if (provider == "Outlook")
               appointment.OutlookCalendarEventId = eventId;
       }

       appointment.UpdatedAt = DateTime.UtcNow;
       await _context.SaveChangesAsync();
   }
   ```
   - Iterates all connected providers for the patient (EC-2: independent per calendar)
   - Stores event ID in the provider-specific field on Appointment

4. **Refactor CalendarSyncJob.UpdateCalendarEventAsync for all providers**:
   ```csharp
   [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })]
   public async Task UpdateCalendarEventAsync(Guid appointmentId)
   {
       var appointment = await _context.Appointments
           .Include(a => a.Provider)
           .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
       if (appointment == null) return;

       var eventData = BuildCalendarEventDto(appointment);

       // Update Google event if exists
       if (!string.IsNullOrEmpty(appointment.GoogleCalendarEventId))
       {
           var googleService = _serviceProvider.GetRequiredKeyedService<ICalendarService>("Google");
           if (await googleService.IsConnectedAsync(appointment.PatientId))
               await googleService.UpdateEventAsync(appointment.PatientId, appointment.GoogleCalendarEventId, eventData);
       }

       // Update Outlook event if exists
       if (!string.IsNullOrEmpty(appointment.OutlookCalendarEventId))
       {
           var outlookService = _serviceProvider.GetRequiredKeyedService<ICalendarService>("Outlook");
           if (await outlookService.IsConnectedAsync(appointment.PatientId))
               await outlookService.UpdateEventAsync(appointment.PatientId, appointment.OutlookCalendarEventId, eventData);
       }
   }
   ```

5. **Refactor CalendarSyncJob.DeleteCalendarEventAsync for all providers**:
   ```csharp
   [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 240, 960 })]
   public async Task DeleteCalendarEventAsync(
       Guid appointmentId, string? googleEventId, string? outlookEventId, Guid patientId)
   {
       if (!string.IsNullOrEmpty(googleEventId))
       {
           var googleService = _serviceProvider.GetRequiredKeyedService<ICalendarService>("Google");
           if (await googleService.IsConnectedAsync(patientId))
               await googleService.DeleteEventAsync(patientId, googleEventId);
       }

       if (!string.IsNullOrEmpty(outlookEventId))
       {
           var outlookService = _serviceProvider.GetRequiredKeyedService<ICalendarService>("Outlook");
           if (await outlookService.IsConnectedAsync(patientId))
               await outlookService.DeleteEventAsync(patientId, outlookEventId);
       }
   }
   ```
   - Now accepts both Google and Outlook event IDs as parameters
   - Each provider deletion is independent — failure in one doesn't block the other

6. **Update AppointmentService.CancelAsync call signature**:
   - The existing CalendarSyncJob enqueue (from US_039/task_003) passes only `googleEventId`
   - Update to pass both event IDs:
     ```csharp
     var googleEventId = appointment.GoogleCalendarEventId;
     var outlookEventId = appointment.OutlookCalendarEventId;

     if (!string.IsNullOrEmpty(googleEventId) || !string.IsNullOrEmpty(outlookEventId))
     {
         BackgroundJob.Enqueue<CalendarSyncJob>(
             job => job.DeleteCalendarEventAsync(
                 appointment.AppointmentId, googleEventId, outlookEventId, appointment.PatientId));
     }
     ```

7. **Refactor CalendarController with provider route parameter**:
   ```csharp
   [ApiController]
   [Route("api/calendar/{provider}")]
   [Authorize]
   public class CalendarController : ControllerBase
   {
       private readonly IServiceProvider _serviceProvider;

       // GET /api/calendar/{provider}/connect
       [HttpGet("connect")]
       public async Task<IActionResult> Connect([FromRoute] string provider)
       {
           ValidateProvider(provider); // Only "google" or "outlook" allowed
           var calendarService = _serviceProvider.GetRequiredKeyedService<ICalendarService>(
               provider.Equals("google", StringComparison.OrdinalIgnoreCase) ? "Google" : "Outlook");
           // ... existing logic
       }

       // GET /api/calendar/{provider}/callback
       [HttpGet("callback")]
       [AllowAnonymous]
       public async Task<IActionResult> Callback(
           [FromRoute] string provider, [FromQuery] string code, [FromQuery] string state)

       // GET /api/calendar/status — Returns connection status for ALL providers
       [HttpGet("/api/calendar/status")]
       public async Task<IActionResult> GetStatus()
       {
           // Returns: { google: { isConnected: true }, outlook: { isConnected: false } }
       }

       // POST /api/calendar/{provider}/disconnect
       [HttpPost("disconnect")]
       public async Task<IActionResult> Disconnect([FromRoute] string provider)
   }
   ```
   - Route parameter `{provider}` accepts "google" or "outlook" (case-insensitive)
   - Status endpoint returns ALL providers at once for efficient frontend polling
   - Provider validation rejects unknown values (whitelist: "google", "outlook")

8. **Extract BuildCalendarEventDto helper**:
   ```csharp
   private static CalendarEventDto BuildCalendarEventDto(Appointment appointment)
   {
       return new CalendarEventDto
       {
           Title = $"Appointment with {appointment.Provider.Name}",
           StartTime = appointment.ScheduledDateTime,
           EndTime = appointment.ScheduledDateTime.AddMinutes(30),
           Description = $"Visit reason: {appointment.VisitReason}",
           Location = $"{appointment.Provider.Name} - {appointment.Provider.Specialty}"
       };
   }
   ```
   - Reusable across create and update operations — DRY principle

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── BackgroundJobs/
│   │   └── CalendarSyncJob.cs           # FROM US_039/task_003 — single-provider, needs refactor
│   ├── Interfaces/
│   │   └── ICalendarService.cs          # FROM US_039/task_002 — no changes needed
│   ├── Services/
│   │   ├── AppointmentService.cs        # FROM US_039/task_003 — enqueues CalendarSyncJob
│   │   ├── GoogleCalendarService.cs     # FROM US_039/task_002
│   │   └── OutlookCalendarService.cs    # FROM US_040/task_002
│   └── DTOs/
│       └── CalendarEventDto.cs          # FROM US_039/task_002 — reused, not duplicated
├── PatientAccess.Web/
│   ├── Controllers/
│   │   └── CalendarController.cs        # FROM US_039/task_002 — single-provider, needs refactor
│   └── Program.cs                       # FROM US_039 — single ICalendarService registration
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Replace single ICalendarService registration with keyed services (Google + Outlook) |
| MODIFY | src/backend/PatientAccess.Business/BackgroundJobs/CalendarSyncJob.cs | Refactor to iterate all connected providers per user, store per-provider event IDs |
| MODIFY | src/backend/PatientAccess.Web/Controllers/CalendarController.cs | Add {provider} route parameter, return multi-provider status |
| MODIFY | src/backend/PatientAccess.Business/Services/AppointmentService.cs | Update CancelAsync to pass both Google and Outlook event IDs to delete job |

## External References

- .NET 8 Keyed Services: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8#keyed-di-services
- Hangfire Automatic Retries: https://docs.hangfire.io/en/latest/background-methods/dealing-with-exceptions.html
- ASP.NET Core Route Parameters: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-constraints

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.sln
```

## Implementation Validation Strategy

- [ ] Keyed services resolve GoogleCalendarService for key "Google" and OutlookCalendarService for key "Outlook"
- [ ] CalendarSyncJob.CreateCalendarEventAsync creates events in all connected providers independently (EC-2)
- [ ] CalendarSyncJob.UpdateCalendarEventAsync updates events in both Google and Outlook when event IDs exist
- [ ] CalendarSyncJob.DeleteCalendarEventAsync deletes events from both providers independently
- [ ] CalendarController accepts `{provider}` route parameter and rejects unknown providers
- [ ] Status endpoint returns connection status for all providers in single response
- [ ] AppointmentService.CancelAsync passes both GoogleCalendarEventId and OutlookCalendarEventId to delete job
- [ ] `[AutomaticRetry]` still configured with exponential backoff on all job methods

## Implementation Checklist

- [x] Replace single `ICalendarService` DI registration with `AddKeyedScoped` for "Google" and "Outlook"
- [x] Refactor `CalendarSyncJob` constructor to inject `IServiceProvider` instead of single `ICalendarService`
- [x] Refactor `CreateCalendarEventAsync` to iterate connected providers and persist per-provider event IDs
- [x] Refactor `UpdateCalendarEventAsync` to update events in both Google and Outlook when IDs exist
- [x] Refactor `DeleteCalendarEventAsync` to accept both Google and Outlook event IDs and delete independently
- [x] Update `AppointmentService.CancelAsync` to pass both event IDs to the delete job
- [x] Refactor `CalendarController` with `{provider}` route parameter and multi-provider status endpoint
- [x] Extract `BuildCalendarEventDto` helper for DRY event data construction
