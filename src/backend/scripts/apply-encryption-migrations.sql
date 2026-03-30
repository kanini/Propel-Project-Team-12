-- ===========================================================================
-- CONSOLIDATED ENCRYPTION MIGRATIONS
-- ===========================================================================
-- Run this script in Supabase SQL Editor to add encryption support
-- Epic: EP-010, User Story: US_056
-- Requirement: FR-041 (AES-256 encryption at rest)
-- ===========================================================================

-- ===========================================================================
-- STEP 1: Enable pgcrypto Extension
-- ===========================================================================

CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Verify extension is loaded
DO $$
DECLARE
    ext_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO ext_count
    FROM pg_extension
    WHERE extname = 'pgcrypto';
    
    IF ext_count = 0 THEN
        RAISE EXCEPTION 'pgcrypto extension failed to load';
    END IF;
    
    RAISE NOTICE 'pgcrypto extension enabled successfully';
END $$;


-- ===========================================================================
-- STEP 2: Add Encrypted Columns to Users Table
-- ===========================================================================

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
COMMENT ON COLUMN "Users"."SSNEncrypted" IS 'AES-256 encrypted Social Security Number (pgcrypto pgp_sym_encrypt). NEVER store plaintext SSN. Use encrypt_ssn/decrypt_ssn functions for encryption/decryption. Encrypted with column encryption key (ENCRYPTION__COLUMNENCRYPTIONKEY environment variable).';

COMMENT ON COLUMN "Users"."InsuranceIDEncrypted" IS 'AES-256 encrypted Insurance ID Number (pgcrypto pgp_sym_encrypt). Use encrypt_insurance_id/decrypt_insurance_id functions for encryption/decryption. Encrypted with column encryption key (ENCRYPTION__COLUMNENCRYPTIONKEY environment variable).';

COMMENT ON COLUMN "Users"."EncryptionKeyVersion" IS 'Encryption key version identifier for key rotation support. Default: v1. Updated during key rotation migrations. Used to identify which encryption key was used to encrypt the SSN/InsuranceID.';

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


-- ===========================================================================
-- STEP 3: Create Encryption Helper Functions
-- ===========================================================================

-- Function: encrypt_ssn
-- Encrypts SSN using AES-256 with application-provided key
CREATE OR REPLACE FUNCTION encrypt_ssn(plaintext_ssn TEXT, encryption_key TEXT)
RETURNS BYTEA
LANGUAGE plpgsql
IMMUTABLE
AS $$
BEGIN
    IF plaintext_ssn IS NULL THEN
        RETURN NULL;
    END IF;
    
    -- pgp_sym_encrypt with AES-256 cipher (FR-041)
    RETURN pgp_sym_encrypt(plaintext_ssn, encryption_key, 'cipher-algo=aes256'); -- gitleaks:allow
END;
$$;

-- Function: decrypt_ssn
-- Decrypts SSN using AES-256 with application-provided key
CREATE OR REPLACE FUNCTION decrypt_ssn(encrypted_ssn BYTEA, encryption_key TEXT)
RETURNS TEXT
LANGUAGE plpgsql
IMMUTABLE
AS $$
BEGIN
    IF encrypted_ssn IS NULL THEN
        RETURN NULL;
    END IF;
    
    -- pgp_sym_decrypt with provided key
    RETURN pgp_sym_decrypt(encrypted_ssn, encryption_key);
END;
$$;

-- Function: encrypt_insurance_id
-- Encrypts Insurance ID using AES-256 with application-provided key
CREATE OR REPLACE FUNCTION encrypt_insurance_id(plaintext_insurance_id TEXT, encryption_key TEXT)
RETURNS BYTEA
LANGUAGE plpgsql
IMMUTABLE
AS $$
BEGIN
    IF plaintext_insurance_id IS NULL THEN
        RETURN NULL;
    END IF;
    
    -- pgp_sym_encrypt with AES-256 cipher (FR-041)
    RETURN pgp_sym_encrypt(plaintext_insurance_id, encryption_key, 'cipher-algo=aes256'); -- gitleaks:allow
END;
$$;

-- Function: decrypt_insurance_id
-- Decrypts Insurance ID using AES-256 with application-provided key
CREATE OR REPLACE FUNCTION decrypt_insurance_id(encrypted_insurance_id BYTEA, encryption_key TEXT)
RETURNS TEXT
LANGUAGE plpgsql
IMMUTABLE
AS $$
BEGIN
    IF encrypted_insurance_id IS NULL THEN
        RETURN NULL;
    END IF;
    
    -- pgp_sym_decrypt with provided key
    RETURN pgp_sym_decrypt(encrypted_insurance_id, encryption_key);
END;
$$;

-- Function: hash_ssn_for_search
-- Creates searchable hash of SSN for lookup queries without exposing plaintext
CREATE OR REPLACE FUNCTION hash_ssn_for_search(plaintext_ssn TEXT)
RETURNS TEXT
LANGUAGE plpgsql
IMMUTABLE
AS $$
BEGIN
    IF plaintext_ssn IS NULL THEN
        RETURN NULL;
    END IF;
    
    -- SHA-256 hash for deterministic search (not reversible)
    RETURN encode(digest(plaintext_ssn, 'sha256'), 'hex');
END;
$$;

-- Function: hash_insurance_id_for_search
-- Creates searchable hash of Insurance ID for lookup queries
CREATE OR REPLACE FUNCTION hash_insurance_id_for_search(plaintext_insurance_id TEXT)
RETURNS TEXT
LANGUAGE plpgsql
IMMUTABLE
AS $$
BEGIN
    IF plaintext_insurance_id IS NULL THEN
        RETURN NULL;
    END IF;
    
    -- SHA-256 hash for deterministic search (not reversible)
    RETURN encode(digest(plaintext_insurance_id, 'sha256'), 'hex');
END;
$$;

-- Display created functions
SELECT 
    routine_name AS "Function Name",
    routine_type AS "Type",
    data_type AS "Return Type"
FROM information_schema.routines
WHERE routine_schema = 'public'
AND routine_name IN (
    'encrypt_ssn', 
    'decrypt_ssn', 
    'encrypt_insurance_id', 
    'decrypt_insurance_id',
    'hash_ssn_for_search',
    'hash_insurance_id_for_search'
)
ORDER BY routine_name;


-- ===========================================================================
-- VERIFICATION
-- ===========================================================================

-- Display final column structure
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

-- Final success message
DO $$
BEGIN
    RAISE NOTICE '✅ All encryption migrations applied successfully!';
END $$;
