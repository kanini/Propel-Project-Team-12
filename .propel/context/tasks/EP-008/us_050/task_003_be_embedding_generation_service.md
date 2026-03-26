# Task - task_003_be_embedding_generation_service

## Requirement Reference
- User Story: US_050
- Story Location: .propel/context/tasks/EP-008/us_050/us_050.md
- Acceptance Criteria:
    - **AC2**: Given chunks are created, When embeddings are generated, Then Azure OpenAI text-embedding-3-small produces 1536-dimensional vectors stored in separate pgvector indices for ICD-10, CPT, and clinical terminology (AIR-R04).
    - **AC3**: Given a code mapping query is submitted, When retrieval executes, Then top-5 chunks with cosine similarity above 0.75 are returned (AIR-R02) using hybrid retrieval combining semantic similarity and keyword matching (AIR-R03).
- Edge Case:
    - How does the system handle knowledge base updates (new code releases)? Re-indexing pipeline can be triggered independently without affecting live queries.

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
| Library | Azure.AI.OpenAI | 1.0+ |
| Library | Hangfire | 1.8.x |
| Library | Polly | 8.x |
| Database | PostgreSQL | 16.x |
| Vector Store | pgvector | 0.5+ |
| AI Gateway | Azure OpenAI Service | text-embedding-3-small |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-R04 (Separate indices per code system), DR-010 (1536-dimensional vectors) |
| **AI Pattern** | RAG Embedding Generation |
| **Prompt Template Path** | N/A (Embeddings use raw text, no prompt engineering) |
| **Guardrails Config** | N/A (Embeddings are deterministic vector transformation) |
| **Model Provider** | Azure OpenAI (text-embedding-3-small, 1536 dimensions) |

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

Create embedding generation service to convert document chunks into 1536-dimensional vector embeddings using Azure OpenAI text-embedding-3-small. This task implements the second stage of the RAG indexing pipeline, consuming DocumentChunk staging records and populating ICD10Codes, CPTCodes, and ClinicalTerminology tables with embeddings per AIR-R04. The service includes batch processing (up to 100 chunks per API call), rate limiting (Azure OpenAI quota management), retry logic (Polly circuit breaker), and Hangfire background job integration. Embeddings are stored with metadata for traceability and support re-indexing without affecting live queries.

**Key Capabilities:**
- EmbeddingGenerationService with Azure OpenAI SDK integration
- Batch processing (up to 100 chunks per API call for efficiency)
- Rate limiting (respect Azure OpenAI TPM/RPM quotas)
- Polly circuit breaker and retry logic (handle Azure transient failures)
- Hangfire background job (GenerateEmbeddingsJob) for async processing
- Separate processing pipelines per code system (ICD10, CPT, ClinicalTerminology)
- Update DocumentChunk.IsProcessed flag after embedding persisted
- Progress tracking (percentage complete, ETA)
- Logging and telemetry (Application Insights)
- Error handling for malformed chunks or API failures

## Dependent Tasks
- EP-008: US_050: task_001_db_vector_indices_schema (ICD10Code/CPTCode/ClinicalTerminology entities with vector columns)
- EP-008: US_050: task_002_be_document_chunking_service (DocumentChunk staging table populated)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/EmbeddingGenerationService.cs` - Core embedding logic
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/IEmbeddingGenerationService.cs` - Service interface
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/GenerateEmbeddingsJob.cs` - Hangfire job
- **NEW**: `src/backend/PatientAccess.Business/Configuration/AzureOpenAISettings.cs` - Configuration class
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add Azure OpenAI configuration
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.Development.json` - Add development Azure OpenAI endpoint
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register IEmbeddingGenerationService, configure Azure OpenAI client
- **MODIFY**: `src/backend/PatientAccess.Business/PatientAccess.Business.csproj` - Add Azure.AI.OpenAI package
- **MODIFY**: `src/backend/PatientAccess.Web/Controllers/KnowledgeBaseController.cs` - Add endpoint to trigger embedding generation

## Implementation Plan

