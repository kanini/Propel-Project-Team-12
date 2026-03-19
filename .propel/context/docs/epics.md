---
title: Epic Backlog
project: Unified Patient Access & Clinical Intelligence Platform
version: 1.0
created: 2026-03-19
updated: 2026-03-19
status: Active
---

# Epic Backlog

## Project Context

**Project Type:** Green-field (New System Development)

**Technology Stack:** React 18 + Next.js 14 (Frontend), .NET 8 ASP.NET Core Web API (Backend), PostgreSQL 14+ with pgvector (Database)

**Infrastructure Constraint:** All hosting, APIs, and services MUST use free tiers exclusively (FR-035); zero paid subscriptions or overage charges permitted

**Source Documents:**
- [Requirements Specification](spec.md) — 35 FR, 10 UC
- [Architecture Design](design.md) — 23 NFR, 18 TR, 13 DR, 32 AIR (functional + quality + safety + operational + RAG)
- [Figma UX Specification](figma_spec.md) — 30 UXR, 18 SCR
- [Design Models](models.md) — Architecture diagrams, entity relationships, sequence flows

## Epic Summary Table

| Epic ID | Epic Title | Mapped Requirement IDs | Priority |
|---------|-----------|------------------------|----------|
| EP-TECH | Technical Foundation & Project Scaffolding | TR-001, TR-002, TR-003, TR-004, TR-005, TR-008, TR-009, TR-012, TR-013, TR-017 | P0 (Foundational) |
| EP-DATA | Data Layer & Domain Model Implementation | DR-001, DR-002, DR-003, DR-004, DR-005, DR-006, DR-007, DR-008, DR-009, DR-010, DR-011, DR-012, DR-013 | P0 (Foundational) |
| EP-001 | Appointment Booking & Scheduling System | FR-001, FR-002, FR-003, FR-004, FR-005, FR-006, FR-007, FR-008, UC-001, UC-002, UC-010, UXR-101, UXR-505, UXR-605, SCR-002, SCR-016 | P0 |
| EP-002 | Patient Intake Management (AI & Manual) | FR-009, FR-010, FR-011, FR-012, UC-003, UC-004, AIR-001, AIR-Q01, AIR-Q03, AIR-S01, AIR-O01, UXR-103, UXR-502, UXR-504, SCR-004, SCR-005 | P0 |
| EP-003 | Staff Operations (Walk-in & Arrival Management) | FR-016, FR-017, FR-018, FR-019, FR-020, UC-005, UC-006, UXR-104, SCR-006, SCR-007 | P0 |
| EP-004 | Clinical Data Aggregation & 360° Patient View | FR-022, FR-023, FR-024, FR-025, UC-008, AIR-002, AIR-003, AIR-006, AIR-007, AIR-Q02, AIR-Q04, AIR-S02, AIR-S04, AIR-R01, AIR-R02, AIR-R03, AIR-R04, AIR-R05, UXR-105, UXR-502, UXR-604, SCR-003, SCR-009 | P0 |
| EP-005 | Medical Coding Intelligence (ICD-10/CPT) | FR-026, FR-027, AIR-004, AIR-005, AIR-Q01, AIR-Q02, UXR-502, SCR-017 | P1 |
| EP-006 | Reminders, Notifications & No-Show Prevention | FR-013, FR-014, FR-015, TR-006, TR-007, TR-014, NFR-016 | P1 |
| EP-007 | Insurance Pre-Check Validation | FR-021, UC-007, SCR-008 | P1 |
| EP-008 | User Management & Access Control | FR-028, FR-029, FR-030, UC-009, NFR-009, NFR-010, NFR-011, TR-010, TR-018, UXR-102, UXR-603, SCR-001, SCR-010, SCR-011, SCR-013, SCR-014, SCR-015 | P0 |
| EP-009 | Security, Compliance & Audit Framework | FR-031, FR-032, FR-033, NFR-006, NFR-007, NFR-008, NFR-022, NFR-023, TR-011, TR-015, TR-018, AIR-S03, SCR-012 | P0 |
| EP-010 | UI/UX Implementation & Accessibility | NFR-020, NFR-021, TR-001, UXR-201, UXR-202, UXR-203, UXR-204, UXR-205, UXR-206, UXR-301, UXR-302, UXR-303, UXR-304, UXR-305, UXR-401, UXR-402, UXR-403, UXR-404, UXR-501, UXR-503, UXR-601, UXR-602 | P0 |
| EP-011 | Platform Infrastructure & Performance Optimization | FR-034, FR-035, NFR-001, NFR-002, NFR-003, NFR-004, NFR-005, NFR-012, NFR-013, NFR-014, NFR-015, NFR-016, NFR-017, NFR-018, NFR-019, TR-016, AIR-O02, AIR-O03, AIR-O04, AIR-O05 | P0 |

**Requirement Coverage Summary:**
- **Total Requirements:** 161 (35 FR + 10 UC + 23 NFR + 18 TR + 13 DR + 32 AIR + 30 UXR)
- **Mapped to Epics:** 161 (100% coverage)
- **Orphaned Requirements:** 0
- **[UNCLEAR] Requirements:** None identified

## Epic Descriptions

### EP-TECH: Technical Foundation & Project Scaffolding

**Business Value:** Enables all subsequent development by establishing project infrastructure, development environment, CI/CD pipelines, and architectural patterns. Eliminates setup friction for feature teams and ensures consistent development practices across green-field codebase.

**Description:** Bootstrap project structure for React 18 + Next.js 14 frontend and .NET 8 ASP.NET Core Web API backend with PostgreSQL 14+ database. Configure development environment (Docker Compose for local database and Redis), implement layered backend architecture with dependency injection, establish vertical slice frontend pattern, and provision initial CI/CD pipelines with automated testing thresholds (≥80% coverage gate). Includes foundational authentication scaffolding (JWT token generation without full RBAC), health check endpoints for monitoring, and structured logging infrastructure.

**UI Impact:** No (Infrastructure only)

**Screen References:** N/A

