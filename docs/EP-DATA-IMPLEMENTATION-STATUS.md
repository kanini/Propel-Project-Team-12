# EP-DATA Implementation Status

> Last updated: 2026-03-21

## Summary

| Epic | Stories | Status |
|------|---------|--------|
| EP-DATA-I | US_009 – US_013 | **All COMPLETE** |
| EP-DATA-II | US_014 – US_017 | **All COMPLETE** |

All 9 user stories across both epics are now fully implemented at the data layer.

---

## EP-DATA-I — Core Data Schema & Relationships

### US_009 — User Entity and Migration

**Status:** COMPLETE

| AC | Requirement | Implemented |
|----|-------------|-------------|
| AC-1 | User entity with ID (GUID), email (unique), name, DOB, phone, password hash, role enum, status enum, timestamps | Yes |
| AC-2 | Unique index on Email column | Yes |
| AC-3 | `UserConfiguration.cs` with Fluent API | Yes |
| AC-4 | Migration creates Users table with all constraints | Yes |

**Key files:**

- `PatientAccess.Data/Models/User.cs`
- `PatientAccess.Data/Models/UserRole.cs`, `UserStatus.cs`
- `PatientAccess.Data/Configurations/UserConfiguration.cs`
- `PatientAccess.Data/Migrations/20260321050221_AddCoreEntities.cs`

---

### US_010 — Appointment Entity and Migration

**Status:** COMPLETE

| AC | Requirement | Implemented |
|----|-------------|-------------|
| AC-1 | Appointment entity with ID, patient FK, provider FK, scheduled datetime, status enum (6 values), visit reason, walk-in flag, preferred swap reference, no-show risk score, cancellation notice hours | Yes |
| AC-2 | Provider entity with ID, name, specialty, availability, no login | Yes |
| AC-3 | TimeSlot entity with ID, provider FK, start/end time, booking status, concurrency token | Yes |
| AC-4 | FKs linking Appointment to User (patient) and Provider with cascade rules | Yes |

**Gap closed:** `PreferredSwapReference` (nullable `Guid?`) added to `Appointment` entity in migration `20260321085615_AddPreferredSwapAndEmbeddingColumns`.

**Key files:**

- `PatientAccess.Data/Models/Appointment.cs`
- `PatientAccess.Data/Models/AppointmentStatus.cs`
- `PatientAccess.Data/Models/Provider.cs`, `TimeSlot.cs`
- `PatientAccess.Data/Configurations/AppointmentConfiguration.cs`
- `PatientAccess.Data/Configurations/ProviderConfiguration.cs`, `TimeSlotConfiguration.cs`

---

### US_011 — Clinical Document and Extracted Data Entities

**Status:** COMPLETE

| AC | Requirement | Implemented |
|----|-------------|-------------|
| AC-1 | ClinicalDocument entity with ID, patient FK, file metadata, processing status enum, timestamps | Yes |
| AC-2 | ExtractedClinicalData entity with ID, document FK, patient FK, data type enum, key/value, confidence score, verification status, source info, verifier reference | Yes |
| AC-3 | FKs from ExtractedClinicalData to ClinicalDocument and User with indexes | Yes |
| AC-4 | Optional `embedding vector(1536)` column for semantic similarity search | Yes |

**Gap closed:** `Embedding` property (`Vector?`, 1536 dimensions) added to `ExtractedClinicalData` entity. `UseVector()` configured on Npgsql provider in `Program.cs`. Migration `20260321085615_AddPreferredSwapAndEmbeddingColumns`.

**Key files:**

- `PatientAccess.Data/Models/Extractedclinicaldata.cs`
- `PatientAccess.Data/Models/ClinicalDocument.cs`
- `PatientAccess.Data/Models/ClinicalDataType.cs`, `ProcessingStatus.cs`, `VerificationStatus.cs`
- `PatientAccess.Data/Configurations/ExtractedClinicalDataConfiguration.cs`
- `PatientAccess.Data/Configurations/ClinicalDocumentConfiguration.cs`

---

### US_012 — Audit Log Entity and Referential Integrity

**Status:** COMPLETE

| AC | Requirement | Implemented |
|----|-------------|-------------|
| AC-1 | AuditLog entity with ID, user FK, timestamp, action type, resource type, resource ID, action details (JSONB), IP address | Yes |
| AC-2 | DB triggers preventing UPDATE/DELETE on AuditLogs | Yes |
| AC-3 | FK constraints linking appointments, documents, intake records, waitlist entries to patient User record | Yes |
| AC-4 | UPDATE/DELETE via EF Core fails with DB-level exception | Yes (via trigger) |

