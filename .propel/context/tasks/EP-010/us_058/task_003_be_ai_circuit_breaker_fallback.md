# Task - task_003_be_ai_circuit_breaker_fallback

## Requirement Reference
- User Story: US_058
- Story Location: .propel/context/tasks/EP-010/us_058/us_058.md
- Acceptance Criteria:
    - **AC3**: Given AI service unavailability (AIR-O03), When Azure OpenAI or Document Intelligence service is unavailable, Then the system falls back to manual workflow within 5 seconds, displays a user-friendly notice, and queues the document for retry when services recover.
    - **Edge Case 1**: Given AI model returns empty/malformed response, When the system receives invalid AI output, Then it discards the response, logs the incident, increments error counter, and routes to manual processing workflow.

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
| Frontend | N/A | N/A |
| Backend | ASP.NET Core | 8.0 |
| Backend | C# | 12.0 |
| Backend | Polly | 8.x |
| Backend | Hangfire | 1.8.x |
| Backend | Azure.AI.OpenAI | 2.0+ |
| Backend | Azure.AI.FormRecognizer | 4.1+ |
| Caching | StackExchange.Redis | 2.8+ |
| Database | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | Azure OpenAI | GPT-4o |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-O03, NFR-015 |
| **AI Pattern** | Circuit breaker, Progressive degradation, Fallback to manual |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | appsettings.json (CircuitBreakerThresholds, RetryPolicy) |
| **Model Provider** | Azure OpenAI (GPT-4o), Azure AI Document Intelligence |

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

Implement circuit breaker pattern using Polly to provide graceful degradation when Azure OpenAI or Document Intelligence services are unavailable (AIR-O03, NFR-015, AD-006). This task creates resilient AI service wrappers with circuit breaker policies that detect failures, open circuit after threshold breaches, and fallback to manual workflow within 5 seconds (AC3). Includes DocumentRetryQueue using Redis to queue failed documents for automatic replay when services recover, ProcessRetryQueueJob Hangfire background job for retry processing, AIServiceHealthCheck for monitoring circuit state, and comprehensive error handling for malformed AI responses (Edge Case 1). Implements progressive AI degradation architecture per AD-006 ensuring full system functionality during AI outages.

**Key Capabilities:**
- Circuit breaker pattern with Polly (Closed/Open/Half-Open states)
- Fallback to manual workflow within 5 seconds (AC3)
- User-friendly notice when AI unavailable
- DocumentRetryQueue stores failed documents in Redis with retry metadata
- ProcessRetryQueueJob replays queued documents when circuit closes
- AIServiceHealthCheck monitors circuit breaker state
- Error handling for empty/malformed AI responses (Edge Case 1)
- Progressive degradation: AI unavailable → manual workflow available
- Configurable failure thresholds and retry policies

## Dependent Tasks
- None (independent infrastructure task)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/ResilientAIService.cs` - Circuit breaker wrapper
- **NEW**: `src/backend/PatientAccess.Business/Services/DocumentRetryQueue.cs` - Redis-based retry queue
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/ProcessRetryQueueJob.cs` - Hangfire retry processor
- **NEW**: `src/backend/PatientAccess.Web/HealthChecks/AIServiceHealthCheck.cs` - Circuit state monitoring
- **NEW**: `docs/AI_CIRCUIT_BREAKER.md` - Circuit breaker documentation
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Configure Polly policies, register services
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add circuit breaker configuration

## Implementation Plan

