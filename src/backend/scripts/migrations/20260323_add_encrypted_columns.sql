-- Migration: Add encrypted columns for SSN and Insurance ID
-- Date: 2026-03-23
-- Epic: EP-010, User Story: US_056, Task: task_001
-- Requirement: FR-041, AC1 (column-level encryption for sensitive fields)
-- Author: AI Assistant via PropelIQ
-- Description: Adds encrypted SSN and InsuranceID columns to Users table with AES-256 encryption.
--              Uses BYTEA type to store pgp_sym_encrypt output. Includes encryption key version
--              column for key rotation support.
-- Prerequisites: 20260323_enable_pgcrypto.sql must be executed first

-- Add encrypted SSN column (stores pgp_sym_encrypt output as BYTEA)
ALTER TABLE "Users"
ADD COLUMN IF NOT EXISTS "SSNEncrypted" BYTEA;

-- Add encrypted Insurance ID column
ALTER TABLE "Users"
ADD COLUMN IF NOT EXISTS "InsuranceIDEncrypted" BYTEA;

-- Add encryption metadata columns for key rotation support
ALTER TABLE "Users"
ADD COLUMN IF NOT EXISTS "EncryptionKeyVersion" VARCHAR(20) DEFAULT 'v1';

-- Create index on key version for efficient key rotation queries
CREATE INDEX IF NOT EXISTS "IX_Users_EncryptionKeyVersion"
ON "Users" ("EncryptionKeyVersion");

-- Add column comments for documentation
COMMENT ON COLUMN "Users"."SSNEncrypted" IS 
    'AES-256 encrypted Social Security Number (pgcrypto pgp_sym_encrypt). ' ||
    'NEVER store plaintext SSN. Use encrypt_ssn/decrypt_ssn functions for encryption/decryption. ' ||
    'Encrypted with column encryption key (ENCRYPTION__COLUMNENCRYPTIONKEY environment variable).';

COMMENT ON COLUMN "Users"."InsuranceIDEncrypted" IS 
    'AES-256 encrypted Insurance ID Number (pgcrypto pgp_sym_encrypt). ' ||
    'Use encrypt_insurance_id/decrypt_insurance_id functions for encryption/decryption. ' ||
    'Encrypted with column encryption key (ENCRYPTION__COLUMNENCRYPTIONKEY environment variable).';

COMMENT ON COLUMN "Users"."EncryptionKeyVersion" IS 
    'Encryption key version identifier for key rotation support. ' ||
    'Default: v1. Updated during key rotation migrations. ' ||
    'Used to identify which encryption key was used to encrypt the SSN/InsuranceID.';

-- Verify columns were created successfully
DO $$
DECLARE
    col_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO col_count
    FROM information_schema.columns
    WHERE table_name = 'Users'
    AND column_name IN ('SSNEncrypted', 'InsuranceIDEncrypted', 'EncryptionKeyVersion');
    
    IF col_count < 3 THEN
        RAISE EXCEPTION 'Failed to create encrypted columns: expected 3, found %', col_count;
    END IF;
    
    RAISE NOTICE 'Encrypted columns added successfully to Users table';
END $$;

-- Display created columns and their types
SELECT 
    column_name AS "Column Name",
    data_type AS "Data Type",
    column_default AS "Default Value",
    is_nullable AS "Nullable"
FROM information_schema.columns
WHERE table_name = 'Users'
AND column_name IN ('SSNEncrypted', 'InsuranceIDEncrypted', 'EncryptionKeyVersion')
ORDER BY ordinal_position;

-- Display created index
SELECT 
    indexname AS "Index Name",
    indexdef AS "Index Definition"
FROM pg_indexes
WHERE tablename = 'Users'
AND indexname = 'IX_Users_EncryptionKeyVersion';

-- Rollback script (execute manually if needed):
-- ALTER TABLE "Users" DROP COLUMN IF EXISTS "SSNEncrypted";
-- ALTER TABLE "Users" DROP COLUMN IF EXISTS "InsuranceIDEncrypted";
-- ALTER TABLE "Users" DROP COLUMN IF EXISTS "EncryptionKeyVersion";
-- DROP INDEX IF EXISTS "IX_Users_EncryptionKeyVersion";
