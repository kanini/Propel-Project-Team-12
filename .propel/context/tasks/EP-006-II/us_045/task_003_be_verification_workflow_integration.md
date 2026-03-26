# Task - task_003_be_verification_workflow_integration

## Requirement Reference
- User Story: US_045
- Story Location: .propel/context/tasks/EP-006-II/us_045/us_045.md
- Acceptance Criteria:
    - **AC6**: Given data extraction completes, When all data points are stored, Then the document status automatically advances to "ProcessingComplete"
    - **AC7**: Given extracted data has low confidence, When confidence is below 50%, Then the system flags the data point for manual staff review and sets RequiresManualReview = true on the document.
    - **AC8**: Given a document is flagged for review, When staff verifies an extracted data point, Then the system updates VerificationStatus to "Verified", records the staff user ID in VerifiedBy, and records VerifiedAt timestamp (future manual review UI will enable this).
- Edge Case:
    - What happens if the extraction service crashes mid-processing? Hangfire retry mechanism reattempts the job; incomplete extraction data is rolled back via transaction.
    - How does the system handle duplicate extracted data points? Check for existing DataType + Value + SourcePageNumber combination; skip duplicates, log warning.

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
| Background Jobs | Hangfire | 1.8.x |
| Real-time | Pusher .NET Server | 5.x |
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

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Implement verification workflow integration for AI-extracted clinical data. This task enhances the DocumentProcessingService to persist extracted data points to the database, automatically flag low-confidence extractions (<50%) for manual review, update document status to "ProcessingComplete" upon success, send real-time Pusher notifications to frontend, and prepare the foundation for future manual verification by staff. The implementation ensures transactional integrity (rollback on failure), duplicate detection, performance monitoring, and comprehensive error handling aligned with Trust-First AI principles.

**Key Capabilities:**
- Batch persistence of ExtractedClinicalData entities
- Confidence-based manual review flagging (<50% threshold)
- Duplicate detection (DataType + Value + SourcePageNumber)
- Document status lifecycle management (Processing → ProcessingComplete)
- Real-time Pusher progress updates with extraction summary
- Transactional database operations (rollback on failure)
- Background job integration with Hangfire retry policy
- Verification status tracking (Suggested → Verified → Rejected)
- Performance monitoring (log warnings if >30s total processing time)

## Dependent Tasks
- task_001_db_extracted_data_schema (ExtractedClinicalData table)
- task_002_ai_document_intelligence_integration (ClinicalDataExtractionService using Gemini + Tesseract + Supabase)
- EP-006-I: US_043: task_001_be_hangfire_processing_pipeline (DocumentProcessingJob, DocumentProcessingService)
- EP-006-I: US_042: task_002_be_chunked_upload_api (PusherService)

## Impacted Components
- **MODIFY**: `src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs` - Add persistence logic
- **MODIFY**: `src/backend/PatientAccess.Business/BackgroundJobs/DocumentProcessingJob.cs` - Enhance error handling
- **MODIFY**: `src/backend/PatientAccess.Business/Services/PusherService.cs` - Add extraction complete event
- **NEW**: `src/backend/PatientAccess.Business/DTOs/ExtractionSummaryDto.cs` - Summary for Pusher event
- **NEW**: `src/backend/PatientAccess.Tests/Services/DocumentProcessingServiceTests.cs` - Unit tests
- **NEW**: `src/backend/PatientAccess.Tests/Integration/ExtractionWorkflowTests.cs` - Integration tests

## Implementation Plan

1. **Enhance DocumentProcessingService.ProcessDocumentAsync**
   - Wrap entire processing in database transaction (`using var transaction = await context.Database.BeginTransactionAsync()`)
   - Call `ClinicalDataExtractionService.ExtractClinicalDataAsync(documentId)` (which internally uses Supabase + Tesseract + Gemini)
   - Receive `ExtractionResultDto` with extracted data points
   - Iterate through extracted data points and persist to database
   - Implement duplicate detection: check if ExtractedClinicalData exists with same DocumentId, DataType, Value, SourcePageNumber
   - Skip duplicates, log warning with correlation ID
   - Set RequiresManualReview = true on ClinicalDocument if any data point has confidence <50%
   - Update document status to "ProcessingComplete" if extraction succeeds
   - Commit transaction if successful, rollback on exception
   - Calculate total processing time (includes Supabase download, OCR, Gemini inference), log warning if >30 seconds

