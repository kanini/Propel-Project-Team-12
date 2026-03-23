# Task - task_001_db_audit_log_schema

## Requirement Reference
- User Story: US_055
- Story Location: .propel/context/tasks/EP-010/us_055/us_055.md
- Acceptance Criteria:
    - **AC1**: Given any user performs an auditable action (FR-040), When the action completes, Then an audit record is created within 200ms containing actor ID, action type, target resource, timestamp (UTC), IP address, and result (success/failure).
    - **AC2**: Given the immutability requirement, When an audit record is written, Then it is append-only with no UPDATE or DELETE operations permitted at the database level (enforced via PostgreSQL row-level security policies).
- Edge Case:
    - How does the system handle audit log storage growth? Logs older than the retention period are archived to cold storage, not deleted, maintaining immutability guarantees.

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

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | N/A | N/A |
| Backend | N/A | N/A |
| Database | PostgreSQL | 16.x |
| Database | Entity Framework Core | 8.0 |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

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

> **AI Impact Legend:**
> - **Yes**: Task involves LLM integration, RAG pipeline, prompt engineering, or AI infrastructure
> - **No**: Task is deterministic (FE/BE/DB only)
>
> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Create immutable audit log database schema with PostgreSQL row-level security (RLS) policies enforcing append-only operations (AC2, AD-007). This task implements AuditLog table capturing actor ID, action type, target resource, timestamp (UTC), IP address, and result per AC1, creates PostgreSQL RLS policies preventing UPDATE/DELETE operations at database level (AC2), configures Entity Framework Core mapping for AuditLog entity with read-only DbSet, creates database indexes for query performance (date range, user ID, action type per NFR-007), implements archive strategy for logs older than retention period (7 years per DR-007, edge case), and adds database migration for schema deployment. Features immutability guarantees via PostgreSQL triggers and RLS policies, HIPAA retention compliance (DR-007), performance optimization with composite indexes, and archive table for cold storage without deletion.

**Key Capabilities:**
- AuditLog table with fields: Id, UserId, ActionType, ResourceType, ResourceId, Timestamp, IpAddress, Result, ActionDetails (JSONB)
- PostgreSQL RLS policies preventing UPDATE/DELETE (AC2)
- Database trigger preventing modifications (double enforcement)
- Composite indexes for query performance: (Timestamp DESC), (UserId, Timestamp), (ActionType, Timestamp), (ResourceType, ResourceId, Timestamp)
- AuditLogArchive table for 7-year retention (DR-007, edge case)
- Entity Framework Core configuration with read-only DbSet
- Database migration script with rollback support

