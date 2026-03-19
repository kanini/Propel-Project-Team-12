# Architecture Design

## Project Overview

The Unified Patient Access & Clinical Intelligence Platform is a HIPAA-compliant healthcare system bridging appointment scheduling with clinical data management. Targeting healthcare organizations with patient-facing booking capabilities and staff-managed clinical workflows, the platform serves three user roles (Patients, Staff, Admins) with distinct security boundaries. Key capabilities include intelligent appointment booking with dynamic slot swapping, flexible AI/manual patient intake, multi-document clinical data aggregation with AI-powered extraction, automated ICD-10/CPT code suggestion, and staff-controlled arrival management. The system must deploy on strictly free infrastructure (FR-035 constraint) while maintaining 99.9% uptime, HIPAA compliance, and sub-3-second responsiveness for core booking workflows.

## Architecture Goals

- **Goal 1: Zero-Trust Security Architecture** — Implement defense-in-depth with TLS 1.2+, AES-256 at-rest encryption, immutable audit logging, RBAC with session timeouts, and HIPAA-compliant data handling to protect sensitive patient health information
- **Goal 2: Cost-Constrained Resilience** — Achieve 99.9% uptime (NFR-002) and sub-3-second API response times (NFR-001) while deploying exclusively on free-tier infrastructure (Netlify/Vercel, Render/Supabase), requiring aggressive caching, health check orchestration, and graceful AI feature degradation under resource limits
- **Goal 3: Hybrid AI-Deterministic Architecture** — Blend deterministic booking/RBAC logic with AI-powered conversational intake and clinical data extraction (AIR-001, AIR-002), maintaining strict separation via confidence-scored outputs, explicit conflict resolution workflows, and staff-in-the-loop verification for all AI-generated medical codes
- **Goal 4: Testability & Maintainability** — Structure codebase for ≥80% automated test coverage (NFR-010) using layered architecture with dependency injection, enabling independent testing of business logic, AI model swaps without core refactoring, and zero-downtime schema migrations (DR-008)
- **Goal 5: Scalability Within Free-Tier Boundaries** — Support 500 concurrent users (NFR-003) and 10K patient records (NFR-012) through pgvector-accelerated similarity search, Redis-based session caching, optimistic locking for slot booking, and horizontally scalable .NET API design (despite free-tier compute constraints)

## Non-Functional Requirements

### Performance

- **NFR-001: System MUST respond to booking requests within 3 seconds at 95th percentile**
  - Scope: Appointment slot selection (FR-001), booking confirmation (FR-002), calendar sync initiation
  - Measurement: Application Insights telemetry tracking request duration from API gateway to database commit
  - Acceptance Criteria: p95 latency ≤3000ms; p99 ≤5000ms; failing requests trigger alert

- **NFR-002: System MUST achieve 99.9% monthly uptime (43.2 minutes max downtime)**
  - Scope: Core booking, intake, and clinical data review features
  - Measurement: External uptime monitor (UptimeRobot free tier) pinging health endpoint every 5 minutes
  - Acceptance Criteria: Monthly availability ≥99.9%; planned maintenance windows excluded; SLA breach triggers post-mortem

- **NFR-003: System MUST support 500 concurrent authenticated users without performance degradation**
  - Scope: Peak hours with 300 patients booking + 150 staff managing arrivals + 50 admins
  - Rationale: Typical outpatient clinic with 50-100 daily appointments × 5-10x concurrent booking window
  - Acceptance Criteria: Load testing at 500 virtual users shows p95 latency within SLA; no 5xx errors

- **NFR-004: System MUST generate PDF confirmations within 10 seconds of booking**
  - Scope: Appointment confirmation PDF generation (FR-007)
  - Rationale: Non-blocking operation, user receives email asynchronously
  - Acceptance Criteria: Background job completes within 10s; failures retry 3x with exponential backoff

- **NFR-005: System MUST extract patient data from PDFs within 30 seconds**
  - Scope: AI-powered clinical document extraction (FR-022)
  - Rationale: Staff tolerance for data prep workflow; blocking low-confidence extractions
  - Acceptance Criteria: Extraction job completes within 30s for documents ≤10 pages; larger documents chunked

### Security

- **NFR-006: System MUST encrypt all data transmission using TLS 1.2 or higher**
  - Scope: All client-server communication, API calls, calendar sync, email/SMS gateways
  - Justification: HIPAA Technical Safeguards § 164.312(e)(1) encryption requirement
  - Acceptance Criteria: SSL Labs scan reports A rating; deprecated protocol handshakes rejected; HSTS header enforced

- **NFR-007: System MUST encrypt all data at rest using AES-256**
  - Scope: PostgreSQL database files, uploaded PDF files, session storage, backup archives
  - Justification: HIPAA Security Rule § 164.312(a)(2)(iv) encryption standard
  - Acceptance Criteria: Database encryption enabled; PDF blob storage encrypted; encryption keys stored in separate vault

- **NFR-008: System MUST implement HIPAA-compliant audit logging**
  - Scope: All CRUD operations on PHI (patient records, clinical documents, intake forms)
  - Justification: HIPAA § 164.308(a)(1)(ii)(D) audit controls requirement
  - Acceptance Criteria: Immutable append-only log table; logs include user ID, action, timestamp, IP, affected resources; 7-year retention (DR-006)

- **NFR-009: System MUST enforce role-based access control (RBAC) for Patient, Staff, and Admin roles**
  - Scope: UI component visibility, API endpoint authorization, database row-level security
  - Justification: HIPAA § 164.308(a)(4) workforce access control
  - Acceptance Criteria: JWT tokens with role claims; middleware rejects unauthorized requests; staff cannot access admin functions; patients cannot view other patients' records

- **NFR-010: System MUST implement automatic session timeout after 15 minutes of inactivity**
  - Scope: All authenticated sessions (Patient, Staff, Admin)
  - Justification: HIPAA § 164.312(a)(2)(iii) automatic logoff requirement; mitigate session hijacking risk
  - Acceptance Criteria: Inactivity timer tracks user actions; warning at 13 min; logout at 15 min; unsaved data preserved in secure storage for 1 hour

- **NFR-011: System MUST prevent concurrent session usage per user account**
  - Scope: Single active session per user across all devices
  - Justification: Reduce credential sharing risk; enforce accountability
  - Acceptance Criteria: New login invalidates previous session; user notified of forced logout

### Availability

- **NFR-012: System MUST implement health check endpoints for readiness and liveness probes**
  - Scope: API health endpoint, database connectivity check, external API availability
  - Justification: Enable monitoring and auto-restart within free-tier platform capabilities
  - Acceptance Criteria: /health/live returns 200 if API responsive; /health/ready returns 200 if database connected; orchestrator restarts on consecutive failures

- **NFR-013: System MUST display maintenance mode page during planned downtime**
  - Scope: Scheduled updates, database migrations
  - Justification: User communication for uptime transparency
  - Acceptance Criteria: Static maintenance.html served when API unavailable; estimated restoration time displayed

### Scalability

- **NFR-014: System MUST scale to 10,000 patient records with sub-linear query performance**
  - Scope: Patient search, appointment history retrieval, clinical data aggregation
  - Justification: Multi-location clinic with 5-10 year patient history
  - Acceptance Criteria: Database indexes on search fields (name, DOB, email); query plan shows index usage; p95 query time ≤100ms at 10K records

- **NFR-015: System MUST handle concurrent appointment bookings with optimistic locking**
  - Scope: Slot availability checks, booking transactions
  - Justification: Prevent double-booking race conditions during peak booking windows
  - Acceptance Criteria: Row-level locking or ETag-based optimistic concurrency; failed booking shows "Slot no longer available"; retry mechanism for concurrent conflicts

### Reliability

- **NFR-016: System MUST retry failed external API calls with exponential backoff**
  - Scope: Calendar sync (FR-005, FR-006), email (FR-008), SMS (FR-013)
  - Justification: Network transient failures should not block core workflows
  - Acceptance Criteria: 3 retry attempts with 1s, 2s, 4s delays; circuit breaker opens after 5 consecutive failures; fallback to manual sync (.ics file)

- **NFR-017: System MUST validate all user inputs with sanitization and type checking**
  - Scope: Forms, API payloads, file uploads
  - Justification: Prevent injection attacks (SQL, XSS, path traversal)
  - Acceptance Criteria: Server-side validation mirrors client-side; rejected payloads return 400 with error details; file uploads scanned for malware (free VirusTotal API)

### Maintainability

- **NFR-018: System MUST achieve ≥80% automated test coverage for critical paths**
  - Scope: Booking logic, RBAC, AI extraction, data consolidation, audit logging
  - Justification: Enable confident refactoring and AI model swaps
  - Acceptance Criteria: xUnit backend tests, Jest/React Testing Library frontend tests; coverage report in CI pipeline; <80% fails merge

- **NFR-019: System MUST support zero-downtime database schema migrations**
  - Scope: Add columns, indexes, tables (not breaking changes)
  - Justification: Maintain uptime during frequent deployments
  - Acceptance Criteria: Migrations use non-blocking DDL (e.g., CREATE INDEX CONCURRENTLY); backward-compatible schema changes; rollback scripts available

### Usability

- **NFR-020: System MUST meet WCAG 2.1 Level AA accessibility standards**
  - Scope: Patient and staff web interfaces
  - Justification: Compliance with ADA; inclusive design for users with disabilities
  - Acceptance Criteria: Automated axe-core testing in CI; keyboard navigation functional; screen reader compatible; color contrast ratio ≥4.5:1

- **NFR-021: System MUST render responsively on desktop (≥1280px), tablet (768-1279px), and mobile (≤767px) viewports**
  - Scope: All UI components
  - Justification: Multi-device access for patients booking from phones, staff using clinic tablets
  - Acceptance Criteria: CSS media queries; touch-friendly tap targets (≥44px); no horizontal scroll; Playwright visual regression tests

