# Task - task_002_be_queue_burst_handling

## Requirement Reference
- User Story: US_046
- Story Location: .propel/context/tasks/EP-006-II/us_046/us_046.md
- Acceptance Criteria:
    - **AC2**: Given the circuit breaker is open, When document processing is attempted, Then documents are queued for later processing and the patient is notified that processing is delayed but will resume automatically.
    - **AC3**: Given burst uploads occur (multiple documents simultaneously), When the queue receives 10+ documents, Then queue-based processing (AIR-O04) ensures orderly processing without overwhelming the AI service.
- Edge Case:
    - What happens when the AI service is completely unavailable for extended periods? System gracefully degrades — documents remain in "Processing" state and are retried when service recovers; core platform functions remain operational.

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
| Monitoring | Application Insights | Latest |
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

Enhance Hangfire queue infrastructure to handle burst document uploads and circuit breaker failures gracefully. This task implements queue-based resilience patterns including: burst throttling (max 3 concurrent document processing jobs to avoid overwhelming Azure AI), priority queue support (retry jobs have higher priority), backoff delay configuration (exponential delays for retried jobs: 5min, 15min, 30min), queue depth monitoring (alert when >50 documents pending), and automatic retry scheduling when circuit breaker recovers. The implementation ensures the system remains operational during AI service outages while protecting against resource exhaustion during burst upload scenarios.

**Key Capabilities:**
- Hangfire queue concurrency limits (max 3 concurrent extraction jobs)
- Priority queue for retry vs. new document processing
- Exponential backoff for retried jobs (5min, 15min, 30min)
- Queue depth monitoring with Application Insights alerts
- Automatic job re-enqueue on circuit breaker failure
- Pusher notifications for queue status updates
- Background cleanup job for stuck documents (>15 min in Processing)
- Rate limiting integration (prevent burst overload)

## Dependent Tasks
- EP-006-II: US_046: task_001_be_circuit_breaker_implementation (CircuitOpenException handling)
- EP-006-I: US_043: task_001_be_hangfire_processing_pipeline (Hangfire infrastructure)
- EP-006-I: US_042: task_002_be_chunked_upload_api (PusherService, document upload)

