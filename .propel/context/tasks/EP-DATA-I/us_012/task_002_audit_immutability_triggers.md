# Task - task_002_audit_immutability_triggers

## Requirement Reference
- User Story: US_012
- Story Location: .propel/context/tasks/EP-DATA-I/us_012/us_012.md
- Acceptance Criteria:
    - AC-2: Database triggers exist on AuditLogs table that raise exceptions on UPDATE or DELETE operations (AD-007 immutability)
    - AC-3: Foreign key constraints link appointments, documents, intake records, waitlist entries to patient User record (DR-006)
    - AC-4: UPDATE or DELETE attempts on audit records fail with database-level exception
- Edge Cases:
    - Audit log insertion failures should not block main operations
    - FK constraints prevent orphaned patient-related records

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
Implement database-level enforcement of audit log immutability through PostgreSQL triggers that prevent UPDATE and DELETE operations (AD-007). Verify and document referential integrity constraints across all patient-related entities per DR-006 requirements.

## Dependent Tasks
- task_001_audit_log_entity — Requires AuditLog table to exist
- All entity tasks from US_009-US_011 — Requires all FK relationships to be established

## Impacted Components
- **NEW**: src/backend/PatientAccess.Data/Migrations/*_AddAuditLogImmutabilityTriggers.cs — Migration with trigger SQL
- **UPDATE**: Database — Add BEFORE UPDATE/DELETE triggers on AuditLogs table

## Implementation Plan
1. **Create custom migration** for trigger creation (EF Core doesn't generate triggers automatically)
2. **Write PostgreSQL trigger function** that raises exception on UPDATE/DELETE attempts
3. **Create BEFORE UPDATE trigger** on AuditLogs table
4. **Create BEFORE DELETE trigger** on AuditLogs table
5. **Verify referential integrity** across all patient-related entities (Appointments, ClinicalDocuments, etc.)
6. **Write migration Down method** to drop triggers on rollback
7. **Test trigger enforcement** via SQL to confirm immutability

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── PatientAccessDbContext.cs
│   ├── Models/
│   │   ├── AuditLog.cs (from task_001)
│   │   └── [all other entities]
│   ├── Configurations/
│   │   └── [all configurations with FK constraints]
│   └── Migrations/
│       └── *_AddAuditLogEntity.cs (from task_001)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddAuditLogImmutabilityTriggers.cs | Custom migration with trigger DDL |
| UPDATE | Database | Add trigger function and triggers on AuditLogs table |

## External References
- [PostgreSQL Triggers Documentation](https://www.postgresql.org/docs/16/trigger-definition.html)
- [HIPAA Audit Trail Requirements](https://www.hhs.gov/hipaa/for-professionals/security/guidance/audit-controls/index.html)
- [EF Core Raw SQL Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli#arbitrary-changes-via-raw-sql)

## Build Commands
- Create empty migration: `dotnet ef migrations add AddAuditLogImmutabilityTriggers --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Test trigger: `psql -d <database> -c \"UPDATE \\\"AuditLogs\\\" SET \\\"ActionType\\\"='test' WHERE \\\"AuditLogId\\\"=(SELECT \\\"AuditLogId\\\" FROM \\\"AuditLogs\\\" LIMIT 1)\"`

## Implementation Validation Strategy
- [ ] Migration generated and contains SQL for trigger creation
- [ ] Migration applies successfully  
- [ ] Trigger function prevent_audit_mutation exists in database
- [ ] UPDATE attempt on AuditLogs raises exception with message \"Audit logs are immutable\"
- [ ] DELETE attempt on AuditLogs raises exception with message \"Audit logs are immutable\"
- [ ] Foreign key constraints verified on all patient-related tables

## Implementation Checklist
- [ ] Generate empty migration using dotnet ef migrations add command
- [ ] Write PostgreSQL function prevent_audit_mutation() that RAISES EXCEPTION
- [ ] Add migrationBuilder.Sql() in Up() method to create trigger function
- [ ] Add BEFORE UPDATE trigger linking to prevent_audit_mutation function
- [ ] Add BEFORE DELETE trigger linking to prevent_audit_mutation function
- [ ] Write Down() method SQL to drop triggers and function for rollback support
- [ ] Verify all FK constraints exist: Appointments->User, ClinicalDocuments->User, etc.
- [ ] Apply migration to development database
- [ ] Test UPDATE on AuditLogs table and verify exception is raised
- [ ] Test DELETE on AuditLogs table and verify exception is raised
- [ ] Document trigger behavior in migration comments