1. **Install Azure.AI.OpenAI Package**
   - Add to `PatientAccess.Business.csproj`:
     ```xml
     <PackageReference Include="Azure.AI.OpenAI" Version="1.0.0-beta.17" />
     <PackageReference Include="Azure.Identity" Version="1.12.0" />
     ```
   - Azure.AI.OpenAI SDK supports text-embedding-3-small model

2. **Create AzureOpenAISettings Configuration**
   - File: `src/backend/PatientAccess.Business/Configuration/AzureOpenAISettings.cs`
   - Properties:
     ```csharp
     public class AzureOpenAISettings
     {
         public string Endpoint { get; set; } // e.g., "https://your-resource.openai.azure.com/"
         public string ApiKey { get; set; } // Azure OpenAI API key
         public string EmbeddingDeploymentName { get; set; } // "text-embedding-3-small"
         public int MaxTokensPerMinute { get; set; } // Rate limit (default: 120,000 TPM)
         public int MaxRequestsPerMinute { get; set; } // Rate limit (default: 720 RPM)
         public int BatchSize { get; set; } // Chunks per API call (default: 100)
     }
     ```
   - Add to `appsettings.json`:
     ```json
     "AzureOpenAI": {
       "Endpoint": "https://your-resource.openai.azure.com/",
       "ApiKey": "YOUR_API_KEY_HERE",
       "EmbeddingDeploymentName": "text-embedding-3-small",
       "MaxTokensPerMinute": 120000,
       "MaxRequestsPerMinute": 720,
       "BatchSize": 100
     }
     ```
   - Use Azure Key Vault for ApiKey in production (reference in appsettings.Production.json)

3. **Create IEmbeddingGenerationService Interface**
   - File: `src/backend/PatientAccess.Business/Interfaces/IEmbeddingGenerationService.cs`
   - Methods:
     ```csharp
     Task<List<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken);
     Task<Dictionary<string, List<float>>> GenerateBatchEmbeddingsAsync(List<string> texts, CancellationToken cancellationToken);
     Task ProcessPendingChunksAsync(string codeSystem, CancellationToken cancellationToken);
     ```

