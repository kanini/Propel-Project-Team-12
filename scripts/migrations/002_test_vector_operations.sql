-- Vector Operations Test Script (DR-010, AIR-R04)
-- This script tests the pgvector knowledge base implementation
-- Run AFTER applying the AddVectorIndices migration

-- ================================================
-- Test 1: Insert Sample ICD-10 Code with Embedding
-- ================================================
INSERT INTO "ICD10Codes" (
    "Code", "Description", "Category", "ChapterCode", 
    "Embedding", "ChunkText", "Metadata", "Version"
)
VALUES (
    'E11.9',
    'Type 2 diabetes mellitus without complications',
    'Endocrine, nutritional and metabolic diseases',
    'E00-E89',
    -- Sample 1536-dimensional zero vector
    array_fill(0, ARRAY[1536])::vector(1536),
    'Type 2 diabetes mellitus without complications (E11.9) - Adult-onset diabetes requiring management',
    '{"version": "ICD-10-CM-2024", "effectiveDate": "2024-10-01", "status": "active", "subcategories": ["diabetes", "endocrine"], "relatedCodes": ["E11.65", "E11.21"]}'::jsonb,
    'ICD-10-CM-2024'
);

-- Verify insertion
SELECT "Code", "Description", vector_dims("Embedding") as embedding_dimensions
FROM "ICD10Codes"
WHERE "Code" = 'E11.9';

-- ================================================
-- Test 2: Insert Sample CPT Code with Embedding
-- ================================================
INSERT INTO "CPTCodes" (
    "Code", "Description", "Category", "Embedding", 
    "ChunkText", "Metadata", "Version"
)
VALUES (
    '99213',
    'Office or other outpatient visit for the evaluation and management of an established patient, which requires a medically appropriate history and/or examination and low level of medical decision making',
    'Evaluation and Management',
    array_fill(0, ARRAY[1536])::vector(1536),
    'CPT 99213 - Established patient E/M visit (low complexity)',
    '{"version": "CPT-2024", "effectiveDate": "2024-01-01", "status": "active", "rvuValue": 1.42, "billingGuidelines": "Document 2 of 3 key components"}'::jsonb,
    'CPT-2024'
);

-- Verify insertion
SELECT "Code", "Description", vector_dims("Embedding") as embedding_dimensions
FROM "CPTCodes"
WHERE "Code" = '99213';

-- ================================================
-- Test 3: Insert Sample Clinical Terminology with Synonyms
-- ================================================
INSERT INTO "ClinicalTerminology" (
    "Term", "Category", "Synonyms", "Embedding", 
    "ChunkText", "Metadata", "Source"
)
VALUES (
    'Type 2 Diabetes Mellitus',
    'Diagnosis',
    '["T2DM", "NIDDM", "Adult-onset diabetes", "Non-insulin-dependent diabetes mellitus"]'::jsonb,
    array_fill(0, ARRAY[1536])::vector(1536),
    'Type 2 Diabetes Mellitus (T2DM) - chronic metabolic disorder characterized by insulin resistance',
    '{"source": "SNOMED-CT", "mappedICD10Codes": ["E11.9", "E11.65"], "mappedCPTCodes": ["99213"], "clinicalContext": "Common chronic condition requiring long-term management"}'::jsonb,
    'SNOMED-CT'
);

-- Verify insertion
SELECT "Term", "Category", "Synonyms", vector_dims("Embedding") as embedding_dimensions
FROM "ClinicalTerminology"
WHERE "Term" = 'Type 2 Diabetes Mellitus';

-- ================================================
-- Test 4: Cosine Similarity Query
-- ================================================
-- Create a query vector (all zeros for testing)
DO $$
DECLARE
    query_vector vector(1536);
    result_code text;
    similarity_score float;
BEGIN
    -- Generate query vector
    query_vector := array_fill(0, ARRAY[1536])::vector(1536);
    
    -- Test cosine similarity search (returns 0 for identical vectors)
    SELECT "Code", ("Embedding" <-> query_vector) INTO result_code, similarity_score
    FROM "ICD10Codes"
    WHERE "Embedding" IS NOT NULL
    ORDER BY "Embedding" <-> query_vector
    LIMIT 1;
    
    RAISE NOTICE 'Top ICD-10 Result: % with similarity score: %', result_code, similarity_score;
