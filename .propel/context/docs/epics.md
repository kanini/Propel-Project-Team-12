# Epic Backlog - Unified Patient Access & Clinical Intelligence Platform

## Document Information

| Attribute | Value |
|-----------|-------|
| Version | 1.0 |
| Created | 2026-03-17 |
| Project Type | Green-field |
| Total Epics | 18 |
| Total Requirements | 155 (1 UNCLEAR) |

---

## Epic Summary Table

| Epic ID | Epic Title | Mapped Requirement IDs | Req Count |
|---------|------------|------------------------|-----------|
| EP-TECH | Project Foundation & Infrastructure | TR-001, TR-002, TR-003, TR-004, TR-005, TR-006, TR-007, TR-008, TR-009, TR-010, TR-011, TR-018, TR-019, TR-020, TR-021 | 15 |
| EP-DATA | Data Structure & Integrity | DR-001, DR-002, DR-003, DR-004, DR-005, DR-006, DR-007, DR-008, DR-009 | 9 |
| EP-001 | User Account & Authentication | FR-001, FR-002, FR-003, NFR-010, NFR-011, NFR-012, UXR-002, UXR-003, UXR-604 | 9 |
| EP-002 | Appointment Booking & Scheduling | FR-004, FR-005, FR-006, FR-007, FR-008, FR-009, UXR-101, UXR-102, UXR-303 | 9 |
| EP-003 | Patient Intake System | FR-010, FR-011, FR-012, FR-013, UXR-103, UXR-602 | 6 |
| EP-004 | Clinical Document Processing | FR-014, FR-015, FR-016, FR-017, FR-018, UXR-501 | 6 |
| EP-005 | Medical Coding & Verification | FR-019, FR-020, FR-021, FR-022, UXR-404 | 5 |
| EP-006 | Notifications & Calendar Integration | FR-023, FR-024, FR-025, FR-026, FR-027, TR-013, TR-014, TR-015, TR-016, TR-017 | 10 |
| EP-007 | Insurance Validation | FR-028, FR-029 | 2 |
| EP-008 | Staff Operations & Queue Management | FR-030, FR-031, FR-032, FR-033, UXR-105, UXR-106 | 6 |
| EP-009 | Administration & User Management | FR-034, FR-035, FR-036, FR-037 | 4 |
| EP-010 | Security & Compliance | FR-038, FR-039, NFR-008, NFR-009, NFR-013, NFR-014, NFR-015, NFR-016, NFR-017 | 9 |
| EP-011 | Performance & Reliability | NFR-001, NFR-002, NFR-003, NFR-004, NFR-005, NFR-006, NFR-007, NFR-018, NFR-019, NFR-020, NFR-021, NFR-022 | 12 |
| EP-012 | AI Core Engine & Extraction | AIR-001, AIR-002, AIR-003, AIR-R01, AIR-R02, AIR-R03, AIR-R04, AIR-O01, AIR-O02, AIR-O03, AIR-O04 | 11 |
| EP-013 | AI Quality & Safety Controls | AIR-004, AIR-005, AIR-006, AIR-Q01, AIR-Q02, AIR-Q03, AIR-Q04, AIR-S01, AIR-S02, AIR-S03, AIR-S04 | 11 |
| EP-014 | Accessibility & Responsiveness | UXR-001, UXR-201, UXR-202, UXR-203, UXR-204, UXR-205, UXR-206, UXR-301, UXR-302, UXR-304 | 10 |
| EP-015 | Visual Design & Interaction | UXR-401, UXR-402, UXR-403, UXR-502, UXR-503, UXR-504, UXR-505, UXR-601, UXR-603, UXR-605 | 10 |
| EP-016 | Data Operations & Security | DR-010, DR-011, DR-012, DR-013, DR-014, DR-015, DR-016, DR-017, DR-018, DR-019 | 10 |

---

## Backlog Refinement Required

The following requirements are tagged as **[UNCLEAR]** and require clarification before epic mapping:

| Requirement ID | Description | Clarification Needed |
|----------------|-------------|----------------------|
| TR-012 | System MUST define specific cloud provider for production scaling beyond free tier | Which cloud provider should be targeted for Phase 2 scaling? Azure, AWS, or GCP? |

