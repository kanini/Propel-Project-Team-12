# Task - task_001_db_calendar_integration_schema

## Requirement Reference

- User Story: us_039
- Story Location: .propel/context/tasks/EP-005/us_039/us_039.md
- Acceptance Criteria:
  - AC-1: Calendar event created via Google Calendar API with appointment date, time, provider, location, and visit reason (requires GoogleCalendarEventId storage on Appointment)
  - AC-2: Google Calendar event updated on reschedule (requires GoogleCalendarEventId for lookup)
  - AC-3: Google Calendar event removed on cancel (requires GoogleCalendarEventId for lookup)
  - AC-4: OAuth2 flow for Google Calendar connection (requires CalendarIntegration entity for token storage)
- Edge Cases:
  - OAuth token expiry: Refresh tokens stored encrypted; if refresh fails, IsConnected is set to false and user is prompted to re-authorize (EC-2)

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

Create the database schema and EF Core model for calendar integration token storage and appointment-level event tracking. This adds a `CalendarIntegration` entity that stores per-user OAuth2 credentials (access token, refresh token, token expiry) keyed by calendar provider (e.g., "Google"), enabling multi-provider support for the upcoming Outlook integration (US_040). The `Appointment` model is extended with a nullable `GoogleCalendarEventId` field to enable O(1) event lookup for update and delete operations (AC-2, AC-3). OAuth tokens are stored with encryption markers in the EF Core configuration to ensure compliance with OWASP cryptographic failure prevention.

## Dependent Tasks

- None (foundational schema task)

## Impacted Components

- **NEW** `src/backend/PatientAccess.Data/Models/CalendarIntegration.cs` — Entity for per-user OAuth token storage per calendar provider
- **NEW** `src/backend/PatientAccess.Data/Configurations/CalendarIntegrationConfiguration.cs` — Fluent API: unique composite index on (UserId, Provider), column constraints
- **MODIFY** `src/backend/PatientAccess.Data/Models/Appointment.cs` — Add nullable `GoogleCalendarEventId` string field
- **MODIFY** `src/backend/PatientAccess.Data/PatientAccessDbContext.cs` — Add `DbSet<CalendarIntegration>`
- **MODIFY** `src/backend/PatientAccess.Data/Configurations/AppointmentConfiguration.cs` — Column config for GoogleCalendarEventId (varchar(256))

## Implementation Plan

1. **Create CalendarIntegration entity**:
   ```csharp
   namespace PatientAccess.Data.Models;

   public class CalendarIntegration
   {
       public Guid CalendarIntegrationId { get; set; }
       public Guid UserId { get; set; }
       public string Provider { get; set; } = string.Empty; // "Google", "Outlook"
       public string AccessToken { get; set; } = string.Empty; // Encrypted at rest
       public string RefreshToken { get; set; } = string.Empty; // Encrypted at rest
       public DateTime TokenExpiry { get; set; }
       public string? CalendarId { get; set; } // e.g., "primary" for Google
       public bool IsConnected { get; set; }
       public DateTime CreatedAt { get; set; }
       public DateTime? UpdatedAt { get; set; }

       // Navigation property
       public User User { get; set; } = null!;
   }
   ```
   - `Provider` column supports multi-provider: "Google" (US_039) and "Outlook" (US_040)
   - `AccessToken` and `RefreshToken` stored encrypted — EF Core value converter or application-level encryption
   - `IsConnected` tracks active connection status; set to false when refresh fails (EC-2)

2. **Create CalendarIntegrationConfiguration with Fluent API**:
   ```csharp
   builder.HasKey(c => c.CalendarIntegrationId);
   builder.HasIndex(c => new { c.UserId, c.Provider }).IsUnique();
   builder.Property(c => c.Provider).HasMaxLength(50).IsRequired();
   builder.Property(c => c.AccessToken).HasMaxLength(2048).IsRequired();
   builder.Property(c => c.RefreshToken).HasMaxLength(2048).IsRequired();
   builder.Property(c => c.CalendarId).HasMaxLength(256);
   builder.HasOne(c => c.User)
          .WithMany()
          .HasForeignKey(c => c.UserId)
          .OnDelete(DeleteBehavior.Cascade);
   ```
   - Unique composite index on (UserId, Provider) ensures one connection per provider per user
   - Token columns sized for encrypted JWT-like tokens (2048 max)
   - Cascade delete: when user deleted, calendar integrations are removed

