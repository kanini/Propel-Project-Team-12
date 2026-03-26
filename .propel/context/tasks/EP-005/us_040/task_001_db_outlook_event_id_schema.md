# Task - task_001_db_outlook_event_id_schema

## Requirement Reference

- User Story: us_040
- Story Location: .propel/context/tasks/EP-005/us_040/us_040.md
- Acceptance Criteria:
  - AC-1: Outlook Calendar event created via Microsoft Graph API on booking (requires OutlookCalendarEventId storage on Appointment)
  - AC-2: Outlook Calendar event updated on reschedule (requires OutlookCalendarEventId for lookup)
  - AC-3: Outlook Calendar event removed on cancel (requires OutlookCalendarEventId for lookup)
- Edge Cases:
  - Both Google and Outlook connected: Events created in both calendars independently; Appointment stores separate event IDs per provider (EC-2)

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
| Database | PostgreSQL with pgvector | PostgreSQL 16, pgvector 0.5+ |
| Backend | .NET 8 ASP.NET Core Web API (EF Core) | .NET 8.0 |

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

Extend the `Appointment` model with a nullable `OutlookCalendarEventId` field for storing Microsoft Graph API event identifiers. This mirrors the `GoogleCalendarEventId` field added by US_039 and enables O(1) event lookup for Outlook Calendar update and delete operations (AC-2, AC-3). The `CalendarIntegration` entity from US_039 already supports the "Outlook" provider via its `Provider` column — no entity changes needed. Only the Appointment model, its EF Core configuration, and a new migration are required.

## Dependent Tasks

- EP-005/us_039/task_001_db_calendar_integration_schema — Provides CalendarIntegration entity with multi-provider support and GoogleCalendarEventId on Appointment

## Impacted Components

- **MODIFY** `src/backend/PatientAccess.Data/Models/Appointment.cs` — Add nullable `OutlookCalendarEventId` string field
- **MODIFY** `src/backend/PatientAccess.Data/Configurations/AppointmentConfiguration.cs` — Column config for OutlookCalendarEventId (varchar 256)

## Implementation Plan

1. **Add OutlookCalendarEventId to Appointment model**:
   ```csharp
   // In Appointment.cs — after GoogleCalendarEventId field (added by US_039)
   public string? OutlookCalendarEventId { get; set; }
   ```
   - Nullable: existing appointments and non-Outlook-connected users will have null
   - Stores the Microsoft Graph API event ID returned on creation, used for update/delete
   - Microsoft Graph event IDs are typically ~120 chars (base64 encoded), 256 provides safe margin

2. **Update AppointmentConfiguration for new field**:
   ```csharp
   builder.Property(a => a.OutlookCalendarEventId).HasMaxLength(256);
   ```
   - Matches the GoogleCalendarEventId configuration pattern from US_039

3. **Generate EF Core migration**:
   ```bash
   dotnet ef migrations add AddOutlookCalendarEventId --project PatientAccess.Data --startup-project PatientAccess.Web
   ```
   - Migration adds a single nullable varchar(256) column to the Appointments table
   - Non-destructive: existing rows get null value

4. **Verify CalendarIntegration already supports "Outlook"**:
   - The `CalendarIntegration.Provider` column from US_039/task_001 accepts any string value
   - The unique composite index on (UserId, Provider) ensures one Outlook connection per user
   - No changes needed to CalendarIntegration entity — confirmed multi-provider ready

## Current Project State

```
src/backend/
├── PatientAccess.Data/
│   ├── Models/
│   │   ├── Appointment.cs              # FROM US_039/task_001 — has GoogleCalendarEventId, no OutlookCalendarEventId
│   │   ├── CalendarIntegration.cs      # FROM US_039/task_001 — Provider supports "Google"/"Outlook"
│   │   └── ...
│   ├── Configurations/
│   │   ├── AppointmentConfiguration.cs # FROM US_039/task_001 — GoogleCalendarEventId configured
│   │   ├── CalendarIntegrationConfiguration.cs # FROM US_039/task_001 — unique (UserId, Provider)
│   │   └── ...
│   └── PatientAccessDbContext.cs        # FROM US_039/task_001 — DbSet<CalendarIntegration> registered
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Data/Models/Appointment.cs | Add nullable OutlookCalendarEventId string property |
| MODIFY | src/backend/PatientAccess.Data/Configurations/AppointmentConfiguration.cs | Add column config for OutlookCalendarEventId (varchar 256) |

## External References

- EF Core Migrations: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations
- Microsoft Graph Event Resource: https://learn.microsoft.com/en-us/graph/api/resources/event

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.Data
dotnet ef migrations add AddOutlookCalendarEventId --project PatientAccess.Data --startup-project PatientAccess.Web
```

## Implementation Validation Strategy

- [ ] OutlookCalendarEventId added to Appointment as nullable string (varchar 256)
- [ ] AppointmentConfiguration includes column config for OutlookCalendarEventId
- [ ] EF Core migration generates without errors
- [ ] Existing CalendarIntegration entity confirmed to support "Outlook" provider without changes

## Implementation Checklist

- [x] Add `OutlookCalendarEventId` nullable string property to `Appointment` model after `GoogleCalendarEventId`
- [x] Update `AppointmentConfiguration` with `HasMaxLength(256)` for OutlookCalendarEventId
- [x] Generate and verify EF Core migration `AddOutlookCalendarEventId`
- [x] Confirm CalendarIntegration entity supports "Outlook" provider value (no changes needed)
