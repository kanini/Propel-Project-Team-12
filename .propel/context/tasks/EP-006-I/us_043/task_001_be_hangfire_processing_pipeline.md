# Task - task_001_be_hangfire_processing_pipeline

## Requirement Reference
- User Story: US_043
- Story Location: .propel/context/tasks/EP-006-I/us_043/us_043.md
- Acceptance Criteria:
    - **AC1**: Given a document is uploaded, When the upload completes, Then a background job is enqueued in Hangfire for document processing with the document ID and patient context.
    - **AC2**: Given the processing job runs, When it starts, Then the document status updates to "Processing" and the patient is notified of the status change via Pusher Channels.
    - **AC3**: Given processing completes, When data extraction finishes (or is delegated to EP-006-II), Then the document status updates to "Completed" and processing finishes within 60 seconds for files up to 10MB (NFR-010).
    - **AC4**: Given processing fails, When an error occurs, Then the document status updates to "Failed" with error details, the patient is notified, and the job is marked for manual review.
- Edge Case:
    - What happens when Hangfire is unavailable? Document upload succeeds but processing is delayed; a health check detects the issue and alerts operations.
    - How does the system handle burst uploads (10+ documents simultaneously)? Queue-based processing ensures jobs execute serially per patient, preventing resource exhaustion.

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
| Backend | .NET | 8.0 |
| Backend | ASP.NET Core Web API | 8.0 |
| Database | PostgreSQL | 16.x |
| Library | Hangfire | 1.8.x |
| Library | Hangfire.PostgreSql | 1.20.x |
| Library | Entity Framework Core | 8.0 |
| Library | Pusher .NET Server | 5.x |
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

Implement the Hangfire-based background job processing pipeline for uploaded clinical documents. This task establishes the asynchronous processing infrastructure that decouples document upload from processing, ensuring user interactions remain responsive while documents are processed in the background. The pipeline enqueues jobs immediately after upload, updates document status through each processing phase (Uploaded → Processing → Completed/Failed), broadcasts real-time status changes via Pusher Channels, and handles errors gracefully with retry policies and manual review flagging.

**Key Capabilities:**
- Hangfire job enqueueing after document upload completion
- DocumentProcessingJob with status lifecycle management
- Real-time status notifications via Pusher Channels
- Error handling with automatic retry (3 attempts with exponential backoff)
- Manual review flagging for failed jobs after retry exhaustion
- Queue-based processing to prevent resource exhaustion
- 60-second processing target for 10MB files (NFR-010)
- Graceful degradation when Hangfire unavailable

