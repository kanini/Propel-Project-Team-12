# Architecture Design

## Project Overview

The Unified Patient Access & Clinical Intelligence Platform is a healthcare technology solution that bridges patient scheduling and clinical data management. The platform serves three primary user groups: Patients (self-service registration, appointment booking, clinical document upload, health dashboard access), Staff (walk-in booking, queue management, arrival marking, clinical data verification), and Admin (user management, role assignment, audit access). Key capabilities include AI-assisted patient intake, intelligent appointment scheduling with dynamic slot swap, clinical document processing with automated data extraction, and a Trust-First 360-Degree Patient View that reduces clinical prep time from 20+ minutes to under 2 minutes.

## Architecture Goals

- AG-001: Enable 99.9% platform uptime with robust session management and graceful degradation
- AG-002: Achieve 100% HIPAA compliance for data handling with encryption at rest and in transit
- AG-003: Reduce clinical preparation time from 20+ minutes to under 2 minutes through automated data aggregation
- AG-004: Maintain AI-Human Agreement Rate above 98% for clinical data suggestions via Trust-First verification
- AG-005: Support free-tier hosting deployment without paid cloud infrastructure
- AG-006: Deliver responsive, accessible user interfaces across all portal types
- AG-007: Enable seamless integration with external calendar and notification services
- AG-008: Provide complete audit trail for all PHI access and modifications
- AG-009: Reduce no-show rates through predictive risk assessment, proactive engagement, and dynamic slot management

## Non-Functional Requirements

- NFR-001: System MUST respond to API requests within 500ms for 95th percentile under normal load conditions
- NFR-002: System MUST retrieve and display 360-Degree Patient View within 2 seconds of request initiation
- NFR-003: System MUST encrypt all Protected Health Information (PHI) at rest using AES-256 encryption algorithm
- NFR-004: System MUST encrypt all data in transit using TLS 1.2 or higher protocol
- NFR-005: System MUST automatically terminate user sessions after 15 minutes of inactivity
- NFR-006: System MUST enforce role-based access control (RBAC) restricting functionality to Patient, Staff, and Admin roles
- NFR-007: System MUST maintain immutable audit logs capturing user ID, timestamp, action type, and resource accessed for all PHI interactions
- NFR-008: System MUST achieve 99.9% uptime measured monthly, excluding scheduled maintenance windows
- NFR-009: System MUST support a minimum of 100 concurrent users across all portals without performance degradation
- NFR-010: System MUST process uploaded clinical documents within 60 seconds for documents up to 10MB
- NFR-011: System MUST maintain API error rate below 0.1% for non-AI endpoints
- NFR-012: System MUST maintain code test coverage above 80% for business logic components
- NFR-013: System MUST complete AI inference operations (code mapping, extraction) within 5 seconds per document page
- NFR-014: System MUST implement minimum necessary access principle, restricting data access based on role and clinical need
- NFR-015: System MUST gracefully degrade when AI services are unavailable, maintaining core booking and data entry functionality
- NFR-016: System MUST calculate no-show risk score within 100ms of appointment booking using historical patient data and appointment factors
- NFR-017: System MUST deliver appointment reminder notifications within 30 seconds of scheduled trigger time with retry on failure
- NFR-018: System MUST provide real-time upload progress indication for clinical documents using chunked upload with progress streaming

## Data Requirements

- DR-001: System MUST store user records with email as unique identifier, including name, date of birth, contact information, role, and credential hash
- DR-002: System MUST store appointment records with unique identifier, patient reference, provider reference, scheduled datetime, status, visit reason, and walk-in flag
- DR-003: System MUST store clinical documents with unique identifier, patient reference, file metadata, processing status, and extracted data reference
- DR-004: System MUST store extracted clinical data with unique identifier, source document reference, data type (Vital, Medication, Allergy, Diagnosis, LabResult), value, confidence score, and verification status
- DR-005: System MUST store audit log entries with immutable unique identifier, user reference, timestamp, action type, resource type, resource identifier, and action details
- DR-006: System MUST enforce referential integrity between patient records and all related entities (appointments, documents, intake records, waitlist entries)
- DR-007: System MUST retain audit logs for a minimum of 7 years in compliance with HIPAA retention requirements
- DR-008: System MUST support database backups with point-in-time recovery capability within 15-minute granularity
- DR-009: System MUST support zero-downtime schema migrations using versioned migration scripts
- DR-010: System MUST store vector embeddings for clinical data and medical codes using pgvector extension with 1536-dimensional vectors
- DR-011: System MUST store waitlist entries with patient reference, provider reference, preferred time ranges, notification preferences, and priority timestamp
- DR-012: System MUST store intake records with appointment reference, intake mode (AI/Manual), structured health data (history, medications, allergies, concerns), and completion status
- DR-013: System MUST store medical code suggestions (ICD-10, CPT) with extracted data reference, code value, description, confidence score, and verification status
- DR-014: System MUST store notification records with recipient reference, channel type (SMS, Email), status, scheduled time, and delivery confirmation
- DR-015: System MUST store insurance provider reference data with provider name, accepted ID patterns, coverage type, and validation rules for insurance pre-check (justified by FR-021)
- DR-016: System MUST store patient no-show history with appointment reference, no-show count, confirmation response rate, and calculated risk score (justified by FR-023)

### Domain Entities

- **User**: Represents any authenticated system user (Patient, Staff, Admin) with unique email identifier, encrypted credentials, role assignment, and account status. Related to AuditLog (author), Appointment (patient context).

