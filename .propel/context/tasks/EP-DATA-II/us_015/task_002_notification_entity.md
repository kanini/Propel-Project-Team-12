# Task - task_002_notification_entity

## Requirement Reference
- User Story: US_015
- Story Location: .propel/context/tasks/EP-DATA-II/us_015/us_015.md
- Acceptance Criteria:
    - AC-2: Notification entity contains ID, recipient reference (FK), appointment reference (FK nullable), channel type (enum: SMS/Email), template name, status (enum: Pending/Sent/Failed/Delivered), scheduled time, sent time, delivery confirmation, retry count, last error message
    - AC-3: Index on Notification (recipient_id, scheduled_time, status) for efficient pending notification queries
- Edge Cases:
    - Notification with missing channel configuration marked as Failed with error message
    - Retry logic managed via retry count field and status tracking

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
| Frontend | N/A | N/A |
| Backend | .NET 8 ASP.NET Core Web API | 8.0 |
| Database | PostgreSQL with pgvector | 16 |
| Library | Entity Framework Core | 8.0.x |

**Note**: All code and libraries MUST be compatible with versions above.

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
Implement Notification entity for multi-channel outbound messaging with delivery tracking, retry management, and failure diagnostics (DR-014). Entity supports FR-022 (appointment reminders), FR-026 (waitlist notifications), and NFR-017 (30-second delivery SLA with retry).

## Dependent Tasks
- us_009/task_001_user_entity_implementation — Requires User entity for recipient FK
- us_010/task_002_appointment_entity — Requires Appointment entity for optional appointment FK

## Impacted Components
- **NEW**: PatientAccess.Data/Models/ChannelType.cs — ChannelType enum
- **NEW**: PatientAccess.Data/Models/NotificationStatus.cs — NotificationStatus enum
- **NEW**: PatientAccess.Data/Models/Notification.cs — Notification entity
- **NEW**: PatientAccess.Data/Configurations/NotificationConfiguration.cs — Fluent API configuration
- **UPDATE**: PatientAccess.Data/PatientAccessDbContext.cs — Register Notifications DbSet

## Implementation Plan
1. **Create ChannelType enum** for SMS vs Email classification
2. **Create NotificationStatus enum** for delivery lifecycle tracking
3. **Define Notification entity** with scheduling, delivery tracking, and retry fields
4. **Implement NotificationConfiguration** with FKs to User (recipient) and Appointment (optional)
5. **Add composite index** on (RecipientId, ScheduledTime, Status) for pending notification queries
6. **Add indexes** for retry processing and delivery monitoring
7. **Generate and apply migration**

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   │   ├── User.cs
│   │   ├── Appointment.cs
│   │   ├── MedicalCode.cs (from task_001)
│   │   └── [other entities]
│   ├── Configurations/
│   │   └── [existing configurations]
│   └── Migrations/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/ChannelType.cs | Enum for notification channels |
| CREATE | src/backend/PatientAccess.Data/Models/NotificationStatus.cs | Enum for delivery status |
| CREATE | src/backend/PatientAccess.Data/Models/Notification.cs | Notification entity with retry fields |
| CREATE | src/backend/PatientAccess.Data/Configurations/NotificationConfiguration.cs | Fluent API with composite index |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add Notifications DbSet |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddNotificationEntity.cs | Migration file |

## External References
- [SMS Gateway Integration Patterns](https://www.twilio.com/docs/usage/webhooks)
- [Email Delivery Tracking](https://www.brevo.com/blog/email-deliverability/)
- [Retry Pattern Best Practices](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry)

## Build Commands
- Generate migration: `dotnet ef migrations add AddNotificationEntity --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Build project: `dotnet build src/backend/PatientAccess.sln`

## Implementation Validation Strategy
- [ ] Migration applies successfully
- [ ] Notifications table created with composite index on (RecipientId, ScheduledTime, Status)
- [ ] FK to User (recipient) and Appointment (optional) verified
- [ ] Index supports efficient queries for pending notifications scheduled in past
- [ ] RetryCount and LastErrorMessage fields support retry logic

## Implementation Checklist
- [ ] Define ChannelType enum (SMS=0, Email=1)
- [ ] Define NotificationStatus enum (Pending=0, Sent=1, Failed=2, Delivered=3)
- [ ] Create Notification entity with ID, RecipientId (FK), AppointmentId (FK nullable), ChannelType
- [ ] Add TemplateName, Status, ScheduledTime, SentTime, DeliveryConfirmationTime, RetryCount, LastErrorMessage fields
- [ ] Implement NotificationConfiguration with FK to User (CASCADE) and Appointment (SET NULL)
- [ ] Add composite index on (RecipientId, ScheduledTime, Status) for pending notification queries
- [ ] Add index on Status for filtering failed notifications needing retry
- [ ] Add index on ScheduledTime for chronological processing
- [ ] Register Notifications DbSet in PatientAccessDbContext
- [ ] Generate migration and verify composite index
- [ ] Apply migration to database