END $$;

-- ================================================
-- Test 5: Hybrid Query (Vector + JSONB Keyword Search)
-- ================================================
-- Find diabetes-related ICD-10 codes using JSONB metadata filter
SELECT 
    "Code",
    "Description",
    "Category",
    "Metadata"->'relatedCodes' as related_codes,
    -- Cosine distance to a zero vector (for testing)
    ("Embedding" <-> array_fill(0, ARRAY[1536])::vector(1536)) as similarity_distance
FROM "ICD10Codes"
WHERE 
    "IsActive" = true
    AND "Metadata" @> '{"subcategories": ["diabetes"]}'::jsonb
ORDER BY "Embedding" <-> array_fill(0, ARRAY[1536])::vector(1536)
LIMIT 5;

-- ================================================
-- Test 6: Index Usage Verification (Must use EXPLAIN ANALYZE)
-- ================================================
EXPLAIN ANALYZE
SELECT "Code", "Description", ("Embedding" <-> array_fill(0, ARRAY[1536])::vector(1536)) as distance
FROM "ICD10Codes"
WHERE "IsActive" = true
ORDER BY "Embedding" <-> array_fill(0, ARRAY[1536])::vector(1536)
LIMIT 10;

-- Expected output should show: "Index Scan using IX_ICD10Codes_Embedding_Cosine"

-- ================================================
-- Test 7: JSONB Metadata Search Performance
-- ================================================
EXPLAIN ANALYZE
SELECT "Code", "Description", "Metadata"
FROM "ICD10Codes"
WHERE "Metadata" @> '{"status": "active"}'::jsonb;

-- Expected output should show: "Bitmap Index Scan on IX_ICD10Codes_Metadata_Gin"

-- ================================================
-- Test 8: Verify Vector Dimensions
-- ================================================
SELECT 
    table_name,
    COUNT(*) as total_records,
    AVG(vector_dims("Embedding")) as avg_dimensions,
    MIN(vector_dims("Embedding")) as min_dimensions,
    MAX(vector_dims("Embedding")) as max_dimensions
FROM (
    SELECT 'ICD10Codes' as table_name, "Embedding" FROM "ICD10Codes"
    UNION ALL
    SELECT 'CPTCodes' as table_name, "Embedding" FROM "CPTCodes"
    UNION ALL
    SELECT 'ClinicalTerminology' as table_name, "Embedding" FROM "ClinicalTerminology"  
) combined
WHERE "Embedding" IS NOT NULL
GROUP BY table_name;

-- All dimensions should be 1536

-- ================================================
-- Test 9: Verify Index Metadata
-- ================================================
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename IN ('ICD10Codes', 'CPTCodes', 'ClinicalTerminology')
ORDER BY tablename, indexname;

-- ================================================
-- Test 10: Performance Baseline
-- ================================================
-- Measure query performance for top-5 retrieval (should be <100ms)
\timing on

SELECT "Code", "Description", ("Embedding" <-> array_fill(0, ARRAY[1536])::vector(1536)) as distance
FROM "ICD10Codes"
WHERE "IsActive" = true
ORDER BY "Embedding" <-> array_fill(0, ARRAY[1536])::vector(1536)
LIMIT 5;

\timing off

-- ================================================
-- Test Summary
-- ================================================
SELECT 
    'ICD10Codes' as table_name,
    COUNT(*) as record_count,
    COUNT("Embedding") as embeddings_count,
    COUNT(*) FILTER (WHERE "IsActive" = true) as active_count
FROM "ICD10Codes"
UNION ALL
SELECT 
    'CPTCodes',
    COUNT(*),
    COUNT("Embedding"),
    COUNT(*) FILTER (WHERE "IsActive" = true)
FROM "CPTCodes"
UNION ALL
SELECT 
    'ClinicalTerminology',
    COUNT(*),
    COUNT("Embedding"),
    COUNT(*) FILTER (WHERE "IsActive" = true)
FROM "ClinicalTerminology";
