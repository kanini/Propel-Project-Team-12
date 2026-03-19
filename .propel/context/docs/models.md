---
title: Design Modelling
project: Unified Patient Access & Clinical Intelligence Platform
version: 1.0
created: 2026-03-19
updated: 2026-03-19
status: Active
---

# Design Modelling

## UML Models Overview

This document provides comprehensive Unified Modeling Language (UML) visual representations of the Unified Patient Access & Clinical Intelligence Platform architecture. The diagrams translate requirements from [spec.md](.propel/context/docs/spec.md) and architectural decisions from [design.md](.propel/context/docs/design.md) into structured visual models that facilitate stakeholder communication, guide implementation, and serve as living documentation.

**Purpose:**
- **System Context Diagram**: Illustrates the system boundary, external actors (patients, staff, admins), and third-party integrations (calendar APIs, email/SMS gateways)
- **Component Architecture Diagram**: Details internal module structure following layered backend and vertical slice frontend patterns
- **Deployment Architecture Diagram**: Maps free-tier cloud infrastructure components (Vercel, Render, Supabase) required by FR-035
- **Data Flow Diagram**: Visualizes data movement from patient intake through AI extraction to clinical data consolidation
- **Logical Data Model (ERD)**: Represents core entities from design.md with relationships, cardinality, and key constraints
- **Sequence Diagrams (UC-001 through UC-010)**: Capture dynamic message flows for each use case specified in spec.md

**Navigation Guide:**
- Architectural views precede behavioral diagrams
- Each sequence diagram links to its source use case specification
- PlantUML diagrams are used for context, deployment, and data flow views per UML standards
- Mermaid diagrams are used for component architectures, ERDs, and sequence diagrams for web-native rendering

## Architectural Views

### System Context Diagram

This diagram shows the Unified Patient Access & Clinical Intelligence Platform as a single system with clear boundaries, illustrating its interactions with external users (Patient, Staff, Admin) and third-party services (Calendar APIs, Email/SMS Gateways, AI Model Providers). The system's primary function is to bridge appointment scheduling with AI-powered clinical data management while maintaining HIPAA compliance.

```plantuml
@startuml
!define RECTANGLE class

skinparam backgroundColor #FFFFFF
skinparam shadowing false
skinparam ArrowColor #333333
skinparam ActorBorderColor #333333
skinparam RectangleBorderColor #1168BD
skinparam RectangleBackgroundColor #FFFFFF

title System Context Diagram - Unified Patient Access & Clinical Intelligence Platform

actor "Patient" as patient <<External User>>
actor "Staff" as staff <<External User>>
actor "Admin" as admin <<External User>>

rectangle "Unified Patient Access &\nClinical Intelligence Platform" as system #1168BD {
}

rectangle "Google Calendar API" as google <<External System>>
rectangle "Outlook Calendar API" as outlook <<External System>>
rectangle "Email Service\n(SendGrid/Mailgun)" as email <<External System>>
rectangle "SMS Gateway\n(Twilio)" as sms <<External System>>
rectangle "AI Model Provider\n(OpenAI/Ollama)" as ai <<External System>>
rectangle "Insurance Database\n(Internal Dummy)" as insurance <<External System>>

patient -down-> system : Books appointments\nCompletes intake\nUploads documents
staff -down-> system : Manages walk-ins\nReviews clinical data\nVerifies extractions
admin -down-> system : Manages users\nConfigures system\nAccesses audit logs

system -right-> google : Syncs calendar events
system -right-> outlook : Syncs calendar events
system -down-> email : Sends confirmations\nSends reminders
system -down-> sms : Sends text reminders
system -left-> ai : Conversational intake\nDocument extraction\nCode suggestion
system -down-> insurance : Validates insurance info

note right of system
  Core Functions:
  - Appointment booking with slot swapping
  - AI/Manual patient intake
  - Multi-document clinical aggregation
  - ICD-10/CPT code suggestion
  - Staff-controlled arrival management
  - HIPAA-compliant audit logging
end note

note bottom of system
  Technology Stack:
  - Frontend: React 18 + Next.js 14 + Tailwind CSS
  - Backend: .NET 8 ASP.NET Core Web API
  - Database: PostgreSQL 14+ with pgvector
  - Hosting: Vercel/Render/Supabase (Free Tier)
end note

@enduml
```

### Component Architecture Diagram

This diagram decomposes the system into major components following a hybrid architectural pattern: layered architecture for the backend (API → Business Services → Data Repositories) and vertical slice pattern for the frontend (feature-isolated modules). Each component is annotated with its primary responsibilities and key technologies.

```mermaid
graph TB
    subgraph "Frontend Layer - React 18 + Next.js 14"
        A[Patient Portal<br/>Booking, Intake, Dashboard]
        B[Staff Portal<br/>Walk-In, Arrival, Clinical Review]
        C[Admin Portal<br/>User Management, Configuration]
        D[Shared Components<br/>shadcn/ui, Tailwind CSS]
        E[API Client Layer<br/>TanStack Query, Axios]
    end

    subgraph "Backend Layer - .NET 8 ASP.NET Core"
        F[API Controllers<br/>REST Endpoints, JWT Auth]
        G[Business Services<br/>Booking, Intake, AI Orchestration]
        H[Data Repositories<br/>EF Core, PostgreSQL Access]
        I[Background Jobs<br/>Hangfire, PDF Generation]
        J[AI Gateway Middleware<br/>Token Budget, Circuit Breaker]
    end

    subgraph "Data Layer"
        K[(PostgreSQL 14+<br/>pgvector Extension)]
        L[(Redis Cache<br/>Upstash Free Tier)]
    end

    subgraph "External Integrations"
        M[Calendar Services<br/>Google/Outlook APIs]
        N[Notification Services<br/>Email/SMS Gateways]
        O[AI Model Provider<br/>OpenAI/Ollama]
    end

    A --> E
    B --> E
    C --> E
    E --> F
    F --> G
    G --> H
    G --> J
    H --> K
    F --> L
    I --> K
    I --> N
    J --> O
    G --> M
    D -.supports.-> A
    D -.supports.-> B
    D -.supports.-> C

    style A fill:#E3F2FD
    style B fill:#E8F5E9
    style C fill:#FFF3E0
    style F fill:#BBDEFB
    style G fill:#BBDEFB
    style H fill:#BBDEFB
    style K fill:#CFD8DC
    style L fill:#CFD8DC
```

**Component Responsibilities:**

| Component | Responsibilities | Key Technologies |
|-----------|-----------------|------------------|
| **Patient Portal** | Appointment booking, AI/manual intake, document upload, dashboard | React hooks, TanStack Query, Next.js SSR |
| **Staff Portal** | Walk-in management, arrival tracking, clinical data review, code verification | React Context, optimistic UI updates |
| **Admin Portal** | User account CRUD, role assignment, audit log access, system configuration | Protected routes, RBAC enforcement |
| **API Controllers** | HTTP request routing, JWT validation, input validation, rate limiting | ASP.NET Core Minimal APIs, FluentValidation |
| **Business Services** | Core logic: booking conflicts, intake processing, AI extraction orchestration, audit logging | Dependency injection, async/await |
| **Data Repositories** | Database CRUD operations, query optimization, EF Core entity mapping | Repository pattern, Unit of Work |
| **Background Jobs** | Async PDF generation, email/SMS sending, embedding generation, waitlist processing | Hangfire dashboard, retry policies |
| **AI Gateway Middleware** | Prompt logging (AIR-S03), token budgets (AIR-O01), circuit breakers (AIR-O02), caching (AIR-O04) | Polly resilience, custom middleware |

### Deployment Architecture Diagram

This diagram visualizes the free-tier cloud landing zone architecture spanning three primary zones: Frontend (Vercel), Backend (Render), and Data (Supabase). The architecture emphasizes HIPAA-compliant data flow with TLS encryption, stateless API design for horizontal scalability, and external integrations via secure API gateways.

```plantuml
@startuml
!define AzurePuml https://raw.githubusercontent.com/plantuml-stdlib/Azure-PlantUML/release/2-2/dist
!includeurl AzurePuml/AzureCommon.puml
!includeurl AzurePuml/Compute/AzureFunction.puml
!includeurl AzurePuml/Databases/AzureCosmosDb.puml
!includeurl AzurePuml/Web/AzureWebApp.puml

skinparam backgroundColor #FFFFFF
skinparam RectangleBorderColor #333333
skinparam ArrowColor #1976D2

title Deployment Architecture - Free-Tier Cloud Infrastructure

package "Client Zone" {
    [Web Browser] as browser
    [Mobile Browser] as mobile
}

cloud "Vercel Free Tier CDN" as vercel {
    [Next.js Frontend\n(Patient/Staff/Admin)\nSSR + ISR] as frontend
    note right of frontend
      - 100GB bandwidth/month
      - Serverless functions
      - Automatic HTTPS
    end note
}

cloud "Render Free Tier" as render {
    [.NET 8 API Container\n(Docker)\n500MB RAM, 0.1 CPU] as api
    [Hangfire Background Jobs\n(In-Process)] as jobs
    note right of api
      - Auto-restart on failure
      - Health check monitoring
      - Logs to stdout
    end note
}

cloud "Supabase Free Tier" as supabase {
    database "PostgreSQL 14+\n(pgvector enabled)\n500MB storage" as db
    storage "Blob Storage\n(Encrypted PDFs)\n1GB storage" as blobs
    note right of db
      - 7-day PITR backup
      - Connection pooling
      - Row-level security
    end note
}

cloud "Upstash Redis\nFree Tier" as upstash {
    [Redis Cache\n10K commands/day] as redis
}

cloud "External Services" as external {
    [Google Calendar API] as google
    [Outlook Calendar API] as outlook
    [SendGrid/Mailgun\nEmail API] as email
    [Twilio SMS API] as sms
    [OpenAI/Ollama\nAI Models] as aimodel
}

browser --> frontend : HTTPS (TLS 1.2+)
mobile --> frontend : HTTPS (TLS 1.2+)
frontend --> api : REST API (HTTPS)\nJWT Bearer Token
api --> db : EF Core\n(Encrypted Connection)
api --> redis : Session Cache\n(Encrypted Connection)
api --> blobs : PDF Upload/Download\n(Server-Side Encryption)
jobs --> email : Async Notifications
jobs --> sms : Async Reminders
api --> google : OAuth 2.0
api --> outlook : OAuth 2.0
api --> aimodel : Model Inference\n(Token Budget Enforced)

note bottom of vercel
  **Frontend Deployment**
  - ISR for static pages
  - SSR for dynamic dashboards
  - Automatic invalidation on deploy
end note

note bottom of render
  **Backend Deployment**
  - Docker multi-stage build
  - EF Core migrations on startup
  - Health checks: /health/live, /health/ready
end note

note bottom of supabase
  **Data Layer**
  - pgvector for embeddings (AIR-R02)
  - Immutable audit log table (DR-005)
  - AES-256 at-rest encryption (NFR-007)
end note

@enduml
```

**Infrastructure Components:**

