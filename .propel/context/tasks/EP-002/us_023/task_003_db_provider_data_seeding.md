# Task - task_003_db_provider_data_seeding

## Requirement Reference
- User Story: US_023
- Story Location: .propel/context/tasks/EP-002/us_023/us_023.md
- Acceptance Criteria:
    - AC-1: Display available providers with name, specialty, ratings summary, and next available slot
    - Edge Case support: Large provider list (100+ providers) for pagination testing
- Edge Case:
    - Providers with no available slots for "Join Waitlist" button testing

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
Create database seed data scripts for Providers and TimeSlots tables to support development and testing of the Provider Browser feature. Generate 120 diverse provider records across various specialties (Cardiology, Dermatology, Pediatrics, Orthopedics, etc.) with realistic ratings (3.5-5.0 stars). Create TimeSlots data with a mix of booked and available slots, including providers with no availability to test edge cases. Ensure data distribution supports pagination testing (20 providers per page for 6 pages) and filtering scenarios.

## Dependent Tasks
- None (schema already exists from complete_schema_ep_data_i_ii.sql)

## Impacted Components
- Database (PostgreSQL):
  - Providers table (INSERT data)
  - TimeSlots table (INSERT data)
- New SQL seed script file to be created

## Implementation Plan
1. **Create provider seed data SQL script**:
   - Generate 120 provider records with UUIDs (use gen_random_uuid())
   - Distribute across 8 specialties: Cardiology (20), Dermatology (15), Pediatrics (20), Orthopedics (15), Neurology (10), Radiology (15), Psychiatry (15), General Practice (10)
   - Rating distribution: 70% (4.0-5.0), 20% (3.5-3.9), 10% (3.0-3.4)
   - IsActive: 115 active (TRUE), 5 inactive (FALSE) for filter testing
   - CreatedAt: Stagger dates over past 12 months

2. **Create TimeSlots seed data**:
   - Generate 1000+ time slots across all providers
   - Date range: Today + 30 days (future appointments)
   - Slot duration: 30-minute intervals (e.g., 9:00 AM, 9:30 AM, 10:00 AM)
   - Booking status: 60% IsBooked = FALSE (available), 40% IsBooked = TRUE
   - Special cases:
     - 10 providers with ZERO available slots (all IsBooked = TRUE)
     - 20 providers with only 1-2 available slots (limited availability)
     - 50 providers with 10+ available slots (good availability)

3. **Implement idempotent seed script**:
   - Add DELETE FROM "TimeSlots" and DELETE FROM "Providers" at script start (cascade deletes)
   - Add conditional checks: IF NOT EXISTS for idempotency
   - Include TRANSACTION block for atomicity (BEGIN; ... COMMIT;)

4. **Add data validation checks**:
   - Verify foreign key constraints (TimeSlots.ProviderId -> Providers.ProviderId)
   - Check indexes exist (IX_Providers_Specialty, IX_Providers_IsActive)
   - Validate data types and constraints (Rating between 0.0 and 5.0, IsActive boolean)

5. **Create search and filter test data**:
   - Include providers with names containing common search terms (e.g., "Smith", "Johnson")
   - Ensure all specialties have multiple providers for filter testing
   - Add edge cases: providers with very long names (50+ chars) for UI testing

6. **Document seed data characteristics**:
   - Add SQL comments explaining data distribution
   - Document expected pagination behavior (6 pages of 20 providers)
   - List provider IDs with no availability for test reference

## Current Project State
```
src/backend/
├── complete_schema_ep_data_i_ii.sql (existing schema)
└── scripts/
    └── (seed_providers_timeslots.sql to be created)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/scripts/seed_providers_timeslots.sql | SQL seed script for Providers and TimeSlots test data |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References
- [PostgreSQL INSERT Documentation](https://www.postgresql.org/docs/16/sql-insert.html)
- [PostgreSQL UUID Functions](https://www.postgresql.org/docs/16/functions-uuid.html)
- [PostgreSQL Transaction Control](https://www.postgresql.org/docs/16/tutorial-transactions.html)
- [PostgreSQL Date/Time Functions](https://www.postgresql.org/docs/16/functions-datetime.html)
- [SQL Seed Data Best Practices](https://www.red-gate.com/simple-talk/databases/sql-server/database-administration-sql-server/populating-a-database-with-test-data/)

## Build Commands
- `psql -U postgres -d patientaccess -f src/backend/scripts/seed_providers_timeslots.sql` - Execute seed script
- `psql -U postgres -d patientaccess -c "SELECT COUNT(*) FROM \"Providers\";"` - Verify provider count
- `psql -U postgres -d patientaccess -c "SELECT COUNT(*) FROM \"TimeSlots\";"` - Verify time slot count

## Implementation Validation Strategy
- [ ] Seed script executes without errors
- [ ] 120 providers inserted (115 active, 5 inactive)
- [ ] 1000+ time slots inserted with correct foreign key references
- [ ] 10 providers have zero available slots (all IsBooked = TRUE)
- [ ] Provider rating distribution verified (70% are 4.0-5.0 stars)
- [ ] All 8 specialties represented with data
- [ ] TimeSlots cover next 30 days from today
- [ ] Foreign key constraints validated (no orphaned TimeSlots)
- [ ] Indexes verified (IX_Providers_Specialty, IX_Providers_IsActive)
- [ ] Script is idempotent (can be re-run without errors)

## Implementation Checklist
- [X] Create seed_providers_timeslots.sql file with BEGIN TRANSACTION block
- [X] Add DELETE FROM "TimeSlots" CASCADE to clear existing data
- [X] Add DELETE FROM "Providers" CASCADE to clear existing data
- [X] Generate 120 INSERT INTO "Providers" statements with gen_random_uuid()
- [X] Distribute providers across 8 specialties (Cardiology, Dermatology, Pediatrics, Orthopedics, Neurology, Radiology, Psychiatry, Family Medicine)
- [X] Assign ratings: 70% between 4.0-5.0, 20% between 3.5-3.9, 10% between 3.0-3.4
- [X] Set IsActive: 115 TRUE, 5 FALSE
- [X] Generate 1000+ INSERT INTO "TimeSlots" statements using generate_series
- [X] Create time slots for next 30 days (CURRENT_DATE to CURRENT_DATE + INTERVAL '30 days')
- [X] Set 30-minute intervals: 9:00 AM - 5:00 PM (16 slots per day)
- [X] Set IsBooked: 60% FALSE (available), 40% TRUE (booked) for providers with good availability
- [X] Assign 10 providers with zero available slots (all TimeSlots IsBooked = TRUE)
- [X] Assign 20 providers with 1-2 available slots only
- [X] Assign remaining providers with varied availability (60% available slots)
- [X] Add SQL comments documenting data distribution and test scenarios
- [X] Include COMMIT at end of transaction
- [ ] Test script execution on local PostgreSQL database
- [ ] Verify 120 providers exist: SELECT COUNT(*) FROM "Providers";
- [ ] Verify 55,200+ time slots exist: SELECT COUNT(*) FROM "TimeSlots";
- [ ] Verify foreign key integrity: SELECT COUNT(*) FROM "TimeSlots" ts LEFT JOIN "Providers" p ON ts."ProviderId" = p."ProviderId" WHERE p."ProviderId" IS NULL;
- [ ] Query providers with zero availability for test reference
- [ ] Document provider IDs with no availability in script comments
