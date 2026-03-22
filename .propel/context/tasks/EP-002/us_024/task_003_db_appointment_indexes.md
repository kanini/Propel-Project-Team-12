# Task - task_003_db_appointment_indexes

## Requirement Reference
- User Story: US_024
- Story Location: .propel/context/tasks/EP-002/us_024/us_024.md
- Acceptance Criteria:
    - AC-2: API responds within 500ms at 95th percentile (NFR-001)
- Edge Case:
    - Efficient query performance for large datasets (1000+ time slots, 100+ providers)

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

> **Wireframe Status Legend:**
> - **AVAILABLE**: Local file exists at specified path
> - **PENDING**: UI-impacting task awaiting wireframe (provide file or URL)
> - **EXTERNAL**: Wireframe provided via external URL
> - **N/A**: Task has no UI impact
>
> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | N/A | N/A |
| Backend | N/A | N/A |
| Database | PostgreSQL | 16.x |
| Library | pgAdmin / psql | Latest |
| AI/ML | N/A | N/A |
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

> **Mobile Impact Legend:**
> - **Yes**: Task involves mobile app development (native or cross-platform)
> - **No**: Task is web, backend, or infrastructure only
>
> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview
Create database indexes on TimeSlots and Appointments tables to optimize query performance for availability calendar and appointment booking operations. Add composite indexes to support common query patterns: availability lookups by provider and date range, slot availability checks, and appointment retrievals by patient. Ensure indexes align with NFR-001 requirement (500ms API response at P95). Analyze query execution plans to verify index usage and prevent full table scans on large datasets.

## Dependent Tasks
- None (schema already exists from complete_schema_ep_data_i_ii.sql)

## Impacted Components
- Database (PostgreSQL):
  - TimeSlots table (ADD INDEX)
  - Appointments table (ADD INDEX)
- New SQL migration script file to be created

## Implementation Plan
1. **Analyze query patterns from AppointmentService**:
   - Monthly availability: WHERE ProviderId = ? AND StartTime BETWEEN ? AND ?
   - Daily availability: WHERE ProviderId = ? AND DATE(StartTime) = ?
   - Slot availability check: WHERE TimeSlotId = ? AND IsBooked = false FOR UPDATE
   - Patient appointments: WHERE PatientId = ? AND Status IN ('Scheduled', 'Confirmed')

2. **Create composite index for availability queries**:
   - Index name: IX_TimeSlots_ProviderId_StartTime_IsBooked
   - Columns: (ProviderId, StartTime, IsBooked)
   - Justification: Supports monthly/daily availability queries with WHERE and ORDER BY clauses
   - Index type: B-tree (default)

3. **Create index for date-based filtering**:
   - Index name: IX_TimeSlots_StartTime_Date
   - Expression: (DATE(StartTime))
   - Justification: Supports daily availability queries with DATE function
   - Index type: B-tree

4. **Create index for appointment patient lookups**:
   - Index name: IX_Appointments_PatientId_Status
   - Columns: (PatientId, Status)
   - Justification: Supports "My Appointments" queries filtering by patient and status
   - Index type: B-tree

5. **Create index for appointment time slot lookups**:
   - Index name: IX_Appointments_TimeSlotId
   - Column: TimeSlotId
   - Justification: Supports foreign key constraint and booking conflict checks
   - Index type: B-tree

6. **Verify index creation and query plan optimization**:
   - Run EXPLAIN ANALYZE on availability queries
   - Confirm index usage (Index Scan vs. Seq Scan)
   - Measure query execution time with 1000+ time slots
   - Ensure no full table scans for common queries

7. **Create idempotent migration script**:
   - Use CREATE INDEX IF NOT EXISTS for safe re-execution
   - Add comments explaining each index purpose
   - Include DROP INDEX IF EXISTS for rollback scenario

8. **Document index maintenance considerations**:
   - Note that indexes increase INSERT/UPDATE overhead
   - Recommend VACUUM ANALYZE after bulk data imports
   - Suggest monitoring index usage with pg_stat_user_indexes