---

## Epic Descriptions

### EP-TECH: Project Foundation & Infrastructure

**Business Value**: Enables all subsequent development by establishing project foundation, technology stack, development environment, and architectural patterns required for the Unified Patient Access & Clinical Intelligence Platform.

**Description**: This foundational epic establishes the technical infrastructure for the green-field project. It includes technology stack setup (ASP.NET Core 8.0, React 18+, PostgreSQL 16+ with pgvector), Clean Architecture implementation with CQRS pattern, development environment configuration, and core architectural patterns. This epic is critical-path and must be completed before all feature development begins.

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:
- ASP.NET Core 8.0 backend project structure with Clean Architecture layers (Domain, Application, Infrastructure, Presentation)
- React 18+ frontend project with Vite build configuration
- PostgreSQL 16+ database with pgvector extension setup
- Upstash Redis integration for session management and caching
- MediatR CQRS implementation with pipeline behaviors
- Entity Framework Core 8 ORM configuration
- FluentValidation integration
- Serilog structured logging setup
- Repository pattern with Unit of Work implementation
- Domain events infrastructure for async operations
- IIS/Windows Services deployment configuration
- Netlify/Vercel frontend deployment pipeline
- Development environment documentation

**Dependent EPICs**: None (foundation epic)

---

### EP-DATA: Data Structure & Integrity

**Business Value**: Enables data operations for all feature epics requiring persistence by establishing the core data model, entity relationships, integrity constraints, and data validation rules.

**Description**: This data foundation epic implements the database schema based on the domain model defined in models.md. It includes entity creation for Patient, Appointment, Slot, SwapPreference, ClinicalDocument, ExtractedClinicalData, MedicalCode, DataConflict, IntakeRecord, User, and AuditLog. The epic establishes referential integrity, de-duplication logic, and version history for patient profiles.

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:
- EF Core entity configurations for all domain entities
- Database migrations for schema creation
- Unique constraints (Patient email, User email)
- Referential integrity between Patient → Appointment → Slot relationships
- Clinical document secure file reference storage pattern
- Extracted clinical data JSONB schema for vitals, medications, history
- Medical codes storage with AI confidence scores and verification status
- Insurance validation against predefined dummy records
- Patient data de-duplication logic
- Version history triggers for patient profile changes

**Dependent EPICs**: EP-TECH

---

### EP-001: User Account & Authentication

**Business Value**: Enables secure patient, staff, and admin access to the platform with role-based permissions, protecting PHI while providing seamless authentication experience.

**Description**: This epic implements the complete authentication and authorization system including patient account creation with email validation, secure password requirements, role-based session management for Patient/Staff/Admin roles, 15-minute automatic session timeout with graceful re-authentication, and account lockout after 5 consecutive failed attempts. Visual distinction between user portals is provided per role.

**UI Impact**: Yes

**Screen References**: SCR-001, SCR-002

**Key Deliverables**:
- ASP.NET Identity integration with JWT authentication
- Patient account registration with email validation
- Secure password policy enforcement
- Role-based access control (Patient, Staff, Admin)
- 15-minute session timeout with auto-save and warning modal
- Account lockout mechanism (5 failed attempts)
- Role-specific portal navigation and color accents
- Session context preservation during timeout
- Re-authentication flow without data loss

**Dependent EPICs**: EP-TECH, EP-DATA

---

### EP-002: Appointment Booking & Scheduling

**Business Value**: Reduces 15% no-show rate through simplified booking with calendar visualization, real-time availability, and dynamic preferred slot swap feature that maximizes schedule utilization.

**Description**: This epic implements the core appointment booking functionality including calendar view with real-time availability updates (within 500ms), slot selection and booking, dynamic preferred slot swap mechanism allowing patients to book available slots while indicating preference for unavailable slots, automatic swap execution when preferred slots become available, and waitlist enrollment. Patients can cancel or reschedule with appropriate notice.

**UI Impact**: Yes

**Screen References**: SCR-003, SCR-004, SCR-005