- **Patient**: Extended user profile for healthcare context with demographics, emergency contact, insurance information, no-show history reference, and confirmation response metrics. Related to Appointments, ClinicalDocuments, IntakeRecords, WaitlistEntries, PatientProfile, NoShowHistory.

- **Appointment**: Scheduled or walk-in patient visit with status lifecycle (Scheduled → Confirmed → Arrived → Completed/Cancelled/No-Show), visit reason, optional preferred slot swap reference, confirmation response received flag, calculated no-show risk score, and configurable cancellation notice hours. Related to Patient, Provider, IntakeRecord, Notifications, NoShowHistory.

- **Provider**: Healthcare provider reference (no login in Phase 1) with name, specialty, and availability schedule. Related to Appointments, TimeSlots.

- **TimeSlot**: Discrete availability window for providers with start/end times and booking status. Related to Provider, Appointment.

- **WaitlistEntry**: Patient preference record for unavailable slots with preferred date ranges, notification preferences, and priority ordering. Related to Patient, Provider.

- **ClinicalDocument**: Uploaded patient document (PDF) with processing status (Uploaded → Processing → Completed/Failed), file metadata, and reference to extracted data. Related to Patient, ExtractedClinicalData.

- **ExtractedClinicalData**: AI-extracted data point from clinical documents with data type classification, value, confidence score, verification status, and source reference. Related to ClinicalDocument, Patient, MedicalCode.

- **PatientProfile**: Aggregated 360-Degree view with de-duplicated conditions, medications, allergies, vital trends, and identified conflicts. Related to Patient.

- **IntakeRecord**: Pre-visit patient intake data with mode indicator (AI Conversational/Manual Form), structured health information, insurance validation status, validated insurance record reference, and completion flag. Related to Appointment, Patient, InsuranceRecord.

- **MedicalCode**: ICD-10 or CPT code suggestion with code system identifier, value, description, confidence score, and verification status. Related to ExtractedClinicalData.

- **AuditLog**: Immutable audit trail entry with action classification, resource identification, detailed action data, and timestamp. Related to User.

- **Notification**: Scheduled or delivered notification with channel type, template reference, status, and delivery tracking. Related to Patient, Appointment.

- **InsuranceRecord**: Reference data for insurance provider validation with provider name, accepted ID patterns (regex), coverage type classification, and active status. Used for FR-021 insurance pre-check validation against dummy records.

- **NoShowHistory**: Patient-level aggregated no-show metrics with total appointment count, no-show count, confirmation response rate, average lead time for no-shows, and last calculated risk score. Related to Patient, used for FR-023 risk assessment.

## AI Consideration

**Status:** Applicable

**Rationale:** The spec.md contains 7 `[AI-CANDIDATE]` tags and 3 `[HYBRID]` tags across the following functional requirements: FR-017 (AI conversational intake), FR-023 (no-show risk assessment), FR-028 (clinical document extraction), FR-030 (data aggregation), FR-031 (conflict detection), FR-032 (360-Degree view generation), FR-034 (ICD-10 mapping), FR-035 (CPT mapping), FR-036 (code verification workflow), and FR-019 (AI-manual intake switch). These features require comprehensive AI architecture design.

## AI Requirements

### AI Functional Requirements

- AIR-001: System MUST process natural language patient responses during conversational intake and extract structured health data (symptoms, history, medications, allergies) using NLU pattern (traces to FR-017)
- AIR-002: System MUST extract clinical data (vitals, medical history, medications, allergies, lab results, diagnoses) from uploaded unstructured PDF reports using document intelligence pattern (traces to FR-028)
- AIR-003: System MUST map extracted clinical data to appropriate ICD-10 diagnosis codes using RAG pattern with medical coding knowledge base (traces to FR-034)
- AIR-004: System MUST map extracted clinical data to appropriate CPT procedure codes using RAG pattern with procedure coding knowledge base (traces to FR-035)
- AIR-005: System MUST aggregate extracted data from multiple clinical documents into a de-duplicated, consolidated patient profile using entity resolution pattern (traces to FR-030)
- AIR-006: System MUST detect and highlight critical data conflicts (conflicting medications, inconsistent diagnoses) across patient documents for staff verification (traces to FR-031)
- AIR-007: System MUST generate 360-Degree Patient View displaying unified patient health summary from aggregated clinical data (traces to FR-032)
- AIR-008: System MUST provide confidence scores (0-100%) for all AI-suggested clinical data and medical codes
- AIR-009: System MUST provide source document references (page number, text excerpt) for all AI-extracted data points

### AI Quality Requirements

- AIR-Q01: System MUST maintain AI-Human Agreement Rate above 98% for suggested clinical data and medical codes measured against staff verification decisions
- AIR-Q02: System MUST complete AI inference operations within 5 seconds per document page for 95th percentile requests
- AIR-Q03: System MUST achieve output schema validity rate above 99% for structured AI responses
- AIR-Q04: System MUST maintain hallucination rate below 2% on clinical document extraction evaluation set
- AIR-Q05: System MUST achieve extraction recall above 95% for critical clinical data elements (medications, allergies)

### AI Safety Requirements

- AIR-S01: System MUST redact or encrypt PII/PHI from prompts before external model invocation when using cloud AI services
- AIR-S02: System MUST log all AI prompts and responses for audit purposes with configurable retention period (minimum 1 year)
- AIR-S03: System MUST implement fallback to manual form entry when AI conversational intake confidence drops below 70% threshold
- AIR-S04: System MUST require mandatory staff verification for all AI-extracted clinical data before clinical use
- AIR-S05: System MUST clearly distinguish AI-suggested data from staff-verified data in all user interfaces

### AI Operational Requirements