| Component | Service | Specification | Free-Tier Limits | HIPAA Compliance |
|-----------|---------|---------------|------------------|------------------|
| **Frontend** | Vercel | Next.js serverless, CDN edge caching | 100GB bandwidth, unlimited deployments | TLS 1.2+, CSP headers |
| **Backend** | Render | Docker container, 500MB RAM, 0.1 CPU | 750 hours/month, auto-sleep after 15min inactivity | Environment variable secrets, TLS enforced |
| **Database** | Supabase PostgreSQL | 500MB storage, unlimited API requests | 2 projects, 7-day backups | BAA available, encryption at rest/transit |
| **Cache** | Upstash Redis | 10K commands/day, 256MB memory | Shared infrastructure | TLS connections, no PHI stored |
| **Blob Storage** | Supabase Storage | 1GB total, server-side encryption | No egress limits | Encrypted file paths (Decision 8) |

### Data Flow Diagram

This diagram illustrates the end-to-end data pipeline from patient intake/document upload through AI-powered extraction, consolidation, and medical coding. It highlights data sources (user inputs, PDFs), transformation processes (AI extraction, conflict detection), and data stores (PostgreSQL tables, pgvector embeddings).

```plantuml
@startuml
skinparam backgroundColor #FFFFFF
skinparam ArrowColor #1976D2

title Data Flow Diagram - Clinical Data Aggregation Pipeline

rectangle "Data Sources" {
    [Patient Intake Form\n(AI or Manual)] as intake
    [Uploaded Clinical PDFs\n(Lab/Imaging/Referral)] as pdfs
    [Post-Visit Notes\n(Staff Entered)] as postvisitnotes
}

rectangle "AI Processing Layer" {
    process "Conversational AI\nNLU Extraction" as aiintake
    process "PDF Text Extraction\n(PyMuPDF/PDFPlumber)" as pdftext
    process "Named Entity Recognition\n(Medications, Diagnoses, Vitals)" as ner
    process "Embedding Generation\n(text-embedding-3-small)" as embeddings
}

rectangle "Transformation Layer" {
    process "Data Consolidation\n(Dedupe, Merge)" as consolidation
    process "Conflict Detection\n(Rule-Based Comparisons)" as conflicts
    process "Medical Code Suggestion\n(ICD-10/CPT RAG)" as coding
}

database "Data Stores" {
    [IntakeForm Table\n(Structured JSON)] as intaketable
    [ClinicalDocument Table\n(Metadata + FilePath)] as doctable
    [ExtractedData Table\n(Entities + Confidence Scores)] as extractedtable
    [pgvector Embeddings\n(1536-dimension vectors)] as vectortable
    [ConsolidatedPatientView\n(Materialized View)] as consolidatedtable
    [ICD-10/CPT Codes Table\n(Suggested Codes)] as codestable
}

rectangle "Access Layer" {
    [Staff Clinical Review\nUI] as staffui
    [Provider Dashboard\n(Future Phase)] as providerui
}

intake --> aiintake : Patient responses
aiintake --> intaketable : Structured data
pdfs --> pdftext : Binary upload
pdftext --> ner : Extracted text
ner --> extractedtable : Entities with confidence
pdfs --> embeddings : Document chunks
embeddings --> vectortable : Cosine similarity index
postvisitnotes --> extractedtable : Post-visit entities

intaketable --> consolidation
extractedtable --> consolidation
consolidation --> conflicts : Merged data
conflicts --> consolidatedtable : Flagged conflicts
consolidatedtable --> coding : Patient profile
coding --> codestable : Suggested codes

consolidatedtable --> staffui : 360-Degree View
extractedtable --> staffui : Source citations
codestable --> staffui : Code verification
consolidatedtable --> providerui : (Phase 2)

note right of aiintake
  AIR-001: Conversational intake
  Token budget: 4000/session
  Fallback: Manual form
end note

note right of ner
  AIR-002: PDF extraction
  Confidence threshold: 0.7
  Low-confidence → Staff review
end note

note right of conflicts
  FR-024: Explicit conflict detection
  Examples: Medication discrepancies,
  Allergy mismatches, Vital anomalies
end note

note right of coding
  AIR-004/AIR-005: Medical coding
  RAG retrieval: Top-5 similar codes
  Staff acceptance required
end note

@enduml
```

**Data Flow Key Points:**

1. **Intake Flow**: Patient completes AI conversational or manual form → NLU extracts structured JSON → Stored in IntakeForm table
2. **Document Flow**: PDF uploaded → Text extraction → NER identifies entities → Stored in ExtractedData with confidence scores → Embeddings generated for vector search
3. **Consolidation Flow**: IntakeForm + ExtractedData + PostVisitNotes → Deduplication & merging → Conflict detection → Materialized view refresh
4. **Coding Flow**: ConsolidatedPatientView queries pgvector for similar ICD-10/CPT codes → Ranked suggestions stored → Staff reviews and accepts/rejects
5. **Retrieval Flow**: Staff UI fetches ConsolidatedPatientView → Displays source citations → Links to original PDF pages

### Logical Data Model (ERD)

This entity-relationship diagram represents the core database schema derived from design.md Domain Entities section. It shows relationships, cardinality, key attributes, and HIPAA-critical constraints (audit logging, encryption, optimistic concurrency).

```mermaid
erDiagram
    Patient ||--o{ Appointment : books
    Patient ||--o{ ClinicalDocument : uploads
    Patient ||--|| IntakeForm : completes
    Patient ||--o{ AuditLog : "generates logs"
    
    Staff ||--o{ AuditLog : "generates logs"
    Staff ||--o{ ExtractedData : verifies
    
    Admin ||--o{ AuditLog : "generates logs"
    
    Appointment ||--o| PreferredSlotSwapRequest : "may have"
    Appointment ||--o{ Reminder : receives
    Appointment }o--|| AppointmentSlot : "books slot"
    
    ClinicalDocument ||--o{ ExtractedData : "contains extractions"
    
    ConsolidatedPatientView ||--|| Patient : "aggregates data for"

    Patient {
        uuid patientID PK
        string email UK "UNIQUE, NOT NULL"
        string firstName
        string lastName
        date dateOfBirth
        string phone
        string passwordHash "bcrypt hashed"
        enum accountStatus "Active/Inactive"
        timestamp createdAt
        timestamp updatedAt
    }

    Staff {
        uuid staffID PK
        string email UK "UNIQUE, NOT NULL"
        string name
        enum specialization "FrontDesk/CallCenter/ClinicalReview"
        string employeeID
        date hireDate
        enum accountStatus "Active/Inactive"
        timestamp createdAt
    }

    Admin {
        uuid adminID PK
        string email UK "UNIQUE, NOT NULL"
        string name
        jsonb privileges "UserManagement, SystemConfig, AuditLogAccess"
        timestamp createdAt
    }

    Appointment {
        uuid appointmentID PK
        uuid patientID FK
        uuid providerID FK
        timestamp slotDateTime
        int durationMinutes
        enum status "Scheduled/Arrived/Completed/Cancelled/NoShow"
        enum bookingSource "Online/WalkIn/Staff"
        uuid preferredSlotSwapRequestID FK "NULLABLE"
        binary rowVersion "Optimistic concurrency control"
        timestamp bookedAt
        timestamp updatedAt
    }

    AppointmentSlot {
        uuid slotID PK
        uuid providerID FK
        timestamp startDateTime
        timestamp endDateTime
        boolean isAvailable
        enum slotType "Regular/WalkIn/Emergency"
        timestamp createdAt
    }

    ClinicalDocument {
        uuid documentID PK
        uuid patientID FK
        enum documentType "Lab/Imaging/Referral/PostVisitNote"
        timestamp uploadedAt
        uuid uploadedByUserID FK
        string filePath "Encrypted blob URL"
        int fileSizeBytes
        vector embeddingVector "pgvector[1536]"
        enum processingStatus "Pending/Completed/Failed"
    }

    ExtractedData {
        uuid extractedDataID PK
        uuid documentID FK
        enum dataType "Vital/Medication/Diagnosis/Allergy/Procedure/LabValue"
        jsonb value "Structured schema per type"
        float confidenceScore "0.0 to 1.0"
        timestamp extractedAt
        uuid verifiedByStaffID FK "NULLABLE"
        timestamp verifiedAt "NULLABLE"
        int sourceDocumentPageNumber
    }

    IntakeForm {
        uuid intakeFormID PK
        uuid patientID FK
        uuid appointmentID FK
        enum intakeMethod "AIConversational/ManualForm"
        timestamp completedAt
        timestamp lastEditedAt
        jsonb demographics "Name, DOB, Address, Insurance"
        jsonb medicalHistory "Conditions, Surgeries, FamilyHistory"
        jsonb currentMedications "Name, Dosage, Frequency"
        jsonb allergies "Substance, Reaction, Severity"
        text reasonForVisit
        jsonb editHistory "Array of edits with timestamps"
    }

    ConsolidatedPatientView {
        uuid patientID PK-FK
        jsonb medications "Merged list with sources"
        jsonb allergies "Merged list with sources"
        jsonb diagnoses "ICD-10 codes with sources"
        jsonb vitals "Latest readings with timestamps"
        jsonb conflictFlags "Array of detected conflicts"
        timestamp lastRefreshedAt
    }

    PreferredSlotSwapRequest {
        uuid swapRequestID PK
        uuid appointmentID FK "UNIQUE constraint"
        timestamp preferredSlotDateTime
        timestamp requestedAt
        enum status "Pending/Swapped/Expired/Cancelled"
        timestamp swappedAt "NULLABLE"
    }

    Reminder {
        uuid reminderID PK
        uuid appointmentID FK
        enum reminderType "Email/SMS"
        timestamp scheduledFor
        timestamp sentAt "NULLABLE"
        enum deliveryStatus "Pending/Sent/Failed/Bounced"
        boolean confirmationReceived
        string confirmationMethod "Reply/Link"
    }

    AuditLog {
        uuid auditLogID PK
        uuid userID FK "Polymorphic: Patient/Staff/Admin"
        enum userRole "Patient/Staff/Admin"
        enum action "Create/Read/Update/Delete/Login/Logout"
        string entityType "Appointment, ClinicalDocument, etc."
        uuid entityID
        timestamp timestamp
        string ipAddress
        string userAgent
        jsonb changeDetails "Before/After values"
    }
```

**ERD Constraints & Indexes:**

| Entity | Constraints | Indexes | HIPAA Notes |
|--------|-------------|---------|-------------|
| **Patient** | `email` UNIQUE, `passwordHash` NOT NULL | `idx_patient_email`, `idx_patient_dob` | Soft delete with `IsDeleted` flag (DR-009) |
| **Appointment** | `rowVersion` for optimistic concurrency, FK cascade to Patient | `idx_appointment_datetime`, `idx_appointment_status` | Immutable after completion (status only) |
| **ClinicalDocument** | `filePath` encrypted, `embeddingVector` using pgvector GIST index | `idx_document_patient`, `idx_embedding_cosine` | Server-side encryption (NFR-007) |
| **ExtractedData** | `confidenceScore` CHECK (>= 0.0 AND <= 1.0), FK to ClinicalDocument | `idx_extracted_confidence`, `idx_extracted_verified` | Low-confidence (<0.7) flagged for review |
| **AuditLog** | Append-only (DB trigger prevents UPDATE/DELETE), 7-year retention | `idx_audit_timestamp`, `idx_audit_userid` | HIPAA § 164.308(a)(1)(ii)(D) compliance |

