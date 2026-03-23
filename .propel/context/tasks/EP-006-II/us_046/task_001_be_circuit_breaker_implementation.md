# Task - task_001_be_circuit_breaker_implementation

## Requirement Reference
- User Story: US_046
- Story Location: .propel/context/tasks/EP-006-II/us_046/us_046.md
- Acceptance Criteria:
    - **AC1**: Given the Azure AI service is responding slowly, When a request exceeds 30 seconds, Then the circuit breaker triggers (AIR-O02), stops sending requests, and retries with exponential backoff after a cooldown period.
    - **AC2**: Given the circuit breaker is open, When document processing is attempted, Then documents are queued for later processing and the patient is notified that processing is delayed but will resume automatically.
- Edge Case:
    - What happens when the AI service is completely unavailable for extended periods? System gracefully degrades — documents remain in "Processing" state and are retried when service recovers; core platform functions remain operational.
    - How does the system handle circuit breaker state transitions? State transitions (closed → open → half-open) are logged and health check endpoint reports circuit breaker status.

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
| Library | Polly | 8.x |
| Monitoring | Application Insights | Latest |
| Real-time | Pusher .NET Server | 5.x |
| AI/ML | Azure AI Document Intelligence | 4.0 |
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

Implement circuit breaker pattern using Polly library to protect the system from cascading failures when Azure AI Document Intelligence service experiences slowdowns or outages. This task wraps all Azure AI service calls with resilience policies including circuit breaker (30-second timeout, automatic open-circuit trigger), exponential backoff retry strategy (3 attempts with 2s, 4s, 8s delays), timeout policies, and fallback logic. The implementation ensures graceful degradation - documents queue for later processing when circuit is open, health checks expose circuit state, and comprehensive telemetry tracks failures, state transitions, and recovery patterns.

**Key Capabilities:**
- Polly circuit breaker policy with 30-second timeout threshold
- State machine: Closed → Open → Half-Open → Closed
- Exponential backoff retry (3 attempts: 2s, 4s, 8s)
- Failure threshold: open circuit after 5 consecutive failures
- Circuit break duration: 60 seconds before half-open attempt
- Health check endpoint reporting circuit state
- Application Insights integration for failure tracking
- Pusher notifications for delayed processing
- Graceful fallback to Hangfire queue

## Dependent Tasks
- EP-006-II: US_045: task_002_ai_document_intelligence_integration (AzureDocumentIntelligenceService must exist)
- EP-006-I: US_043: task_001_be_hangfire_processing_pipeline (Hangfire queue infrastructure)
- EP-006-I: US_042: task_002_be_chunked_upload_api (PusherService)

## Impacted Components
- **MODIFY**: `src/backend/PatientAccess.Business/Services/AzureDocumentIntelligenceService.cs` - Wrap with Polly policies
- **NEW**: `src/backend/PatientAccess.Business/Resilience/CircuitBreakerPolicy.cs` - Circuit breaker configuration
- **NEW**: `src/backend/PatientAccess.Business/Resilience/IAzureAIResiliencePolicy.cs` - Resilience policy interface
- **NEW**: `src/backend/PatientAccess.Web/HealthChecks/CircuitBreakerHealthCheck.cs` - Health check for circuit state
- **MODIFY**: `src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs` - Handle circuit open state
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register Polly policies and health check
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add circuit breaker configuration
- **NEW**: `src/backend/PatientAccess.Tests/Services/CircuitBreakerTests.cs` - Unit tests

## Implementation Plan

1. **Add Polly NuGet Package**
   - Install `Polly` version 8.x
   - Install `Polly.Extensions.Http` for HTTP client integration
   - Install `Microsoft.Extensions.Diagnostics.HealthChecks` (if not already present)

2. **Create CircuitBreakerPolicy Configuration**
   - Create `IAzureAIResiliencePolicy` interface with method: `IAsyncPolicy<T> GetResiliencePolicyAsync<T>()`
   - Implement `CircuitBreakerPolicy.cs` class:
     - **Timeout Policy**: 30 seconds per request (AIR-O02)
     - **Retry Policy**: Exponential backoff with 3 attempts (2s, 4s, 8s delays)
     - **Circuit Breaker Policy**:
       * Failure threshold: 5 consecutive failures trigger open state
       * Break duration: 60 seconds before half-open attempt
       * Allowed exceptions: HttpRequestException, TaskCanceledException, Azure.RequestFailedException
     - **Fallback Policy**: Log circuit open event, return null result
   - Combine policies using `Policy.WrapAsync` in correct order: Fallback → CircuitBreaker → Retry → Timeout

