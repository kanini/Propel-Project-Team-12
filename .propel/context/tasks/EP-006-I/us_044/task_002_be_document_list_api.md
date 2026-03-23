# Task - task_002_be_document_list_api

## Requirement Reference
- User Story: US_044
- Story Location: .propel/context/tasks/EP-006-I/us_044/us_044.md
- Acceptance Criteria:
    - **AC1**: Given I have uploaded documents, When I navigate to the Document Status page, Then all my documents are listed with name, upload date, file size, and current processing status.
    - **AC3**: Given a document has completed processing, When I view its status, Then a "View Extracted Data" link appears allowing me to see the data extracted from the document.
- Edge Case:
    - How does the system handle documents that fail extraction? Status shows "Failed" with a "Retry" button that re-queues the processing job.

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
| Library | Entity Framework Core | 8.0 |
| Library | Hangfire | 1.8.x |
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

Implement REST API endpoints for document status retrieval and retry functionality. This task creates two endpoints: GET /api/documents (returns all documents for authenticated user with status and metadata) and POST /api/documents/{id}/retry (re-enqueues failed document for processing). The implementation ensures proper authorization (users can only access their own documents), efficient database queries with eager loading, and integration with Hangfire for job re-enqueueing.

**Key Capabilities:**
- Fetch all documents for authenticated user with filtering and sorting
- Return document metadata including processing time and status
- Calculate "stuck processing" indicator (processing >5 minutes)
- Re-enqueue failed documents for processing via Hangfire
- Prevent retry of non-failed documents (validation)
- Audit logging for retry actions
- Efficient database queries with pagination support

## Dependent Tasks
- US_042: task_003_db_document_schema_migration (ClinicalDocument table must exist)
- US_043: task_001_be_hangfire_processing_pipeline (DocumentProcessingJob must exist for retry)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/DTOs/DocumentStatusDto.cs` - Document status response DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/RetryProcessingRequestDto.cs` - Retry request DTO (optional)
- **MODIFY**: `src/backend/PatientAccess.Web/Controllers/DocumentsController.cs` - Add GET and POST retry endpoints
- **MODIFY**: `src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs` - Add retry logic

## Implementation Plan

1. **Create DocumentStatusDto**
   - Define properties: Id (Guid), FileName (string), UploadedAt (DateTime), FileSize (long), Status (string), ProcessingTime (int?, milliseconds), ErrorMessage (string, nullable), IsStuckProcessing (bool)
   - Add computed property for human-readable file size
   - Follow existing DTO patterns in `PatientAccess.Business/DTOs/`

2. **Add GET /api/documents Endpoint to DocumentsController**
   - Add `[HttpGet]` endpoint with `[Authorize]` attribute
   - Extract current user ID from JWT claims
   - Query ClinicalDocument table filtered by UploadedBy = userId
   - Include eager loading for Patient navigation property (if needed)
   - Order by UploadedAt descending (newest first)
   - Map entities to DocumentStatusDto
   - Calculate IsStuckProcessing: Status == "Processing" AND (DateTime.UtcNow - UploadedAt) > 5 minutes
   - Return 200 OK with List<DocumentStatusDto>
   - Handle errors: return 500 Internal Server Error with error details

3. **Add POST /api/documents/{id}/retry Endpoint**
   - Add `[HttpPost("{id}/retry")]` endpoint with `[Authorize]` attribute
   - Extract current user ID from JWT claims
   - Load ClinicalDocument by ID
   - Validate: document exists, belongs to current user, status == "Failed"
   - If validation fails: return 400 Bad Request or 404 Not Found
   - Update document status to "Uploaded" (ready for reprocessing)
   - Clear ErrorMessage field
   - Enqueue background job: `BackgroundJob.Enqueue(() => DocumentProcessingJob.Execute(documentId))`
   - Log audit event: "Document {documentId} retry initiated by user {userId}"
   - Return 200 OK with updated DocumentStatusDto
   - Handle errors: return 500 Internal Server Error

4. **Enhance DocumentProcessingService for Retry Support**
   - Add method `RetryProcessingAsync(Guid documentId, Guid userId)`
   - Validate document ownership and status
   - Update status and enqueue job (same logic as endpoint)
   - Return success/failure result
   - Follow existing service patterns