1. **Configure Polly Circuit Breaker Policies**
   - File: `src/backend/PatientAccess.Web/Program.cs`
   - Circuit breaker configuration:
     ```csharp
     using Polly;
     using Polly.CircuitBreaker;
     using Polly.Extensions.Http;
     
     // Circuit Breaker Policy for Azure OpenAI (AC3 - US_058)
     var openAICircuitBreakerPolicy = Policy
         .Handle<HttpRequestException>()
         .Or<TaskCanceledException>()
         .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
         .CircuitBreakerAsync(
             handledEventsAllowedBeforeBreaking: 5,  // Open circuit after 5 consecutive failures
             durationOfBreak: TimeSpan.FromSeconds(30) // Stay open for 30 seconds before Half-Open
         );
     
     // Timeout Policy (5 seconds per AC3 - AIR-O03)
     var openAITimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
         TimeSpan.FromSeconds(5),
         Polly.Timeout.TimeoutStrategy.Pessimistic
     );
     
     // Retry Policy (exponential backoff)
     var openAIRetryPolicy = HttpPolicyExtensions
         .HandleTransientHttpError()
         .WaitAndRetryAsync(
             retryCount: 3,
             sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
             onRetry: (outcome, timespan, retryAttempt, context) =>
             {
                 // Log retry attempts
             }
         );
     
     // Combine policies: Timeout -> Retry -> Circuit Breaker
     var openAIPolicyWrap = Policy.WrapAsync(
         openAICircuitBreakerPolicy,
         openAIRetryPolicy,
         openAITimeoutPolicy
     );
     
     // Apply to HttpClient for Azure OpenAI
     builder.Services.AddHttpClient("AzureOpenAI", client =>
     {
         client.BaseAddress = new Uri(builder.Configuration["AzureOpenAI:Endpoint"]);
         client.DefaultRequestHeaders.Add("api-key", builder.Configuration["AzureOpenAI:ApiKey"]);
         client.Timeout = TimeSpan.FromSeconds(10); // Overall timeout
     })
     .AddPolicyHandler(openAIPolicyWrap);
     
     // Similar circuit breaker for Azure Document Intelligence
     var documentIntelligenceCircuitBreaker = Policy
         .Handle<HttpRequestException>()
         .Or<TaskCanceledException>()
         .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
         .CircuitBreakerAsync(
             handledEventsAllowedBeforeBreaking: 5,
             durationOfBreak: TimeSpan.FromSeconds(30)
         );
     
     builder.Services.AddHttpClient("AzureDocumentIntelligence", client =>
     {
         client.BaseAddress = new Uri(builder.Configuration["AzureDocumentIntelligence:Endpoint"]);
         client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", builder.Configuration["AzureDocumentIntelligence:ApiKey"]);
         client.Timeout = TimeSpan.FromSeconds(10);
     })
     .AddPolicyHandler(Policy.WrapAsync(
         documentIntelligenceCircuitBreaker,
         openAIRetryPolicy,
         openAITimeoutPolicy
     ));
     
     // Register circuit breaker services
     builder.Services.AddSingleton<DocumentRetryQueue>();
     builder.Services.AddScoped<ResilientAIService>();
     
     // Register Hangfire recurring job for retry processing
     RecurringJob.AddOrUpdate<ProcessRetryQueueJob>(
         "process-ai-retry-queue",
         job => job.ExecuteAsync(CancellationToken.None),
         Cron.Minutely // Check every minute for retry-able documents
     );
     ```

