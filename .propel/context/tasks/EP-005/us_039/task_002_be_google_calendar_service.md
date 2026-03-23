# Task - task_002_be_google_calendar_service

## Requirement Reference

- User Story: us_039
- Story Location: .propel/context/tasks/EP-005/us_039/us_039.md
- Acceptance Criteria:
  - AC-1: Calendar event created via Google Calendar API with appointment date, time, provider, location, and visit reason
  - AC-2: Google Calendar event updated to reflect new date and time on reschedule
  - AC-3: Google Calendar event removed on cancellation
  - AC-4: OAuth2 flow initiated by "Add to Google Calendar" button for non-connected users
- Edge Cases:
  - OAuth token expiry: Refresh tokens used to obtain new access tokens; if refresh fails, IsConnected set to false and user prompted to re-authorize (EC-2)

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
| Library | Google.Apis.Calendar.v3 | Latest stable compatible with .NET 8 |

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

Implement the Google Calendar API integration service and OAuth2 authorization flow. This creates `ICalendarService` as a provider-agnostic interface and `GoogleCalendarService` as the Google-specific implementation using the official `Google.Apis.Calendar.v3` NuGet package. The service handles three calendar CRUD operations: creating events on booking (AC-1), updating events on reschedule (AC-2), and deleting events on cancellation (AC-3). The OAuth2 flow generates an authorization URL for user consent (AC-4), handles the callback to exchange the authorization code for tokens, and stores them in the `CalendarIntegration` entity. Token refresh logic automatically obtains new access tokens using stored refresh tokens; on refresh failure, the connection is marked inactive (EC-2). A `CalendarController` exposes the OAuth endpoints.

## Dependent Tasks

- EP-005/us_039/task_001_db_calendar_integration_schema — Provides CalendarIntegration entity and Appointment.GoogleCalendarEventId field

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/Interfaces/ICalendarService.cs` — Provider-agnostic calendar interface
- **NEW** `src/backend/PatientAccess.Business/Services/GoogleCalendarService.cs` — Google Calendar API v3 implementation
- **NEW** `src/backend/PatientAccess.Business/DTOs/CalendarEventDto.cs` — DTO for calendar event data
- **NEW** `src/backend/PatientAccess.Web/Controllers/CalendarController.cs` — OAuth2 endpoints (connect, callback, status, disconnect)
- **MODIFY** `src/backend/PatientAccess.Web/Program.cs` — Register ICalendarService, add HttpClient for Google API
- **MODIFY** `src/backend/PatientAccess.Web/appsettings.json` — Add GoogleCalendarSettings configuration section
- **MODIFY** `src/backend/PatientAccess.Business/PatientAccess.Business.csproj` — Add Google.Apis.Calendar.v3 NuGet package

## Implementation Plan

1. **Add Google.Apis.Calendar.v3 NuGet package**:
   ```xml
   <PackageReference Include="Google.Apis.Calendar.v3" Version="1.*" />
   ```
   The official Google client library for .NET provides Calendar API v3 bindings with built-in token management.

2. **Create CalendarEventDto**:
   ```csharp
   public class CalendarEventDto
   {
       public string Title { get; set; } = string.Empty;        // "Appointment with Dr. Smith"
       public DateTime StartTime { get; set; }
       public DateTime EndTime { get; set; }
       public string Description { get; set; } = string.Empty;  // Visit reason
       public string Location { get; set; } = string.Empty;     // Provider name + specialty
   }
   ```

3. **Define ICalendarService interface**:
   ```csharp
   namespace PatientAccess.Business.Interfaces;

   public interface ICalendarService
   {
       Task<string> GetAuthorizationUrlAsync(Guid userId, string redirectUri);
       Task HandleCallbackAsync(Guid userId, string authorizationCode, string redirectUri);
       Task<string?> CreateEventAsync(Guid userId, CalendarEventDto eventData);
       Task UpdateEventAsync(Guid userId, string eventId, CalendarEventDto eventData);
       Task DeleteEventAsync(Guid userId, string eventId);
       Task<bool> IsConnectedAsync(Guid userId);
       Task DisconnectAsync(Guid userId);
   }
   ```
   - `GetAuthorizationUrlAsync` returns the Google OAuth2 consent URL (AC-4)
   - `HandleCallbackAsync` exchanges auth code for tokens and stores them (AC-4)
   - `CreateEventAsync` returns the Google event ID for storage on Appointment (AC-1)
   - `UpdateEventAsync` modifies event start/end time (AC-2)
   - `DeleteEventAsync` removes event from Google Calendar (AC-3)
   - `IsConnectedAsync` checks if user has active connection
   - `DisconnectAsync` revokes token and marks inactive

4. **Implement GoogleCalendarService — OAuth2 flow**:
   - Read `GoogleCalendarSettings` from configuration (ClientId, ClientSecret, Scopes)
   - `GetAuthorizationUrlAsync`: Build Google OAuth2 consent URL:
     ```
     https://accounts.google.com/o/oauth2/v2/auth
       ?client_id={ClientId}
       &redirect_uri={RedirectUri}
       &response_type=code
       &scope=https://www.googleapis.com/auth/calendar.events
       &access_type=offline
       &prompt=consent
       &state={userId}
     ```
     - `access_type=offline` ensures a refresh token is returned
     - `prompt=consent` forces re-consent to always get a refresh token
     - `state` parameter carries userId for CSRF protection and user identification
   - `HandleCallbackAsync`: Exchange authorization code for tokens:
     - POST to `https://oauth2.googleapis.com/token` with code, client_id, client_secret, redirect_uri, grant_type=authorization_code
     - Parse response for access_token, refresh_token, expires_in
     - Upsert CalendarIntegration record (encrypt tokens before storage)
     - Set IsConnected = true, CalendarId = "primary"