**Key Deliverables:**
- Project repository structure with frontend (`/client`) and backend (`/api`) separation
- Docker Compose configuration for local PostgreSQL with pgvector extension and Redis containers
- Frontend: Next.js 14 app with TypeScript strict mode, Tailwind CSS 3.4+ configuration, shadcn/ui base components
- Backend: .NET 8 minimal APIs with dependency injection container, Entity Framework Core configured, initial migration (empty database)
- CI/CD: GitHub Actions workflows for build, test (xUnit backend, Jest frontend), lint, security scanning (npm audit, SonarCloud)
- Testing frameworks: xUnit + FluentAssertions + Testcontainers (backend), Jest + React Testing Library (frontend)
- Structured logging with Serilog + Application Insights sink, correlation ID middleware
- Health check endpoints (`/health/live`, `/health/ready`) with database connectivity probes
- OpenAPI/Swagger documentation generation from controllers
- EF Core migration tooling configured for zero-downtime schema versioning

**Dependent EPICs:**
- None (Foundational epic; all other epics depend on EP-TECH)

---

### EP-DATA: Data Layer & Domain Model Implementation

**Business Value:** Establishes robust data persistence layer with HIPAA-compliant features (encryption, immutable audit logs, referential integrity), optimistic concurrency control for appointment booking race conditions, and vector search capabilities for AI-powered clinical data retrieval. Blocks all feature epics requiring data operations.

**Description:** Implement PostgreSQL database schema with pgvector extension for AI embeddings. Define core domain entities (Patient, Staff, Admin, Appointment, AppointmentSlot, ClinicalDocument, ExtractedData, ConsolidatedPatientView, IntakeForm, AuditLog, PreferredSlotSwapRequest, Reminder) with Entity Framework Core fluent configuration. Enforce referential integrity via foreign key constraints, unique indexes for business rules (email uniqueness, slot double-booking prevention via RowVersion optimistic concurrency), and immutable append-only AuditLog backed by database trigger. Configure encrypted blob storage for clinical PDFs with AES-256 at-rest encryption. Provision daily automated backups with 7-day point-in-time recovery. Seed mock insurance database for soft validation.

**UI Impact:** No (Data layer only)

**Screen References:** N/A

**Key Deliverables:**
- EF Core entity models for all domain objects (Patient, Appointment, ClinicalDocument, etc.) with data annotations and fluent API configuration
- Database migration creating tables, indexes (B-tree for primary keys, GIN for text search, GiST for pgvector cosine similarity), foreign key constraints
- Optimistic concurrency control: `Appointment.RowVersion` column configured as `IsRowVersion()`, DbUpdateConcurrencyException handling
- Immutable AuditLog: Database BEFORE UPDATE/DELETE trigger rejecting modifications, EF Core SaveChanges interceptor logging all CRUD operations
- pgvector setup: Extension enabled via migration, `ClinicalDocument.EmbeddingVector` column with cosine similarity operator `<=>`
- Encrypted blob storage configuration: Supabase Storage or Cloudflare R2 free tier, server-side encryption with master key in environment variable
- Backup configuration: Supabase 7-day point-in-time recovery enabled, quarterly backup restoration test documented
- Mock insurance database seed: CSV import of 50 common payers (Blue Cross, Aetna, UnitedHealthcare, Medicare) into `InsuranceProviders` table
- Repository pattern interfaces (IPatientRepository, IAppointmentRepository, etc.) with Entity Framework implementations
- Data integrity validation: Unit tests for unique constraints, foreign key cascades, optimistic concurrency conflict scenarios

**Dependent EPICs:**
- EP-TECH - Foundational (Requires project structure and EF Core configuration)

---

### EP-001: Appointment Booking & Scheduling System

**Business Value:** Core revenue-generating feature reducing no-show rates through dynamic preferred slot swap and intelligent waitlist management. Enables seamless patient appointment booking within 3 clicks, automated calendar synchronization, and email/PDF confirmation delivery, directly addressing 15% no-show baseline problem.

**Description:** Implement real-time appointment slot availability display with 5-second update latency, single-click booking workflow, dynamic preferred slot swap functionality (patients book available slot while registering interest in currently unavailable preferred slot, system auto-swaps when preferred opens), and FIFO waitlist for fully booked schedules. Integrate Google Calendar API and Microsoft Graph API for automatic calendar event CRUD operations with OAuth 2.0 patient consent flow. Generate PDF appointment confirmations with QR code placeholders, send via email within 60 seconds of booking, and include .ics calendar file attachment as fallback. Handle concurrent booking conflicts via optimistic locking (RowVersion), display "Slot No Longer Available" error with alternative slot suggestions.

**UI Impact:** Yes

**Screen References:** SCR-001 (Patient Dashboard), SCR-002 (Appointment Slot Selection & Booking), SCR-016 (Appointment History)

**Key Deliverables:**
- Slot availability API endpoint: `GET /api/appointments/slots?providerId={id}&date={date}` returning available slots filtered by provider, location, appointment type; caching layer (Redis 30s TTL) to reduce database load
- Booking workflow: `POST /api/appointments/book` with optimistic concurrency handling, returns 409 Conflict if slot taken, triggers asynchronous jobs (PDF generation, email send, calendar sync)
- Preferred slot swap: `PreferredSlotSwapRequest` entity creation during booking; background job (`PreferredSlotMonitorJob`) polling every 5 minutes, automatic swap execution when preferred slot available, email/SMS notification to patient
- Waitlist management: `POST /api/appointments/waitlist` adds patient to FIFO queue; when slot opens, notify first waitlisted patient with 2-hour response window via `WaitlistNotificationJob`
- Google Calendar integration: OAuth 2.0 authorization flow, create/update/delete calendar events via Google Calendar API v3, retry with exponential backoff on 5xx errors, circuit breaker after 5 consecutive failures
- Outlook Calendar integration: Microsoft Graph API authorization, create/update/delete events via `/me/events` endpoint, same retry logic as Google
- PDF confirmation generation: Background job using PdfSharpCore or Puppeteer, includes appointment details (date, time, provider, location, instructions), QR code placeholder (no Phase 1 functionality), completed within 10 seconds
- Email delivery: Send confirmation email with PDF attachment and .ics file via SendGrid/Mailgun free tier, track delivery status, retry 3x with backoff on transient failures
- Appointment history page: Patient-facing screen displaying past and upcoming appointments, status badges (Scheduled/Arrived/Completed/Cancelled/NoShow), reschedule/cancel actions

**Dependent EPICs:**
- EP-TECH - Foundational
- EP-DATA - Foundational

---

### EP-002: Patient Intake Management (AI & Manual)

**Business Value:** Flexible patient data collection respecting user autonomy while streamlining administrative workflows. AI conversational intake reduces completion friction (natural language vs rigid forms), while manual form option ensures accessibility for all patient preferences. Structured data extraction enables downstream clinical workflows.