## Dependent Tasks
- US_042: task_002_be_chunked_upload_api (must complete to trigger job enqueueing)
- US_042: task_003_db_document_schema_migration (ClinicalDocument table must exist)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/DocumentProcessingJob.cs` - Hangfire job implementation
- **NEW**: `src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs` - Processing orchestration
- **NEW**: `src/backend/PatientAccess.Business/Services/IDocumentProcessingService.cs` - Service interface
- **MODIFY**: `src/backend/PatientAccess.Business/Services/DocumentUploadService.cs` - Add job enqueueing after finalize
- **MODIFY**: `src/backend/PatientAccess.Business/Services/PusherService.cs` - Add processing status event methods
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Configure Hangfire with PostgreSQL storage
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add Hangfire connection string

## Implementation Plan

1. **Configure Hangfire with PostgreSQL Storage**
   - Add NuGet packages: `Hangfire.Core`, `Hangfire.AspNetCore`, `Hangfire.PostgreSql`
   - Add Hangfire connection string to `appsettings.json`
   - Register Hangfire services in `Program.cs` with PostgreSQL storage
   - Configure Hangfire dashboard (secure with authorization)
   - Set worker count and poll interval (optimized for document processing)
   - Follow existing background job patterns from `ConfirmationEmailJob.cs`

2. **Create IDocumentProcessingService Interface**
   - Define `ProcessDocumentAsync(Guid documentId)` method signature
   - Define `UpdateDocumentStatusAsync(Guid documentId, string status, string errorMessage = null)` method
   - Follow existing service interface patterns in `PatientAccess.Business/Interfaces/`

3. **Implement DocumentProcessingService**
   - Inject `ApplicationDbContext`, `PusherService`, `ILogger`
   - Implement `ProcessDocumentAsync(documentId)`:
     - Load ClinicalDocument entity from database
     - Update status to "Processing"
     - Trigger Pusher event `processing-started` with document metadata
     - Execute placeholder processing logic (EP-006-II will add AI extraction)
     - Measure processing time (log warning if exceeds 60 seconds)
     - Update status to "Completed" on success
     - Trigger Pusher event `processing-completed`
   - Implement `UpdateDocumentStatusAsync` with atomic database update
   - Handle errors: catch exceptions, log details, update status to "Failed", trigger `processing-failed` event

4. **Create DocumentProcessingJob**
   - Implement static `Execute(Guid documentId)` method for Hangfire
   - Resolve `IDocumentProcessingService` from service provider
   - Call `ProcessDocumentAsync(documentId)`
   - Use structured logging with document ID correlation
   - Implement retry policy: 3 attempts with exponential backoff (1min, 5min, 15min)
   - Add `[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]` attribute
   - On final failure: flag document for manual review (add "RequiresManualReview" field or status)

5. **Enhance DocumentUploadService to Enqueue Jobs**
   - Modify `FinalizeUploadAsync` method
   - After creating ClinicalDocument record and triggering `upload-complete` event
   - Enqueue background job: `BackgroundJob.Enqueue(() => DocumentProcessingJob.Execute(documentId))`
   - Wrap in try-catch: if Hangfire unavailable, log error but don't fail upload
   - Document remains in "Uploaded" status for health check to detect

6. **Enhance PusherService for Processing Events**
   - Add method `TriggerProcessingStartedAsync(documentId, patientId, fileName)`
   - Add method `TriggerProcessingCompletedAsync(documentId, patientId, fileName, processingTimeMs)`
   - Add method `TriggerProcessingFailedAsync(documentId, patientId, fileName, errorMessage)`
   - Use channel name pattern: `patient-{patientId}-documents`
   - Implement graceful degradation (log event if Pusher unavailable)

7. **Implement Queue Management for Burst Uploads**
   - Configure Hangfire queue: `[Queue("document-processing")]` attribute on job
   - Set worker count in `Program.cs` to limit concurrent processing (e.g., 2 workers)
   - Jobs execute serially per queue, preventing resource exhaustion
   - Log queue depth metric for monitoring

8. **Add Comprehensive Error Handling**
   - Wrap processing logic in try-catch blocks
   - Log detailed error information (document ID, patient ID, exception details)
   - Distinguish transient errors (network, timeout) from permanent errors (corrupt file)
   - For transient errors: rely on automatic retry
   - For permanent errors: fail immediately and flag for manual review
   - Store error message in ClinicalDocument.ErrorMessage field

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── AppointmentService.cs
│   │   ├── AuthService.cs
│   │   ├── DocumentUploadService.cs (from US_042)
│   │   ├── PusherService.cs
│   │   └── WaitlistService.cs
│   ├── BackgroundJobs/
│   │   ├── ConfirmationEmailJob.cs
│   │   └── SlotAvailabilityMonitor.cs
│   └── Interfaces/
│       └── IAppointmentService.cs
├── PatientAccess.Web/
│   ├── Controllers/
│   │   ├── AppointmentsController.cs
│   │   ├── AuthController.cs
│   │   ├── DocumentsController.cs (from US_042)
│   │   └── ProvidersController.cs
│   ├── Program.cs
│   └── appsettings.json
└── PatientAccess.Data/
    └── Entities/
        └── ClinicalDocument.cs (from US_042)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/DocumentProcessingJob.cs | Hangfire job for document processing |
| CREATE | src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs | Processing orchestration service |
| CREATE | src/backend/PatientAccess.Business/Services/IDocumentProcessingService.cs | Service interface |
| MODIFY | src/backend/PatientAccess.Business/Services/DocumentUploadService.cs | Add job enqueueing in FinalizeUploadAsync |
| MODIFY | src/backend/PatientAccess.Business/Services/PusherService.cs | Add processing status event methods |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Configure Hangfire with PostgreSQL storage and dashboard |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add Hangfire connection string configuration |
| MODIFY | src/backend/PatientAccess.Data/Entities/ClinicalDocument.cs | Add RequiresManualReview flag (optional) |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Hangfire Documentation
- **Getting Started**: https://docs.hangfire.io/en/latest/getting-started/index.html
- **PostgreSQL Storage**: https://docs.hangfire.io/en/latest/configuration/using-sql-server.html
- **Background Jobs**: https://docs.hangfire.io/en/latest/background-methods/calling-methods-in-background.html
- **Automatic Retry**: https://docs.hangfire.io/en/latest/background-processing/dealing-with-exceptions.html
- **Job Queues**: https://docs.hangfire.io/en/latest/background-processing/processing-jobs-in-queues.html
- **Dashboard Security**: https://docs.hangfire.io/en/latest/configuration/using-dashboard.html#configuring-authorization

### ASP.NET Core Integration
- **Background Tasks**: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
- **Dependency Injection**: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection

### Design Requirements
- **TR-017**: System MUST implement background job processing using Hangfire or equivalent for document processing queue (design.md)
- **NFR-010**: System MUST process uploaded clinical documents within 60 seconds for documents up to 10MB (design.md)

### Existing Codebase Patterns
- **Background Job Pattern**: `src/backend/PatientAccess.Business/BackgroundJobs/ConfirmationEmailJob.cs`
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/AppointmentService.cs`
- **Pusher Service Pattern**: `src/backend/PatientAccess.Business/Services/PusherService.cs`
- **Document Upload Service**: `src/backend/PatientAccess.Business/Services/DocumentUploadService.cs`

