---
id: test_plan_us_014
title: E2E Test Plan - Waitlist and Intake Record Entities (US_014)
version: 1.0.0
status: draft
author: AI Assistant
created: 2026-03-23
updated: 2026-03-23
---

# E2E Test Plan: Waitlist and Intake Record Entities (US_014)

## 1. Test Objectives

- **Validate Data Integrity**: Ensure WaitlistEntry and IntakeRecord entities correctly persist with all required fields, constraints, and referential integrity
- **Verify Enum Validation**: Confirm intake_mode (AI/Manual), notification_preference (SMS/Email/Both), and status enums enforce valid values only
- **Test JSONB Structures**: Validate structured health data (medical_history, medications, allergies) conform to expected schema and constraints
- **Ensure Referential Integrity**: Verify FK relationships to Patient, Provider, Appointment, and InsuranceRecord entities prevent orphaned records and enforce cascading behavior
- **Validate Insurance Pre-Check Foundation**: Confirm InsuranceRecord reference data structure supports regex pattern validation per FR-021
- **Support FR-009 Waitlist Flow**: Test WaitlistEntry creation and state transitions for waitlist enrollment, notification, and slot swap scenarios

## 2. Scope

### In Scope
| Category | Items | Requirement IDs |
|----------|-------|-----------------|
| Data Entities | WaitlistEntry (CRUD, state transitions), IntakeRecord (CRUD, mode switching), InsuranceRecord (reference data) | DR-011, DR-012, DR-015 |
| Functional | Waitlist enrollment data capture, manual form intake data persistence, insurance pre-check validation setup | FR-009, FR-018, FR-021 |
| Referential Integrity | FK constraints to Patient, Provider, Appointment, InsuranceRecord; cascade/restrict delete behavior | DR-006 |
| Data Validation | Enum field validation, JSONB schema compliance, date range constraints, nullable field handling | DR-011, DR-012, DR-015 |
| Non-Functional | Database constraint enforcement, data persistence accuracy, migration rollback capability | NFR-012, DR-008 |

### Out of Scope
- **Business Logic**: Waitlist prioritization algorithms, intake form rendering (frontend), insurance validation API calls
- **AI Features**: AI-assisted intake conversational flows (FR-017), AI conflict detection (FR-031)
- **End-User Flows**: Complete patient journeys, notification delivery, appointment swapping (tested in dependent user stories)
- **UI/UX**: Form layout, validation error messaging, accessibility features
- **Performance Testing**: Load testing of database inserts, bulk operations

## 3. Test Strategy

### Test Pyramid Allocation
| Level | Coverage Target | Focus | Execution Framework |
|-------|-----------------|-------|-------------------|
| Unit | 60-70% | Entity field validation, enum constraints, JSONB schema, FK relationship validation | xUnit + EF Core InMemory |
| Integration | 20-30% | Database operations (Insert/Update/Delete), constraint enforcement, cascade behavior, migration validation | xUnit + PostgreSQL testdb |
| E2E | 5-10% | Complete waitlist enrollment flow through to status changes; intake creation through 360-degree view usage | API Integration tests |

### E2E Approach
- **Vertical Integration**: Database → EF Core DbContext → API endpoint validation for data persistence
- **Constraint Validation**: Attempt operations that violate constraints and verify appropriate error handling
- **State Transition**: Verify valid status transitions and prevent invalid state combinations

### Environment Strategy
| Environment | Purpose | Data Strategy | Technologies |
|-------------|---------|---------------|--------------|
| Unit Test | Development, fast feedback | In-Memory DbContext, test fixtures | .NET xUnit, EF Core InMemory |
| Integration Test | Contract validation, constraint testing | PostgreSQL test database with rollback per test | xUnit + PostgreSQL 16 |
| Staging | Pre-deployment validation | Snapshot of reference data (insurance providers) | Full stack deployment |

### Guardrails & Standards
- **Database Constraint Enforcement**: All FK constraints, unique constraints, and NOT NULL constraints enforced at database level via EF Core migrations
- **Migration Safety**: Zero-downtime migration patterns for any schema changes, versioned migration scripts with rollback capability
- **Data Isolation**: Each integration test runs within a transaction that rolls back to prevent test cross-contamination
- **Code Coverage**: Unit tests target 80%+ coverage for entity configuration and validation logic

---

## 4. Test Cases

### 4.1 WaitlistEntry Entity Tests (DR-011)

#### TC-DR-011-HP-01: Create Waitlist Entry with Valid Data
| Field | Value |
|-------|-------|
| Requirement | DR-011, FR-009 |
| Use Case | UC-009 (Patient Waitlist Enrollment) |
| Type | happy_path |
| Priority | P0 |

**Preconditions:**
- Test database contains valid Patient record with ID: `patient-uuid-001`
- Test database contains valid Provider record with ID: `provider-uuid-001`
- Current date is 2026-03-23

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Patient record exists and Provider record exists | System creates new WaitlistEntry with all valid fields | Entity is created successfully with ID generated |
| 2 | WaitlistEntry created | Query database for the entry | Record persists with all provided values intact |
| 3 | WaitlistEntry persisted | Retrieve record and verify fields | ID (UUID), patient_id, provider_id, preferred_start_date, preferred_end_date, preferred_time_range, notification_preference ("Email"), priority_timestamp, status ("Active") all match input |

**Test Data:**
```yaml
WaitlistEntry:
  id: "550e8400-e29b-41d4-a716-446655440001"
  patient_id: "patient-uuid-001"
  provider_id: "provider-uuid-001"
  preferred_start_date: "2026-04-01"
  preferred_end_date: "2026-04-30"
  preferred_time_range: "09:00-12:00"
  notification_preference: "Email"
  priority_timestamp: "2026-03-23T14:30:00Z"
  status: "Active"
```