3. **Add Circuit Breaker Configuration to appsettings.json**
   - Add section:
     ```json
     "CircuitBreaker": {
       "TimeoutSeconds": 30,
       "FailureThreshold": 5,
       "BreakDurationSeconds": 60,
       "RetryAttempts": 3,
       "RetryBaseDelaySeconds": 2
     }
     ```

4. **Wrap AzureDocumentIntelligenceService with Polly Policies**
   - Inject `IAzureAIResiliencePolicy` into `AzureDocumentIntelligenceService`
   - Modify `AnalyzeDocumentAsync` method:
     - Wrap Azure AI SDK call with `resiliencePolicy.ExecuteAsync(() => ...)`
     - Catch `BrokenCircuitException` specifically
     - On BrokenCircuitException: log circuit open, throw custom `CircuitOpenException` for upstream handling
   - Add correlation IDs to all log entries for traceability

5. **Enhance DocumentProcessingService for Circuit Open Handling**
   - Wrap extraction service call in try-catch for `CircuitOpenException`
   - On circuit open:
     - Update document status to "Processing" (NOT "Failed")
     - Set ProcessingNotes: "AI service temporarily unavailable. Processing will resume automatically."
     - Log warning with correlation ID
     - Call PusherService to notify patient: "Document processing delayed due to temporary service issue. Processing will resume shortly."
     - Throw exception to trigger Hangfire retry (will queue for later)
   - Hangfire retry policy (from US_043) handles automatic retry when circuit recovers

6. **Create CircuitBreakerHealthCheck**
   - Implement `IHealthCheck` interface
   - Check circuit breaker state via Polly's `ICircuitBreakerPolicy.CircuitState`
   - Return:
     - **Healthy** if state = Closed
     - **Degraded** if state = HalfOpen
     - **Unhealthy** if state = Open
   - Include metadata: lastFailureTime, failureCount, nextAttemptTime

7. **Register Polly Policies in Program.cs**
   - Register `CircuitBreakerPolicy` as singleton: `services.AddSingleton<IAzureAIResiliencePolicy, CircuitBreakerPolicy>()`
   - Bind configuration: `services.Configure<CircuitBreakerOptions>(configuration.GetSection("CircuitBreaker"))`
   - Register health check: `services.AddHealthChecks().AddCheck<CircuitBreakerHealthCheck>("circuit_breaker")`
   - Map health check endpoint: `app.MapHealthChecks("/health/circuit-breaker")`

8. **Add Telemetry and Monitoring**
   - Log circuit state transitions (Closed → Open, Open → HalfOpen, HalfOpen → Closed)
   - Track metrics in Application Insights:
     - Custom metric: `azure_ai_circuit_breaker_state` (0=Closed, 1=Open, 2=HalfOpen)
     - Custom event: `CircuitBreakerOpened` with failure details
     - Custom event: `CircuitBreakerRecovered` with downtime duration
   - Log all retry attempts with attempt number and delay duration
   - Include document ID in all log entries for correlation

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── AzureDocumentIntelligenceService.cs (from US_045, to be enhanced)
│   │   ├── DocumentProcessingService.cs (from US_043, to be enhanced)
│   │   └── PusherService.cs (from US_042)
│   └── BackgroundJobs/
│       └── DocumentProcessingJob.cs (from US_043)
├── PatientAccess.Web/
│   ├── Program.cs (to be modified)
│   ├── appsettings.json (to be modified)
│   └── HealthChecks/
│       └── HangfireHealthCheck.cs (from US_043)
└── PatientAccess.Tests/
    └── Services/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Resilience/IAzureAIResiliencePolicy.cs | Resilience policy interface |