5. **Add Authorization and Ownership Validation**
   - Ensure users can only retrieve their own documents
   - Prevent users from retrying documents uploaded by others
   - Use existing JWT claims extraction pattern from other controllers
   - Log unauthorized access attempts

6. **Optimize Database Queries**
   - Use `.AsNoTracking()` for read-only GET endpoint (performance optimization)
   - Add index on UploadedBy column (if not already exists)
   - Consider pagination for large document lists (optional enhancement)
   - Measure query performance with > 1000 documents

7. **Add Comprehensive Logging**
   - Log document fetch requests with user ID and result count
   - Log retry attempts with document ID and user ID
   - Log validation failures (retry on non-failed document)
   - Use structured logging with correlation IDs

8. **Implement Error Handling**
   - Wrap all operations in try-catch blocks
   - Return appropriate HTTP status codes (400, 404, 500)
   - Include actionable error messages in response
   - Don't expose internal implementation details in error messages

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── DTOs/
│   │   ├── DocumentUploadResponseDto.cs (from US_042)
│   │   └── InitializeUploadRequestDto.cs (from US_042)
│   ├── Services/
│   │   ├── DocumentProcessingService.cs (from US_043)
│   │   └── DocumentUploadService.cs (from US_042)
│   └── BackgroundJobs/
│       └── DocumentProcessingJob.cs (from US_043)
├── PatientAccess.Web/
│   ├── Controllers/
│   │   ├── AppointmentsController.cs
│   │   ├── DocumentsController.cs (from US_042)
│   │   └── AuthController.cs
│   └── Program.cs
└── PatientAccess.Data/
    └── Entities/
        └── ClinicalDocument.cs (from US_042)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/DTOs/DocumentStatusDto.cs | Document status response DTO |
| MODIFY | src/backend/PatientAccess.Web/Controllers/DocumentsController.cs | Add GET list and POST retry endpoints |
| MODIFY | src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs | Add RetryProcessingAsync method |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### ASP.NET Core Documentation
- **Controller Actions**: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions
- **Authorization**: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple
- **Model Validation**: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation

### Entity Framework Core
- **AsNoTracking**: https://learn.microsoft.com/en-us/ef/core/querying/tracking#no-tracking-queries
- **Eager Loading**: https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager

### Hangfire
- **Enqueue Jobs**: https://docs.hangfire.io/en/latest/background-methods/calling-methods-in-background.html

### Design Requirements
- **FR-029**: System MUST allow patients to track processing status of uploaded clinical documents with real-time updates (spec.md)

### Existing Codebase Patterns
- **Controller Pattern**: `src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs`
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/DocumentUploadService.cs`
- **DTO Pattern**: `src/backend/PatientAccess.Business/DTOs/DocumentUploadResponseDto.cs`

## Build Commands
```powershell
# Navigate to backend directory
cd src/backend

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
- [ ] Unit tests pass (DocumentProcessingService retry logic, ownership validation)
- [ ] Integration tests pass (GET endpoint returns correct documents, POST retry works)
- [ ] GET /api/documents returns only authenticated user's documents
- [ ] Documents sorted by upload date descending
- [ ] IsStuckProcessing correctly identifies processing >5 minutes
- [ ] POST retry endpoint validates document status (only Failed documents)
- [ ] POST retry endpoint validates ownership (prevents unauthorized retry)
- [ ] Hangfire job enqueued successfully on retry
- [ ] Audit logging records retry attempts
- [ ] Error handling returns appropriate status codes (400, 404, 500)

## Implementation Checklist
- [ ] Create DocumentStatusDto with all required properties and computed fields
- [ ] Add GET /api/documents endpoint with authorization and ownership filtering
- [ ] Implement efficient database query with AsNoTracking and ordering
- [ ] Calculate IsStuckProcessing flag based on processing time
- [ ] Add POST /api/documents/{id}/retry endpoint with validation
- [ ] Implement RetryProcessingAsync in DocumentProcessingService
- [ ] Add authorization and ownership validation for all endpoints
- [ ] Implement comprehensive error handling and logging
