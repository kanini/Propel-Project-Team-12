# Task - task_002_be_outlook_calendar_service

## Requirement Reference

- User Story: us_040
- Story Location: .propel/context/tasks/EP-005/us_040/us_040.md
- Acceptance Criteria:
  - AC-1: Outlook Calendar event created via Microsoft Graph API with appointment date, time, provider, location, and visit reason
  - AC-2: Outlook Calendar event updated to reflect new date and time on reschedule
  - AC-3: Outlook Calendar event removed on cancellation
  - AC-4: Microsoft OAuth2 flow initiated by "Add to Outlook" button for non-connected users
- Edge Cases:
  - Microsoft Graph API rate limits exceeded: Requests queued and retried after rate limit window resets; catch HTTP 429, respect Retry-After header (EC-1)

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
| Library | Microsoft.Graph | Latest stable compatible with .NET 8 |

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

Implement the Microsoft Outlook Calendar integration service using the Microsoft Graph API. This creates `OutlookCalendarService` implementing the existing `ICalendarService` interface (defined by US_039) using the official `Microsoft.Graph` NuGet SDK. The service handles calendar event CRUD: creating events on booking (AC-1), updating events on reschedule (AC-2), and deleting events on cancellation (AC-3). The Microsoft OAuth2 flow uses Azure AD endpoints (`login.microsoftonline.com/common/oauth2/v2.0`) to generate an authorization URL for user consent (AC-4), exchange the authorization code for tokens, and store them in the `CalendarIntegration` entity with Provider="Outlook". Token refresh uses the Microsoft identity platform token endpoint. Rate limit handling catches HTTP 429 responses and respects the `Retry-After` header (EC-1). Configuration settings for Azure AD app registration are stored in `OutlookCalendarSettings`.

## Dependent Tasks

- EP-005/us_039/task_001_db_calendar_integration_schema — Provides CalendarIntegration entity for token storage
- EP-005/us_039/task_002_be_google_calendar_service — Provides ICalendarService interface and CalendarEventDto
- EP-005/us_040/task_001_db_outlook_event_id_schema — Provides Appointment.OutlookCalendarEventId field

## Impacted Components

- **NEW** `src/backend/PatientAccess.Business/Services/OutlookCalendarService.cs` — Microsoft Graph API implementation of ICalendarService
- **MODIFY** `src/backend/PatientAccess.Web/appsettings.json` — Add OutlookCalendarSettings configuration section
- **MODIFY** `src/backend/PatientAccess.Business/PatientAccess.Business.csproj` — Add Microsoft.Graph NuGet package

## Implementation Plan

1. **Add Microsoft.Graph NuGet package**:
   ```xml
   <PackageReference Include="Microsoft.Graph" Version="5.*" />
   ```
   The official Microsoft Graph SDK for .NET provides Calendar API bindings with built-in authentication support.

2. **Implement OutlookCalendarService — OAuth2 flow**:
   - Read `OutlookCalendarSettings` from configuration (TenantId, ClientId, ClientSecret, RedirectUri)
   - `GetAuthorizationUrlAsync`: Build Microsoft OAuth2 consent URL:
     ```
     https://login.microsoftonline.com/common/oauth2/v2.0/authorize
       ?client_id={ClientId}
       &redirect_uri={RedirectUri}
       &response_type=code
       &scope=Calendars.ReadWrite offline_access
       &state={userId}
       &prompt=consent
     ```
     - `offline_access` scope ensures a refresh token is returned
     - `common` tenant allows any Microsoft account (personal + organizational)
     - `state` parameter carries userId for CSRF protection and user identification
   - `HandleCallbackAsync`: Exchange authorization code for tokens:
     - POST to `https://login.microsoftonline.com/common/oauth2/v2.0/token` with code, client_id, client_secret, redirect_uri, grant_type=authorization_code, scope=Calendars.ReadWrite offline_access
     - Parse response for access_token, refresh_token, expires_in
     - Upsert CalendarIntegration record with Provider="Outlook" (encrypt tokens before storage)
     - Set IsConnected = true