4. **Implement EmbeddingGenerationService**
   - File: `src/backend/PatientAccess.Business/Services/EmbeddingGenerationService.cs`
   - Constructor dependencies:
     - ILogger<EmbeddingGenerationService>
     - ApplicationDbContext
     - IOptions<AzureOpenAISettings>
     - OpenAIClient (Azure OpenAI SDK)
   - Initialize Azure OpenAI client:
     ```csharp
     private readonly OpenAIClient _openAIClient;
     private readonly AzureOpenAISettings _settings;
     
     public EmbeddingGenerationService(
         ILogger<EmbeddingGenerationService> logger,
         ApplicationDbContext context,
         IOptions<AzureOpenAISettings> settings,
         OpenAIClient openAIClient)
     {
         _logger = logger;
         _context = context;
         _settings = settings.Value;
         _openAIClient = openAIClient;
     }
     ```
   - Implement single embedding generation:
     ```csharp
     public async Task<List<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
     {
         var options = new EmbeddingsOptions(_settings.EmbeddingDeploymentName, new[] { text });
         var response = await _openAIClient.GetEmbeddingsAsync(options, cancellationToken);
         
         return response.Value.Data[0].Embedding.ToList();
     }
     ```
   - Implement batch embedding generation (up to 100 chunks):
     ```csharp
     public async Task<Dictionary<string, List<float>>> GenerateBatchEmbeddingsAsync(
         List<string> texts, CancellationToken cancellationToken)
     {
         if (texts.Count > _settings.BatchSize)
         {
             throw new ArgumentException($"Batch size exceeds limit: {texts.Count} > {_settings.BatchSize}");
         }
         
         var options = new EmbeddingsOptions(_settings.EmbeddingDeploymentName, texts);
         var response = await _openAIClient.GetEmbeddingsAsync(options, cancellationToken);
         
         var embeddings = new Dictionary<string, List<float>>();
         for (int i = 0; i < texts.Count; i++)
         {
             embeddings[texts[i]] = response.Value.Data[i].Embedding.ToList();
         }
         
         return embeddings;
     }
     ```
   - Implement ProcessPendingChunksAsync (processes all unprocessed chunks for a code system):
     ```csharp
     public async Task ProcessPendingChunksAsync(string codeSystem, CancellationToken cancellationToken)
     {
         var pendingChunks = await _context.DocumentChunks
             .Where(c => c.CodeSystem == codeSystem && !c.IsProcessed)
             .OrderBy(c => c.ChunkIndex)
             .ToListAsync(cancellationToken);
         
         _logger.LogInformation("Processing {Count} pending chunks for {CodeSystem}", 
             pendingChunks.Count, codeSystem);
         
         // Process in batches
         for (int i = 0; i < pendingChunks.Count; i += _settings.BatchSize)
         {
             var batch = pendingChunks.Skip(i).Take(_settings.BatchSize).ToList();
             await ProcessBatchAsync(batch, codeSystem, cancellationToken);
             
             // Rate limiting: delay between batches
             await Task.Delay(TimeSpan.FromSeconds(60.0 / _settings.MaxRequestsPerMinute), cancellationToken);
         }
         
         _logger.LogInformation("Completed processing {Count} chunks for {CodeSystem}", 
             pendingChunks.Count, codeSystem);
     }
     
     private async Task ProcessBatchAsync(List<DocumentChunk> chunks, string codeSystem, CancellationToken cancellationToken)
     {
         var texts = chunks.Select(c => c.SourceText).ToList();
         var embeddings = await GenerateBatchEmbeddingsAsync(texts, cancellationToken);
         
         // Persist embeddings to appropriate table
         foreach (var chunk in chunks)
         {
             var embedding = embeddings[chunk.SourceText];
             
             switch (codeSystem)
             {
                 case "ICD10":
                     await PersistICD10EmbeddingAsync(chunk, embedding, cancellationToken);
                     break;
                 case "CPT":
                     await PersistCPTEmbeddingAsync(chunk, embedding, cancellationToken);
                     break;
                 case "ClinicalTerminology":
                     await PersistClinicalTerminologyEmbeddingAsync(chunk, embedding, cancellationToken);
                     break;
             }
             
             // Mark chunk as processed
             chunk.IsProcessed = true;
             chunk.ProcessedAt = DateTime.UtcNow;
         }
         
         await _context.SaveChangesAsync(cancellationToken);
     }
     
     private async Task PersistICD10EmbeddingAsync(DocumentChunk chunk, List<float> embedding, CancellationToken cancellationToken)
     {
         // Parse ICD-10 code from chunk text (e.g., extract "E11.9" from "E11.9 Type 2 diabetes mellitus")
         var codeMatch = Regex.Match(chunk.SourceText, @"([A-Z]\d{2}\.\d{1,2})");
         var code = codeMatch.Success ? codeMatch.Value : "UNKNOWN";
         
         var icd10Code = new ICD10Code
         {
             Id = Guid.NewGuid(),
             Code = code,
             Description = chunk.SourceText,
             Category = ExtractCategoryFromChunk(chunk.SourceText),
             Embedding = embedding.ToArray(), // pgvector expects float[] not List<float>
             ChunkText = chunk.SourceText,
             Metadata = new { chunkId = chunk.Id, chunkIndex = chunk.ChunkIndex },
             Version = "ICD-10-CM-2024",
             IsActive = true,
             CreatedAt = DateTime.UtcNow,
             UpdatedAt = DateTime.UtcNow
         };
         
         await _context.ICD10Codes.AddAsync(icd10Code, cancellationToken);
         chunk.TargetEntityId = icd10Code.Id;
     }
     
     // Similar methods for CPT and ClinicalTerminology...
     ```

