# Task - task_004_be_hybrid_retrieval_service

## Requirement Reference
- User Story: US_050
- Story Location: .propel/context/tasks/EP-008/us_050/us_050.md
- Acceptance Criteria:
    - **AC3**: Given a code mapping query is submitted, When retrieval executes, Then top-5 chunks with cosine similarity above 0.75 are returned (AIR-R02) using hybrid retrieval combining semantic similarity and keyword matching (AIR-R03).
    - **AC4**: Given the knowledge base is populated, When I query for a clinical term like "Type 2 Diabetes Mellitus", Then relevant ICD-10 codes (e.g., E11.x) and related CPT codes are retrieved with similarity scores.
- Edge Case:
    - What happens when a query term has no matches above 0.75 threshold? System returns "No confident matches found" and falls back to keyword-only search with reduced confidence scoring.

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
| Library | Npgsql.EntityFrameworkCore.PostgreSQL | 8.0 |
| Database | PostgreSQL | 16.x |
| Vector Store | pgvector | 0.5+ |
| Caching | Upstash Redis | Redis 7.x compatible |
| AI Gateway | Azure OpenAI Service | text-embedding-3-small |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-R02 (Top-5 retrieval, cosine >0.75), AIR-R03 (Hybrid retrieval: semantic + keyword), AIR-R04 (Separate indices per code system) |
| **AI Pattern** | RAG Hybrid Retrieval (Semantic + Keyword) |
| **Prompt Template Path** | N/A (Retrieval is vector similarity + FTS, no LLM inference) |
| **Guardrails Config** | N/A (Deterministic retrieval, no LLM generation) |
| **Model Provider** | Azure OpenAI (text-embedding-3-small for query embeddings) |

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

Create hybrid retrieval service combining semantic similarity (pgvector cosine distance) with keyword matching (PostgreSQL full-text search) to retrieve top-5 medical codes per AIR-R02 and AIR-R03. This task implements the final stage of the RAG knowledge base, enabling queries like "Type 2 Diabetes Mellitus" to return relevant ICD-10 codes (E11.x) and CPT codes with confidence scores. The service handles edge cases (no matches above 0.75 threshold triggers keyword-only fallback), supports separate retrieval per code system (ICD10, CPT, ClinicalTerminology per AIR-R04), and includes Redis caching for performance (<200ms retrieval). The API endpoint integrates with the medical coding pipeline (US_051+).

**Key Capabilities:**
- HybridRetrievalService with semantic + keyword search
- Query embedding generation (Azure OpenAI text-embedding-3-small)
- pgvector cosine similarity search (ORDER BY Embedding <-> query_vector)
- PostgreSQL full-text search (FTS) for keyword matching
- Hybrid scoring: 0.7 * semantic_score + 0.3 * keyword_score (configurable weights)
- Top-5 retrieval with cosine >0.75 threshold per AIR-R02
- Fallback to keyword-only if no semantic matches >0.75
- Separate retrieval per code system (ICD10, CPT, ClinicalTerminology per AIR-R04)
- Redis caching (15-minute TTL) for frequent queries
- REST API endpoint: GET /api/knowledge/search
- Response includes: code, description, similarity score, match type (semantic/keyword/hybrid)

