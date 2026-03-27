-- Create test data for Medical Code Verification (US_052)
-- This will create an ExtractedClinicalData record and associated MedicalCodes

DO $$
DECLARE
    test_extracted_id UUID := '369e49ef-c8ae-4786-82f3-f60d16cca28c';
    test_patient_id UUID;
    test_document_id UUID;
BEGIN
    -- Get first patient (or create a test patient)
    SELECT "UserId" INTO test_patient_id 
    FROM "Users" 
    WHERE "Role" = 0 -- Patient role
    LIMIT 1;

    -- If no patient exists, you'll need to create one first
    IF test_patient_id IS NULL THEN
        RAISE EXCEPTION 'No patient found. Please create a patient first.';
    END IF;

    -- Get or create a test document
    INSERT INTO "ClinicalDocuments" (
        "DocumentId", 
        "PatientId", 
        "FileName", 
        "FileSize", 
        "MimeType", 
        "StoragePath", 
        "UploadedBy", 
        "UploadedAt", 
        "ProcessingStatus", 
        "CreatedAt"
    )
    VALUES (
        gen_random_uuid(),
        test_patient_id,
        'test_clinical_notes.pdf',
        1024,
        'application/pdf',
        'test/path/document.pdf',
        test_patient_id,
        NOW(),
        3, -- Completed
        NOW()
    )
    ON CONFLICT DO NOTHING
    RETURNING "DocumentId" INTO test_document_id;

    -- If document already exists, get it
    IF test_document_id IS NULL THEN
        SELECT "DocumentId" INTO test_document_id 
        FROM "ClinicalDocuments" 
        WHERE "PatientId" = test_patient_id
        LIMIT 1;
    END IF;

    -- Create ExtractedClinicalData
    INSERT INTO "ExtractedClinicalData" (
        "ExtractedDataId",
        "DocumentId",
        "PatientId",
        "DataType",
        "DataKey",
        "DataValue",
        "ConfidenceScore",
        "VerificationStatus",
        "SourcePageNumber",
        "SourceTextExcerpt",
        "ExtractedAt",
        "CreatedAt"
    )
    VALUES (
        test_extracted_id,
        test_document_id,
        test_patient_id,
        0, -- Diagnosis
        'Primary_Diagnosis',
        'Type 2 Diabetes Mellitus',
        92.5,
        0, -- Pending
        1,
        'Patient presents with type 2 diabetes mellitus without complications. HbA1c 7.2%. No retinopathy or neuropathy noted.',
        NOW(),
        NOW()
    )
    ON CONFLICT ("ExtractedDataId") DO NOTHING;

    -- Create test ICD-10 codes
    INSERT INTO "MedicalCodes" (
        "MedicalCodeId",
        "ExtractedDataId",
        "CodeSystem",
        "CodeValue",
        "CodeDescription",
        "ConfidenceScore",
        "Rationale",
        "Rank",
        "IsTopSuggestion",
        "VerificationStatus",
        "CreatedAt"
    )
    VALUES 
    -- Top suggestion
    (
        gen_random_uuid(),
        test_extracted_id,
        0, -- ICD10
        'E11.9',
        'Type 2 diabetes mellitus without complications',
        96.8,
        'Clinical text explicitly mentions "type 2 diabetes mellitus without complications". High confidence match.',
        1,
        true,
        1, -- AISuggested
        NOW()
    ),
    -- Alternative 1
    (
        gen_random_uuid(),
        test_extracted_id,
        0, -- ICD10
        'E11.65',
        'Type 2 diabetes mellitus with hyperglycemia',
        87.3,
        'Patient has elevated HbA1c (7.2%) suggesting hyperglycemia component.',
        2,
        false,
        1, -- AISuggested
        NOW()
    ),
    -- Alternative 2
    (
        gen_random_uuid(),
        test_extracted_id,
        0, -- ICD10
        'E11.69',
        'Type 2 diabetes mellitus with other specified complication',
        75.2,
        'Consider if patient has any unmentioned complications requiring monitoring.',
        3,
        false,
        1, -- AISuggested
        NOW()
    ),
    -- CPT code for office visit
    (
        gen_random_uuid(),
        test_extracted_id,
        1, -- CPT
        '99213',
        'Office or other outpatient visit, established patient, low to moderate complexity',
        89.5,
        'Standard office visit code for diabetes follow-up appointment.',
        1,
        true,
        1, -- AISuggested
        NOW()
    )
    ON CONFLICT DO NOTHING;

    RAISE NOTICE 'Test data created successfully for ExtractedDataId: %', test_extracted_id;
END $$;

-- Verify the data was created
SELECT 
    mc."MedicalCodeId",
    mc."CodeSystem",
    mc."CodeValue",
    mc."CodeDescription",
    mc."ConfidenceScore",
    mc."Rank",
    mc."IsTopSuggestion"
FROM "MedicalCodes" mc
WHERE mc."ExtractedDataId" = '369e49ef-c8ae-4786-82f3-f60d16cca28c'
ORDER BY mc."Rank";