### Compliance

- **NFR-022: System MUST comply with HIPAA Security Rule (45 CFR Part 164)**
  - Scope: All technical, administrative, and physical safeguards applicable to software systems
  - Justification: Legal requirement for handling PHI
  - Acceptance Criteria: Pre-launch HIPAA compliance audit; Business Associate Agreements (BAAs) with free-tier providers supporting them (e.g., Supabase); risk assessment documented

- **NFR-023: System MUST implement OWASP Top 10 mitigations**
  - Scope: Injection, broken authentication, sensitive data exposure, XXE, broken access control, security misconfiguration, XSS, insecure deserialization, using components with known vulnerabilities, insufficient logging
  - Justification: Industry-standard security baseline
  - Acceptance Criteria: Dependency scanning in CI (npm audit, dotnet list package --vulnerable); content security policy headers; parameterized queries

## Data Requirements

### Data Structures

- **DR-001: System MUST define Patient entity with unique email-based identifier**
  - Attributes: PatientID (GUID), Email (unique), FirstName, LastName, DateOfBirth, Phone, CreatedAt, UpdatedAt
  - Relationships: 1:N with Appointments, 1:N with ClinicalDocuments, 1:1 with IntakeForm
  - Justification: Email as natural key for patient identity; GUID for internal references
  - Acceptance Criteria: Email uniqueness enforced via database constraint; GUID auto-generated on insert

- **DR-002: System MUST define Appointment entity with optimistic locking for concurrent booking**
  - Attributes: AppointmentID (GUID), PatientID (FK), ProviderID (FK), SlotDateTime, Status (Scheduled/Arrived/Cancelled), BookedAt, Version (RowVersion)
  - Relationships: N:1 with Patient, N:1 with Provider, 1:1 with PreferredSlotSwapRequest (optional)
  - Justification: RowVersion enables optimistic concurrency control (NFR-015)
  - Acceptance Criteria: Concurrent booking attempts conflict on RowVersion mismatch; failed transaction returns 409 Conflict

- **DR-003: System MUST define ClinicalDocument entity with vector embeddings for AI search**
  - Attributes: DocumentID (GUID), PatientID (FK), DocumentType (Lab/Imaging/Referral), UploadedAt, FilePath (encrypted blob reference), EmbeddingVector (pgvector, 1536 dimensions)
  - Relationships: N:1 with Patient, 1:N with ExtractedData
  - Justification: pgvector extension enables similarity search for RAG (AIR-R02)
  - Acceptance Criteria: Embedding generated on upload; cosine similarity index created; retrieval queries use vector distance

- **DR-004: System MUST define ExtractedData entity with confidence scoring**
  - Attributes: ExtractedDataID (GUID), DocumentID (FK), DataType (Vital/Medication/Diagnosis/Allergy), Value (JSON), ConfidenceScore (0.0-1.0), VerifiedByStaffID (FK, nullable), ExtractedAt
  - Relationships: N:1 with ClinicalDocument, N:1 with Staff (verifier)
  - Justification: Track AI extraction accuracy; enable staff verification workflow (FR-022)
  - Acceptance Criteria: ConfidenceScore <0.7 flags for manual review; staff verification updates VerifiedByStaffID and timestamp

- **DR-005: System MUST define AuditLog entity with immutable append-only storage**
  - Attributes: AuditLogID (GUID), UserID (FK), Action (Create/Read/Update/Delete), EntityType, EntityID, Timestamp, IPAddress, UserAgent
  - Relationships: N:1 with User (polymorphic: Patient/Staff/Admin)
  - Justification: HIPAA audit log requirement (NFR-008)
  - Acceptance Criteria: No UPDATE or DELETE permissions on table; database trigger blocks modifications; 7-year retention policy (DR-006)

### Data Integrity

- **DR-006: System MUST enforce referential integrity via foreign key constraints**
  - Scope: All entity relationships (Patient-Appointment, Document-ExtractedData, User-AuditLog)
  - Justification: Prevent orphaned records; maintain data consistency
  - Acceptance Criteria: Database foreign keys with CASCADE or RESTRICT delete rules; failed constraint violations return 400 with descriptive error

- **DR-007: System MUST validate unique identifiers (email, appointment slot, document ID)**
  - Scope: Patient email uniqueness, appointment slot double-booking prevention, document GUID uniqueness
  - Justification: Business logic constraints beyond referential integrity
  - Acceptance Criteria: Unique indexes on constrained columns; duplicate insert attempts return 409 Conflict with field-specific error

### Data Retention

- **DR-008: System MUST retain audit logs for 7 years per HIPAA recordkeeping requirements**
  - Scope: All audit log entries (DR-005)
  - Justification: HIPAA § 164.530(j)(2) retention standard
  - Acceptance Criteria: Automated job archives logs older than 7 years to cold storage; active logs partitioned by year for query performance

- **DR-009: System MUST retain patient clinical documents for duration of active patient status**
  - Scope: Uploaded PDFs, extracted data, post-visit notes
  - Justification: Clinical care continuity requirement
  - Acceptance Criteria: Soft delete with IsDeleted flag; retention policy TBD based on legal counsel (typically 7-10 years post-last-visit)

### Data Backup

- **DR-010: System MUST perform daily automated database backups with point-in-time recovery capability**
  - Scope: PostgreSQL database (all tables)
  - Justification: Disaster recovery; protection against data corruption or accidental deletion
  - Acceptance Criteria: Supabase free tier provides 7-day point-in-time recovery; backup restoration tested quarterly; RTO ≤4 hours, RPO ≤24 hours

- **DR-011: System MUST backup uploaded clinical documents to geographically separate storage**
  - Scope: Encrypted PDF blob storage
  - Justification: Redundancy for critical patient data
  - Acceptance Criteria: Free-tier cloud storage replication (e.g., Supabase Storage or Cloudflare R2 free tier); backup verified monthly

### Data Migration

- **DR-012: System MUST support zero-downtime schema migrations for additive changes**
  - Scope: Add columns, indexes, tables; not breaking changes (drop column, change type)
  - Justification: Maintain uptime (NFR-002) during frequent deployments
  - Acceptance Criteria: Entity Framework Core migrations use non-blocking DDL; forward-compatible application code deploys before migration; rollback procedure documented

- **DR-013: System MUST version database schema with semantic versioning**
  - Scope: Migration scripts tracked in version control
  - Justification: Traceability for schema changes; correlation with application releases
  - Acceptance Criteria: Migration files named YYYYMMDD_HHMM_Description.sql; applied migrations recorded in __EFMigrationsHistory table

### Domain Entities

- **Patient**: Represents registered users seeking healthcare appointments. Attributes: unique email identifier, personal demographics (name, DOB, phone), authentication credentials (hashed password), role (Patient), account status (Active/Inactive), createdAt timestamp. Relationships: 1:N with Appointments, 1:N with ClinicalDocuments, 1:1 with IntakeForm, 1:N with AuditLogs. Business rules: Email uniqueness enforced; inactive patients cannot book appointments; soft delete preserves audit trail.

- **Staff**: Represents administrative and clinical personnel managing patient flow. Attributes: unique email, name, role (Staff), specialization (FrontDesk/CallCenter/ClinicalReview), employeeID, hireDate, account status. Relationships: 1:N with ProcessedWalkIns, 1:N with VerifiedExtractions, 1:N with AuditLogs. Business rules: Cannot access admin functions; can view only assigned patients' clinical data; session timeout enforced (NFR-010).

- **Admin**: Represents system administrators managing user accounts and configuration. Attributes: unique email, name, role (Admin), privileges (UserManagement/SystemConfig/AuditLogAccess). Relationships: 1:N with ManagedUsers, 1:N with ConfigChanges, 1:N with AuditLogs. Business rules: Full system access; cannot delete audit logs; must use MFA (future enhancement).

- **Appointment**: Represents scheduled patient visits. Attributes: appointmentID (GUID), patientID (FK), providerID (FK), slotDateTime (timestamp), duration (minutes), status (Scheduled/Arrived/Completed/Cancelled/NoShow), bookingSource (Online/WalkIn/Staff), preferredSlotSwapRequestID (FK, nullable), rowVersion (optimistic concurrency). Relationships: N:1 with Patient, N:1 with Provider, 1:1 with PreferredSlotSwapRequest, 1:N with Reminders. Business rules: Slot uniqueness per provider; concurrent booking conflict on rowVersion mismatch; past appointments immutable (status only).

- **AppointmentSlot**: Represents provider availability windows. Attributes: slotID, providerID (FK), startDateTime, endDateTime, isAvailable (boolean), slotType (Regular/WalkIn/Emergency). Relationships: N:1 with Provider, 1:1 with Appointment (if booked). Business rules: Slot marked unavailable on booking; released on cancellation; overlapping slots prevented via database constraint.

- **ClinicalDocument**: Represents uploaded patient medical records. Attributes: documentID (GUID), patientID (FK), documentType (Lab/Imaging/Referral/PostVisitNote), uploadedAt, uploadedByUserID (FK), filePath (encrypted blob URL), fileSize, embeddingVector (pgvector[1536]), processingStatus (Pending/Completed/Failed). Relationships: N:1 with Patient, 1:N with ExtractedData, 1:1 with UploadAuditLog. Business rules: File size limit 10MB (free-tier constraint); PDF format only (Phase 1); virus scan before storage (VirusTotal free API).

- **ExtractedData**: Represents AI-extracted structured information from clinical documents. Attributes: extractedDataID (GUID), documentID (FK), dataType (Vital/Medication/Diagnosis/Allergy/Procedure/LabValue), value (JSON schema), confidenceScore (0.0-1.0), extractedAt, verifiedByStaffID (FK, nullable), verifiedAt (nullable), sourceDocumentPageNumber. Relationships: N:1 with ClinicalDocument, N:1 with Staff (verifier). Business rules: ConfidenceScore <0.7 requires staff verification; conflicting extractions flagged in ConsolidatedPatientView; verified data immutable.