**Key Deliverables**:
- Calendar view component with month navigation
- Real-time slot availability updates (500ms)
- Slot selection and booking flow (≤3 clicks)
- Preferred slot swap registration and monitoring
- Automatic swap execution with original slot release
- Patient notification on swap completion
- Waitlist enrollment for unavailable slots
- Appointment cancellation and rescheduling
- Concurrent booking conflict resolution
- Mobile-friendly calendar with touch gestures

**Dependent EPICs**: EP-001, EP-DATA

---

### EP-003: Patient Intake System

**Business Value**: Provides flexible intake options (AI conversational or manual form) that accommodate diverse patient preferences, with seamless mode switching that preserves data and enables completion without human assistance.

**Description**: This epic implements dual-mode patient intake: AI-driven conversational intake using natural language dialogue and traditional manual form-based intake. Patients can freely switch between modes at any time with data preservation. All intake information can be edited without requiring human assistance. The system displays intake summary for review before confirmation.

**UI Impact**: Yes

**Screen References**: SCR-006, SCR-007

**Key Deliverables**:
- AI conversational intake interface with chat UI
- Manual form-based intake with structured fields
- Seamless mode toggle with data preservation
- AI-to-structured data extraction
- Intake summary review screen
- Inline editing capability
- Partial intake preservation on session timeout
- Graceful AI fallback when service unavailable
- Form validation with inline error messages
- Auto-save functionality

**Dependent EPICs**: EP-001, EP-DATA

---

### EP-004: Clinical Document Processing

**Business Value**: Transforms 20-minute manual extraction into 2-minute verification by automating clinical document upload, AI-powered structured data extraction, unified 360-degree patient view generation, and proactive conflict detection for safety risk prevention.

**Description**: This epic enables patients to upload historical clinical documents in PDF format with automated data extraction. The AI engine extracts vitals, medical history, and medications from uploaded documents. Multiple documents are aggregated into a unified 360-degree patient view. Critical data conflicts (e.g., conflicting medications) are detected and explicitly highlighted for human review. De-duplication ensures clean consolidated data.

**UI Impact**: Yes

**Screen References**: SCR-008, SCR-009

**Key Deliverables**:
- PDF document upload interface with drag-and-drop
- File format and size validation
- Secure document storage with file references
- AI-powered clinical data extraction
- 360-degree patient view aggregation
- Multi-document data conflict detection
- Conflict severity classification
- De-duplication logic for medications, diagnoses
- Loading feedback during upload (<200ms)
- Document processing status tracking
- Extracted data summary view

**Dependent EPICs**: EP-001, EP-DATA, EP-012

---

### EP-005: Medical Coding & Verification

**Business Value**: Achieves >98% AI-Human Agreement Rate for suggested medical codes, enabling rapid verification workflow while maintaining clinical accuracy and supporting future claims submission readiness.

**Description**: This epic implements AI-powered mapping of extracted clinical data to ICD-10 diagnostic codes and CPT procedure codes. A verification interface allows staff to confirm, modify, or reject AI-suggested codes with supporting evidence from clinical documents. The system tracks AI-Human Agreement Rate metrics and provides visual distinction between AI-suggested and human-verified codes.

**UI Impact**: Yes

**Screen References**: SCR-016

**Key Deliverables**:
- ICD-10 code mapping from clinical narratives
- CPT procedure code mapping
- Confidence score display for suggestions
- Supporting evidence citations from source documents
- Staff verification interface (confirm/modify/reject)
- Agreement rate metrics tracking
- Visual badge for AI vs human-verified codes
- Low confidence flagging for extra attention
- Verification audit trail
- Manual code addition capability

**Dependent EPICs**: EP-004, EP-012

---

### EP-006: Notifications & Calendar Integration

**Business Value**: Reduces no-show rates through multi-channel reminders (SMS/Email) with calendar synchronization, ensuring patients have appointment visibility in their preferred tools.

**Description**: This epic implements automated appointment reminders via SMS and Email channels, PDF confirmation generation with email delivery, and calendar synchronization with Google Calendar and Outlook Calendar via free API integration. Queue-based delivery with retry logic handles API rate limits gracefully.

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:
- SMS gateway integration for reminders
- Email gateway integration for reminders and confirmations
- PDF appointment confirmation generation
- Google Calendar API integration
- Outlook Calendar API integration
- Reminder scheduling configuration
- Queue-based delivery with retry logic
- Rate limit handling with exponential backoff
- Delivery status logging
- Failed notification handling and alerting