## Dependent Tasks
- EP-008: US_050: task_001_db_vector_indices_schema (ICD10Code/CPTCode/ClinicalTerminology entities with vector columns and JSONB)
- EP-008: US_050: task_003_be_embedding_generation_service (IEmbeddingGenerationService for query embeddings)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Business/Services/HybridRetrievalService.cs` - Core retrieval logic
- **NEW**: `src/backend/PatientAccess.Business/Interfaces/IHybridRetrievalService.cs` - Service interface
- **NEW**: `src/backend/PatientAccess.Business/DTOs/CodeRetrievalResultDto.cs` - Response DTO
- **NEW**: `src/backend/PatientAccess.Business/DTOs/CodeSearchRequestDto.cs` - Request DTO
- **NEW**: `src/backend/PatientAccess.Web/Controllers/KnowledgeController.cs` - REST API endpoint
- **MODIFY**: `src/backend/PatientAccess.Web/Program.cs` - Register IHybridRetrievalService
- **MODIFY**: `src/backend/PatientAccess.Data/ApplicationDbContext.cs` - Add FTS configuration for JSONB metadata
- **NEW**: `src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddFullTextSearch.cs` - Add FTS indexes

## Implementation Plan

1. **Create CodeSearchRequestDto**
   - File: `src/backend/PatientAccess.Business/DTOs/CodeSearchRequestDto.cs`
   - Properties:
     ```csharp
     public class CodeSearchRequestDto
     {
         [Required]
         [StringLength(500, MinimumLength = 2)]
         public string Query { get; set; } // e.g., "Type 2 Diabetes Mellitus"
         
         [Required]
         public string CodeSystem { get; set; } // "ICD10", "CPT", or "ClinicalTerminology"
         
         public int TopK { get; set; } = 5; // Default top-5 per AIR-R02
         
         public double MinSimilarityThreshold { get; set; } = 0.75; // Per AIR-R02
     }
     ```

2. **Create CodeRetrievalResultDto**
   - File: `src/backend/PatientAccess.Business/DTOs/CodeRetrievalResultDto.cs`
   - Properties:
     ```csharp
     public class CodeRetrievalResultDto
     {
         public string Code { get; set; } // ICD-10 code: "E11.9"
         public string Description { get; set; } // "Type 2 diabetes mellitus without complications"
         public string Category { get; set; } // "Endocrine, nutritional and metabolic diseases"
         public double SimilarityScore { get; set; } // Cosine similarity (0-1)
         public double KeywordScore { get; set; } // FTS rank (0-1 normalized)
         public double FinalScore { get; set; } // Hybrid: 0.7*semantic + 0.3*keyword
         public string MatchType { get; set; } // "Semantic", "Keyword", "Hybrid"
         public Dictionary<string, object> Metadata { get; set; } // JSONB metadata
     }
     
     public class CodeSearchResponseDto
     {
         public string Query { get; set; }
         public string CodeSystem { get; set; }
         public int ResultCount { get; set; }
         public List<CodeRetrievalResultDto> Results { get; set; }
         public string Message { get; set; } // "No confident matches found" if fallback triggered
     }
     ```

3. **Create IHybridRetrievalService Interface**
   - File: `src/backend/PatientAccess.Business/Interfaces/IHybridRetrievalService.cs`
   - Methods:
     ```csharp
     Task<CodeSearchResponseDto> SearchAsync(CodeSearchRequestDto request, CancellationToken cancellationToken);
     Task<List<CodeRetrievalResultDto>> SemanticSearchAsync(string query, string codeSystem, int topK, double minThreshold, CancellationToken cancellationToken);
     Task<List<CodeRetrievalResultDto>> KeywordSearchAsync(string query, string codeSystem, int topK, CancellationToken cancellationToken);
     Task<List<CodeRetrievalResultDto>> HybridSearchAsync(string query, string codeSystem, int topK, double minThreshold, CancellationToken cancellationToken);
     ```

4. **Implement HybridRetrievalService**
   - File: `src/backend/PatientAccess.Business/Services/HybridRetrievalService.cs`
   - Constructor dependencies:
     - ILogger<HybridRetrievalService>
     - ApplicationDbContext
     - IEmbeddingGenerationService (for query embeddings)
     - IDistributedCache (Redis for caching)
   - Implement semantic search (pgvector cosine similarity):
     ```csharp
     public async Task<List<CodeRetrievalResultDto>> SemanticSearchAsync(
         string query, string codeSystem, int topK, double minThreshold, CancellationToken cancellationToken)
     {
         // 1. Generate query embedding
         var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);
         
         // 2. Execute pgvector cosine similarity query
         List<CodeRetrievalResultDto> results = codeSystem switch
         {
             "ICD10" => await SemanticSearchICD10Async(queryEmbedding, topK, minThreshold, cancellationToken),
             "CPT" => await SemanticSearchCPTAsync(queryEmbedding, topK, minThreshold, cancellationToken),
             "ClinicalTerminology" => await SemanticSearchClinicalAsync(queryEmbedding, topK, minThreshold, cancellationToken),
             _ => throw new ArgumentException($"Invalid code system: {codeSystem}")
         };
         
         return results;
     }
     
     private async Task<List<CodeRetrievalResultDto>> SemanticSearchICD10Async(
         List<float> queryEmbedding, int topK, double minThreshold, CancellationToken cancellationToken)
     {
         var results = await _context.ICD10Codes
             .Where(c => c.IsActive) // Only active codes
             .OrderBy(c => c.Embedding.CosineDistance(queryEmbedding)) // pgvector cosine similarity
             .Take(topK)
             .Select(c => new 
             {
                 c.Code,
                 c.Description,
                 c.Category,
                 c.Metadata,
                 Similarity = 1 - c.Embedding.CosineDistance(queryEmbedding) // Convert distance to similarity
             })
             .ToListAsync(cancellationToken);
         
         return results
             .Where(r => r.Similarity >= minThreshold) // Filter by threshold
             .Select(r => new CodeRetrievalResultDto
             {
                 Code = r.Code,
                 Description = r.Description,
                 Category = r.Category,
                 SimilarityScore = r.Similarity,
                 KeywordScore = 0,
                 FinalScore = r.Similarity,
                 MatchType = "Semantic",
                 Metadata = r.Metadata
             })
             .ToList();
     }
     ```
   - Implement keyword search (PostgreSQL FTS):
     ```csharp
     public async Task<List<CodeRetrievalResultDto>> KeywordSearchAsync(
         string query, string codeSystem, int topK, CancellationToken cancellationToken)
     {
         List<CodeRetrievalResultDto> results = codeSystem switch
         {
             "ICD10" => await KeywordSearchICD10Async(query, topK, cancellationToken),
             "CPT" => await KeywordSearchCPTAsync(query, topK, cancellationToken),
             "ClinicalTerminology" => await KeywordSearchClinicalAsync(query, topK, cancellationToken),
             _ => throw new ArgumentException($"Invalid code system: {codeSystem}")
         };
         
         return results;
     }
     
     private async Task<List<CodeRetrievalResultDto>> KeywordSearchICD10Async(
         string query, int topK, CancellationToken cancellationToken)
     {
         // PostgreSQL FTS using to_tsvector and ts_rank
         var results = await _context.ICD10Codes
             .FromSqlRaw(@"
                 SELECT 
                     ""Id"", ""Code"", ""Description"", ""Category"", ""Metadata"",
                     ts_rank(to_tsvector('english', ""Description""), plainto_tsquery('english', {0})) AS rank
                 FROM ""ICD10Codes""
                 WHERE ""IsActive"" = true
                     AND to_tsvector('english', ""Description"") @@ plainto_tsquery('english', {0})
                 ORDER BY rank DESC
                 LIMIT {1}
             ", query, topK)
             .Select(c => new CodeRetrievalResultDto
             {
                 Code = c.Code,
                 Description = c.Description,
                 Category = c.Category,
                 SimilarityScore = 0,
                 KeywordScore = 0, // Populated by raw SQL rank
                 FinalScore = 0, // Populated after normalization
                 MatchType = "Keyword",
                 Metadata = c.Metadata
             })
             .ToListAsync(cancellationToken);
         
         // Normalize keyword scores to 0-1 range
         if (results.Any())
         {
             var maxRank = results.Max(r => r.KeywordScore);
             foreach (var result in results)
             {
                 result.KeywordScore /= maxRank;
                 result.FinalScore = result.KeywordScore;
             }
         }
         
         return results;
     }
     ```
   - Implement hybrid search (combine semantic + keyword):
     ```csharp
     public async Task<List<CodeRetrievalResultDto>> HybridSearchAsync(
         string query, string codeSystem, int topK, double minThreshold, CancellationToken cancellationToken)
     {
         const double SEMANTIC_WEIGHT = 0.7;
         const double KEYWORD_WEIGHT = 0.3;
         
         // Execute both searches in parallel
         var semanticTask = SemanticSearchAsync(query, codeSystem, topK * 2, 0, cancellationToken); // Expand for merging
         var keywordTask = KeywordSearchAsync(query, codeSystem, topK * 2, cancellationToken);
         
         await Task.WhenAll(semanticTask, keywordTask);
         
         var semanticResults = semanticTask.Result.ToDictionary(r => r.Code);
         var keywordResults = keywordTask.Result.ToDictionary(r => r.Code);
         
         // Merge results
         var allCodes = semanticResults.Keys.Union(keywordResults.Keys).ToList();
         var hybridResults = allCodes.Select(code =>
         {
             var semanticScore = semanticResults.ContainsKey(code) ? semanticResults[code].SimilarityScore : 0;
             var keywordScore = keywordResults.ContainsKey(code) ? keywordResults[code].KeywordScore : 0;
             var finalScore = SEMANTIC_WEIGHT * semanticScore + KEYWORD_WEIGHT * keywordScore;
             
             var result = semanticResults.ContainsKey(code) ? semanticResults[code] : keywordResults[code];
             result.SimilarityScore = semanticScore;
             result.KeywordScore = keywordScore;
             result.FinalScore = finalScore;
             result.MatchType = semanticScore > 0 && keywordScore > 0 ? "Hybrid" 
                              : semanticScore > 0 ? "Semantic" 
                              : "Keyword";
             
             return result;
         })
         .OrderByDescending(r => r.FinalScore)
         .Take(topK)
         .ToList();
         
         return hybridResults;
     }
     ```
   - Implement SearchAsync with fallback logic:
     ```csharp
     public async Task<CodeSearchResponseDto> SearchAsync(
         CodeSearchRequestDto request, CancellationToken cancellationToken)
     {
         // Check Redis cache
         var cacheKey = $"knowledge:search:{request.CodeSystem}:{request.Query}:{request.TopK}";
         var cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);
         if (!string.IsNullOrEmpty(cachedResult))
         {
             _logger.LogInformation("Cache hit for query: {Query}", request.Query);
             return JsonSerializer.Deserialize<CodeSearchResponseDto>(cachedResult);
         }
         
         // Try hybrid search first
         var results = await HybridSearchAsync(
             request.Query, request.CodeSystem, request.TopK, request.MinSimilarityThreshold, cancellationToken);
         
         var response = new CodeSearchResponseDto
         {
             Query = request.Query,
             CodeSystem = request.CodeSystem,
             Results = results,
             ResultCount = results.Count
         };
         
         // Fallback to keyword-only if no semantic matches above threshold
         if (results.All(r => r.SimilarityScore < request.MinSimilarityThreshold))
         {
             _logger.LogWarning("No semantic matches above {Threshold} for query: {Query}", 
                 request.MinSimilarityThreshold, request.Query);
             
             results = await KeywordSearchAsync(request.Query, request.CodeSystem, request.TopK, cancellationToken);
             response.Results = results;
             response.ResultCount = results.Count;
             response.Message = "No confident matches found. Showing keyword-based results with reduced confidence.";
         }
         
         // Cache for 15 minutes
         var cacheOptions = new DistributedCacheEntryOptions
         {
             AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
         };
         await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), cacheOptions, cancellationToken);
         
         return response;
     }
     ```

5. **Create KnowledgeController**
   - File: `src/backend/PatientAccess.Web/Controllers/KnowledgeController.cs`
   - Endpoints:
     ```csharp
     [ApiController]
     [Route("api/knowledge")]
     [Authorize] // Requires authentication
     public class KnowledgeController : ControllerBase
     {
         private readonly IHybridRetrievalService _retrievalService;
         private readonly ILogger<KnowledgeController> _logger;
         
         [HttpGet("search")]
         [ProducesResponseType(typeof(CodeSearchResponseDto), StatusCodes.Status200OK)]
         [ProducesResponseType(StatusCodes.Status400BadRequest)]
         public async Task<IActionResult> Search(
             [FromQuery] string query,
             [FromQuery] string codeSystem,
             [FromQuery] int topK = 5,
             [FromQuery] double minSimilarityThreshold = 0.75,
             CancellationToken cancellationToken = default)
         {
             if (string.IsNullOrWhiteSpace(query))
                 return BadRequest("Query cannot be empty");
             
             if (!new[] { "ICD10", "CPT", "ClinicalTerminology" }.Contains(codeSystem))
                 return BadRequest("Invalid code system. Must be ICD10, CPT, or ClinicalTerminology");
             
             var request = new CodeSearchRequestDto
             {
                 Query = query,
                 CodeSystem = codeSystem,
                 TopK = topK,
                 MinSimilarityThreshold = minSimilarityThreshold
             };
             
             var response = await _retrievalService.SearchAsync(request, cancellationToken);
             
             return Ok(response);
         }
         
         [HttpGet("search/{codeSystem}/{code}")]
         [ProducesResponseType(typeof(CodeRetrievalResultDto), StatusCodes.Status200OK)]
         [ProducesResponseType(StatusCodes.Status404NotFound)]
         public async Task<IActionResult> GetCodeDetails(string codeSystem, string code)
         {
             // Direct lookup by code (for when code is known)
             // Implementation omitted for brevity
         }
     }
     ```

6. **Add Full-Text Search Indexes**
   - Create EF Core migration: `Add-Migration AddFullTextSearch`
   - File: `src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddFullTextSearch.cs`
   - Add FTS indexes:
     ```sql
     CREATE INDEX idx_icd10_description_fts ON "ICD10Codes" USING gin(to_tsvector('english', "Description"));
     CREATE INDEX idx_cpt_description_fts ON "CPTCodes" USING gin(to_tsvector('english', "Description"));
     CREATE INDEX idx_clinical_term_fts ON "ClinicalTerminology" USING gin(to_tsvector('english', "Term"));
     ```

7. **Configure Redis Caching in Program.cs**
   - Register IDistributedCache:
     ```csharp
     builder.Services.AddStackExchangeRedisCache(options =>
     {
         options.Configuration = builder.Configuration.GetConnectionString("Redis");
         options.InstanceName = "KnowledgeBase:";
     });
     
     builder.Services.AddScoped<IHybridRetrievalService, HybridRetrievalService>();
     ```

8. **Add Logging and Telemetry**
   - Log query metrics: query, code system, result count, retrieval time, match type
   - Application Insights custom events: "KnowledgeSearchExecuted", "SemanticSearchFallback"
   - Track cache hit rate (cache hits / total queries)

9. **Implement Performance Optimization**
   - pgvector HNSW index for fast approximate nearest neighbor search
   - Redis caching for frequent queries (15-minute TTL)
   - Database connection pooling (EF Core default)
   - Parallel execution of semantic + keyword searches

10. **Add Input Validation and Error Handling**
    - Validate query length (2-500 characters)
    - Validate code system (ICD10, CPT, ClinicalTerminology only)
    - Validate topK (1-20 range)
    - Handle Azure OpenAI failures (retry with Polly)
    - Return meaningful error messages

## Current Project State

```
src/backend/
├── PatientAccess.Business/
│   ├── Services/
│   │   ├── EmbeddingGenerationService.cs (from task_003)
│   │   └── DocumentChunkingService.cs (from task_002)
│   └── Interfaces/
│       ├── IEmbeddingGenerationService.cs (from task_003)
│       └── IDocumentChunkingService.cs (from task_002)
├── PatientAccess.Data/
│   ├── Entities/
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
| CREATE | src/backend/PatientAccess.Business/Services/HybridRetrievalService.cs | Core retrieval logic |
| CREATE | src/backend/PatientAccess.Business/Interfaces/IHybridRetrievalService.cs | Service interface |
| CREATE | src/backend/PatientAccess.Business/DTOs/CodeRetrievalResultDto.cs | Response DTO |
| CREATE | src/backend/PatientAccess.Business/DTOs/CodeSearchRequestDto.cs | Request DTO |
| CREATE | src/backend/PatientAccess.Web/Controllers/KnowledgeController.cs | REST API endpoint |
| CREATE | src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddFullTextSearch.cs | FTS indexes |
| MODIFY | src/backend/PatientAccess.Web/Program.cs | Register IHybridRetrievalService, Redis cache |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add Redis connection string |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### pgvector Similarity Search
- **Cosine Distance**: https://github.com/pgvector/pgvector#distances
- **HNSW Index**: https://github.com/pgvector/pgvector#hnsw (Hierarchical Navigable Small World)
- **Query Performance**: https://github.com/pgvector/pgvector#query-performance