## Dependent Tasks
- None (foundational schema)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Data/Entities/AuditLog.cs` - Audit log entity
- **NEW**: `src/backend/PatientAccess.Data/Configurations/AuditLogConfiguration.cs` - EF Core configuration
- **NEW**: `src/backend/scripts/migrations/create_audit_log_schema.sql` - Database schema migration
- **NEW**: `src/backend/scripts/migrations/create_audit_rls_policies.sql` - RLS policies script
- **MODIFY**: `src/backend/PatientAccess.Data/Context/PatientAccessDbContext.cs` - Add AuditLogs DbSet

## Implementation Plan

1. **Create AuditLog Entity**
   - File: `src/backend/PatientAccess.Data/Entities/AuditLog.cs`
   - Immutable audit log entity:
     ```csharp
     namespace PatientAccess.Data.Entities
     {
         /// <summary>
         /// Immutable audit log entry for HIPAA compliance (AC1, AC2, NFR-007).
         /// </summary>
         public sealed class AuditLog
         {
             /// <summary>
             /// Unique identifier for the audit entry (immutable).
             /// </summary>
             public long Id { get; init; }
             
             /// <summary>
             /// User ID who performed the action (nullable for system actions).
             /// </summary>
             public int? UserId { get; init; }
             
             /// <summary>
             /// Action type: Login, Logout, Create, Read, Update, Delete, Export, Print.
             /// </summary>
             public required string ActionType { get; init; }
             
             /// <summary>
             /// Resource type: User, Patient, Appointment, ClinicalDocument, ExtractedClinicalData.
             /// </summary>
             public required string ResourceType { get; init; }
             
             /// <summary>
             /// Identifier of the resource being accessed/modified.
             /// </summary>
             public string? ResourceId { get; init; }
             
             /// <summary>
             /// Timestamp when the action occurred (UTC, AC1).
             /// </summary>
             public DateTime Timestamp { get; init; } = DateTime.UtcNow;
             
             /// <summary>
             /// IP address of the client (AC1).
             /// </summary>
             public required string IpAddress { get; init; }
             
             /// <summary>
             /// Result of the action: Success, Failure, PartialSuccess (AC1).
             /// </summary>
             public required string Result { get; init; }
             
             /// <summary>
             /// Additional action details in JSONB format (e.g., changed fields, error messages).
             /// </summary>
             public string? ActionDetails { get; init; }
             
             /// <summary>
             /// User agent string from HTTP request.
             /// </summary>
             public string? UserAgent { get; init; }
             
             /// <summary>
             /// Session ID for correlating actions within a session.
             /// </summary>
             public string? SessionId { get; init; }
             
             /// <summary>
             /// Whether this log entry has been archived to cold storage (edge case).
             /// </summary>
             public bool IsArchived { get; init; } = false;
         }
     }
     ```

2. **Create AuditLogArchive Entity**
   - File: `src/backend/PatientAccess.Data/Entities/AuditLogArchive.cs`
   - Archive table for 7-year retention (DR-007, edge case):
     ```csharp
     namespace PatientAccess.Data.Entities
     {
         /// <summary>
         /// Archive table for audit logs older than retention period (DR-007, edge case).
         /// </summary>
         public sealed class AuditLogArchive
         {
             /// <summary>
             /// Original audit log ID from AuditLog table.
             /// </summary>
             public long Id { get; init; }
             
             public int? UserId { get; init; }
             public required string ActionType { get; init; }
             public required string ResourceType { get; init; }
             public string? ResourceId { get; init; }
             public DateTime Timestamp { get; init; }
             public required string IpAddress { get; init; }
             public required string Result { get; init; }
             public string? ActionDetails { get; init; }
             public string? UserAgent { get; init; }
             public string? SessionId { get; init; }
             
             /// <summary>
             /// Timestamp when this entry was archived.
             /// </summary>
             public DateTime ArchivedAt { get; init; } = DateTime.UtcNow;
         }
     }
     ```

3. **Create Database Schema Migration**
   - File: `src/backend/scripts/migrations/create_audit_log_schema.sql`
   - PostgreSQL schema with indexes:
     ```sql
     -- Create AuditLog table with immutability guarantees (AC2, AD-007)
     CREATE TABLE IF NOT EXISTS "AuditLogs" (
         "Id" BIGSERIAL PRIMARY KEY,
         "UserId" INTEGER,
         "ActionType" VARCHAR(50) NOT NULL,
         "ResourceType" VARCHAR(100) NOT NULL,
         "ResourceId" VARCHAR(100),
         "Timestamp" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
         "IpAddress" VARCHAR(45) NOT NULL, -- IPv6 max length
         "Result" VARCHAR(20) NOT NULL,
         "ActionDetails" JSONB,
         "UserAgent" TEXT,
         "SessionId" VARCHAR(100),
         "IsArchived" BOOLEAN NOT NULL DEFAULT FALSE,
         
         -- Foreign key to Users table
         CONSTRAINT "FK_AuditLogs_Users" FOREIGN KEY ("UserId") 
             REFERENCES "Users"("Id") ON DELETE SET NULL
     );
     
     -- Create composite indexes for query performance (NFR-007: <2s query time)
     CREATE INDEX IF NOT EXISTS "IX_AuditLogs_Timestamp_DESC" 
         ON "AuditLogs" ("Timestamp" DESC);
     
     CREATE INDEX IF NOT EXISTS "IX_AuditLogs_UserId_Timestamp" 
         ON "AuditLogs" ("UserId", "Timestamp" DESC);
     
     CREATE INDEX IF NOT EXISTS "IX_AuditLogs_ActionType_Timestamp" 
         ON "AuditLogs" ("ActionType", "Timestamp" DESC);
     
     CREATE INDEX IF NOT EXISTS "IX_AuditLogs_ResourceType_ResourceId_Timestamp" 
         ON "AuditLogs" ("ResourceType", "ResourceId", "Timestamp" DESC);
     
     -- GIN index for JSONB ActionDetails
     CREATE INDEX IF NOT EXISTS "IX_AuditLogs_ActionDetails" 
         ON "AuditLogs" USING GIN ("ActionDetails");
     
     -- Create AuditLogArchive table for 7-year retention (DR-007)
     CREATE TABLE IF NOT EXISTS "AuditLogArchives" (
         "Id" BIGINT PRIMARY KEY,
         "UserId" INTEGER,
         "ActionType" VARCHAR(50) NOT NULL,
         "ResourceType" VARCHAR(100) NOT NULL,
         "ResourceId" VARCHAR(100),
         "Timestamp" TIMESTAMPTZ NOT NULL,
         "IpAddress" VARCHAR(45) NOT NULL,
         "Result" VARCHAR(20) NOT NULL,
         "ActionDetails" JSONB,
         "UserAgent" TEXT,
         "SessionId" VARCHAR(100),
         "ArchivedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
     );
     
     CREATE INDEX IF NOT EXISTS "IX_AuditLogArchives_Timestamp_DESC" 
         ON "AuditLogArchives" ("Timestamp" DESC);
     
     -- Rollback script
     -- DROP TABLE IF EXISTS "AuditLogArchives" CASCADE;
     -- DROP TABLE IF EXISTS "AuditLogs" CASCADE;
     ```

4. **Create PostgreSQL RLS Policies**
   - File: `src/backend/scripts/migrations/create_audit_rls_policies.sql`
   - Row-level security preventing UPDATE/DELETE (AC2):
     ```sql
     -- Enable row-level security on AuditLogs table (AC2)
     ALTER TABLE "AuditLogs" ENABLE ROW LEVEL SECURITY;
     
     -- Policy: Allow INSERT for all authenticated users
     CREATE POLICY "audit_insert_policy" ON "AuditLogs"
         FOR INSERT
         TO PUBLIC
         WITH CHECK (true);
     
     -- Policy: Allow SELECT for Admin users only (query access)
     CREATE POLICY "audit_select_policy" ON "AuditLogs"
         FOR SELECT
         TO PUBLIC
         USING (true); -- Application-level authorization via RBAC
     
     -- Policy: DENY all UPDATE operations (AC2 - immutability enforcement)
     CREATE POLICY "audit_deny_update_policy" ON "AuditLogs"
         FOR UPDATE
         TO PUBLIC
         USING (false);
     
     -- Policy: DENY all DELETE operations (AC2 - immutability enforcement)
     CREATE POLICY "audit_deny_delete_policy" ON "AuditLogs"
         FOR DELETE
         TO PUBLIC
         USING (false);
     
     -- Create trigger to prevent modifications (double enforcement with RLS)
     CREATE OR REPLACE FUNCTION prevent_audit_log_modification()
     RETURNS TRIGGER AS $$
     BEGIN
         IF TG_OP = 'UPDATE' THEN
             RAISE EXCEPTION 'Audit logs are immutable. UPDATE operations are not permitted.';
         ELSIF TG_OP = 'DELETE' THEN
             RAISE EXCEPTION 'Audit logs are immutable. DELETE operations are not permitted.';
         END IF;
         RETURN NULL;
     END;
     $$ LANGUAGE plpgsql;
     
     CREATE TRIGGER prevent_audit_modification
         BEFORE UPDATE OR DELETE ON "AuditLogs"
         FOR EACH ROW
         EXECUTE FUNCTION prevent_audit_log_modification();
     
     -- Rollback script
     -- DROP TRIGGER IF EXISTS prevent_audit_modification ON "AuditLogs";
     -- DROP FUNCTION IF EXISTS prevent_audit_log_modification();
     -- DROP POLICY IF EXISTS "audit_insert_policy" ON "AuditLogs";
     -- DROP POLICY IF EXISTS "audit_select_policy" ON "AuditLogs";
     -- DROP POLICY IF EXISTS "audit_deny_update_policy" ON "AuditLogs";
     -- DROP POLICY IF EXISTS "audit_deny_delete_policy" ON "AuditLogs";
     -- ALTER TABLE "AuditLogs" DISABLE ROW LEVEL SECURITY;
     ```

5. **Create Archive Function**
   - File: `src/backend/scripts/migrations/create_audit_archive_function.sql`
   - Function to archive logs older than retention period (edge case):
     ```sql
     -- Function to archive audit logs older than retention period (DR-007: 7 years)
     CREATE OR REPLACE FUNCTION archive_old_audit_logs(retention_days INTEGER DEFAULT 2555)
     RETURNS TABLE(archived_count BIGINT) AS $$
     DECLARE
         cutoff_date TIMESTAMPTZ;
         rows_archived BIGINT;
     BEGIN
         -- Calculate cutoff date (7 years = 2555 days)
         cutoff_date := NOW() - INTERVAL '1 day' * retention_days;
         
         -- Insert old logs into archive table
         WITH archived AS (
             INSERT INTO "AuditLogArchives" (
                 "Id", "UserId", "ActionType", "ResourceType", "ResourceId",
                 "Timestamp", "IpAddress", "Result", "ActionDetails",
                 "UserAgent", "SessionId", "ArchivedAt"
             )
             SELECT 
                 "Id", "UserId", "ActionType", "ResourceType", "ResourceId",
                 "Timestamp", "IpAddress", "Result", "ActionDetails",
                 "UserAgent", "SessionId", NOW()
             FROM "AuditLogs"
             WHERE "Timestamp" < cutoff_date
                 AND "IsArchived" = FALSE
             RETURNING "Id"
         )
         SELECT COUNT(*) INTO rows_archived FROM archived;
         
         -- Mark archived logs (no DELETE to maintain immutability)
         UPDATE "AuditLogs"
         SET "IsArchived" = TRUE
         WHERE "Timestamp" < cutoff_date
             AND "IsArchived" = FALSE;
         
         RETURN QUERY SELECT rows_archived;
     END;
     $$ LANGUAGE plpgsql;
     
     -- Rollback script
     -- DROP FUNCTION IF EXISTS archive_old_audit_logs(INTEGER);
     ```

6. **Create EF Core Configuration**
   - File: `src/backend/PatientAccess.Data/Configurations/AuditLogConfiguration.cs`
   - Entity Framework Core mapping with read-only DbSet:
     ```csharp
     using Microsoft.EntityFrameworkCore;
     using Microsoft.EntityFrameworkCore.Metadata.Builders;
     using PatientAccess.Data.Entities;
     
     namespace PatientAccess.Data.Configurations
     {
         public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
         {
             public void Configure(EntityTypeBuilder<AuditLog> builder)
             {
                 builder.ToTable("AuditLogs");
                 
                 builder.HasKey(a => a.Id);
                 
                 builder.Property(a => a.Id)
                     .ValueGeneratedOnAdd();
                 
                 builder.Property(a => a.ActionType)
                     .IsRequired()
                     .HasMaxLength(50);
                 
                 builder.Property(a => a.ResourceType)
                     .IsRequired()
                     .HasMaxLength(100);
                 
                 builder.Property(a => a.ResourceId)
                     .HasMaxLength(100);
                 
                 builder.Property(a => a.Timestamp)
                     .IsRequired()
                     .HasDefaultValueSql("NOW()");
                 
                 builder.Property(a => a.IpAddress)
                     .IsRequired()
                     .HasMaxLength(45); // IPv6 max length
                 
                 builder.Property(a => a.Result)
                     .IsRequired()
                     .HasMaxLength(20);
                 
                 builder.Property(a => a.ActionDetails)
                     .HasColumnType("jsonb");
                 
                 builder.Property(a => a.UserAgent)
                     .HasColumnType("text");
                 
                 builder.Property(a => a.SessionId)
                     .HasMaxLength(100);
                 
                 builder.Property(a => a.IsArchived)
                     .IsRequired()
                     .HasDefaultValue(false);
                 
                 // Foreign key to Users
                 builder.HasOne<User>()
                     .WithMany()
                     .HasForeignKey(a => a.UserId)
                     .OnDelete(DeleteBehavior.SetNull);
                 
                 // Indexes (matching SQL migration)
                 builder.HasIndex(a => a.Timestamp)
                     .IsDescending();
                 
                 builder.HasIndex(a => new { a.UserId, a.Timestamp })
                     .IsDescending(false, true);
                 
                 builder.HasIndex(a => new { a.ActionType, a.Timestamp })
                     .IsDescending(false, true);
                 
                 builder.HasIndex(a => new { a.ResourceType, a.ResourceId, a.Timestamp })
                     .IsDescending(false, false, true);
             }
         }
     }
     ```

7. **Update DbContext**
   - File: `src/backend/PatientAccess.Data/Context/PatientAccessDbContext.cs`
   - Add AuditLogs DbSet with query filter:
     ```csharp
     public class PatientAccessDbContext : DbContext
     {
         public PatientAccessDbContext(DbContextOptions<PatientAccessDbContext> options)
             : base(options) { }
         
         // Existing DbSets...
         
         /// <summary>
         /// Immutable audit logs (AC2 - read-only via application logic).
         /// </summary>
         public DbSet<AuditLog> AuditLogs { get; set; }
         
         /// <summary>
         /// Archived audit logs for 7-year retention (DR-007).
         /// </summary>
         public DbSet<AuditLogArchive> AuditLogArchives { get; set; }
         
         protected override void OnModelCreating(ModelBuilder modelBuilder)
         {
             base.OnModelCreating(modelBuilder);
             
             // Apply configurations
             modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
             
             // Global query filter to exclude archived logs from default queries
             modelBuilder.Entity<AuditLog>()
                 .HasQueryFilter(a => !a.IsArchived);
         }
     }
     ```

8. **Create Database Migration with EF Core**
   - Command: `dotnet ef migrations add CreateAuditLogSchema --project PatientAccess.Data`
   - Migration includes:
     * AuditLogs table creation
     * AuditLogArchives table creation
     * Composite indexes
     * Foreign key to Users table

## Current Project State

```
src/backend/
├── PatientAccess.Data/
│   ├── Context/
│   │   └── PatientAccessDbContext.cs
│   ├── Entities/
│   └── Configurations/
└── scripts/
    └── migrations/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Data/Entities/AuditLog.cs | Audit log entity |