**Dependent EPICs**: EP-TECH

---

### EP-007: Insurance Validation

**Business Value**: Prevents claim denials and patient frustration by providing soft insurance pre-check validation before appointments with clear pass/fail results.

**Description**: This epic implements insurance pre-check functionality that validates patient-provided insurance name and ID against internal predefined dummy records. Clear pass/fail validation results are displayed to staff with actionable error messages for failed validations.

**UI Impact**: Yes

**Screen References**: SCR-017

**Key Deliverables**:
- Insurance validation interface
- Validation against predefined dummy records
- Clear pass/fail result display
- Failure reason messaging
- Retry and override options
- Validation outcome recording
- Pre-appointment validation workflow

**Dependent EPICs**: EP-001

---

### EP-008: Staff Operations & Queue Management

**Business Value**: Streamlines front desk operations with walk-in booking, same-day queue management, and patient arrival marking, enabling efficient patient flow and reducing wait times.

**Description**: This epic implements staff-facing operations including walk-in appointment creation with optional patient account creation, same-day queue management dashboard with ordered patient list, patient arrival marking (centralized, no self-check-in), and no-show risk assessment based on rule-based weightings and pattern detection.

**UI Impact**: Yes

**Screen References**: SCR-011, SCR-012, SCR-013, SCR-014, SCR-015

**Key Deliverables**:
- Walk-in booking interface
- Patient search with autocomplete
- New patient minimal info capture
- Optional account creation post-booking
- Same-day queue dashboard
- Queue ordering and reordering
- Patient arrival status marking
- Wait time monitoring
- No-show risk assessment display
- Bulk actions on queue (multi-select)
- Keyboard shortcuts for high-frequency actions
- Conflict review interface

**Dependent EPICs**: EP-001, EP-002

---

### EP-009: Administration & User Management

**Business Value**: Enables system governance through user account management, role configuration, and audit compliance monitoring for HIPAA requirements.

**Description**: This epic implements administrative functions including user account creation, update, and deactivation with role assignment. Role-based access control is enforced for Patient, Staff, and Admin roles. Immutable audit logs capture all patient and staff actions, with admin-accessible search and view capabilities.

**UI Impact**: Yes

**Screen References**: SCR-018, SCR-019, SCR-020, SCR-021

**Key Deliverables**:
- User management console (CRUD operations)
- Role assignment interface
- Account deactivation (soft delete)
- Role-based access control enforcement
- Immutable audit log storage
- Audit log search and filtering
- Audit log export for compliance
- Admin dashboard with system metrics
- User status management

**Dependent EPICs**: EP-001

---

### EP-010: Security & Compliance

**Business Value**: Ensures 100% HIPAA compliance with PHI encryption, secure data transmission, immutable audit logging, and minimum necessary access controls to prevent data breaches and regulatory penalties.

**Description**: This epic implements comprehensive security controls including AES-256 encryption for PHI at rest, TLS 1.3 encryption for data in transit, input sanitization to prevent injection attacks, immutable audit logs with 7-year retention, and minimum necessary access standard for PHI. All security measures align with HIPAA requirements.

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:
- AES-256 encryption for PHI at rest
- TLS 1.3 configuration for all endpoints
- Database-level transparent encryption for PHI columns
- Input sanitization middleware
- Immutable audit log implementation with triggers
- 7-year audit log retention configuration
- Minimum necessary access policy enforcement
- PHI masking in non-production environments
- Security headers configuration
- CORS policy implementation

**Dependent EPICs**: EP-TECH

---

### EP-011: Performance & Reliability

**Business Value**: Achieves 99.9% uptime target with 2-second API response time, supporting 200 concurrent users and 500+ same-day patients per facility for healthcare-critical operations.