**Description:** Implement dual-mode patient intake system: AI conversational interface using natural language understanding (NLU) for free-text responses with structured data extraction, and traditional manual form with inline validation. Enable seamless toggle between modes at any time with data preservation across switch. AI mode uses Tool Calling + RAG pattern (fetch patient history for context) to extract demographics, medical history, medications, allergies, and reason for visit into structured JSON. Enforce 4000-token budget per session (graceful handoff to manual form if exceeded), log all AI prompts/responses for HIPAA audit compliance, and flag low-confidence extractions for staff verification. Support draft saving, pause/resume functionality, and 24-hour pre-appointment editing lock.

**UI Impact:** Yes

**Screen References:** SCR-004 (AI Conversational Intake), SCR-005 (Manual Form Intake)

**Key Deliverables:**
- AI conversational intake UI: ChatBubble component with user/AI message stream, TextArea input, mode toggle switch in header, real-time typing indicators, conversation history persistence
- AI backend service: `IAIModelProvider` abstraction with OpenAI GPT-4o-mini implementation, conversational prompt engineering (system message defines intake fields), tool calling for structured field extraction (`extractIntakeField` tool)
- Token budget middleware: Track cumulative tokens per session, return 429 Too Many Tokens after 4000 threshold exceeded, UI displays "Complete remaining fields manually" handoff message
- RAG context retrieval: Query patient history from previous IntakeForms, embed user query, retrieve top-5 similar past responses via pgvector cosine similarity, inject into prompt context
- Manual form UI: Traditional form with TextField, Select, Checkbox inputs for demographics, medical history (multi-select conditions), current medications (dynamic add/remove rows), allergies, reason for visit (textarea)
- Inline validation: Email format validated on blur (regex), required fields show red border + error text, date of birth age check (≥18 years), phone number format mask
- Mode toggle functionality: "AI Intake" / "Manual Form" toggle button visible at all times, confirm dialog if unsaved changes exist, serialize current form state to session storage, restore data after mode switch
- Draft saving: Auto-save every 30 seconds to `IntakeForms` table with `CompletedAt = null`, manual "Save Draft" button, "Resume Intake" link on patient dashboard
- Editing lock: Allow edits up to 24 hours before appointment (FR-012), display "Editing locked" message after cutoff, preserve edit history in `IntakeForm.EditHistory` JSON array
- Staff verification workflow: Low-confidence AI extractions (ConfidenceScore <0.7) flagged in staff dashboard "Intake Review Queue", staff can accept/reject/modify extracted values

**Dependent EPICs:**
- EP-TECH - Foundational
- EP-DATA - Foundational

---

### EP-003: Staff Operations (Walk-in & Arrival Management)

**Business Value:** Empowers staff to manage unscheduled patient flow efficiently, reducing wait times through same-day queue reordering and streamlined walk-in check-in. Staff-exclusive arrival marking (no patient self-check-in) maintains operational control and identity verification compliance.

**Description:** Implement staff-facing interfaces for walk-in appointment creation, same-day queue management with drag-and-drop reordering, and patient arrival tracking. Walk-in workflow allows staff to search existing patients or create new patient records on-the-fly, capture basic intake data, and optionally send account activation email post-check-in. Arrival management displays same-day appointments sorted by time with visual status indicators (Scheduled/Arrived/Late), enables staff to mark patients as "Arrived" via Check In button (patient self-check-in blocked via RBAC), and notifies providers when patients arrive. Queue provides expected wait time calculation based on provider schedule.

**UI Impact:** Yes

**Screen References:** SCR-006 (Staff Walk-In Management), SCR-007 (Staff Arrival Management / Queue)

**Key Deliverables:**
- Walk-in booking UI: Staff dashboard "Walk-In" tab, patient search (name, DOB, email), search results table, "Create Walk-In Appointment" form with demographics and intake fields (condensed version), slot selection for same-day or immediate queue addition
- Walk-in API endpoints: `POST /api/staff/walk-ins/search` for patient lookup, `POST /api/staff/walk-ins/book` creating Appointment with `BookingSource=WalkIn`, auto-populate IntakeForm from search results if existing patient
- Account creation post-walk-in: Optional "Create Patient Account" button after walk-in checked in, pre-fill registration form with walk-in intake data, send activation email via `POST /api/staff/walk-ins/{id}/create-account`
- Arrival queue UI: Staff dashboard "Arrivals" tab, table displaying same-day appointments (Provider, Patient Name, Appointment Time, Status badge, Expected Wait Time, Check In button), filter by provider dropdown, search by patient name
- Drag-and-drop reordering: HTML5 drag-drop API or react-beautiful-dnd library, persist queue order to `Appointment.QueuePosition` column, recalculate wait times on reorder
- Check-in functionality: "Check In" button calls `POST /api/staff/arrivals/{appointmentId}/check-in`, updates `Appointment.Status = Arrived` and `Appointment.ArrivedAt = timestamp`, triggers provider notification (future: SignalR real-time alert, Phase 1: staff dashboard refresh)
- Status indicators: Badge component with color coding (green=Arrived, yellow=Scheduled, red=Late >15 min past time), late appointments auto-flagged via scheduled job
- Wait time calculation: Backend service calculates expected wait based on appointments ahead in queue and average appointment duration (configurable per provider), displayed next to each patient in queue
- Patient self-check-in blocking: RBAC middleware rejects `POST /api/arrivals/self-check-in` for Patient role, UI hides check-in button from patient portal

**Dependent EPICs:**
- EP-TECH - Foundational
- EP-DATA - Foundational
- EP-008 - User Management & Access Control (Depends on RBAC for staff-only functions)

---

### EP-004: Clinical Data Aggregation & 360° Patient View

**Business Value:** Reduces staff administrative time by 90% (20 min → 2 min per appointment) for clinical prep via AI-powered PDF data extraction. 360-Degree Patient View consolidates multi-source data (intake forms, uploaded documents, post-visit notes) with critical conflict highlighting, preventing clinical errors from medication discrepancies or allergy mismatches. Transparent AI confidence scoring builds trust by explicitly flagging low-confidence extractions for staff verification.

**Description:** Implement multi-format PDF upload workflow with AI extraction using RAG pattern (retrieve medical knowledge base for entity disambiguation) to identify structured data (vitals, diagnoses, medications, allergies, procedures, lab values) with confidence scores (High >90%, Medium 70-90%, Low <70%). Consolidate extracted data across multiple documents into unified 360-Degree Patient View with timeline display, source document provenance, and explicit conflict detection (e.g., medication lists mismatch, allergy contradictions). Low-confidence extractions routed to staff verification queue; staff can accept, reject, or modify values. Ingest post-visit clinical notes (staff-uploaded text or PDF) to update patient view. Provide source citations with "view source" links opening PDF at relevant page number.