| CREATE | src/backend/PatientAccess.Data/Entities/AuditLogArchive.cs | Archive entity |
| CREATE | src/backend/PatientAccess.Data/Configurations/AuditLogConfiguration.cs | EF Core config |
| CREATE | src/backend/scripts/migrations/create_audit_log_schema.sql | Schema migration |
| CREATE | src/backend/scripts/migrations/create_audit_rls_policies.sql | RLS policies |
| CREATE | src/backend/scripts/migrations/create_audit_archive_function.sql | Archive function |
| MODIFY | src/backend/PatientAccess.Data/Context/PatientAccessDbContext.cs | Add DbSets |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### PostgreSQL Documentation
- **Row-Level Security**: https://www.postgresql.org/docs/16/ddl-rowsecurity.html
- **Triggers**: https://www.postgresql.org/docs/16/plpgsql-trigger.html
- **JSONB Type**: https://www.postgresql.org/docs/16/datatype-json.html
- **Indexes**: https://www.postgresql.org/docs/16/indexes.html

### Entity Framework Core
- **Indexes**: https://learn.microsoft.com/en-us/ef/core/modeling/indexes
- **Query Filters**: https://learn.microsoft.com/en-us/ef/core/querying/filters

### Design Requirements
- **FR-040**: Comprehensive audit logging (spec.md)
- **NFR-007**: Immutable audit logs (design.md)
- **DR-005**: Audit log data structure (design.md)
- **DR-007**: 7-year retention (design.md)
- **AD-007**: Immutable audit trail architecture (design.md)

