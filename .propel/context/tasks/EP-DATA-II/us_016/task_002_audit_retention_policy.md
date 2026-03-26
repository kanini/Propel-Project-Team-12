# Task - task_002_audit_retention_policy

## Requirement Reference
- User Story: US_016
- Story Location: .propel/context/tasks/EP-DATA-II/us_016/us_016.md
- Acceptance Criteria:
    - AC-3: Scheduled job or database policy prevents deletion of audit logs younger than 7 years; logs older than 7 years may be archived (DR-007 HIPAA compliance)
- Edge Cases:
    - Audit logs exceeding storage limits require archival strategyto cold storage
    - Retention policy must be enforced at database level to prevent accidental deletion

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
| Library | Quartz.NET or Hangfire | Latest stable |

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
Implement HIPAA-compliant 7-year audit log retention policy using database trigger to prevent premature deletion plus optional scheduled archival job for logs older than 7 years (DR-007). Policy ensures compliance with healthcare audit trail requirements while managing storage growth.

## Dependent Tasks
- us_012/task_001_audit_log_entity — Requires AuditLog table to exist
- us_012/task_002_audit_immutability_triggers — Requires immutability triggers

## Impacted Components
- **NEW**: src/backend/PatientAccess.Data/Migrations/*_AddAuditRetentionTrigger.cs — Migration with retention trigger
- **NEW**: docs/AUDIT_RETENTION.md — Audit retention policy documentation
- **OPTIONAL**: src/backend/PatientAccess.Web/Jobs/AuditArchivalJob.cs — Background job for archival (if Hangfire/Quartz used)

## Implementation Plan
1. **Create database trigger** that prevents DELETE on AuditLogs where Timestamp > NOW() - INTERVAL '7 years'
2. **Write trigger function** that raises exception for premature deletion attempts
3. **Document archival strategy** for logs older than 7 years (manual export or automated archival)
4. **Optional: Implement scheduled job** for automated archival to cold storage (S3/Azure Blob)
5. **Create migration for trigger** with Up/Down methods
6. **Document compliance verification** procedures for auditors

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   ├── Models/
│   │   └── AuditLog.cs (from US_012)
│   ├── Configurations/
│   │   └── AuditLogConfiguration.cs
│   └── Migrations/
│       ├── *_AddAuditLogEntity.cs
│       └── *_AddAuditLogImmutabilityTriggers.cs
docs/
└── [existing documentation]
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Migrations/*_AddAuditRetentionTrigger.cs | Migration with retention trigger DDL |
| CREATE | docs/AUDIT_RETENTION.md | Retention policy and archival procedures |
| UPDATE | Database | Add BEFORE DELETE trigger on AuditLogs table |

## External References
- [HIPAA Audit Log Requirements](https://www.hhs.gov/hipaa/for-professionals/security/laws-regulations/index.html)
- [PostgreSQL Date/Time Functions](https://www.postgresql.org/docs/16/functions-datetime.html)
- [Data Archival Best Practices](https://learn.microsoft.com/en-us/azure/architecture/best-practices/data-partitioning)

## Build Commands
- Create empty migration: `dotnet ef migrations add AddAuditRetentionTrigger --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Apply migration: `dotnet ef database update --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Test trigger: `psql -d <database> -c \"DELETE FROM \\\"AuditLogs\\\" WHERE \\\"Timestamp\\\" > NOW() - INTERVAL '6 years'\"`

## Implementation Validation Strategy
- [ ] Migration applied successfully
- [ ] Trigger function enforce_audit_retention exists
- [ ] DELETE attempt on recent audit log (< 7 years) raises exception with retention policy message
- [ ] DELETE attempt on old audit log (> 7 years) succeeds (manual archival path)
- [ ] Documentation reviewed for compliance procedures

## Implementation Checklist
- [ ] Generate empty migration using dotnet ef migrations add command
- [ ] Write PostgreSQL function enforce_audit_retention() that checks age before allowing DELETE
- [ ] Add BEFORE DELETE trigger on AuditLogs linking to enforcement function
- [ ] Trigger logic: IF (OLD."Timestamp" > NOW() - INTERVAL '7 years') THEN RAISE EXCEPTION
- [ ] Write Down() method to drop trigger and function for rollback
- [ ] Create docs/AUDIT_RETENTION.md documenting 7-year policy
- [ ] Document manual archival procedure: Export logs > 7 years to compressed CSV
- [ ] Optional: Document automated archival job configuration if implementing background worker
- [ ] Apply migration to development database
- [ ] Test restriction: Attempt DELETE on recent logs and verify exception
- [ ] Test archival path: Attempt DELETE on ancient logs (if any) and verify success