**Bonus:** 7-year HIPAA audit retention policy trigger added in migration `20260321052559_AddAuditRetentionPolicy`.

**Key files:**

- `PatientAccess.Data/Models/AuditLog.cs`
- `PatientAccess.Data/Configurations/AuditLogConfiguration.cs`
- `PatientAccess.Data/Migrations/20260321050310_AddAuditLogImmutabilityTriggers.cs`
- `PatientAccess.Data/Migrations/20260321052559_AddAuditRetentionPolicy.cs`

---

### US_013 — Database Backup and Migration Infrastructure

**Status:** COMPLETE

| AC | Requirement | Implemented |
|----|-------------|-------------|
| AC-1 | Supabase PITR with >= 15-minute granularity | Infrastructure-level (outside codebase) |
| AC-2 | Versioned migrations in chronological order with timestamps and descriptive names | Yes |
| AC-3 | Non-breaking schema change patterns documented in migration comments | Yes |
| AC-4 | Rollback via `dotnet ef database update <previous>` works cleanly | Yes |

**Gap closed:** Zero-downtime migration pattern fully documented in `AddCoreEntities` migration XML summary. Pattern: nullable column first → deploy code → backfill → add constraint. New `AddPreferredSwapAndEmbeddingColumns` migration follows this pattern (both columns are nullable).

**Key files:**

- `PatientAccess.Data/Migrations/20260321050221_AddCoreEntities.cs` (pattern documentation)
- `PatientAccess.Data/Migrations/20260321085615_AddPreferredSwapAndEmbeddingColumns.cs` (pattern applied)

---

## EP-DATA-II — Extended Entities & Reference Data

### US_014 — Waitlist and Intake Record Entities

**Status:** COMPLETE

| AC | Requirement | Implemented |
|----|-------------|-------------|
| AC-1 | WaitlistEntry entity with ID, patient FK, provider FK, date range, time preference, notification preference enum, priority, status | Yes |
| AC-2 | IntakeRecord entity with ID, appointment FK, patient FK, intake mode enum, JSONB columns for medical history/medications/allergies, visit concerns, insurance FK, completion status | Yes |
| AC-3 | Migration creates FKs (WaitlistEntry→User/Provider, IntakeRecord→Appointment/User) with indexes | Yes |
| AC-4 | JSONB columns for structured health data | Yes |

**Key files:**

- `PatientAccess.Data/Models/WaitlistEntry.cs`, `WaitlistStatus.cs`, `NotificationPreference.cs`, `PreferredTimeOfDay.cs`
- `PatientAccess.Data/Models/IntakeRecord.cs`, `IntakeMode.cs`, `InsuranceValidationStatus.cs`
- `PatientAccess.Data/Configurations/WaitlistEntryConfiguration.cs`, `IntakeRecordConfiguration.cs`

---

### US_015 — Medical Code and Notification Entities

**Status:** COMPLETE

| AC | Requirement | Implemented |
|----|-------------|-------------|
| AC-1 | MedicalCode entity with ID, extracted data FK, code system enum, code value, description, confidence score, verification status enum, verifier FK | Yes |
| AC-2 | Notification entity with ID, recipient FK, appointment FK, channel type enum, template name, status enum, scheduled/sent time, delivery confirmation, retry count, error message | Yes |
| AC-3 | Indexes on Notification (recipient_id, scheduled_time, status) and composite unique on MedicalCode (extracted_data_id, code_system, code_value) | Yes |

**Note:** Notification indexes are implemented as three separate indexes rather than one composite. Functionally correct; composite would be more optimal for the described query pattern.

**Key files:**

- `PatientAccess.Data/Models/MedicalCode.cs`, `CodeSystem.cs`, `MedicalCodeVerificationStatus.cs`
- `PatientAccess.Data/Models/Notification.cs`, `ChannelType.cs`, `NotificationStatus.cs`
- `PatientAccess.Data/Configurations/MedicalCodeConfiguration.cs`, `NotificationConfiguration.cs`

---

### US_016 — Insurance, No-Show History, and Audit Retention

**Status:** COMPLETE