5. **Add Polly Circuit Breaker and Retry Logic**
   - Wrap Azure OpenAI API calls in Polly policy:
     ```csharp
     private readonly AsyncRetryPolicy _retryPolicy;
     private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
     
     // In constructor:
     _retryPolicy = Policy
         .Handle<RequestFailedException>() // Azure SDK exception
         .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
             onRetry: (exception, timeSpan, retryCount, context) =>
             {
                 _logger.LogWarning("Retry {RetryCount} after {Delay}s: {Message}", 
                     retryCount, timeSpan.TotalSeconds, exception.Message);
             });
     
     _circuitBreakerPolicy = Policy
         .Handle<RequestFailedException>()
         .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1),
             onBreak: (exception, duration) =>
             {
                 _logger.LogError("Circuit breaker opened for {Duration}s", duration.TotalSeconds);
             },
             onReset: () =>
             {
                 _logger.LogInformation("Circuit breaker reset");
             });
     
     // Wrap API calls:
     public async Task<List<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
     {
         return await _circuitBreakerPolicy.ExecuteAsync(() =>
             _retryPolicy.ExecuteAsync(() =>
             {
                 var options = new EmbeddingsOptions(_settings.EmbeddingDeploymentName, new[] { text });
                 return _openAIClient.GetEmbeddingsAsync(options, cancellationToken)
                     .ContinueWith(t => t.Result.Value.Data[0].Embedding.ToList(), cancellationToken);
             }));
     }
     ```

6. **Create GenerateEmbeddingsJob Hangfire Job**
   - File: `src/backend/PatientAccess.Business/BackgroundJobs/GenerateEmbeddingsJob.cs`
   - Job parameters: CodeSystem (ICD10, CPT, ClinicalTerminology)
   - Job logic:
     ```csharp
     [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 300, 900, 1800 })]
     public async Task ExecuteAsync(string codeSystem, CancellationToken cancellationToken)
     {
         _logger.LogInformation("Starting embedding generation job for {CodeSystem}", codeSystem);
         
         await _embeddingService.ProcessPendingChunksAsync(codeSystem, cancellationToken);
         
         _logger.LogInformation("Embedding generation job completed for {CodeSystem}", codeSystem);
     }
     ```
   - Integrate with Hangfire dashboard for monitoring

7. **Update KnowledgeBaseController**
   - Add endpoint to trigger embedding generation:
     ```csharp
     [Authorize(Roles = "Admin")]
     [HttpPost("embeddings/generate/{codeSystem}")]
     public IActionResult TriggerEmbeddingGeneration(string codeSystem)
     {
         var jobId = BackgroundJob.Enqueue<GenerateEmbeddingsJob>(
             job => job.ExecuteAsync(codeSystem, CancellationToken.None));
         
         return Ok(new { JobId = jobId, Status = "Enqueued" });
     }
     
     [Authorize(Roles = "Admin")]
     [HttpGet("embeddings/{codeSystem}/progress")]
     public async Task<IActionResult> GetEmbeddingProgress(string codeSystem)
     {
         var totalChunks = await _context.DocumentChunks
             .Where(c => c.CodeSystem == codeSystem)
             .CountAsync();
         
         var processedChunks = await _context.DocumentChunks
             .Where(c => c.CodeSystem == codeSystem && c.IsProcessed)
             .CountAsync();
         
         var percentage = totalChunks > 0 ? (processedChunks * 100.0 / totalChunks) : 0;
         
         return Ok(new 
         { 
             CodeSystem = codeSystem,
             TotalChunks = totalChunks,
             ProcessedChunks = processedChunks,
             PercentageComplete = percentage
         });
     }
     ```

8. **Configure Azure OpenAI Client in Program.cs**
   - Register OpenAIClient with DI:
     ```csharp
     builder.Services.Configure<AzureOpenAISettings>(
         builder.Configuration.GetSection("AzureOpenAI"));
     
     builder.Services.AddSingleton(sp =>
     {
         var settings = sp.GetRequiredService<IOptions<AzureOpenAISettings>>().Value;
         return new OpenAIClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
     });
     
     builder.Services.AddScoped<IEmbeddingGenerationService, EmbeddingGenerationService>();
     ```

9. **Add Telemetry and Logging**
   - Log embedding metrics: batch size, API latency, embeddings generated per minute
   - Application Insights custom events: "EmbeddingGenerationStarted", "EmbeddingBatchCompleted", "EmbeddingGenerationCompleted"
   - Track rate limiting metrics (requests per minute, tokens per minute)

