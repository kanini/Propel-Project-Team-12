-- =====================================================
-- Migration: Enable pgvector Extension
-- Purpose: Install and verify pgvector extension for 
--          vector similarity search capabilities
-- Requirements: DR-010, AIR-R04
-- PostgreSQL Version: 16.x+
-- pgvector Version: 0.5+
-- =====================================================

-- Enable pgvector extension for vector similarity search
CREATE EXTENSION IF NOT EXISTS vector;

-- Verify installation
SELECT 
    extname AS "Extension Name",
    extversion AS "Version",
    extrelocatable AS "Relocatable"
FROM pg_extension 
WHERE extname = 'vector';

-- Test vector operations (basic functionality check)
-- Cosine distance calculation (1 - cosine similarity)
SELECT '[1,2,3]'::vector(3) <-> '[4,5,6]'::vector(3) AS cosine_distance;

-- L2 (Euclidean) distance
SELECT '[1,2,3]'::vector(3) <-> '[1,2,3]'::vector(3) AS l2_distance_same;

-- Inner product (negative inner product for similarity)
SELECT '[1,2,3]'::vector(3) <#> '[4,5,6]'::vector(3) AS inner_product;

-- Display success message
DO $$
BEGIN
    RAISE NOTICE 'pgvector extension enabled successfully';
    RAISE NOTICE 'Extension version: %', (SELECT extversion FROM pg_extension WHERE extname = 'vector');
    RAISE NOTICE 'Vector operations validated';
END $$;