3. **Implement OutlookCalendarService — token refresh**:
   - Before each API call, check `TokenExpiry` against `DateTime.UtcNow`
   - If expired, POST to `https://login.microsoftonline.com/common/oauth2/v2.0/token` with refresh_token, client_id, client_secret, grant_type=refresh_token, scope=Calendars.ReadWrite offline_access
   - On success: update AccessToken and TokenExpiry in CalendarIntegration
   - On failure (400/401 from Microsoft): set IsConnected = false, log warning, throw `CalendarTokenExpiredException`

4. **Implement OutlookCalendarService — CRUD operations**:
   - `CreateEventAsync`:
     ```csharp
     var graphClient = GetGraphClientForUser(userId); // builds GraphServiceClient with user's tokens
     var outlookEvent = new Microsoft.Graph.Models.Event
     {
         Subject = eventData.Title,
         Body = new ItemBody { ContentType = BodyType.Text, Content = eventData.Description },
         Start = new DateTimeTimeZone { DateTime = eventData.StartTime.ToString("o"), TimeZone = "UTC" },
         End = new DateTimeTimeZone { DateTime = eventData.EndTime.ToString("o"), TimeZone = "UTC" },
         Location = new Location { DisplayName = eventData.Location }
     };
     var created = await graphClient.Me.Events.PostAsync(outlookEvent);
     return created?.Id; // Store as Appointment.OutlookCalendarEventId
     ```
   - `UpdateEventAsync`:
     ```csharp
     var patchEvent = new Microsoft.Graph.Models.Event
     {
         Start = new DateTimeTimeZone { DateTime = eventData.StartTime.ToString("o"), TimeZone = "UTC" },
         End = new DateTimeTimeZone { DateTime = eventData.EndTime.ToString("o"), TimeZone = "UTC" }
     };
     await graphClient.Me.Events[eventId].PatchAsync(patchEvent);
     ```
   - `DeleteEventAsync`:
     ```csharp
     await graphClient.Me.Events[eventId].DeleteAsync();
     ```

5. **Implement rate limit handling (EC-1)**:
   ```csharp
   catch (ServiceException ex) when (ex.ResponseStatusCode == 429)
   {
       var retryAfter = ex.ResponseHeaders?
           .TryGetValues("Retry-After", out var values) == true
           ? int.Parse(values.First())
           : 60; // Default 60s if no header

       _logger.LogWarning(
           "Microsoft Graph rate limit hit for user {UserId}. Retry after {Seconds}s",
           userId, retryAfter);

       throw; // Rethrow for Hangfire retry mechanism to handle
   }
   ```
   - The Hangfire `[AutomaticRetry]` on CalendarSyncJob (from US_039/task_003) handles the retry timing
   - Rate limit info is logged for monitoring

6. **Build GraphServiceClient from stored tokens**:
   ```csharp
   private GraphServiceClient GetGraphClientForUser(Guid userId)
   {
       var integration = await _context.CalendarIntegrations
           .FirstOrDefaultAsync(c => c.UserId == userId && c.Provider == "Outlook" && c.IsConnected);

       if (integration == null)
           throw new InvalidOperationException("Outlook Calendar not connected");

       // Refresh token if expired
       if (integration.TokenExpiry <= DateTime.UtcNow)
           await RefreshTokenAsync(integration);

       var tokenCredential = new AccessTokenCredential(Decrypt(integration.AccessToken));
       return new GraphServiceClient(tokenCredential);
   }
   ```
   - Uses a simple token credential wrapper since tokens are managed manually
   - Decrypts stored access token before use

7. **Add OutlookCalendarSettings to appsettings.json**:
   ```json
   "OutlookCalendarSettings": {
       "TenantId": "common",
       "ClientId": "",
       "ClientSecret": "",
       "RedirectUri": "https://localhost:5001/api/calendar/outlook/callback",
       "Scopes": ["Calendars.ReadWrite", "offline_access"]
   }
   ```