## Current Project State
```
src/backend/
├── complete_schema_ep_data_i_ii.sql (existing schema)
└── scripts/
    └── migrations/
        └── (003_add_appointment_indexes.sql to be created)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/scripts/migrations/003_add_appointment_indexes.sql | SQL migration for performance indexes |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [PostgreSQL Indexes Documentation](https://www.postgresql.org/docs/16/indexes.html)
- [PostgreSQL EXPLAIN ANALYZE](https://www.postgresql.org/docs/16/using-explain.html)
- [PostgreSQL Index Types](https://www.postgresql.org/docs/16/indexes-types.html)
- [PostgreSQL Composite Indexes](https://www.postgresql.org/docs/16/indexes-multicolumn.html)
- [PostgreSQL Expression Indexes](https://www.postgresql.org/docs/16/indexes-expressional.html)
- [Database Indexing Best Practices](https://use-the-index-luke.com/)

## Build Commands
- `psql -U postgres -d patientaccess -f src/backend/scripts/migrations/003_add_appointment_indexes.sql` - Apply migration
- `psql -U postgres -d patientaccess -c "\d \"TimeSlots\""` - View TimeSlots indexes
- `psql -U postgres -d patientaccess -c "\d \"Appointments\""` - View Appointments indexes
- `psql -U postgres -d patientaccess -c "EXPLAIN ANALYZE SELECT * FROM \"TimeSlots\" WHERE \"ProviderId\" = '<uuid>' AND \"StartTime\" BETWEEN '2024-01-01' AND '2024-01-31';"` - Verify index usage

## Implementation Validation Strategy
- [ ] Migration script executes without errors
- [ ] All 4 indexes created successfully (IX_TimeSlots_ProviderId_StartTime_IsBooked, IX_TimeSlots_StartTime_Date, IX_Appointments_PatientId_Status, IX_Appointments_TimeSlotId)
- [ ] EXPLAIN ANALYZE shows Index Scan (not Seq Scan) for availability queries
- [ ] Monthly availability query execution time < 50ms for 1000+ time slots
- [ ] Daily availability query execution time < 20ms for 100+ time slots
- [ ] Slot availability check (FOR UPDATE) execution time < 10ms
- [ ] Patient appointments query execution time < 30ms for 50+ appointments
- [ ] pg_stat_user_indexes confirms indexes are being used  - Script is idempotent (can be re-run without errors)
- [ ] Rollback script successfully drops indexes

## Implementation Checklist
- [X] Create migration file: 003_add_appointment_indexes.sql
- [X] Add BEGIN TRANSACTION block
- [X] Create composite index: CREATE INDEX IF NOT EXISTS "IX_TimeSlots_ProviderId_StartTime_IsBooked" ON "TimeSlots" ("ProviderId", "StartTime", "IsBooked");
- [X] Add SQL comment explaining ProviderId_StartTime_IsBooked index purpose (monthly/daily availability queries)
- [X] Create expression index: CREATE INDEX IF NOT EXISTS "IX_TimeSlots_StartTime_Date" ON "TimeSlots" (DATE("StartTime"));
- [X] Add SQL comment explaining StartTime_Date index purpose (daily availability with DATE function)
- [X] Create patient lookup index: CREATE INDEX IF NOT EXISTS "IX_Appointments_PatientId_Status" ON "Appointments" ("PatientId", "Status");
- [X] Add SQL comment explaining PatientId_Status index purpose (My Appointments queries)
- [X] Create time slot lookup index: CREATE INDEX IF NOT EXISTS "IX_Appointments_TimeSlotId" ON "Appointments" ("TimeSlotId");
- [X] Add SQL comment explaining TimeSlotId index purpose (foreign key and conflict checks)
- [X] Add COMMIT at end of transaction
- [X] Create rollback section with DROP INDEX IF EXISTS statements (commented out)
- [ ] Test script execution on local PostgreSQL database
- [ ] Run EXPLAIN ANALYZE on monthly availability query: SELECT * FROM "TimeSlots" WHERE "ProviderId" = '<uuid>' AND "StartTime" BETWEEN '2024-01-01' AND '2024-01-31';
- [ ] Verify Index Scan appears in query plan (not Seq Scan)
- [ ] Run EXPLAIN ANALYZE on daily availability query: SELECT * FROM "TimeSlots" WHERE "ProviderId" = '<uuid>' AND DATE("StartTime") = '2024-01-15';
- [ ] Verify Index Scan on IX_TimeSlots_StartTime_Date
- [ ] Run EXPLAIN ANALYZE on patient appointments query: SELECT * FROM "Appointments" WHERE "PatientId" = '<uuid>' AND "Status" = 'Scheduled';
- [ ] Verify Index Scan on IX_Appointments_PatientId_Status
- [ ] Measure query execution time with 1000+ time slots in database
- [ ] Verify monthly availability query < 50ms
- [ ] Verify daily availability query < 20ms
- [ ] Query pg_stat_user_indexes to confirm index usage: SELECT * FROM pg_stat_user_indexes WHERE schemaname = 'public';
- [ ] Verify idx_scan > 0 for created indexes
- [X] Document index maintenance recommendations in migration comments
- [ ] Test rollback: Execute DROP INDEX statements successfully
- [ ] Re-apply migration to verify idempotency