- AIR-O01: System MUST enforce token budget of 4000 tokens per AI request to manage costs and latency
- AIR-O02: System MUST implement circuit breaker pattern for AI provider failures with 30-second timeout and exponential backoff
- AIR-O03: System MUST support AI model version rollback within 15 minutes of deployment
- AIR-O04: System MUST queue document processing requests to handle burst uploads without degrading real-time operations
- AIR-O05: System MUST implement rate limiting for AI services to prevent cost overruns (configurable daily/monthly limits)

### RAG Pipeline Requirements

- AIR-R01: System MUST chunk medical coding documents into 512-token segments with 64-token (12.5%) overlap for knowledge base indexing
- AIR-R02: System MUST retrieve top-5 chunks with cosine similarity score above 0.75 threshold for code mapping queries
- AIR-R03: System MUST implement hybrid retrieval combining semantic similarity with keyword matching for medical terminology
- AIR-R04: System MUST maintain separate vector indices for ICD-10 codes, CPT codes, and clinical terminology

### AI Architecture Pattern

**Selected Pattern:** Hybrid

**Rationale:** The platform requires multiple distinct AI patterns working together:
1. **RAG Pattern** for ICD-10 and CPT code mapping - grounding code suggestions in official medical coding standards ensures accuracy and provides citation capability
2. **Document Intelligence Pattern** for PDF clinical document extraction - specialized processing for medical document layouts, tables, and forms
3. **Conversational AI Pattern** for patient intake - multi-turn dialogue with NLU for natural health data collection
4. **Rule-Based Pattern** for no-show risk assessment (FR-023) - deterministic scoring using historical data factors

The hybrid approach enables optimal pattern selection per use case while maintaining the Trust-First principle where AI assists and staff decides.

## Architecture and Design Decisions

- **AD-001 (Three-Layer Architecture)**: Backend implements a traditional three-layer architecture separating Presentation (Web), Business Logic, and Data Access concerns. This straightforward layered approach provides clear boundaries, simplified dependency flow, easy onboarding, and maintainable codebase structure while supporting HIPAA compliance through centralized data access control and comprehensive audit logging.

- **AD-002 (API-First Design)**: OpenAPI specification defines all API contracts before implementation, enabling parallel frontend/backend development, automatic client generation, and comprehensive API documentation.

- **AD-003 (Event-Driven Asynchronous Processing)**: Document processing, notification delivery, and slot swap detection use asynchronous event patterns to prevent blocking user interactions and enable horizontal scaling of background workers.

- **AD-004 (Trust-First Verification Workflow)**: All AI-generated clinical data tagged with confidence scores and verification status, requiring mandatory staff verification before clinical use. Visual distinction between AI-suggested (yellow badge) and staff-verified (green badge) data.

- **AD-005 (Zero-PHI Caching Strategy)**: Redis cache stores only session tokens and non-PHI data. All PHI remains in encrypted PostgreSQL database. Session tokens encrypted with short TTL (15 minutes).

- **AD-006 (Progressive AI Degradation)**: All AI features have deterministic fallbacks - conversational intake falls back to manual forms, code suggestions show "manual entry required" when AI unavailable, maintaining full system functionality.

- **AD-007 (Immutable Audit Trail)**: Append-only audit log table with database triggers preventing UPDATE/DELETE operations. Centralized audit service captures all PHI access patterns.

## Technology Stack

| Layer | Technology | Version | Justification (NFR/DR/AIR) |
|-------|------------|---------|----------------------------|
| Frontend | React + TypeScript + Redux Toolkit + Tailwind CSS | React 18.x, TypeScript 5.x, Redux Toolkit 2.x | NFR-001, NFR-006 (responsive, type-safe UI with RBAC support, centralized state management) |
| Mobile | N/A | - | Out of scope for Phase 1 |
| Backend | .NET 8 ASP.NET Core Web API | .NET 8.0 | NFR-001, NFR-003, NFR-004, NFR-007 (enterprise security, audit capabilities) |
| Database | PostgreSQL with pgvector | PostgreSQL 16, pgvector 0.5+ | DR-001 through DR-014, DR-010 (relational integrity, vector storage) |
| AI/ML | Azure OpenAI Service, Azure AI Document Intelligence | GPT-4o, Document Intelligence 4.0 | AIR-001 through AIR-R04 (HIPAA BAA available, document extraction) |
| Caching | Upstash Redis | Redis 7.x compatible | NFR-005 (session management with 15-minute timeout) |
| Testing | xUnit, Playwright, Jest | Latest stable | NFR-012 (80% coverage target), NFR-011 (error rate validation) |
| Infrastructure | Vercel (Frontend), Railway/Render (Backend), Supabase (Database) | Latest | NFR-008, TR-006 (free-tier compliant with 99.9% SLA) |
| Security | ASP.NET Core Identity, JWT, BCrypt | Built-in | NFR-003, NFR-004, NFR-006, NFR-014 (encryption, RBAC, minimum access) |
| Deployment | GitHub Actions | Latest | NFR-008 (automated deployment, direct platform deployment) |
| Monitoring | Application Insights, Seq | Latest | NFR-007, NFR-011 (audit logging, error tracking) |
| PDF Generation | QuestPDF | Latest stable | TR-019, FR-012 (appointment confirmation PDF generation) |
| Real-time | Pusher | Pusher Channels SDK | TR-022, NFR-018 (upload progress, real-time notifications, managed WebSocket service) |
| Documentation | OpenAPI/Swagger, Markdown | OpenAPI 3.0 | AD-002 (API-first design) |

### Alternative Technology Options

