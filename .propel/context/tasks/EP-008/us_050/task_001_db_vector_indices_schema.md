# Task - task_001_db_vector_indices_schema

## Requirement Reference
- User Story: US_050
- Story Location: .propel/context/tasks/EP-008/us_050/us_050.md
- Acceptance Criteria:
    - **AC2**: Given chunks are created, When embeddings are generated, Then Azure OpenAI text-embedding-3-small produces 1536-dimensional vectors stored in separate pgvector indices for ICD-10, CPT, and clinical terminology (AIR-R04).
    - **AC4**: Given the knowledge base is populated, When I query for a clinical term like "Type 2 Diabetes Mellitus", Then relevant ICD-10 codes (e.g., E11.x) and related CPT codes are retrieved with similarity scores.
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
| Database | PostgreSQL | 16.x |
| Library | Entity Framework Core | 8.0 |
| Library | Npgsql.EntityFrameworkCore.PostgreSQL | 8.0 |
| Vector Store | pgvector | 0.5+ |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | Yes |
| **AIR Requirements** | AIR-R04 (Separate vector indices for ICD-10, CPT, clinical terminology) |
| **AI Pattern** | RAG Knowledge Base |
| **Prompt Template Path** | N/A (Infrastructure task - no prompts) |
| **Guardrails Config** | N/A (Infrastructure task) |
| **Model Provider** | Azure OpenAI (text-embedding-3-small for embeddings) |

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

Create PostgreSQL database schema with pgvector extension to store medical coding knowledge base embeddings. This task implements three separate vector indices for ICD-10 codes, CPT codes, and clinical terminology per AIR-R04, enabling efficient semantic retrieval for medical code mapping. Each table stores 1536-dimensional embeddings (text-embedding-3-small), original text content, metadata (code system, version, category), and supports re-indexing without affecting live queries. The schema design enables hybrid retrieval (semantic + keyword) with cosine similarity search while maintaining data lineage and version control.

**Key Capabilities:**
- pgvector extension installation and configuration
- ICD10Codes table with vector(1536) embeddings and metadata
- CPTCodes table with vector(1536) embeddings and metadata
- ClinicalTerminology table with vector(1536) embeddings and metadata
- Vector indexes using vector_cosine_ops for fast similarity search
- GIN indexes on metadata JSONB columns for keyword search
- B-tree indexes on code values for exact matching
- Version tracking for code system updates
- Soft delete support for deprecated codes
- Partition strategy for large-scale datasets (>1M entries)

## Dependent Tasks
- None (foundational infrastructure task)

## Impacted Components
- **NEW**: `src/backend/PatientAccess.Data/Entities/ICD10Code.cs` - ICD-10 knowledge base entity
- **NEW**: `src/backend/PatientAccess.Data/Entities/CPTCode.cs` - CPT knowledge base entity
- **NEW**: `src/backend/PatientAccess.Data/Entities/ClinicalTerminology.cs` - Clinical terms entity
- **NEW**: `src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddVectorIndices.cs` - EF Core migration
- **MODIFY**: `src/backend/PatientAccess.Data/ApplicationDbContext.cs` - Add DbSet properties and configure pgvector
- **NEW**: `scripts/migrations/001_enable_pgvector.sql` - Manual pgvector extension script (run before EF migrations)

## Implementation Plan

1. **Enable pgvector Extension**
   - Create migration script: `scripts/migrations/001_enable_pgvector.sql`
   - Content:
     ```sql
     -- Enable pgvector extension
     CREATE EXTENSION IF NOT EXISTS vector;
     
     -- Verify installation
     SELECT * FROM pg_extension WHERE extname = 'vector';
     
     -- Test vector operations
     SELECT '[1,2,3]'::vector(3) <-> '[4,5,6]'::vector(3) AS cosine_distance;
     ```
   - Execute manually before running EF Core migrations
   - Document in DATABASE_SETUP.md