10. **Implement Progress Tracking**
    - Update DocumentChunk.ProcessedAt timestamp
    - Calculate percentage complete: processedChunks / totalChunks * 100
    - Estimate ETA based on average processing time per chunk

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── BackgroundJobs/
│   │   └── ChunkDocumentsJob.cs (from task_002)
│   ├── Services/
│   │   └── DocumentChunkingService.cs (from task_002)
│   └── Interfaces/
│       └── IDocumentChunkingService.cs (from task_002)
├── PatientAccess.Data/
│   ├── Entities/
│   │   ├── ICD10Code.cs (from task_001)
│   │   ├── CPTCode.cs (from task_001)
│   │   ├── ClinicalTerminology.cs (from task_001)
│   │   └── DocumentChunk.cs (from task_002)
│   └── ApplicationDbContext.cs
└── PatientAccess.Web/
    ├── Controllers/
    │   └── KnowledgeBaseController.cs (from task_002)
    ├── appsettings.json
    └── Program.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/EmbeddingGenerationService.cs | Core embedding logic |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IEmbeddingGenerationService.cs | Service interface |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/GenerateEmbeddingsJob.cs | Hangfire job |
| CREATE | src/backend/PatientAccess.Business/Configuration/AzureOpenAISettings.cs | Configuration POCO |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add AzureOpenAI config |
| MODIFY | src/backend/PatientAccess.Web/appsettings.Development.json | Add dev Azure OpenAI endpoint |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register OpenAIClient, IEmbeddingGenerationService |
| MODIFY | src/backend/PatientAccess.Business/PatientAccess.Business.csproj | Add Azure.AI.OpenAI, Azure.Identity |
| MODIFY | src/backend/PatientAccess.Web/Controllers/KnowledgeBaseController.cs | Add embedding generation endpoints |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### Azure OpenAI SDK Documentation
- **Azure.AI.OpenAI NuGet**: https://www.nuget.org/packages/Azure.AI.OpenAI
- **Embeddings Quickstart**: https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/embeddings
- **Rate Limits**: https://learn.microsoft.com/en-us/azure/ai-services/openai/quotas-limits
- **Batch Processing**: https://platform.openai.com/docs/guides/embeddings/embedding-multiple-inputs

### text-embedding-3-small Model
- **Model Overview**: https://platform.openai.com/docs/guides/embeddings/embedding-models
- **Dimensions**: 1536 (default for text-embedding-3-small)
- **Max Input Tokens**: 8192 tokens (far exceeds our 512-token chunks)

### Polly Resilience Patterns
- **Retry Policy**: https://github.com/App-vNext/Polly#retry
- **Circuit Breaker**: https://github.com/App-vNext/Polly#circuit-breaker
- **Combining Policies**: https://github.com/App-vNext/Polly#policywrap

### pgvector Integration
- **Vector Type Mapping**: https://www.npgsql.org/doc/types/vector.html
- **Inserting Vectors**: https://github.com/pgvector/pgvector#c-1

### Design Requirements
- **DR-010**: System MUST store vector embeddings for clinical data and medical codes using pgvector extension with 1536-dimensional vectors (design.md)
- **AIR-R04**: System MUST maintain separate vector indices for ICD-10 codes, CPT codes, and clinical terminology (design.md)

### Existing Codebase Patterns
- **Hangfire Jobs**: `src/backend/PatientAccess.Business/BackgroundJobs/ChunkDocumentsJob.cs`
- **Polly Circuit Breaker**: Used in US_046 task_001 (AzureDocumentIntelligenceService)