2. **Create ResilientAIService with Circuit Breaker**
   - File: `src/backend/PatientAccess.Business/Services/ResilientAIService.cs`
   - Circuit breaker wrapper for AI services:
     ```csharp
     using Polly.CircuitBreaker;
     using Azure.AI.OpenAI;
     using Azure.AI.FormRecognizer;
     
     namespace PatientAccess.Business.Services
     {
         /// <summary>
         /// Resilient AI service with circuit breaker and fallback (AC3 - US_058, AIR-O03, NFR-015, AD-006).
         /// </summary>
         public class ResilientAIService
         {
             private readonly IHttpClientFactory _httpClientFactory;
             private readonly DocumentRetryQueue _retryQueue;
             private readonly ILogger<ResilientAIService> _logger;
             
             public ResilientAIService(
                 IHttpClientFactory httpClientFactory,
                 DocumentRetryQueue retryQueue,
                 ILogger<ResilientAIService> logger)
             {
                 _httpClientFactory = httpClientFactory;
                 _retryQueue = retryQueue;
                 _logger = logger;
             }
             
             /// <summary>
             /// Processes clinical document with circuit breaker protection (AC3).
             /// Falls back to manual workflow if AI unavailable within 5 seconds.
             /// </summary>
             public async Task<AIProcessingResult> ProcessDocumentAsync(
                 int documentId,
                 string documentUri,
                 CancellationToken cancellationToken = default)
             {
                 try
                 {
                     // Attempt AI processing with circuit breaker
                     var startTime = DateTime.UtcNow;
                     
                     // Check circuit breaker state BEFORE attempt (fast fail if Open)
                     var httpClient = _httpClientFactory.CreateClient("AzureDocumentIntelligence");
                     
                     // Call Azure Document Intelligence
                     var response = await httpClient.PostAsJsonAsync(
                         "/formrecognizer/documentModels/prebuilt-layout:analyze",
                         new { urlSource = documentUri },
                         cancellationToken);
                     
                     var processingTime = (DateTime.UtcNow - startTime).TotalSeconds;
                     
                     if (!response.IsSuccessStatusCode)
                     {
                         var statusCode = (int)response.StatusCode;
                         _logger.LogWarning(
                             "AI service returned {StatusCode} for document {DocumentId}. " +
                             "Falling back to manual workflow (AC3 - US_058).",
                             statusCode, documentId);
                         
                         // Queue for retry
                         await _retryQueue.EnqueueAsync(documentId, documentUri, "HTTP " + statusCode);
                         
                         // Return fallback result
                         return AIProcessingResult.Fallback(
                             documentId,
                             $"AI service temporarily unavailable (HTTP {statusCode}). Document queued for manual processing.",
                             processingTime);
                     }
                     
                     // Parse response
                     var result = await response.Content.ReadFromJsonAsync<DocumentAnalysisResult>(cancellationToken);
                     
                     // Validate response (Edge Case 1: empty/malformed)
                     if (result == null || result.AnalyzeResult == null || result.AnalyzeResult.Content == null)
                     {
                         _logger.LogError(
                             "AI returned empty/malformed response for document {DocumentId}. " +
                             "Edge Case 1 - routing to manual processing.",
                             documentId);
                         
                         // Discard response, log incident, increment error counter
                         await _retryQueue.EnqueueAsync(documentId, documentUri, "Malformed response");
                         
                         return AIProcessingResult.Fallback(
                             documentId,
                             "AI response invalid. Document queued for manual processing.",
                             processingTime);
                     }
                     
                     _logger.LogInformation(
                         "AI successfully processed document {DocumentId} in {ProcessingTime}s.",
                         documentId, processingTime);
                     
                     return AIProcessingResult.Success(documentId, result, processingTime);
                 }
                 catch (BrokenCircuitException ex)
                 {
                     // Circuit is OPEN - fast fail to manual workflow (AC3)
                     _logger.LogWarning(
                         ex,
                         "Circuit breaker OPEN for document {DocumentId}. " +
                         "Falling back to manual workflow (AC3 - AIR-O03).",
                         documentId);
                     
                     await _retryQueue.EnqueueAsync(documentId, documentUri, "Circuit breaker open");
                     
                     return AIProcessingResult.Fallback(
                         documentId,
                         "AI services temporarily unavailable. Document queued for manual processing.",
                         0.0);
                 }
                 catch (TimeoutException ex)
                 {
                     // 5-second timeout exceeded (AC3)
                     _logger.LogWarning(
                         ex,
                         "AI processing timeout (>5s) for document {DocumentId}. Fallback to manual (AC3 - US_058).",
                         documentId);
                     
                     await _retryQueue.EnqueueAsync(documentId, documentUri, "Timeout");
                     
                     return AIProcessingResult.Fallback(
                         documentId,
                         "AI processing timeout. Document queued for manual processing.",
                         5.0);
                 }
                 catch (Exception ex)
                 {
                     _logger.LogError(
                         ex,
                         "Unexpected error processing document {DocumentId}. Fallback to manual.",
                         documentId);
                     
                     await _retryQueue.EnqueueAsync(documentId, documentUri, ex.Message);
                     
                     return AIProcessingResult.Fallback(
                         documentId,
                         "AI processing error. Document queued for manual processing.",
                         0.0);
                 }
             }
         }
         
         /// <summary>
         /// AI processing result with success/fallback states.
         /// </summary>
         public class AIProcessingResult
         {
             public int DocumentId { get; set; }
             public bool IsSuccess { get; set; }
             public bool RequiresManualProcessing { get; set; }
             public string UserFriendlyMessage { get; set; }
             public object? AnalysisResult { get; set; }
             public double ProcessingTimeSeconds { get; set; }
             
             public static AIProcessingResult Success(int documentId, object result, double processingTime)
             {
                 return new AIProcessingResult
                 {
                     DocumentId = documentId,
                     IsSuccess = true,
                     RequiresManualProcessing = false,
                     UserFriendlyMessage = "Document processed successfully",
                     AnalysisResult = result,
                     ProcessingTimeSeconds = processingTime
                 };
             }
             
             public static AIProcessingResult Fallback(int documentId, string message, double processingTime)
             {
                 return new AIProcessingResult
                 {
                     DocumentId = documentId,
                     IsSuccess = false,
                     RequiresManualProcessing = true,
                     UserFriendlyMessage = message,
                     AnalysisResult = null,
                     ProcessingTimeSeconds = processingTime
                 };
             }
         }
     }
     ```