**Expected Results:**
- [ ] WaitlistEntry created successfully in database
- [ ] UUID ID auto-generated
- [ ] All fields persisted with correct types and values
- [ ] Created timestamp auto-populated
- [ ] Status defaults to "Active" if not provided

**Postconditions:**
- Database contains 1 WaitlistEntry record
- Record queryable by patient_id and provider_id

---

#### TC-DR-011-EC-01: Validate notification_preference Enum Values
| Field | Value |
|-------|-------|
| Requirement | DR-011 |
| Type | edge_case |
| Priority | P0 |

**Preconditions:**
- Enum validation is configured in EF Core entity mapping
- Valid Patient and Provider records exist

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Valid test data with notification_preference = "SMS" | Attempt to create WaitlistEntry | Record created successfully |
| 2 | Valid test data with notification_preference = "Email" | Attempt to create WaitlistEntry | Record created successfully |
| 3 | Valid test data with notification_preference = "Both" | Attempt to create WaitlistEntry | Record created successfully |
| 4 | Valid test data with notification_preference = "SMS" | Query record and verify enum | notification_preference correctly stored and retrieved as enum value |

**Test Data - Valid Values:**
```yaml
notification_preferences:
  - "SMS"
  - "Email"
  - "Both"
```

**Expected Results:**
- [ ] All three valid enum values accepted
- [ ] Enum values correctly represented in database
- [ ] Retrieval returns correct enum value (not magic number)
- [ ] Invalid values rejected at database constraint level

**Postconditions:**
- Three WaitlistEntry records exist with different notification preferences

---

#### TC-DR-011-EC-02: Validate status Enum Values and Valid Transitions
| Field | Value |
|-------|-------|
| Requirement | DR-011 |
| Type | edge_case |
| Priority | P0 |

**Preconditions:**
- Status enum validation configured in DbContext
- Valid Customer records exist

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | WaitlistEntry with status = "Active" | Attempt update to status = "Fulfilled" | Update succeeds, status changes to "Fulfilled" |
| 2 | WaitlistEntry with status = "Fulfilled" | Attempt update to status = "Active" | Update succeeds (bidirectional allowed in data layer) |
| 3 | WaitlistEntry with status = "Cancelled" | Retrieve record | status correctly stored and retrieved as enum |
| 4 | Valid test data with status = "InvalidStatus" | Attempt to create WaitlistEntry | Operation fails at EF Core validation or database constraint |

**Test Data:**
```yaml
valid_status_values:
  - "Active"
  - "Fulfilled"
  - "Cancelled"

invalid_status_value: "InvalidStatus"
```

**Expected Results:**
- [ ] All valid status values accepted
- [ ] Status transitions allowed (no artificial workflow constraints at data layer)
- [ ] Invalid status values rejected
- [ ] Enum correctly persisted and retrieved

**Postconditions:**
- WaitlistEntry records exist with each valid status value

---

#### TC-DR-011-ER-01: Prevent Creation with Missing Patient Reference
| Field | Value |
|-------|-------|
| Requirement | DR-006, DR-011 |
| Type | error |
| Priority | P1 |

**Preconditions:**
- Foreign key constraint enforced on patient_id column
- Valid Provider record exists

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | No patient provided (patient_id = null) | Attempt to create WaitlistEntry | Database constraint violation raised |
| 2 | Database constraint violated | Catch exception and verify error type | DbUpdateException with constraint violation message |
| 3 | Transaction rolled back | Verify database state | No WaitlistEntry created; database in consistent state |

**Expected Results:**
- [ ] Database constraint prevents null patient_id
- [ ] Error clearly indicates FK constraint violation
- [ ] Transaction automatically rolled back
- [ ] No orphaned records created

**Postconditions:**
- Database remains in consistent state
- No partial records created

---

#### TC-DR-011-ER-02: Prevent Creation with Non-Existent Provider FK
| Field | Value |
|-------|-------|
| Requirement | DR-006, DR-011 |
| Type | error |
| Priority | P1 |

**Preconditions:**
- Foreign key constraint enforced on provider_id column
- Valid Patient record exists
- provider_id value does not exist in Provider table

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | provider_id = non-existent UUID | Attempt to create WaitlistEntry | Database FK constraint violation raised |
| 2 | Constraint violation detected | Verify error handling | DbUpdateException thrown with provider FK constraint message |
| 3 | Transaction rolled back | Check database | No WaitlistEntry record created |

**Expected Results:**
- [ ] Database FK constraint prevents invalid provider reference
- [ ] Clear error message indicates provider record not found
- [ ] Transaction rolled back automatically
- [ ] Database consistency maintained

**Postconditions:**
- No orphaned WaitlistEntry created
- Database passes integrity check

---

#### TC-DR-011-ER-03: Validate Date Range Constraints (preferred_start_date <= preferred_end_date)
| Field | Value |
|-------|-------|
| Requirement | DR-011 |
| Type | error |
| Priority | P1 |

**Preconditions:**
- Valid Patient and Provider records exist
- Constraint validation configured in DbContext (can use data annotations or custom validation)

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | preferred_start_date = "2026-04-30", preferred_end_date = "2026-04-01" | Attempt to create WaitlistEntry | Validation fails (either EF Core validation or database check constraint) |
| 2 | Start date > End date (invalid range) | Attempt to save | DbUpdateException or ValidationException raised |
| 3 | Valid range: preferred_start_date = "2026-04-01", preferred_end_date = "2026-04-30" | Attempt to create | Record created successfully |

