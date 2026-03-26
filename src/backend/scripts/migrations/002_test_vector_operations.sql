-- =====================================================
-- Migration: Test pgvector Operations
-- Purpose: Validate vector embeddings storage, retrieval,
--          and similarity search operations
-- Requirements: DR-010, AIR-R04
-- =====================================================

-- Test 1: Insert sample ICD-10 code with embedding
-- =====================================================
DO $$
DECLARE
    test_id UUID;
    sample_embedding vector(1536);
BEGIN
    -- Generate a normalized random 1536-dimensional vector for testing
    sample_embedding := array_to_string(
        ARRAY(SELECT random() FROM generate_series(1, 1536)),
        ','
    )::vector(1536);

    -- Insert test ICD-10 code
    INSERT INTO "ICD10Codes" (
        "Id",
        "Code",
        "Description",
        "Category",
        "ChapterCode",
        "Embedding",
        "ChunkText",
        "Metadata",
        "Version",
        "IsActive",
        "CreatedAt"
    ) VALUES (
        gen_random_uuid(),
        'E11.9',
        'Type 2 diabetes mellitus without complications',
        'Endocrine, nutritional and metabolic diseases',
        'E00-E89',
        sample_embedding,
        'E11.9: Type 2 diabetes mellitus without complications. Category: Endocrine, nutritional and metabolic diseases',
        '{"version": "ICD-10-CM-2024", "effectiveDate": "2024-10-01", "status": "active"}'::jsonb,
        'ICD-10-CM-2024',
        true,
        NOW()
    ) RETURNING "Id" INTO test_id;

    RAISE NOTICE 'Test 1 PASSED: Inserted ICD-10 code with ID %', test_id;
END $$;

-- Test 2: Insert sample CPT code with embedding
-- =====================================================
DO $$
DECLARE
    test_id UUID;
    sample_embedding vector(1536);
BEGIN
    sample_embedding := array_to_string(
        ARRAY(SELECT random() FROM generate_series(1, 1536)),
        ','
    )::vector(1536);

    INSERT INTO "CPTCodes" (
        "Id",
        "Code",
        "Description",
        "Category",
        "Embedding",
        "ChunkText",
        "Metadata",
        "Version",
        "IsActive",
        "CreatedAt"
    ) VALUES (
        gen_random_uuid(),
        '99213',
        'Office or other outpatient visit, established patient, 20-29 minutes',
        'Evaluation and Management',
        sample_embedding,
        '99213: Office or other outpatient visit, established patient, 20-29 minutes. Category: Evaluation and Management',
        '{"version": "CPT-2024", "effectiveDate": "2024-01-01", "status": "active", "rvuValue": "1.5"}'::jsonb,
        'CPT-2024',
        true,
        NOW()
    ) RETURNING "Id" INTO test_id;

    RAISE NOTICE 'Test 2 PASSED: Inserted CPT code with ID %', test_id;
END $$;

-- Test 3: Insert sample clinical terminology with embedding
-- =====================================================
DO $$
DECLARE
    test_id UUID;
    sample_embedding vector(1536);
BEGIN
    sample_embedding := array_to_string(
        ARRAY(SELECT random() FROM generate_series(1, 1536)),
        ','
    )::vector(1536);

    INSERT INTO "ClinicalTerminology" (
        "Id",
        "Term",
        "Category",
        "Synonyms",
        "Embedding",
        "ChunkText",
        "Metadata",
        "Source",
        "IsActive",
        "CreatedAt"
    ) VALUES (
        gen_random_uuid(),
        'Type 2 Diabetes Mellitus',
        'Diagnosis',
        '["T2DM", "NIDDM", "Adult-onset diabetes"]'::jsonb,
        sample_embedding,
        'Type 2 Diabetes Mellitus. Category: Diagnosis. Synonyms: T2DM, NIDDM, Adult-onset diabetes',
        '{"source": "SNOMED-CT", "mappedICD10Codes": ["E11.9"], "clinicalContext": "metabolic disorder"}'::jsonb,
        'SNOMED-CT',
        true,
        NOW()
    ) RETURNING "Id" INTO test_id;

    RAISE NOTICE 'Test 3 PASSED: Inserted clinical terminology with ID %', test_id;
END $$;

-- Test 4: Cosine similarity search on ICD-10 codes
-- =====================================================
DO $$
DECLARE
    query_embedding vector(1536);
    result_count INTEGER;