| CREATE | src/backend/PatientAccess.Business/Resilience/CircuitBreakerPolicy.cs | Polly circuit breaker configuration |
| CREATE | src/backend/PatientAccess.Web/HealthChecks/CircuitBreakerHealthCheck.cs | Health check for circuit state |
| CREATE | src/backend/PatientAccess.Tests/Services/CircuitBreakerTests.cs | Unit tests for circuit breaker behavior |
| MODIFY | src/backend/PatientAccess.Business/Services/AzureDocumentIntelligenceService.cs | Wrap with Polly policies |
| MODIFY | src/backend/PatientAccess.Business/Services/DocumentProcessingService.cs | Handle CircuitOpenException |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register Polly policies and health check |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add CircuitBreaker configuration section |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Polly Documentation
- **Getting Started**: https://www.pollydocs.org/
- **Circuit Breaker**: https://www.pollydocs.org/strategies/circuit-breaker
- **Retry Policy**: https://www.pollydocs.org/strategies/retry
- **Timeout Policy**: https://www.pollydocs.org/strategies/timeout
- **Policy Wrap**: https://www.pollydocs.org/strategies/policy-wrap
- **.NET Integration**: https://www.pollydocs.org/advanced/dependency-injection

### Circuit Breaker Pattern
- **Microsoft Docs**: https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker
- **Azure Well-Architected**: https://learn.microsoft.com/en-us/azure/well-architected/resiliency/patterns

### Design Requirements
- **AIR-O02**: System MUST implement circuit breaker pattern for AI provider failures with 30-second timeout and exponential backoff (design.md)
- **AIR-Q02**: System MUST complete AI inference operations within 5 seconds per document page for 95th percentile requests (design.md)
- **NFR-007**: System MUST log all data access, changes, and authentication events with timestamp, user ID, action, and affected resources for audit compliance (design.md)
- **NFR-011**: System MUST maintain application error rate below 0.1% of requests and log all errors with stack traces to monitoring system (design.md)
- **NFR-013**: System MUST complete AI inference operations within 5 seconds per document page (design.md)

### Existing Codebase Patterns
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/AzureDocumentIntelligenceService.cs`
- **Health Check Pattern**: `src/backend/PatientAccess.Web/HealthChecks/HangfireHealthCheck.cs`
- **Background Job Pattern**: `src/backend/PatientAccess.Business/BackgroundJobs/DocumentProcessingJob.cs`

## Build Commands
```powershell
# Add Polly NuGet packages
cd src/backend
dotnet add PatientAccess.Business package Polly
dotnet add PatientAccess.Business package Polly.Extensions.Http

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
cd PatientAccess.Web
dotnet run

# Test health check endpoint
Invoke-WebRequest -Uri "http://localhost:5000/health/circuit-breaker" -Method Get
```

## Implementation Validation Strategy
- [ ] Unit tests pass (circuit breaker state transitions, retry logic, timeouts)
- [ ] Integration tests pass (Azure AI service call with circuit breaker)
- [ ] Circuit breaker opens after 5 consecutive failures
- [ ] Circuit breaker closes after successful half-open attempt
- [ ] Exponential backoff delays verified (2s, 4s, 8s)
- [ ] Timeout triggers after 30 seconds
- [ ] Health check endpoint returns correct circuit state
- [ ] CircuitOpenException handled correctly in DocumentProcessingService
- [ ] Pusher notification sent when circuit opens
- [ ] Hangfire retry queues document for later processing
- [ ] Application Insights logs circuit state transitions
- [ ] Correlation IDs present in all log entries
- [ ] Circuit recovers automatically after break duration
- [ ] No cascading failures to other system components

## Implementation Checklist
- [ ] Add Polly and Polly.Extensions.Http NuGet packages to PatientAccess.Business
- [ ] Create IAzureAIResiliencePolicy interface
- [ ] Implement CircuitBreakerPolicy with timeout, retry, circuit breaker, and fallback policies
- [ ] Add CircuitBreaker configuration section to appsettings.json
- [ ] Wrap AzureDocumentIntelligenceService.AnalyzeDocumentAsync with Polly policies
- [ ] Enhance DocumentProcessingService to handle CircuitOpenException
- [ ] Create CircuitBreakerHealthCheck implementing IHealthCheck
- [ ] Register Polly policies and health check in Program.cs
- [ ] Add telemetry tracking for circuit state transitions to Application Insights
- [ ] Write unit tests for circuit breaker behavior (open, half-open, closed states)
- [ ] Write integration tests for end-to-end resilience with Azure AI service
- [ ] Validate health check endpoint returns correct circuit state
