# Task - task_001_db_reminder_configuration_schema

## Requirement Reference

- User Story: us_037
- Story Location: .propel/context/tasks/EP-005/us_037/us_037.md
- Acceptance Criteria:
  - AC-4: Admin-configurable reminder intervals; future appointments use new intervals while already-scheduled reminders remain unchanged
- Edge Cases:
  - None directly applicable to this task (schema foundation only)

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
| Backend | .NET 8 ASP.NET Core Web API | .NET 8.0 |

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

Create the database schema foundation for the configurable reminder system. This includes a `SystemSettings` key-value table for storing admin-configurable reminder intervals, a `Cancelled` status in the `NotificationStatus` enum to support the appointment-cancellation edge case, a composite index on the `Notifications` table for efficient pending-reminder queries, seeding default reminder intervals (48h, 24h, 2h), and generating the EF Core migration. The key-value design allows admin-configurable intervals without future schema changes (AC-4).

## Dependent Tasks

- None (schema foundation task)

## Impacted Components

- **NEW** `src/backend/PatientAccess.Data/Models/SystemSetting.cs` — Key-value entity for system configuration
- **NEW** `src/backend/PatientAccess.Data/Configurations/SystemSettingConfiguration.cs` — EF Core Fluent API configuration for SystemSettings table
- **MODIFY** `src/backend/PatientAccess.Data/Models/NotificationStatus.cs` — Add `Cancelled = 5` enum value
- **MODIFY** `src/backend/PatientAccess.Data/PatientAccessDbContext.cs` — Add `DbSet<SystemSetting>` and seed default reminder intervals
- **NEW** `src/backend/PatientAccess.Data/Migrations/<timestamp>_AddSystemSettingsAndReminderSupport.cs` — Generated migration

## Implementation Plan

1. **Create SystemSetting entity model**:
   ```csharp
   public class SystemSetting
   {
       public Guid SystemSettingId { get; set; }
       public string Key { get; set; } = string.Empty;
       public string Value { get; set; } = string.Empty;
       public string? Description { get; set; }
       public DateTime CreatedAt { get; set; }
       public DateTime? UpdatedAt { get; set; }
   }
   ```
   Simple key-value store. Keys use dot-notation convention (e.g., `Reminder.Intervals`, `Reminder.SmsEnabled`, `Reminder.EmailEnabled`).

2. **Create SystemSettingConfiguration**:
   - Table name: `SystemSettings`
   - Primary key: `SystemSettingId` with `gen_random_uuid()` default
   - `Key`: required, max 200 chars, unique index for fast lookups
   - `Value`: required, max 2000 chars (accommodates JSON arrays like `[48, 24, 2]`)
   - `Description`: optional, text type
   - `CreatedAt`: required, timestamptz, default `NOW()`
   - `UpdatedAt`: optional, timestamptz

3. **Add Cancelled status to NotificationStatus enum**:
   ```csharp
   public enum NotificationStatus
   {
       Pending = 1,
       Sent = 2,
       Failed = 3,
       Delivered = 4,
       Cancelled = 5
   }
   ```
   Required for the edge case: "appointment cancelled after reminders are scheduled — pending reminders are cancelled."

4. **Add composite index on Notifications table**:
   Add to `NotificationConfiguration`:
   ```csharp
   builder.HasIndex(n => new { n.Status, n.ScheduledTime })
       .HasDatabaseName("IX_Notifications_Status_ScheduledTime");
   ```
   Optimizes the recurring job query: `WHERE Status = Pending AND ScheduledTime <= NOW()`.

5. **Register DbSet and seed data in PatientAccessDbContext**:
   - Add `public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();`
   - Seed default values in `OnModelCreating`:
     - `Reminder.Intervals` = `[48, 24, 2]` (hours before appointment)
     - `Reminder.SmsEnabled` = `true`
     - `Reminder.EmailEnabled` = `true`

6. **Generate EF Core migration**:
   ```bash
   dotnet ef migrations add AddSystemSettingsAndReminderSupport --project PatientAccess.Data --startup-project PatientAccess.Web
   ```

## Current Project State

```
src/backend/
├── PatientAccess.Data/
│   ├── Models/
│   │   ├── Notification.cs              # EXISTS — uses NotificationStatus enum
│   │   ├── NotificationStatus.cs        # EXISTS — Pending, Sent, Failed, Delivered
│   │   ├── ChannelType.cs               # EXISTS — SMS=1, Email=2
│   │   └── ...
│   ├── Configurations/
│   │   ├── NotificationConfiguration.cs # EXISTS — table config, needs index addition
│   │   └── ...
│   ├── PatientAccessDbContext.cs         # EXISTS — has DbSet<Notification>
│   └── Migrations/
│       └── ...
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/SystemSetting.cs | Key-value entity for system configuration |
| CREATE | src/backend/PatientAccess.Data/Configurations/SystemSettingConfiguration.cs | Fluent API config with unique key index |
| MODIFY | src/backend/PatientAccess.Data/Models/NotificationStatus.cs | Add `Cancelled = 5` enum value |
| MODIFY | src/backend/PatientAccess.Data/Configurations/NotificationConfiguration.cs | Add composite index on (Status, ScheduledTime) |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add DbSet and seed default reminder settings |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddSystemSettingsAndReminderSupport.cs | Generated migration |

## External References

- EF Core 8.0 Migrations: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations
- EF Core 8.0 Data Seeding: https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
- PostgreSQL Index Types: https://www.postgresql.org/docs/16/indexes-types.html

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.Data
dotnet ef migrations add AddSystemSettingsAndReminderSupport --project PatientAccess.Data --startup-project PatientAccess.Web
dotnet ef database update --project PatientAccess.Data --startup-project PatientAccess.Web
```

## Implementation Validation Strategy

- [ ] Migration generates without errors
- [ ] `SystemSettings` table created with unique index on `Key`
- [ ] Default seed data inserted (3 reminder settings)
- [ ] `NotificationStatus` enum includes `Cancelled = 5`
- [ ] Composite index `IX_Notifications_Status_ScheduledTime` created on `Notifications` table
- [ ] Database update applies cleanly (forward migration)
- [ ] Rollback migration works (verify with `dotnet ef migrations remove`)

## Implementation Checklist

- [ ] Create `SystemSetting.cs` entity model with key-value structure
- [ ] Create `SystemSettingConfiguration.cs` with unique key index and column constraints
- [ ] Add `Cancelled = 5` to `NotificationStatus` enum
- [ ] Add composite index `(Status, ScheduledTime)` to `NotificationConfiguration`
- [ ] Register `DbSet<SystemSetting>` and seed default reminder intervals in `PatientAccessDbContext`
- [ ] Generate and verify EF Core migration
