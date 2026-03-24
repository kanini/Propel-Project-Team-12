-- Migration: Add UploadedBy field to ClinicalDocument table
-- Task: task_003_db_document_schema_migration (US_042)
-- Purpose: Track who uploaded the document (patient or staff)
-- Date: 2026-03-23

BEGIN;

-- Add UploadedBy column (nullable FK to Users table)
ALTER TABLE "ClinicalDocuments"
ADD COLUMN "UploadedBy" uuid NULL;

-- Create index on UploadedBy for query performance
CREATE INDEX "IX_ClinicalDocuments_UploadedBy"
ON "ClinicalDocuments" ("UploadedBy");

-- Add foreign key constraint with SET NULL on delete
ALTER TABLE "ClinicalDocuments"
ADD CONSTRAINT "FK_ClinicalDocuments_UploadedBy"
FOREIGN KEY ("UploadedBy")
REFERENCES "Users" ("UserId")
ON DELETE SET NULL;

-- Add comment for documentation
COMMENT ON COLUMN "ClinicalDocuments"."UploadedBy" IS 
'User who uploaded the document (may differ from PatientId if staff uploads on behalf of patient)';

COMMIT;