## Build Commands
```powershell
# Navigate to backend directory
cd src/backend

# Add Hangfire NuGet packages
dotnet add PatientAccess.Web package Hangfire.Core
dotnet add PatientAccess.Web package Hangfire.AspNetCore
dotnet add PatientAccess.Web package Hangfire.PostgreSql

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
cd PatientAccess.Web
dotnet run
```

## Implementation Validation Strategy
- [ ] Unit tests pass (DocumentProcessingService, status updates, error handling)
- [ ] Integration tests pass (job enqueueing, execution, retry logic)
- [ ] Hangfire dashboard accessible and secured with authorization
- [ ] Background job enqueued successfully after document upload
- [ ] Document status updates correctly (Uploaded → Processing → Completed/Failed)
- [ ] Pusher events broadcast for each status change
- [ ] Processing completes within 60 seconds for 10MB files (performance test)
- [ ] Retry policy executes on transient errors (3 attempts with backoff)
- [ ] Manual review flag set after retry exhaustion
- [ ] Burst upload scenario handled (10+ documents processed serially)
- [ ] Graceful degradation when Hangfire unavailable (upload succeeds, processing delayed)

## Implementation Checklist
- [x] Add Hangfire NuGet packages and configure PostgreSQL storage in Program.cs
- [x] Create IDocumentProcessingService interface with ProcessDocumentAsync method
- [x] Implement DocumentProcessingService with status management and error handling
- [x] Create DocumentProcessingJob with retry policy and manual review flagging
- [x] Modify DocumentUploadService.FinalizeUploadAsync to enqueue background job
- [x] Enhance PusherService with processing status event methods (started, completed, failed)
- [x] Configure Hangfire dashboard with authorization and worker count settings
- [x] Add Hangfire connection string to appsettings.json