### PostgreSQL Full-Text Search
- **FTS Tutorial**: https://www.postgresql.org/docs/16/textsearch.html
- **ts_rank Function**: https://www.postgresql.org/docs/16/textsearch-controls.html#TEXTSEARCH-RANKING
- **GIN Indexes**: https://www.postgresql.org/docs/16/gin-intro.html

### Hybrid Search Best Practices
- **Combining Vector + Keyword**: https://www.pinecone.io/learn/hybrid-search/
- **Score Normalization**: Min-max scaling, z-score normalization
- **Weighting Strategies**: 0.7 semantic + 0.3 keyword (common default)

### Redis Caching
- **IDistributedCache**: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed
- **StackExchangeRedisCache**: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed#distributed-redis-cache

### Design Requirements
- **AIR-R02**: System MUST retrieve top-5 chunks with cosine similarity score above 0.75 threshold (design.md)
- **AIR-R03**: System MUST implement hybrid retrieval combining semantic similarity with keyword matching (design.md)
- **AIR-R04**: System MUST maintain separate vector indices for ICD-10 codes, CPT codes, and clinical terminology (design.md)

### Existing Codebase Patterns
- **REST API Controller**: `src/backend/PatientAccess.Web/Controllers/AppointmentsController.cs`
- **Service Pattern**: `src/backend/PatientAccess.Business/Services/EmbeddingGenerationService.cs`