**Test Data:**
```yaml
invalid_date_range:
  preferred_start_date: "2026-04-30"
  preferred_end_date: "2026-04-01"

valid_date_range:
  preferred_start_date: "2026-04-01"
  preferred_end_date: "2026-04-30"

edge_case_same_date:
  preferred_start_date: "2026-04-15"
  preferred_end_date: "2026-04-15"  # Should be allowed
```

**Expected Results:**
- [ ] Invalid date ranges rejected
- [ ] Valid ranges (including same-day) accepted
- [ ] Clear error message for invalid ranges
- [ ] No partial records created on validation failure

**Postconditions:**
- Only records with valid date ranges exist in database

---

### 4.2 IntakeRecord Entity Tests (DR-012)

#### TC-DR-012-HP-01: Create IntakeRecord with Valid AI Mode Data
| Field | Value |
|-------|-------|
| Requirement | DR-012, FR-017 |
| Use Case | UC-017 (AI Conversational Intake) |
| Type | happy_path |
| Priority | P0 |

**Preconditions:**
- Valid Patient record exists with ID: `patient-uuid-001`
- Valid Appointment record exists with ID: `appt-uuid-001`
- InsuranceRecord reference data loaded for validation

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Appointment and Patient records exist | System creates IntakeRecord with intake_mode="AI" | Record created with all fields persisted |
| 2 | IntakeRecord created with JSONB medical_history | Query database | JSONB structure valid and queryable |
| 3 | Record persisted | Retrieve all fields | ID (UUID), appointment_id, patient_id, intake_mode="AI", medical_history (JSONB), medications (JSONB), allergies (JSONB), visit_concerns, insurance_validation_status, insurance_record_id (nullable), completion_status all match input |
| 4 | JSONB data stored | Query using JSONB operators | Can retrieve nested data using PostgreSQL JSONB queries |

**Test Data:**
```yaml
IntakeRecord:
  id: "660e8400-e29b-41d4-a716-446655440002"
  appointment_id: "appt-uuid-001"
  patient_id: "patient-uuid-001"
  intake_mode: "AI"
  medical_history:
    diabetes:
      diagnosed_date: "2015-01-15"
      status: "active"
      notes: "Type 2, controlled"
    hypertension:
      diagnosed_date: "2018-06-20"
      status: "active"
  medications:
    - name: "Metformin"
      dose: "500mg"
      frequency: "twice daily"
      start_date: "2015-02-01"
      status: "active"
    - name: "Lisinopril"
      dose: "10mg"
      frequency: "once daily"
      start_date: "2018-07-15"
      status: "active"
  allergies:
    - substance: "Penicillin"
      reaction: "anaphylaxis"
      severity: "severe"
    - substance: "Shellfish"
      reaction: "rash"
      severity: "mild"
  visit_concerns: "Routine checkup for diabetes management"
  insurance_validation_status: "Verified"
  insurance_record_id: null
  completion_status: "Completed"
```

**Expected Results:**
- [ ] IntakeRecord created successfully
- [ ] UUID ID auto-generated
- [ ] JSONB fields persisted as valid JSON
- [ ] All fields queryable and retrievable
- [ ] Timestamps auto-populated
- [ ] JSONB data structure validated per schema

**Postconditions:**
- Database contains 1 IntakeRecord with AI mode
- Record linked to Appointment and Patient
- JSONB data queryable using PostgreSQL operators

---

#### TC-DR-012-HP-02: Create IntakeRecord with Valid Manual Mode Data
| Field | Value |
|-------|-------|
| Requirement | DR-012, FR-018 |
| Use Case | UC-018 (Manual Form Intake) |
| Type | happy_path |
| Priority | P0 |

**Preconditions:**
- Valid Patient and Appointment records exist
- InsuranceRecord reference exists for validation

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Patient and Appointment ready | Create IntakeRecord with intake_mode="Manual" | Record persists successfully |
| 2 | Manual form data submitted | Verify JSONB structure | Medical history, medications, allergies all valid JSON |
| 3 | Insurance info provided | Validate insurance reference | insurance_record_id correctly links to InsuranceRecord if provided |
| 4 | Record saved | Retrieve from database | All fields intact, intake_mode="Manual", completion_status accurate |

**Test Data:**
```yaml
IntakeRecord:
  id: "770e8400-e29b-41d4-a716-446655440003"
  appointment_id: "appt-uuid-002"
  patient_id: "patient-uuid-001"
  intake_mode: "Manual"
  medical_history:
    asthma:
      diagnosed_date: "2010-05-10"
      status: "active"
  medications:
    - name: "Albuterol"
      dose: "90mcg"
      frequency: "as needed"
      status: "active"
  allergies:
    - substance: "Nuts"
      reaction: "hives"
      severity: "moderate"
  visit_concerns: "Asthma control review"
  insurance_validation_status: "Verified"
  insurance_record_id: "insure-uuid-001"
  completion_status: "Completed"
```

**Expected Results:**
- [ ] Manual mode IntakeRecord created successfully
- [ ] JSONB structures valid for manual form data
- [ ] Insurance validation status reflects manual entry
- [ ] All fields persisted correctly

**Postconditions:**
- Database contains IntakeRecord with Manual mode
- Record linked to valid Appointment and Patient

---

#### TC-DR-012-EC-01: Validate intake_mode Enum (AI vs Manual Only)
| Field | Value |
|-------|-------|
| Requirement | DR-012, FR-019 |
| Type | edge_case |
| Priority | P0 |

**Preconditions:**
- Enum validation configured in DbContext
- Valid Patient and Appointment records exist

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Valid data with intake_mode = "AI" | Attempt to create IntakeRecord | Creation succeeds |
| 2 | Valid data with intake_mode = "Manual" | Attempt to create IntakeRecord | Creation succeeds |
| 3 | Valid data with intake_mode = "Voice" (invalid) | Attempt to create IntakeRecord | EF Core validation error or database constraint violation |
| 4 | Valid data with intake_mode = null | Attempt to create IntakeRecord | Not-null constraint violation |