**Description**: This epic implements performance and reliability requirements including 2-second p95 API response time, 30-second clinical document processing for 50-page documents, 200 concurrent user support, 500ms real-time availability updates, 99.9% uptime target, graceful degradation for external service failures, health check endpoints, horizontal scaling capability, and retry logic with exponential backoff.

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:
- API performance optimization (2s p95)
- Document processing pipeline optimization
- Connection pooling and caching strategy
- Redis cache integration for hot data
- Health check endpoint implementation
- Load balancing configuration
- Horizontal scaling capability
- Circuit breaker implementation
- Retry logic with exponential backoff
- Graceful degradation patterns
- Performance monitoring and alerting
- Scalability for 500+ same-day queue

**Dependent EPICs**: EP-TECH

---

### EP-012: AI Core Engine & Extraction

**Business Value**: Powers the clinical intelligence engine with RAG-based document extraction, medical code mapping, and operational guardrails that enable the 20-minute to 2-minute transformation in clinical data preparation.

**Description**: This epic implements the core AI infrastructure including RAG pipeline for clinical document extraction, document chunking (512 tokens, 10% overlap), vector embeddings with pgvector storage (HNSW indexing), semantic retrieval (top-5 chunks, cosine ≥0.75), ICD-10 and CPT code mapping with source citations, and operational controls (4,000 token budget, circuit breaker, model rollback, embedding caching).

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:
- RAG pipeline implementation
- Document text extraction (OCR fallback)
- 512-token chunking with 10% overlap
- Embedding generation (text-embedding-3-small)
- pgvector storage with HNSW indexing
- Cosine similarity retrieval (≥0.75)
- Semantic reranking with recency weighting
- ICD-10 code mapping from clinical text
- CPT code mapping from narratives
- Source text citation generation
- 4,000 token budget enforcement
- Circuit breaker (3 failures → 30s cooldown)
- Model version rollback capability
- Embedding cache for processed documents

**Dependent EPICs**: EP-TECH, EP-DATA

---

### EP-013: AI Quality & Safety Controls

**Business Value**: Maintains >98% AI-Human Agreement Rate with hallucination rate <2%, ensuring Trust-First AI architecture with mandatory human verification and comprehensive safety guardrails.

**Description**: This epic implements AI quality and safety controls including conversational intake with tool calling pattern, semantic conflict detection across documents, source citations for all suggestions, hallucination rate <2% enforcement, output schema validity ≥99%, PII redaction before model invocation, role-based document access filtering, prompt/response audit logging with 7-year retention, and automatic fallback to manual workflow when confidence <80%.

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:
- Conversational intake AI integration
- Tool calling pattern for structured extraction
- Semantic conflict detection algorithm
- Source citation generation for all outputs
- Hallucination monitoring and enforcement
- Output schema validation (≥99% valid)
- Structured output enforcement
- PII redaction middleware
- Role-based RAG retrieval filtering
- Prompt/response audit logging
- Confidence threshold enforcement (80%)
- Manual fallback workflow trigger
- AI-Human Agreement Rate tracking

**Dependent EPICs**: EP-012

---

### EP-014: Accessibility & Responsiveness

**Business Value**: Ensures healthcare accessibility for all patients including those with disabilities through WCAG 2.2 AA compliance, screen reader support, and responsive design across mobile, tablet, and desktop.

**Description**: This epic implements accessibility and responsiveness requirements including consistent global navigation across all views, WCAG 2.2 AA compliance with zero critical errors, visible focus indicators (≥3:1 contrast), screen reader support with semantic HTML and ARIA labels, text alternatives for all images, 44x44px minimum touch targets on mobile, keyboard navigation for all workflows, and responsive layouts for mobile (320px), tablet (768px), and desktop (1024px+).

**UI Impact**: Yes

**Screen References**: All screens

**Key Deliverables**:
- Global navigation component
- WCAG 2.2 AA audit and compliance
- Focus indicator styling (3:1 contrast)
- Semantic HTML structure
- ARIA labels and roles
- Alt text for all images and icons
- 44x44px touch targets
- Keyboard navigation implementation
- Tab order optimization
- Responsive breakpoint layouts
- Mobile navigation (bottom nav/hamburger)
- Vertical form stacking on mobile
- prefers-reduced-motion support

**Dependent EPICs**: EP-TECH

---

