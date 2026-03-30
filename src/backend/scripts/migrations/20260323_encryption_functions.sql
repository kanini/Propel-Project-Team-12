-- Migration: Encryption helper functions
-- Date: 2026-03-23
-- Epic: EP-010, User Story: US_056, Task: task_001
-- Requirement: FR-041, AC1 (AES-256 encryption at rest)
-- Author: AI Assistant via PropelIQ
-- Description: Database functions for encrypting/decrypting SSN and Insurance ID fields.
--              Uses pgcrypto pgp_sym_encrypt with AES-256 cipher.
--              Includes input validation and error handling.
-- Prerequisites: 20260323_enable_pgcrypto.sql must be executed first

-- ========================================
-- Function: encrypt_ssn
-- ========================================
-- Encrypts Social Security Number with AES-256 encryption
-- Parameters:
--   ssn_plaintext: SSN in format XXX-XX-XXXX
--   encryption_key: AES-256 encryption key (32+ bytes recommended)
-- Returns: BYTEA (encrypted SSN)
-- Raises: Exception if SSN format invalid
CREATE OR REPLACE FUNCTION encrypt_ssn(
    ssn_plaintext TEXT,
    encryption_key TEXT
)
RETURNS BYTEA AS $$
BEGIN
    -- Input validation: SSN format (XXX-XX-XXXX)
    IF ssn_plaintext IS NULL OR LENGTH(TRIM(ssn_plaintext)) = 0 THEN
        RAISE EXCEPTION 'SSN cannot be null or empty';
    END IF;
    
    IF ssn_plaintext !~ '^\d{3}-\d{2}-\d{4}$' THEN
        RAISE EXCEPTION 'Invalid SSN format. Expected: XXX-XX-XXXX (e.g., 123-45-6789)';
    END IF;
    
    -- Key validation
    IF encryption_key IS NULL OR LENGTH(encryption_key) < 8 THEN
        RAISE EXCEPTION 'Encryption key must be at least 8 characters';
    END IF;
    
    -- Encrypt using AES-256 (pgp_sym_encrypt defaults to AES-128, explicitly specify AES-256)
    RETURN pgp_sym_encrypt(
        ssn_plaintext,
        encryption_key,
        'cipher-algo=aes256'
    );
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'SSN encryption failed: %', SQLERRM;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

COMMENT ON FUNCTION encrypt_ssn(TEXT, TEXT) IS 
    'Encrypts Social Security Number with AES-256 encryption. ' ||
    'Validates SSN format (XXX-XX-XXXX) before encryption. ' ||
    'Returns BYTEA for storage in SSNEncrypted column. ' ||
    'Usage: INSERT INTO Users (..., SSNEncrypted) VALUES (..., encrypt_ssn(''123-45-6789'', $KEY));';

-- ========================================
-- Function: decrypt_ssn
-- ========================================
-- Decrypts Social Security Number
-- Parameters:
--   ssn_encrypted: Encrypted SSN (BYTEA from SSNEncrypted column)
--   encryption_key: AES-256 decryption key (same as encryption key)
-- Returns: TEXT (decrypted SSN in XXX-XX-XXXX format) or NULL if decryption fails
-- Note: Returns NULL on wrong key (graceful failure, logs warning)
CREATE OR REPLACE FUNCTION decrypt_ssn(
    ssn_encrypted BYTEA,
    encryption_key TEXT
)
RETURNS TEXT AS $$
BEGIN
    IF ssn_encrypted IS NULL THEN
        RETURN NULL;
    END IF;
    
    IF encryption_key IS NULL OR LENGTH(encryption_key) < 8 THEN
        RAISE EXCEPTION 'Decryption key must be at least 8 characters';
    END IF;
    
    RETURN pgp_sym_decrypt(
        ssn_encrypted,
        encryption_key
    );
EXCEPTION
    WHEN OTHERS THEN
        -- Log decryption failure (wrong key, corrupted data, or cipher mismatch)
        RAISE WARNING 'SSN decryption failed: %. This may indicate wrong decryption key or corrupted data.', SQLERRM;
        RETURN NULL; -- Graceful failure
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

COMMENT ON FUNCTION decrypt_ssn(BYTEA, TEXT) IS 
    'Decrypts Social Security Number from BYTEA encrypted with encrypt_ssn(). ' ||
    'Returns NULL and logs warning if decryption fails (wrong key or corrupted data). ' ||
    'Usage: SELECT decrypt_ssn(SSNEncrypted, $KEY) FROM Users WHERE UserId = $ID;';