## Impacted Components
- **MODIFY**: `src/backend/PatientAccess.Business/BackgroundJobs/DocumentProcessingJob.cs` - Add priority and retry logic
- **NEW**: `src/backend/PatientAccess.Business/Services/IDocumentQueueService.cs` - Queue management interface
- **NEW**: `src/backend/PatientAccess.Business/Services/DocumentQueueService.cs` - Queue operations and monitoring
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/QueueMonitoringJob.cs` - Queue health monitoring
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Configure Hangfire queue limits
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add queue configuration
- **NEW**: `src/backend/PatientAccess.Tests/Services/DocumentQueueServiceTests.cs` - Unit tests

## Implementation Plan

1. **Configure Hangfire Queue Concurrency Limits**
   - Modify `Program.cs` Hangfire configuration:
     - Set `WorkerCount = 3` for document extraction queue (prevent burst overload to Azure AI)
     - Create separate queues: `"extraction"` (normal priority), `"extraction-retry"` (high priority)
     - Configure priority ordering: retry queue processed before normal queue
   - Update appsettings.json:
     ```json
     "Hangfire": {
       "WorkerCount": 3,
       "Queues": ["extraction-retry", "extraction"],
       "RetryDelayMinutes": [5, 15, 30]
     }
     ```

2. **Create DocumentQueueService**
   - Implement `IDocumentQueueService` interface with methods:
     - `EnqueueDocumentAsync(Guid documentId, int priority = 0)` - Queue document for processing
     - `EnqueueWithDelayAsync(Guid documentId, TimeSpan delay)` - Queue with exponential backoff
     - `GetQueueDepthAsync()` - Return count of pending jobs
     - `GetQueueStatsAsync()` - Return stats (processing, pending, failed counts)
   - Use Hangfire `BackgroundJob.Enqueue` and `BackgroundJob.Schedule` APIs
   - Track queue priority: 0=normal, 1=retry, 2=manual retry

3. **Enhance DocumentProcessingJob with Retry Logic**
   - Add job attributes: `[Queue("extraction")]` for normal, `[Queue("extraction-retry")]` for retries
   - Modify `ProcessDocumentAsync` method:
     - Catch `CircuitOpenException` from task_001
     - On circuit open:
       * Calculate retry attempt from job context (1st, 2nd, 3rd)
       * Apply exponential backoff: 5min (1st), 15min (2nd), 30min (3rd)
       * Call `DocumentQueueService.EnqueueWithDelayAsync(documentId, delay)`
       * Update document ProcessingNotes: "Queued for retry due to service unavailability. Attempt {N}/3."
       * Send Pusher notification: "Document processing delayed. Will automatically retry in {X} minutes."
     - Log retry scheduling with correlation ID

4. **Implement Burst Upload Throttling**
   - In `DocumentUploadService.CompleteUploadAsync` (from US_042):
     - Check current queue depth via `DocumentQueueService.GetQueueDepthAsync()`
     - If queue depth > 50:
       * Log warning: "Queue depth exceeded threshold, delaying new job"
       * Add 1-minute delay before enqueuing: `EnqueueWithDelayAsync(documentId, TimeSpan.FromMinutes(1))`
       * Send Pusher notification: "Document uploaded. Processing will begin shortly due to high volume."
   - This prevents burst uploads from overwhelming the queue

5. **Create QueueMonitoringJob**
   - Implement background job that runs every 5 minutes
   - Tasks:
     - Query queue depth via Hangfire API: `JobStorage.Current.GetMonitoringApi()`
     - Track metrics in Application Insights:
       * `document_queue_depth` (count of pending jobs)
       * `document_queue_processing` (count of active jobs)
       * `document_queue_failed` (count of failed jobs)
     - Alert if queue depth > 100 (critical threshold)
     - Identify stuck documents (Status="Processing", UpdatedAt >15 min ago):
       * Update status to "Failed"
       * Set RequiresManualReview = true
       * Log stuck document IDs for investigation
   - Schedule via Hangfire recurring job: `RecurringJob.AddOrUpdate("queue-monitoring", () => queueMonitor.MonitorAsync(), "*/5 * * * *")`

6. **Add Priority Queue Logic**
   - Modify `DocumentProcessingJob` to check job priority from context
   - Retry jobs (from circuit breaker failures) use `extraction-retry` queue
   - Normal jobs (from uploads) use `extraction` queue
   - Hangfire processes `extraction-retry` before `extraction` (configured in Program.cs)
   - Log priority in job metadata for observability

7. **Implement Queue Stats Endpoint**
   - Add controller endpoint: `GET /api/admin/queue/stats`
   - Requires Admin role authorization
   - Return JSON:
     ```json
     {
       "queueDepth": 42,
       "processingCount": 3,
       "failedCount": 2,
       "averageWaitTimeMinutes": 5.2,
       "oldestJobCreatedAt": "2026-03-23T10:15:00Z"
     }
     ```
   - Use Hangfire monitoring API for data retrieval

8. **Add Comprehensive Telemetry**
   - Log all queue operations:
     - Job enqueued with document ID, priority, delay
     - Job started processing with worker ID
     - Job completed successfully or failed with reason
     - Circuit breaker triggered job retry
     - Queue depth alerts (>50, >100)
   - Track custom Application Insights events:
     - `DocumentQueuedForRetry` (reason, attempt number, delay)
     - `QueueDepthExceeded` (depth, threshold)
     - `StuckDocumentDetected` (document ID, age in minutes)

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── DocumentProcessingService.cs (from US_043)
│   │   ├── DocumentUploadService.cs (from US_042)
│   │   └── PusherService.cs (from US_042)
│   └── BackgroundJobs/
│       └── DocumentProcessingJob.cs (from US_043, to be enhanced)
├── PatientAccess.Web/
│   ├── Program.cs (to be modified)
│   ├── appsettings.json (to be modified)
│   └── Controllers/
│       └── DocumentsController.cs (from US_042)
└── PatientAccess.Tests/
    └── Services/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/IDocumentQueueService.cs | Queue management interface |
| CREATE | src/backend/PatientAccess.Business/Services/DocumentQueueService.cs | Queue operations and stats |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/QueueMonitoringJob.cs | Queue health monitoring |
| CREATE | src/backend/PatientAccess.Web/Controllers/AdminController.cs | Queue stats endpoint |
| CREATE | src/backend/PatientAccess.Tests/Services/DocumentQueueServiceTests.cs | Unit tests for queue service |
| MODIFY | src/backend/PatientAccess.Business/BackgroundJobs/DocumentProcessingJob.cs | Add retry logic and priority |
| MODIFY | src/backend/PatientAccess.Business/Services/DocumentUploadService.cs | Add burst throttling check |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Configure Hangfire queues and worker count |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add Hangfire queue configuration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Hangfire Documentation
- **Background Jobs**: https://docs.hangfire.io/en/latest/background-methods/index.html
- **Queue Priority**: https://docs.hangfire.io/en/latest/background-processing/configuring-queues.html
- **Recurring Jobs**: https://docs.hangfire.io/en/latest/background-methods/performing-recurrent-tasks.html
- **Monitoring API**: https://docs.hangfire.io/en/latest/configuration/using-monitoring-api.html
- **Job Scheduling**: https://docs.hangfire.io/en/latest/background-methods/scheduling-jobs.html

### Queue Design Patterns
- **Microsoft Docs Queue-Based Load Leveling**: https://learn.microsoft.com/en-us/azure/architecture/patterns/queue-based-load-leveling
- **Circuit Breaker with Queue**: https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker#retry-pattern

### Design Requirements
- **AIR-O02**: System MUST implement circuit breaker pattern for AI provider failures with 30-second timeout and exponential backoff (design.md)
- **AIR-O04**: System MUST queue document processing requests to handle burst uploads without degrading real-time operations (design.md)
- **TR-017**: System MUST implement background job processing using Hangfire for document processing queue (design.md)
- **NFR-011**: System MUST maintain application error rate below 0.1% of requests and log all errors (design.md)
- **NFR-017**: System MUST support concurrent use by 50 users with response time degradation <10% (design.md)

### Existing Codebase Patterns
- **Background Job Pattern**: `src/backend/PatientAccess.Business/BackgroundJobs/DocumentProcessingJob.cs`
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs`
- **Controller Pattern**: `src/backend/PatientAccess.Web/Controllers/DocumentsController.cs`