8. **Implement IsConnectedAsync and DisconnectAsync**:
   - `IsConnectedAsync`: Query CalendarIntegration where UserId matches and Provider="Outlook" and IsConnected=true
   - `DisconnectAsync`: Set IsConnected=false, clear tokens from CalendarIntegration record
   - Both follow the same pattern as GoogleCalendarService but filtered by Provider="Outlook"

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Interfaces/
│   │   └── ICalendarService.cs         # FROM US_039/task_002 — provider-agnostic interface
│   ├── Services/
│   │   ├── GoogleCalendarService.cs    # FROM US_039/task_002 — Google implementation
│   │   └── (no OutlookCalendarService)
│   ├── DTOs/
│   │   └── CalendarEventDto.cs         # FROM US_039/task_002 — reused, not duplicated
│   └── PatientAccess.Business.csproj   # No Microsoft.Graph package
├── PatientAccess.Data/
│   └── Models/
│       ├── CalendarIntegration.cs      # FROM US_039/task_001 — supports Provider="Outlook"
│       └── Appointment.cs              # FROM US_040/task_001 — has OutlookCalendarEventId
├── PatientAccess.Web/
│   └── appsettings.json               # FROM US_039/task_002 — has GoogleCalendarSettings, no OutlookCalendarSettings
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/OutlookCalendarService.cs | Microsoft Graph API implementation of ICalendarService with OAuth2 and rate limit handling |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add OutlookCalendarSettings section (TenantId, ClientId, ClientSecret, RedirectUri, Scopes) |
| MODIFY | src/backend/PatientAccess.Business/PatientAccess.Business.csproj | Add Microsoft.Graph NuGet package reference |

## External References

- Microsoft Graph Calendar Events API: https://learn.microsoft.com/en-us/graph/api/user-post-events
- Microsoft Identity Platform OAuth2: https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow
- Microsoft.Graph NuGet SDK: https://www.nuget.org/packages/Microsoft.Graph
- Microsoft Graph Rate Limiting: https://learn.microsoft.com/en-us/graph/throttling

## Build Commands

```bash
cd src/backend
dotnet restore PatientAccess.Business
dotnet build PatientAccess.sln
```

## Implementation Validation Strategy

- [ ] OutlookCalendarService compiles and implements all 7 ICalendarService methods
- [ ] Microsoft.Graph NuGet package resolves and builds successfully
- [ ] OAuth2 authorization URL targets login.microsoftonline.com/common with correct scopes
- [ ] Callback handler exchanges code for tokens and stores encrypted in CalendarIntegration with Provider="Outlook"
- [ ] Token refresh updates AccessToken on expiry; marks IsConnected=false on refresh failure
- [ ] CreateEventAsync returns Microsoft Graph event ID for storage
- [ ] Rate limit handling catches HTTP 429 and logs Retry-After value (EC-1)
- [ ] OutlookCalendarSettings configuration section present in appsettings.json

## Implementation Checklist

- [ ] Add `Microsoft.Graph` NuGet package to PatientAccess.Business.csproj
- [ ] Create `OutlookCalendarService` implementing `ICalendarService` with Microsoft OAuth2 flow
- [ ] Implement token refresh via Microsoft identity platform token endpoint
- [ ] Implement calendar event CRUD (CreateEventAsync, UpdateEventAsync, DeleteEventAsync) using Graph SDK
- [ ] Implement rate limit handling: catch HTTP 429, log Retry-After, rethrow for Hangfire retry
- [ ] Implement IsConnectedAsync and DisconnectAsync filtered by Provider="Outlook"
- [ ] Build GraphServiceClient helper using decrypted stored tokens with expiry check
- [ ] Add OutlookCalendarSettings configuration section to appsettings.json