- **Frontend Alternative (Angular)**: Considered Angular 17+ for enterprise features and built-in RxJS. Not selected due to larger bundle size impacting free-tier hosting limits and team familiarity with React ecosystem.

- **Backend Alternative (Node.js/Express)**: Considered Node.js for JavaScript full-stack consistency. Not selected as .NET 8 provides superior enterprise security features, native Windows deployment, and better HIPAA compliance tooling.

- **Database Alternative (MongoDB)**: Considered MongoDB for document storage flexibility. Not selected as PostgreSQL with pgvector provides both relational integrity for transactional healthcare data and vector similarity for AI use cases in a single system.

- **AI Alternative (OpenAI Direct)**: Considered direct OpenAI API integration. Not selected as Azure OpenAI provides HIPAA Business Associate Agreement (BAA), enterprise security controls, and private endpoint options essential for healthcare compliance.

### AI Component Stack

| Component | Technology | Purpose |
|-----------|------------|---------|
| Model Provider | Azure OpenAI Service (GPT-4o) | LLM inference for code mapping, intake conversation, data aggregation |
| Vector Store | PostgreSQL pgvector | Embedding storage and similarity search for RAG pipeline |
| Document Intelligence | Azure AI Document Intelligence | PDF extraction, table recognition, form processing |
| Embedding Model | Azure OpenAI text-embedding-3-small | 1536-dimensional embeddings for semantic search |
| AI Gateway | Custom ASP.NET Core middleware | Request routing, token budgeting, circuit breaker, audit logging |
| Guardrails | Custom validation layer | Output schema validation, content filtering, confidence thresholds |

### Technology Decision

| Metric (from NFR/DR/AIR) | Azure OpenAI | OpenAI Direct | Anthropic Claude | Rationale |
|--------------------------|--------------|---------------|------------------|-----------|
| HIPAA BAA Available (NFR-003) | 10 | 0 | 5 | Azure provides formal BAA; OpenAI lacks BAA; Anthropic limited |
| Document Extraction (AIR-002) | 10 | 6 | 7 | Azure Document Intelligence specialized for medical docs |
| .NET Integration (TR-002) | 10 | 7 | 7 | Azure SDK native to .NET ecosystem |
| Cost Efficiency (TR-006) | 7 | 8 | 6 | All have usage costs; Azure has enterprise pricing |
| Latency (AIR-Q02) | 8 | 9 | 8 | OpenAI slightly faster; all meet 5s requirement |
| **Weighted Total** | **45** | **30** | **33** | **Azure OpenAI selected** |

## Technical Requirements

- TR-001: System MUST implement frontend using React 18+ with TypeScript strict mode, Redux Toolkit for state management, and Tailwind CSS for styling (justified by NFR-001 performance, NFR-006 RBAC UI patterns)
- TR-002: System MUST implement backend using .NET 8 ASP.NET Core Web API with Controller-based routing (justified by NFR-003, NFR-004 security requirements)
- TR-003: System MUST use PostgreSQL 16+ with pgvector extension 0.5+ as primary database (justified by DR-001 through DR-014 data structure requirements, DR-010 vector storage)
- TR-004: System MUST use Upstash Redis for session token storage with 15-minute TTL (justified by NFR-005 session timeout)
- TR-005: System MUST expose RESTful APIs following OpenAPI 3.0 specification with JSON request/response format (justified by AD-002 API-first decision)
- TR-006: System MUST deploy to free-tier hosting platforms (Vercel, Railway, Supabase) without paid cloud infrastructure (justified by constraint in spec.md)
- TR-007: System MUST support Windows Services and IIS deployment for on-premise scenarios (justified by constraint in spec.md)
- TR-008: System MUST integrate with Google Calendar API for appointment synchronization (justified by FR-024)
- TR-009: System MUST integrate with Microsoft Graph API for Outlook Calendar synchronization (justified by FR-025)
- TR-010: System MUST integrate with SMS gateway (Twilio or equivalent with free tier) for appointment reminders (justified by FR-022)
- TR-011: System MUST integrate with email service (SendGrid or equivalent with free tier) for notifications (justified by FR-022)
- TR-012: System MUST use JWT Bearer tokens for API authentication with RS256 signing algorithm (justified by NFR-006 RBAC)
- TR-013: System MUST use BCrypt with cost factor 12 for password hashing (justified by NFR-003 security)
- TR-014: System MUST implement CORS policy restricting origins to configured frontend domains (justified by NFR-014 minimum access)
- TR-015: System MUST use Azure OpenAI Service with HIPAA BAA for all AI inference operations (justified by AIR-S01 PHI protection)
- TR-016: System MUST use Azure AI Document Intelligence for PDF clinical document extraction (justified by AIR-002)
- TR-017: System MUST implement background job processing using Hangfire or equivalent for document processing queue (justified by AIR-O04)
- TR-018: System MUST implement health check endpoints for monitoring and load balancer integration (justified by NFR-008 availability)
- TR-019: System MUST use QuestPDF or equivalent .NET PDF generation library for creating appointment confirmation PDFs (justified by FR-012)
- TR-020: System MUST implement rule-based no-show risk scoring engine using configurable factors: appointment lead time weight, previous no-show history weight, and confirmation response rate weight (justified by FR-023, NFR-016)
- TR-021: System MUST implement insurance pre-check validation service matching patient insurance name and ID against InsuranceRecord reference data using pattern matching (justified by FR-021)
- TR-022: System MUST implement chunked file upload with progress tracking via Pusher Channels for clinical document uploads (justified by FR-027, NFR-018)
- TR-023: System MUST implement notification retry strategy with exponential backoff (max 3 retries) for failed SMS and Email deliveries (justified by NFR-017)