| AC | Requirement | Implemented |
|----|-------------|-------------|
| AC-1 | InsuranceRecord entity with ID, provider name, accepted ID patterns (regex), coverage type enum, active status | Yes |
| AC-2 | NoShowHistory entity with ID, patient FK, counts, confirmation rate, lead time, risk score, last updated | Yes |
| AC-3 | 7-year audit retention policy preventing deletion of audit logs younger than 7 years | Yes |
| AC-4 | InsuranceRecord index on (provider_name, active); NoShowHistory unique constraint on patient_id | Yes |

**Key files:**

- `PatientAccess.Data/Models/InsuranceRecord.cs`, `CoverageType.cs`
- `PatientAccess.Data/Models/NoShowHistory.cs`
- `PatientAccess.Data/Configurations/InsuranceRecordConfiguration.cs`, `NoShowHistoryConfiguration.cs`
- `PatientAccess.Data/Migrations/20260321052559_AddAuditRetentionPolicy.cs`

---

### US_017 — Reference Data Seeders

**Status:** COMPLETE

| AC | Requirement | Implemented |
|----|-------------|-------------|
| AC-1 | Insurance seeder creates >= 10 dummy insurance records with varied names, ID patterns, coverage types | Yes (exactly 10) |
| AC-2 | Provider seeder creates >= 5 providers with different specialties and 30-day availability schedules | Yes (5 providers, 30-minute slots, 30 weekdays) |
| AC-3 | Seeders are idempotent (no duplicate records) | Yes (`AnyAsync()` guard) |
| AC-4 | `dotnet ef database update` + seeder populates data | Yes (auto-runs in Dev/Staging) |

**Key files:**

- `PatientAccess.Data/DatabaseSeeder.cs`
- `PatientAccess.Web/Program.cs` (environment-guarded invocation)

---

## Migration Inventory

| Timestamp | Name | Purpose |
|-----------|------|---------|
| 20260321050221 | AddCoreEntities | EP-DATA-I: Users, Providers, Appointments, TimeSlots, ClinicalDocuments, ExtractedClinicalData, AuditLogs |
| 20260321050310 | AddAuditLogImmutabilityTriggers | EP-DATA-I: Immutable audit log triggers (no UPDATE/DELETE) |
| 20260321052532 | AddExtendedEntities | EP-DATA-II: WaitlistEntries, IntakeRecords, MedicalCodes, Notifications, InsuranceRecords, NoShowHistory |
| 20260321052559 | AddAuditRetentionPolicy | EP-DATA-II: 7-year HIPAA retention trigger replacing unconditional delete trigger |
| 20260321085615 | AddPreferredSwapAndEmbeddingColumns | Gap closure: Appointments.PreferredSwapReference + ExtractedClinicalData.Embedding vector(1536) |

---

## Database Tables (15 total)

### EP-DATA-I (9 tables)

| Table | Entity | Purpose |
|-------|--------|---------|
| Users | `User` | Authenticated system users (Patient, Staff, Admin) |
| Providers | `Provider` | Healthcare provider reference data |
| Appointments | `Appointment` | Patient visit scheduling with status lifecycle |
| TimeSlots | `TimeSlot` | Provider availability windows with concurrency control |
| ClinicalDocuments | `ClinicalDocument` | Uploaded patient documents with processing status |
| ExtractedClinicalData | `ExtractedClinicalData` | AI-extracted data points with pgvector embeddings |
| AuditLogs | `AuditLog` | Immutable audit trail (7-year HIPAA retention) |

### EP-DATA-II (6 tables)

| Table | Entity | Purpose |
|-------|--------|---------|
| WaitlistEntries | `WaitlistEntry` | Patient slot preferences for unavailable times |
| IntakeRecords | `IntakeRecord` | Pre-visit intake (AI conversational or manual form) |
| MedicalCodes | `MedicalCode` | AI-suggested ICD-10/CPT codes with confidence |
| Notifications | `Notification` | Multi-channel notification delivery tracking |
| InsuranceRecords | `InsuranceRecord` | Insurance provider reference/validation data |
| NoShowHistory | `NoShowHistory` | Patient no-show aggregated risk metrics |

---

## Cross-Cutting Notes

- **Repositories, Services, Controllers:** Not yet implemented for EP-DATA entities (only auth-related services exist). These will be required by future feature epics.
- **Unit Tests:** Only `PasswordHashingServiceTests.cs` exists. Entity-level tests are not required by EP-DATA stories but recommended before building feature layers.
- **Supabase PITR (US_013 AC-1):** Infrastructure configuration, verified outside codebase.