5. **Implement GoogleCalendarService — token refresh (EC-2)**:
   - Before each API call, check `TokenExpiry` against `DateTime.UtcNow`
   - If expired, POST to `https://oauth2.googleapis.com/token` with refresh_token, client_id, client_secret, grant_type=refresh_token
   - On success: update AccessToken and TokenExpiry in CalendarIntegration
   - On failure (400/401 from Google): set IsConnected = false, log warning, throw `CalendarTokenExpiredException`
   - Callers can catch this exception to prompt user re-authorization

6. **Implement GoogleCalendarService — CRUD operations**:
   - `CreateEventAsync`:
     ```csharp
     var calendarService = GetCalendarServiceForUser(userId); // builds Google CalendarService with user's tokens
     var calendarEvent = new Event
     {
         Summary = eventData.Title,
         Description = eventData.Description,
         Location = eventData.Location,
         Start = new EventDateTime { DateTimeDateTimeOffset = eventData.StartTime, TimeZone = "UTC" },
         End = new EventDateTime { DateTimeDateTimeOffset = eventData.EndTime, TimeZone = "UTC" }
     };
     var created = await calendarService.Events.Insert(calendarEvent, "primary").ExecuteAsync();
     return created.Id; // Store as Appointment.GoogleCalendarEventId
     ```
   - `UpdateEventAsync`: Fetch existing event, update Start/End, call `Events.Update`
   - `DeleteEventAsync`: Call `Events.Delete("primary", eventId).ExecuteAsync()`
   - All operations wrapped in try/catch for `GoogleApiException` — log and rethrow for retry by background job