## Build Commands
```powershell
# Add Azure OpenAI SDK
cd src/backend/PatientAccess.Business
dotnet add package Azure.AI.OpenAI --version 1.0.0-beta.17
dotnet add package Azure.Identity --version 1.12.0

# Build solution
cd ..
dotnet build

# Run tests
cd PatientAccess.Tests
dotnet test
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Services/EmbeddingGenerationServiceTests.cs`
- Test cases:
  1. **Test_GenerateEmbeddingAsync_Returns1536DimensionalVector**
     - Input: Sample text "Type 2 Diabetes Mellitus"
     - Expected: Embedding with 1536 dimensions
     - Assert: embedding.Count == 1536
  2. **Test_GenerateBatchEmbeddingsAsync_Processes100Chunks**
     - Input: List of 100 chunk texts
     - Expected: Dictionary with 100 embeddings
     - Assert: embeddings.Count == 100, each embedding.Count == 1536
  3. **Test_ProcessPendingChunksAsync_UpdatesIsProcessedFlag**
     - Setup: Insert 50 DocumentChunk records with IsProcessed = false
     - Execute: ProcessPendingChunksAsync("ICD10")
     - Assert: All 50 chunks have IsProcessed = true
  4. **Test_RateLimiting_DelaysBetweenBatches**
     - Setup: 200 pending chunks (2 batches)
     - Execute: ProcessPendingChunksAsync("ICD10")
     - Assert: Processing time ≥ expected delay (60s / MaxRequestsPerMinute)
  5. **Test_PollyRetryPolicy_HandlesTransientFailures**
     - Mock: Azure OpenAI API throws RequestFailedException once, then succeeds
     - Execute: GenerateEmbeddingAsync("test")
     - Assert: Embedding returned after retry

### Integration Tests
- File: `src/backend/PatientAccess.Tests/BackgroundJobs/GenerateEmbeddingsJobTests.cs`
- Test cases:
  1. **Test_GenerateEmbeddingsJob_ICD10_StoresEmbeddingsInDatabase**
     - Setup: Insert 10 DocumentChunk records with CodeSystem = "ICD10"
     - Enqueue: GenerateEmbeddingsJob("ICD10")
     - Wait for job completion
     - Assert: ICD10Code table contains 10 records with non-null Embedding vectors
  2. **Test_EmbeddingGeneration_ProgressTracking_Accuracy**
     - Setup: Insert 100 DocumentChunk records
     - Execute: ProcessPendingChunksAsync in background
     - Poll: GET /api/knowledgebase/embeddings/ICD10/progress
     - Assert: PercentageComplete increases from 0 to 100

### Acceptance Criteria Validation
- **AC2**: ✅ Azure OpenAI text-embedding-3-small generates 1536-dimensional vectors stored in separate pgvector indices
- **AC3**: ✅ Embeddings stored with cosine similarity indexing (enables top-5 retrieval in task_004)
- **Edge Case**: ✅ Re-indexing updates DocumentChunk.IsProcessed without affecting existing ICD10Code/CPTCode/ClinicalTerminology records

## Success Criteria Checklist
- [MANDATORY] Azure.AI.OpenAI SDK installed and OpenAIClient configured
- [MANDATORY] AzureOpenAISettings configuration in appsettings.json
- [MANDATORY] EmbeddingGenerationService implements IEmbeddingGenerationService interface
- [MANDATORY] GenerateEmbeddingAsync returns 1536-dimensional vector
- [MANDATORY] GenerateBatchEmbeddingsAsync processes up to 100 chunks per API call
- [MANDATORY] ProcessPendingChunksAsync updates DocumentChunk.IsProcessed flag
- [MANDATORY] ICD-10 embeddings persisted to ICD10Codes table with vector(1536) column
- [MANDATORY] CPT embeddings persisted to CPTCodes table with vector(1536) column
- [MANDATORY] Clinical terminology embeddings persisted to ClinicalTerminology table
- [MANDATORY] Polly retry policy: 3 retries with exponential backoff
- [MANDATORY] Polly circuit breaker: 5 failures trigger 1-minute break
- [MANDATORY] GenerateEmbeddingsJob Hangfire job processes code system asynchronously
- [MANDATORY] KnowledgeBaseController /embeddings/generate endpoint triggers job (Admin role)
- [MANDATORY] KnowledgeBaseController /embeddings/{codeSystem}/progress endpoint returns percentage
- [MANDATORY] Unit test: Embedding has 1536 dimensions
- [MANDATORY] Integration test: Embeddings stored in database with pgvector vector(1536) type
- [RECOMMENDED] Rate limiting: Delay between batches (60s / MaxRequestsPerMinute)
- [RECOMMENDED] Application Insights telemetry: "EmbeddingBatchCompleted" with batch size, latency

## Estimated Effort
**5 hours** (Service implementation + Azure OpenAI integration + Polly resilience + unit tests)