## Build Commands
```powershell
# Create FTS migration
cd src/backend/PatientAccess.Data
dotnet ef migrations add AddFullTextSearch --startup-project ../PatientAccess.Web

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
- File: `src/backend/PatientAccess.Tests/Services/HybridRetrievalServiceTests.cs`
- Test cases:
  1. **Test_SemanticSearch_ReturnsTop5Results_Above75ThresholdTest_SemanticSearch_ReturnsTop5Results_Above75Threshold**
     - Setup: Insert 20 ICD10Code records with embeddings
     - Input: Query "Type 2 Diabetes Mellitus"
     - Expected: ≤5 results, all with SimilarityScore ≥0.75
     - Assert: results.Count ≤ 5, results.All(r => r.SimilarityScore >= 0.75)
  2. **Test_KeywordSearch_ReturnsFTSResults**
     - Setup: Insert ICD10Code records with "diabetes" in Description
     - Input: Query "diabetes"
     - Expected: Results ranked by FTS score
     - Assert: results.Any(r => r.Description.Contains("diabetes"))
  3. **Test_HybridSearch_CombinesSemanticAndKeyword**
     - Setup: Insert records with varying semantic/keyword match strengths
     - Input: Query matching both semantic and keyword
     - Expected: Hybrid scores = 0.7*semantic + 0.3*keyword
     - Assert: Verify FinalScore calculation
  4. **Test_SearchAsync_FallbackToKeywordOnly_WhenNoSemanticMatchesAboveThreshold**
     - Setup: Insert records with low semantic similarity
     - Input: Query with semantic scores <0.75
     - Expected: Fallback to keyword search, response.Message contains "No confident matches found"
     - Assert: response.Message != null, results.All(r => r.MatchType == "Keyword")
  5. **Test_SearchAsync_UsesCachedResults**
     - Execute: Search query twice
     - Mock: Redis cache returns cached result on second call
     - Assert: EmbeddingGenerationService.GenerateEmbeddingAsync called once (not twice)

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Controllers/KnowledgeControllerTests.cs`
- Test cases:
  1. **Test_KnowledgeController_SearchEndpoint_ReturnsTop5ICD10Codes**
     - Setup: Populate ICD10Codes table with embeddings
     - Request: GET /api/knowledge/search?query=Type%202%20Diabetes&codeSystem=ICD10&topK=5
     - Expected: 200 OK, results.Count ≤ 5, results contain E11.x codes
     - Assert: Response status 200, results[0].Code.StartsWith("E11")
  2. **Test_KnowledgeController_SearchEndpoint_ReturnsError_InvalidCodeSystem**
     - Request: GET /api/knowledge/search?query=diabetes&codeSystem=InvalidSystem
     - Expected: 400 Bad Request
     - Assert: Response status 400
  3. **Test_HybridSearch_Performance_Under200ms**
     - Setup: 1000 ICD10Code records with embeddings
     - Request: GET /api/knowledge/search?query=hypertension&codeSystem=ICD10
     - Measure: Response time
     - Assert: Response time <200ms (includes embedding generation, vector search, FTS)