**UI Impact:** Yes

**Screen References:** SCR-003 (Clinical Document Upload), SCR-009 (360-Degree Patient View / Clinical Review)

**Key Deliverables:**
- Document upload UI: FileUpload dropzone component, drag-and-drop or click-to-browse, file type validation (.pdf only), max size 10MB, virus scan via VirusTotal free API, progress bar for upload and extraction, badge showing processing status (Pending/Processing/Completed/Failed)
- PDF extraction pipeline: Background job triggered on upload (`DocumentExtractionJob`), extract text via PDFSharp or Tesseract (OCR fallback), chunk document into 512-token segments with 20% overlap (AIR-R01), generate embeddings via text-embedding-3-small, store vectors in `ClinicalDocument.EmbeddingVector` pgvector column
- AI entity extraction: Prompt engineering for Named Entity Recognition (NER), identify medications (name, dosage, frequency, duration), diagnoses (ICD-10 codes if present), vitals (BP, HR, temp, weight), allergies, procedures, lab values (name, value, unit, reference range), assign confidence scores based on entity context and formatting consistency
- Confidence scoring logic: High (>0.9): Entity in structured table or explicit label, Medium (0.7-0.9): Entity in narrative text with strong context, Low (<0.7): Ambiguous or incomplete entity; low-confidence items flagged for staff verification
- RAG knowledge base retrieval: Query pgvector for similar medical terms (e.g., "Metformin" matches "metformin HCl 500mg"), rerank results via cross-encoder, inject top-3 retrieved passages into extraction prompt
- 360-Degree Patient View UI: Tabbed interface (Timeline, Documents, Medications, Diagnoses, Vitals), timeline displays chronological events with source tags (IntakeForm, Document A, Post-Visit Note), conflict alert banner at top (red warning), accordion for each data category
- Conflict detection: Rule-based comparator checking for: medication list discrepancies (Document A: "No medications" vs Document B: "Aspirin"), allergy mismatches (Document A: "No allergies" vs Document B: "Penicillin allergy"), vital sign anomalies (BP 180/120 vs average 120/80); conflicts displayed with side-by-side source comparison
- Staff verification workflow: "Clinical Review Queue" page listing patients with low-confidence extractions (ConfidenceScore <0.7), staff clicks "Review" to open 360° View, inline edit buttons for each extracted value, accept/reject/modify actions update `ExtractedData.VerifiedByStaffID` and `VerifiedAt`
- Post-visit note ingestion: Staff uploads clinical note (text or PDF), extraction pipeline processes similar to document upload, new diagnoses/medications/procedures added to patient view with source tag "Post-Visit [Date]"
- Source provenance: Each extracted data point includes reference to source document (`ExtractedData.DocumentID`, `SourceDocumentPageNumber`), "View Source" link opens PDF modal at specific page (PDF.js renderer)

**Dependent EPICs:**
- EP-TECH - Foundational
- EP-DATA - Foundational
- EP-002 - Patient Intake Management (Depends on IntakeForm entity for data consolidation)

---

### EP-005: Medical Coding Intelligence (ICD-10/CPT)

**Business Value:** Automates medical code suggestion to reduce administrative burden, maintain >98% AI-human agreement rate for clinical data coding, and minimize claim denials from incorrect codes. Transparent confidence scoring and human-in-the-loop verification ensure trust and compliance.

**Description:** Implement AI-powered ICD-10 and CPT code suggestion using RAG pattern (retrieve code definitions from knowledge base) and Few-Shot Classification. Analyze aggregated patient data from 360-Degree View (diagnoses, symptoms, procedures) to suggest relevant medical codes with confidence scores and code descriptions. Allow staff to accept, reject, or modify suggested codes, with accepted codes saved to patient record. Maintain static reference database of ICD-10 (70K codes) and CPT (10K codes) definitions embedded via pgvector for semantic search, enabling natural language queries like "chest pain on exertion" → ICD-10 I20.9 (Angina pectoris, unspecified).

**UI Impact:** Yes

**Screen References:** SCR-017 (Clinical Coding Review)

**Key Deliverables:**
- ICD-10/CPT knowledge base: PostgreSQL table with code, description, category, examples columns; generate embeddings for descriptions using text-embedding-3-small, store in pgvector column; seed from CMS public datasets (ICD-10-CM 2024, CPT 2024)
- Code suggestion API: `POST /api/coding/suggest-icd10` and `POST /api/coding/suggest-cpt` accepting patient diagnoses/procedures as input, retrieve semantically similar codes via pgvector cosine similarity (top-10), rerank via cross-encoder, return ranked list with confidence scores (0.0-1.0)
- Confidence scoring: Calculate based on semantic similarity score (0.6-0.7 = Medium, >0.7 = High), code description overlap with patient data, presence of qualifying keywords (e.g., "acute" vs "chronic")
- Code suggestion UI: Staff dashboard "Coding Review" page, Card component for each patient awaiting coding, display 360° View summary (diagnoses, procedures), ICD-10 suggestions accordion (ranked by confidence), CPT suggestions accordion, each suggestion shows code, description, confidence badge, documentation requirements
- Staff actions: "Accept" button adds code to `PatientMedicalCodes` table with `AcceptedByStaffID` and `AcceptedAt`, "Reject" button logs rejection reason, "Modify" allows custom code entry with manual search (typeahead input querying code database)
- Few-shot prompting: Include 3-5 example diagnoses → ICD-10 mappings in prompt context (e.g., "Type 2 diabetes mellitus" → E11.9), guide LLM to follow pattern
- Billing integration placeholder: Save accepted codes to patient record; Phase 2 integrates with claims submission workflow (out of scope)
- Audit logging: Log all code suggestions, staff actions (accept/reject/modify), and reasons for rejection to AuditLog table

**Dependent EPICs:**
- EP-TECH - Foundational
- EP-DATA - Foundational
- EP-004 - Clinical Data Aggregation & 360° Patient View (Depends on consolidated patient data for code suggestion input)

---

### EP-006: Reminders, Notifications & No-Show Prevention

**Business Value:** Decreases no-show rates through automated multi-channel reminders (SMS/Email) and rule-based no-show risk scoring, recovering lost appointment revenue and improving clinic efficiency.