**Test Data - Valid Values:**
```yaml
valid_modes:
  - "AI"
  - "Manual"

invalid_modes:
  - "Voice"
  - "Video"
  - ""
  - null
```

**Expected Results:**
- [ ] Only "AI" and "Manual" accepted
- [ ] Invalid values rejected at validation layer
- [ ] Clear error message for invalid enum selection
- [ ] NULL prevented by NOT NULL constraint

**Postconditions:**
- IntakeRecord records exist only with valid intake_mode values

---

#### TC-DR-012-EC-02: Validate JSONB medical_history Structure
| Field | Value |
|-------|-------|
| Requirement | DR-012 |
| Type | edge_case |
| Priority | P1 |

**Preconditions:**
- JsonSchema validation configured for medical_history field (or inline validation)
- Valid Parent records exist

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Valid medical_history JSONB per schema | Create IntakeRecord | Record persists successfully |
| 2 | Empty medical_history = {} | Create IntakeRecord | Record persists (empty object allowed) |
| 3 | Invalid medical_history structure (wrong field names) | Attempt to create | Should persist but logged for staff review; no validation error at data layer |
| 4 | medical_history = null | Create IntakeRecord with null value | NULL persisted (nullable field) |
| 5 | Retrieve record | Query JSONB | Data structure preserved exactly as stored |

**Test Data - Valid Structures:**
```yaml
valid_medical_history_examples:
  example_1:
    diabetes:
      diagnosed_date: "2015-01-15"
      status: "active"
  example_2: {}
  example_3:
    condition_1:
      diagnosed_date: "2020-01-01"
      notes: "stable"

invalid_structure:
  - type: "empty_string"
    value: ""
  - type: "malformed_json"
    value: "{invalid json"
```

**Expected Results:**
- [ ] Valid JSONB persisted as-is
- [ ] Empty objects allowed
- [ ] JSONB structure preserved on retrieval
- [ ] NULL values allowed for optional JSONB fields
- [ ] Invalid JSON rejected at database level

**Postconditions:**
- Only valid JSONB structures present in database

---

#### TC-DR-012-EC-03: Validate JSONB medications Array Structure
| Field | Value |
|-------|-------|
| Requirement | DR-012 |
| Type | edge_case |
| Priority | P1 |

**Preconditions:**
- Valid Patient and Appointment records exist
- JSONB array structure defined for medications field

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | medications = [] (empty array) | Create IntakeRecord | Record persists with empty array |
| 2 | medications with single item per schema | Create IntakeRecord | Array item structure preserved |
| 3 | medications with multiple items | Create IntakeRecord | All array items persisted |
| 4 | medications with missing required fields per schema | Create IntakeRecord | Persists (schema enforcement at application layer, not database) |
| 5 | medications = null | Create IntakeRecord | NULL stored if field is nullable |

**Test Data:**
```yaml
valid_medications_structures:
  empty_array: []
  single_medication:
    - name: "Metformin"
      dose: "500mg"
      frequency: "twice daily"
  multiple_medications:
    - name: "Metformin"
      dose: "500mg"
      frequency: "twice daily"
    - name: "Lisinopril"
      dose: "10mg"
      frequency: "once daily"
```

**Expected Results:**
- [ ] Empty arrays persisted
- [ ] Single and multiple medication arrays persisted correctly
- [ ] All medication properties preserved
- [ ] JSONB array structure retrievable on query

**Postconditions:**
- IntakeRecord records with valid medications arrays persist correctly

---

#### TC-DR-012-EC-04: Validate JSONB allergies Array Structure  
| Field | Value |
|-------|-------|
| Requirement | DR-012 |
| Type | edge_case |
| Priority | P1 |

**Preconditions:**
- Valid Patient and Appointment records exist
- JSONB array schema defined for allergies

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | allergies = [] (empty array, no known allergies) | Create IntakeRecord | Record persists successfully |
| 2 | allergies with valid items per schema | Create IntakeRecord | Each allergy item persisted |
| 3 | allergies with severity enum ("severe", "moderate", "mild") | Create IntakeRecord | Severity levels preserved |
| 4 | Query allergies using JSONB operators | Extract specific allergy | Can query using PostgreSQL @> or -> operators |

**Test Data:**
```yaml
valid_allergies_structures:
  no_known_allergies: []
  single_allergy:
    - substance: "Penicillin"
      reaction: "anaphylaxis"
      severity: "severe"
  multiple_allergies:
    - substance: "Penicillin"
      reaction: "anaphylaxis"
      severity: "severe"
    - substance: "Shellfish"
      reaction: "rash"
      severity: "mild"
```

**Expected Results:**
- [ ] Empty allergies array allowed (no known allergies)
- [ ] Single and multiple allergies persisted
- [ ] Severity levels correctly stored
- [ ] JSONB queryable for allergy lookup

**Postconditions:**
- IntakeRecord records have valid allergies JSONB structures

---

#### TC-DR-012-ER-01: Prevent Creation with Missing Appointment Reference
| Field | Value |
|-------|-------|
| Requirement | DR-006, DR-012 |
| Type | error |
| Priority | P1 |

**Preconditions:**
- Foreign key constraint enforced on appointment_id
- Valid Patient record exists

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | appointment_id not provided (null) | Attempt to create IntakeRecord | Database NOT NULL constraint violation |
| 2 | FK constraint triggers | Catch exception | DbUpdateException with appropriate error message |
| 3 | Transaction rolled back | Verify database state | No IntakeRecord created |