2. **Create ICD10Code Entity**
   - File: `src/backend/PatientAccess.Data/Entities/ICD10Code.cs`
   - Properties:
     - Id (Guid, PK)
     - Code (string, 20 chars, indexed, unique: "E11.9", "E11.65")
     - Description (string, 1000 chars: full ICD-10 code description)
     - Category (string, 200 chars: "Endocrine, nutritional and metabolic diseases")
     - ChapterCode (string, 10 chars: "E00-E89")
     - Embedding (Vector, 1536 dimensions, pgvector type)
     - ChunkText (string, 2000 chars: original text used for embedding)
     - Metadata (JSONB: {version, effectiveDate, status, subcategories, relatedCodes})
     - Version (string, 20 chars: "ICD-10-CM-2024")
     - IsActive (bool, default true, false for deprecated codes)
     - CreatedAt (DateTime, UTC)
     - UpdatedAt (DateTime, UTC)
   - Indexes:
     - Code (B-tree, unique)
     - Embedding (vector_cosine_ops for cosine similarity)
     - Metadata (GIN for JSONB keyword search)
     - IsActive (B-tree for filtering active codes)

3. **Create CPTCode Entity**
   - File: `src/backend/PatientAccess.Data/Entities/CPTCode.cs`
   - Properties:
     - Id (Guid, PK)
     - Code (string, 10 chars, indexed, unique: "99213", "80053")
     - Description (string, 1000 chars: full CPT code description)
     - Category (string, 200 chars: "Evaluation and Management", "Laboratory Procedures")
     - Modifier (string, 50 chars, nullable: "25", "59")
     - Embedding (Vector, 1536 dimensions, pgvector type)
     - ChunkText (string, 2000 chars)
     - Metadata (JSONB: {version, effectiveDate, status, rvuValue, billingGuidelines})
     - Version (string, 20 chars: "CPT-2024")
     - IsActive (bool, default true)
     - CreatedAt (DateTime, UTC)
     - UpdatedAt (DateTime, UTC)
   - Indexes:
     - Code (B-tree, unique)
     - Embedding (vector_cosine_ops)
     - Metadata (GIN)
     - IsActive (B-tree)

4. **Create ClinicalTerminology Entity**
   - File: `src/backend/PatientAccess.Data/Entities/ClinicalTerminology.cs`
   - Properties:
     - Id (Guid, PK)
     - Term (string, 500 chars, indexed: "Type 2 Diabetes Mellitus", "Hypertension")
     - Category (string, 100 chars: "Diagnosis", "Medication", "Procedure", "Symptom")
     - Synonyms (List<string>, JSONB array: ["T2DM", "NIDDM", "Adult-onset diabetes"])
     - Embedding (Vector, 1536 dimensions, pgvector type)
     - ChunkText (string, 2000 chars)
     - Metadata (JSONB: {source, mappedICD10Codes, mappedCPTCodes, clinicalContext})
     - Source (string, 100 chars: "SNOMED-CT", "LOINC", "Internal")
     - IsActive (bool, default true)
     - CreatedAt (DateTime, UTC)
     - UpdatedAt (DateTime, UTC)
   - Indexes:
     - Term (B-tree)
     - Embedding (vector_cosine_ops)
     - Metadata (GIN)
     - Category (B-tree)
     - IsActive (B-tree)

5. **Configure pgvector in ApplicationDbContext**
   - File: `src/backend/PatientAccess.Data/ApplicationDbContext.cs`
   - Add DbSet properties:
     ```csharp
     public DbSet<ICD10Code> ICD10Codes { get; set; }
     public DbSet<CPTCode> CPTCodes { get; set; }
     public DbSet<ClinicalTerminology> ClinicalTerminology { get; set; }
     ```
   - Configure in `OnModelCreating`:
     ```csharp
     // ICD-10 configuration
     modelBuilder.Entity<ICD10Code>(entity =>
     {
         entity.HasIndex(e => e.Code).IsUnique();
         entity.HasIndex(e => e.IsActive);
         entity.Property(e => e.Embedding).HasColumnType("vector(1536)");
         entity.HasIndex(e => e.Embedding).HasMethod("hnsw")
             .HasOperators("vector_cosine_ops");
         entity.Property(e => e.Metadata).HasColumnType("jsonb");
         entity.HasIndex(e => e.Metadata).HasMethod("gin");
     });
     
     // CPT configuration (similar)
     // ClinicalTerminology configuration (similar)
     ```
   - Add pgvector dependency in .csproj: `Npgsql.EntityFrameworkCore.PostgreSQL.Vector`