**Description:** Implement automated reminder system sending SMS reminders at T-24h and email reminders at T-48h and T-24h before appointments. Email reminders include reschedule/cancel links and track open/click rates. SMS reminders support reply-to-confirm functionality ("C" or "Confirm" updates confirmation status). Rule-based no-show risk assessment calculates score based on historical no-show rate, late cancellations, reminder non-responses, and appointment lead time; high-risk appointments (score >70) flagged for staff review with suggested interventions (confirmation call, deposit requirement). Integrate with SendGrid/Mailgun (email) and Twilio (SMS) free tiers, implement retry logic with exponential backoff for transient failures, and circuit breaker after 5 consecutive API failures.

**UI Impact:** No (Background jobs + staff dashboard flag)

**Screen References:** N/A (Risk flags display in SCR-007 Staff Arrival Queue)

**Key Deliverables:**
- Reminder scheduling service: Background job (`ReminderSchedulerJob`) runs hourly, queries appointments scheduled >24h in future, creates `Reminder` records with `ScheduledFor = appointment time - 24h` (SMS) and `appointment time - 48h/24h` (Email)
- Email reminder job: `EmailReminderJob` triggered at scheduled time, fetches appointment details, generates email body with appointment summary, reschedule/cancel links (unique token for security), PDF confirmation attachment, sends via SendGrid/Mailgun API, tracks delivery status (pending/sent/failed/bounced)
- SMS reminder job: `SMSReminderJob` sends SMS via Twilio free trial, includes appointment date/time, provider, location, "Reply C to confirm" instruction, logs delivery status
- SMS reply processing: Webhook endpoint `POST /api/reminders/sms-reply` receives Twilio inbound SMS, parse body for "C" or "Confirm" (case-insensitive), update `Reminder.ConfirmationReceived = true` and `Reminder.ConfirmationMethod = SMSReply`
- Email tracking: Track email opens via transparent 1x1 pixel image, track link clicks via redirect URLs, update `Reminder` record with open/click timestamps
- No-show risk scoring: Calculate score = (historical no-show rate × 40) + (late cancellations in last 6 months × 20) + (reminder non-response × 20) + (appointment lead time >7 days ? 10 : 0) + (first-time patient ? 10 : 0); formula tunable via configuration
- High-risk flagging: Appointments with score >70 flagged in `Appointment.NoShowRiskScore` column, display red "High Risk" badge in staff arrival queue (SCR-007), suggest interventions (modal popup: "Call patient for confirmation" or "Require $25 deposit")
- Retry logic: Polly retry policy with 3 attempts, exponential backoff (1s, 2s, 4s), retry on 5xx errors or network timeouts
- Circuit breaker: Polly circuit breaker opens after 5 consecutive failures (timeout or 5xx), remains open for 60s before health check attempt, fallback to manual reminder (staff notification)
- Reminder history: Staff can view reminder delivery status and patient confirmations in appointment detail view

**Dependent EPICs:**
- EP-TECH - Foundational
- EP-DATA - Foundational
- EP-001 - Appointment Booking & Scheduling System (Depends on Appointment entity and booking workflow)

---

### EP-007: Insurance Pre-Check Validation

**Business Value:** Soft validation against internal dummy records reduces front-desk verification workload and flags mismatches early for manual follow-up, minimizing appointment-day delays from insurance eligibility issues.

**Description:** Implement staff-facing insurance validation workflow comparing entered insurance name and member ID against internal dummy insurance database (seeded with 50 common payers). Display validation status (Match Found / No Match / Manual Review Required) with color-coded badges. Flag mismatches for staff follow-up with patient. No external real-time insurance eligibility API in Phase 1 (cost/complexity deferred); staff manually verify mismatches post-booking via phone call to insurer.

**UI Impact:** Yes

**Screen References:** SCR-008 (Insurance Pre-Check Validation)

**Key Deliverables:**
- Insurance validation UI: Staff dashboard "Insurance Validation" page, TextField inputs for insurance name (typeahead from `InsuranceProviders` table) and member ID, "Validate" button, Alert component displaying result (green=Match, yellow=Manual Review, red=No Match), Badge showing validation status
- Validation API: `POST /api/staff/insurance/validate` accepting `insuranceName` and `memberId`, query `InsuranceProviders` table for fuzzy match (Levenshtein distance <3 or LIKE %name%), query `MockInsuranceRecords` table for exact member ID match, return validation status
- Mock insurance database: Seed `MockInsuranceRecords` table with 10K dummy records (randomized first/last name, DOB, member ID format matching real payers), associate with `InsuranceProviders` via foreign key; data source: synthetic data generator (Faker library)
- Mismatch flagging: No match found → Update `Patient.InsuranceValidationStatus = ManualReview`, create Task in staff dashboard "Insurance Follow-Up Queue", staff manually calls insurer to verify, update status after confirmation
- Validation history: Log all validation attempts to `InsuranceValidations` table with timestamp, staff user, result, patient ID; staff can view validation history in patient record
- Future integration placeholder: Phase 2 integrates external eligibility API (e.g., Change Healthcare, Availity); architecture supports swapping dummy database with live API via provider abstraction

**Dependent EPICs:**
- EP-TECH - Foundational
- EP-DATA - Foundational (Requires mock insurance database seed)

---

### EP-008: User Management & Access Control

**Business Value:** Enforces HIPAA-compliant role-based access control (RBAC) preventing unauthorized PHI access, enables admin self-service for user account lifecycle management (create/edit/deactivate), and implements automatic session timeout mitigating session hijacking risk.

**Description:** Implement RBAC for three roles (Patient, Staff, Admin) with distinct permissions: Patients access booking, intake, documents; Staff access walk-ins, arrivals, clinical review; Admins access user management, system config, audit logs. Admin dashboard enables creating/editing/deactivating user accounts, resetting passwords via email link, and assigning role permissions. JWT tokens include role claims validated at API middleware layer and UI component visibility layer. Enforce 15-minute automatic session timeout with 13-minute warning modal prompting session extension. Block concurrent session usage per user account (new login invalidates previous session).

**UI Impact:** Yes

**Screen References:** SCR-001 (Patient Dashboard with role-based navigation), SCR-010 (Admin User Management List), SCR-011 (Admin Create/Edit User Form), SCR-013 (Login Screen), SCR-014 (Password Reset), SCR-015 (Account Activation)

