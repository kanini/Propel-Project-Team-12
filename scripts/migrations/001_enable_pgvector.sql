-- Enable pgvector extension for vector similarity search (DR-010, AIR-R04)
-- This script MUST be run before EF Core migrations
-- Requires PostgreSQL 16+ with pgvector 0.5+ installed

-- Enable vector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Verify extension installation
SELECT 
    extname AS extension_name,
    extversion AS version
FROM pg_extension 
WHERE extname = 'vector';

-- Test basic vector operations to ensure proper installation
-- Test 1: Vector creation and storage
SELECT '[1,2,3]'::vector(3) AS test_vector;

-- Test 2: Cosine distance calculation (used in AIR-R04 retrieval)
SELECT 
    '[1,2,3]'::vector(3) <-> '[4,5,6]'::vector(3) AS cosine_distance,
    '[1,2,3]'::vector(3) <=> '[4,5,6]'::vector(3) AS l2_distance,
    '[1,2,3]'::vector(3) <#> '[4,5,6]'::vector(3) AS inner_product;

-- Test 3: Verify 1536-dimensional vector support (text-embedding-3-small)
DO $$
DECLARE
    test_vector vector(1536);
    dimension_count int;
BEGIN
    -- Generate a test 1536-dimensional vector (all zeros)
    test_vector := array_fill(0, ARRAY[1536])::vector(1536);
    
    -- Verify dimension count
    SELECT vector_dims(test_vector) INTO dimension_count;
    
    IF dimension_count = 1536 THEN
        RAISE NOTICE '✓ 1536-dimensional vector support verified';
    ELSE
        RAISE EXCEPTION '✗ Vector dimension mismatch: expected 1536, got %', dimension_count;
    END IF;
END $$;

-- Output success message
SELECT 
    'pgvector extension enabled successfully' AS status,
    version() AS postgres_version,
    (SELECT extversion FROM pg_extension WHERE extname = 'vector') AS pgvector_version;