### EP-015: Visual Design & Interaction

**Business Value**: Delivers professional healthcare aesthetic with design token consistency, dark/light mode support, smooth interactions, and clear feedback that builds patient trust and staff efficiency.

**Description**: This epic implements visual design and interaction patterns including design token usage for all colors, typography, and spacing, light and dark mode support with accessible contrast, healthcare-appropriate calming color palette, loading feedback within 200ms, toast notifications for successful actions, smooth screen transitions (150-300ms), inline form validation, and prefers-reduced-motion honor.

**UI Impact**: Yes

**Screen References**: All screens

**Key Deliverables**:
- Design token implementation
- Theme provider with light/dark mode
- Healthcare color palette application
- Loading skeleton and spinner components
- Toast notification system
- Screen transition animations
- Inline validation feedback
- Error message styling (actionable)
- Empty state designs with CTAs
- Form data persistence on errors
- prefers-reduced-motion detection

**Dependent EPICs**: EP-014

---

### EP-016: Data Operations & Security

**Business Value**: Ensures data durability and compliance with automated backups, point-in-time recovery, zero-downtime migrations, and PHI masking that protects patient data throughout its lifecycle.

**Description**: This epic implements data operations and security including audit log retention (7 years minimum), configurable document retention policies, soft delete for patient records, daily automated database backups, point-in-time recovery within 24-hour window, encrypted backup storage with separate key management, zero-downtime schema migrations, database schema versioning with rollback, PHI masking in non-production environments, and database-level transparent encryption.

**UI Impact**: No

**Screen References**: N/A

**Key Deliverables**:
- Audit log retention automation
- Document retention policy engine
- Soft delete implementation
- Daily backup automation
- Point-in-time recovery configuration
- Encrypted backup storage
- Key management separation
- EF Core migration strategy
- Schema versioning with rollback scripts
- PHI masking utility for lower environments
- Transparent data encryption configuration

**Dependent EPICs**: EP-DATA

---

## Appendix: Requirement Traceability Matrix

### Functional Requirements (FR)

| FR-ID | Description | Epic |
|-------|-------------|------|
| FR-001 | Patient account creation with email validation | EP-001 |
| FR-002 | Role-based session management | EP-001 |
| FR-003 | 15-minute automatic session timeout | EP-001 |
| FR-004 | Calendar view with real-time availability | EP-002 |
| FR-005 | Book appointments from available slots | EP-002 |
| FR-006 | Dynamic preferred slot swap | EP-002 |
| FR-007 | Automatic swap execution | EP-002 |
| FR-008 | Waitlist enrollment | EP-002 |
| FR-009 | Appointment cancel/reschedule | EP-002 |
| FR-010 | AI conversational intake | EP-003 |
| FR-011 | Manual form intake | EP-003 |
| FR-012 | Switch between intake modes | EP-003 |
| FR-013 | Edit intake without human assistance | EP-003 |
| FR-014 | Clinical document upload (PDF) | EP-004 |
| FR-015 | Extract structured data from documents | EP-004 |
| FR-016 | Generate 360-degree patient view | EP-004 |
| FR-017 | Detect and highlight data conflicts | EP-004 |
| FR-018 | De-duplicate patient data | EP-004 |
| FR-019 | Map to ICD-10 codes | EP-005 |
| FR-020 | Map to CPT codes | EP-005 |
| FR-021 | Staff verification interface | EP-005 |
| FR-022 | >98% AI-Human Agreement Rate | EP-005 |
| FR-023 | SMS appointment reminders | EP-006 |
| FR-024 | Email appointment reminders | EP-006 |
| FR-025 | PDF confirmation via email | EP-006 |
| FR-026 | Google Calendar sync | EP-006 |
| FR-027 | Outlook Calendar sync | EP-006 |
| FR-028 | Insurance pre-check validation | EP-007 |
| FR-029 | Pass/fail validation results | EP-007 |
| FR-030 | Walk-in booking with optional account | EP-008 |
| FR-031 | Same-day queue dashboard | EP-008 |
| FR-032 | Staff marks patient arrival | EP-008 |
| FR-033 | No-show risk assessment | EP-008 |
| FR-034 | Admin user management | EP-009 |
| FR-035 | Role-based access control | EP-009 |
| FR-036 | Immutable audit logs | EP-009 |
| FR-037 | Audit log search | EP-009 |
| FR-038 | PHI encryption at rest (AES-256) | EP-010 |
| FR-039 | Data encryption in transit (TLS 1.3) | EP-010 |