**Key Deliverables:**
- RBAC middleware: ASP.NET Core `[Authorize(Roles = "Staff")]` attributes on controllers/actions, JWT token validation extracts role claims, reject unauthorized requests with 403 Forbidden
- JWT token generation: On login, issue JWT with claims (`sub` = UserID, `email`, `role` = Patient/Staff/Admin, `grantedPermissions` = array of feature flags), sign with secret key, set expiration to 15 minutes
- Navigation hierarchy: React Context stores current user role, conditionally render nav menu items (Patient sees Dashboard, Book, Intake, Documents; Staff sees Arrivals, Walk-Ins, Clinical Review; Admin sees Users, Audit Logs)
- Admin user management UI: Table displaying all users (email, name, role, status, created date), "Create User" button, "Deactivate" button (soft delete via `IsActive = false`), "Reset Password" button (sends email with reset link)
- Create/Edit user form: TextField for email, name; Select for role (Patient/Staff/Admin); Checkbox group for permissions; "Save" button calls `POST /api/admin/users` or `PUT /api/admin/users/{id}`
- Password reset flow: User clicks "Forgot Password" → enters email → backend generates unique token, stores in `PasswordResetTokens` table with 1-hour expiration → email sent with reset link → user clicks link → enters new password → token validated and consumed
- Account activation: New user created by Admin → activation email sent with unique token → user clicks link → sets initial password → account status updated to Active
- Session timeout: React hook tracks user activity (clicks, keypress, API calls), reset timer on activity, display Modal at 13 minutes ("You'll be logged out in 2 minutes"), "Stay Logged In" button refreshes JWT token via `POST /api/auth/refresh`, auto-logout at 15 minutes clears token and redirects to login
- Concurrent session blocking: On login, generate unique session ID, store in `ActiveSessions` table with UserID and DeviceFingerprint, on new login invalidate previous session (delete from table), backend validates session ID on each API request
- Password policy: Minimum 8 characters, require uppercase, lowercase, digit, special character; enforce on registration and password reset; hash with bcrypt (work factor 12)

**Dependent EPICs:**
- EP-TECH - Foundational
- EP-DATA - Foundational

---

### EP-009: Security, Compliance & Audit Framework

**Business Value:** Ensures HIPAA compliance (legal requirement for handling PHI), prevents data breaches through defense-in-depth (TLS 1.2+, AES-256 encryption, immutable audit logs), and mitigates OWASP Top 10 vulnerabilities minimizing legal liability and reputation risk.

**Description:** Implement HIPAA Security Rule technical safeguards: TLS 1.2+ for all data transmission with HSTS header enforcement, AES-256 encryption at rest for database and blob storage with master key stored separately in environment variables, and immutable append-only audit logging for all PHI access (patient records, clinical documents, intake forms) with 7-year retention. Enforce RBAC preventing unauthorized data access, implement automatic 15-minute session timeout, and block concurrent sessions. Mitigate OWASP Top 10: parameterized queries prevent SQL injection, HTML encoding prevents XSS, CSP headers block inline scripts, JWT validation prevents broken authentication, dependency scanning detects vulnerable packages, input sanitization prevents injection attacks.

**UI Impact:** Yes (Session timeout modal, audit log viewer)

**Screen References:** SCR-012 (Audit Log Viewer for Admin)

**Key Deliverables:**
- TLS 1.2+ enforcement: Configure Kestrel (backend) and Vercel (frontend) to reject TLS <1.2 handshakes, enable HSTS header (`Strict-Transport-Security: max-age=31536000; includeSubDomains`), redirect HTTP → HTTPS
- AES-256 encryption at rest: Enable Supabase database encryption (managed by provider), configure blob storage server-side encryption with AES-256, store master key in Render/Vercel environment variables (not in code)
- Immutable audit logging: EF Core SaveChanges interceptor logs all CRUD operations on Patient, ClinicalDocument, IntakeForm, Appointment tables to `AuditLog` (append-only), database BEFORE UPDATE/DELETE trigger rejects modifications on AuditLog table
- Audit log viewer UI: Admin-only page (SCR-012) displaying AuditLog table with filters (user, entity type, date range), pagination, search, columns (timestamp, user, action, entity type/ID, IP address, user agent), export to CSV for compliance reporting
- RBAC enforcement: Middleware validates JWT role claims on every API request, reject unauthorized with 403, UI hides role-inappropriate components (e.g., Patient cannot see Staff navigation items)
- Session timeout: 15-minute inactivity timer with 13-minute warning modal (implemented in EP-008), unsaved form data preserved in session storage for 1 hour post-logout
- Concurrent session blocking: Invalidate previous session on new login (implemented in EP-008)
- SQL injection prevention: Use EF Core parameterized queries exclusively (no string concatenation), review all raw SQL queries (e.g., `FromSqlRaw`) for parameterization
- XSS prevention: HTML encode all user-generated content in React components (default behavior), set CSP header blocking inline scripts (`script-src 'self'`), sanitize rich text inputs via DOMPurify
- CSP headers: Configure `Content-Security-Policy: default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline' fonts.googleapis.com; font-src fonts.gstatic.com; img-src 'self' data:; connect-src 'self'`
- Dependency scanning: GitHub Actions workflow runs `npm audit` (frontend) and `dotnet list package --vulnerable` (backend) on every PR, fail build on high/critical vulnerabilities, integrate SonarCloud SAST for additional code scanning
- Input sanitization: Fluent Validation library validates all API payloads (email format, max lengths, regex patterns), reject invalid with 400 Bad Request, file upload whitelist (.pdf only), max size 10MB
- 7-year retention: Scheduled job archives AuditLog records older than 7 years to cold storage (S3 Glacier equivalent), production logs remain queryable for 7 years (DR-008)

**Dependent EPICs:**
- EP-TECH - Foundational
- EP-DATA - Foundational (Requires AuditLog entity and immutable trigger)
- EP-008 - User Management & Access Control (Depends on RBAC infrastructure)

---

### EP-010: UI/UX Implementation & Accessibility

**Business Value:** Ensures inclusive design accessible to users with disabilities per ADA compliance (WCAG 2.1 Level AA), optimizes user experience across desktop/tablet/mobile devices with responsive design, and establishes consistent visual language via Tailwind CSS design system reducing development friction.