## Build Commands
```powershell
# Create migration
cd src/backend
dotnet ef migrations add CreateAuditLogSchema --project PatientAccess.Data

# Apply migration
dotnet ef database update --project PatientAccess.Data

# Run SQL scripts manually (if needed)
psql -U postgres -d patient_access -f scripts/migrations/create_audit_log_schema.sql
psql -U postgres -d patient_access -f scripts/migrations/create_audit_rls_policies.sql
psql -U postgres -d patient_access -f scripts/migrations/create_audit_archive_function.sql
```

## Validation Strategy

### Database Tests
- File: `src/backend/PatientAccess.Tests/Database/AuditLogSchemaTests.cs`
- Test cases:
  1. **Test_AuditLog_CanInsert**
     - Insert: New audit log entry
     - Assert: Entry saved successfully with all fields
  2. **Test_AuditLog_UpdateThrowsException**
     - Attempt: UPDATE existing audit log entry
     - Assert: Exception thrown with message "Audit logs are immutable. UPDATE operations are not permitted."
  3. **Test_AuditLog_DeleteThrowsException**
     - Attempt: DELETE existing audit log entry
     - Assert: Exception thrown with message "Audit logs are immutable. DELETE operations are not permitted."
  4. **Test_AuditLog_IndexesExist**
     - Query: pg_indexes table
     - Assert: All composite indexes created (Timestamp, UserId+Timestamp, ActionType+Timestamp, ResourceType+ResourceId+Timestamp)
  5. **Test_ArchiveFunction_MovesOldLogs**
     - Setup: Create audit logs older than 7 years
     - Call: archive_old_audit_logs()
     - Assert: Logs moved to AuditLogArchives, IsArchived = TRUE