## Technical Constraints & Assumptions

### Technical Constraints

1. **Free-Tier Hosting Limitation**: No paid cloud infrastructure (AWS, Azure hosting) allowed - must use Netlify, Vercel, Railway, or equivalent free tiers. This constrains compute resources, concurrent connections, and storage limits.

2. **No Provider Logins**: Provider-facing actions and provider authentication are out of scope for Phase 1. Providers are reference entities only.

3. **Standalone System**: No direct EHR integration in Phase 1. System operates independently while maintaining integration-ready architecture.

4. **English-Only AI Processing**: AI extraction models trained on English medical terminology. Non-English documents may have reduced accuracy.

5. **PDF-Only Document Support**: Clinical document upload limited to PDF format in Phase 1. Other formats (images, DICOM) require future enhancement.

6. **AI Service Dependency**: AI features require internet connectivity to Azure OpenAI services. No offline AI capability.

### Assumptions

1. **Email Access**: All patients have email access for account verification and appointment confirmations.

2. **Modern Browsers**: Users access the platform via modern browsers (Chrome 90+, Firefox 90+, Safari 14+, Edge 90+) with JavaScript enabled.

3. **Staff Availability**: Staff members are available during business hours to perform clinical data verification within service level expectations.

4. **Document Quality**: Uploaded clinical documents are legible PDFs with reasonable scan quality for AI extraction.

5. **Network Connectivity**: All users have reliable internet connectivity for real-time platform interactions.

6. **Volume Projections**: Initial deployment supports up to 100 concurrent users, 1000 patient records, and 5000 clinical documents.

## Project Structure