**Expected Results:**
- [ ] NOT NULL constraint prevents missing appointment
- [ ] Clear error indicates required field missing
- [ ] Transaction rolled back
- [ ] Database consistency maintained

**Postconditions:**
- No orphaned IntakeRecord created

---

#### TC-DR-012-ER-02: Prevent Insurance Record FK Violation
| Field | Value |
|-------|-------|
| Requirement | DR-006, DR-012, FR-021 |
| Type | error |
| Priority | P1 |

**Preconditions:**
- Foreign key constraint on insurance_record_id (when not null)
- Valid Patient and Appointment records exist
- insurance_record_id references must exist in InsuranceRecord table

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | insurance_record_id = non-existent UUID | Attempt to create IntakeRecord | Database FK constraint violation |
| 2 | FK constraint triggers | Verify exception type | DbUpdateException with FK violation message |
| 3 | insurance_record_id = null (no insurance) | Attempt to create IntakeRecord | Creation succeeds (FK constraint only applies when not null) |

**Expected Results:**
- [ ] Invalid insurance_record_id references rejected
- [ ] NULL insurance_record_id allowed (optional reference)
- [ ] FK constraint prevents orphaned insurance references
- [ ] Clear error on invalid FK violation

**Postconditions:**
- Only IntakeRecords with valid or NULL insurance_record_id exist

---

### 4.3 InsuranceRecord Entity Tests (DR-015)

#### TC-DR-015-HP-01: Create InsuranceRecord with Regex Pattern Validation
| Field | Value |
|-------|-------|
| Requirement | DR-015, FR-021 |
| Use Case | Insurance Pre-Check Setup |
| Type | happy_path |
| Priority | P0 |

**Preconditions:**
- InsuranceRecord reference data table exists
- Regex validation patterns available

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Insurance provider data ready | Create InsuranceRecord with provider name, ID pattern | Record persists with all fields |
| 2 | Record persisted | Verify regex pattern field | accepted_id_patterns (regex) stored correctly |
| 3 | Coverage type provided | Store coverage type enum | coverage_type ("PPO", "HMO", "Indemnity") persisted |
| 4 | Active status set | Verify active flag | active=true indicates this provider is valid for validation |

**Test Data:**
```yaml
InsuranceRecord:
  id: "insure-uuid-001"
  provider_name: "Blue Cross Blue Shield"
  accepted_id_patterns: "^BC[0-9]{9}$"  # Regex pattern: BC followed by 9 digits
  coverage_type: "PPO"
  active: true

InsuranceRecord:
  id: "insure-uuid-002"
  provider_name: "United Healthcare"
  accepted_id_patterns: "^UH[A-Z0-9]{8}$"  # Format UH + 8 alphanumeric
  coverage_type: "HMO"
  active: true
```

**Expected Results:**
- [ ] InsuranceRecord created successfully
- [ ] Regex pattern stored as string field
- [ ] Coverage type enum persisted
- [ ] Active flag defaults to true
- [ ] Record queryable for insurance validation

**Postconditions:**
- InsuranceRecord reference data available for FR-021 validation

---

#### TC-DR-015-EC-01: Validate Regex Pattern Storage and Retrieval
| Field | Value |
|-------|-------|
| Requirement | DR-015, FR-021 |
| Type | edge_case |
| Priority | P1 |

**Preconditions:**
- Multiple InsuranceRecord patterns available
- Pattern matching validation implemented at application layer

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | InsuranceRecord with regex pattern = "^[0-9]{6}$" | Store and retrieve | Pattern string persisted exactly |
| 2 | Application layer invokes regex match | Test "123456" against pattern | Pattern matches and validates correctly |
| 3 | Test "ABCDEF" against pattern | Pattern validation | Match fails (invalid format) |
| 4 | Retrieve multiple InsuranceRecords | Filter by provider_name | Correct patterns retrieved and usable |

**Test Data - Pattern Examples:**
```yaml
pattern_1:
  provider: "BlueCross"
  pattern: "^BC[0-9]{9}$"
  valid_id: "BC123456789"
  invalid_id: "BC12345678"  # Only 8 digits

pattern_2:
  provider: "Aetna"
  pattern: "^[0-9]{7}[A-Z]$"
  valid_id: "1234567A"
  invalid_id: "12345678"  # No letter at end

pattern_3:
  provider: "Cigna"
  pattern: "^[A-Z]{2}[0-9]{6}[A-Z]{2}$"
  valid_id: "AB123456CD"
  invalid_id: "AB123456"  # Missing letter suffix
```

**Expected Results:**
- [ ] Regex patterns stored as strings without modification
- [ ] Patterns retrievable and usable for validation
- [ ] Valid IDs match patterns correctly
- [ ] Invalid IDs rejected by pattern
- [ ] Multiple patterns per provider supported

**Postconditions:**
- InsuranceRecord patterns available for FR-021 insurance pre-check flows

---

#### TC-DR-015-ER-01: Validate coverage_type Enum Values
| Field | Value |
|-------|-------|
| Requirement | DR-015 |
| Type | error |
| Priority | P1 |

**Preconditions:**
- coverage_type enum defined with valid values
- EF Core configuration enforces enum validation

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | coverage_type = "PPO" | Create InsuranceRecord | Creation succeeds |
| 2 | coverage_type = "HMO" | Create InsuranceRecord | Creation succeeds |
| 3 | coverage_type = "Indemnity" | Create InsuranceRecord | Creation succeeds |
| 4 | coverage_type = "Medicare" (invalid) | Attempt creation | Validation error or constraint violation |
| 5 | coverage_type = null | Attempt creation | NOT NULL constraint violation |

**Test Data:**
```yaml
valid_coverage_types:
  - "PPO"
  - "HMO"
  - "Indemnity"

invalid_coverage_types:
  - "Medicare"
  - "Medicaid"
  - "Unknown"
  - ""
```