### Performance Tests
- File: `src/backend/PatientAccess.Tests/Performance/AuditLogPerformanceTests.cs`
- Test cases:
  1. **Test_AuditInsert_Within200ms**
     - Measure: Time to insert 1 audit log entry
     - Assert: Execution time < 200ms (AC1)
  2. **Test_AuditQuery_ReturnsWithin2Seconds**
     - Setup: 1 million audit log entries
     - Query: Filter by date range + user ID
     - Assert: Query execution time < 2s (NFR-007)

### Acceptance Criteria Validation
- **AC1**: ✅ Audit record created with actor ID, action type, resource, timestamp, IP address, result
- **AC2**: ✅ Append-only enforced via PostgreSQL RLS policies + trigger preventing UPDATE/DELETE
- **Edge Case**: ✅ Archive function moves logs to cold storage without deletion (DR-007)

## Success Criteria Checklist
- [MANDATORY] AuditLog table created with all required fields (AC1)
- [MANDATORY] PostgreSQL RLS policies prevent UPDATE/DELETE operations (AC2)
- [MANDATORY] Database trigger prevents modifications (AC2, double enforcement)
- [MANDATORY] Composite indexes created for query performance (NFR-007)
- [MANDATORY] AuditLogArchive table created for 7-year retention (DR-007)
- [MANDATORY] archive_old_audit_logs() function implemented (edge case)
- [MANDATORY] Entity Framework Core configuration with read-only DbSet
- [MANDATORY] Database migration with rollback script
- [MANDATORY] Foreign key to Users table with ON DELETE SET NULL
- [MANDATORY] JSONB column for ActionDetails with GIN index
- [MANDATORY] Global query filter excludes archived logs
- [MANDATORY] Database test: INSERT succeeds
- [MANDATORY] Database test: UPDATE throws exception
- [MANDATORY] Database test: DELETE throws exception
- [MANDATORY] Performance test: INSERT within 200ms (AC1)
- [RECOMMENDED] Partitioning strategy for large-scale deployments
- [RECOMMENDED] Automated archive job via Hangfire (separate task)

## Estimated Effort
**3 hours** (Schema + RLS policies + EF Core config + migration + tests)