3. **Create DocumentRetryQueue (Redis)**
   - File: `src/backend/PatientAccess.Business/Services/DocumentRetryQueue.cs`
   - Redis-based retry queue for failed documents:
     ```csharp
     using StackExchange.Redis;
     using System.Text.Json;
     
     namespace PatientAccess.Business.Services
     {
         /// <summary>
         /// Redis-based queue for documents awaiting AI retry (AC3 - US_058).
         /// </summary>
         public class DocumentRetryQueue
         {
             private readonly IConnectionMultiplexer _redis;
             private readonly ILogger<DocumentRetryQueue> _logger;
             
             private const string QueueKey = "ai:retry:documents";
             private const int MaxRetries = 5;
             
             public DocumentRetryQueue(
                 IConnectionMultiplexer redis,
                 ILogger<DocumentRetryQueue> logger)
             {
                 _redis = redis;
                 _logger = logger;
             }
             
             /// <summary>
             /// Adds document to retry queue with metadata (AC3).
             /// </summary>
             public async Task EnqueueAsync(int documentId, string documentUri, string failureReason)
             {
                 var db = _redis.GetDatabase();
                 
                 var retryItem = new DocumentRetryItem
                 {
                     DocumentId = documentId,
                     DocumentUri = documentUri,
                     FailureReason = failureReason,
                     EnqueuedAt = DateTime.UtcNow,
                     RetryCount = 0,
                     NextRetryAt = DateTime.UtcNow.AddMinutes(1) // Retry after 1 minute
                 };
                 
                 var json = JsonSerializer.Serialize(retryItem);
                 await db.ListRightPushAsync(QueueKey, json);
                 
                 _logger.LogInformation(
                     "Document {DocumentId} queued for retry. Reason: {Reason} (AC3 - US_058).",
                     documentId, failureReason);
             }
             
             /// <summary>
             /// Gets documents ready for retry (NextRetryAt has passed).
             /// </summary>
             public async Task<List<DocumentRetryItem>> GetRetryableDocumentsAsync()
             {
                 var db = _redis.GetDatabase();
                 var now = DateTime.UtcNow;
                 
                 var allItems = await db.ListRangeAsync(QueueKey);
                 var retryableItems = new List<DocumentRetryItem>();
                 
                 foreach (var item in allItems)
                 {
                     var retryItem = JsonSerializer.Deserialize<DocumentRetryItem>(item.ToString());
                     
                     if (retryItem != null && retryItem.NextRetryAt <= now && retryItem.RetryCount < MaxRetries)
                     {
                         retryableItems.Add(retryItem);
                     }
                 }
                 
                 return retryableItems;
             }
             
             /// <summary>
             /// Removes document from retry queue (successful retry or max retries exceeded).
             /// </summary>
             public async Task DequeueAsync(int documentId)
             {
                 var db = _redis.GetDatabase();
                 
                 var allItems = await db.ListRangeAsync(QueueKey);
                 
                 foreach (var item in allItems)
                 {
                     var retryItem = JsonSerializer.Deserialize<DocumentRetryItem>(item.ToString());
                     
                     if (retryItem?.DocumentId == documentId)
                     {
                         await db.ListRemoveAsync(QueueKey, item);
                         _logger.LogInformation(
                             "Document {DocumentId} removed from retry queue.",
                             documentId);
                         break;
                     }
                 }
             }
             
             /// <summary>
             /// Increments retry count and updates NextRetryAt (exponential backoff).
             /// </summary>
             public async Task IncrementRetryCountAsync(int documentId)
             {
                 var db = _redis.GetDatabase();
                 
                 var allItems = await db.ListRangeAsync(QueueKey);
                 
                 for (int i = 0; i < allItems.Length; i++)
                 {
                     var retryItem = JsonSerializer.Deserialize<DocumentRetryItem>(allItems[i].ToString());
                     
                     if (retryItem?.DocumentId == documentId)
                     {
                         retryItem.RetryCount++;
                         retryItem.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, retryItem.RetryCount)); // Exponential backoff
                         
                         // Replace item in queue
                         await db.ListSetByIndexAsync(QueueKey, i, JsonSerializer.Serialize(retryItem));
                         
                         _logger.LogInformation(
                             "Document {DocumentId} retry count incremented to {RetryCount}. Next retry at {NextRetryAt}.",
                             documentId, retryItem.RetryCount, retryItem.NextRetryAt);
                         
                         break;
                     }
                 }
             }
             
             /// <summary>
             /// Gets count of documents in retry queue.
             /// </summary>
             public async Task<long> GetQueueLengthAsync()
             {
                 var db = _redis.GetDatabase();
                 return await db.ListLengthAsync(QueueKey);
             }
         }
         
         /// <summary>
         /// Metadata for document retry queue item.
         /// </summary>
         public class DocumentRetryItem
         {
             public int DocumentId { get; set; }
             public string DocumentUri { get; set; }
             public string FailureReason { get; set; }
             public DateTime EnqueuedAt { get; set; }
             public int RetryCount { get; set; }
             public DateTime NextRetryAt { get; set; }
         }
     }
     ```

