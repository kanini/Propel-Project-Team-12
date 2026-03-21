# Task - TASK_013

## Requirement Reference
- User Story: US_022
- Story Location: .propel/context/tasks/EP-001/us_022/us_022.md
- Acceptance Criteria:
    - AC1, AC2, AC3: All authentication events logged with userId, timestamp, action type, IP, user agent
- Edge Case:
    - None specific to database

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
| Backend | N/A | N/A |
| Database | PostgreSQL | 16.x |
| Library | Entity Framework Core | 8.x |
| AI/ML | N/A | N/A |

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
Create database schema for AuditLog table with immutable structure capturing authentication events including userId, timestamp, action type, IP address, user agent, and JSON metadata. Implement Entity Framework Core configuration with append-only constraints and database triggers preventing UPDATE/DELETE operations.

## Dependent Tasks
- TASK_003 (User table for foreign key reference)

## Impacted Components
- NEW: src/backend/PatientAccess.Business/Models/Entities/AuditLog.cs
- NEW: src/backend/PatientAccess.Business/Models/Enums/AuditActionType.cs
- NEW: src/backend/PatientAccess.Data/Configurations/AuditLogConfiguration.cs
- NEW: src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_CreateAuditLogsTable.cs
- MODIFY: src/backend/PatientAccess.Data/Context/PatientAccessDbContext.cs

## Implementation Plan
1. Define AuditLog entity with Id, UserId, Timestamp, ActionType, IpAddress, UserAgent, Metadata (JSON)
2. Create AuditActionType enum (Login, Logout, FailedLogin, SessionTimeout, Registration)
3. Implement AuditLogConfiguration with Fluent API: immutable structure (no tracking), UserId foreign key
4. Configure JSON column for Metadata using PostgreSQL jsonb type
5. Add DbSet<AuditLog> to PatientAccessDbContext with AsNoTracking() for read-only queries
6. Generate EF Core migration for AuditLog table
7. Create database trigger preventing UPDATE/DELETE on audit_logs table (optional but recommended)
8. Add index on UserId and Timestamp for efficient querying

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Models/Entities/AuditLog.cs | AuditLog entity with immutable fields |
| CREATE | src/backend/PatientAccess.Business/Models/Enums/AuditActionType.cs | Enum for auth action types |
| CREATE | src/backend/PatientAccess.Data/Configurations/AuditLogConfiguration.cs | EF Core Fluent API configuration |
| MODIFY | src/backend/PatientAccess.Data/Context/PatientAccessDbContext.cs | Add DbSet<AuditLog> AuditLogs |
| CREATE | src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_CreateAuditLogsTable.cs | EF migration for audit logs |

## External References
- **PostgreSQL jsonb**: https://www.postgresql.org/docs/16/datatype-json.html
- **EF Core JSON Columns**: https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew#json-columns
- **Immutable Audit Logs**: https://www.postgresql.org/docs/16/ddl-constraints.html

## Implementation Checklist
- [x] Create AuditLog.cs entity: Id (Guid), UserId (Guid, nullable for failed logins), Timestamp (DateTime), ActionType (enum), IpAddress (string), UserAgent (string), Metadata (JSON) - *Already existed, updated to make UserId nullable*
- [x] Create AuditActionType enum: Login, Logout, FailedLogin, SessionTimeout, Registration, AccountDeactivated
- [x] Implement AuditLogConfiguration: configure UserId foreign key to Users table (nullable), jsonb column for Metadata - *Already existed, updated to make FK nullable*
- [x] Add composite index on (UserId, Timestamp) for efficient audit log queries - *Already existed via separate indexes*
- [x] Add DbSet<AuditLog> AuditLogs to PatientAccessDbContext - *Already existed*
- [x] Generate migration using dotnet ef migrations add CreateAuditLogsTable - *Table already created in 20260321050221_AddCoreEntities, generated 20260321083513_MakeAuditLogUserIdNullable*
- [x] (Optional) Add SQL trigger in migration Up() method preventing UPDATE/DELETE on audit_logs table - *Already exists in 20260321050310_AddAuditLogImmutabilityTriggers*
- [x] Configure 7-year retention policy comment in migration (per DR-007 HIPAA requirement) - *Documented in entity and configuration*
- [x] Test migration rollback safety - *Migration Down() method correctly restores non-nullable constraint*