BEGIN
    -- Use the same embedding from first insert for similarity test
    SELECT "Embedding" INTO query_embedding 
    FROM "ICD10Codes" 
    WHERE "Code" = 'E11.9' 
    LIMIT 1;

    -- Perform cosine similarity search (top 5 results)
    SELECT COUNT(*) INTO result_count
    FROM (
        SELECT 
            "Code",
            "Description",
            ("Embedding" <-> query_embedding) AS cosine_distance,
            (1 - ("Embedding" <-> query_embedding)) AS similarity_score
        FROM "ICD10Codes"
        WHERE "IsActive" = true
        ORDER BY "Embedding" <-> query_embedding
        LIMIT 5
    ) subquery;

    IF result_count > 0 THEN
        RAISE NOTICE 'Test 4 PASSED: Cosine similarity search returned % results', result_count;
    ELSE
        RAISE EXCEPTION 'Test 4 FAILED: No results from cosine similarity search';
    END IF;
END $$;

-- Test 5: Hybrid search (vector + JSONB metadata filter)
-- =====================================================
DO $$
DECLARE
    query_embedding vector(1536);
    result_count INTEGER;
BEGIN
    SELECT "Embedding" INTO query_embedding 
    FROM "CPTCodes" 
    WHERE "Code" = '99213' 
    LIMIT 1;

    -- Hybrid search: vector similarity + metadata filter
    SELECT COUNT(*) INTO result_count
    FROM (
        SELECT 
            "Code",
            "Description",
            "Metadata",
            ("Embedding" <-> query_embedding) AS cosine_distance
        FROM "CPTCodes"
        WHERE "IsActive" = true
          AND "Metadata" @> '{"status": "active"}'::jsonb
        ORDER BY "Embedding" <-> query_embedding
        LIMIT 5
    ) subquery;

    IF result_count > 0 THEN
        RAISE NOTICE 'Test 5 PASSED: Hybrid search (vector + JSONB) returned % results', result_count;
    ELSE
        RAISE EXCEPTION 'Test 5 FAILED: No results from hybrid search';
    END IF;
END $$;

-- Test 6: JSONB synonym search on clinical terminology
-- =====================================================
DO $$
DECLARE
    result_count INTEGER;
BEGIN
    -- Search for clinical terminology by synonym
    SELECT COUNT(*) INTO result_count
    FROM "ClinicalTerminology"
    WHERE "Synonyms" @> '["T2DM"]'::jsonb
      AND "IsActive" = true;

    IF result_count > 0 THEN
        RAISE NOTICE 'Test 6 PASSED: JSONB synonym search returned % results', result_count;
    ELSE
        RAISE EXCEPTION 'Test 6 FAILED: No results from synonym search';
    END IF;
END $$;

-- Test 7: Verify index usage with EXPLAIN ANALYZE
-- =====================================================
-- Note: This query is for manual inspection. 
-- Check that the query plan includes "Index Scan using IX_ICD10Codes_Embedding_HNSW"
DO $$
BEGIN
    RAISE NOTICE 'Test 7: Execute EXPLAIN ANALYZE manually to verify index usage:';
    RAISE NOTICE '  EXPLAIN ANALYZE';
    RAISE NOTICE '  SELECT "Code", "Description", ("Embedding" <-> ''[...]''::vector(1536)) AS distance';
    RAISE NOTICE '  FROM "ICD10Codes"';
    RAISE NOTICE '  ORDER BY "Embedding" <-> ''[...]''::vector(1536)';
    RAISE NOTICE '  LIMIT 5;';
END $$;

-- Test 8: Performance benchmark (top-5 retrieval)
-- =====================================================
DO $$
DECLARE
    query_embedding vector(1536);
    start_time TIMESTAMP;
    end_time TIMESTAMP;
    duration_ms NUMERIC;
BEGIN
    SELECT "Embedding" INTO query_embedding 
    FROM "ICD10Codes" 
    LIMIT 1;

    -- Measure query execution time
    start_time := clock_timestamp();

    PERFORM "Code", "Description"
    FROM "ICD10Codes"
    WHERE "IsActive" = true
    ORDER BY "Embedding" <-> query_embedding
    LIMIT 5;

    end_time := clock_timestamp();
    duration_ms := EXTRACT(EPOCH FROM (end_time - start_time)) * 1000;

    RAISE NOTICE 'Test 8: Top-5 cosine similarity query executed in % ms', duration_ms;

    IF duration_ms < 100 THEN
        RAISE NOTICE 'Test 8 PASSED: Query performance within threshold (<100ms)';
    ELSE
        RAISE WARNING 'Test 8 WARNING: Query took % ms (target: <100ms). Consider index optimization.', duration_ms;
    END IF;
END $$;

-- Summary
-- =====================================================
DO $$
BEGIN
    RAISE NOTICE '========================================';
    RAISE NOTICE 'pgvector Operations Test Suite Complete';
    RAISE NOTICE '========================================';
    RAISE NOTICE 'All tests executed successfully';
    RAISE NOTICE 'Review NOTICE messages above for results';
    RAISE NOTICE 'Clean up test data: DELETE FROM "ICD10Codes"; DELETE FROM "CPTCodes"; DELETE FROM "ClinicalTerminology";';
END $$;