### Non-Functional Requirements (NFR)

| NFR-ID | Description | Epic |
|--------|-------------|------|
| NFR-001 | 2-second API response (p95) | EP-011 |
| NFR-002 | 30-second document processing | EP-011 |
| NFR-003 | 200 concurrent users | EP-011 |
| NFR-004 | 500ms availability updates | EP-011 |
| NFR-005 | 99.9% uptime | EP-011 |
| NFR-006 | Graceful degradation | EP-011 |
| NFR-007 | Health check endpoints | EP-011 |
| NFR-008 | AES-256 encryption at rest | EP-010 |
| NFR-009 | TLS 1.3 encryption in transit | EP-010 |
| NFR-010 | Role-based access control | EP-001 |
| NFR-011 | 15-minute session timeout | EP-001 |
| NFR-012 | Account lockout (5 attempts) | EP-001 |
| NFR-013 | Input sanitization | EP-010 |
| NFR-014 | Immutable audit logs | EP-010 |
| NFR-015 | 7-year audit retention | EP-010 |
| NFR-016 | Minimum necessary access | EP-010 |
| NFR-017 | Audit log search/export | EP-010 |
| NFR-018 | Horizontal scaling | EP-011 |
| NFR-019 | 500+ same-day patients | EP-011 |
| NFR-020 | 10,000+ documents per patient | EP-011 |
| NFR-021 | Retry logic with backoff | EP-011 |
| NFR-022 | Preserve partial data on timeout | EP-011 |

### Technical Requirements (TR)

| TR-ID | Description | Epic |
|-------|-------------|------|
| TR-001 | ASP.NET Core 8.0 | EP-TECH |
| TR-002 | React 18+ | EP-TECH |
| TR-003 | PostgreSQL 16+ with pgvector | EP-TECH |
| TR-004 | Windows Services/IIS deployment | EP-TECH |
| TR-005 | Upstash Redis | EP-TECH |
| TR-006 | Clean Architecture | EP-TECH |
| TR-007 | CQRS pattern | EP-TECH |
| TR-008 | Repository with Unit of Work | EP-TECH |
| TR-009 | Domain events | EP-TECH |
| TR-010 | Free-tier platform deployment | EP-TECH |
| TR-011 | Native deployment (no containers) | EP-TECH |
| TR-012 | [UNCLEAR] Cloud provider for scaling | CLARIFICATION NEEDED |
| TR-013 | Google Calendar API | EP-006 |
| TR-014 | Outlook Calendar API | EP-006 |
| TR-015 | SMS gateway integration | EP-006 |
| TR-016 | Email gateway integration | EP-006 |
| TR-017 | Queue with retry for rate limits | EP-006 |
| TR-018 | FluentValidation | EP-TECH |
| TR-019 | MediatR for CQRS | EP-TECH |
| TR-020 | Entity Framework Core 8 | EP-TECH |
| TR-021 | Serilog structured logging | EP-TECH |

### Data Requirements (DR)

| DR-ID | Description | Epic |
|-------|-------------|------|
| DR-001 | Patient email as unique ID | EP-DATA |
| DR-002 | Appointments with audit trail | EP-DATA |
| DR-003 | Documents with secure file refs | EP-DATA |
| DR-004 | Extracted data with traceability | EP-DATA |
| DR-005 | Medical codes with confidence | EP-DATA |
| DR-006 | Referential integrity | EP-DATA |
| DR-007 | Insurance validation records | EP-DATA |
| DR-008 | Patient data de-duplication | EP-DATA |
| DR-009 | Patient profile version history | EP-DATA |
| DR-010 | 7-year audit log retention | EP-016 |
| DR-011 | Document retention policies | EP-016 |
| DR-012 | Soft delete for patients | EP-016 |
| DR-013 | Daily automated backups | EP-016 |
| DR-014 | 24-hour point-in-time recovery | EP-016 |
| DR-015 | Encrypted backup storage | EP-016 |
| DR-016 | Zero-downtime migrations | EP-016 |
| DR-017 | Schema versioning with rollback | EP-016 |
| DR-018 | PHI masking in non-prod | EP-016 |
| DR-019 | Database-level PHI encryption | EP-016 |