## Build Commands
```powershell
# Build solution
cd src/backend
dotnet build

# Run tests
dotnet test

# Run application
cd PatientAccess.Web
dotnet run

# Test queue stats endpoint (requires Admin auth)
$token = "your-admin-jwt-token"
Invoke-WebRequest -Uri "http://localhost:5000/api/admin/queue/stats" -Method Get -Headers @{ Authorization = "Bearer $token" }
```

## Implementation Validation Strategy
- [ ] Unit tests pass (queue enqueue, retry logic, backoff calculation)
- [ ] Integration tests pass (burst upload handling, circuit breaker retry)
- [ ] Hangfire processes max 3 concurrent extraction jobs
- [ ] Retry jobs processed before normal jobs (priority queue)
- [ ] Exponential backoff delays correct (5min, 15min, 30min)
- [ ] Queue depth monitoring logs alerts when >50 documents
- [ ] Stuck documents (>15 min) detected and failed automatically
- [ ] Burst uploads (>50 documents) throttled with 1-minute delay
- [ ] CircuitOpenException triggers automatic retry with delay
- [ ] Pusher notifications sent for delayed processing
- [ ] Queue stats endpoint returns correct counts
- [ ] Application Insights logs queue metrics
- [ ] System remains responsive during burst uploads
- [ ] No resource exhaustion during extended AI outages

## Implementation Checklist
- [ ] Configure Hangfire WorkerCount=3 and separate queues in Program.cs
- [ ] Add Hangfire queue configuration to appsettings.json
- [ ] Create IDocumentQueueService interface
- [ ] Implement DocumentQueueService with enqueue and stats methods
- [ ] Enhance DocumentProcessingJob with retry logic and exponential backoff
- [ ] Add burst throttling check to DocumentUploadService.CompleteUploadAsync
- [ ] Create QueueMonitoringJob for queue health and stuck document detection
- [ ] Add GET /api/admin/queue/stats endpoint with Admin authorization
- [ ] Register QueueMonitoringJob as Hangfire recurring job
- [ ] Add telemetry tracking for queue operations to Application Insights
- [ ] Write unit tests for DocumentQueueService
- [ ] Write integration tests for burst upload and retry scenarios