```
patient-access-platform/
├── src/
│   ├── frontend/                          # React TypeScript Frontend
│   │   ├── public/
│   │   │   ├── index.html
│   │   │   └── favicon.ico
│   │   ├── src/
│   │   │   ├── api/                       # API client (generated from OpenAPI)
│   │   │   │   ├── client.ts
│   │   │   │   └── types.ts
│   │   │   ├── components/                # Reusable UI components
│   │   │   │   ├── common/
│   │   │   │   │   ├── Button.tsx
│   │   │   │   │   ├── Input.tsx
│   │   │   │   │   ├── Modal.tsx
│   │   │   │   │   ├── Table.tsx
│   │   │   │   │   └── Badge.tsx
│   │   │   │   ├── layout/
│   │   │   │   │   ├── Header.tsx
│   │   │   │   │   ├── Sidebar.tsx
│   │   │   │   │   └── Footer.tsx
│   │   │   │   └── forms/
│   │   │   │       ├── IntakeForm.tsx
│   │   │   │       └── AppointmentForm.tsx
│   │   │   ├── features/                  # Feature-based modules
│   │   │   │   ├── auth/
│   │   │   │   │   ├── components/
│   │   │   │   │   ├── hooks/
│   │   │   │   │   ├── pages/
│   │   │   │   │   │   ├── LoginPage.tsx
│   │   │   │   │   │   └── RegisterPage.tsx
│   │   │   │   │   └── authSlice.ts
│   │   │   │   ├── appointments/
│   │   │   │   │   ├── components/
│   │   │   │   │   │   ├── Calendar.tsx
│   │   │   │   │   │   ├── SlotPicker.tsx
│   │   │   │   │   │   └── AppointmentCard.tsx
│   │   │   │   │   ├── hooks/
│   │   │   │   │   ├── pages/
│   │   │   │   │   │   ├── BookingPage.tsx
│   │   │   │   │   │   └── AppointmentsListPage.tsx
│   │   │   │   │   └── appointmentsSlice.ts
│   │   │   │   ├── intake/
│   │   │   │   │   ├── components/
│   │   │   │   │   │   ├── ConversationalIntake.tsx
│   │   │   │   │   │   └── ManualIntakeForm.tsx
│   │   │   │   │   ├── hooks/
│   │   │   │   │   └── pages/
│   │   │   │   │       └── IntakePage.tsx
│   │   │   │   ├── documents/
│   │   │   │   │   ├── components/
│   │   │   │   │   │   ├── DocumentUploader.tsx
│   │   │   │   │   │   └── ProcessingStatus.tsx
│   │   │   │   │   ├── hooks/
│   │   │   │   │   └── pages/
│   │   │   │   │       └── DocumentsPage.tsx
│   │   │   │   ├── patient-view/
│   │   │   │   │   ├── components/
│   │   │   │   │   │   ├── PatientProfile360.tsx
│   │   │   │   │   │   ├── MedicationsList.tsx
│   │   │   │   │   │   ├── AllergiesList.tsx
│   │   │   │   │   │   ├── VitalsTrend.tsx
│   │   │   │   │   │   └── ConflictAlert.tsx
│   │   │   │   │   └── pages/
│   │   │   │   │       └── PatientDashboard.tsx
│   │   │   │   ├── staff/
│   │   │   │   │   ├── components/
│   │   │   │   │   │   ├── QueueManager.tsx
│   │   │   │   │   │   ├── PatientSearch.tsx
│   │   │   │   │   │   └── VerificationPanel.tsx
│   │   │   │   │   └── pages/
│   │   │   │   │       ├── StaffDashboard.tsx
│   │   │   │   │       ├── WalkInBooking.tsx
│   │   │   │   │       └── DataVerification.tsx
│   │   │   │   └── admin/
│   │   │   │       ├── components/
│   │   │   │       │   └── UserManagement.tsx
│   │   │   │       └── pages/
│   │   │   │           ├── AdminDashboard.tsx
│   │   │   │           └── AuditLogs.tsx
│   │   │   ├── hooks/                     # Shared custom hooks
│   │   │   │   ├── useAuth.ts
│   │   │   │   ├── useApi.ts
│   │   │   │   └── usePusher.ts
│   │   │   ├── store/                     # Redux store configuration
│   │   │   │   ├── index.ts               # Store configuration
│   │   │   │   ├── rootReducer.ts         # Combined reducers
│   │   │   │   └── middleware.ts          # Custom middleware
│   │   │   ├── contexts/                  # React contexts
│   │   │   │   ├── AuthContext.tsx
│   │   │   │   └── NotificationContext.tsx
│   │   │   ├── utils/                     # Utility functions
│   │   │   │   ├── validators.ts
│   │   │   │   ├── formatters.ts
│   │   │   │   └── constants.ts
│   │   │   ├── types/                     # TypeScript type definitions
│   │   │   │   └── index.ts
│   │   │   ├── styles/                    # Global styles
│   │   │   │   └── globals.css
│   │   │   ├── App.tsx
│   │   │   ├── main.tsx
│   │   │   └── router.tsx
│   │   ├── package.json
│   │   ├── tsconfig.json
│   │   ├── tailwind.config.js
│   │   ├── vite.config.ts
│   │   └── .env.example
│   │
│   └── backend/                           # .NET 8 ASP.NET Core Backend
│       ├── PatientAccess.Web/             # Presentation Layer (API, Controllers)
│       │   ├── Controllers/
│       │   │   ├── AuthController.cs
│       │   │   ├── AppointmentsController.cs
│       │   │   ├── PatientsController.cs
│       │   │   ├── DocumentsController.cs
│       │   │   ├── IntakeController.cs
│       │   │   ├── ClinicalDataController.cs
│       │   │   ├── StaffController.cs
│       │   │   ├── AdminController.cs
│       │   │   └── HealthController.cs
│       │   ├── Middleware/
│       │   │   ├── AuditLoggingMiddleware.cs
│       │   │   ├── ExceptionHandlingMiddleware.cs
│       │   │   └── RequestTimingMiddleware.cs
│       │   ├── Filters/
│       │   │   ├── ValidationFilter.cs
│       │   │   └── AuditActionFilter.cs
│       │   ├── RealTime/
│       │   │   └── PusherService.cs
│       │   ├── Program.cs
│       │   ├── appsettings.json
│       │   ├── appsettings.Development.json
│       │   └── PatientAccess.Web.csproj
│       │
│       ├── PatientAccess.Business/        # Business Logic Layer (Services, Models)
│       │   ├── Services/
│       │   │   ├── AuthService.cs
│       │   │   ├── AppointmentService.cs
│       │   │   ├── PatientService.cs
│       │   │   ├── IntakeService.cs
│       │   │   ├── DocumentService.cs
│       │   │   ├── ClinicalDataService.cs
│       │   │   ├── AdminService.cs
│       │   │   ├── TokenService.cs
│       │   │   ├── EmailService.cs
│       │   │   ├── SmsService.cs
│       │   │   ├── CalendarService.cs
│       │   │   ├── PdfService.cs
│       │   │   ├── NoShowRiskService.cs
│       │   │   └── InsuranceValidationService.cs
│       │   ├── Interfaces/
│       │   │   ├── IAuthService.cs
│       │   │   ├── IAppointmentService.cs
│       │   │   ├── IPatientService.cs
│       │   │   ├── IIntakeService.cs
│       │   │   ├── IDocumentService.cs
│       │   │   ├── IClinicalDataService.cs
│       │   │   ├── IAdminService.cs
│       │   │   ├── ITokenService.cs
│       │   │   ├── IEmailService.cs
│       │   │   ├── ISmsService.cs
│       │   │   ├── ICalendarService.cs
│       │   │   ├── IPdfService.cs
│       │   │   ├── INoShowRiskService.cs
│       │   │   └── IInsuranceValidationService.cs
│       │   ├── Models/
│       │   │   ├── Entities/
│       │   │   │   ├── User.cs
│       │   │   │   ├── Patient.cs
│       │   │   │   ├── Provider.cs
│       │   │   │   ├── Appointment.cs
│       │   │   │   ├── TimeSlot.cs
│       │   │   │   ├── WaitlistEntry.cs
│       │   │   │   ├── ClinicalDocument.cs
│       │   │   │   ├── ExtractedClinicalData.cs
│       │   │   │   ├── PatientProfile.cs
│       │   │   │   ├── IntakeRecord.cs
│       │   │   │   ├── MedicalCode.cs
│       │   │   │   ├── AuditLog.cs
│       │   │   │   ├── Notification.cs
│       │   │   │   ├── InsuranceRecord.cs
│       │   │   │   └── NoShowHistory.cs
│       │   │   ├── Enums/
│       │   │   │   ├── UserRole.cs
│       │   │   │   ├── AppointmentStatus.cs
│       │   │   │   ├── DocumentProcessingStatus.cs
│       │   │   │   ├── VerificationStatus.cs
│       │   │   │   └── NotificationChannel.cs
│       │   │   └── ValueObjects/
│       │   │       ├── Email.cs
│       │   │       ├── PhoneNumber.cs
│       │   │       └── ConfidenceScore.cs
│       │   ├── DTOs/
│       │   │   ├── AppointmentDto.cs
│       │   │   ├── PatientDto.cs
│       │   │   ├── IntakeDto.cs
│       │   │   └── ClinicalDataDto.cs
│       │   ├── Validators/
│       │   │   ├── BookAppointmentValidator.cs
│       │   │   └── RegisterValidator.cs
│       │   ├── Events/
│       │   │   ├── AppointmentBookedEvent.cs
│       │   │   ├── DocumentUploadedEvent.cs
│       │   │   └── SlotAvailableEvent.cs
│       │   └── PatientAccess.Business.csproj
│       │
│       ├── PatientAccess.Data/            # Data Access Layer (Repositories, DbContext)
│       │   ├── Context/
│       │   │   └── PatientAccessDbContext.cs
│       │   ├── Configurations/
│       │   │   ├── UserConfiguration.cs
│       │   │   ├── AppointmentConfiguration.cs
│       │   │   └── AuditLogConfiguration.cs
│       │   ├── Repositories/
│       │   │   ├── Interfaces/
│       │   │   │   ├── IRepository.cs
│       │   │   │   ├── IUserRepository.cs
│       │   │   │   ├── IPatientRepository.cs
│       │   │   │   ├── IAppointmentRepository.cs
│       │   │   │   ├── IDocumentRepository.cs
│       │   │   │   ├── IClinicalDataRepository.cs
│       │   │   │   ├── IIntakeRepository.cs
│       │   │   │   ├── INotificationRepository.cs
│       │   │   │   ├── IAuditLogRepository.cs
│       │   │   │   └── IUnitOfWork.cs
│       │   │   ├── BaseRepository.cs
│       │   │   ├── UserRepository.cs
│       │   │   ├── PatientRepository.cs
│       │   │   ├── AppointmentRepository.cs
│       │   │   ├── DocumentRepository.cs
│       │   │   ├── ClinicalDataRepository.cs
│       │   │   ├── IntakeRepository.cs
│       │   │   ├── NotificationRepository.cs
│       │   │   ├── AuditLogRepository.cs
│       │   │   └── UnitOfWork.cs
│       │   ├── Migrations/
│       │   ├── Seeders/
│       │   │   ├── InsuranceDataSeeder.cs
│       │   │   └── ProviderDataSeeder.cs
│       │   ├── ExternalServices/
│       │   │   ├── GoogleCalendarClient.cs
│       │   │   ├── OutlookCalendarClient.cs
│       │   │   ├── SendGridClient.cs
│       │   │   ├── TwilioClient.cs
│       │   │   └── PusherClient.cs
│       │   ├── AI/
│       │   │   ├── AzureOpenAIClient.cs
│       │   │   ├── DocumentIntelligenceClient.cs
│       │   │   ├── EmbeddingService.cs
│       │   │   ├── ConversationalIntakeService.cs
│       │   │   ├── MedicalCodeMappingService.cs
│       │   │   └── DataAggregationService.cs
│       │   ├── Caching/
│       │   │   └── RedisSessionService.cs
│       │   ├── BackgroundJobs/
│       │   │   ├── DocumentProcessingJob.cs
│       │   │   ├── NotificationJob.cs
│       │   │   └── SlotSwapJob.cs
│       │   └── PatientAccess.Data.csproj
│       │
│       └── PatientAccess.Tests/           # Test Projects
│           ├── Unit/
│           │   ├── Business/
│           │   └── Data/
│           ├── Integration/
│           │   └── Web/
│           └── PatientAccess.Tests.csproj
│
├── tests/
│   └── e2e/                               # Playwright E2E Tests
│       ├── tests/
│       │   ├── auth.spec.ts
│       │   ├── booking.spec.ts
│       │   ├── intake.spec.ts
│       │   └── clinical-data.spec.ts
│       ├── pages/
│       │   ├── LoginPage.ts
│       │   ├── BookingPage.ts
│       │   └── DashboardPage.ts
│       ├── fixtures/
│       ├── playwright.config.ts
│       └── package.json
│
├── docs/
│   ├── api/
│   │   └── openapi.yaml                   # OpenAPI 3.0 Specification
│   ├── architecture/
│   │   └── diagrams/
│   └── runbooks/
│
├── scripts/
│   ├── setup-dev.ps1
│   ├── run-migrations.ps1
│   └── seed-data.ps1
│
├── .github/
│   └── workflows/
│       ├── ci.yml
│       ├── cd-staging.yml
│       └── cd-production.yml
│
├── PatientAccess.sln
├── README.md
└── .gitignore
```