### AI Requirements (AIR)

| AIR-ID | Description | Epic |
|--------|-------------|------|
| AIR-001 | Clinical data extraction with RAG | EP-012 |
| AIR-002 | ICD-10 mapping with citation | EP-012 |
| AIR-003 | CPT mapping with citation | EP-012 |
| AIR-004 | Conversational intake (Tool Calling) | EP-013 |
| AIR-005 | Semantic conflict detection | EP-013 |
| AIR-006 | Source citations for suggestions | EP-013 |
| AIR-Q01 | <2% hallucination rate | EP-013 |
| AIR-Q02 | ≥98% AI-Human Agreement | EP-013 |
| AIR-Q03 | ≤5s p95 latency | EP-013 |
| AIR-Q04 | ≥99% output schema validity | EP-013 |
| AIR-S01 | PII redaction from prompts | EP-013 |
| AIR-S02 | Role-based RAG filtering | EP-013 |
| AIR-S03 | Prompt/response audit (7yr) | EP-013 |
| AIR-S04 | Manual fallback if <80% confidence | EP-013 |
| AIR-O01 | 4,000 token budget | EP-012 |
| AIR-O02 | Circuit breaker (3 fail → 30s) | EP-012 |
| AIR-O03 | Model rollback within 1hr | EP-012 |
| AIR-O04 | Embedding cache | EP-012 |
| AIR-R01 | 512-token chunks, 10% overlap | EP-012 |
| AIR-R02 | Top-5 retrieval, cosine ≥0.75 | EP-012 |
| AIR-R03 | Semantic reranking | EP-012 |
| AIR-R04 | pgvector with HNSW | EP-012 |

### UX Requirements (UXR)

| UXR-ID | Description | Epic |
|--------|-------------|------|
| UXR-001 | Consistent navigation | EP-014 |
| UXR-002 | Visual distinction between portals | EP-001 |
| UXR-003 | Session context preservation | EP-001 |
| UXR-101 | ≤3 clicks for booking | EP-002 |
| UXR-102 | 500ms availability updates | EP-002 |
| UXR-103 | Seamless AI/manual switch | EP-003 |
| UXR-104 | 360-view visual hierarchy | EP-004 |
| UXR-105 | Bulk actions on queue | EP-008 |
| UXR-106 | Keyboard shortcuts for staff | EP-008 |
| UXR-201 | WCAG 2.2 AA compliance | EP-014 |
| UXR-202 | Focus indicators ≥3:1 | EP-014 |
| UXR-203 | Screen reader support | EP-014 |
| UXR-204 | Text alternatives for images | EP-014 |
| UXR-205 | 44x44px touch targets | EP-014 |
| UXR-206 | Keyboard navigation | EP-014 |
| UXR-301 | Responsive layouts | EP-014 |
| UXR-302 | Mobile navigation | EP-014 |
| UXR-303 | Mobile calendar usability | EP-002 |
| UXR-304 | Vertical form stacking | EP-014 |
| UXR-401 | Design tokens | EP-015 |
| UXR-402 | Light/dark mode | EP-015 |
| UXR-403 | Healthcare color palette | EP-015 |
| UXR-404 | AI vs human-verified visual | EP-005 |
| UXR-501 | Loading feedback <200ms | EP-004 |
| UXR-502 | Success toast notifications | EP-015 |
| UXR-503 | Screen transitions | EP-015 |
| UXR-504 | Inline form validation | EP-015 |
| UXR-505 | prefers-reduced-motion | EP-015 |
| UXR-601 | User-friendly errors | EP-015 |
| UXR-602 | AI graceful degradation | EP-003 |
| UXR-603 | Form data persistence | EP-015 |
| UXR-604 | Session timeout warning | EP-001 |
| UXR-605 | Offline state handling | EP-015 |