## Use Case Sequence Diagrams

> **Note**: The following sequence diagrams detail the dynamic message flows for each use case (UC-001 through UC-010) defined in [spec.md](.propel/context/docs/spec.md). Each diagram maps success scenario steps to interactions between actors, system components, and external services.

### UC-001: Patient Books Appointment with Available Slot
**Source**: [spec.md#UC-001](.propel/context/docs/spec.md#UC-001)

```mermaid
sequenceDiagram
    participant Patient
    participant Frontend as Patient Portal<br/>(React/Next.js)
    participant API as API Controller<br/>(ASP.NET Core)
    participant BookingService as Booking Service<br/>(Business Logic)
    participant AppointmentRepo as Appointment Repository<br/>(EF Core)
    participant Database as PostgreSQL<br/>(Appointments Table)
    participant BackgroundJob as Background Job<br/>(Hangfire)
    participant EmailService as Email Service<br/>(SendGrid/Mailgun)
    participant CalendarAPI as Calendar API<br/>(Google/Outlook)

    Note over Patient,CalendarAPI: UC-001 - Patient Books Appointment with Available Slot

    Patient->>Frontend: Navigate to appointment booking
    Frontend->>API: GET /api/appointments/slots?provider={id}&date={date}
    API->>BookingService: GetAvailableSlots(provider, date)
    BookingService->>AppointmentRepo: QueryAvailableSlots(provider, date)
    AppointmentRepo->>Database: SELECT * FROM AppointmentSlots WHERE isAvailable=true
    Database-->>AppointmentRepo: Available slots list
    AppointmentRepo-->>BookingService: Slot DTOs
    BookingService-->>API: Filtered slots with timezone
    API-->>Frontend: 200 OK (JSON array of slots)
    Frontend-->>Patient: Display available slots

    Patient->>Frontend: Select desired slot and confirm
    Frontend->>API: POST /api/appointments<br/>{patientId, slotId, reason}
    API->>BookingService: BookAppointment(appointmentDto)
    BookingService->>AppointmentRepo: BeginTransaction()
    AppointmentRepo->>Database: BEGIN TRANSACTION
    
    BookingService->>AppointmentRepo: CheckSlotAvailability(slotId)
    AppointmentRepo->>Database: SELECT rowVersion FROM Appointments WHERE slotId={id}
    Database-->>AppointmentRepo: Slot still available
    
    BookingService->>AppointmentRepo: CreateAppointment(appointment)
    AppointmentRepo->>Database: INSERT INTO Appointments (patientId, slotId, status='Scheduled')
    Database-->>AppointmentRepo: appointmentID
    
    BookingService->>AppointmentRepo: MarkSlotUnavailable(slotId)
    AppointmentRepo->>Database: UPDATE AppointmentSlots SET isAvailable=false
    Database-->>AppointmentRepo: Success
    
    BookingService->>AppointmentRepo: CommitTransaction()
    AppointmentRepo->>Database: COMMIT
    Database-->>AppointmentRepo: Transaction committed
    
    AppointmentRepo-->>BookingService: Appointment created
    BookingService->>BackgroundJob: EnqueueGeneratePDF(appointmentID)
    BookingService->>BackgroundJob: EnqueueSendConfirmationEmail(appointmentID)
    BookingService->>BackgroundJob: EnqueueSyncCalendar(appointmentID)
    BookingService-->>API: appointmentDTO with confirmationNumber
    API-->>Frontend: 201 Created (confirmationNumber)
    Frontend-->>Patient: Success! Confirmation number displayed

    Note over BackgroundJob,CalendarAPI: Async processing (non-blocking)
    
    BackgroundJob->>BackgroundJob: GeneratePDF(appointmentID)
    BackgroundJob->>EmailService: SendEmail(to, subject, pdfAttachment)
    EmailService-->>Patient: Confirmation email with PDF (within 60s)
    
    BackgroundJob->>CalendarAPI: CreateCalendarEvent(patientId, appointmentDetails)
    CalendarAPI-->>BackgroundJob: Calendar event created
    BackgroundJob->>Database: UPDATE Appointment SET calendarSyncStatus='Synced'

    alt PDF Generation Fails
        BackgroundJob->>EmailService: SendEmail(plain text details only)
        BackgroundJob->>Database: INSERT INTO AuditLog (action='PDFGenerationFailed')
    end

    alt Calendar Sync Fails
        BackgroundJob->>Database: UPDATE Appointment SET calendarSyncStatus='Failed'
        BackgroundJob->>EmailService: Include .ics file attachment as fallback
    end
```

### UC-002: Patient Books with Preferred Slot Swap
**Source**: [spec.md#UC-002](.propel/context/docs/spec.md#UC-002)

```mermaid
sequenceDiagram
    participant Patient
    participant Frontend as Patient Portal<br/>(React/Next.js)
    participant API as API Controller
    participant BookingService as Booking Service
    participant AppointmentRepo as Appointment Repository
    participant SwapRepo as Swap Request Repository
    participant Database as PostgreSQL
    participant BackgroundMonitor as Slot Monitor Job<br/>(Scheduled Hangfire)
    participant NotificationService as Notification Service<br/>(Email/SMS)

    Note over Patient,NotificationService: UC-002 - Patient Books with Preferred Slot Swap

    Patient->>Frontend: Select available slot (primary)
    Frontend->>Patient: Prompt: "Would you prefer a different time?"
    Patient->>Frontend: Select currently unavailable preferred slot
    
    Frontend->>API: POST /api/appointments/with-swap<br/>{primarySlotId, preferredSlotDateTime}
    API->>BookingService: BookAppointmentWithSwap(dto)
    
    BookingService->>AppointmentRepo: BeginTransaction()
    BookingService->>AppointmentRepo: CreateAppointment(primarySlotId)
    AppointmentRepo->>Database: INSERT INTO Appointments (slotId={primarySlot}, status='Scheduled')
    Database-->>AppointmentRepo: appointmentID
    
    BookingService->>SwapRepo: CreateSwapRequest(appointmentID, preferredSlotDateTime)
    SwapRepo->>Database: INSERT INTO PreferredSlotSwapRequests<br/>(appointmentID, preferredSlotDateTime, status='Pending')
    Database-->>SwapRepo: swapRequestID
    
    BookingService->>AppointmentRepo: CommitTransaction()
    AppointmentRepo->>Database: COMMIT
    
    BookingService-->>API: Appointment + SwapRequest DTOs
    API-->>Frontend: 201 Created (confirmationNumber, swapPending=true)
    Frontend-->>Patient: Booking confirmed! Will auto-swap if preferred slot opens

    BookingService->>NotificationService: SendConfirmationEmail(appointmentID, swapExplanation)
    NotificationService-->>Patient: Email: "Booked {primarySlot}, monitoring {preferredSlot}"

    Note over BackgroundMonitor: Time passes - Slot monitor runs every 5 minutes

    BackgroundMonitor->>SwapRepo: GetPendingSwapRequests()
    SwapRepo->>Database: SELECT * FROM PreferredSlotSwapRequests WHERE status='Pending'
    Database-->>SwapRepo: List of pending swaps
    SwapRepo-->>BackgroundMonitor: Swap request list
    
    loop For each pending swap
        BackgroundMonitor->>AppointmentRepo: CheckSlotAvailability(preferredSlotDateTime)
        AppointmentRepo->>Database: SELECT isAvailable FROM AppointmentSlots WHERE slotDateTime={preferred}
        Database-->>AppointmentRepo: isAvailable=true (slot opened!)
        AppointmentRepo-->>BackgroundMonitor: Preferred slot available
        
        BackgroundMonitor->>BookingService: ExecuteSwap(swapRequestID)
        BookingService->>AppointmentRepo: BeginTransaction()
        
        BookingService->>AppointmentRepo: GetAppointment(appointmentID)
        AppointmentRepo->>Database: SELECT * FROM Appointments WHERE appointmentID={id}
        Database-->>AppointmentRepo: Current appointment details
        
        BookingService->>AppointmentRepo: UpdateAppointmentSlot(appointmentID, preferredSlotId)
        AppointmentRepo->>Database: UPDATE Appointments SET slotId={preferredSlotId}, updatedAt=NOW()
        Database-->>AppointmentRepo: Success
        
        BookingService->>AppointmentRepo: MarkSlotAvailable(originalSlotId)
        AppointmentRepo->>Database: UPDATE AppointmentSlots SET isAvailable=true WHERE slotId={original}
        Database-->>AppointmentRepo: Original slot released
        
        BookingService->>AppointmentRepo: MarkSlotUnavailable(preferredSlotId)
        AppointmentRepo->>Database: UPDATE AppointmentSlots SET isAvailable=false WHERE slotId={preferred}
        Database-->>AppointmentRepo: Preferred slot booked
        
        BookingService->>SwapRepo: UpdateSwapStatus(swapRequestID, 'Swapped')
        SwapRepo->>Database: UPDATE PreferredSlotSwapRequests SET status='Swapped', swappedAt=NOW()
        Database-->>SwapRepo: Success
        
        BookingService->>AppointmentRepo: CommitTransaction()
        AppointmentRepo->>Database: COMMIT
        
        BookingService->>NotificationService: SendSwapNotification(appointmentID, newSlotDetails)
        NotificationService-->>Patient: Email & SMS: "Success! Swapped to {preferredSlot}"
        
        BookingService->>Database: INSERT INTO AuditLog (action='PreferredSlotSwapped', appointmentID)
    end

    alt Patient Canceled Before Swap
        BookingService->>SwapRepo: UpdateSwapStatus(swapRequestID, 'Cancelled')
        SwapRepo->>Database: UPDATE PreferredSlotSwapRequests SET status='Cancelled'
    end

    alt Preferred Slot Never Opens
        Note over BackgroundMonitor: Appointment date arrives without swap
        BackgroundMonitor->>SwapRepo: UpdateSwapStatus(swapRequestID, 'Expired')
        SwapRepo->>Database: UPDATE SET status='Expired'
    end
```

### UC-003: Patient Completes AI Conversational Intake
**Source**: [spec.md#UC-003](.propel/context/docs/spec.md#UC-003)

```mermaid
sequenceDiagram
    participant Patient
    participant Frontend as Patient Portal<br/>(Chat UI)
    participant API as API Controller
    participant IntakeService as Intake Service
    participant AIGateway as AI Gateway Middleware
    participant AIModel as AI Model Provider<br/>(OpenAI/Ollama)
    participant IntakeRepo as Intake Repository
    participant Database as PostgreSQL

    Note over Patient,Database: UC-003 - Patient Completes AI Conversational Intake

    Patient->>Frontend: Select "Start AI Intake"
    Frontend->>API: POST /api/intake/start-ai-session<br/>{appointmentID, patientID}
    API->>IntakeService: InitializeAISession(appointmentID)
    IntakeService->>Database: INSERT INTO IntakeForms (patientID, intakeMethod='AIConversational', status='InProgress')
    Database-->>IntakeService: intakeFormID
    IntakeService-->>API: sessionID, firstPrompt
    API-->>Frontend: 200 OK {sessionID, message: "Hi! Let's get your info. What's your full name?"}
    Frontend-->>Patient: AI greeting displayed

    loop Conversational Data Collection
        Patient->>Frontend: Type free-text response<br/>"John Doe, born Jan 15, 1980"
        Frontend->>API: POST /api/intake/ai-message<br/>{sessionID, userMessage}
        
        API->>AIGateway: RouteToAI(userMessage, conversationHistory)
        AIGateway->>AIGateway: CheckTokenBudget(sessionID)
        Note over AIGateway: Token budget: 4000/session (AIR-O01)
        
        alt Token Budget Exceeded
            AIGateway-->>API: 429 Too Many Requests
            API-->>Frontend: {fallback: true, message: "Please complete remaining fields manually"}
            Frontend-->>Patient: Switch to manual form (data preserved)
        end
        
        AIGateway->>AIModel: GenerateResponse(prompt, userMessage, context)
        Note over AIModel: Extracts: firstName="John", lastName="Doe", DOB="1980-01-15"
        AIModel-->>AIGateway: {extractedData: {...}, nextQuestion: "What medications are you currently taking?"}
        
        AIGateway->>IntakeService: UpdateIntakeForm(intakeFormID, extractedData)
        IntakeService->>IntakeRepo: UpsertDemographics(intakeFormID, {firstName, lastName, DOB})
        IntakeRepo->>Database: UPDATE IntakeForms SET demographics=jsonb_set(demographics, ...)
        Database-->>IntakeRepo: Success
        
        AIGateway->>Database: INSERT INTO AuditLog (action='AIInference', promptTokens, responseTokens)
        
        AIGateway-->>API: {message: nextQuestion, extractedData}
        API-->>Frontend: 200 OK {message, progress: "25% complete"}
        Frontend-->>Patient: AI next question displayed
    end

    Patient->>Frontend: Complete all sections (medical history, meds, allergies, reason)
    Frontend->>API: POST /api/intake/ai-confirm<br/>{sessionID}
    API->>IntakeService: GenerateIntakeSummary(intakeFormID)
    IntakeService->>IntakeRepo: GetIntakeForm(intakeFormID)
    IntakeRepo->>Database: SELECT * FROM IntakeForms WHERE intakeFormID={id}
    Database-->>IntakeRepo: Full intake data
    IntakeRepo-->>IntakeService: IntakeFormDTO
    IntakeService-->>API: Formatted summary
    API-->>Frontend: 200 OK {summary: {...}}
    Frontend-->>Patient: "Review your information" (editable summary)

    Patient->>Frontend: Confirm accuracy and submit
    Frontend->>API: POST /api/intake/finalize<br/>{intakeFormID, confirmed=true}
    API->>IntakeService: FinalizeIntake(intakeFormID)
    IntakeService->>IntakeRepo: UpdateIntakeStatus(intakeFormID, 'Completed')
    IntakeRepo->>Database: UPDATE IntakeForms SET status='Completed', completedAt=NOW()
    Database-->>IntakeRepo: Success
    IntakeService-->>API: Success
    API-->>Frontend: 200 OK {message: "Intake Complete"}
    Frontend-->>Patient: "Intake Complete! Ready for appointment"

    alt Patient Identifies Incorrect Extraction
        Patient->>Frontend: Click "Edit" on extracted field
        Frontend->>Patient: Inline editor appears
        Patient->>Frontend: Correct value "Aspirin 100mg" → "Aspirin 81mg"
        Frontend->>API: PATCH /api/intake/{intakeFormID}/field<br/>{field: 'medications[0].dosage', value: '81mg'}
        API->>IntakeService: CorrectExtraction(intakeFormID, field, value)
        IntakeService->>Database: UPDATE IntakeForms, INSERT INTO editHistory
        Database-->>IntakeService: Success
        IntakeService->>AIModel: LogCorrectionForFeedback(originalValue, correctedValue)
    end

    alt Patient Pauses Intake
        Patient->>Frontend: Click "Save & Continue Later"
        Frontend->>API: POST /api/intake/pause<br/>{intakeFormID}
        API->>IntakeService: PauseIntake(intakeFormID)
        IntakeService->>Database: UPDATE IntakeForms SET status='Paused', lastEditedAt=NOW()
        Database-->>IntakeService: Success
        IntakeService-->>API: Success
        API-->>Frontend: 200 OK
        Frontend-->>Patient: "Progress saved. Resume anytime from dashboard"
    end
```

### UC-004: Patient Completes Manual Form Intake
**Source**: [spec.md#UC-004](.propel/context/docs/spec.md#UC-004)

```mermaid
sequenceDiagram
    participant Patient
    participant Frontend as Patient Portal<br/>(Form UI)
    participant API as API Controller
    participant IntakeService as Intake Service
    participant IntakeRepo as Intake Repository
    participant Database as PostgreSQL

    Note over Patient,Database: UC-004 - Patient Completes Manual Form Intake

    Patient->>Frontend: Select "Start Manual Form"
    Frontend->>API: POST /api/intake/start-manual<br/>{appointmentID, patientID}
    API->>IntakeService: InitializeManualIntake(appointmentID)
    IntakeService->>IntakeRepo: CreateIntakeForm(patientID, 'ManualForm')
    IntakeRepo->>Database: INSERT INTO IntakeForms (patientID, appointmentID, intakeMethod='ManualForm', status='InProgress')
    Database-->>IntakeRepo: intakeFormID
    IntakeRepo-->>IntakeService: intakeFormID
    IntakeService-->>API: intakeFormID
    API-->>Frontend: 201 Created {intakeFormID}
    Frontend-->>Patient: Display intake form with sections

    loop Fill Form Sections
        Patient->>Frontend: Enter demographic information<br/>(Name, DOB, Phone, Address, Insurance)
        Frontend->>Frontend: Real-time validation (required fields, format checks)
        
        alt Invalid Format
            Frontend-->>Patient: Inline error: "Phone must be (XXX) XXX-XXXX"
            Patient->>Frontend: Correct phone format
        end
        
        Patient->>Frontend: Enter medical history<br/>(Past conditions, surgeries, family history)
        Patient->>Frontend: Enter current medications<br/>(Name, dosage, frequency, start date)
        Patient->>Frontend: Enter allergies<br/>(Substance, reaction, severity)
        Patient->>Frontend: Enter reason for visit<br/>(Chief complaint, duration)
        
        Frontend->>Frontend: Auto-save every 30 seconds
        Frontend->>API: PATCH /api/intake/{intakeFormID}/draft<br/>{fieldUpdates}
        API->>IntakeService: SaveDraft(intakeFormID, fieldUpdates)
        IntakeService->>IntakeRepo: UpdateIntakeForm(intakeFormID, fieldUpdates)
        IntakeRepo->>Database: UPDATE IntakeForms SET demographics=..., lastEditedAt=NOW()
        Database-->>IntakeRepo: Success
        IntakeRepo-->>IntakeService: Success
        IntakeService-->>API: Success
        API-->>Frontend: 200 OK (silent save)
    end

    Patient->>Frontend: Click "Review" button
    Frontend->>Frontend: Client-side validation (check required fields)
    
    alt Required Fields Missing
        Frontend-->>Patient: Highlight missing fields in red<br/>"Please complete: Medical History"
        Patient->>Frontend: Complete missing sections
    end

    Frontend->>API: GET /api/intake/{intakeFormID}/summary
    API->>IntakeService: GetIntakeSummary(intakeFormID)
    IntakeService->>IntakeRepo: GetIntakeForm(intakeFormID)
    IntakeRepo->>Database: SELECT * FROM IntakeForms WHERE intakeFormID={id}
    Database-->>IntakeRepo: Full intake data
    IntakeRepo-->>IntakeService: IntakeFormDTO
    IntakeService-->>API: Formatted summary
    API-->>Frontend: 200 OK {summary: {...}}
    Frontend-->>Patient: Display summary page (all sections)

    Patient->>Frontend: Review summary and click "Submit"
    Frontend->>API: POST /api/intake/{intakeFormID}/submit
    API->>IntakeService: SubmitIntake(intakeFormID)
    
    IntakeService->>IntakeService: ServerValidation(intakeData)
    alt Server Validation Fails
        IntakeService-->>API: 400 Bad Request {errors: [...]}
        API-->>Frontend: 400 Bad Request
        Frontend-->>Patient: "Validation failed: {errors}"
    end
    
    IntakeService->>IntakeRepo: FinalizeIntake(intakeFormID)
    IntakeRepo->>Database: UPDATE IntakeForms SET status='Completed', completedAt=NOW()
    Database-->>IntakeRepo: Success
    
    IntakeService->>Database: INSERT INTO AuditLog (action='IntakeFormSubmitted', userID, intakeFormID)
    Database-->>IntakeService: Audit logged
    
    IntakeService-->>API: Success
    API-->>Frontend: 200 OK {message: "Intake Complete"}
    Frontend-->>Patient: "Intake Complete! Ready for appointment"

    alt Patient Saves Draft
        Patient->>Frontend: Click "Save Draft" anytime
        Frontend->>API: POST /api/intake/{intakeFormID}/save-draft
        API->>IntakeService: SaveDraft(intakeFormID)
        IntakeService->>IntakeRepo: UpdateIntakeForm(status='Draft', lastEditedAt=NOW())
        IntakeRepo->>Database: UPDATE IntakeForms SET status='Draft'
        Database-->>IntakeRepo: Success
        IntakeRepo-->>IntakeService: Success
        IntakeService-->>API: Success
        API-->>Frontend: 200 OK
        Frontend-->>Patient: "Draft saved. Resume from dashboard anytime"
    end

    alt Patient Toggles to AI Intake
        Patient->>Frontend: Click "Switch to AI Intake" toggle
        Frontend->>Patient: Confirmation dialog: "Switch mode? Data will be preserved"
        Patient->>Frontend: Confirm
        Frontend->>API: POST /api/intake/{intakeFormID}/switch-to-ai
        API->>IntakeService: SwitchIntakeMethod(intakeFormID, 'AIConversational')
        IntakeService->>IntakeRepo: UpdateIntakeMethod(intakeFormID, 'AIConversational')
        IntakeRepo->>Database: UPDATE IntakeForms SET intakeMethod='AIConversational'
        Database-->>IntakeRepo: Success
        IntakeRepo-->>IntakeService: Success
        IntakeService-->>API: aiSessionID
        API-->>Frontend: 200 OK {aiSessionID, preservedData}
        Frontend-->>Patient: Switch to AI chat interface (data pre-filled)
    end
```

### UC-005: Staff Processes Walk-In Appointment
**Source**: [spec.md#UC-005](.propel/context/docs/spec.md#UC-005)

```mermaid
sequenceDiagram
    participant Staff
    participant StaffPortal as Staff Portal<br/>(React UI)
    participant API as API Controller
    participant WalkInService as Walk-In Service
    participant PatientRepo as Patient Repository
    participant AppointmentRepo as Appointment Repository
    participant Database as PostgreSQL
    participant EmailService as Email Service

    Note over Staff,EmailService: UC-005 - Staff Processes Walk-In Appointment

    Staff->>StaffPortal: Navigate to "Walk-In Management"
    Staff->>StaffPortal: Click "New Walk-In" button
    StaffPortal->>API: GET /api/walk-ins/new
    API-->>StaffPortal: 200 OK {form template}
    StaffPortal-->>Staff: Display walk-in intake form

    Staff->>StaffPortal: Enter patient info<br/>(Name, DOB, Phone, Reason for visit)
    StaffPortal->>API: POST /api/walk-ins/search-patient<br/>{name, dob}
    API->>WalkInService: SearchPatient(name, dob)
    WalkInService->>PatientRepo: FindByNameAndDOB(name, dob)
    PatientRepo->>Database: SELECT * FROM Patients WHERE firstName ILIKE '%{name}%' AND dateOfBirth={dob}
    Database-->>PatientRepo: Patient records (if exist)
    PatientRepo-->>WalkInService: Patient DTOs or null
    WalkInService-->>API: SearchResult
    API-->>StaffPortal: 200 OK {existingPatient: {...} or null}

    alt Existing Patient Found
        StaffPortal-->>Staff: "Patient found! {name}. Use existing record?"
        Staff->>StaffPortal: Confirm "Use Existing"
        StaffPortal->>StaffPortal: Load patient data into form
    else No Existing Patient
        StaffPortal-->>Staff: "New patient. Continue data entry"
        Staff->>StaffPortal: Complete walk-in intake fields<br/>(Quick demographics, chief complaint)
    end

    Staff->>StaffPortal: Select "Book Walk-In Slot"
    StaffPortal->>API: POST /api/walk-ins/create<br/>{patientInfo, providerId, reason}
    API->>WalkInService: CreateWalkInAppointment(dto)
    
    WalkInService->>WalkInService: BeginTransaction()
    
    alt No Existing Patient
        WalkInService->>PatientRepo: CreatePatient(patientInfo)
        PatientRepo->>Database: INSERT INTO Patients (firstName, lastName, dob, phone, accountStatus='Inactive')
        Note over Database: Account created as Inactive (no login credentials yet)
        Database-->>PatientRepo: patientID
        PatientRepo-->>WalkInService: patientID
    end
    
    WalkInService->>AppointmentRepo: FindAvailableWalkInSlot(providerId)
    AppointmentRepo->>Database: SELECT * FROM AppointmentSlots WHERE slotType='WalkIn' AND isAvailable=true LIMIT 1
    Database-->>AppointmentRepo: Available walk-in slot or null
    
    alt No Walk-In Slot Available
        AppointmentRepo->>Database: INSERT INTO SameDayQueue (patientID, providerId, arrivedAt=NOW())
        Database-->>AppointmentRepo: queuePosition
        AppointmentRepo-->>WalkInService: {queuePosition, estimatedWait}
        WalkInService-->>API: {queueAssigned: true, position: 3, waitTime: "30 min"}
        API-->>StaffPortal: 201 Created {queuePosition}
        StaffPortal-->>Staff: "Walk-in added to queue. Position: 3, Est. wait: 30 min"
    else Walk-In Slot Available
        WalkInService->>AppointmentRepo: CreateAppointment(patientID, slotID, bookingSource='WalkIn', status='Scheduled')
        AppointmentRepo->>Database: INSERT INTO Appointments (patientID, slotId, bookingSource='WalkIn', status='Scheduled', bookedAt=NOW())
        Database-->>AppointmentRepo: appointmentID
        AppointmentRepo->>Database: UPDATE AppointmentSlots SET isAvailable=false WHERE slotId={id}
        Database-->>AppointmentRepo: Slot booked
        AppointmentRepo-->>WalkInService: appointmentID
        WalkInService-->>API: {appointmentID, confirmationNumber}
        API-->>StaffPortal: 201 Created {confirmationNumber}
        StaffPortal-->>Staff: "Walk-in booked! Confirmation: {number}"
    end
    
    WalkInService->>Database: INSERT INTO AuditLog (action='WalkInCreated', staffID, patientID)
    
    WalkInService->>WalkInService: CommitTransaction()
    Database-->>WalkInService: Transaction committed

    Staff->>StaffPortal: Click "Create Account for Patient?" (optional)
    StaffPortal->>API: POST /api/walk-ins/{appointmentID}/create-account
    API->>WalkInService: CreatePatientAccount(patientID)
    
    WalkInService->>PatientRepo: GetPatient(patientID)
    PatientRepo->>Database: SELECT * FROM Patients WHERE patientID={id}
    Database-->>PatientRepo: Patient data
    PatientRepo-->>WalkInService: PatientDTO
    
    WalkInService->>WalkInService: GenerateTemporaryPassword()
    WalkInService->>PatientRepo: UpdatePatientAccount(patientID, hashedPassword, accountStatus='Active')
    PatientRepo->>Database: UPDATE Patients SET passwordHash={hash}, accountStatus='Active'
    Database-->>PatientRepo: Success
    
    WalkInService->>EmailService: SendAccountActivationEmail(patientEmail, temporaryPassword)
    EmailService-->>Patient: Email: "Your account is ready. Login with temp password"
    
    WalkInService-->>API: {accountCreated: true}
    API-->>StaffPortal: 200 OK {message: "Account created. Activation email sent"}
    StaffPortal-->>Staff: "Patient account activated. Email sent"

    alt Patient Declines Account Creation
        Staff->>StaffPortal: Select "Account Declined"
        StaffPortal->>API: POST /api/walk-ins/{appointmentID}/decline-account
        API->>WalkInService: MarkAccountDeclined(patientID)
        WalkInService->>Database: UPDATE Patients SET accountStatus='Declined'
        Database-->>WalkInService: Success
        WalkInService-->>API: Success
        API-->>StaffPortal: 200 OK
        StaffPortal-->>Staff: "Walk-in complete. No account created"
    end
```

### UC-006: Staff Marks Patient as Arrived
**Source**: [spec.md#UC-006](.propel/context/docs/spec.md#UC-006)

```mermaid
sequenceDiagram
    participant Staff
    participant StaffPortal as Staff Portal<br/>(Arrival Dashboard)
    participant API as API Controller
    participant ArrivalService as Arrival Service
    participant AppointmentRepo as Appointment Repository
    participant QueueRepo as Queue Repository
    participant Database as PostgreSQL
    participant NotificationService as Notification Service
    participant Provider

    Note over Staff,Provider: UC-006 - Staff Marks Patient as Arrived

    Staff->>StaffPortal: Navigate to "Arrival Management"
    StaffPortal->>API: GET /api/arrivals/today
    API->>ArrivalService: GetTodaysScheduledAppointments()
    ArrivalService->>AppointmentRepo: QueryTodaysAppointments(status='Scheduled')
    AppointmentRepo->>Database: SELECT * FROM Appointments<br/>WHERE DATE(slotDateTime)=CURRENT_DATE<br/>AND status='Scheduled'<br/>ORDER BY slotDateTime
    Database-->>AppointmentRepo: Today's appointment list
    AppointmentRepo-->>ArrivalService: Appointment DTOs
    ArrivalService-->>API: Appointment list with patient names
    API-->>StaffPortal: 200 OK {appointments: [...]}
    StaffPortal-->>Staff: Display scheduled appointments table

    Staff->>StaffPortal: Patient arrives at front desk
    Staff->>StaffPortal: Search for patient by name or DOB
    StaffPortal->>API: GET /api/arrivals/search?query={name}
    API->>ArrivalService: SearchTodaysAppointments(query)
    ArrivalService->>AppointmentRepo: FindAppointment(name, today)
    AppointmentRepo->>Database: SELECT * FROM Appointments a<br/>JOIN Patients p ON a.patientID=p.patientID<br/>WHERE p.firstName ILIKE '%{name}%'<br/>AND DATE(a.slotDateTime)=CURRENT_DATE
    Database-->>AppointmentRepo: Matching appointments
    AppointmentRepo-->>ArrivalService: SearchResults
    ArrivalService-->>API: SearchResults
    API-->>StaffPortal: 200 OK {results: [...]}
    StaffPortal-->>Staff: Display search results

    alt Patient Not Found
        StaffPortal-->>Staff: "No appointment found for today"
        Staff->>StaffPortal: Verify appointment details
        alt Is Walk-In
            Staff->>StaffPortal: Redirect to "Create Walk-In" (UC-005)
        end
    end

    Staff->>StaffPortal: Select appointment and click "Check In"
    StaffPortal->>API: POST /api/arrivals/{appointmentID}/check-in
    API->>ArrivalService: MarkPatientArrived(appointmentID)
    
    ArrivalService->>AppointmentRepo: GetAppointment(appointmentID)
    AppointmentRepo->>Database: SELECT * FROM Appointments WHERE appointmentID={id}
    Database-->>AppointmentRepo: Appointment details
    AppointmentRepo-->>ArrivalService: AppointmentDTO
    
    ArrivalService->>ArrivalService: ValidateCheckIn(appointment)
    alt Appointment Already Checked In
        ArrivalService-->>API: 409 Conflict {error: "Patient already checked in"}
        API-->>StaffPortal: 409 Conflict
        StaffPortal-->>Staff: "Error: Already checked in at {timestamp}"
    end
    
    alt Patient More Than 15 Minutes Late
        ArrivalService->>ArrivalService: CalculateLateness(slotDateTime, NOW())
        Note over ArrivalService: Lateness > 15 minutes detected
        ArrivalService->>AppointmentRepo: UpdateAppointment(appointmentID, status='LateArrival', arrivedAt=NOW())
        ArrivalService->>NotificationService: NotifyProviderLateArrival(appointmentID)
    else Patient On Time
        ArrivalService->>AppointmentRepo: UpdateAppointment(appointmentID, status='Arrived', arrivedAt=NOW())
    end
    
    AppointmentRepo->>Database: UPDATE Appointments<br/>SET status='Arrived', arrivedAt=NOW(), updatedAt=NOW()<br/>WHERE appointmentID={id}
    Database-->>AppointmentRepo: Success
    AppointmentRepo-->>ArrivalService: Updated appointment
    
    ArrivalService->>QueueRepo: AddToProviderQueue(appointmentID, providerId)
    QueueRepo->>Database: INSERT INTO ProviderQueue (appointmentID, providerId, queuedAt=NOW())
    Database-->>QueueRepo: Queue position
    QueueRepo-->>ArrivalService: queuePosition
    
    ArrivalService->>QueueRepo: CalculateEstimatedWaitTime(providerId, queuePosition)
    QueueRepo->>Database: SELECT AVG(visitDurationMinutes)<br/>FROM Appointments<br/>WHERE providerID={id} AND status='Completed'<br/>AND completedAt >= NOW() - INTERVAL '7 days'
    Database-->>QueueRepo: avgDuration
    QueueRepo-->>ArrivalService: estimatedWaitMinutes
    
    ArrivalService->>NotificationService: NotifyProvider(appointmentID, providerID, 'PatientArrived')
    NotificationService->>Provider: Push notification or dashboard alert<br/>"Patient {name} checked in"
    
    ArrivalService->>Database: INSERT INTO AuditLog<br/>(action='PatientCheckedIn', staffID, appointmentID, timestamp)
    Database-->>ArrivalService: Audit logged
    
    ArrivalService-->>API: {status: 'Arrived', queuePosition, estimatedWait}
    API-->>StaffPortal: 200 OK {message: "Check-in successful", queuePosition: 2, waitTime: "15 min"}
    StaffPortal-->>Staff: "Patient checked in! Queue position: 2, Est. wait: 15 min"
    
    StaffPortal->>StaffPortal: Refresh today's appointment list
    StaffPortal->>API: GET /api/arrivals/queue/{providerId}
    API->>QueueRepo: GetProviderQueue(providerId)
    QueueRepo->>Database: SELECT * FROM ProviderQueue pq<br/>JOIN Appointments a ON pq.appointmentID=a.appointmentID<br/>JOIN Patients p ON a.patientID=p.patientID<br/>WHERE pq.providerId={id} AND a.status='Arrived'<br/>ORDER BY pq.queuedAt
    Database-->>QueueRepo: Queue list
    QueueRepo-->>API: Queue DTOs with wait times
    API-->>StaffPortal: 200 OK {queue: [...]}
    StaffPortal-->>Staff: Display updated provider queue with wait times

    alt Staff Needs to Reorder Queue
        Staff->>StaffPortal: Drag and drop queue items
        StaffPortal->>API: PATCH /api/arrivals/queue/{providerId}/reorder<br/>{newOrder: [...]}
        API->>QueueRepo: ReorderQueue(providerId, newOrder)
        QueueRepo->>Database: UPDATE ProviderQueue SET queuePosition=... WHERE appointmentID IN (...)
        Database-->>QueueRepo: Success
        QueueRepo-->>API: Success
        API-->>StaffPortal: 200 OK
        StaffPortal-->>Staff: Queue reordered successfully
    end
```

### UC-007: Staff Validates Patient Insurance
**Source**: [spec.md#UC-007](.propel/context/docs/spec.md#UC-007)

```mermaid
sequenceDiagram
    participant Staff
    participant StaffPortal as Staff Portal<br/>(Patient Record View)
    participant API as API Controller
    participant InsuranceService as Insurance Service
    participant PatientRepo as Patient Repository
    participant InsuranceDB as Insurance Database<br/>(Internal Dummy)
    participant Database as PostgreSQL

    Note over Staff,Database: UC-007 - Staff Validates Patient Insurance

    Staff->>StaffPortal: Navigate to patient record
    StaffPortal->>API: GET /api/patients/{patientID}
    API->>PatientRepo: GetPatient(patientID)
    PatientRepo->>Database: SELECT * FROM Patients WHERE patientID={id}
    Database-->>PatientRepo: Patient data
    PatientRepo-->>API: PatientDTO
    API-->>StaffPortal: 200 OK {patient: {...}}
    StaffPortal-->>Staff: Display patient details

    Staff->>StaffPortal: Navigate to "Insurance" tab
    StaffPortal->>API: GET /api/patients/{patientID}/insurance
    API->>PatientRepo: GetPatientInsuranceInfo(patientID)
    PatientRepo->>Database: SELECT insurance FROM IntakeForms<br/>WHERE patientID={id}<br/>ORDER BY completedAt DESC LIMIT 1
    Database-->>PatientRepo: Insurance info from intake
    PatientRepo-->>API: InsuranceDTO (name, memberId, groupNumber)
    API-->>StaffPortal: 200 OK {insurance: {...}, validationStatus: 'Pending'}
    StaffPortal-->>Staff: Display insurance information

    Staff->>StaffPortal: Click "Validate Insurance"
    StaffPortal->>API: POST /api/insurance/validate<br/>{patientID, insuranceName, memberId}
    API->>InsuranceService: ValidateInsurance(insuranceName, memberId)
    
    InsuranceService->>InsuranceDB: QueryInsuranceRecord(insuranceName, memberId)
    Note over InsuranceDB: Internal dummy database with common payers
    InsuranceDB->>Database: SELECT * FROM InternalInsuranceRecords<br/>WHERE LOWER(payerName) LIKE '%{insuranceName}%'<br/>AND memberId={memberId}
    Database-->>InsuranceDB: Matching records or null
    InsuranceDB-->>InsuranceService: ValidationResult
    
    alt Match Found
        InsuranceService->>InsuranceService: MarkValidationStatus('Match')
        InsuranceService-->>API: {status: 'Match', payerName: 'Blue Cross Blue Shield', groupNumber: 'G12345'}
        API-->>StaffPortal: 200 OK {validationStatus: 'Match', details: {...}}
        StaffPortal-->>Staff: Display green checkmark<br/>"Insurance validated: {payerName}"
        
        StaffPortal->>API: PATCH /api/patients/{patientID}/insurance<br/>{validationStatus: 'Match', validatedAt: NOW()}
        API->>PatientRepo: UpdateInsuranceValidation(patientID, 'Match')
        PatientRepo->>Database: UPDATE IntakeForms SET insuranceValidationStatus='Match'<br/>WHERE patientID={id}
        Database-->>PatientRepo: Success
        
    else No Match Found
        InsuranceService->>InsuranceService: MarkValidationStatus('NoMatch')
        InsuranceService-->>API: {status: 'NoMatch', message: 'No matching record found'}
        API-->>StaffPortal: 200 OK {validationStatus: 'NoMatch'}
        StaffPortal-->>Staff: Display yellow warning<br/>"Validation failed: No match. Flag for manual review"
        
        StaffPortal->>API: POST /api/patients/{patientID}/insurance/flag<br/>{reason: 'NoMatchFound'}
        API->>InsuranceService: FlagForManualReview(patientID, reason)
        InsuranceService->>Database: UPDATE IntakeForms SET insuranceValidationStatus='ManualReviewRequired'<br/>WHERE patientID={id}
        InsuranceService->>Database: INSERT INTO ReviewQueue (patientID, reviewType='InsuranceValidation', flaggedAt=NOW())
        Database-->>InsuranceService: Flagged successfully
        InsuranceService-->>API: Success
        API-->>StaffPortal: 200 OK {flagged: true}
        StaffPortal-->>Staff: "Flagged for manual review. Added to review queue"
        
    else Partial Match
        InsuranceService->>InsuranceService: MarkValidationStatus('PartialMatch')
        InsuranceService-->>API: {status: 'PartialMatch', message: 'Payer name matches, but member ID differs', suggestion: {...}}
        API-->>StaffPortal: 200 OK {validationStatus: 'PartialMatch', suggestion: {...}}
        StaffPortal-->>Staff: Display amber icon<br/>"Partial match: Payer OK, but member ID mismatch"
        Staff->>StaffPortal: Review suggestion and choose action
        
        alt Staff Accepts Suggestion
            Staff->>StaffPortal: Click "Accept Suggested Member ID"
            StaffPortal->>API: PATCH /api/patients/{patientID}/insurance<br/>{memberId: '{suggestedId}', validationStatus: 'Match'}
            API->>PatientRepo: UpdateInsurance(patientID, suggestedId)
            PatientRepo->>Database: UPDATE IntakeForms SET insurance.memberId='{suggestedId}', validationStatus='Match'
            Database-->>PatientRepo: Success
            PatientRepo-->>API: Updated
            API-->>StaffPortal: 200 OK
            StaffPortal-->>Staff: "Insurance updated and validated"
        else Staff Flags for Manual Review
            Staff->>StaffPortal: Click "Flag for Manual Review"
            StaffPortal->>API: POST /api/patients/{patientID}/insurance/flag<br/>{reason: 'PartialMatchNeedsReview'}
            API->>InsuranceService: FlagForManualReview(patientID, reason)
            InsuranceService->>Database: INSERT INTO ReviewQueue (patientID, reviewType='InsuranceValidation')
            Database-->>InsuranceService: Success
            InsuranceService-->>API: Flagged
            API-->>StaffPortal: 200 OK
            StaffPortal-->>Staff: "Flagged for manual follow-up"
        end
    end
    
    InsuranceService->>Database: INSERT INTO AuditLog<br/>(action='InsuranceValidated', staffID, patientID, validationResult)
    Database-->>InsuranceService: Audit logged

    alt Insurance Info Missing from Intake
        StaffPortal-->>Staff: "No insurance info provided"
        Staff->>StaffPortal: Click "Prompt Patient for Insurance Info"
        StaffPortal->>API: POST /api/patients/{patientID}/request-insurance
        API->>PatientRepo: CreateInsuranceRequest(patientID)
        PatientRepo->>Database: INSERT INTO PatientTasks (patientID, taskType='ProvideInsurance', createdAt=NOW())
        Database-->>PatientRepo: taskID
        PatientRepo-->>API: taskID
        API-->>StaffPortal: 201 Created {taskID}
        StaffPortal-->>Staff: "Task created. Patient will be prompted to provide insurance info in portal"
    end
```

### UC-008: Staff Reviews 360-Degree Patient View
**Source**: [spec.md#UC-008](.propel/context/docs/spec.md#UC-008)

```mermaid
sequenceDiagram
    participant Staff
    participant StaffPortal as Staff Portal<br/>(Clinical Review)
    participant API as API Controller
    participant ClinicalService as Clinical Data Service
    participant ConsolidatedViewRepo as Consolidated View Repository
    participant ExtractedDataRepo as Extracted Data Repository
    participant Database as PostgreSQL
    participant BlobStorage as Blob Storage<br/>(Supabase)

    Note over Staff,BlobStorage: UC-008 - Staff Reviews 360-Degree Patient View

    Staff->>StaffPortal: Navigate to clinical data prep for appointment
    StaffPortal->>API: GET /api/clinical/patients/{patientID}/360-view
    API->>ClinicalService: Get360DegreePatientView(patientID)
    
    ClinicalService->>ConsolidatedViewRepo: GetConsolidatedView(patientID)
    ConsolidatedViewRepo->>Database: SELECT * FROM ConsolidatedPatientView WHERE patientID={id}
    Database-->>ConsolidatedViewRepo: ConsolidatedView data
    ConsolidatedViewRepo-->>ClinicalService: ViewDTO (medications, allergies, diagnoses, vitals, conflicts)
    
    ClinicalService->>ExtractedDataRepo: GetExtractionSources(patientID)
    ExtractedDataRepo->>Database: SELECT ed.*, cd.documentType, cd.uploadedAt<br/>FROM ExtractedData ed<br/>JOIN ClinicalDocuments cd ON ed.documentID=cd.documentID<br/>WHERE cd.patientID={patientID}<br/>ORDER BY ed.extractedAt DESC
    Database-->>ExtractedDataRepo: Extractions with sources
    ExtractedDataRepo-->>ClinicalService: SourceAttributions
    
    ClinicalService->>ClinicalService: AnnotateWithConfidenceScores(viewDTO, sourceAttributions)
    ClinicalService-->>API: 360ViewDTO with confidence and sources
    API-->>StaffPortal: 200 OK {consolidatedView: {...}, conflicts: [...], sourceMap: {...}}
    
    StaffPortal-->>Staff: Display 360-Degree Patient View<br/>- Timeline view of patient history<br/>- Medications with sources<br/>- Allergies with sources<br/>- Diagnoses with ICD-10 codes<br/>- Recent vitals
    
    alt Conflicts Detected
        StaffPortal-->>Staff: Red warning banner:<br/>"3 conflicts require review before appointment"
        Staff->>StaffPortal: Click "Review Conflicts"
        
        loop For each conflict
            StaffPortal-->>Staff: Display conflict details:<br/>"Document A (Lab Report, 2025-12-01): No allergies<br/>Document B (Referral Note, 2026-01-15): Penicillin allergy"
            
            Staff->>StaffPortal: Click "View Source Document A"
            StaffPortal->>API: GET /api/clinical/documents/{documentID}
            API->>ClinicalService: GetDocumentMetadata(documentID)
            ClinicalService->>Database: SELECT * FROM ClinicalDocuments WHERE documentID={id}
            Database-->>ClinicalService: Document metadata
            ClinicalService-->>API: {filePath, documentType, uploadedAt}
            API-->>StaffPortal: 200 OK {metadata: {...}}
            
            StaffPortal->>API: GET /api/clinical/documents/{documentID}/view
            API->>ClinicalService: GetSignedBlobURL(documentID)
            ClinicalService->>Database: SELECT filePath FROM ClinicalDocuments WHERE documentID={id}
            Database-->>ClinicalService: Encrypted file path
            ClinicalService->>ClinicalService: DecryptFilePath(encryptedPath)
            ClinicalService->>BlobStorage: GenerateSignedURL(filePath, expiresIn=300)
            BlobStorage-->>ClinicalService: Signed URL (5-min expiry)
            ClinicalService-->>API: Signed URL
            API-->>StaffPortal: 200 OK {signedURL: '...'}
            StaffPortal->>StaffPortal: Open PDF in new tab
            StaffPortal-->>Staff: PDF opens at relevant page (page 3)
            
            Staff->>Staff: Review source document and determine correct value
            Staff->>StaffPortal: Select conflict resolution:<br/>"Document B is correct: Penicillin allergy"
            StaffPortal->>API: POST /api/clinical/conflicts/{conflictID}/resolve<br/>{resolution: 'UseDocumentB', notes: 'Lab report outdated'}
            API->>ClinicalService: ResolveConflict(conflictID, resolution)
            
            ClinicalService->>Database: UPDATE ConsolidatedPatientView<br/>SET allergies=jsonb_set(allergies, '{0}', '{"substance": "Penicillin", ...}'),<br/>conflictFlags=jsonb_remove(conflictFlags, '{0}'),<br/>lastRefreshedAt=NOW()
            Database-->>ClinicalService: Conflict resolved
            
            ClinicalService->>Database: INSERT INTO AuditLog<br/>(action='ConflictResolved', staffID, patientID, conflictDetails, resolution)
            Database-->>ClinicalService: Audit logged
            
            ClinicalService-->>API: {conflictResolved: true}
            API-->>StaffPortal: 200 OK {message: "Conflict resolved"}
            StaffPortal-->>Staff: Conflict removed from warning list
        end
        
        Staff->>StaffPortal: All conflicts resolved
        StaffPortal->>API: POST /api/clinical/patients/{patientID}/acknowledge-conflicts
        API->>ClinicalService: AcknowledgeConflictReview(patientID, staffID)
        ClinicalService->>Database: UPDATE Appointments SET conflictsAcknowledged=true<br/>WHERE patientID={patientID} AND DATE(slotDateTime)=CURRENT_DATE
        Database-->>ClinicalService: Success
        ClinicalService-->>API: Success
        API-->>StaffPortal: 200 OK
        StaffPortal-->>Staff: "Conflicts resolved. Patient ready for provider"
    end
    
    alt Low-Confidence Extraction Requires Verification
        StaffPortal-->>Staff: Yellow highlight on medication:<br/>"Aspirin 81mg (Confidence: 65%) - Verify"
        Staff->>StaffPortal: Click "Verify"
        
        StaffPortal->>API: GET /api/clinical/extracted-data/{extractedDataID}
        API->>ExtractedDataRepo: GetExtractedData(extractedDataID)
        ExtractedDataRepo->>Database: SELECT * FROM ExtractedData WHERE extractedDataID={id}
        Database-->>ExtractedDataRepo: ExtractedData details
        ExtractedDataRepo-->>API: ExtractedDataDTO
        API-->>StaffPortal: 200 OK {data: {...}, sourceDocument: {...}}
        
        StaffPortal-->>Staff: Display extraction with source:<br/>"Extracted: Aspirin 81mg<br/>Source: Lab Report, Page 2, Line 15"
        Staff->>StaffPortal: Click "View Source"
        StaffPortal->>BlobStorage: Open PDF at page 2 (same flow as above)
        
        Staff->>Staff: Verify extraction against source
        alt Extraction Correct
            Staff->>StaffPortal: Click "Verified - Correct"
            StaffPortal->>API: POST /api/clinical/extracted-data/{extractedDataID}/verify<br/>{verified: true}
            API->>ExtractedDataRepo: VerifyExtraction(extractedDataID, staffID)
            ExtractedDataRepo->>Database: UPDATE ExtractedData<br/>SET verifiedByStaffID={staffID}, verifiedAt=NOW()
            Database-->>ExtractedDataRepo: Success
            ExtractedDataRepo-->>API: Success
            API-->>StaffPortal: 200 OK
            StaffPortal-->>Staff: Green checkmark: "Verified"
        else Extraction Incorrect
            Staff->>StaffPortal: Click "Correct Extraction"
            StaffPortal->>Staff: Inline editor appears
            Staff->>StaffPortal: Correct value: "Aspirin 100mg"
            StaffPortal->>API: POST /api/clinical/extracted-data/{extractedDataID}/correct<br/>{correctedValue: '100mg'}
            API->>ExtractedDataRepo: CorrectExtraction(extractedDataID, correctedValue, staffID)
            ExtractedDataRepo->>Database: UPDATE ExtractedData<br/>SET value.dosage='100mg', verifiedByStaffID={staffID}, verifiedAt=NOW()
            ExtractedDataRepo->>Database: INSERT INTO AuditLog (action='ExtractionCorrected', staffID, originalValue, correctedValue)
            Database-->>ExtractedDataRepo: Correction saved
            ExtractedDataRepo-->>API: Success
            API-->>StaffPortal: 200 OK
            StaffPortal-->>Staff: "Correction saved. AI feedback logged"
        end
    end
    
    alt No Uploaded Documents
        StaffPortal-->>Staff: "360-Degree View shows intake form data only<br/>No uploaded clinical documents found"
        Staff->>StaffPortal: Note in appointment: "Request patient upload prior medical records"
    end
    
    Staff->>StaffPortal: Review complete, click "Mark Ready for Provider"
    StaffPortal->>API: POST /api/clinical/patients/{patientID}/mark-ready
    API->>ClinicalService: MarkPatientReadyForProvider(patientID, staffID)
    ClinicalService->>Database: UPDATE Appointments SET clinicalDataReviewedBy={staffID}, reviewedAt=NOW()<br/>WHERE patientID={patientID} AND DATE(slotDateTime)=CURRENT_DATE
    Database-->>ClinicalService: Success
    ClinicalService->>Database: INSERT INTO AuditLog (action='ClinicalDataReviewed', staffID, patientID, reviewDurationMinutes)
    Database-->>ClinicalService: Audit logged
    ClinicalService-->>API: Success
    API-->>StaffPortal: 200 OK {message: "Patient ready for provider"}
    StaffPortal-->>Staff: "Clinical data prep complete. Est. time saved: 18 minutes"
```

### UC-009: Admin Manages User Accounts
**Source**: [spec.md#UC-009](.propel/context/docs/spec.md#UC-009)

```mermaid
sequenceDiagram
    participant Admin
    participant AdminPortal as Admin Portal<br/>(User Management UI)
    participant API as API Controller
    participant AdminService as Admin Service
    participant UserRepo as User Repository
    participant Database as PostgreSQL
    participant EmailService as Email Service
    participant NewUser as New User (Staff)

    Note over Admin,NewUser: UC-009 - Admin Manages User Accounts

    rect rgb(230, 245, 255)
    Note over Admin,NewUser: Scenario: Create New User
    
    Admin->>AdminPortal: Navigate to "User Management"
    AdminPortal->>API: GET /api/admin/users
    API->>AdminService: GetAllUsers()
    AdminService->>UserRepo: QueryUsers(includeInactive=false)
    UserRepo->>Database: SELECT * FROM Users WHERE accountStatus='Active' ORDER BY createdAt DESC
    Database-->>UserRepo: User list
    UserRepo-->>AdminService: UserDTOs
    AdminService-->>API: User list
    API-->>AdminPortal: 200 OK {users: [...]}
    AdminPortal-->>Admin: Display user table (Patient, Staff, Admin columns)
    
    Admin->>AdminPortal: Click "Create New User"
    AdminPortal->>Admin: Display user creation form
    Admin->>AdminPortal: Fill form:<br/>- Email: staff@clinic.com<br/>- Name: Jane Smith<br/>- Role: Staff<br/>- Specialization: ClinicalReview
    Admin->>AdminPortal: Click "Create User"
    
    AdminPortal->>API: POST /api/admin/users<br/>{email, name, role, specialization}
    API->>AdminService: CreateUser(userDto)
    
    AdminService->>UserRepo: CheckEmailUniqueness(email)
    UserRepo->>Database: SELECT COUNT(*) FROM Users WHERE email='{email}'
    Database-->>UserRepo: count
    
    alt Email Already Exists
        UserRepo-->>AdminService: EmailExists=true
        AdminService-->>API: 409 Conflict {error: "User with this email already exists"}
        API-->>AdminPortal: 409 Conflict
        AdminPortal-->>Admin: Error: "Email already in use"
    else Email Unique
        AdminService->>AdminService: GenerateTemporaryPassword()
        AdminService->>AdminService: HashPassword(tempPassword)
        
        AdminService->>UserRepo: CreateStaffUser(email, name, hashedPassword, role, specialization)
        UserRepo->>Database: INSERT INTO Staff<br/>(email, name, passwordHash, role='Staff', specialization='ClinicalReview', accountStatus='Active', createdAt=NOW())
        Database-->>UserRepo: staffID
        UserRepo-->>AdminService: User created
        
        AdminService->>UserRepo: AssignRolePermissions(staffID, role='Staff')
        UserRepo->>Database: INSERT INTO UserPermissions (userID, permission) VALUES<br/>('{staffID}', 'ManageWalkIns'),<br/>('{staffID}', 'ReviewClinicalData'),<br/>('{staffID}', 'VerifyExtractions')
        Database-->>UserRepo: Permissions assigned
        
        AdminService->>EmailService: SendActivationEmail(email, tempPassword, activationLink)
        EmailService-->>NewUser: Email: "Account created. Temp password: {pass}. Change on first login"
        
        AdminService->>Database: INSERT INTO AuditLog (action='UserCreated', adminID, newUserID, role)
        Database-->>AdminService: Audit logged
        
        AdminService-->>API: {userID, accountStatus: 'Active', emailSent: true}
        API-->>AdminPortal: 201 Created {userID}
        AdminPortal-->>Admin: "User created! Activation email sent to staff@clinic.com"
        
        AdminPortal->>API: GET /api/admin/users (refresh list)
        API-->>AdminPortal: 200 OK {users: [...]}
        AdminPortal-->>Admin: New user appears in table
    end
    end
    
    rect rgb(255, 245, 230)
    Note over Admin,NewUser: Scenario: Deactivate User
    
    Admin->>AdminPortal: Select user from table
    Admin->>AdminPortal: Click "Deactivate" button
    AdminPortal->>Admin: Confirmation dialog: "Deactivate {userName}? Active sessions will be terminated"
    Admin->>AdminPortal: Confirm
    
    AdminPortal->>API: POST /api/admin/users/{userID}/deactivate
    API->>AdminService: DeactivateUser(userID)
    
    AdminService->>UserRepo: UpdateAccountStatus(userID, 'Inactive')
    UserRepo->>Database: UPDATE Users SET accountStatus='Inactive', deactivatedAt=NOW() WHERE userID={id}
    Database-->>UserRepo: Success
    
    AdminService->>UserRepo: InvalidateActiveSessions(userID)
    UserRepo->>Database: DELETE FROM ActiveSessions WHERE userID={userID}
    Database-->>UserRepo: Sessions cleared
    
    AdminService->>Database: INSERT INTO AuditLog (action='UserDeactivated', adminID, deactivatedUserID)
    Database-->>AdminService: Audit logged
    
    AdminService-->>API: {deactivated: true}
    API-->>AdminPortal: 200 OK {message: "User deactivated"}
    AdminPortal-->>Admin: User status changed to "Inactive" in table
    
    alt User Tries to Login After Deactivation
        NewUser->>API: POST /api/auth/login {email, password}
        API->>AdminService: Authenticate(email, password)
        AdminService->>UserRepo: GetUserByEmail(email)
        UserRepo->>Database: SELECT * FROM Users WHERE email={email}
        Database-->>UserRepo: User data (accountStatus='Inactive')
        UserRepo-->>AdminService: UserDTO
        AdminService-->>API: 403 Forbidden {error: "Account deactivated. Contact administrator"}
        API-->>NewUser: 403 Forbidden
    end
    end
    
    rect rgb(245, 255, 230)
    Note over Admin,NewUser: Scenario: Reset Password
    
    Admin->>AdminPortal: Select user from table
    Admin->>AdminPortal: Click "Reset Password"
    AdminPortal->>Admin: Confirmation: "Send password reset email to {userEmail}?"
    Admin->>AdminPortal: Confirm
    
    AdminPortal->>API: POST /api/admin/users/{userID}/reset-password
    API->>AdminService: ResetUserPassword(userID)
    
    AdminService->>AdminService: GeneratePasswordResetToken()
    AdminService->>UserRepo: StoreResetToken(userID, token, expiresAt=NOW()+1hour)
    UserRepo->>Database: UPDATE Users SET passwordResetToken={token}, resetTokenExpiresAt=NOW() + INTERVAL '1 hour'<br/>WHERE userID={userID}
    Database-->>UserRepo: Success
    
    AdminService->>EmailService: SendPasswordResetEmail(userEmail, resetToken, resetLink)
    EmailService-->>NewUser: Email: "Password reset requested. Click link to reset (expires in 1 hour)"
    
    alt Email Bounces
        EmailService-->>AdminService: EmailBouncedEvent
        AdminService->>Database: INSERT INTO AuditLog (action='PasswordResetEmailBounced', adminID, userID)
        AdminService-->>API: {emailSent: false, bounced: true}
        API-->>AdminPortal: 200 OK {warning: "Email bounced. Provide password manually"}
        AdminPortal-->>Admin: Warning: "Email failed. Generate temp password manually?"
        
        Admin->>AdminPortal: Click "Generate Manual Password"
        AdminPortal->>API: POST /api/admin/users/{userID}/manual-reset
        API->>AdminService: GenerateManualPassword(userID)
        AdminService->>AdminService: GenerateTempPassword()
        AdminService->>AdminService: HashPassword(tempPassword)
        AdminService->>UserRepo: UpdatePassword(userID, hashedPassword)
        UserRepo->>Database: UPDATE Users SET passwordHash={hashedPassword}
        Database-->>UserRepo: Success
        AdminService-->>API: {tempPassword: '...'}
        API-->>AdminPortal: 200 OK {tempPassword: '...'}
        AdminPortal-->>Admin: Display temp password securely:<br/>"Temp password: {pass}. Communicate to user securely"
    else Email Sent Successfully
        AdminService->>Database: INSERT INTO AuditLog (action='PasswordResetInitiated', adminID, userID)
        AdminService-->>API: {emailSent: true}
        API-->>AdminPortal: 200 OK {message: "Reset email sent"}
        AdminPortal-->>Admin: "Reset email sent to {userEmail}"
    end
    end
```

### UC-010: System Automatically Swaps Preferred Slot When Available
**Source**: [spec.md#UC-010](.propel/context/docs/spec.md#UC-010)

```mermaid
sequenceDiagram
    participant System as Slot Monitor Job<br/>(Scheduled Hangfire)
    participant SwapRepo as Swap Request Repository
    participant AppointmentRepo as Appointment Repository
    participant SlotRepo as Slot Repository
    participant Database as PostgreSQL
    participant NotificationService as Notification Service
    participant CalendarAPI as Calendar API<br/>(Google/Outlook)
    participant Patient

    Note over System,Patient: UC-010 - System Automatically Swaps Preferred Slot When Available

    loop Every 5 Minutes
        System->>System: ExecuteSlotMonitor()
        System->>SwapRepo: GetPendingSwapRequests()
        SwapRepo->>Database: SELECT sr.*, a.patientID, a.slotId AS currentSlotId<br/>FROM PreferredSlotSwapRequests sr<br/>JOIN Appointments a ON sr.appointmentID=a.appointmentID<br/>WHERE sr.status='Pending'<br/>AND a.status='Scheduled'<br/>AND sr.preferredSlotDateTime >= NOW() + INTERVAL '24 hours'
        Note over Database: Only swap if ≥24 hours before preferred slot
        Database-->>SwapRepo: Pending swap requests
        SwapRepo-->>System: List of swap requests
        
        loop For each swap request
            System->>SlotRepo: CheckSlotAvailability(preferredSlotDateTime, providerId)
            SlotRepo->>Database: SELECT slotID, isAvailable FROM AppointmentSlots<br/>WHERE slotDateTime={preferredSlotDateTime}<br/>AND providerID={providerId}
            Database-->>SlotRepo: Slot details
            
            alt Preferred Slot Now Available
                SlotRepo-->>System: {slotID, isAvailable=true}
                System->>System: BeginSwapTransaction()
                
                System->>AppointmentRepo: GetAppointment(appointmentID)
                AppointmentRepo->>Database: SELECT * FROM Appointments WHERE appointmentID={id}
                Database-->>AppointmentRepo: Current appointment details
                AppointmentRepo-->>System: AppointmentDTO
                
                System->>System: ValidateSwapEligibility(appointment)
                alt Appointment Canceled Before Swap
                    System->>SwapRepo: UpdateSwapStatus(swapRequestID, 'Cancelled')
                    SwapRepo->>Database: UPDATE PreferredSlotSwapRequests SET status='Cancelled'
                    Database-->>SwapRepo: Success
                    System->>System: SkipToNextSwapRequest()
                end
                
                alt Preferred Slot Less Than 24 Hours Away
                    System->>SwapRepo: UpdateSwapStatus(swapRequestID, 'Expired')
                    SwapRepo->>Database: UPDATE SET status='Expired'
                    Database-->>SwapRepo: Success
                    System->>System: SkipToNextSwapRequest()
                end
                
                System->>AppointmentRepo: UpdateAppointmentSlot(appointmentID, preferredSlotID)
                AppointmentRepo->>Database: UPDATE Appointments<br/>SET slotId={preferredSlotID}, updatedAt=NOW(), rowVersion=rowVersion+1<br/>WHERE appointmentID={appointmentID}
                Database-->>AppointmentRepo: Success
                
                System->>SlotRepo: ReleaseSlot(currentSlotID)
                SlotRepo->>Database: UPDATE AppointmentSlots SET isAvailable=true WHERE slotID={currentSlotID}
                Database-->>SlotRepo: Original slot released
                
                System->>SlotRepo: BookSlot(preferredSlotID)
                SlotRepo->>Database: UPDATE AppointmentSlots SET isAvailable=false WHERE slotID={preferredSlotID}
                Database-->>SlotRepo: Preferred slot booked
                
                System->>SwapRepo: UpdateSwapStatus(swapRequestID, 'Swapped', swappedAt=NOW())
                SwapRepo->>Database: UPDATE PreferredSlotSwapRequests<br/>SET status='Swapped', swappedAt=NOW()
                Database-->>SwapRepo: Success
                
                System->>Database: COMMIT TRANSACTION
                Database-->>System: Transaction committed successfully
                
                System->>NotificationService: SendSwapNotificationEmail(appointmentID, newSlotDetails)
                NotificationService->>NotificationService: FormatEmailTemplate(appointment, oldSlot, newSlot)
                NotificationService->>EmailService: SendEmail(patientEmail, subject, body)
                EmailService-->>Patient: Email: "Good news! We swapped your appointment to {preferredSlotDateTime}"
                
                System->>NotificationService: SendSwapNotificationSMS(patientPhone, newSlotDetails)
                NotificationService->>SMSService: SendSMS(patientPhone, message)
                SMSService-->>Patient: SMS: "Appointment swapped to {preferredDateTime}. See email for details"
                
                System->>CalendarAPI: UpdateCalendarEvent(appointmentID, newSlotDateTime)
                CalendarAPI->>CalendarAPI: AuthenticatePatient(patientID)
                CalendarAPI->>GoogleCalendar: UpdateEvent(calendarEventID, newStartTime, newEndTime)
                GoogleCalendar-->>CalendarAPI: Event updated
                CalendarAPI-->>System: Calendar synced
                
                alt Calendar Sync Fails
                    CalendarAPI-->>System: SyncFailedError
                    System->>Database: UPDATE Appointments SET calendarSyncStatus='FailedUpdate'
                    System->>NotificationService: SendEmail(patientEmail, manualCalendarUpdateInstructions, icsAttachment)
                    NotificationService-->>Patient: Email with .ics file: "Calendar sync failed, import this file manually"
                    System->>Database: INSERT INTO AuditLog (action='CalendarSyncFailed', appointmentID)
                end
                
                System->>Database: INSERT INTO AuditLog<br/>(action='PreferredSlotSwapped', appointmentID, originalSlot, newSlot, swapRequestID, timestamp)
                Database-->>System: Audit logged
                
            else Preferred Slot Still Unavailable
                SlotRepo-->>System: {isAvailable=false}
                System->>System: ContinueMonitoring()
            end
        end
        
        System->>System: Sleep(5 minutes)
    end
    
    alt Appointment Date Arrives Without Swap
        System->>SwapRepo: GetExpiredSwapRequests()
        SwapRepo->>Database: SELECT * FROM PreferredSlotSwapRequests<br/>WHERE status='Pending'<br/>AND (SELECT slotDateTime FROM Appointments WHERE appointmentID=appointmentID) < NOW()
        Database-->>SwapRepo: Expired swap requests
        
        loop For each expired swap
            System->>SwapRepo: UpdateSwapStatus(swapRequestID, 'Expired')
            SwapRepo->>Database: UPDATE PreferredSlotSwapRequests SET status='Expired'
            Database-->>SwapRepo: Success
            System->>Database: INSERT INTO AuditLog (action='SwapRequestExpired', swapRequestID)
        end
    end
```