4. **Create ProcessRetryQueueJob (Hangfire)**
   - File: `src/backend/PatientAccess.Business/BackgroundJobs/ProcessRetryQueueJob.cs`
   - Background job to replay queued documents:
     ```csharp
     using Hangfire;
     
     namespace PatientAccess.Business.BackgroundJobs
     {
         /// <summary>
         /// Hangfire job to process AI retry queue when services recover (AC3 - US_058).
         /// </summary>
         public class ProcessRetryQueueJob
         {
             private readonly DocumentRetryQueue _retryQueue;
             private readonly ResilientAIService _aiService;
             private readonly ILogger<ProcessRetryQueueJob> _logger;
             
             public ProcessRetryQueueJob(
                 DocumentRetryQueue retryQueue,
                 ResilientAIService aiService,
                 ILogger<ProcessRetryQueueJob> logger)
             {
                 _retryQueue = retryQueue;
                 _aiService = aiService;
                 _logger = logger;
             }
             
             /// <summary>
             /// Executes retry queue processing (runs every minute via Hangfire).
             /// </summary>
             public async Task ExecuteAsync(CancellationToken cancellationToken)
             {
                 var queueLength = await _retryQueue.GetQueueLengthAsync();
                 
                 if (queueLength == 0)
                 {
                     // No documents to retry
                     return;
                 }
                 
                 _logger.LogInformation(
                     "Processing AI retry queue. {QueueLength} documents awaiting retry.",
                     queueLength);
                 
                 var retryableDocuments = await _retryQueue.GetRetryableDocumentsAsync();
                 
                 foreach (var doc in retryableDocuments)
                 {
                     if (cancellationToken.IsCancellationRequested)
                         break;
                     
                     try
                     {
                         _logger.LogInformation(
                             "Retrying AI processing for document {DocumentId} (attempt {RetryCount}/{MaxRetries}).",
                             doc.DocumentId, doc.RetryCount + 1, 5);
                         
                         var result = await _aiService.ProcessDocumentAsync(
                             doc.DocumentId,
                             doc.DocumentUri,
                             cancellationToken);
                         
                         if (result.IsSuccess)
                         {
                             // Success - remove from queue
                             await _retryQueue.DequeueAsync(doc.DocumentId);
                             
                             _logger.LogInformation(
                                 "Document {DocumentId} successfully processed on retry. Removed from queue.",
                                 doc.DocumentId);
                         }
                         else
                         {
                             // Still failing - increment retry count
                             await _retryQueue.IncrementRetryCountAsync(doc.DocumentId);
                             
                             if (doc.RetryCount + 1 >= 5)
                             {
                                 // Max retries exceeded - remove from queue (permanent manual processing)
                                 await _retryQueue.DequeueAsync(doc.DocumentId);
                                 
                                 _logger.LogWarning(
                                     "Document {DocumentId} exceeded max retries. Requires permanent manual processing.",
                                     doc.DocumentId);
                             }
                         }
                     }
                     catch (Exception ex)
                     {
                         _logger.LogError(
                             ex,
                             "Error retrying document {DocumentId}. Will retry later.",
                             doc.DocumentId);
                     }
                 }
             }
         }
     }
     ```