7. **Create CalendarController**:
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   [Authorize]
   public class CalendarController : ControllerBase
   {
       // GET /api/calendar/connect — Returns OAuth2 authorization URL (AC-4)
       [HttpGet("connect")]
       public async Task<IActionResult> Connect()

       // GET /api/calendar/callback — Handles OAuth2 redirect with authorization code
       [HttpGet("callback")]
       [AllowAnonymous] // Callback from Google, validated via state parameter
       public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)

       // GET /api/calendar/status — Returns connection status for current user
       [HttpGet("status")]
       public async Task<IActionResult> GetStatus()

       // POST /api/calendar/disconnect — Revokes Google Calendar connection
       [HttpPost("disconnect")]
       public async Task<IActionResult> Disconnect()
   }
   ```
   - All endpoints except `callback` require `[Authorize]` with JWT authentication
   - `callback` uses `[AllowAnonymous]` because it's a redirect from Google; validated via `state` parameter containing encrypted userId
   - Extract userId from JWT claims using `User.FindFirst("sub")` pattern (existing project pattern)

8. **Add GoogleCalendarSettings to appsettings.json and register DI**:
   ```json
   "GoogleCalendarSettings": {
       "ClientId": "",
       "ClientSecret": "",
       "RedirectUri": "https://localhost:5001/api/calendar/callback",
       "Scopes": ["https://www.googleapis.com/auth/calendar.events"]
   }
   ```
   - Register `ICalendarService` as Scoped (uses DbContext):
     ```csharp
     builder.Services.AddScoped<ICalendarService, GoogleCalendarService>();
     ```
   - Add HttpClient for Google token exchange:
     ```csharp
     builder.Services.AddHttpClient("GoogleOAuth", client => {
         client.BaseAddress = new Uri("https://oauth2.googleapis.com/");
     });
     ```

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Interfaces/                     # No ICalendarService
│   ├── Services/                       # No GoogleCalendarService
│   ├── DTOs/                           # No CalendarEventDto
│   └── PatientAccess.Business.csproj   # No Google.Apis.Calendar.v3 package
├── PatientAccess.Data/
│   └── Models/
│       ├── CalendarIntegration.cs      # FROM task_001
│       └── Appointment.cs              # FROM task_001 (has GoogleCalendarEventId)
├── PatientAccess.Web/
│   ├── Controllers/                    # No CalendarController
│   ├── Program.cs                      # No ICalendarService registration
│   └── appsettings.json               # No GoogleCalendarSettings section
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Interfaces/ICalendarService.cs | Provider-agnostic calendar interface with CRUD + OAuth methods |
| CREATE | src/backend/PatientAccess.Business/Services/GoogleCalendarService.cs | Google Calendar API v3 implementation with OAuth2 token management |
| CREATE | src/backend/PatientAccess.Business/DTOs/CalendarEventDto.cs | DTO for calendar event title, start/end time, description, location |
| CREATE | src/backend/PatientAccess.Web/Controllers/CalendarController.cs | OAuth2 endpoints (connect, callback, status, disconnect) |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register ICalendarService as Scoped, add named HttpClient for Google OAuth |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add GoogleCalendarSettings section (ClientId, ClientSecret, RedirectUri, Scopes) |
| MODIFY | src/backend/PatientAccess.Business/PatientAccess.Business.csproj | Add Google.Apis.Calendar.v3 NuGet package reference |

## External References

- Google Calendar API v3 .NET Quickstart: https://developers.google.com/calendar/api/quickstart/dotnet
- Google OAuth2 for Web Server Applications: https://developers.google.com/identity/protocols/oauth2/web-server
- Google.Apis.Calendar.v3 NuGet: https://www.nuget.org/packages/Google.Apis.Calendar.v3
- ASP.NET Core Named HttpClient: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#named-clients

## Build Commands

```bash
cd src/backend
dotnet restore PatientAccess.Business
dotnet build PatientAccess.sln
```

## Implementation Validation Strategy

- [ ] ICalendarService interface compiles with all 7 methods
- [ ] GoogleCalendarService builds with Google.Apis.Calendar.v3 dependency resolved
- [ ] OAuth2 authorization URL includes correct scopes, access_type=offline, prompt=consent
- [ ] Callback handler exchanges code for tokens and stores encrypted in CalendarIntegration
- [ ] Token refresh logic updates AccessToken on expiry; marks IsConnected=false on refresh failure (EC-2)
- [ ] CreateEventAsync returns Google event ID for storage
- [ ] UpdateEventAsync and DeleteEventAsync operate on correct event via eventId
- [ ] CalendarController endpoints are protected by [Authorize] except callback

## Implementation Checklist

- [ ] Add `Google.Apis.Calendar.v3` NuGet package to PatientAccess.Business.csproj
- [ ] Create `CalendarEventDto` with title, start/end time, description, location fields
- [ ] Create `ICalendarService` interface with CRUD and OAuth methods
- [ ] Implement `GoogleCalendarService` OAuth2 flow (authorization URL generation, callback token exchange)
- [ ] Implement token refresh with automatic retry and IsConnected=false on failure
- [ ] Implement calendar event CRUD (CreateEventAsync, UpdateEventAsync, DeleteEventAsync) using Google API
- [ ] Create `CalendarController` with connect, callback, status, disconnect endpoints
- [ ] Register ICalendarService in DI, add GoogleCalendarSettings to appsettings.json, add named HttpClient