## Key Dependencies

### Frontend (package.json)

```json
{
  "name": "patient-access-frontend",
  "version": "1.0.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview",
    "lint": "eslint . --ext ts,tsx --report-unused-disable-directives --max-warnings 0",
    "test": "vitest",
    "test:e2e": "playwright test"
  },
  "dependencies": {
    "react": "^18.3.1",
    "react-dom": "^18.3.1",
    "react-router-dom": "^6.23.0",
    "@reduxjs/toolkit": "^2.2.5",
    "react-redux": "^9.1.2",
    "@tanstack/react-query": "^5.40.0",
    "axios": "^1.7.2",
    "pusher-js": "^8.4.0",
    "react-hook-form": "^7.51.5",
    "@hookform/resolvers": "^3.4.2",
    "zod": "^3.23.8",
    "date-fns": "^3.6.0",
    "react-big-calendar": "^1.12.0",
    "recharts": "^2.12.7",
    "clsx": "^2.1.1",
    "tailwind-merge": "^2.3.0",
    "@radix-ui/react-dialog": "^1.0.5",
    "@radix-ui/react-dropdown-menu": "^2.0.6",
    "@radix-ui/react-toast": "^1.1.5",
    "@radix-ui/react-tabs": "^1.0.4",
    "lucide-react": "^0.379.0"
  },
  "devDependencies": {
    "@types/react": "^18.3.3",
    "@types/react-dom": "^18.3.0",
    "@types/react-big-calendar": "^1.8.9",
    "@typescript-eslint/eslint-plugin": "^7.11.0",
    "@typescript-eslint/parser": "^7.11.0",
    "@vitejs/plugin-react": "^4.3.0",
    "autoprefixer": "^10.4.19",
    "eslint": "^8.57.0",
    "eslint-plugin-react-hooks": "^4.6.2",
    "eslint-plugin-react-refresh": "^0.4.7",
    "postcss": "^8.4.38",
    "tailwindcss": "^3.4.3",
    "typescript": "^5.4.5",
    "vite": "^5.2.12",
    "vitest": "^1.6.0",
    "@playwright/test": "^1.44.1"
  }
}
```

