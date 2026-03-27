-- Query 1: Find existing ExtractedDataIds with medical codes
SELECT 
    mc."ExtractedDataId", 
    COUNT(*) as "CodeCount",
    STRING_AGG(DISTINCT mc."CodeSystem"::text, ', ') as "CodeSystems"
FROM "MedicalCodes" mc
GROUP BY mc."ExtractedDataId"
ORDER BY COUNT(*) DESC
LIMIT 10;

-- Query 2: Check if the specific GUID exists
SELECT 
    ecd."ExtractedDataId",
    ecd."DataKey",
    ecd."DataValue",
    ecd."SourceTextExcerpt",
    COUNT(mc."MedicalCodeId") as "CodeCount"
FROM "ExtractedClinicalData" ecd
LEFT JOIN "MedicalCodes" mc ON mc."ExtractedDataId" = ecd."ExtractedDataId"
WHERE ecd."ExtractedDataId" = '369e49ef-c8ae-4786-82f3-f60d16cca28c'
GROUP BY ecd."ExtractedDataId", ecd."DataKey", ecd."DataValue", ecd."SourceTextExcerpt";

-- Query 3: View sample MedicalCodes records
SELECT 
    "MedicalCodeId",
    "ExtractedDataId",
    "CodeValue",
    "CodeDescription",
    "CodeSystem",
    "ConfidenceScore",
    "VerificationStatus",
    "Rank",
    "IsTopSuggestion"
FROM "MedicalCodes"
ORDER BY "CreatedAt" DESC
LIMIT 5;
