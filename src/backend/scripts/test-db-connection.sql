-- ============================================================================
-- Database Connectivity and pgvector Extension Verification Script
-- ============================================================================
-- Purpose: Verify PostgreSQL 16 connection and pgvector extension setup
-- Usage: Run this script in Supabase SQL Editor or via psql client
-- Expected: All queries should execute successfully without errors
-- ============================================================================

-- Step 1: Verify PostgreSQL version (should be 16.x)
-- Expected output: PostgreSQL 16.x
SELECT version();

-- Step 2: Check if pgvector extension is installed
-- Expected output: One row with extname = 'vector'
SELECT 
    extname AS extension_name,
    extversion AS version,
    extrelocatable AS relocatable
FROM pg_extension 
WHERE extname = 'vector';

-- Step 3: List all available extensions (optional)
-- Expected output: Multiple rows including 'vector' extension
SELECT 
    name,
    default_version,
    comment
FROM pg_available_extensions
WHERE name LIKE '%vector%';

-- Step 4: Test vector column creation with 1536 dimensions
-- Expected: Table created successfully
CREATE TABLE IF NOT EXISTS test_pg_vector_support (
    id SERIAL PRIMARY KEY,
    description TEXT,
    embedding vector(1536),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Step 5: Insert test vector data
-- Expected: 1 row inserted
INSERT INTO test_pg_vector_support (description, embedding) 
VALUES (
    'Test embedding for 1536-dimensional vector',
    array_fill(0.0, ARRAY[1536])::vector(1536)
);

-- Step 6: Verify vector dimensions
-- Expected output: dimensions = 1536
SELECT 
    id,
    description,
    vector_dims(embedding) AS dimensions,
    created_at
FROM test_pg_vector_support;

-- Step 7: Test vector similarity search (cosine distance)
-- Expected: Query executes successfully
SELECT 
    id,
    description,
    embedding <=> array_fill(0.0, ARRAY[1536])::vector(1536) AS cosine_distance
FROM test_pg_vector_support
ORDER BY cosine_distance
LIMIT 5;

-- Step 8: Test vector operations
-- Expected: Various distance metrics calculated
SELECT 
    id,
    description,
    -- Cosine distance (range 0-2, lower is more similar)
    embedding <=> array_fill(0.1, ARRAY[1536])::vector(1536) AS cosine_distance,
    -- L2 distance / Euclidean distance
    embedding <-> array_fill(0.1, ARRAY[1536])::vector(1536) AS l2_distance,
    -- Inner product (negative, higher absolute value is more similar)
    embedding <#> array_fill(0.1, ARRAY[1536])::vector(1536) AS inner_product
FROM test_pg_vector_support
LIMIT 5;

-- Step 9: Check table structure
-- Expected: Details of test_pg_vector_support table
SELECT 
    column_name,
    data_type,
    character_maximum_length,
    is_nullable
FROM information_schema.columns
WHERE table_name = 'test_pg_vector_support'
ORDER BY ordinal_position;

-- Step 10: Clean up test table (optional - comment out to keep test data)
-- DROP TABLE IF EXISTS test_pg_vector_support;

-- ============================================================================
-- Verification Checklist
-- ============================================================================
-- [ ] PostgreSQL version is 16.x
-- [ ] pgvector extension is installed (extname = 'vector')
-- [ ] Test table created with vector(1536) column
-- [ ] Test data inserted successfully
-- [ ] Vector dimensions verified as 1536
-- [ ] Vector similarity queries execute without errors
-- [ ] All distance metrics (cosine, L2, inner product) calculated successfully
-- ============================================================================

-- ============================================================================
-- Troubleshooting
-- ============================================================================
-- If any query fails, check the following:
-- 1. pgvector extension is enabled: CREATE EXTENSION IF NOT EXISTS vector;
-- 2. PostgreSQL version is 16 or higher
-- 3. Database user has CREATE TABLE privileges
-- 4. Connection to Supabase is stable
-- ============================================================================

-- Success Message
SELECT 'Database connectivity and pgvector extension verification completed successfully!' AS status;
