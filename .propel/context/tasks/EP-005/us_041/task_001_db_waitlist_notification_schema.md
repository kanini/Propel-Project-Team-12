# Task - task_001_db_waitlist_notification_schema

## Requirement Reference

- User Story: us_041
- Story Location: .propel/context/tasks/EP-005/us_041/us_041.md
- Acceptance Criteria:
  - AC-1: Notification sent via preferred channel (requires tracking which slot was offered and when)
  - AC-2: Confirm books into slot, removes from waitlist (requires ResponseToken for secure link, NotifiedSlotId for slot reference)
  - AC-3: Decline keeps on waitlist, offers to next (requires ResponseToken for secure link)
  - AC-4: Timeout treated as decline (requires ResponseDeadline for timeout detection)
- Edge Cases:
  - Multiple patients on same slot: Sequential notification by priority timestamp (EC-1)
  - Slot re-booked before delivery: Availability check on confirm using NotifiedSlotId (EC-2)

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

Extend the `WaitlistEntry` model with fields required for the notification-confirm-decline-timeout lifecycle. Adds `NotifiedAt` (when notification was sent), `ResponseToken` (unique secure token for confirm/decline links), `ResponseDeadline` (when timeout expires, AC-4), and `NotifiedSlotId` (which specific slot was offered, for availability re-check on confirm per EC-2). The existing `WaitlistStatus.Notified` enum value already supports the Notified state — no enum changes needed.

## Dependent Tasks

- None (WaitlistEntry model and WaitlistStatus enum already exist from US_025)

## Impacted Components

- **MODIFY** `src/backend/PatientAccess.Data/Models/WaitlistEntry.cs` — Add 4 nullable fields for notification lifecycle
- **MODIFY** `src/backend/PatientAccess.Data/Configurations/WaitlistEntryConfiguration.cs` — Add column configs and unique index on ResponseToken

## Implementation Plan

1. **Add notification lifecycle fields to WaitlistEntry model**:
   ```csharp
   // In WaitlistEntry.cs — after existing fields
   public DateTime? NotifiedAt { get; set; }
   public string? ResponseToken { get; set; }
   public DateTime? ResponseDeadline { get; set; }
   public Guid? NotifiedSlotId { get; set; }

   // Navigation property for the offered slot
   public TimeSlot? NotifiedSlot { get; set; }
   ```
   - `NotifiedAt`: Timestamp when notification was dispatched. Null when Status=Active.
   - `ResponseToken`: Cryptographically random URL-safe token (32 bytes, base64url encoded). Used in confirm/decline links. Unique index ensures O(1) lookup.
   - `ResponseDeadline`: NotifiedAt + configured timeout (e.g., 30 minutes per AC-4). Null when not notified.
   - `NotifiedSlotId`: FK to the specific TimeSlot offered to this patient. Used for availability re-check on confirm (EC-2).

2. **Update WaitlistEntryConfiguration**:
   ```csharp
   builder.Property(w => w.ResponseToken).HasMaxLength(64);
   builder.HasIndex(w => w.ResponseToken).IsUnique()
       .HasFilter("\"ResponseToken\" IS NOT NULL");

   builder.Property(w => w.NotifiedSlotId);
   builder.HasOne(w => w.NotifiedSlot)
       .WithMany()
       .HasForeignKey(w => w.NotifiedSlotId)
       .OnDelete(DeleteBehavior.SetNull);

   // Index for timeout detection job: find Notified entries past deadline
   builder.HasIndex(w => new { w.Status, w.ResponseDeadline })
       .HasFilter("\"Status\" = 2"); // WaitlistStatus.Notified = 2
   ```
   - Unique filtered index on ResponseToken for O(1) lookup from confirm/decline URLs
   - Partial index on Status + ResponseDeadline for efficient timeout scanning
   - FK to TimeSlot with SetNull behavior (if slot deleted, entry remains but NotifiedSlotId cleared)

3. **Generate EF Core migration**:
   ```bash
   dotnet ef migrations add AddWaitlistNotificationFields --project PatientAccess.Data --startup-project PatientAccess.Web
   ```
   - Adds 4 nullable columns: NotifiedAt, ResponseToken (varchar 64), ResponseDeadline, NotifiedSlotId (FK)
   - Creates unique filtered index on ResponseToken
   - Creates partial index on (Status, ResponseDeadline)
   - Non-destructive: existing rows get null values

## Current Project State

```
src/backend/
├── PatientAccess.Data/
│   ├── Models/
│   │   ├── WaitlistEntry.cs            # EXISTS — has PatientId, ProviderId, PreferredDate*, NotificationPreference, Status, Priority
│   │   ├── WaitlistStatus.cs           # EXISTS — Active=1, Notified=2, Fulfilled=3, Cancelled=4
│   │   ├── NotificationPreference.cs   # EXISTS — Email=1, SMS=2, Both=3
│   │   ├── TimeSlot.cs                 # EXISTS — TimeSlotId, StartTime, IsBooked
│   │   └── ...
│   ├── Configurations/
│   │   ├── WaitlistEntryConfiguration.cs # EXISTS — current config without notification fields
│   │   └── ...
│   └── PatientAccessDbContext.cs
```

## Expected Changes

| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Data/Models/WaitlistEntry.cs | Add NotifiedAt, ResponseToken, ResponseDeadline, NotifiedSlotId fields and NotifiedSlot navigation |
| MODIFY | src/backend/PatientAccess.Data/Configurations/WaitlistEntryConfiguration.cs | Add column configs, unique filtered index on ResponseToken, partial index on (Status, ResponseDeadline), FK to TimeSlot |

## External References

- EF Core Filtered Indexes: https://learn.microsoft.com/en-us/ef/core/modeling/indexes#index-filter
- EF Core Migrations: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations

## Build Commands

```bash
cd src/backend
dotnet build PatientAccess.Data
dotnet ef migrations add AddWaitlistNotificationFields --project PatientAccess.Data --startup-project PatientAccess.Web
```

## Implementation Validation Strategy

- [ ] NotifiedAt, ResponseToken, ResponseDeadline, NotifiedSlotId added as nullable fields on WaitlistEntry
- [ ] Unique filtered index on ResponseToken ensures O(1) lookup for confirm/decline
- [ ] Partial index on (Status, ResponseDeadline) enables efficient timeout scanning
- [ ] FK relationship from NotifiedSlotId to TimeSlot with SetNull delete behavior
- [ ] EF Core migration generates without errors

## Implementation Checklist

- [ ] Add `NotifiedAt`, `ResponseToken`, `ResponseDeadline`, `NotifiedSlotId` nullable fields to `WaitlistEntry` model
- [ ] Add `NotifiedSlot` navigation property with FK to TimeSlot
- [ ] Configure `ResponseToken` as varchar(64) with unique filtered index in WaitlistEntryConfiguration
- [ ] Add partial index on `(Status, ResponseDeadline)` filtered to Notified status for timeout job
- [ ] Generate and verify EF Core migration `AddWaitlistNotificationFields`