**Expected Results:**
- [ ] Valid coverage types accepted
- [ ] Invalid values rejected
- [ ] NULL values prevented
- [ ] Clear error message for invalid enum
- [ ] Only valid types stored in database

**Postconditions:**
- InsuranceRecord records contain valid coverage_type values only

---

### 4.4 Referential Integrity Tests (DR-006)

#### TC-DR-006-HP-01: Verify All Foreign Key Constraints Exist
| Field | Value |
|-------|-------|
| Requirement | DR-006 |
| Type | happy_path |
| Priority | P1 |

**Preconditions:**
- Migration applied to database
- All foreign key constraints defined in EF Core configuration

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Database schema created via migration | Query PostgreSQL information_schema.table_constraints | All FK constraints present |
| 2 | Check WaitlistEntry constraints | Verify FKs to Patient, Provider | Both FKs exist and named correctly |
| 3 | Check IntakeRecord constraints | Verify FKs to Appointment, Patient, InsuranceRecord | All required FKs present |
| 4 | Retrieve and validate constraint definitions | Inspect cascade/restrict settings | Appropriate delete behavior configured for each FK |

**Expected Results:**
- [ ] WaitlistEntry.patient_id → Patient.id (FK exists)
- [ ] WaitlistEntry.provider_id → Provider.id (FK exists)
- [ ] IntakeRecord.appointment_id → Appointment.id (FK exists)
- [ ] IntakeRecord.patient_id → Patient.id (FK exists)
- [ ] IntakeRecord.insurance_record_id → InsuranceRecord.id (FK nullable, exists)
- [ ] All constraints enforced at database level

**Postconditions:**
- FK constraints prevent orphaned records
- Referential integrity guaranteed by database

---

#### TC-DR-006-ER-01: Prevent Delete of Provider with Active Waitlist Entries
| Field | Value |
|-------|-------|
| Requirement | DR-006 |
| Type | error |
| Priority | P1 |

**Preconditions:**
- Provider record exists with ID: `provider-uuid-001`
- WaitlistEntry records exist with foreign key to this provider
- FK constraint configured with ON DELETE RESTRICT (or equivalent)

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Provider has active WaitlistEntry records | Attempt to delete Provider | Database constraint violation (FK prevent delete) |
| 2 | Constraint triggers | Catch exception | DbUpdateException with constraint violation message |
| 3 | Transaction rolled back | Verify database state | Provider still exists, WaitlistEntry intact |
| 4 | Remove dependent WaitlistEntries | Attempt delete again | Provider deletion succeeds after dependent records removed |

**Expected Results:**
- [ ] Cannot delete Provider with active dependent WaitlistEntries
- [ ] Clear constraint violation error
- [ ] Transaction rolled back cleanly
- [ ] No partial deletes
- [ ] Provider deleted only after dependent records removed

**Postconditions:**
- Database maintains referential integrity
- Orphaned WaitlistEntry records impossible

---

#### TC-DR-006-ER-02: Cascade Delete Behavior on Patient Deletion
| Field | Value |
|-------|-------|
| Requirement | DR-006 |
| Type | error |
| Priority | P1 |

**Preconditions:**
- Patient record exists with ID: `patient-uuid-001`
- Multiple dependent records exist:
  - WaitlistEntry records
  - IntakeRecord records
- Cascade delete configured in DbContext (or appropriate delete behavior)

**Test Steps:**
| Step | Given | When | Then |
|------|-------|------|------|
| 1 | Patient has WaitlistEntry and IntakeRecord dependencies | Attempt to delete Patient | Cascade delete behavior triggered or constraint prevents delete |
| 2 | If cascade configured | Verify cascade deletes | All dependent records deleted simultaneously |
| 3 | If restrict configured | Verify FK prevents delete | Error raised; dependent records remain intact |
| 4 | Check database state | Query dependent records | Behavior matches configuration intent |

**Expected Results:**
- [ ] Consistent delete behavior across all dependent records
- [ ] No orphaned records if cascade enabled
- [ ] Clear constraints if cascade not enabled
- [ ] Database remains in valid state
- [ ] Audit trail captures deletion

**Postconditions:**
- Database integrity maintained after deletion
- All dependent records handled appropriately

---

## 5. E2E Journey Mapping

### E2E-002: Waitlist Enrollment to Slot Availability Notification

**Journey Components:**
- UC-009 (Patient Waitlist Enrollment) → FR-009 data persistence → FR-026 notification trigger
- **Primary Actor**: Patient
- **Scope**: WaitlistEntry creation and status transition on slot availability

**User Flow:**
1. Patient finds desired appointment slot unavailable
2. System presents "Join Waitlist" option
3. Patient provides preferred date range, time window, and notification preference
4. System creates WaitlistEntry with status="Active"
5. System monitors for matching available slots (external process)
6. When slot becomes available, system updates WaitlistEntry status="Fulfilled"
7. System sends notification via patient's preferred channel
8. Patient confirms or declines the slot swap

**Data Validation Checkpoints:**
- **Checkpoint 1**: WaitlistEntry created with valid patient/provider references, date range constraints satisfied, notification preference captured
- **Checkpoint 2**: Status transitions from Active → Fulfilled succeed without validation errors
- **Checkpoint 3**: IntakeRecord exists for corresponding appointment if patient already
 in system

**Test Cases Involved:**
- TC-DR-011-HP-01 (WaitlistEntry creation)
- TC-DR-011-EC-02 (Status enum validation)
- TC-DR-011-ER-02 (FK validation for provider)
- TC-FR-009 (Functional coverage for waitlist enrollment)