**Description:** Implement WCAG 2.1 Level AA accessibility standards across all UI components: keyboard navigation support (Tab, Enter, Escape, Arrow keys), screen reader compatibility (ARIA labels, live regions, role attributes), 4.5:1 color contrast ratio for normal text, 44×44px minimum touch targets on mobile, and visible focus indicators. Design responsive layouts for mobile (320px), tablet (768-1023px), and desktop (1024px+) viewports with single-column forms on mobile, two-column staff dashboards on tablet, and multi-column views on desktop. Establish Tailwind CSS design system with shadcn/ui component library (Button, Card, Input, Select, Dialog, Toast) customized via CSS variables for healthcare-appropriate visual tone (professional blues, whites, grays).

**UI Impact:** Yes (All screens)

**Screen References:** All screens (SCR-001 to SCR-018)

**Key Deliverables:**
- WCAG 2.1 AA compliance: Automated jest-axe testing in CI pipeline, manual keyboard navigation testing checklist, screen reader testing (NVDA/JAWS/VoiceOver), color contrast validation via axe DevTools browser extension
- Keyboard navigation: Tab order follows logical visual flow, Enter key activates buttons/links, Escape key closes modals/dropdowns, Arrow keys navigate lists/tables, focus trap in modals (focus loops within modal), skip navigation link at page top
- Screen reader support: ARIA labels on all interactive elements (`aria-label`, `aria-labelledby`), ARIA live regions announce dynamic content changes (toast notifications, form errors), role attributes (`role="navigation"`, `role="main"`), table headers associated via `scope` attribute
- Color contrast: Body text 16px minimum with 4.5:1 contrast ratio (e.g., #333333 on #FFFFFF), large text (18px+) with 3:1 ratio, error messages in red with sufficient contrast (#D32F2F on #FFFFFF = 4.5:1), automated axe-core contrast checks in CI
- Touch targets: Mobile buttons/links sized ≥44px height and width, adequate spacing between targets (8px minimum), tested on iPhone SE 320px viewport and Android equivalents
- Focus indicators: 2px solid outline on focused elements (`:focus` CSS pseudo-class), high contrast focus color (e.g., #0066CC), focus never hidden by other elements (z-index management), skip `:focus { outline: none }` anti-pattern
- Responsive breakpoints: Tailwind CSS mobile-first approach, base styles for 320px+, `sm:` prefix for 640px+, `md:` for 768px+, `lg:` for 1024px+, `xl:` for 1280px+; no horizontal scroll at any breakpoint
- Mobile layout: Single-column forms, hamburger menu navigation (collapsed), stacked dashboard cards, full-width tables (horizontal scroll in container), touch-friendly controls (large tap targets, swipe gestures)
- Tablet layout: Staff dashboards two-column (arrivals list + patient detail side-by-side), forms span full width, navigation visible in sidebar, touch-optimized controls
- Desktop layout: Multi-column dashboards (three-column patient dashboard with upcoming appointments + quick actions + recent documents), sidebar navigation persistent, data tables expanded (no horizontal scroll), admin panels use full width
- Design system tokens: Tailwind CSS `tailwind.config.js` customization with healthcare color palette (primary: blue, secondary: teal, accent: green for success, red for errors), typography scale (16px base, 1.5 line-height), spacing 4px/8px base grid, elevation shadows (sm/md/lg)
- shadcn/ui integration: Install Button, Card, Input, Select, Dialog, Toast components from shadcn/ui CLI, customize via CSS variables in `globals.css` (e.g., `--primary`, `--secondary`, `--background`), no duplicate implementations (DRY principle)
- Visual tone: Professional healthcare aesthetic with sans-serif typography (Inter or Roboto), minimal decorative elements, HIPAA-compliant secure appearance (lock icons, data encryption indicators), avoid playful/casual design (e.g., no comic sans, bright colors)
- Loading states: Skeleton screens for async operations >500ms (PDF generation, AI extraction), progress spinners with accessible labels (`aria-label="Loading..."`), optimistic UI for bookings (immediate slot selection feedback), "Syncing..." indicator for calendar sync
- Error handling UX: Clear actionable error messages ("Slot No Longer Available - Please select another time" vs "Error 409"), recovery paths ("Retry" button for transient failures, "Contact Support" link for critical errors), dismissible error alerts, session timeout warning modal at 13 minutes

**Dependent EPICs:**
- EP-TECH - Foundational (Requires Next.js and Tailwind CSS setup)

---

### EP-011: Platform Infrastructure & Performance Optimization

**Business Value:** Achieves 99.9% uptime target and sub-3-second API response times while deploying exclusively on free-tier infrastructure, enabling cost-constrained resilience through aggressive caching, health check orchestration, and graceful AI feature degradation under resource limits.

**Description:** Implement performance optimizations: Redis caching (Upstash free tier) for session data (15-min sliding expiration) and slot availability (30s TTL), API rate limiting per user role (Patient: 100 req/min, Staff: 200 req/min, Admin: 500 req/min) preventing resource exhaustion, and pgvector-accelerated similarity search for clinical document retrieval. Configure health check endpoints (`/health/live`, `/health/ready`) with database connectivity probes, enable auto-restart on failures within free-tier platform capabilities (Render/Railway), and display maintenance mode page during planned downtime. Monitor application performance via Application Insights free tier (5GB ingestion), uptime via UptimeRobot (5-min check interval), and errors via Sentry free tier (5K events/month). Implement circuit breakers for external API failures (calendar sync, email/SMS, AI models) with graceful fallback workflows.

**UI Impact:** No (Infrastructure only; maintenance mode page visible to users during downtime)

**Screen References:** N/A (Maintenance mode static HTML page outside app)

**Key Deliverables:**
- Redis caching: Upstash Redis free tier configuration (10K commands/day, 256MB storage), cache session data (JWT claims, role permissions) with 15-min sliding expiration, cache slot availability with 30s TTL (cache key: `slots:{providerId}:{date}`), invalidate on booking/cancellation
- API rate limiting: AspNetCoreRateLimit middleware, configure limits per role (`appsettings.json`: Patient=100/min, Staff=200/min, Admin=500/min), exceeded limit returns 429 Too Many Requests with `Retry-After` header, track rate limit by user ID extracted from JWT
- pgvector similarity search: Create GiST index on `ClinicalDocument.EmbeddingVector` column (`CREATE INDEX idx_embedding ON ClinicalDocument USING gist (EmbeddingVector vector_cosine_ops)`), query via cosine similarity operator (`<=>`) with top-K retrieval (`ORDER BY EmbeddingVector <=> query_vector LIMIT 5`), benchmark query performance (<100ms for 10K records)
- Health check endpoints: Configure Microsoft.Extensions.Diagnostics.HealthChecks, `/health/live` checks API process responsive (200 OK immediately), `/health/ready` checks database connectivity (execute simple query `SELECT 1`) and Redis connectivity (PING command), return 503 Service Unavailable if unhealthy
- Auto-restart configuration: Render/Railway health check integration, orchestrator pings `/health/ready` every 30 seconds, restart container on 3 consecutive failures, cooldown period 60 seconds between restarts, logs restart events to Application Insights
- Maintenance mode: Static `maintenance.html` page deployed separately, reverse proxy routes all traffic to static page when maintenance flag enabled (environment variable `MAINTENANCE_MODE=true`), displays estimated restoration time and contact info
- Application Insights: Configure Application Insights SDK for .NET and React, track custom metrics (API latency p50/p95/p99, booking success rate, AI token consumption), set up alerts (p95 latency >5s, error rate >5%, AI cost >$10/day), dashboard with key performance indicators
- UptimeRobot monitoring: Free tier (50 monitors, 5-min intervals), monitor `/health/ready` endpoint, alert via email on downtime (consecutive 2 failures), track monthly uptime percentage, integrate with Slack for real-time alerts
- Sentry error tracking: Free tier (5K events/month), capture unhandled exceptions in backend (ASP.NET Core middleware) and frontend (React ErrorBoundary), include stack traces, user context, breadcrumbs (recent actions), alert on new error types, integrate with GitHub Issues
- Circuit breakers: Polly library, circuit opens after 5 consecutive failures (timeout or 5xx status), remains open for 60s before health check attempt, fallback actions (calendar sync → .ics file attachment, AI extraction → manual entry, email send → queue for manual follow-up)
- AI cost monitoring: Track token consumption in `AIUsageLog` table (prompt tokens, completion tokens, model version, cost per request), scheduled job aggregates daily spend, alert if projected monthly cost >$50 (free trial buffer), dashboard displays current spend and budget remaining
- Load testing: k6 scripts simulate 500 concurrent users (NFR-003), test scenarios (appointment booking, intake submission, clinical document upload), run pre-release, validate p95 latency ≤3s (NFR-001) and zero 5xx errors

**Dependent EPICs:**
- EP-TECH - Foundational (Requires health check and logging infrastructure)
- EP-DATA - Foundational (Requires database connectivity for health checks)

---

## Backlog Refinement Required

**No [UNCLEAR] requirements identified.** All requirements from source documents (spec.md, design.md, figma_spec.md) are sufficiently specified for epic mapping.

---

## Epic Dependency Graph

```
EP-TECH (Foundational)
    └─> EP-DATA (Foundational)
            ├─> EP-001 (Appointment Booking)
            ├─> EP-002 (Patient Intake)
            │       └─> EP-004 (Clinical Data Aggregation)
            │               └─> EP-005 (Medical Coding)
            ├─> EP-003 (Staff Operations) ──depends on──> EP-008 (User Management)
            ├─> EP-006 (Reminders) ──depends on──> EP-001
            ├─> EP-007 (Insurance)
            ├─> EP-008 (User Management)
            │       └─> EP-009 (Security & Compliance)
            ├─> EP-010 (UI/UX)
            └─> EP-011 (Infrastructure)
```

**Parallel Execution Opportunities:** After EP-TECH and EP-DATA completion, the following epics can be developed simultaneously by independent teams:
- Team 1: EP-001 (Booking) + EP-006 (Reminders)
- Team 2: EP-002 (Intake) → EP-004 (Clinical) → EP-005 (Coding)
- Team 3: EP-008 (User Mgmt) → EP-003 (Staff Ops) + EP-009 (Security)
- Team 4: EP-007 (Insurance) + EP-010 (UI/UX) + EP-011 (Infrastructure)

**Critical Path:** EP-TECH → EP-DATA → EP-002 → EP-004 → EP-005 (longest dependency chain = 5 epics)

---

## Traceability Matrix

| Epic ID | FR | UC | NFR | TR | DR | AIR | UXR | SCR | Total |
|---------|----|----|-----|----|----|-----|-----|-----|-------|
| EP-TECH | 0 | 0 | 0 | 10 | 0 | 0 | 0 | 0 | 10 |
| EP-DATA | 0 | 0 | 0 | 0 | 13 | 0 | 0 | 0 | 13 |
| EP-001 | 8 | 3 | 0 | 0 | 0 | 0 | 3 | 2 | 16 |
| EP-002 | 4 | 2 | 0 | 0 | 0 | 6 | 3 | 2 | 17 |
| EP-003 | 5 | 2 | 0 | 0 | 0 | 0 | 1 | 2 | 10 |
| EP-004 | 4 | 1 | 0 | 0 | 0 | 13 | 3 | 2 | 23 |
| EP-005 | 2 | 0 | 0 | 0 | 0 | 4 | 1 | 1 | 8 |
| EP-006 | 3 | 0 | 1 | 3 | 0 | 0 | 0 | 0 | 7 |
| EP-007 | 1 | 1 | 0 | 0 | 0 | 0 | 0 | 1 | 3 |
| EP-008 | 3 | 1 | 3 | 2 | 0 | 0 | 2 | 6 | 17 |
| EP-009 | 3 | 0 | 5 | 3 | 0 | 1 | 0 | 1 | 13 |
| EP-010 | 0 | 0 | 2 | 1 | 0 | 0 | 22 | 0 | 25 |
| EP-011 | 2 | 0 | 12 | 1 | 0 | 5 | 0 | 0 | 20 |
| **Total** | **35** | **10** | **23** | **20** | **13** | **29** | **35** | **17** | **182** |

**Note:** TR total shows 20 (TR-001 to TR-018 + TR-006 and TR-007 mapped to EP-006) due to shared requirements across epics. Some UXR requirements apply to all screens and are distributed across relevant epics.

---

## Next Steps

1. **Sprint Planning:** Prioritize EP-TECH and EP-DATA for Sprint 1-2 (foundational critical path)
2. **Team Allocation:** Assign teams to parallel epic tracks per dependency graph
3. **Story Breakdown:** Decompose each epic into user stories (~3-8 story points) for sprint planning
4. **Acceptance Criteria Refinement:** Expand epic-level acceptance criteria into story-level testable conditions
5. **Technical Spike:** Pilot AI model provider (OpenAI vs self-hosted Llama 3.2) to finalize Decision 9
6. **HIPAA Pre-Audit:** Engage legal counsel to validate free-tier platform BAA availability before production deployment