2. **Implement Batch Persistence Logic**
   - Create list of `ExtractedClinicalData` entities from `ExtractionResultDto`
   - Set initial VerificationStatus = "Suggested" for all data points
   - Set ExtractedAt = DateTime.UtcNow
   - Set CreatedAt = DateTime.UtcNow
   - Add entities to DbContext in batch: `context.ExtractedClinicalData.AddRange(entities)`
   - Call `await context.SaveChangesAsync()` within transaction
   - Return count of persisted data points

3. **Implement Confidence-Based Flagging**
   - After persisting all data points, query for low-confidence items: `context.ExtractedClinicalData.Where(e => e.DocumentId == documentId && e.ConfidenceScore < 50).AnyAsync()`
   - If any low-confidence data points exist:
     - Set `document.RequiresManualReview = true`
     - Set `document.ProcessingNotes = $"Flagged for manual review: {lowConfidenceCount} data points below 50% confidence"`
   - Update document status to "ProcessingComplete" (even if flagged for review)
   - Save changes within transaction

4. **Enhance PusherService with Extraction Complete Event**
   - Add method: `SendExtractionCompleteAsync(int patientId, Guid documentId, ExtractionSummaryDto summary)`
   - Event channel: `private-patient-{patientId}`
   - Event name: `document-extraction-complete`
   - Event payload:
     - documentId
     - status: "ProcessingComplete"
     - totalDataPoints: count of extracted data points
     - flaggedForReview: count of low-confidence data points
     - extractionTimestamp: DateTime.UtcNow
   - Call this method from DocumentProcessingService after successful extraction

5. **Create ExtractionSummaryDto**
   - Properties:
     - Guid DocumentId
     - int TotalDataPoints
     - int FlaggedForReview
     - Dictionary<string, int> DataTypeBreakdown (e.g., "Vital": 5, "Medication": 3)
     - bool RequiresManualReview
     - DateTime ExtractedAt
   - Use this DTO in Pusher event payload

6. **Enhance DocumentProcessingJob Error Handling**
   - Add try-catch around `DocumentProcessingService.ProcessDocumentAsync` call
   - On exception:
     - Update document status to "Failed"
     - Set ProcessingNotes with error details (sanitize stack trace)
     - Send Pusher error event to frontend
     - Log error with correlation ID
     - Throw exception to trigger Hangfire retry (max 3 attempts from US_043)
   - On final retry failure (after 3 attempts):
     - Update document status to "Failed"
     - Set RequiresManualReview = true
     - Do NOT throw exception (prevent infinite retry loop)

7. **Implement Duplicate Detection Logic**
   - Before adding entity to DbContext, check:
     ```csharp
     var exists = await context.ExtractedClinicalData
         .AnyAsync(e => e.DocumentId == documentId 
             && e.DataType == dataPoint.DataType 
             && e.Value == dataPoint.Value 
             && e.SourcePageNumber == dataPoint.SourcePageNumber);
     ```
   - If exists: skip insertion, log warning: `"Duplicate data point detected: {DataType} - {Value} on page {SourcePageNumber}"`
   - Continue processing remaining data points

8. **Add Comprehensive Unit Tests**
   - Test: extraction persists data points with correct fields
   - Test: low-confidence data points trigger RequiresManualReview flag
   - Test: duplicate data points are skipped, no exception thrown
   - Test: transaction rolls back on extraction service exception
   - Test: document status advances to "ProcessingComplete" on success
   - Test: Pusher event sent with correct extraction summary
   - Test: processing time logged if >30 seconds
   - Use in-memory database for fast test execution

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── DocumentProcessingService.cs (from EP-006-I, to be enhanced)
│   │   ├── SupabaseStorageService.cs (from task_002)
│   │   ├── TesseractOcrService.cs (from task_002)
│   │   ├── GeminiAiService.cs (from task_002)
│   │   ├── ClinicalDataExtractionService.cs (from task_002)
│   │   └── PusherService.cs (from EP-006-I, to be enhanced)
│   ├── BackgroundJobs/
│   │   └── DocumentProcessingJob.cs (from EP-006-I, to be enhanced)
│   └── DTOs/
│       ├── ExtractionResultDto.cs (from task_002)
│       ├── ExtractedDataPointDto.cs (from task_002)
│       └── OcrResultDto.cs (from task_002)
├── PatientAccess.Data/
│   └── Entities/
│       ├── ClinicalDocument.cs (from EP-006-I)
│       └── ExtractedClinicalData.cs (from task_001)
└── PatientAccess.Tests/
    └── Services/ (will add tests here)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs | Add persistence and flagging logic |