- **ConsolidatedPatientView**: Materialized view aggregating patient data from intake forms, extracted clinical data, and post-visit notes. Attributes: patientID (FK), medications (JSON array with sources), allergies (JSON array with sources), diagnoses (JSON array with ICD-10 codes), vitals (JSON with timestamps), conflictFlags (JSON array). Relationships: 1:1 with Patient. Business rules: Refreshed on document upload or extraction completion; conflicts highlighted with source document references; staff must acknowledge conflicts before appointment starts.

- **IntakeForm**: Represents patient-provided pre-appointment questionnaire. Attributes: intakeFormID (GUID), patientID (FK), appointmentID (FK), intakeMethod (AIConversational/ManualForm), completedAt, lastEditedAt, demographics (JSON), medicalHistory (JSON), currentMedications (JSON), allergies (JSON), reasonForVisit (text), editHistory (JSON array with timestamps). Relationships: 1:1 with Patient, 1:1 with Appointment. Business rules: Editable up to 24 hours before appointment (FR-012); AI conversational responses stored for extraction; toggle between AI/manual preserves data (FR-011).

- **AuditLog**: Immutable record of all system actions involving PHI. Attributes: auditLogID (GUID), userID (FK), userRole (Patient/Staff/Admin), action (Create/Read/Update/Delete/Login/Logout), entityType, entityID, timestamp, ipAddress, userAgent, changeDetails (JSON). Relationships: N:1 with User (polymorphic). Business rules: Append-only table; no modifications permitted; 7-year retention (DR-008); indexed by timestamp and userID for compliance reporting.

- **PreferredSlotSwapRequest**: Tracks patient preference for alternate appointment slots. Attributes: swapRequestID (GUID), appointmentID (FK), preferredSlotDateTime, requestedAt, status (Pending/Swapped/Expired/Cancelled), swappedAt (nullable). Relationships: 1:1 with Appointment. Business rules: Expires if preferred slot not available by appointment date; automated swap triggers on slot availability (UC-010); patient notified via email/SMS on swap.

- **Reminder**: Tracks automated appointment reminders sent to patients. Attributes: reminderID (GUID), appointmentID (FK), reminderType (Email/SMS), scheduledFor (timestamp), sentAt (nullable), deliveryStatus (Pending/Sent/Failed/Bounced), confirmationReceived (boolean), confirmationMethod (Reply/Link). Relationships: N:1 with Appointment. Business rules: Email reminders at T-48h and T-24h (FR-014); SMS reminder at T-24h (FR-013); failed deliveries retry 3x; confirmations update appointment confidence score.

## AI Consideration

**Status:** Applicable

**Rationale:** spec.md contains 6 `[AI-CANDIDATE]` requirements (FR-009, FR-022, FR-023, FR-025, FR-026, FR-027) and 2 `[HYBRID]` requirements (FR-003, FR-015), indicating significant AI/ML components. AI fit assessment scores multiple features as HIGH-FIT (5/5) for NLU-based conversational intake, document extraction, entity resolution, and medical code suggestion. Project requires hybrid architecture blending deterministic logic (booking, RBAC, audit) with AI-powered data extraction and conversational interfaces.

## AI Requirements

### AI Functional Requirements

- **AIR-001: System MUST provide conversational AI intake using natural language understanding**
  - Pattern: Tool Calling + RAG (fetch patient history for context)
  - Scope: Patient intake questionnaire (FR-009) collecting demographics, medical history, medications, allergies, reason for visit
  - Justification: Improves user experience (NFR-020) by allowing free-text responses vs rigid form fields; extracts structured data for downstream workflows
  - Input: Patient conversational text responses (e.g., "I've been taking aspirin for 5 years, 81mg daily")
  - Output: Structured JSON (e.g., `{medication: "Aspirin", dosage: "81mg", frequency: "daily", duration: "5 years"}`)
  - Acceptance Criteria: >95% field completion accuracy (FR-009); patient can pause/resume conversation; toggle to manual form without data loss (FR-011)

- **AIR-002: System MUST extract patient clinical data from uploaded PDF documents with confidence scoring**
  - Pattern: RAG (retrieve similar medical terms from knowledge base) + Named Entity Recognition
  - Scope: Multi-format PDF uploads (lab reports, imaging results, referral notes) per FR-022
  - Justification: Reduces staff time from 20 minutes to 2 minutes for clinical prep (business justification)
  - Input: PDF document (binary)
  - Output: Structured data (vitals, diagnoses, medications, allergies, procedures, lab values) with confidence scores (High >90%, Medium 70-90%, Low <70%)
  - Acceptance Criteria: Extraction completes within 30 seconds (NFR-005); low-confidence items flagged for staff verification; extraction logged in AuditLog

- **AIR-003: System MUST consolidate multi-document patient data with conflict detection**
  - Pattern: Entity Resolution + Rule-Based Conflict Detection
  - Scope: 360-Degree Patient View aggregation (FR-023) from intake forms, uploaded documents, post-visit notes
  - Justification: Prevent clinical errors from conflicting medication lists or allergy mismatches
  - Input: Multiple ExtractedData entries from different documents
  - Output: Deduplicated ConsolidatedPatientView with explicit conflict flags (e.g., "Document A: No allergies" vs "Document B: Penicillin allergy")
  - Acceptance Criteria: Conflicts highlighted in red warning UI (FR-024); staff must acknowledge before appointment; resolution logged

- **AIR-004: System MUST suggest ICD-10 codes from aggregated patient data with confidence scores**
  - Pattern: RAG (retrieve ICD-10 code definitions) + Few-Shot Classification
  - Scope: Medical coding assistance (FR-026)
  - Justification: Automate code suggestion to reduce administrative burden; maintain >98% AI-human agreement rate
  - Input: Patient diagnoses, symptoms, procedures from ConsolidatedPatientView
  - Output: Ranked list of ICD-10 codes with confidence scores and code descriptions
  - Acceptance Criteria: >90% relevance accuracy (FR-026); staff can accept/reject/modify; accepted codes saved to patient record

- **AIR-005: System MUST suggest CPT codes from procedure documentation with confidence scores**
  - Pattern: RAG (retrieve CPT code definitions) + Few-Shot Classification
  - Scope: Medical coding assistance (FR-027)
  - Justification: Automate procedural code suggestion for billing workflows
  - Input: Procedures performed (from post-visit notes or extracted data)
  - Output: Ranked list of CPT codes with confidence scores, modifiers, and billing requirements
  - Acceptance Criteria: >90% relevance accuracy (FR-027); staff can accept/reject/modify; accepted codes saved to patient record

- **AIR-006: System MUST provide source citations for all AI-generated clinical data**
  - Pattern: RAG Provenance Tracking
  - Scope: All AI extractions (AIR-002), consolidations (AIR-003), code suggestions (AIR-004, AIR-005)
  - Justification: "Trust-First" architecture requires verifiable source attribution; HIPAA audit compliance
  - Acceptance Criteria: Each ExtractedData entry includes sourceDocumentID and pageNumber; UI displays "view source" link; clicked link opens PDF at relevant page

- **AIR-007: System MUST fallback to manual workflow when AI confidence below threshold**
  - Pattern: Confidence-Based Routing
  - Scope: Low-confidence extractions (<70% per FR-022), failed model invocations
  - Justification: Graceful degradation ensures core workflows remain functional during AI failures
  - Acceptance Criteria: ConfidenceScore <0.7 routes to staff verification queue; model timeout (>30s) shows "Manual Entry Required" message; circuit breaker triggers after 5 consecutive failures

### AI Quality Requirements

- **AIR-Q01: System MUST maintain extraction accuracy ≥95% on evaluation dataset**
  - Metric: F1 score for extracted entities (medications, diagnoses, vitals) against human-labeled gold standard
  - Measurement: Weekly batch evaluation on 100 randomly sampled documents from production; stratified by document type (lab/imaging/referral)
  - Acceptance Criteria: F1 ≥0.95; alerts trigger retraining if <0.90 for 2 consecutive weeks

- **AIR-Q02: System MUST maintain hallucination rate <5% for AI-generated content**
  - Metric: Percentage of extracted data points not present in source document (false positives)
  - Measurement: Human auditor reviews 10% of AI extractions weekly; flags hallucinations
  - Acceptance Criteria: Hallucination rate ≤5%; >5% triggers model rollback or prompt engineering iteration

- **AIR-Q03: System MUST achieve p95 latency ≤3 seconds for conversational intake responses**
  - Metric: Time from user message submission to AI response rendered in UI
  - Measurement: Application Insights timing from API request to response stream completion
  - Acceptance Criteria: p95 ≤3000ms; p99 ≤5000ms; >5s responses show "Thinking..." indicator

- **AIR-Q04: System MUST enforce output schema validity ≥98% for structured extractions**
  - Metric: Percentage of AI outputs passing JSON schema validation (e.g., medication dosage as string, not embedded in narrative)
  - Measurement: Runtime schema validation using JSON Schema Draft 7; failed validations logged
  - Acceptance Criteria: ≥98% valid schemas; invalid outputs routed to manual entry; schema errors trigger prompt refinement

### AI Safety Requirements

- **AIR-S01: System MUST redact PII from prompts before model invocation when not clinically necessary**
  - Scope: Patient names, email addresses, phone numbers in conversational intake (AIR-001) when querying general medical knowledge
  - Justification: Minimize PHI exposure to external AI providers; HIPAA minimum necessary standard
  - Acceptance Criteria: Regex-based PII detection; redacted prompts logged; clinical context (diagnosis, medications) preserved; implemented via middleware