### Acceptance Criteria Validation
- **AC3**: ✅ Top-5 chunks with cosine >0.75 returned using hybrid retrieval (semantic + keyword)
- **AC4**: ✅ Query "Type 2 Diabetes Mellitus" returns relevant ICD-10 codes (E11.x) with similarity scores
- **Edge Case**: ✅ No matches above 0.75 triggers fallback to keyword-only with reduced confidence message

## Success Criteria Checklist
- [x] [MANDATORY] HybridRetrievalService implements IHybridRetrievalService interface
- [x] [MANDATORY] SemanticSearchAsync uses pgvector cosine similarity (ORDER BY Embedding <-> query_vector)
- [x] [MANDATORY] KeywordSearchAsync uses PostgreSQL FTS (to_tsvector, ts_rank)
- [x] [MANDATORY] HybridSearchAsync combines semantic + keyword with 0.7/0.3 weighting
- [x] [MANDATORY] SearchAsync returns top-5 results with cosine >0.75 (AIR-R02)
- [x] [MANDATORY] SearchAsync falls back to keyword-only if no semantic matches >0.75
- [x] [MANDATORY] Separate retrieval per code system (ICD10, CPT, ClinicalTerminology per AIR-R04)
- [x] [MANDATORY] KnowledgeController GET /api/knowledge/search endpoint implemented
- [x] [MANDATORY] Redis caching with 15-minute TTL
- [x] [MANDATORY] CodeRetrievalResultDto includes Code, Description, SimilarityScore, MatchType
- [x] [MANDATORY] FTS indexes created (idx_icd10_description_fts, idx_cpt_description_fts, idx_clinical_term_fts)
- [ ] [MANDATORY] Unit test: Semantic search returns results with SimilarityScore ≥0.75
- [ ] [MANDATORY] Unit test: Hybrid search calculates FinalScore = 0.7*semantic + 0.3*keyword
- [ ] [MANDATORY] Unit test: Fallback triggered when no semantic matches >0.75
- [ ] [MANDATORY] Integration test: Query "Type 2 Diabetes Mellitus" returns E11.x codes
- [ ] [MANDATORY] Integration test: Response time <200ms for top-5 retrieval
- [ ] [RECOMMENDED] Application Insights telemetry: "KnowledgeSearchExecuted", "SemanticSearchFallback"
- [ ] [RECOMMENDED] Cache hit rate monitoring (logged or tracked in Application Insights)

## Estimated Effort
**6 hours** (Service implementation + pgvector + FTS + Redis caching + unit tests)
