# Database Setup Guide

## Overview

This guide walks you through provisioning a PostgreSQL 16 database instance on Supabase with pgvector extension for the Patient Access Platform.

## Prerequisites

- A valid email address for Supabase account creation
- Internet connection
- Access to the backend project configuration

## Step 1: Create Supabase Account and Project

### 1.1 Sign Up for Supabase

1. Navigate to [https://supabase.com](https://supabase.com)
2. Click "Start your project" or "Sign up"
3. Create an account using GitHub, Google, or email
4. Verify your email address if required

### 1.2 Create New Project

1. After logging in, click "New Project" from the dashboard
2. Fill in the following details:
   - **Project Name**: `patient-access-platform` (or your preferred name)
   - **Database Password**: Create a strong password (save this securely!)
   - **Region**: Select the region closest to your users
   - **Pricing Plan**: Select "Free" tier
3. Click "Create new project"
4. Wait 2-3 minutes for the database to provision

**Important**: Save your database password immediately - you won't be able to retrieve it later!

## Step 2: Enable pgvector Extension

### 2.1 Access SQL Editor

1. In your Supabase project dashboard, navigate to the **SQL Editor** from the left sidebar
2. Click "New query" to create a new SQL script

### 2.2 Enable pgvector

Execute the following SQL command:

```sql
-- Enable pgvector extension for vector similarity search
CREATE EXTENSION IF NOT EXISTS vector;
```

Click "Run" to execute the query.

### 2.3 Verify pgvector Installation

Run the following verification query:

```sql
-- Verify pgvector extension is enabled
SELECT * FROM pg_extension WHERE extname = 'vector';
```

You should see a row with `extname = 'vector'` in the results.

### 2.4 Test Vector Column Support

Create a test table to confirm 1536-dimensional vector support:

```sql
-- Test vector column creation with 1536 dimensions
CREATE TABLE IF NOT EXISTS test_vectors (
    id SERIAL PRIMARY KEY,
    embedding vector(1536),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Insert a test vector
INSERT INTO test_vectors (embedding) 
VALUES (array_fill(0, ARRAY[1536])::vector);

-- Verify the test
SELECT id, vector_dims(embedding) as dimensions FROM test_vectors;

-- Clean up test table (optional)
DROP TABLE test_vectors;
```

Expected output: `dimensions = 1536`

## Step 3: Retrieve Connection String

### 3.1 Get Connection Details

1. Navigate to **Settings** → **Database** in the left sidebar
2. Scroll down to the **Connection string** section
3. Select the **URI** tab (not the JDBC or .NET tabs)
4. Find the connection string in this format:

```
postgresql://postgres:[YOUR-PASSWORD]@db.[PROJECT-REF].supabase.co:5432/postgres
```

5. Copy the connection string
6. Replace `[YOUR-PASSWORD]` with the database password you created in Step 1.2

### 3.2 Connection String Format

The final connection string should look like:

```
postgresql://postgres:your_actual_password@db.abcdefghijklmnop.supabase.co:5432/postgres
```

**Security Note**: Never commit this connection string directly to version control!

## Step 4: Configure Local Environment

### 4.1 Create .env File

1. Navigate to the project root directory: `Propel-Project-Team-12/`
2. Create a new file named `.env` (note the leading dot)
3. Add the following content:

```env
# Supabase PostgreSQL Connection String
# Replace with your actual connection string from Step 3
DB_CONNECTION_STRING=postgresql://postgres:YOUR_PASSWORD@db.YOUR_PROJECT_REF.supabase.co:5432/postgres

# Example:
# DB_CONNECTION_STRING=postgresql://postgres:mySecureP@ssw0rd@db.abcdefghijklmnop.supabase.co:5432/postgres
```

4. Save the file
5. Verify `.env` is listed in `.gitignore` (it should already be included)

### 4.2 Using .NET User Secrets (Alternative - Recommended for Development)

For enhanced security during development, you can use .NET User Secrets instead of `.env`:

1. Open a terminal in the backend project directory:
   ```powershell
   cd src\backend\PatientAccess.Web
   ```

2. Initialize user secrets:
   ```powershell
   dotnet user-secrets init
   ```

3. Set the connection string:
   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://postgres:YOUR_PASSWORD@db.YOUR_PROJECT_REF.supabase.co:5432/postgres"
   ```

4. Verify the secret was set:
   ```powershell
   dotnet user-secrets list
   ```

**Note**: User Secrets are stored outside the project directory and are specific to your local machine.

## Step 5: Verify Database Connectivity

### 5.1 Test Connection Using psql (Optional)

If you have PostgreSQL client tools installed:

```powershell
# Test basic connection
psql "postgresql://postgres:YOUR_PASSWORD@db.YOUR_PROJECT_REF.supabase.co:5432/postgres" -c "SELECT version();"

# Test pgvector extension
psql "postgresql://postgres:YOUR_PASSWORD@db.YOUR_PROJECT_REF.supabase.co:5432/postgres" -c "SELECT * FROM pg_extension WHERE extname = 'vector';"
```

### 5.2 Test Connection from .NET Application

1. Navigate to the backend project:
   ```powershell
   cd src\backend\PatientAccess.Web
   ```

2. Run the application:
   ```powershell
   dotnet run
   ```

3. Check the console output for any database connection errors
4. If the application starts successfully without database errors, the connection is working

## Troubleshooting

### Connection Refused

**Problem**: Cannot connect to the database.

**Solutions**:
- Verify your internet connection
- Check that the connection string is correct
- Ensure the Supabase project is fully provisioned (wait a few minutes if just created)
- Verify your IP is not blocked by firewall rules

### Authentication Failed

**Problem**: Password authentication failed.

**Solutions**:
- Double-check the password in your connection string
- Ensure there are no extra spaces or special characters that need URL encoding
- Try resetting the database password in Supabase Settings → Database → "Reset database password"

### pgvector Extension Not Found

**Problem**: Vector column creation fails.

**Solutions**:
- Re-run the `CREATE EXTENSION IF NOT EXISTS vector;` command
- Verify you're using PostgreSQL 16 (check with `SELECT version();`)
- Contact Supabase support if the extension is not available

### Environment Variable Not Loaded

**Problem**: Application cannot find the connection string.

**Solutions**:
- Verify `.env` file is in the project root directory
- Check that the environment variable name matches exactly: `DB_CONNECTION_STRING`
- Restart your IDE or terminal to reload environment variables
- Consider using .NET User Secrets for development (see Step 4.2)

## Connection String Security Best Practices

1. **Never commit** `.env` files to version control
2. **Use User Secrets** for local development in .NET projects
3. **Use environment variables** for production deployments
4. **Rotate passwords** regularly (every 90 days recommended)
5. **Use connection pooling** to manage database connections efficiently
6. **Enable SSL** for all database connections (Supabase enables this by default)

## Next Steps

After completing this setup:

1. ✅ Supabase project created with PostgreSQL 16
2. ✅ pgvector extension enabled and verified
3. ✅ Connection string securely stored in `.env` or User Secrets
4. ✅ Database connectivity verified

You can now proceed to:
- Set up Entity Framework Core DbContext
- Create database migrations
- Define data models and entities
- Implement repository pattern for data access

## pgvector Knowledge Base Setup

### Overview

The Patient Access Platform uses pgvector for medical coding knowledge base with semantic search capabilities (AIR-R04, DR-010). Three separate vector indices store ICD-10 codes, CPT codes, and clinical terminology with 1536-dimensional embeddings from Azure OpenAI text-embedding-3-small.

### Prerequisites

- PostgreSQL 16+ with pgvector extension 0.5+ enabled (completed in Step 2)
- Entity Framework Core migrations applied
- Pgvector.EntityFrameworkCore NuGet package (included in PatientAccess.Data.csproj)

### Knowledge Base Schema

Three tables support semantic medical code retrieval:

| Table | Purpose | Dimensions | Index Type |
|-------|---------|------------|------------|
| `ICD10Codes` | ICD-10-CM diagnosis codes | vector(1536) | HNSW + GIN + B-tree |
| `CPTCodes` | CPT procedure/service codes | vector(1536) | HNSW + GIN + B-tree |
| `ClinicalTerminology` | Clinical terms mapped to codes | vector(1536) | HNSW + GIN + B-tree |

**Index Types:**
- **HNSW** (Hierarchical Navigable Small World): Fast cosine similarity search on embeddings
- **GIN** (Generalized Inverted Index): Efficient JSONB metadata and synonym keyword search
- **B-tree**: Exact code lookups, IsActive filtering, category queries

### EF Core Migration Setup

The database schema is created via Entity Framework Core migrations with custom SQL for pgvector indexes.

#### 1. Apply EF Core Migration

Navigate to the backend project and run:

```powershell
cd src\backend\PatientAccess.Data
dotnet ef database update --startup-project ../PatientAccess.Web
```

This creates the three knowledge base tables with:
- Vector(1536) columns for embeddings
- JSONB columns for metadata
- Standard indexes (B-tree, unique constraints)
- HNSW indexes for vector similarity (created via raw SQL in migration)
- GIN indexes for JSONB fields (created via raw SQL in migration)

#### 2. Run Manual Migration Scripts (Optional)

For standalone pgvector setup without EF migrations:

```powershell
# Enable pgvector extension
psql "$DB_CONNECTION_STRING" -f src/backend/scripts/migrations/001_enable_pgvector.sql

# Create vector tables (if not using EF migrations)
# Tables are created automatically by EF migrations in normal workflow
```

#### 3. Validate Vector Operations

Run the test script to verify functionality:

```powershell
# Test vector storage, retrieval, and similarity search
psql "$DB_CONNECTION_STRING" -f src/backend/scripts/migrations/002_test_vector_operations.sql
```

Expected output:
```
NOTICE: Test 1 PASSED: Inserted ICD-10 code with ID [uuid]
NOTICE: Test 2 PASSED: Inserted CPT code with ID [uuid]
NOTICE: Test 3 PASSED: Inserted clinical terminology with ID [uuid]
NOTICE: Test 4 PASSED: Cosine similarity search returned N results
NOTICE: Test 5 PASSED: Hybrid search (vector + JSONB) returned N results
NOTICE: Test 6 PASSED: JSONB synonym search returned N results
NOTICE: Test 8: Top-5 cosine similarity query executed in X ms
NOTICE: Test 8 PASSED: Query performance within threshold (<100ms)
```

### Sample Vector Search Queries

#### Cosine Similarity Search (ICD-10 Codes)

Find top-5 most similar ICD-10 codes to a query embedding:

```sql
-- Replace [...] with actual 1536-dimensional embedding array
SELECT 
    "Code",
    "Description",
    "Category",
    ("Embedding" <-> '[...]'::vector(1536)) AS cosine_distance,
    (1 - ("Embedding" <-> '[...]'::vector(1536))) AS similarity_score
FROM "ICD10Codes"
WHERE "IsActive" = true
ORDER BY "Embedding" <-> '[...]'::vector(1536)
LIMIT 5;
```

**Distance Operators:**
- `<->` : Cosine distance (1 - cosine similarity) — **Use this for semantic search**
- `<#>` : Negative inner product
- `<=>` : L2 (Euclidean) distance

#### Hybrid Search (Vector + Keyword Metadata Filter)

Combine semantic similarity with metadata filtering:

```sql
SELECT 
    "Code",
    "Description",
    "Metadata"->>'version' AS version,
    ("Embedding" <-> '[...]'::vector(1536)) AS distance
FROM "CPTCodes"
WHERE "IsActive" = true
  AND "Category" = 'Evaluation and Management'
  AND "Metadata" @> '{"status": "active"}'::jsonb
ORDER BY "Embedding" <-> '[...]'::vector(1536)
LIMIT 10;
```

**JSONB Operators:**
- `@>` : Contains (metadata contains JSON object)
- `?` : Key exists
- `->` : Get JSON object field as JSON
- `->>` : Get JSON object field as text

#### Clinical Term Synonym Search

Search clinical terminology by synonyms:

```sql
SELECT 
    "Term",
    "Category",
    "Synonyms",
    "Metadata"->>'mappedICD10Codes' AS mapped_icd10
FROM "ClinicalTerminology"
WHERE "Synonyms" @> '["T2DM"]'::jsonb
  AND "IsActive" = true
ORDER BY "CreatedAt" DESC
LIMIT 20;
```

### Performance Considerations

#### Query Performance Targets

| Operation | Target | Actual (Sample) | Notes |
|-----------|--------|-----------------|-------|
| Top-5 vector search | <100ms | ~50-80ms | With HNSW index on <100K rows |
| Hybrid query (vector + filter) | <200ms | ~100-150ms | Depends on filter selectivity |
| Exact code lookup | <10ms | ~2-5ms | B-tree index on Code column |

#### Index Maintenance

For datasets >100K entries, periodically rebuild HNSW indexes:

```sql
-- Rebuild HNSW index concurrently (no downtime)
REINDEX INDEX CONCURRENTLY "IX_ICD10Codes_Embedding_HNSW";
REINDEX INDEX CONCURRENTLY "IX_CPTCodes_Embedding_HNSW";
REINDEX INDEX CONCURRENTLY "IX_ClinicalTerminology_Embedding_HNSW";
```

#### Monitoring Index Usage

Verify HNSW index is used for queries:

```sql
EXPLAIN ANALYZE
SELECT "Code", "Description"
FROM "ICD10Codes"
WHERE "IsActive" = true
ORDER BY "Embedding" <-> '[...]'::vector(1536)
LIMIT 5;
```

Expected query plan includes:
```
-> Index Scan using IX_ICD10Codes_Embedding_HNSW on ICD10Codes
```

If index is not used, check:
1. Statistics are up to date: `ANALYZE "ICD10Codes";`
2. Index exists: `\d "ICD10Codes"` in psql
3. Query uses correct distance operator: `<->` for cosine distance

#### HNSW Index Parameters (Advanced)

For large datasets (>1M rows), tune HNSW parameters:

```sql
-- Example: Create HNSW index with custom parameters
CREATE INDEX "IX_ICD10Codes_Embedding_HNSW" 
ON "ICD10Codes" 
USING hnsw ("Embedding" vector_cosine_ops)
WITH (m = 16, ef_construction = 64);
```

**Parameters:**
- `m`: Max connections per layer (default 16, range 2-100). Higher = better recall, more memory.
- `ef_construction`: Size of dynamic candidate list (default 64, range 1-1000). Higher = better recall, slower build.

**Recommendation:** Use defaults for datasets <500K rows. Tune only if query performance degrades.

### Data Versioning and Re-indexing

#### Version Control Strategy

Each knowledge base table tracks code system versions:

- **ICD10Codes.Version**: `"ICD-10-CM-2024"` (updated annually Oct 1)
- **CPTCodes.Version**: `"CPT-2024"` (updated annually Jan 1)
- **ClinicalTerminology.Source**: `"SNOMED-CT"`, `"LOINC"`, `"Internal"`

#### Re-indexing Without Downtime

When updating knowledge base (new code releases):

1. **Insert new codes** with updated version:
   ```sql
   INSERT INTO "ICD10Codes" (..., "Version", "IsActive")
   VALUES (..., 'ICD-10-CM-2025', true);
   ```

2. **Deprecate old codes** (soft delete):
   ```sql
   UPDATE "ICD10Codes"
   SET "IsActive" = false, "UpdatedAt" = NOW()
   WHERE "Version" = 'ICD-10-CM-2024';
   ```

3. **Concurrent index rebuild** (optional):
   ```sql
   REINDEX INDEX CONCURRENTLY "IX_ICD10Codes_Embedding_HNSW";
   ```

Queries automatically filter `WHERE "IsActive" = true` to exclude deprecated codes.

### Troubleshooting

#### Vector Query Returns No Results

**Problem**: Cosine similarity query returns empty result set.

**Solutions**:
- Verify embeddings are not NULL: `SELECT COUNT(*) FROM "ICD10Codes" WHERE "Embedding" IS NOT NULL;`
- Check vector dimensions match: `SELECT vector_dims("Embedding") FROM "ICD10Codes" LIMIT 1;` (expected: 1536)
- Ensure query vector dimensions match: `'[...]'::vector(1536)` must have exactly 1536 elements

#### HNSW Index Not Used

**Problem**: EXPLAIN ANALYZE shows Seq Scan instead of Index Scan.

**Solutions**:
- Run `ANALYZE "ICD10Codes";` to update statistics
- Verify index exists: `\d "ICD10Codes"` or `SELECT indexname FROM pg_indexes WHERE tablename = 'ICD10Codes';`
- Check query uses correct operator: `<->` for cosine distance, not `<=>` (L2 distance)
- For small datasets (<1000 rows), PostgreSQL may choose Seq Scan (faster for small tables)

#### Slow Query Performance (>200ms)

**Problem**: Vector similarity queries exceed 100ms target.

**Solutions**:
- Rebuild index concurrently: `REINDEX INDEX CONCURRENTLY "IX_ICD10Codes_Embedding_HNSW";`
- Update table statistics: `ANALYZE "ICD10Codes";`
- Check dataset size: If >500K rows, consider partitioning by Category or Version
- Tune HNSW parameters (see HNSW Index Parameters section above)
- Verify adequate database resources (CPU, memory, I/O)

### Backup and Restore

#### Backup Vector Data

```powershell
# Backup all knowledge base tables
pg_dump "$DB_CONNECTION_STRING" \
  --table='"ICD10Codes"' \
  --table='"CPTCodes"' \
  --table='"ClinicalTerminology"' \
  --file=knowledge_base_backup.sql
```

#### Restore Vector Data

```powershell
# Restore from backup
psql "$DB_CONNECTION_STRING" -f knowledge_base_backup.sql
```

**Note**: pgvector extension must be enabled before restoring vector data.



## Additional Resources

- [Supabase Documentation](https://supabase.com/docs)
- [pgvector Extension Documentation](https://github.com/pgvector/pgvector)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/16/)
- [.NET Configuration Documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Npgsql Connection Strings](https://www.npgsql.org/doc/connection-string-parameters.html)

## Support

If you encounter issues not covered by this guide:
- Review Supabase documentation: https://supabase.com/docs/guides/database
- Check pgvector GitHub issues: https://github.com/pgvector/pgvector/issues
- Consult the project team or technical lead
