# Task - task_001_audit_log_entity

## Requirement Reference
- User Story: US_012
- Story Location: .propel/context/tasks/EP-DATA-I/us_012/us_012.md
- Acceptance Criteria:
    - AC-1: AuditLog entity contains ID, user reference (FK), timestamp, action type, resource type, resource ID, action details (JSONB), IP address
- Edge Cases:
    - Audit log insertion failures logged to application error logs without blocking main operations
    - High-volume audit writes require batched or async write patterns

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
Implement immutable AuditLog entity for HIPAA-compliant audit trail of all PHI access and modifications (DR-005). Entity uses JSONB for flexible action details storage and supports high-volume async writes. Immutability will be enforced via database triggers in task_002.

## Dependent Tasks
- us_009/task_001_user_entity_implementation — Requires User entity for author FK

## Impacted Components
- **NEW**: PatientAccess.Data/Models/AuditLog.cs — Audit log entity
- **NEW**: PatientAccess.Data/Configurations/AuditLogConfiguration.cs — Fluent API configuration
- **UPDATE**: PatientAccess.Data/PatientAccessDbContext.cs — Register AuditLogs DbSet
- **UPDATE**: PatientAccess.Data/Models/User.cs — Add AuditLogs navigation property

## Implementation Plan
1. **Define AuditLog entity** with all required fields per DR-005
2. **Configure JSONB column** for ActionDetails to store flexible audit metadata
3. **Implement AuditLogConfiguration** with FK to User, indexes for query performance
4. **Add navigation property** to User entity
5. **Configure indexes** for HIPAA compliance queries (UserId, Timestamp, ResourceType)
6. **Generate and apply migration**

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   │   ├── User.cs
│   │   └── [other entities]
│   ├── Configurations/
│   │   └── [existing configurations]
│   └── Migrations/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Models/AuditLog.cs | Immutable audit log entity |
| CREATE | src/backend/PatientAccess.Data/Configurations/AuditLogConfiguration.cs | Fluent API with JSONB column |
| MODIFY | src/backend/PatientAccess.Data/Models/User.cs | Add AuditLogs navigation property |
| MODIFY | src/backend/PatientAccess.Data/PatientAccessDbContext.cs | Add AuditLogs DbSet |
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddAuditLogEntity.cs | Migration file |

## External References
- [HIPAA Audit Requirements](https://www.hhs.gov/hipaa/for-professionals/security/laws-regulations/index.html)
- [PostgreSQL JSONB Type](https://www.postgresql.org/docs/16/datatype-json.html)
- [EF Core JSON Columns](https://learn.microsoft.com/en-us/ef/core/modeling/primitive-collections)

## Build Commands
- Generate migration: `dotnet ef migrations add AddAuditLogEntity --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Build project: `dotnet build src/backend/PatientAccess.sln`

## Implementation Validation Strategy
- [ ] Migration applies successfully
- [ ] AuditLogs table created with JSONB column for ActionDetails
- [ ] FK to User verified with nullable constraint (system actions may not have user)
- [ ] Indexes on Timestamp and ResourceType for audit queries
- [ ] JSONB column stores structured data correctly

## Implementation Checklist
- [ ] Create AuditLog entity with ID (GUID), UserId (FK, nullable), Timestamp, ActionType (string)
- [ ] Add ResourceType (string), ResourceId (GUID), ActionDetails (Dictionary for JSONB), IpAddress (string)
- [ ] Implement AuditLogConfiguration with table name, primary key, column types
- [ ] Configure ActionDetails as JSONB column using HasColumnType("jsonb")
- [ ] Configure FK to User with SET NULL delete behavior (preserve audit even if user deleted)
- [ ] Add index on Timestamp (descending) for chronological audit queries
- [ ] Add composite index on (ResourceType, ResourceId) for resource-specific audit lookups
- [ ] Update User entity with AuditLogs navigation property
- [ ] Register AuditLogs DbSet in PatientAccessDbContext
- [ ] Generate migration and verify JSONB column type
- [ ] Apply migration to database
