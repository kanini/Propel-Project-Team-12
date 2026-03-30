-- Migration: Enable pgcrypto extension for column-level encryption
-- Date: 2026-03-23
-- Epic: EP-010, User Story: US_056, Task: task_001
-- Requirement: FR-041, NFR-003 (AES-256 encryption at rest)
-- Author: AI Assistant via PropelIQ
-- Description: Enables PostgreSQL pgcrypto extension for AES-256 symmetric encryption
--              of sensitive PHI fields (SSN, Insurance ID). Provides pgp_sym_encrypt
--              and pgp_sym_decrypt functions for column-level encryption.

-- Enable pgcrypto extension (provides pgp_sym_encrypt/pgp_sym_decrypt functions)
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

-- Test encryption functions (validate installation)
DO $$
DECLARE
    test_plaintext TEXT := 'test-123-45-6789';
    test_key TEXT := 'test-encryption-key-do-not-use-in-production';
    test_encrypted BYTEA;
    test_decrypted TEXT;
BEGIN
    -- Test encryption
    test_encrypted := pgp_sym_encrypt(test_plaintext, test_key, 'cipher-algo=aes256');
    
    IF test_encrypted IS NULL THEN
        RAISE EXCEPTION 'pgp_sym_encrypt failed: returned NULL';
    END IF;
    
    -- Test decryption
    test_decrypted := pgp_sym_decrypt(test_encrypted, test_key);
    
    IF test_decrypted != test_plaintext THEN
        RAISE EXCEPTION 'pgcrypto validation failed: decrypted value does not match original';
    END IF;
    
    RAISE NOTICE 'pgcrypto AES-256 encryption/decryption validated successfully';
END $$;

-- Display loaded extension details
SELECT 
    extname AS "Extension Name",
    extversion AS "Version",
    extnamespace::regnamespace AS "Schema"
FROM pg_extension
WHERE extname = 'pgcrypto';

-- Rollback script (execute manually if needed):
-- DROP EXTENSION IF EXISTS pgcrypto CASCADE;
-- WARNING: CASCADE will drop all dependent functions and encrypted columns