- **AIR-S02: System MUST enforce document ACL filtering in vector retrieval for RAG**
  - Scope: Clinical document retrieval (AIR-002, AIR-004, AIR-005)
  - Justification: Staff should only retrieve documents for patients they are authorized to view (RBAC per NFR-009)
  - Acceptance Criteria: pgvector similarity query includes WHERE patientID IN (authorized_patient_ids) filter; unauthorized retrieval blocked at database level

- **AIR-S03: System MUST log all AI prompts and responses for HIPAA audit compliance**
  - Scope: Conversational intake (AIR-001), extraction (AIR-002), code suggestion (AIR-004, AIR-005)
  - Justification: HIPAA audit trail requirement (NFR-008) extends to AI-generated content
  - Acceptance Criteria: Prompt/response pairs stored in AuditLog with action type "AIInference"; retention 7 years (DR-008); includes modelVersion, tokenCount, latency

- **AIR-S04: System MUST validate and sanitize AI-generated outputs before database storage**
  - Scope: Extracted medications, diagnoses, vitals (AIR-002)
  - Justification: Prevent injection attacks via malicious prompts generating SQL/XSS payloads
  - Acceptance Criteria: Output validation using whitelist of allowed characters; SQL injection patterns blocked; stored outputs HTML-escaped in UI

### AI Operational Requirements

- **AIR-O01: System MUST enforce token budget of 4000 tokens per conversational intake session**
  - Justification: Cost control under free-tier constraint (FR-035); prevent runaway costs from extended conversations
  - Scope: Conversational AI intake (AIR-001)
  - Acceptance Criteria: Middleware tracks cumulative tokens per session; >4000 triggers graceful handoff to manual form; user notified "Please complete remaining fields manually"

- **AIR-O02: System MUST implement circuit breaker for model provider failures**
  - Justification: Graceful degradation (AIR-007) when AI provider unavailable
  - Scope: All AI model invocations (conversational, extraction, coding)
  - Acceptance Criteria: Circuit opens after 5 consecutive failures (timeout or 5xx status); remains open for 60s; health check call attempts closure; fallback to manual workflows during open state

- **AIR-O03: System MUST support model version rollback within 1 hour**
  - Justification: Rapid mitigation for degraded model performance (AIR-Q01, AIR-Q02)
  - Scope: Model version configuration stored in database; application references active version
  - Acceptance Criteria: Admin can toggle modelVersion flag via UI; API reads flag on startup; no code redeployment required; rollback tested quarterly

- **AIR-O04: System MUST cache repeated AI inferences for identical inputs**
  - Justification: Cost optimization; latency reduction for common queries
  - Scope: ICD-10/CPT code suggestions for identical diagnosis/procedure text
  - Acceptance Criteria: Redis cache with TTL 7 days; cache key hashed from input text + modelVersion; cache invalidation on model update

- **AIR-O05: System MUST monitor and alert on AI model cost exceeding budget threshold**
  - Justification: Early warning for cost overruns under free-tier constraint (FR-035)
  - Scope: All token consumption across conversational, extraction, coding
  - Acceptance Criteria: Daily aggregation of token counts; alert if projected monthly cost >$50 (free trial buffer); dashboard displays current spend

### RAG Pipeline Requirements

- **AIR-R01: System MUST chunk clinical documents into 512-token segments with 20% overlap**
  - Justification: Balance context window (4096 tokens) with retrieval precision; overlap prevents information loss at chunk boundaries
  - Scope: PDF document processing (AIR-002)
  - Acceptance Criteria: Chunking algorithm splits on paragraph boundaries; each chunk ≤512 tokens; 102-token overlap (20%); chunk metadata includes documentID and pageRange

- **AIR-R02: System MUST retrieve top-5 chunks with cosine similarity ≥0.7 for RAG context**
  - Justification: Precision-recall tradeoff for clinical accuracy; avoid irrelevant context dilution
  - Scope: Medical code suggestion (AIR-004, AIR-005), conversational intake history retrieval (AIR-001)
  - Acceptance Criteria: pgvector cosine similarity query with LIMIT 5 and WHERE similarity ≥0.7; if <5 chunks meet threshold, fallback to top-3 regardless of score

- **AIR-R03: System MUST re-rank retrieved chunks using semantic relevance scoring**
  - Justification: Improve retrieval quality beyond cosine similarity; prioritize clinically relevant passages
  - Scope: All RAG retrievals (AIR-001, AIR-002, AIR-004, AIR-005)
  - Acceptance Criteria: Re-ranking model (e.g., cross-encoder) scores query-chunk pairs; final ranking by re-rank score; top-3 re-ranked chunks used in prompt

- **AIR-R04: System MUST generate embeddings for uploaded documents within 5 minutes of upload**
  - Justification: Enable timely retrieval for staff clinical review workflows; non-blocking background job
  - Scope: Clinical document uploads (FR-022)
  - Acceptance Criteria: Background job triggered on PDF upload; embedding API called for each chunk; vectors stored in pgvector column; completion status tracked in ClinicalDocument.processingStatus

- **AIR-R05: System MUST use hybrid search combining vector similarity and keyword matching**
  - Justification: Improve recall for exact medical terms (e.g., "hypertension" vs synonyms)
  - Scope: Clinical document retrieval (AIR-002, AIR-004, AIR-005)
  - Acceptance Criteria: PostgreSQL full-text search (tsvector) + pgvector cosine similarity; weighted combination (0.7 vector + 0.3 keyword); BM25 ranking for keyword component

### AI Architecture Pattern

**Selected Pattern:** Hybrid (RAG + Tool Calling + Rule-Based Routing)

**Rationale:**
- **RAG**: Required for conversational intake (AIR-001) to fetch patient history context, clinical document extraction (AIR-002) to retrieve medical knowledge base for entity disambiguation, and medical coding (AIR-004, AIR-005) to retrieve ICD-10/CPT code definitions with examples
- **Tool Calling**: Conversational intake (AIR-001) needs structured data extraction (tool: extractIntakeField) and calendar slot availability checks (tool: checkSlotAvailability); medical coding may invoke external CPT validation API
- **Rule-Based Routing**: Confidence-based fallback (AIR-007) to manual workflows; conflict detection (AIR-003) uses deterministic logic to compare extracted values; RBAC enforcement (AIR-S02) hard-codes authorization checks

**Architecture Justification:**
- **No Fine-Tuning**: Rapidly changing medical codes and clinic-specific terminology make fine-tuning impractical; RAG enables dynamic knowledge updates without retraining
- **Deterministic Safety Layer**: All AI outputs pass through rule-based validation (AIR-S04) and confidence thresholding (AIR-007) before storage; staff-in-the-loop verification for low-confidence extractions (AIR-002) prevents clinical errors
- **Cost Containment**: Token budgets (AIR-O01), caching (AIR-O04), and circuit breakers (AIR-O02) enforce free-tier constraints (FR-035); RAG retrieves focused context vs full document prompts

## Architecture and Design Decisions

- **Decision 1: Layered Architecture with Vertical Slice Hybrid for Frontend**
  - Description: Backend follows traditional 3-layer architecture (API Controllers → Business Services → Data Repositories) for clear separation of concerns and testability (NFR-018); frontend adopts Vertical Slice pattern where each feature (Booking, Intake, ClinicalReview) encapsulates its components, state, and API hooks, reducing coupling and enabling parallel team development (Architecture Goal 4)
  - Justification: Layered backend supports dependency injection for unit testing business logic without database; vertical slice frontend avoids cross-feature imports tangling React components as feature set grows
  - Trade-offs: Vertical slice may duplicate utility code across slices (e.g., date formatting), but duplication is acceptable until ≥3 slices use it

- **Decision 2: Optimistic Concurrency Control for Appointment Booking**
  - Description: Appointment entity includes RowVersion column (DR-002); concurrent booking attempts on same slot trigger conflict via EF Core optimistic concurrency (catch DbUpdateConcurrencyException); failed transaction returns 409 Conflict with "Slot no longer available" message
  - Justification: Pessimistic locking (SELECT FOR UPDATE) holds database locks too long under free-tier constraints with limited connection pools; optimistic approach retries failed bookings client-side, acceptable for low-conflict scenarios
  - Trade-offs: Increased client retry logic; potential user frustration if multiple conflicts occur (mitigated by real-time slot availability updates via SignalR, future enhancement)

- **Decision 3: PostgreSQL with pgvector Extension for AI Vector Search**
  - Description: Single PostgreSQL database stores relational data (patients, appointments) and vector embeddings (ClinicalDocument.embeddingVector) using pgvector extension; avoids separate vector database (e.g., Pinecone) to meet free-tier constraint (FR-035)
  - Justification: pgvector provides adequate performance for 10K patient records (NFR-014) with cosine similarity index; eliminates cross-database sync complexity and additional service costs
  - Trade-offs: Vector search performance degrades >100K records (mitigate via partitioning by year if needed); pgvector lacks advanced features like hybrid search reranking (implement application-level per AIR-R03)

