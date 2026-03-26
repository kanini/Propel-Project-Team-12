# Task - task_001_backup_and_migration_infrastructure

## Requirement Reference
- User Story: US_013
- Story Location: .propel/context/tasks/EP-DATA-I/us_013/us_013.md
- Acceptance Criteria:
    - AC-1: Point-in-time recovery enabled on Supabase with 15-minute granularity recovery window
    - AC-2: All migrations listed in chronological order with timestamps and descriptive names
    - AC-3: Migrations use non-breaking schema change patterns (nullable-first, backfill, then constraint)
    - AC-4: Migration rollback using dotnet ef database update <previous_migration> reverts cleanly
- Edge Cases:
    - Failed migration mid-execution rolls back automatically via transaction
    - Backup failure recoverable via Supabase PITR within retention window

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
| Database | PostgreSQL with pgvector @ Supabase | 16 |
| Library | Entity Framework Core | 8.0.x |
| Infrastructure | Supabase | N/A |

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
Configure Supabase point-in-time recovery (PITR) for 15-minute granularity backup with 7-day retention (DR-008) and establish zero-downtime migration best practices using nullable-first pattern for schema evolution (DR-009). Document migration workflows and rollback procedures for production deployments.

## Dependent Tasks
- None — Infrastructure and operational configuration task

## Impacted Components
- **NEW**: docs/MIGRATIONS.md — Migration best practices documentation
- **UPDATE**: Supabase project settings — Enable PITR with 15-minute granularity
- **NEW**: scripts/verify-backup-config.sql — SQL script to verify backup configuration
- **NEW**: docs/BACKUP_RECOVERY.md — Backup and recovery procedures documentation

## Implementation Plan
1. **Verify Supabase PITR configuration** via Supabase dashboard (Settings > Database > Backups)
2. **Confirm 15-minute recovery granularity** and adequate retention period (minimum 7 days)
3. **Document zero-downtime migration pattern**:
   - Phase 1: Add nullable column
   - Phase 2: Backfill data via background job
   - Phase 3: Add NOT NULL constraint
4. **Create migration naming convention** guide (YYYYMMDDHHMMSS_DescriptiveAction)
5. **Write migration rollback procedures** with testing checklist
6. **Create backup verification SQL script** for monitoring PITR availability
7. **Document recovery procedures** for various failure scenarios

## Current Project State
```
src/backend/
├── PatientAccess.Data/
│   └── Migrations/
│       └── [all existing migrations]
docs/
└── [existing documentation]
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | docs/MIGRATIONS.md | Migration best practices and patterns |
| CREATE | docs/BACKUP_RECOVERY.md | Backup configuration and recovery procedures |
| CREATE | scripts/verify-backup-config.sql | SQL script to check PITR status |
| UPDATE | Supabase Dashboard | Enable PITR with 15-minute granularity |

## External References
- [Supabase Backup Documentation](https://supabase.com/docs/guides/platform/backups)
- [Zero-Downtime Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli#zero-downtime-migrations)
- [EF Core Migration Best Practices](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [PostgreSQL PITR](https://www.postgresql.org/docs/16/continuous-archiving.html)

## Build Commands
- List migrations: `dotnet ef migrations list --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Rollback migration: `dotnet ef database update <TargetMigration> --project src/backend/PatientAccess.Data --startup-project src/backend/PatientAccess.Web`
- Verify backup: `psql -d <database> -f scripts/verify-backup-config.sql`

## Implementation Validation Strategy
- [ ] Supabase PITR enabled and verified via dashboard
- [ ] 15-minute granularity confirmed in backup settings
- [ ] Retention period set to minimum 7 days  
- [ ] Migration list command shows all migrations in chronological order
- [ ] Rollback test: Revert one migration and verify schema state
- [ ] Documentation reviewed with zero-downtime pattern examples

## Implementation Checklist
- [ ] Log into Supabase dashboard and navigate to project Database settings
- [ ] Enable Point-in-Time Recovery (PITR) if not already enabled
- [ ] Configure recovery granularity to 15 minutes (minimum)
- [ ] Set retention period to 7 days minimum (HIPAA DR-007 requires longer archival separately)
- [ ] Create docs/MIGRATIONS.md with zero-downtime migration patterns and examples
- [ ] Document nullable-first pattern: Add column (nullable) -> Backfill -> Add constraint
- [ ] Create docs/BACKUP_RECOVERY.md with PITR recovery procedures
- [ ] Write scripts/verify-backup-config.sql to query pg_stat_archiver for backup status
- [ ] Test migration rollback procedure on development database
- [ ] Add migration naming convention to docs/MIGRATIONS.md
- [ ] Document when to use custom SQL migrations vs EF Core auto-generated migrations