5. **Create AIServiceHealthCheck**
   - File: `src/backend/PatientAccess.Web/HealthChecks/AIServiceHealthCheck.cs`
   - Health check monitoring circuit breaker state:
     ```csharp
     using Microsoft.Extensions.Diagnostics.HealthChecks;
     
     namespace PatientAccess.Web.HealthChecks
     {
         /// <summary>
         /// Health check for AI service circuit breaker state (AC3 - US_058).
         /// </summary>
         public class AIServiceHealthCheck : IHealthCheck
         {
             private readonly IHttpClientFactory _httpClientFactory;
             private readonly DocumentRetryQueue _retryQueue;
             private readonly ILogger<AIServiceHealthCheck> _logger;
             
             public AIServiceHealthCheck(
                 IHttpClientFactory httpClientFactory,
                 DocumentRetryQueue retryQueue,
                 ILogger<AIServiceHealthCheck> logger)
             {
                 _httpClientFactory = httpClientFactory;
                 _retryQueue = retryQueue;
                 _logger = logger;
             }
             
             public async Task<HealthCheckResult> CheckHealthAsync(
                 HealthCheckContext context,
                 CancellationToken cancellationToken = default)
             {
                 try
                 {
                     // Check retry queue length
                     var queueLength = await _retryQueue.GetQueueLengthAsync();
                     
                     var data = new Dictionary<string, object>
                     {
                         { "retryQueueLength", queueLength }
                     };
                     
                     // Determine health status based on queue length
                     if (queueLength == 0)
                     {
                         return HealthCheckResult.Healthy(
                             "AI services operational. No documents in retry queue.",
                             data);
                     }
                     else if (queueLength < 10)
                     {
                         return HealthCheckResult.Degraded(
                             $"AI services degraded. {queueLength} documents in retry queue.",
                             data: data);
                     }
                     else
                     {
                         return HealthCheckResult.Unhealthy(
                             $"AI services unhealthy. {queueLength} documents in retry queue.",
                             data: data);
                     }
                 }
                 catch (Exception ex)
                 {
                     _logger.LogError(ex, "Error checking AI service health.");
                     return HealthCheckResult.Unhealthy("Error checking AI service health.", ex);
                 }
             }
         }
     }
     ```

6. **Configure Circuit Breaker Settings**
   - File: `src/backend/PatientAccess.Web/appsettings.json`
   - Add configuration:
     ```json
     {
       "AICircuitBreaker": {
         "FailureThreshold": 5,
         "DurationOfBreakSeconds": 30,
         "TimeoutSeconds": 5,
         "MaxRetries": 3,
         "RetryQueueMaxItems": 1000
       },
       "AzureOpenAI": {
         "Endpoint": "https://YOUR_INSTANCE.openai.azure.com/",
         "ApiKey": "YOUR_API_KEY",
         "DeploymentName": "gpt-4o"
       },
       "AzureDocumentIntelligence": {
         "Endpoint": "https://YOUR_INSTANCE.cognitiveservices.azure.com/",
         "ApiKey": "YOUR_API_KEY"
       }
     }
     ```

7. **Document Circuit Breaker Pattern**
   - File: `docs/AI_CIRCUIT_BREAKER.md`
   - Comprehensive documentation with diagrams and examples (Edge Case 1 handling, retry policies, circuit states)

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   └── BackgroundJobs/
├── PatientAccess.Web/
│   ├── HealthChecks/
│   ├── Program.cs
│   └── appsettings.json
└── docs/
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/ResilientAIService.cs | Circuit breaker wrapper for AI services |
| CREATE | src/backend/PatientAccess.Business/Services/DocumentRetryQueue.cs | Redis-based retry queue |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/ProcessRetryQueueJob.cs | Hangfire retry processor |
| CREATE | src/backend/PatientAccess.Web/HealthChecks/AIServiceHealthCheck.cs | Circuit state health check |
| CREATE | docs/AI_CIRCUIT_BREAKER.md | Circuit breaker documentation |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Configure Polly policies, register services |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add circuit breaker configuration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Polly Documentation
- **Circuit Breaker Pattern**: https://github.com/App-vNext/Polly/wiki/Circuit-Breaker
- **Policy Wrap**: https://github.com/App-vNext/Polly/wiki/PolicyWrap
- **Timeout**: https://github.com/App-vNext/Polly/wiki/Timeout

### Azure AI Documentation
- **Azure OpenAI Service**: https://learn.microsoft.com/en-us/azure/ai-services/openai/
- **Azure Document Intelligence**: https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/