### Backend (.csproj References)

#### PatientAccess.Web.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- ASP.NET Core -->
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.6" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.6" />
    
    <!-- API Documentation -->
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.6" />
    
    <!-- Real-time Communication -->
    <PackageReference Include="PusherServer" Version="5.0.0" />
    
    <!-- Health Checks -->
    <PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.1" />
    <PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.0.1" />
    
    <!-- Logging -->
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.1" />
    <PackageReference Include="Serilog.Sinks.Seq" Version="7.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\PatientAccess.Business\PatientAccess.Business.csproj" />
    <ProjectReference Include="..\PatientAccess.Data\PatientAccess.Data.csproj" />
  </ItemGroup>
</Project>
```

#### PatientAccess.Business.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Validation -->
    <PackageReference Include="FluentValidation" Version="11.9.1" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.1" />
    
    <!-- Mapping -->
    <PackageReference Include="AutoMapper" Version="13.0.1" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
  </ItemGroup>
</Project>
```

#### PatientAccess.Data.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Entity Framework Core with PostgreSQL -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.6" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.4" />
    <PackageReference Include="Pgvector.EntityFrameworkCore" Version="0.2.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.6">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    
    <!-- Redis Caching -->
    <PackageReference Include="StackExchange.Redis" Version="2.7.33" />
    <PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.6" />
    
    <!-- Azure AI Services -->
    <PackageReference Include="Azure.AI.OpenAI" Version="1.0.0-beta.17" />
    <PackageReference Include="Azure.AI.FormRecognizer" Version="4.1.0" />
    
    <!-- Background Jobs -->
    <PackageReference Include="Hangfire.Core" Version="1.8.14" />
    <PackageReference Include="Hangfire.AspNetCore" Version="1.8.14" />
    <PackageReference Include="Hangfire.PostgreSql" Version="1.20.9" />
    
    <!-- PDF Generation -->
    <PackageReference Include="QuestPDF" Version="2024.6.0" />
    
    <!-- Email Service -->
    <PackageReference Include="SendGrid" Version="9.29.3" />
    
    <!-- SMS Service -->
    <PackageReference Include="Twilio" Version="7.1.0" />
    
    <!-- Calendar Integration -->
    <PackageReference Include="Google.Apis.Calendar.v3" Version="1.68.0.3375" />
    <PackageReference Include="Microsoft.Graph" Version="5.56.0" />
    
    <!-- Security -->
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.6.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\PatientAccess.Business\PatientAccess.Business.csproj" />
  </ItemGroup>
</Project>
```

#### PatientAccess.Tests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <!-- Testing Framework -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.10.0" />
    <PackageReference Include="xunit" Version="2.8.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    
    <!-- Mocking -->
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="NSubstitute" Version="5.1.0" />
    
    <!-- Assertions -->
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    
    <!-- Integration Testing -->
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.6" />
    <PackageReference Include="Testcontainers.PostgreSql" Version="3.8.0" />
    <PackageReference Include="Testcontainers.Redis" Version="3.8.0" />
    
    <!-- Code Coverage -->
    <PackageReference Include="coverlet.collector" Version="6.0.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\PatientAccess.Web\PatientAccess.Web.csproj" />
  </ItemGroup>
</Project>
```

## Development Workflow

1. **Environment Setup**: Configure local development environment with .NET 8 SDK, Node.js 20+, PostgreSQL with pgvector, and Upstash Redis connection. Initialize project structure following three-layer architecture.

2. **API Contract Development**: Define comprehensive OpenAPI 3.0 specification for all endpoints including authentication, appointment booking, document upload, and clinical data APIs. Generate TypeScript client types for frontend consumption.

3. **Core Infrastructure Implementation**: Build authentication middleware with JWT validation, audit logging infrastructure with append-only persistence, and HIPAA-compliant encryption services. Implement RBAC authorization policies.

4. **Feature Development Cycles**: Implement features in vertical slices ordered by dependency (Authentication → Patient Registration → Appointment Booking → Document Upload → AI Processing → 360-View). Each slice includes API, service logic, data access, and frontend components.

5. **AI Pipeline Integration**: Configure Azure OpenAI and Document Intelligence connections with circuit breaker patterns. Implement RAG pipeline for medical code lookup. Build document processing queue with status tracking.

6. **Testing Pyramid Execution**: Unit tests for business logic (xUnit), integration tests for API endpoints, E2E tests for critical user journeys (Playwright). Target 80% code coverage for business logic.

7. **Security Validation**: Execute OWASP Top 10 security review, penetration testing for authentication flows, and HIPAA compliance audit checklist verification.

8. **Deployment Pipeline**: Configure GitHub Actions for CI/CD with build, test, and deploy stages. Set up staging environment mirroring production. Deploy frontend to Vercel, backend to Railway, database to Supabase.