-- ========================================
-- Function: encrypt_insurance_id
-- ========================================
-- Encrypts Insurance ID Number with AES-256 encryption
-- Parameters:
--   insurance_id_plaintext: Insurance ID (alphanumeric, any format)
--   encryption_key: AES-256 encryption key (32+ bytes recommended)
-- Returns: BYTEA (encrypted Insurance ID)
-- Raises: Exception if Insurance ID empty
CREATE OR REPLACE FUNCTION encrypt_insurance_id(
    insurance_id_plaintext TEXT,
    encryption_key TEXT
)
RETURNS BYTEA AS $$
BEGIN
    -- Input validation: Insurance ID is not empty
    IF insurance_id_plaintext IS NULL OR LENGTH(TRIM(insurance_id_plaintext)) = 0 THEN
        RAISE EXCEPTION 'Insurance ID cannot be null or empty';
    END IF;
    
    -- Key validation
    IF encryption_key IS NULL OR LENGTH(encryption_key) < 8 THEN
        RAISE EXCEPTION 'Encryption key must be at least 8 characters';
    END IF;
    
    -- Encrypt using AES-256
    RETURN pgp_sym_encrypt(
        insurance_id_plaintext,
        encryption_key,
        'cipher-algo=aes256'
    );
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Insurance ID encryption failed: %', SQLERRM;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

COMMENT ON FUNCTION encrypt_insurance_id(TEXT, TEXT) IS 
    'Encrypts Insurance ID Number with AES-256 encryption. ' ||
    'Accepts any alphanumeric format (no format validation). ' ||
    'Returns BYTEA for storage in InsuranceIDEncrypted column. ' ||
    'Usage: INSERT INTO Users (..., InsuranceIDEncrypted) VALUES (..., encrypt_insurance_id(''ABC123456'', $KEY));';

-- ========================================
-- Function: decrypt_insurance_id
-- ========================================
-- Decrypts Insurance ID Number
-- Parameters:
--   insurance_id_encrypted: Encrypted Insurance ID (BYTEA from InsuranceIDEncrypted column)
--   encryption_key: AES-256 decryption key (same as encryption key)
-- Returns: TEXT (decrypted Insurance ID) or NULL if decryption fails
-- Note: Returns NULL on wrong key (graceful failure, logs warning)
CREATE OR REPLACE FUNCTION decrypt_insurance_id(
    insurance_id_encrypted BYTEA,
    encryption_key TEXT
)
RETURNS TEXT AS $$
BEGIN
    IF insurance_id_encrypted IS NULL THEN
        RETURN NULL;
    END IF;
    
    IF encryption_key IS NULL OR LENGTH(encryption_key) < 8 THEN
        RAISE EXCEPTION 'Decryption key must be at least 8 characters';
    END IF;
    
    RETURN pgp_sym_decrypt(
        insurance_id_encrypted,
        encryption_key
    );
EXCEPTION
    WHEN OTHERS THEN
        -- Log decryption failure (wrong key, corrupted data, or cipher mismatch)
        RAISE WARNING 'Insurance ID decryption failed: %. This may indicate wrong decryption key or corrupted data.', SQLERRM;
        RETURN NULL; -- Graceful failure
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

COMMENT ON FUNCTION decrypt_insurance_id(BYTEA, TEXT) IS 
    'Decrypts Insurance ID Number from BYTEA encrypted with encrypt_insurance_id(). ' ||
    'Returns NULL and logs warning if decryption fails (wrong key or corrupted data). ' ||
    'Usage: SELECT decrypt_insurance_id(InsuranceIDEncrypted, $KEY) FROM Users WHERE UserId = $ID;';

-- ========================================
-- Test Functions
-- ========================================
-- Verify functions work correctly
DO $$
DECLARE
    test_ssn TEXT := '123-45-6789';
    test_insurance_id TEXT := 'ABC123456XYZ';
    test_key TEXT := 'test-encryption-key-32-bytes-long!!';
    encrypted_ssn BYTEA;
    decrypted_ssn TEXT;
    encrypted_insurance BYTEA;
    decrypted_insurance TEXT;
BEGIN
    -- Test SSN encryption/decryption
    encrypted_ssn := encrypt_ssn(test_ssn, test_key);
    decrypted_ssn := decrypt_ssn(encrypted_ssn, test_key);
    
    IF decrypted_ssn != test_ssn THEN
        RAISE EXCEPTION 'SSN encryption/decryption test failed: expected %, got %', test_ssn, decrypted_ssn;
    END IF;
    
    -- Test Insurance ID encryption/decryption
    encrypted_insurance := encrypt_insurance_id(test_insurance_id, test_key);
    decrypted_insurance := decrypt_insurance_id(encrypted_insurance, test_key);
    
    IF decrypted_insurance != test_insurance_id THEN
        RAISE EXCEPTION 'Insurance ID encryption/decryption test failed: expected %, got %', test_insurance_id, decrypted_insurance;
    END IF;
    
    RAISE NOTICE 'Encryption functions validated successfully';
END $$;

-- Display created functions
SELECT 
    proname AS "Function Name",
    pg_get_function_arguments(oid) AS "Arguments",
    pg_get_function_result(oid) AS "Return Type"
FROM pg_proc
WHERE proname IN ('encrypt_ssn', 'decrypt_ssn', 'encrypt_insurance_id', 'decrypt_insurance_id')
ORDER BY proname;

-- Rollback script (execute manually if needed):
-- DROP FUNCTION IF EXISTS encrypt_ssn(TEXT, TEXT);
-- DROP FUNCTION IF EXISTS decrypt_ssn(BYTEA, TEXT);
-- DROP FUNCTION IF EXISTS encrypt_insurance_id(TEXT, TEXT);
-- DROP FUNCTION IF EXISTS decrypt_insurance_id(BYTEA, TEXT);