6. **Create EF Core Migration**
   - Command: `dotnet ef migrations add AddVectorIndices --startup-project ../PatientAccess.Web`
   - Migration file: `src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddVectorIndices.cs`
   - Ensure migration includes:
     - CREATE TABLE statements for ICD10Codes, CPTCodes, ClinicalTerminology
     - Vector column definitions: `Embedding vector(1536)`
     - JSONB column definitions: `Metadata jsonb`
     - Vector indexes: `CREATE INDEX USING hnsw (Embedding vector_cosine_ops)`
     - GIN indexes: `CREATE INDEX USING gin (Metadata)`
     - B-tree indexes for Code, IsActive, Category

7. **Test Vector Operations**
   - Create test script: `scripts/migrations/002_test_vector_operations.sql`
   - Test cases:
     - Insert sample ICD-10 code with embedding
     - Query using cosine similarity: `ORDER BY Embedding <-> query_vector`
     - Verify index usage: `EXPLAIN ANALYZE SELECT ... ORDER BY Embedding <-> ...`
     - Test JSONB metadata search: `WHERE Metadata @> '{"status": "active"}'`
     - Test composite query (vector + keyword): combine cosine similarity with JSONB filters

8. **Implement Partition Strategy (Optional for Large Datasets)**
   - If dataset exceeds 1M entries, partition tables by Category or Version
   - PostgreSQL native partitioning:
     ```sql
     CREATE TABLE ICD10Codes (...) PARTITION BY LIST (Category);
     CREATE TABLE ICD10Codes_Endocrine PARTITION OF ICD10Codes FOR VALUES IN ('Endocrine...');
     ```
   - Document partitioning strategy in design.md

9. **Update DATABASE_SETUP.md Documentation**
   - Add pgvector installation steps
   - Document minimum PostgreSQL version (16.x)
   - Document minimum pgvector version (0.5+)
   - Include sample queries for vector search
   - Include index maintenance commands: `REINDEX INDEX CONCURRENTLY idx_embedding;`

10. **Verify Schema Performance**
    - Test cosine similarity query performance (<100ms for top-5 retrieval)
    - Test hybrid query (vector + keyword) performance (<200ms)
    - Verify index usage with EXPLAIN ANALYZE
    - Document query patterns in design.md

## Current Project State

```
src/backend/
├── PatientAccess.Data/
│   ├── Entities/
│   │   ├── Patient.cs (from EP-001)
│   │   ├── ClinicalDocument.cs (from EP-006-I)
│   │   └── ExtractedClinicalData.cs (from EP-006-II)
│   ├── ApplicationDbContext.cs
│   └── Migrations/
└── PatientAccess.Web/
    └── Program.cs

scripts/
└── migrations/ (does not exist yet)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | scripts/migrations/001_enable_pgvector.sql | pgvector extension setup |
| CREATE | scripts/migrations/002_test_vector_operations.sql | Vector operation tests |
| CREATE | src/backend/PatientAccess.Data/Entities/ICD10Code.cs | ICD-10 knowledge base entity |
| CREATE | src/backend/PatientAccess.Data/Entities/CPTCode.cs | CPT knowledge base entity |
| CREATE | src/backend/PatientAccess.Data/Entities/ClinicalTerminology.cs | Clinical terminology entity |
| CREATE | src/backend/PatientAccess.Data/Migrations/YYYYMMDDHHMMSS_AddVectorIndices.cs | EF migration |
| MODIFY | src/backend/PatientAccess.Data/ApplicationDbContext.cs | Add DbSet properties, pgvector config |
| MODIFY | src/backend/PatientAccess.Data/PatientAccess.Data.csproj | Add Npgsql.EntityFrameworkCore.PostgreSQL.Vector |
| MODIFY | docs/DATABASE_SETUP.md | pgvector installation guide |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### pgvector Documentation
- **pgvector GitHub**: https://github.com/pgvector/pgvector
- **Installation**: https://github.com/pgvector/pgvector#installation
- **C# Integration**: https://github.com/pgvector/pgvector-dotnet
- **Index Types**: https://github.com/pgvector/pgvector#indexing (HNSW, IVFFlat)
- **Distance Functions**: https://github.com/pgvector/pgvector#distances (cosine, L2, inner product)

### Entity Framework Core + pgvector
- **Npgsql.EntityFrameworkCore.PostgreSQL.Vector**: https://www.npgsql.org/efcore/mapping/vector.html
- **Vector Type Mapping**: https://www.npgsql.org/doc/types/vector.html
- **EF Core Migrations**: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/

### PostgreSQL JSONB
- **JSONB Type**: https://www.postgresql.org/docs/16/datatype-json.html
- **GIN Indexes**: https://www.postgresql.org/docs/16/gin-intro.html
- **JSONB Operators**: https://www.postgresql.org/docs/16/functions-json.html

### Azure OpenAI Embeddings
- **text-embedding-3-small**: https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models#embeddings
- **Embedding Dimensions**: 1536-dimensional vectors

### Design Requirements
- **DR-010**: System MUST store vector embeddings for clinical data and medical codes using pgvector extension with 1536-dimensional vectors (design.md)
- **AIR-R04**: System MUST maintain separate vector indices for ICD-10 codes, CPT codes, and clinical terminology (design.md)
- **TR-003**: System MUST use PostgreSQL 16+ with pgvector extension 0.5+ as primary database (design.md)

## Build Commands
```powershell
# Enable pgvector extension (manual step)
psql -U postgres -d patientaccess -f scripts/migrations/001_enable_pgvector.sql