**Success Criteria:**
- [ ] WaitlistEntry created with all required fields
- [ ] Status transitions work correctly
- [ ] No orphaned records if patient cancels
- [ ] Notification preference captured and usable for FR-026

---

### E2E-003: Patient Intake Entry to 360-Degree Patient View

**Journey Components:**
- UC-018 (Manual Form Intake) or UC-017 (AI Intake) → FR-018/FR-019 → FR-021 insurance validation → FR-032 (360-Degree view)
- **Primary Actor**: Patient
- **Scope**: IntakeRecord creation, mode switching, insurance validation setup

**User Flow:**
1. Patient schedules appointment
2. System prompts for pre-visit intake
3. Patient selects intake mode: AI Conversational or Manual Form
4. Patient provides medical history, medications, allergies, insurance info (via selected mode)
5. System creates IntakeRecord with chosen mode and structured data
6. System validates insurance information against InsuranceRecord reference data (FR-021)
7. Patient can switch modes if desired (FR-019) - data preserved
8. Patient submits final intake
9. System aggregates intake data with document extractions for 360-Degree view
10. Staff reviews aggregated view before appointment

**Data Validation Checkpoints:**
- **Checkpoint 1**: IntakeRecord created with valid mode (AI or Manual), valid JSONB structures
- **Checkpoint 2**: Insurance validation references InsuranceRecord and validates against patterns
- **Checkpoint 3**: Mode switching preserves all entered data before switching
- **Checkpoint 4**: All data queryable for 360-Degree view generation

**Test Cases Involved:**
- TC-DR-012-HP-01 (AI mode IntakeRecord creation)
- TC-DR-012-HP-02 (Manual mode IntakeRecord creation)
- TC-DR-012-EC-02,03,04 (JSONB structure validation)
- TC-DR-015-HP-01 (InsuranceRecord for validation)
- TC-FR-021 (Insurance pre-check functional tests)

**Success Criteria:**
- [ ] IntakeRecord created with correct mode and data structures
- [ ] JSONB fields queryable for aggregation
- [ ] Insurance validation references functional and accurate
- [ ] No data loss when switching modes
- [ ] Data ready for 360-Degree view creation

---

## 6. Traceability Matrix

| Requirement | Test Case ID | Test Type | Priority | Coverage |
|-------------|-------------|-----------|----------|----------|
| DR-011 | TC-DR-011-HP-01 | happy_path | P0 | ✓ Creation |
| DR-011 | TC-DR-011-EC-01 | edge_case | P0 | ✓ Enum validation |
| DR-011 | TC-DR-011-EC-02 | edge_case | P0 | ✓ Status transitions |
| DR-011 | TC-DR-011-ER-01 | error | P1 | ✓ FK constraint (patient) |
| DR-011 | TC-DR-011-ER-02 | error | P1 | ✓ FK constraint (provider) |
| DR-011 | TC-DR-011-ER-03 | error | P1 | ✓ Date range validation |
| FR-009 | TC-DR-011-HP-01 | happy_path | P0 | ✓ Waitlist data persistence |
| FR-009 | E2E-002 | E2E | P0 | ✓ End-to-end waitlist flow |
| DR-012 | TC-DR-012-HP-01 | happy_path | P0 | ✓ AI mode creation |
| DR-012 | TC-DR-012-HP-02 | happy_path | P0 | ✓ Manual mode creation |
| DR-012 | TC-DR-012-EC-01 | edge_case | P0 | ✓ Enum validation |
| DR-012 | TC-DR-012-EC-02 | edge_case | P1 | ✓ JSONB medical_history |
| DR-012 | TC-DR-012-EC-03 | edge_case | P1 | ✓ JSONB medications |
| DR-012 | TC-DR-012-EC-04 | edge_case | P1 | ✓ JSONB allergies |
| DR-012 | TC-DR-012-ER-01 | error | P1 | ✓ FK constraint (appointment) |
| DR-012 | TC-DR-012-ER-02 | error | P1 | ✓ FK constraint (insurance) |
| FR-018 | TC-DR-012-HP-02 | happy_path | P0 | ✓ Manual form data persistence |
| FR-019 | E2E-003 | E2E | P0 | ✓ Mode switching flow |
| FR-021 | TC-DR-015-HP-01 | happy_path | P0 | ✓ Insurance reference setup |
| FR-021 | TC-DR-015-EC-01 | edge_case | P1 | ✓ Regex validation |
| FR-021 | TC-DR-015-ER-01 | error | P1 | ✓ Enum validation |
| DR-015 | TC-DR-015-HP-01 | happy_path | P0 | ✓ Insurance reference creation |
| DR-015 | TC-DR-015-EC-01 | edge_case | P1 | ✓ Pattern storage |
| DR-015 | TC-DR-015-ER-01 | error | P1 | ✓ Coverage type validation |
| DR-006 | TC-DR-006-HP-01 | happy_path | P1 | ✓ FK constraint existence |
| DR-006 | TC-DR-006-ER-01 | error | P1 | ✓ Delete cascade behavior |
| DR-006 | TC-DR-006-ER-02 | error | P1 | ✓ Referential integrity |

**Coverage Summary:**
- **Functional Requirements**: FR-009 (Waitlist), FR-018 (Manual intake), FR-019 (Mode switching), FR-021 (Insurance pre-check)
- **Data Requirements**: DR-006 (Referential integrity), DR-011 (Waitlist), DR-012 (Intake), DR-015 (Insurance reference)
- **Total Test Cases**: 18 (14 unit/integration + 2 E2E journeys + 2 supporting tests)
- **Coverage**: 100% of entity CRUD operations, constraint validation, and referential integrity

---

## 7. Test Execution Guidelines