- **Decision 4: Asynchronous Background Jobs for AI Processing and Email/SMS**
  - Description: Hangfire library (open-source, free) manages background jobs for PDF data extraction (AIR-002), embedding generation (AIR-R04), reminder sending (FR-013, FR-014), and PDF confirmation generation (FR-007); jobs queued on event triggers (e.g., document upload), processed by worker threads within API process (no separate worker service to minimize free-tier resource usage)
  - Justification: Non-blocking user workflows (user doesn't wait 30 seconds for extraction); retry logic with exponential backoff (NFR-016); job persistence survives application restarts
  - Trade-offs: Jobs compete for API CPU/memory; under high load, may impact request latency (mitigate via job priority tiers and throttle concurrent jobs)

- **Decision 5: JWT Bearer Tokens with Role Claims for RBAC**
  - Description: Authentication via JWT tokens issued on login; tokens include role claim (Patient/Staff/Admin) plus user-specific claims (PatientID, GrantedPermissions); API middleware validates token signature, expiration, and role-based authorization attributes on controllers/methods
  - Justification: Stateless authentication enables horizontal scaling (future); RBAC enforcement at API layer (NFR-009) prevents unauthorized data access; token expiration enforces session timeout (NFR-010)
  - Trade-offs: Token refresh complexity for long-lived sessions (15-min timeout simplifies); token revocation requires blacklist (deferred to Phase 2)

- **Decision 6: Client-Side Data Sovereignty for Patient Portal**
  - Description: Patient portal React app stores no patient data locally (no IndexedDB, localStorage for PHI); all data fetched from API per request; session token stored in httpOnly cookie (not localStorage)
  - Justification: HIPAA data protection (NFR-007); prevent data leakage if patient uses shared device; simplifies logout (delete cookie vs clearing storage)
  - Trade-offs: Increased API calls and latency; no offline mode (acceptable per NFR-005 assumption of internet connectivity)

- **Decision 7: Incremental Static Regeneration (ISR) for Frontend Deployment on Vercel Free Tier**
  - Description: Next.js with ISR deploys to Vercel free tier; static pages (marketing, login) pre-rendered; dynamic pages (patient dashboard, clinical review) server-side rendered (SSR) on-demand with caching
  - Justification: Free tier provides 100GB bandwidth and ~20K requests/month; ISR + SSR balance performance and dynamic content needs; static files served via Vercel CDN
  - Trade-offs: SSR adds latency vs pure SPA (acceptable for <3s SLA per NFR-001); Vercel vendor lock-in (mitigated by Docker fallback to Render/Railway if free tier exceeded)

- **Decision 8: Single Shared Secret for Symmetric Encryption of Blob Storage Paths**
  - Description: Encrypted PDF file paths (DR-003) use AES-256 symmetric encryption with single master key stored in environment variable (not rotated in Phase 1); decryption performed server-side before blob retrieval
  - Justification: Meets HIPAA encryption at-rest requirement (NFR-007); simpler than asymmetric key management under free-tier constraints
  - Trade-offs: Key rotation complexity deferred; key compromise exposes all file paths (mitigated by restricted access to production environment variables)

- **Decision 9: AI Model Provider Selection TBD Based on Free Trial Duration**
  - Description: Architecture supports pluggable AI providers via abstraction layer (IAIModelProvider interface); pilot phase tests OpenAI GPT-4o-mini (cost-effective) vs open-source Llama 3.2 via Ollama self-hosted on free-tier compute
  - Justification: Free-tier constraint (FR-035) demands cost evaluation; abstraction enables provider swap without business logic refactor (Architecture Goal 4); self-hosted option maintains PHI on-premise (HIPAA compliance)
  - Trade-offs: Self-hosted models require more DevOps overhead and may exceed free-tier CPU/memory; commercial APIs incur ongoing costs post-trial (requires budget approval)

- **Decision 10: No Real-Time Features (WebSockets) in Phase 1**
  - Description: Slot availability updates (FR-001) use polling (every 30 seconds when booking page active) vs WebSockets/SignalR; no live waitlist notifications
  - Justification: Simplifies infrastructure (fewer concurrent connections on free tier); acceptable latency tradeoff for non-critical real-time needs
  - Trade-offs: 30-second delay in slot availability may cause occasional double-booking conflicts (handled by optimistic concurrency per Decision 2); future enhancement to SignalR if needed

## Technology Stack

| Layer | Technology | Version | Justification (NFR/DR/AIR) |
|-------|------------|---------|----------------------------|
| **Frontend** | React | 18.3+ | NFR-020 (accessibility with React ARIA), NFR-021 (responsive design with CSS-in-JS), Architecture Goal 4 (component testability with Jest/RTL) |
| Frontend Framework | Next.js | 14+ | TR-007 (ISR for static pages on Vercel free tier), NFR-002 (SSR caching for uptime), Decision 7 (deploy target) |
| UI Components | Tailwind CSS + shadcn/ui | 3.4+ / latest | NFR-021 (responsive utilities), NFR-020 (accessible components), rapid prototyping for free-tier time constraints |
| State Management | React Context + TanStack Query | - / 5+ | NFR-001 (query caching reduces API latency), Architecture Goal 4 (query testability), vertical slice isolation |
| **Backend** | .NET | 8.0 | TR-002 (minimal APIs for performance), NFR-001 (async/await for I/O concurrency), NFR-018 (xUnit testing), Architecture Goal 4 (DI container) |
| API Framework | ASP.NET Core Web API | 8.0 | NFR-006 (built-in HTTPS middleware), NFR-009 (JWT authentication middleware), NFR-017 (model validation attributes) |
| ORM | Entity Framework Core | 8.0 | DR-012 (migration tooling), DR-002 (optimistic concurrency), NFR-019 (code-first schema versioning) |
| Background Jobs | Hangfire | 1.8+ | Decision 4 (async AI processing), NFR-016 (retry with backoff), DR-010 (scheduled backups) |
| **Database** | PostgreSQL | 14+ | DR-003 (pgvector extension for embeddings), NFR-014 (GIN indexes for search), DR-010 (PITR backup capability), free tier available (Supabase) |
| Vector Search | pgvector | 0.5+ | AIR-R02 (cosine similarity queries), AIR-R05 (hybrid search with tsvector), Decision 3 (avoid separate vector DB cost) |
| Caching | Redis | 7+ (Upstash free tier) | NFR-001 (session caching), AIR-O04 (AI inference caching), Decision 4 (Hangfire persistence) |
| **AI/ML** | Model Provider | OpenAI GPT-4o-mini / Llama 3.2 (TBD) | AIR-001 (conversational intake), AIR-002 (extraction), AIR-Q03 (latency), Decision 9 (cost vs self-hosted trade-off) |
| Vector Store | pgvector (integrated) | 0.5+ | AIR-R01 (document chunking storage), AIR-R02 (retrieval), AIR-S02 (ACL filtering), Decision 3 (single database) |
| Embedding Model | text-embedding-3-small / sentence-transformers (TBD) | - | AIR-R04 (embedding generation), cost optimization (1536 dimensions), Decision 9 (API vs self-hosted) |
| AI Gateway | Custom ASP.NET Core Middleware | - | AIR-O01 (token budget enforcement), AIR-O02 (circuit breaker), AIR-S03 (prompt/response logging), Decision 9 (abstraction layer) |
| Guardrails | JSON Schema Validator (Newtonsoft) | 13+ | AIR-Q04 (output schema validation), AIR-S04 (injection prevention), runtime validation |
| **Testing** | Backend Unit/Integration | xUnit + FluentAssertions + Testcontainers | 2.6+ | NFR-018 (≥80% coverage), TR-008, Architecture Goal 4 (DI testability), database integration tests |
| Frontend Unit/Component | Jest + React Testing Library | 29+ / 14+ | NFR-018 (component coverage), NFR-020 (accessibility testing with jest-axe), TR-009 |
| E2E Testing | Playwright | 1.40+ | NFR-021 (responsive viewport testing), AI workflow validation (conversational intake, extraction), user journey smoke tests |
| Load Testing | k6 (open-source) | 0.48+ | NFR-003 (500 concurrent user validation), NFR-001 (p95 latency under load), pre-release gate |
| **Infrastructure** | Frontend Hosting | Vercel Free Tier | NFR-002 (CDN uptime), NFR-035 (zero cost), TR-007 (Next.js native deploy), Decision 7 (ISR support) |
| Backend Hosting | Render Free Tier / Railway Free Tier (TBD) | - | NFR-002 (auto-restart on failure), NFR-035 (zero cost), NFR-012 (health check integration), TR-005 (containerization) |
| Database Hosting | Supabase Free Tier | - | NFR-035 (zero cost), DR-010 (7-day PITR backup), NFR-006 (SSL enforced), NFR-022 (BAA available for HIPAA) |
| Blob Storage | Supabase Storage / Cloudflare R2 Free Tier (TBD) | - | DR-003 (encrypted PDF storage), DR-011 (geo-redundancy), NFR-007 (server-side encryption), NFR-035 (10GB free) |
| **Security** | Authentication | ASP.NET Core Identity + JWT | 8.0 | NFR-009 (RBAC role claims), NFR-010 (token expiration), Decision 5 (stateless auth), NFR-008 (user actions logged) |
| Secrets Management | Environment Variables (Render/Vercel) | - | NFR-007 (encryption key storage), NFR-035 (free tier env vars), Decision 8 (master key access), TR-002 (no hardcoded secrets) |
| TLS/SSL | Let's Encrypt (auto-provisioned) | - | NFR-006 (TLS 1.2+ enforcement), NFR-022 (HIPAA in-transit encryption), free certificates via hosting provider |
| SAST (Static Analysis) | SonarCloud Free Tier | - | NFR-023 (OWASP Top 10 detection), technical debt tracking, PR quality gate (≥B rating), NFR-018 (code quality) |
| Dependency Scanning | npm audit + dotnet list package --vulnerable | - | NFR-023 (known vulnerability prevention), CI pipeline integration, PR blocking on high/critical CVEs |
| **Deployment** | Containerization | Docker | 24+ | TR-005 (portable deployments), consistency across local/staging/prod, Render requires Dockerfile |
| CI/CD | GitHub Actions | - | NFR-010 (automated test runs), NFR-019 (migration testing), NFR-035 (2000 free minutes/month), TR-008/TR-009 triggers |
| **Monitoring** | Application Monitoring | Application Insights Free Tier (Azure) | - | NFR-001 (latency tracking), NFR-012 (health check logs), AIR-Q03 (AI latency), NFR-002 (uptime alerts), NFR-035 (5GB free ingestion) |
| Uptime Monitoring | UptimeRobot Free Tier | - | NFR-002 (external uptime validation), 50 monitors free, 5-minute check interval, email alerts |
| Error Tracking | Sentry Free Tier | - | NFR-016 (error rate monitoring), stack traces for debugging, 5K events/month free, production error alerts |
| **Documentation** | API Docs | Swagger/OpenAPI (Swashbuckle) | 6.5+ | TR-004 (API contract documentation), developer onboarding, auto-generated from controllers |
| Architecture Diagrams | PlantUML / Mermaid | - | Versioned with code, rendered in Markdown, C4 model for system context/container/component views |

### Alternative Technology Options

**Considered but Not Selected:**

1. **Frontend: Vue.js 3 vs React 18**
   - Rejected: React chosen for larger ecosystem (shadcn/ui components), stronger community support for healthcare apps, team familiarity assumption
   - Trade-off: Vue's simpler learning curve not critical; React's composition patterns better for vertical slice architecture

2. **Backend: Node.js (Fastify/NestJS) vs .NET 8**
   - Rejected: Node considered for unified TypeScript stack, but .NET chosen per spec.md constraint (TR-002); .NET superior performance for CPU-bound AI orchestration, stronger type safety, mature EF Core ORM
   - Trade-off: Node would reduce context switching for full-stack developers; .NET better for healthcare domain complexity

3. **Database: MongoDB Atlas Free Tier vs PostgreSQL**
   - Rejected: MongoDB's flexible schema appealing for clinical document variety, but PostgreSQL chosen for pgvector requirement (AIR-R02) and ACID guarantees for appointment booking (DR-002 optimistic concurrency)
   - Trade-off: MongoDB better for rapidly evolving schemas; PostgreSQL relational integrity critical for healthcare

4. **AI Provider: Self-Hosted Llama 3.2 (Ollama) vs OpenAI GPT-4o-mini**
   - TBD: Pilot phase will benchmark both options
   - Self-Hosted Pros: No per-token cost, PHI stays on-premise (HIPAA), vendor independence
   - Self-Hosted Cons: Requires 8GB+ RAM (exceeds free-tier limits), slower inference (~10s latency), manual model updates
   - OpenAI Pros: Superior accuracy (95%+ vs 85% for open-source NER), <2s latency, managed service
   - OpenAI Cons: Pay-per-token cost (~$0.10/1K tokens), PHI exposure requires BAA, vendor lock-in
   - Decision factors: Budget approval for AI costs, acceptable latency (AIR-Q03), extraction accuracy (AIR-Q01)

5. **Hosting: Render vs Railway vs Fly.io Free Tiers**
   - All offer 500MB RAM, 0.1 CPU, 750 hours/month
   - Render selected tentatively for better documentation, health check integration, easier Dockerfile deploys
   - Railway considered for built-in Redis (Upstash alternative), but Render + external Upstash preferred for separation of concerns

6. **Vector Database: Pinecone vs Weaviate vs pgvector**
   - Rejected Pinecone: 1M vectors free but requires separate service, cross-DB sync complexity, overkill for 10K records (NFR-014)
   - Rejected Weaviate: Self-hosted would exceed free-tier compute, managed version not free
   - Selected pgvector: Integrated with PostgreSQL (Decision 3), adequate performance for scale, simpler architecture

### AI Component Stack

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Model Provider** | OpenAI GPT-4o-mini (pilot) / Llama 3.2 via Ollama (fallback) | LLM inference for conversational intake (AIR-001), clinical entity extraction (AIR-002), medical code suggestion (AIR-004, AIR-005) |
| **Vector Store** | pgvector (PostgreSQL extension) | Embedding storage (AIR-R04) and similarity search retrieval (AIR-R02) for RAG pipeline; cosine similarity index for top-K queries |
| **Embedding Model** | text-embedding-3-small (OpenAI) / sentence-transformers (self-hosted fallback) | Generate 1536-dimension vectors from clinical document chunks (AIR-R01) for semantic search; cost-effective at ~$0.02/1M tokens |
| **AI Gateway** | Custom ASP.NET Core Middleware | Request routing to provider abstraction (Decision 9), token budget enforcement (AIR-O01), circuit breaker (AIR-O02), prompt/response logging (AIR-S03), caching layer (AIR-O04) |
| **Guardrails** | Newtonsoft.Json JSON Schema Validator + Regex PII Detector | Output schema validation (AIR-Q04), PII redaction (AIR-S01), injection attack prevention (AIR-S04), confidence threshold routing (AIR-007) |
| **Chunking** | LangChain TextSplitter (ported to C#) or custom algorithm | Segment clinical documents into 512-token chunks with 20% overlap (AIR-R01), preserve semantic boundaries (paragraph/sentence splits) |
| **Re-Ranking** | Cross-Encoder (sentence-transformers) self-hosted or Cohere Re-rank API | Semantic relevance scoring for retrieved chunks (AIR-R03), prioritize clinically relevant passages beyond cosine similarity |
| **Knowledge Base** | Static reference data (ICD-10/CPT code definitions) in PostgreSQL + pgvector embeddings | RAG retrieval for medical code suggestion (AIR-004, AIR-005), enable semantic search over code descriptions and examples |

### Technology Decision Matrix

| Metric (from NFR/DR/AIR) | React + Next.js (Selected) | Vue + Nuxt | Rationale |
|--------------------------|----------------------------|------------|-----------|
| NFR-020 (Accessibility) | Score: 9 (React ARIA library) | Score: 8 (Vue a11y plugins) | React ecosystem stronger; shadcn/ui components built-in WCAG 2.1 AA |
| NFR-018 (Testability) | Score: 10 (Jest + RTL standard) | Score: 9 (Vitest + Vue Testing Library) | React testing maturity; vertical slice pattern better documented in React |
| NFR-021 (Responsive) | Score: 9 (Tailwind + Next.js SSR) | Score: 9 (Tailwind + Nuxt SSR) | Tie; both excellent |
| Architecture Goal 4 | Score: 10 (vertical slice + hooks) | Score: 8 (composition API) | React hooks enable better feature isolation in vertical slices |
| Community/Ecosystem | Score: 10 (largest healthcare OSS) | Score: 7 (smaller healthcare presence) | React dominates enterprise healthcare (Epic, Cerner integrations) |
| **Total Weighted Score** | **48/50** | **41/50** | React wins on testability, ecosystem, and team familiarity assumption |

| Metric (from NFR/DR/AIR) | PostgreSQL + pgvector (Selected) | MongoDB Atlas | Rationale |
|--------------------------|----------------------------------|---------------|-----------|
| DR-002 (Optimistic Concurrency) | Score: 10 (RowVersion native) | Score: 6 (manual version field) | PostgreSQL EF Core optimistic concurrency built-in; MongoDB requires custom logic |
| DR-003 (Vector Storage) | Score: 10 (pgvector extension) | Score: 3 (no native vectors) | pgvector cosine similarity index (AIR-R02); MongoDB requires external vector DB |
| NFR-014 (Query Performance) | Score: 9 (B-tree + GiST indexes) | Score: 8 (compound indexes) | PostgreSQL EXPLAIN plans; MongoDB better for deeply nested JSON but not needed here |
| DR-006 (Referential Integrity) | Score: 10 (foreign keys enforced) | Score: 5 (application-level) | PostgreSQL ACID guarantees critical for appointment booking integrity |
| NFR-035 (Free Tier) | Score: 10 (Supabase 500MB free) | Score: 9 (Atlas 512MB free) | Tie; both adequate for 10K records |
| **Total Weighted Score** | **49/50** | **31/50** | PostgreSQL wins decisively on pgvector requirement and data integrity |

| Metric (from NFR/DR/AIR) | OpenAI GPT-4o-mini | Self-Hosted Llama 3.2 | Rationale |
|--------------------------|--------------------|-----------------------|-----------|
| AIR-Q01 (Extraction Accuracy) | Score: 10 (>95% F1 observed) | Score: 7 (~85% F1 for NER) | OpenAI superior for clinical entity extraction; domain-specific training |
| AIR-Q03 (Latency) | Score: 10 (p95 <2s) | Score: 5 (p95 ~8-10s on 8GB RAM) | OpenAI managed infrastructure; self-hosted CPU-bound on free tier |
| NFR-035 (Cost Constraint) | Score: 3 (pay-per-token, ~$50-200/mo est.) | Score: 10 (free compute if fits in tier) | Self-hosted wins on cost IF free-tier compute sufficient (unclear) |
| AIR-S01 (PHI Privacy) | Score: 7 (requires BAA, external) | Score: 10 (on-premise, no external) | Self-hosted keeps PHI internal; OpenAI requires trust in BAA |
| AIR-O03 (Model Rollback) | Score: 9 (API version pinning easy) | Score: 6 (manual model file swap) | OpenAI simpler version management |
| **Total Weighted Score** | **39/50 (if budget approved)** | **38/50 (if fits free tier)** | **TBD based on pilot**; OpenAI preferred for quality IF budget available |

## Technical Requirements

- **TR-001: System MUST use React 18+ with TypeScript for frontend UI development**
  - Justification: NFR-020 (accessibility via React ARIA), NFR-021 (responsive components), NFR-018 (Jest + RTL testability), Architecture Goal 4 (vertical slice isolation with hooks)
  - Scope: All patient, staff, and admin web interfaces
  - Acceptance Criteria: package.json specifies react@^18.3.0; TypeScript strict mode enabled; no PropTypes (use TypeScript interfaces)

- **TR-002: System MUST use .NET 8 ASP.NET Core Web API for backend services**
  - Justification: NFR-001 (async/await I/O performance), NFR-018 (DI container for testability), NFR-010 (xUnit + FluentAssertions), TR-008 (mature testing ecosystem), spec.md technology constraint
  - Scope: All API endpoints, business logic, data access layer
  - Acceptance Criteria: Target framework net8.0; minimal API pattern for route handlers; dependency injection for all services

- **TR-003: System MUST use PostgreSQL 14+ with pgvector extension for primary database**
  - Justification: DR-003 (vector embeddings for AI search), DR-004 (AIR-R02 similarity queries), DR-006 (referential integrity), NFR-014 (GIN indexes for text search), Decision 3 (avoid separate vector DB)
  - Scope: All persistent data (patients, appointments, clinical documents, audit logs, embeddings)
  - Acceptance Criteria: PostgreSQL version ≥14; pgvector extension enabled; cosine similarity operator <=> available; connection pooling configured

- **TR-004: System MUST implement RESTful API design with OpenAPI 3.0 specification**
  - Justification: TR-001 (client-server architecture), NFR-009 (resource-based authorization), standards-based API for future integrations
  - Scope: All HTTP endpoints
  - Acceptance Criteria: Swagger UI available at /swagger; Conventional HTTP verbs (GET/POST/PUT/DELETE); Resource URIs (e.g., /api/patients/{id}/appointments); 400/401/403/404/500 status codes; Swashbuckle generates OpenAPI from controllers

- **TR-005: System MUST use Docker for containerized deployment**
  - Justification: NFR-002 (consistent deployment across environments), TR-002 (Render/Railway require Dockerfile), Architecture Goal 4 (local dev parity with production)
  - Scope: Backend API, background jobs (Hangfire in same container), database migrations
  - Acceptance Criteria: Dockerfile multi-stage build (build → runtime); docker-compose.yml for local dev with PostgreSQL + Redis; health check endpoint tested in container

- **TR-006: System MUST integrate with Google Calendar API and Microsoft Graph API for calendar synchronization**
  - Justification: FR-005 (Google Calendar sync), FR-006 (Outlook Calendar sync), NFR-016 (retry logic for external API failures)
  - Scope: Appointment booking, rescheduling, cancellation events
  - Acceptance Criteria: OAuth 2.0 authorization flow for patient consent; calendar event CRUD operations; retry with exponential backoff on 5xx errors; circuit breaker after 5 consecutive failures; fallback to .ics file attachment

- **TR-007: System MUST integrate with SMTP email service and SMS gateway for notifications**
  - Justification: FR-008 (email confirmations), FR-013 (SMS reminders), FR-014 (email reminders), NFR-016 (retry transient failures)
  - Scope: Appointment confirmations, reminders, swap notifications, account activation
  - Acceptance Criteria: SMTP via SendGrid/Mailgun free tier (100 emails/day); SMS via Twilio free trial; async job queue (Hangfire); 3 retry attempts with backoff; failed deliveries logged for manual follow-up

- **TR-008: System MUST use xUnit, FluentAssertions, and Testcontainers for backend testing**
  - Justification: NFR-018 (≥80% test coverage), TR-002 (.NET ecosystem standard), Architecture Goal 4 (DI testability), DR-012 (migration testing)
  - Scope: Unit tests (business logic), integration tests (API endpoints + database)
  - Acceptance Criteria: xUnit test projects for each API layer; FluentAssertions for readable assertions; Testcontainers spin up PostgreSQL for integration tests; coverage report in CI pipeline; <80% blocks PR merge

- **TR-009: System MUST use Jest, React Testing Library, and Playwright for frontend testing**
  - Justification: NFR-018 (component test coverage), NFR-020 (accessibility testing with jest-axe), NFR-021 (responsive viewport E2E), TR-001 (React ecosystem standard)
  - Scope: Unit tests (utility functions), component tests (UI components), E2E tests (user journeys)
  - Acceptance Criteria: Jest + RTL for unit/component tests; Playwright for E2E smoke tests; jest-axe for accessibility violations; visual regression snapshots; coverage report in CI

- **TR-010: System MUST implement API rate limiting per user role**
  - Justification: NFR-003 (prevent resource exhaustion under free tier), NFR-023 (DoS mitigation), NFR-035 (protect limited compute)
  - Scope: All API endpoints
  - Acceptance Criteria: AspNetCoreRateLimit middleware; Patient: 100 req/min, Staff: 200 req/min, Admin: 500 req/min; exceeded limit returns 429 Too Many Requests with Retry-After header

- **TR-011: System MUST implement Content Security Policy (CSP) headers**
  - Justification: NFR-023 (XSS prevention), NFR-006 (secure data transmission), HIPAA security controls
  - Scope: All HTTP responses from API and frontend
  - Acceptance Criteria: CSP header blocks inline scripts (require nonce or hash); only allow resources from same origin + trusted CDNs (fonts, icons); report-uri logs violations

- **TR-012: System MUST use Entity Framework Core migrations for database schema versioning**
  - Justification: DR-012 (zero-downtime migrations), DR-013 (schema version tracking), NFR-019 (repeatable deployments), Architecture Goal 4 (code-first testability)
  - Scope: All database schema changes
  - Acceptance Criteria: EF Core migrations generated via CLI; migration history in __EFMigrationsHistory table; rollback scripts included; migrations applied during deployment with --if-applied check; backward-compatible changes only (add column vs drop)

- **TR-013: System MUST implement structured logging with correlation IDs**
  - Justification: NFR-008 (audit trail), NFR-012 (health check diagnostics), AIR-S03 (AI prompt/response logging), distributed tracing across API/jobs
  - Scope: All API requests, background jobs, database queries, external API calls
  - Acceptance Criteria: Serilog for structured logging; correlation ID in HTTP header (X-Correlation-ID); logs include timestamp, level, message, user ID, IP, request path; Application Insights sink for centralized aggregation

- **TR-014: System MUST implement Circuit Breaker pattern for external API calls**
  - Justification: NFR-016 (graceful degradation), AIR-O02 (AI model failure handling), Architecture Goal 2 (resilience under free-tier constraints)
  - Scope: Calendar sync (TR-006), email/SMS (TR-007), AI model invocations (AIR-001, AIR-002, AIR-004, AIR-005)
  - Acceptance Criteria: Polly library; circuit opens after 5 consecutive failures; remains open for 60s before health check; fallback actions defined (manual sync, manual extraction)

- **TR-015: System MUST implement data sanitization for all user inputs**
  - Justification: NFR-017 (SQL injection, XSS prevention), NFR-023 (OWASP Top 10), AIR-S04 (prevent malicious prompts)
  - Scope: All API payloads, form submissions, query parameters
  - Acceptance Criteria: Fluent Validation library for input validation; HTML encoding for output rendering; parameterized SQL queries (no string concatenation); file upload whitelist (.pdf only); max file size 10MB

- **TR-016: System MUST implement pessimistic caching with Redis for session data**
  - Justification: NFR-001 (reduce database load for repeated session queries), NFR-010 (15-min session timeout), Architecture Goal 2 (free-tier resource optimization)
  - Scope: User sessions (JWT claims, role permissions), slot availability cache (30s TTL)
  - Acceptance Criteria: Upstash Redis free tier; session data cached with 15-min sliding expiration; cache invalidation on logout; slot availability cached with 30s TTL to balance freshness vs DB queries

- **TR-017: System MUST implement health check endpoints for liveness and readiness probes**
  - Justification: NFR-012 (monitoring integration), NFR-002 (auto-restart on failure), TR-005 (orchestrator health checks)
  - Scope: API service, database connectivity, Redis connectivity
  - Acceptance Criteria: /health/live returns 200 if API process responsive; /health/ready returns 200 if database + Redis connectable; orchestrator (Render/Railway) configured to restart on consecutive failures

- **TR-018: System MUST implement audit logging via database triggers and application interceptors**
  - Justification: NFR-008 (HIPAA audit controls), DR-005 (immutable logs), AIR-S03 (AI action logging)
  - Scope: All PHI access (patient records, clinical documents, intake forms)
  - Acceptance Criteria: EF Core SaveChanges interceptor logs CRUD operations; database trigger prevents UPDATE/DELETE on AuditLog table; logs include user ID, action, timestamp, IP, entity type/ID, before/after values (for updates)

## Technical Constraints & Assumptions

**Constraints:**

1. **Zero Infrastructure Cost (FR-035):** All hosting, databases, AI APIs, and third-party services MUST use free tiers exclusively; no paid subscriptions or overage charges permitted in Phase 1. This constraint drives technology selection (Vercel/Render/Supabase/Upstash free tiers) and feature trade-offs (e.g., no real-time WebSockets, limited AI token usage, manual workarounds for free-tier limits).

2. **HIPAA Compliance Mandatory:** All components MUST meet HIPAA Security Rule requirements (45 CFR Part 164, Subpart C); requires Business Associate Agreements (BAAs) with free-tier providers where PHI is stored or processed (Supabase confirmed BAA availability; Vercel/Render TBD). Non-compliant free services cannot be used even if technically superior.

3. **Technology Stack Pre-Defined:** Frontend MUST use React (TypeScript) + Tailwind CSS (TR-001); backend MUST use .NET 8 (TR-002); database MUST use PostgreSQL with pgvector (TR-003). These are non-negotiable per spec.md; alternative technologies (Vue, Node.js, MongoDB) excluded despite potential benefits.

4. **No Provider-Facing Features (Phase 1):** Admin and Staff roles implemented; Provider role deferred. Provider dashboards, clinical note authoring, prescription workflows out of scope. Staff act as provider proxies for uploading post-visit notes (FR-025).

5. **PDF-Only Document Format (Phase 1):** Clinical document uploads restricted to machine-readable PDFs (FR-022); handwritten notes, JPEG/PNG scans, DICOM images, HL7 FHIR bundles out of scope. OCR for scanned documents deferred to Phase 2.

6. **No Patient Self-Check-In:** Patients cannot check themselves in via web portal, mobile app, or QR code (FR-020); only Staff can mark "Arrived" status. This business rule constrains UI design (no check-in button in patient portal) and API authorization (Patient role lacks check-in permission).

7. **15-Minute Session Timeout Non-Negotiable:** HIPAA requirement (NFR-010); all sessions expire after 15 minutes of inactivity regardless of user complaints. No "remember me" or extended session options in Phase 1.

**Assumptions:**

1. **Patient Email Access Universal:** All patients assumed to have email addresses for appointment confirmations (FR-008), reminders (FR-014), swap notifications (UC-010). SMS serves as secondary channel (FR-013) but email is primary. No postal mail or phone call fallback workflows in Phase 1.

2. **Machine-Readable PDFs Only:** Uploaded clinical documents assumed to be machine-readable text PDFs (e.g., lab reports from LabCorp, imaging reports from radiology systems). Handwritten notes scanned as PDFs will fail AI extraction (AIR-002) with no fallback; staff must manually enter data. Quality threshold not enforced at upload (future enhancement: check text extractability).

3. **Internal Insurance Database Pre-Populated:** Soft insurance validation (FR-021) assumes dummy insurance database already seeded with common payers (e.g., Blue Cross, Aetna, Medicare). No external real-time insurance eligibility API (costly, complex) in Phase 1. Staff manually verifies mismatches post-booking.

4. **Free Calendar API Availability:** Google Calendar API and Microsoft Graph API assumed to remain free for personal use cases (<10K requests/day). If free tier discontinued or rate-limited, fallback to .ics file attachments via email (FR-008). No calendar sync status monitoring in UI (future enhancement).

5. **AI Model Free Trials Sufficient for Pilot:** OpenAI free trial credits (~$5-18 depending on promotion) assumed adequate for pilot phase (estimated 100-200 patient intakes and document extractions). Post-pilot budget approval required or fallback to self-hosted Llama 3.2 (Decision 9 trade-off). AI features may be disabled if budget unavailable (not a blocker for deterministic features).

6. **Internet Connectivity Always Available:** No offline mode; patients and staff assumed to have reliable internet access (NFR-005 assumption per spec.md). Offline patient intake forms (e.g., kiosk mode) deferred to Phase 2. Staff must have fallback network (mobile hotspot) if clinic internet down.

7. **Single-Location Clinic (Phase 1):** Architecture assumes single clinic location (no multi-tenancy, geo-distributed slots, timezone complexity). Provider and appointment slot entities lack LocationID foreign key. Multi-location expansion in Phase 2 requires migration (DR-012 zero-downtime requirement applies).

8. **Staff-to-Patient Ratio Adequate:** Clinical data prep workflow (UC-008) assumes sufficient staff to review AI extractions flagged for verification (ConfidenceScore <0.7 per FR-022). If 50% of extractions flagged and 100 documents/day uploaded, staff spend 2 min/document × 50 = 100 min/day on review. No automated escalation if staff review queue exceeds capacity (future ML feedback loop).

9. **English Language Only (Phase 1):** All UI text, clinical documents, and AI models assume English language. No internationalization (i18n) or multilingual support. Spanish patient intake (common in U.S. healthcare) deferred to Phase 2; may require separate embedding model or multilingual LLM.

10. **7-Year Audit Log Retention Sufficient:** Assumes 7-year retention (DR-008) meets legal requirements for jurisdiction. Some U.S. states require pediatric records retained until age 21+7 years. Legal counsel must confirm; if longer retention needed, modify DR-008 and scheduled archival job (out of scope for architecture).

## Development Workflow

1. **Environment Setup**
   - Clone repository from GitHub; install .NET 8 SDK, Node.js 20+, Docker Desktop
   - Run `docker-compose up -d` to start local PostgreSQL (with pgvector) and Redis containers
   - Copy `.env.example` to `.env.local`; configure database connection string, JWT secret, API keys placeholders (no real keys in repo)
   - Run `dotnet restore` (backend) and `npm install` (frontend) to install dependencies
   - Run EF Core migrations: `dotnet ef database update` to provision local database schema

2. **Feature Development (Vertical Slice Approach)**
   - Create feature branch from `main` (naming: `feature/booking-slot-swap` or `bugfix/session-timeout`)
   - Backend: Create new controller (e.g., `AppointmentsController`), service (e.g., `BookingService`), and repository (e.g., `AppointmentRepository`) with dependency injection
   - Frontend: Create feature folder `src/features/Booking/` containing components, hooks, API client, types; isolate state management within feature (TanStack Query for server state)
   - Write tests: xUnit unit tests for business logic, Testcontainers integration tests for API + database, Jest + RTL for React components
   - Run tests locally: `dotnet test` (backend), `npm test` (frontend); ensure ≥80% coverage or PR blocked

3. **Database Migration Workflow**
   - Backend code-first: Modify EF Core entities (e.g., add `Appointment.PreferredSlotSwapRequestID?` foreign key)
   - Generate migration: `dotnet ef migrations add AddPreferredSlotSwapRequest`; review generated SQL in `Migrations/` folder
   - Test migration: `dotnet ef database update` on local dev database; verify schema changes in pgAdmin/DBeaver
   - Document rollback: Create rollback script manually (EF Core doesn't auto-generate); test rollback on separate database instance
   - Commit migration files to Git; CI pipeline applies migrations to staging database during deployment

4. **AI Model Integration Workflow** (AIR-001, AIR-002, AIR-004, AIR-005)
   - Implement `IAIModelProvider` interface abstraction (Decision 9); methods: `GenerateConversationalResponse()`, `ExtractEntitiesFromDocument()`, `SuggestMedicalCodes()`
   - Create provider implementations: `OpenAIModelProvider` (uses OpenAI SDK) and `OllamaModelProvider` (HTTP client to local Ollama server)
   - Configure active provider via environment variable `AI_MODEL_PROVIDER=OpenAI` or `AI_MODEL_PROVIDER=Ollama`; dependency injection resolves provider at runtime
   - Add guardrails middleware: `TokenBudgetMiddleware` (AIR-O01), `CircuitBreakerMiddleware` (AIR-O02), `PromptLoggingMiddleware` (AIR-S03)
   - Test with mock provider: `MockAIModelProvider` returns fixed responses in unit tests; integration tests use real provider (check for API key presence, skip if missing)

5. **Continuous Integration (GitHub Actions)**
   - Push feature branch to GitHub; PR triggers CI workflow (`.github/workflows/ci.yml`)
   - CI steps: Lint (ESLint, dotnet format), Build (dotnet build, npm build), Test (dotnet test, npm test with coverage report), Security Scan (npm audit, dotnet list package --vulnerable, SAST via SonarCloud)
   - Quality gate: ≥80% test coverage, zero high/critical vulnerabilities, SonarCloud Quality Gate ≥B rating; failing gate blocks PR merge
   - Preview deployment: Vercel automatically deploys frontend PR preview; Render creates temporary backend preview URL (if configured)

6. **Code Review & Merge**
   - Team reviews PR for: business logic correctness, test coverage adequacy, security best practices (OWASP checklist), HIPAA compliance (audit logging, encryption), adherence to architecture decisions (layered backend, vertical slice frontend)
   - Approval required from 1+ reviewers; merge to `main` triggers production deployment workflow
   - Delete feature branch post-merge; tags release with semantic version (e.g., `v0.2.0-alpha`)

7. **Deployment to Production (Free-Tier Platforms)**
   - Merge to `main` triggers `.github/workflows/deploy.yml`
   - Backend deployment: GitHub Actions builds Docker image, pushes to Render; Render deploys container to free-tier instance, runs EF Core migrations (`dotnet ef database update --if-applied`), restarts API service
   - Frontend deployment: GitHub Actions triggers Vercel deployment; Vercel builds Next.js app with ISR, deploys to CDN, purges cache for updated pages
   - Database migrations: Applied during backend deployment; zero-downtime backward-compatible changes only (e.g., add nullable column, not drop column)
   - Post-deployment verification: Automated smoke tests (Playwright E2E) run against production URLs; uptime monitor (UptimeRobot) receives first health check ping

8. **Monitoring & Incident Response**
   - Application Insights telemetry dashboards track: API latency (p50/p95/p99), error rate, uptime, AI inference latency (AIR-Q03), token consumption (AIR-O05)
   - Alerts configured: p95 latency >5s, error rate >5%, uptime <99.9%, AI cost >$10/day
   - Incident workflow: Alert triggers Slack/email notification; on-call engineer reviews Application Insights logs and error tracking (Sentry stack traces); rollback via Render/Vercel UI if code regression; post-mortem document root cause and prevention

9. **AI Model Evaluation & Improvement**
   - Weekly batch evaluation (AIR-Q01): Sample 100 production documents, run extraction, compare to human-labeled ground truth, calculate F1 score
   - Hallucination audit (AIR-Q02): Human auditor flags 10% of extractions; if hallucination rate >5%, trigger prompt engineering iteration or model version rollback (AIR-O03)
   - Feedback loop: Staff corrections (verified extractions) stored in `VerifiedExtractions` table; periodically export as fine-tuning dataset (Phase 2) or RAG knowledge base updates
   - Model version updates: Admin toggles `ActiveModelVersion` in database; API reads flag on startup; A/B testing via feature flag (future enhancement)

10. **Documentation Maintenance**
    - Architecture decisions logged in `docs/adr/` folder (Markdown ADRs following MADR template); new decisions append sequentially (ADR-011, ADR-012, etc.)
    - API documentation auto-generated from Swagger/OpenAPI; published to `/swagger` endpoint in production (read-only, no "Try it out" in prod)
    - UML diagrams (C4 model: system context, container, component) maintained as PlantUML files in `docs/diagrams/`; rendered in README.md via PlantUML server or embedded PNG
    - changelog updated manually on each release: Added/Changed/Deprecated/Removed/Fixed/Security sections per Keep a Changelog format