| MODIFY | src/backend/PatientAccess.Business/BackgroundJobs/DocumentProcessingJob.cs | Enhance error handling |
| MODIFY | src/backend/PatientAccess.Business/Services/PusherService.cs | Add extraction complete event |
| CREATE | src/backend/PatientAccess.Business/DTOs/ExtractionSummaryDto.cs | Summary DTO for Pusher |
| CREATE | src/backend/PatientAccess.Tests/Services/DocumentProcessingServiceTests.cs | Unit tests |
| CREATE | src/backend/PatientAccess.Tests/Integration/ExtractionWorkflowTests.cs | Integration tests |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Entity Framework Core Documentation
- **Transactions**: https://learn.microsoft.com/en-us/ef/core/saving/transactions
- **Batch Operations**: https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating#bulk-operations
- **Concurrency**: https://learn.microsoft.com/en-us/ef/core/saving/concurrency
- **Testing**: https://learn.microsoft.com/en-us/ef/core/testing/

### Design Requirements
- **FR-029**: System MUST flag AI-extracted data points with confidence <50% for manual staff review (spec.md)
- **FR-030**: System MUST allow staff to verify or reject AI-suggested clinical data (spec.md, future manual review UI)
- **NFR-013**: System MUST provide sub-5-second feedback to users for standard interactions including AI inference (design.md)
- **NFR-017**: System MUST support concurrent use by 50 users with response time degradation <10% (design.md)
- **AIR-003**: System MUST implement Trust-First design pattern: all AI-suggested clinical data marked as "suggested" until staff verification (design.md)
- **AIR-008**: System MUST provide confidence scores (0-100%) for all AI-suggested clinical data (design.md)

### Existing Codebase Patterns
- **Background Job Pattern**: `src/backend/PatientAccess.Business/BackgroundJobs/DocumentProcessingJob.cs`
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs`
- **Pusher Event Pattern**: `src/backend/PatientAccess.Business/Services/PusherService.cs`
- **Supabase Storage Pattern**: `src/backend/PatientAccess.Business/Services/SupabaseStorageService.cs`
- **OCR Pattern**: `src/backend/PatientAccess.Business/Services/TesseractOcrService.cs`
- **Gemini AI Pattern**: `src/backend/PatientAccess.Business/Services/GeminiAiService.cs`

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build

# Run unit tests
dotnet test --filter "FullyQualifiedName~DocumentProcessingServiceTests"

# Run integration tests
dotnet test --filter "FullyQualifiedName~ExtractionWorkflowTests"

# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run application
cd PatientAccess.Web
dotnet run
```

## Implementation Validation Strategy
- [ ] Unit tests pass (persistence, flagging, duplicate detection, transaction rollback)
- [ ] Integration tests pass (end-to-end extraction workflow)
- [ ] Extracted data persists to ExtractedClinicalData table with correct fields
- [ ] Low-confidence data points (<50%) trigger RequiresManualReview flag
- [ ] Duplicate data points are skipped without errors
- [ ] Transaction rolls back on extraction service exception
- [ ] Document status advances to "ProcessingComplete" on success
- [ ] Pusher event sent with extraction summary (totalDataPoints, flaggedForReview)
- [ ] Processing time monitored (log warning if >30s)
- [ ] Hangfire retry policy works correctly (3 attempts)
- [ ] Error handling prevents infinite retry loops
- [ ] Verification status defaults to "Suggested" for all new data points

## Implementation Checklist
- [ ] Wrap DocumentProcessingService logic in database transaction with rollback
- [ ] Implement batch persistence of ExtractedClinicalData entities
- [ ] Add duplicate detection logic (DataType + Value + SourcePageNumber)
- [ ] Implement confidence-based flagging (RequiresManualReview = true if <50%)
- [ ] Update document status to "ProcessingComplete" on success
- [ ] Enhance PusherService with SendExtractionCompleteAsync method
- [ ] Create ExtractionSummaryDto with data type breakdown
- [ ] Enhance DocumentProcessingJob error handling (retry vs. final failure)
- [ ] Add unit tests for persistence, flagging, duplicates, transactions
- [ ] Add integration tests for end-to-end extraction workflow
- [ ] Verify processing time monitoring logs warnings if >30 seconds
- [ ] Validate Pusher event payload contains correct extraction summary