# Create migration
cd src/backend/PatientAccess.Data
dotnet ef migrations add AddVectorIndices --startup-project ../PatientAccess.Web

# Update database
dotnet ef database update --startup-project ../PatientAccess.Web

# Test vector operations
psql -U postgres -d patientaccess -f scripts/migrations/002_test_vector_operations.sql

# Build solution
cd ..
dotnet build
```

## Validation Strategy

### Unit Tests
- N/A (Database schema task - verify with integration tests)

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Data/VectorIndicesTests.cs`
- Test cases:
  1. **Test_ICD10Code_VectorEmbedding_StorageAndRetrieval**
     - Insert ICD-10 code with 1536-dimensional embedding
     - Query using cosine similarity
     - Assert: Top result matches expected code with similarity >0.9
  2. **Test_CPTCode_HybridSearch_VectorAndKeyword**
     - Insert multiple CPT codes with embeddings
     - Query using vector similarity + JSONB metadata filter
     - Assert: Results filtered by category and ranked by similarity
  3. **Test_ClinicalTerminology_CosineSimilarity_Performance**
     - Insert 1000 clinical terms
     - Execute cosine similarity query with ORDER BY
     - Assert: Query time <100ms for top-5 retrieval
  4. **Test_VectorIndex_HNSW_UsageVerification**
     - Execute EXPLAIN ANALYZE on vector query
     - Assert: Query plan includes "Index Scan using idx_embedding"
  5. **Test_ReIndexing_NoDowntime**
     - Insert ICD-10 codes
     - Trigger REINDEX CONCURRENTLY
     - Assert: Queries succeed during re-indexing

### Acceptance Criteria Validation
- **AC2**: ✅ Three separate tables (ICD10Codes, CPTCodes, ClinicalTerminology) with vector(1536) embeddings
- **AC4**: ✅ Cosine similarity query retrieves relevant ICD-10/CPT codes with similarity scores
- **Edge Case**: ✅ Re-indexing strategy documented with REINDEX CONCURRENTLY

## Success Criteria Checklist
- [MANDATORY] pgvector extension enabled and verified (SELECT * FROM pg_extension WHERE extname = 'vector')
- [MANDATORY] ICD10Code entity created with vector(1536) embedding column
- [MANDATORY] CPTCode entity created with vector(1536) embedding column
- [MANDATORY] ClinicalTerminology entity created with vector(1536) embedding column
- [MANDATORY] Vector indexes created using HNSW with vector_cosine_ops
- [MANDATORY] GIN indexes created on Metadata JSONB columns
- [MANDATORY] ApplicationDbContext configured for pgvector (HasColumnType("vector(1536)"))
- [MANDATORY] EF Core migration generated and applied successfully
- [MANDATORY] Cosine similarity query returns results (ORDER BY Embedding <-> query_vector)
- [MANDATORY] Integration test: Vector search returns top-5 results in <100ms
- [MANDATORY] Integration test: EXPLAIN ANALYZE confirms index usage
- [MANDATORY] DATABASE_SETUP.md updated with pgvector installation steps
- [RECOMMENDED] Partition strategy documented for datasets >1M entries
- [RECOMMENDED] Re-indexing procedure documented (REINDEX CONCURRENTLY)

## Estimated Effort
**3 hours** (Database schema + pgvector configuration + integration tests)
