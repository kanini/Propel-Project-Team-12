-- Rollback Migration: Remove UploadedBy field from ClinicalDocument table
-- Task: task_003_db_document_schema_migration (US_042)
-- Purpose: Rollback migration V002
-- Date: 2026-03-23

BEGIN;

-- Drop foreign key constraint
ALTER TABLE "ClinicalDocuments"
DROP CONSTRAINT "FK_ClinicalDocuments_UploadedBy";

-- Drop index
DROP INDEX "IX_ClinicalDocuments_UploadedBy";

-- Drop column
ALTER TABLE "ClinicalDocuments"
DROP COLUMN "UploadedBy";

COMMIT;