### Design Requirements
- **AIR-O03**: AI model rollback support (design.md)
- **NFR-015**: Graceful degradation when AI unavailable (design.md)
- **AD-006**: Progressive AI degradation architecture (design.md)

## Build Commands
```powershell
# Install Polly NuGet package
cd src/backend/PatientAccess.Business
dotnet add package Polly --version 8.0.0
dotnet add package Polly.Extensions.Http --version 3.0.0

# Build solution
cd ..
dotnet build PatientAccess.sln

# Run tests
dotnet test PatientAccess.Tests/PatientAccess.Tests.csproj

# Run backend
cd PatientAccess.Web
dotnet run
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Services/ResilientAIServiceTests.cs`
- Test cases:
  1. **Test_ProcessDocument_SuccessPath**
     - Mock successful AI response
     - Assert: IsSuccess=true, no retry queue entry
  2. **Test_ProcessDocument_CircuitBreakerOpen_FallsBackImmediately**
     - Simulate open circuit
     - Assert: RequiresManualProcessing=true, document queued, <1s response (fast fail)
  3. **Test_ProcessDocument_Timeout_FallsBackWithin5Seconds**
     - Mock 6-second AI delay
     - Assert: Timeout exception, fallback result, ProcessingTimeSeconds ≈ 5.0, document queued (AC3)
  4. **Test_ProcessDocument_MalformedResponse_DiscardsAndQueues**
     - Mock empty/null AI response
     - Assert: Edge Case 1 handling, document queued, error logged
  5. **Test_RetryQueue_ExponentialBackoff**
     - Enqueue document, increment retry count 3 times
     - Assert: NextRetryAt = 1min, 2min, 4min, 8min (exponential backoff)

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/CircuitBreakerIntegrationTests.cs`
- Test cases:
  1. **Test_CircuitBreaker_OpensAfter5Failures**
     - Trigger 5 consecutive failures
     - Assert: Circuit state = Open, 6th request fast-fails
  2. **Test_RetryQueue_ProcessesDocumentsOnRecovery**
     - Queue 3 documents during circuit Open
     - Close circuit (Half-Open → Closed)
     - Run ProcessRetryQueueJob
     - Assert: All 3 documents processed, queue empty

### Acceptance Criteria Validation
- **AC3**: ✅ Circuit breaker detects failures, fallback within 5s, user-friendly notice, retry queue implemented
- **Edge Case 1**: ✅ Malformed AI responses discarded, logged, queued for manual processing

## Success Criteria Checklist
- [MANDATORY] Polly circuit breaker policy configured for Azure OpenAI
- [MANDATORY] Polly circuit breaker policy configured for Azure Document Intelligence
- [MANDATORY] Timeout policy set to 5 seconds (AC3 - AIR-O03)
- [MANDATORY] Retry policy with exponential backoff (3 retries)
- [MANDATORY] ResilientAIService wraps AI calls with circuit breaker
- [MANDATORY] AIProcessingResult includes IsSuccess, RequiresManualProcessing, UserFriendlyMessage
- [MANDATORY] Fallback to manual workflow when circuit Open (<1s fast fail)
- [MANDATORY] Fallback when timeout exceeded (5 seconds per AC3)
- [MANDATORY] Edge Case 1: Malformed response handling (discard, log, queue)
- [MANDATORY] DocumentRetryQueue stores failed documents in Redis
- [MANDATORY] DocumentRetryItem includes metadata (DocumentId, FailureReason, RetryCount, NextRetryAt)
- [MANDATORY] ProcessRetryQueueJob replays queued documents every minute
- [MANDATORY] Exponential backoff for retry attempts (1min, 2min, 4min, 8min, 16min)
- [MANDATORY] Max 5 retries before permanent manual processing
- [MANDATORY] AIServiceHealthCheck monitors circuit state and queue length
- [MANDATORY] Health check: Healthy (queue=0), Degraded (queue<10), Unhealthy (queue>=10)
- [MANDATORY] Circuit breaker configuration in appsettings.json
- [RECOMMENDED] AI_CIRCUIT_BREAKER.md documentation with circuit state diagrams
- [RECOMMENDED] Integration test: Circuit opens after threshold breaches

## Estimated Effort
**4 hours** (Polly configuration + ResilientAIService + DocumentRetryQueue + ProcessRetryQueueJob + health check + docs + tests)