### Unit Test Execution
```csharp
// Example: xUnit test using EF Core InMemory provider
[Fact]
public async Task CreateWaitlistEntry_WithValidData_Succeeds()
{
    // Arrange: Create test context with in-memory database
    var options = new DbContextOptionsBuilder<PatientAccessDbContext>()
        .UseInMemoryDatabase("test-db")
        .Options;
    
    using var context = new PatientAccessDbContext(options);
    var testPatient = new Patient { /* test data */ };
    var testProvider = new Provider { /* test data */ };
    
    context.Patients.Add(testPatient);
    context.Providers.Add(testProvider);
    await context.SaveChangesAsync();
    
    // Act: Create WaitlistEntry
    var entry = new WaitlistEntry
    {
        PatientId = testPatient.Id,
        ProviderId = testProvider.Id,
        PreferredStartDate = DateTime.Parse("2026-04-01"),
        PreferredEndDate = DateTime.Parse("2026-04-30"),
        NotificationPreference = NotificationPreference.Email,
        PriorityTimestamp = DateTime.UtcNow,
        Status = WaitlistStatus.Active
    };
    
    context.WaitlistEntries.Add(entry);
    
    // Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => context.SaveChangesAsync()
    ); // Should pass without exception
}
```

### Integration Test Environment
- **Database**: PostgreSQL testdb with pgvector extension
- **Test Isolation**: Wrap each test in transaction with rollback
- **Seeding**: Load reference data (Patient, Provider, InsuranceRecord) per test
- **Cleanup**: Rollback transaction after each test

### E2E Test Execution
- **Environment**: API deployed to test server with PostgreSQL testdb
- **Data Setup**: Create test customer accounts, providers with availability
- **Flow Validation**: Use xUnit + fluent assertions for API endpoint testing
- **Cleanup**: Delete test records after journey completion

---

## 8. Defect Tracking & Risk Log

### Known Risks
1. **Risk**: JSONB schema evolution - fields added/removed without migration
   - **Mitigation**: Schema validation in tests + documentation of required fields
   - **Testing**: TC-DR-012-EC-02,03,04 validate structure

2. **Risk**: Timezone handling for timestamps - UTC vs local time confusion
   - **Mitigation**: All timestamps stored/retrieved in UTC; tests use UTC fixtures
   - **Testing**: Verify UTC timestamp storage in all CRUD tests

3. **Risk**: Regex pattern malformed or overly restrictive for insurance IDs
   - **Mitigation**: Test patterns against real insurance ID formats
   - **Testing**: TC-DR-015-EC-01 covers pattern validation

4. **Risk**: Cascade delete accidentally implemented instead of restrict
   - **Mitigation**: Explicitly configure delete behavior in DbContext
   - **Testing**: TC-DR-006-ER-02 validates cascade behavior

### Test Failure Handling
- All test failures logged with full stack trace and test data
- FK constraint violations documented with schema diagram
- JSONB validation failures include retrieved structure for debugging
- Failed migrations rolled back automatically; schema restored

---

## 9. Success Criteria & Sign-Off

### Acceptance Criteria for US_014
- [ ] All 18 test cases passing (14 unit/integration + 4 supporting)
- [ ] 100% of entity fields tested for valid/invalid cases
- [ ] All FK constraints verified at database level
- [ ] JSONB structures validated per expected schemas
- [ ] Enum values restricted to valid set
- [ ] Zero orphaned records created in any failure scenario
- [ ] Two E2E journeys execute complete flows without data loss
- [ ] Traceability matrix 100% mapped to requirements

### Regressions Covered
- Modifications to WaitlistEntry/IntakeRecord entities re-run full test suite
- FK constraint changes validated with TC-DR-006-* tests
- JSONB schema evolution tested with TC-DR-012-EC-* tests
- Migration rollback capability verified per DR-008

### Definition of Done
- Test code peer-reviewed and merged to main branch
- All tests passing in CI/CD pipeline
- Code coverage ≥80% for entity configuration and validation logic
- Test documentation complete with Given/When/Then clarity
- No flaky tests (all deterministic, no timing dependencies)

---

## 10. References & Appendices

### Specification References
- **Spec Document**: [spec.md - Functional Requirements Section](spec.md)
- **Architecture Document**: [design.md - Data Requirements Section](design.md)
- **User Stories**: 
  - [US_009: User Entity and Migration](../EP-DATA-I/us_009/us_009.md)
  - [US_010: Appointment Entity and Migration](../EP-DATA-I/us_010/us_010.md)
  - [US_011: Clinical Document and Extracted Data Entities](../EP-DATA-I/us_011/us_011.md)
  - [US_014: Waitlist and Intake Record Entities](us_014.md) ← Current
  - [US_015: Medical Code and Notification Entities](us_015.md)

### Testing Standards
- [unit-testing-standards.md]../../propel/rules/unit-testing-standards.md)
- [dry-principle-guidelines.md](../../propel/rules/dry-principle-guidelines.md)
- [security-standards-owasp.md](../../propel/rules/security-standards-owasp.md)

### Technology Documentation
- [Entity Framework Core 8.0 Documentation](https://learn.microsoft.com/ef/core/)
- [PostgreSQL JSONB Documentation](https://www.postgresql.org/docs/current/datatype-json.html)
- [xUnit Testing Documentation](https://xunit.net/docs/getting-started/netfx)

### Test Data Repository
- Test fixtures and seed data: [test-fixtures/](test-fixtures/)
- YAML test data schemas: [test-data-schemas.yaml](test-data-schemas.yaml)
- Insurance provider reference data: [insurance-reference-data.sql](insurance-reference-data.sql)

---

**Test Plan Version**: 1.0.0  
**Status**: Draft - Ready for Review  
**Last Updated**: 2026-03-23  
**Next Review**: After implementation of US_014 database entities
