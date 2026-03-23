# Task - task_002_be_document_chunking_service

## Requirement Reference
- User Story: US_050
- Story Location: .propel/context/tasks/EP-008/us_050/us_050.md
- Acceptance Criteria:
    - **AC1**: Given medical coding documents are available, When the indexing pipeline runs, Then documents are chunked into 512-token segments with 64-token overlap (12.5%) per AIR-R01.
    - **AC2**: Given chunks are created, When embeddings are generated, Then Azure OpenAI text-embedding-3-small produces 1536-dimensional vectors stored in separate pgvector indices for ICD-10, CPT, and clinical terminology (AIR-R04).
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
| Library | SharpToken (Tiktoken C# port) | 2.0+ |
| Library | Hangfire | 1.8.x |
| Database | PostgreSQL | 16.x |
| Vector Store | pgvector | 0.5+ |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-R01 (512-token chunks, 64-token overlap), AIR-R04 (Separate indices per code system) |
| **AI Pattern** | RAG Document Chunking |
| **Prompt Template Path** | N/A (Chunking is deterministic tokenization, not LLM-based) |
| **Guardrails Config** | N/A (No LLM inference in chunking) |
| **Model Provider** | Azure OpenAI (for tokenization model: cl100k_base used by text-embedding-3-small) |

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

Create document chunking service to process medical coding documents (ICD-10, CPT, clinical terminology) into 512-token segments with 64-token overlap per AIR-R01. This task implements the first stage of the RAG indexing pipeline using SharpToken (Tiktoken C# port) for cl100k_base tokenization (matching Azure OpenAI text-embedding-3-small). The service processes documents separately by code system (ICD-10, CPT, clinical terminology) to maintain separation for AIR-R04, handles edge cases (incomplete chunks, code boundaries), and integrates with Hangfire for background processing. Chunks are persisted to staging tables before embedding generation.

**Key Capabilities:**
- DocumentChunkingService with ICD-10/CPT/clinical terminology processing
- Tiktoken cl100k_base tokenization (matches text-embedding-3-small)
- 512-token chunks with 64-token (12.5%) overlap per AIR-R01
- Chunk boundary detection (preserve code entries, avoid mid-code splits)
- Staging table for pre-embedding chunks (DocumentChunk entity)
- Hangfire background job (ChunkDocumentsJob) for async processing
- Separate processing pipelines per code system (AIR-R04)
- Re-indexing support without affecting live queries
- Logging and telemetry (Application Insights)
- Error handling and retry logic (Polly)

## Dependent Tasks
- EP-008: US_050: task_001_db_vector_indices_schema (DocumentChunk staging table, ICD10Code/CPTCode/ClinicalTerminology entities)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/DocumentChunkingService.cs` - Core chunking logic
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/IDocumentChunkingService.cs` - Service interface
- **NEW**: `src/backend/PatientAccess.Business/BackgroundJobs/ChunkDocumentsJob.cs` - Hangfire job
- **NEW**: `src/backend/PatientAccess.Data/Entities/DocumentChunk.cs` - Staging entity for pre-embedding chunks
- **NEW**: `src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddDocumentChunkStaging.cs` - EF migration
- **MODIFY**: `src/backend/PatientAccess.Data/ApplicationDbContext.cs` - Add DocumentChunk DbSet
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register IDocumentChunkingService
- **MODIFY**: `src/backend/PatientAccess.Business/PatientAccess.Business.csproj` - Add SharpToken package
- **NEW**: `src/backend/PatientAccess.Web/Controllers/KnowledgeBaseController.cs` - API endpoint for triggering re-indexing

## Implementation Plan

1. **Install SharpToken Package**
   - Add to `PatientAccess.Business.csproj`:
     ```xml
     <PackageReference Include="SharpToken" Version="2.0.3" />
     ```
   - SharpToken is C# port of OpenAI's Tiktoken library
   - Supports cl100k_base encoding (used by text-embedding-3-small)

2. **Create DocumentChunk Staging Entity**
   - File: `src/backend/PatientAccess.Data/Entities/DocumentChunk.cs`
   - Properties:
     - Id (Guid, PK)
     - CodeSystem (string, 50 chars: "ICD10", "CPT", "ClinicalTerminology")
     - SourceText (string, 2000 chars: original chunk text)
     - TokenCount (int: chunk size in tokens, should be ≤512)
     - ChunkIndex (int: sequence number within source document)
     - StartToken (int: starting token offset in source document)
     - EndToken (int: ending token offset in source document)
     - OverlapWithPrevious (bool: indicates if chunk overlaps with previous)
     - TargetEntityId (Guid, nullable, FK to ICD10Code/CPTCode/ClinicalTerminology after embedding)
     - IsProcessed (bool: true after embedding generated)
     - ProcessedAt (DateTime, nullable)
     - CreatedAt (DateTime, UTC)
   - Indexes:
     - CodeSystem (B-tree for filtering)
     - IsProcessed (B-tree for querying pending chunks)
     - TargetEntityId (B-tree for FK lookup)

3. **Create IDocumentChunkingService Interface**
   - File: `src/backend/PatientAccess.Business/Interfaces/IDocumentChunkingService.cs`
   - Methods:
     ```csharp
     Task<List<DocumentChunk>> ChunkICD10DocumentAsync(string documentText, CancellationToken cancellationToken);
     Task<List<DocumentChunk>> ChunkCPTDocumentAsync(string documentText, CancellationToken cancellationToken);
     Task<List<DocumentChunk>> ChunkClinicalTerminologyAsync(string documentText, CancellationToken cancellationToken);
     Task<int> GetTokenCountAsync(string text);
     ```

4. **Implement DocumentChunkingService**
   - File: `src/backend/PatientAccess.Business/Services/DocumentChunkingService.cs`
   - Constructor dependencies:
     - ILogger<DocumentChunkingService>
     - ApplicationDbContext (for persisting chunks)
   - Initialize Tiktoken encoder:
     ```csharp
     private readonly GptEncoding _encoder;
     
     public DocumentChunkingService(ILogger<DocumentChunkingService> logger, ApplicationDbContext context)
     {
         _logger = logger;
         _context = context;
         _encoder = GptEncoding.GetEncoding("cl100k_base"); // Matches text-embedding-3-small
     }
     ```
   - Core chunking logic (reusable for all code systems):
     ```csharp
     private List<DocumentChunk> ChunkDocument(string documentText, string codeSystem)
     {
         const int MAX_CHUNK_SIZE = 512;
         const int OVERLAP_SIZE = 64; // 12.5% of 512
         
         var tokens = _encoder.Encode(documentText);
         var chunks = new List<DocumentChunk>();
         
         int startToken = 0;
         int chunkIndex = 0;
         
         while (startToken < tokens.Count)
         {
             int endToken = Math.Min(startToken + MAX_CHUNK_SIZE, tokens.Count);
             var chunkTokens = tokens.Skip(startToken).Take(endToken - startToken).ToList();
             var chunkText = _encoder.Decode(chunkTokens);
             
             chunks.Add(new DocumentChunk
             {
                 Id = Guid.NewGuid(),
                 CodeSystem = codeSystem,
                 SourceText = chunkText,
                 TokenCount = chunkTokens.Count,
                 ChunkIndex = chunkIndex,
                 StartToken = startToken,
                 EndToken = endToken,
                 OverlapWithPrevious = chunkIndex > 0,
                 IsProcessed = false,
                 CreatedAt = DateTime.UtcNow
             });
             
             // Move forward by (MAX_CHUNK_SIZE - OVERLAP_SIZE)
             startToken += (MAX_CHUNK_SIZE - OVERLAP_SIZE);
             chunkIndex++;
         }
         
         return chunks;
     }
     ```
   - Implement ICD-10 chunking with code boundary detection:
     ```csharp
     public async Task<List<DocumentChunk>> ChunkICD10DocumentAsync(string documentText, CancellationToken cancellationToken)
     {
         _logger.LogInformation("Chunking ICD-10 document ({Length} chars)", documentText.Length);
         
         // Preserve ICD-10 code entries (avoid splitting mid-code)
         var preprocessedText = PreprocessICD10Document(documentText);
         var chunks = ChunkDocument(preprocessedText, "ICD10");
         
         await _context.DocumentChunks.AddRangeAsync(chunks, cancellationToken);
         await _context.SaveChangesAsync(cancellationToken);
         
         _logger.LogInformation("Created {ChunkCount} ICD-10 chunks", chunks.Count);
         return chunks;
     }
     
     private string PreprocessICD10Document(string documentText)
     {
         // Add newlines between ICD-10 codes to preserve boundaries
         // Example: "E11.9 Type 2 diabetes mellitus without complications\nE11.65 Type 2 diabetes with hyperglycemia"
         // Regex pattern: ICD-10 format (Letter + 2 digits + . + 1-2 digits)
         var pattern = @"([A-Z]\d{2}\.\d{1,2})";
         return Regex.Replace(documentText, pattern, "\n$1", RegexOptions.Multiline);
     }
     ```
   - Implement CPT chunking (similar logic with CPT code boundary detection)
   - Implement ClinicalTerminology chunking (simpler, no code boundary constraints)

5. **Create ChunkDocumentsJob Hangfire Job**
   - File: `src/backend/PatientAccess.Business/BackgroundJobs/ChunkDocumentsJob.cs`
   - Job parameters: CodeSystem (ICD10, CPT, ClinicalTerminology), SourceDocumentPath (file path or blob URI)
   - Job logic:
     ```csharp
     [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 900 })]
     public async Task ExecuteAsync(string codeSystem, string sourceDocumentPath, CancellationToken cancellationToken)
     {
         _logger.LogInformation("Starting chunking job for {CodeSystem} from {Path}", codeSystem, sourceDocumentPath);
         
         // 1. Load document text from file or blob storage
         var documentText = await LoadDocumentAsync(sourceDocumentPath, cancellationToken);
         
         // 2. Chunk based on code system
         List<DocumentChunk> chunks = codeSystem switch
         {
             "ICD10" => await _chunkingService.ChunkICD10DocumentAsync(documentText, cancellationToken),
             "CPT" => await _chunkingService.ChunkCPTDocumentAsync(documentText, cancellationToken),
             "ClinicalTerminology" => await _chunkingService.ChunkClinicalTerminologyAsync(documentText, cancellationToken),
             _ => throw new ArgumentException($"Invalid code system: {codeSystem}")
         };
         
         _logger.LogInformation("Chunking job completed: {ChunkCount} chunks created", chunks.Count);
     }
     ```
   - Integrate with Hangfire dashboard for monitoring

6. **Create KnowledgeBaseController for Re-indexing**
   - File: `src/backend/PatientAccess.Web/Controllers/KnowledgeBaseController.cs`
   - Endpoints:
     ```csharp
     [Authorize(Roles = "Admin")]
     [HttpPost("reindex/{codeSystem}")]
     public IActionResult TriggerReIndexing(string codeSystem, [FromBody] ReIndexRequest request)
     {
         var jobId = BackgroundJob.Enqueue<ChunkDocumentsJob>(
             job => job.ExecuteAsync(codeSystem, request.SourceDocumentPath, CancellationToken.None));
         
         return Ok(new { JobId = jobId, Status = "Enqueued" });
     }
     
     [Authorize(Roles = "Admin")]
     [HttpGet("chunks/{codeSystem}/pending")]
     public async Task<IActionResult> GetPendingChunks(string codeSystem)
     {
         var pendingChunks = await _context.DocumentChunks
             .Where(c => c.CodeSystem == codeSystem && !c.IsProcessed)
             .OrderBy(c => c.ChunkIndex)
             .ToListAsync();
         
         return Ok(new { Count = pendingChunks.Count, Chunks = pendingChunks });
     }
     ```
   - Authorization: Admin role only (re-indexing affects entire knowledge base)

7. **Add Error Handling and Retry Logic**
   - Use Polly for transient failure handling (file I/O, database writes)
   - Circuit breaker for external dependencies (blob storage)
   - Exponential backoff: 2s, 4s, 8s delays
   - Log all failures to Application Insights

8. **Implement Token Count Validation**
   - Ensure chunks ≤512 tokens (MANDATORY per AIR-R01)
   - Log warning if chunk exceeds limit (indicates bug)
   - Unit test: Assert all chunks ≤512 tokens

9. **Add Telemetry and Logging**
   - Log chunking metrics: chunk count, average token size, processing time
   - Application Insights custom events: "ChunkingJobStarted", "ChunkingJobCompleted"
   - Track token distribution histogram (useful for tuning)

10. **Update Program.cs for DI Registration**
    - Register IDocumentChunkingService:
      ```csharp
      builder.Services.AddScoped<IDocumentChunkingService, DocumentChunkingService>();
      ```
    - Register ChunkDocumentsJob with Hangfire

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── BackgroundJobs/
│   │   ├── ConfirmationEmailJob.cs (from EP-002)
│   │   └── SlotAvailabilityMonitor.cs (from EP-003)
│   ├── Services/
│   │   └── AzureDocumentIntelligenceService.cs (from EP-006-II)
│   └── Interfaces/
├── PatientAccess.Data/
│   ├── Entities/
│   │   ├── ClinicalDocument.cs (from EP-006-I)
│   │   ├── ICD10Code.cs (from task_001)
│   │   ├── CPTCode.cs (from task_001)
│   │   └── ClinicalTerminology.cs (from task_001)
│   └── ApplicationDbContext.cs
└── PatientAccess.Web/
    ├── Controllers/
    └── Program.cs
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/PatientAccess.Business/Services/DocumentChunkingService.cs | Core chunking logic |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IDocumentChunkingService.cs | Service interface |
| CREATE | src/backend/PatientAccess.Business/BackgroundJobs/ChunkDocumentsJob.cs | Hangfire job |
| CREATE | src/backend/PatientAccess.Data/Entities/DocumentChunk.cs | Staging entity |
| CREATE | src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddDocumentChunkStaging.cs | EF migration |
| CREATE | src/backend/PatientAccess.Web/Controllers/KnowledgeBaseController.cs | Re-indexing API |
| MODIFY | src/backend/PatientAccess.Data/ApplicationDbContext.cs | Add DocumentChunk DbSet |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register DI services |
| MODIFY | src/backend/PatientAccess.Business/PatientAccess.Business.csproj | Add SharpToken package |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### SharpToken Documentation
- **SharpToken GitHub**: https://github.com/dmitry-brazhenko/SharpToken
- **Tiktoken Documentation**: https://github.com/openai/tiktoken
- **cl100k_base Encoding**: Used by text-embedding-3-small, GPT-4, GPT-3.5-turbo

### Token Chunking Best Practices
- **OpenAI Embeddings Guide**: https://platform.openai.com/docs/guides/embeddings/use-cases
- **Chunk Size Recommendations**: 256-1024 tokens (512 is middle ground per AIR-R01)
- **Overlap Strategies**: 10-20% overlap (12.5% = 64/512 per AIR-R01)

### Hangfire Background Jobs
- **Hangfire Dashboard**: https://docs.hangfire.io/en/latest/background-methods/index.html
- **Automatic Retry**: https://docs.hangfire.io/en/latest/background-processing/dealing-with-exceptions.html

### Design Requirements
- **AIR-R01**: System MUST chunk medical coding documents into 512-token segments with 64-token (12.5%) overlap (design.md)
- **AIR-R04**: System MUST maintain separate vector indices for ICD-10 codes, CPT codes, and clinical terminology (design.md)

### Existing Codebase Patterns
- **Hangfire Jobs**: `src/backend/PatientAccess.Business/BackgroundJobs/ConfirmationEmailJob.cs`
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/AzureDocumentIntelligenceService.cs`

## Build Commands
```powershell
# Add SharpToken package
cd src/backend/PatientAccess.Business
dotnet add package SharpToken --version 2.0.3

# Create migration
cd ../PatientAccess.Data
dotnet ef migrations add AddDocumentChunkStaging --startup-project ../PatientAccess.Web

# Update database
dotnet ef database update --startup-project ../PatientAccess.Web

# Build solution
cd ..
dotnet build

# Run tests
cd PatientAccess.Tests
dotnet test
```

## Validation Strategy

### Unit Tests
- File: `src/backend/PatientAccess.Tests/Services/DocumentChunkingServiceTests.cs`
- Test cases:
  1. **Test_ChunkICD10Document_512TokenChunks_64TokenOverlap**
     - Input: ICD-10 document with 2000 tokens
     - Expected: 4 chunks (0-512, 448-960, 896-1408, 1344-1856, 1792-2000)
     - Assert: Chunk sizes ≤512 tokens, overlap = 64 tokens (12.5%)
  2. **Test_ChunkICD10Document_PreservesCodeBoundaries**
     - Input: "E11.9 Type 2 diabetes\nE11.65 Type 2 diabetes with hyperglycemia"
     - Expected: No ICD-10 code split across chunks
     - Assert: Each chunk contains complete code entries
  3. **Test_ChunkCPTDocument_HandlesModifiers**
     - Input: CPT codes with modifiers (99213-25, 80053)
     - Expected: Modifiers preserved within chunks
     - Assert: No modifier orphaned from parent code
  4. **Test_GetTokenCount_Accuracy**
     - Input: Known text samples
     - Expected: Token counts match Tiktoken cl100k_base encoder
     - Assert: Token count = expected value
  5. **Test_ChunkDocument_EmptyInput_ReturnsEmptyList**
     - Input: Empty string
     - Expected: Empty list
     - Assert: chunks.Count == 0

### Integration Tests
- File: `src/backend/PatientAccess.Tests/BackgroundJobs/ChunkDocumentsJobTests.cs`
- Test cases:
  1. **Test_ChunkDocumentsJob_ICD10_CreatesChunksInDatabase**
     - Enqueue Hangfire job with ICD-10 document
     - Wait for job completion
     - Assert: DocumentChunk records exist with CodeSystem = "ICD10"
  2. **Test_ChunkDocumentsJob_ReIndex_DoesNotAffectLiveData**
     - Insert existing ICD10Code records
     - Trigger re-indexing job
     - Assert: Existing ICD10Code records unchanged during chunking

### Acceptance Criteria Validation
- **AC1**: ✅ Documents chunked into 512-token segments with 64-token overlap (12.5%)
- **AC2**: ✅ Separate chunking pipelines for ICD-10, CPT, clinical terminology
- **Edge Case**: ✅ Re-indexing API endpoint allows triggering without affecting live queries

## Success Criteria Checklist
- [MANDATORY] SharpToken library installed and cl100k_base encoder initialized
- [MANDATORY] DocumentChunk entity created with CodeSystem, SourceText, TokenCount
- [MANDATORY] DocumentChunkingService implements IDocumentChunkingService interface
- [MANDATORY] ChunkICD10DocumentAsync creates chunks ≤512 tokens with 64-token overlap
- [MANDATORY] ChunkCPTDocumentAsync creates chunks ≤512 tokens with 64-token overlap
- [MANDATORY] ChunkClinicalTerminologyAsync creates chunks ≤512 tokens with 64-token overlap
- [MANDATORY] ICD-10 code boundary detection prevents mid-code splits
- [MANDATORY] ChunkDocumentsJob Hangfire job processes documents asynchronously
- [MANDATORY] KnowledgeBaseController /reindex endpoint triggers chunking job (Admin role)
- [MANDATORY] Unit test: All chunks ≤512 tokens
- [MANDATORY] Unit test: Overlap = 64 tokens (12.5%)
- [MANDATORY] Integration test: ChunkDocumentsJob persists DocumentChunk records
- [MANDATORY] Logging: Chunk count, average token size, processing time
- [RECOMMENDED] Polly retry logic for transient failures (3 retries, exponential backoff)
- [RECOMMENDED] Application Insights telemetry: "ChunkingJobStarted", "ChunkingJobCompleted"

## Estimated Effort
**4 hours** (Service implementation + Hangfire integration + unit tests)