3. **Add GoogleCalendarEventId to Appointment model**:
   ```csharp
   // In Appointment.cs — after existing PdfFilePath field
   public string? GoogleCalendarEventId { get; set; }
   ```
   - Nullable: legacy appointments and non-connected users will have null
   - Stores the Google Calendar API event ID returned on creation, used for update/delete

4. **Update AppointmentConfiguration for new field**:
   ```csharp
   builder.Property(a => a.GoogleCalendarEventId).HasMaxLength(256);
   ```
   - Google Calendar event IDs are typically ~26 chars, 256 provides safe margin

5. **Register DbSet and create EF Core migration**:
   - Add `public DbSet<CalendarIntegration> CalendarIntegrations { get; set; }` to PatientAccessDbContext
   - Apply CalendarIntegrationConfiguration in `OnModelCreating`
   - Generate migration: `dotnet ef migrations add AddCalendarIntegration`

## Current Project State

```
src/backend/
├── PatientAccess.Data/
│   ├── Models/
│   │   ├── Appointment.cs               # EXISTS — no GoogleCalendarEventId field
│   │   ├── User.cs                      # EXISTS — no calendar integration navigation
│   │   └── ...
│   ├── Configurations/
│   │   ├── AppointmentConfiguration.cs  # EXISTS — needs GoogleCalendarEventId column config
│   │   └── ...                          # No CalendarIntegrationConfiguration
│   └── PatientAccessDbContext.cs         # EXISTS — no DbSet<CalendarIntegration>
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/CalendarIntegration.cs | Entity with UserId, Provider, AccessToken, RefreshToken, TokenExpiry, IsConnected |
| CREATE | src/backend/PatientAccess.Data/Configurations/CalendarIntegrationConfiguration.cs | Fluent API with unique index on (UserId, Provider), token column constraints |
| MODIFY | src/backend/PatientAccess.Data/Models/Appointment.cs | Add nullable GoogleCalendarEventId string property |
| MODIFY | src/backend/PatientAccess.Data/Configurations/AppointmentConfiguration.cs | Add column config for GoogleCalendarEventId (varchar 256) |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add DbSet<CalendarIntegration>, apply configuration |

## External References

- EF Core Migrations: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations
- EF Core Fluent API: https://learn.microsoft.com/en-us/ef/core/modeling/relationships
- OWASP Cryptographic Failures: https://owasp.org/Top10/A02_2021-Cryptographic_Failures/

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.Data
dotnet ef migrations add AddCalendarIntegration --project PatientAccess.Data --startup-project PatientAccess.Web
```

## Implementation Validation Strategy

- [ ] CalendarIntegration entity compiles with all required properties
- [ ] Unique composite index on (UserId, Provider) prevents duplicate connections
- [ ] AccessToken and RefreshToken columns sized for encrypted tokens (2048)
- [ ] GoogleCalendarEventId added to Appointment as nullable string (varchar 256)
- [ ] DbSet<CalendarIntegration> registered in PatientAccessDbContext
- [ ] EF Core migration generates without errors

## Implementation Checklist

- [ ] Create `CalendarIntegration` entity with UserId, Provider, encrypted token fields, IsConnected flag
- [ ] Create `CalendarIntegrationConfiguration` with unique composite index and column constraints
- [ ] Add `GoogleCalendarEventId` nullable string property to `Appointment` model
- [ ] Update `AppointmentConfiguration` with column config for GoogleCalendarEventId
- [ ] Add `DbSet<CalendarIntegration>` to `PatientAccessDbContext` and apply configuration in `OnModelCreating`
