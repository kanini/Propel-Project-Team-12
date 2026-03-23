# Task - task_001_db_encryption_configuration

## Requirement Reference
- User Story: US_056
- Story Location: .propel/context/tasks/EP-010/us_056/us_056.md
- Acceptance Criteria:
    - **AC1**: Given PHI is stored in PostgreSQL (FR-041), When data is written to disk, Then AES-256 encryption is applied via Supabase's transparent data encryption and column-level encryption for sensitive fields (SSN, insurance IDs).

## Design References (Frontend Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **UI Impact** | No |
| **Figma URL** | N/A |
| **Wireframe Status** | N/A |
| **Wireframe Type** | N/A |
| **Wireframe Path/URL** | N/A |
| **Screen Spec** | N/A |
| **UXR Requirements** | N/A |
| **Design Tokens** | N/A |

> If UI Impact = No, all design references should be N/A

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Frontend | N/A | N/A |
| Backend | N/A | N/A |
| Database | PostgreSQL | 16.x |
| Database | Supabase | Latest (free tier) |
| Caching | N/A | N/A |
| Vector Store | N/A | N/A |
| AI Gateway | N/A | N/A |
| Mobile | N/A | N/A |

**Note**: All code, and libraries, MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

> **AI Impact Legend:**
> - **Yes**: Task involves LLM integration, RAG pipeline, prompt engineering, or AI infrastructure
> - **No**: Task is deterministic (FE/BE/DB only)
>
> If AI Impact = No, all AI references should be N/A

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

> If Mobile Impact = No, all Mobile references should be N/A

## Task Overview

Configure PostgreSQL database encryption at rest using Supabase's transparent data encryption (AES-256) and implement column-level encryption for highly sensitive PHI fields (SSN, insurance ID numbers). This task ensures compliance with FR-041 (AES-256 encryption at rest) and NFR-003 (encrypt all PHI at rest). Since Supabase provides transparent data encryption by default for all free-tier PostgreSQL instances, the primary focus is verifying encryption is enabled and implementing additional column-level encryption using pgcrypto extension for sensitive PII fields. This provides defense-in-depth: database-level encryption protects against disk theft, while column-level encryption protects against database administrator access without application-level decryption keys.

**Key Capabilities:**
- Verify Supabase transparent data encryption (AES-256) is enabled (AC1)
- Enable pgcrypto extension for column-level encryption
- Create encrypted columns for Patient.SSN and Patient.InsuranceID
- Implement encryption key management using environment variables
- Create database functions for encrypting/decrypting sensitive fields
- Update Patient table schema with encrypted columns
- Migration script with rollback support
- Document encryption key rotation procedure

## Dependent Tasks
- None (foundational security task)

## Impacted Components
- **NEW**: `src/backend/scripts/migrations/20260323_enable_pgcrypto.sql` - Enable pgcrypto extension
- **NEW**: `src/backend/scripts/migrations/20260323_add_encrypted_columns.sql` - Add encrypted SSN/InsuranceID columns
- **NEW**: `src/backend/scripts/migrations/20260323_encryption_functions.sql` - Encryption helper functions
- **NEW**: `docs/ENCRYPTION_KEY_ROTATION.md` - Key rotation procedure documentation
- **MODIFY**: `src/backend/PatientAccess.Data/Entities/Patient.cs` - Add encrypted properties
- **MODIFY**: `src/backend/PatientAccess.Web/appsettings.json` - Add encryption key configuration placeholder
- **MODIFY**: `docs/DATABASE_SETUP.md` - Document encryption configuration

## Implementation Plan

1. **Verify Supabase Transparent Encryption**
   - Access Supabase project dashboard
   - Navigate to Database → Settings → General
   - Confirm "Encryption at Rest" is enabled (AES-256)
   - Document verification in `docs/DATABASE_SETUP.md`:
     ```markdown
     ## Encryption Configuration
     
     ### Transparent Data Encryption (TDE)
     Supabase provides AES-256 encryption at rest for all PostgreSQL data by default.
     
     **Verification Steps:**
     1. Login to Supabase dashboard: https://supabase.com/dashboard
     2. Navigate to your project → Settings → Database
     3. Confirm "Encryption at Rest: AES-256" is displayed
     
     **Coverage:**
     - All table data files (.dat)
     - Write-Ahead Log (WAL) files
     - Database backups
     - Temporary files
     ```

2. **Enable pgcrypto Extension**
   - File: `src/backend/scripts/migrations/20260323_enable_pgcrypto.sql`
   - Migration script:
     ```sql
     -- Migration: Enable pgcrypto extension for column-level encryption
     -- Date: 2026-03-23
     -- Epic: EP-010, User Story: US_056, Task: task_001
     -- Requirement: FR-041, NFR-003 (AES-256 encryption at rest)
     
     -- Enable pgcrypto extension (provides pgp_sym_encrypt/pgp_sym_decrypt functions)
     CREATE EXTENSION IF NOT EXISTS pgcrypto;
     
     -- Verify extension is loaded
     SELECT * FROM pg_extension WHERE extname = 'pgcrypto';
     
     -- Test encryption functions (validate installation)
     DO $$
     BEGIN
         IF pgp_sym_decrypt(
             pgp_sym_encrypt('test', 'test-key'),
             'test-key'
         ) != 'test' THEN
             RAISE EXCEPTION 'pgcrypto extension validation failed';
         END IF;
     END $$;
     
     -- Rollback script:
     -- DROP EXTENSION IF EXISTS pgcrypto CASCADE;
     ```

3. **Create Encryption Key Configuration**
   - File: `src/backend/PatientAccess.Web/appsettings.json`
   - Add encryption key placeholder (NEVER commit actual keys):
     ```json
     {
       "Encryption": {
         "ColumnEncryptionKey": "PLACEHOLDER_SET_IN_ENVIRONMENT_VARIABLE",
         "KeyRotationDate": "2026-03-23"
       }
     }
     ```
   - File: `src/backend/PatientAccess.Web/appsettings.Development.json`
   - Development-only encryption key:
     ```json
     {
       "Encryption": {
         "ColumnEncryptionKey": "dev-only-key-DO-NOT-USE-IN-PRODUCTION-32chars!",
         "KeyRotationDate": "2026-03-23"
       }
     }
     ```
   - Environment variable setup (for Railway/Render deployment):
     ```bash
     # Production environment variable (set in Railway/Render dashboard)
     ENCRYPTION__COLUMNENCRYPTIONKEY=<generate-secure-32-byte-key>
     
     # Generate production key using OpenSSL:
     openssl rand -base64 32
     ```

4. **Add Encrypted Columns to Patient Table**
   - File: `src/backend/scripts/migrations/20260323_add_encrypted_columns.sql`
   - Migration script:
     ```sql
     -- Migration: Add encrypted columns for SSN and Insurance ID
     -- Date: 2026-03-23
     -- Epic: EP-010, User Story: US_056, Task: task_001
     -- Requirement: FR-041, AC1 (column-level encryption for sensitive fields)
     
     -- Add encrypted SSN column (stores pgp_sym_encrypt output as BYTEA)
     ALTER TABLE "Patients"
     ADD COLUMN IF NOT EXISTS "SSNEncrypted" BYTEA;
     
     -- Add encrypted Insurance ID column
     ALTER TABLE "Patients"
     ADD COLUMN IF NOT EXISTS "InsuranceIDEncrypted" BYTEA;
     
     -- Add encryption metadata columns
     ALTER TABLE "Patients"
     ADD COLUMN IF NOT EXISTS "EncryptionKeyVersion" VARCHAR(20) DEFAULT 'v1';
     
     -- Create index on key version for future key rotation queries
     CREATE INDEX IF NOT EXISTS "IX_Patients_EncryptionKeyVersion"
     ON "Patients" ("EncryptionKeyVersion");
     
     -- Document columns
     COMMENT ON COLUMN "Patients"."SSNEncrypted" IS 'AES-256 encrypted Social Security Number (pgcrypto pgp_sym_encrypt)';
     COMMENT ON COLUMN "Patients"."InsuranceIDEncrypted" IS 'AES-256 encrypted Insurance ID Number (pgcrypto pgp_sym_encrypt)';
     COMMENT ON COLUMN "Patients"."EncryptionKeyVersion" IS 'Encryption key version identifier for key rotation support';
     
     -- Rollback script:
     -- ALTER TABLE "Patients" DROP COLUMN IF EXISTS "SSNEncrypted";
     -- ALTER TABLE "Patients" DROP COLUMN IF EXISTS "InsuranceIDEncrypted";
     -- ALTER TABLE "Patients" DROP COLUMN IF EXISTS "EncryptionKeyVersion";
     -- DROP INDEX IF EXISTS "IX_Patients_EncryptionKeyVersion";
     ```

5. **Create Encryption Helper Functions**
   - File: `src/backend/scripts/migrations/20260323_encryption_functions.sql`
   - Database functions for encrypt/decrypt operations:
     ```sql
     -- Migration: Encryption helper functions
     -- Date: 2026-03-23
     -- Epic: EP-010, User Story: US_056, Task: task_001
     
     -- Function: Encrypt SSN
     CREATE OR REPLACE FUNCTION encrypt_ssn(
         ssn_plaintext TEXT,
         encryption_key TEXT
     )
     RETURNS BYTEA AS $$
     BEGIN
         -- Validate SSN format (XXX-XX-XXXX)
         IF ssn_plaintext !~ '^\d{3}-\d{2}-\d{4}$' THEN
             RAISE EXCEPTION 'Invalid SSN format. Expected: XXX-XX-XXXX';
         END IF;
         
         -- Encrypt using AES-256 (pgp_sym_encrypt uses AES-128 by default, specify cipher)
         RETURN pgp_sym_encrypt(
             ssn_plaintext,
             encryption_key,
             'cipher-algo=aes256'
         );
     END;
     $$ LANGUAGE plpgsql;
     
     -- Function: Decrypt SSN
     CREATE OR REPLACE FUNCTION decrypt_ssn(
         ssn_encrypted BYTEA,
         encryption_key TEXT
     )
     RETURNS TEXT AS $$
     BEGIN
         IF ssn_encrypted IS NULL THEN
             RETURN NULL;
         END IF;
         
         RETURN pgp_sym_decrypt(
             ssn_encrypted,
             encryption_key
         );
     EXCEPTION
         WHEN OTHERS THEN
             -- Log decryption failure (wrong key or corrupted data)
             RAISE WARNING 'SSN decryption failed: %', SQLERRM;
             RETURN NULL;
     END;
     $$ LANGUAGE plpgsql;
     
     -- Function: Encrypt Insurance ID
     CREATE OR REPLACE FUNCTION encrypt_insurance_id(
         insurance_id_plaintext TEXT,
         encryption_key TEXT
     )
     RETURNS BYTEA AS $$
     BEGIN
         -- Validate Insurance ID is not empty
         IF insurance_id_plaintext IS NULL OR LENGTH(TRIM(insurance_id_plaintext)) = 0 THEN
             RAISE EXCEPTION 'Insurance ID cannot be empty';
         END IF;
         
         RETURN pgp_sym_encrypt(
             insurance_id_plaintext,
             encryption_key,
             'cipher-algo=aes256'
         );
     END;
     $$ LANGUAGE plpgsql;
     
     -- Function: Decrypt Insurance ID
     CREATE OR REPLACE FUNCTION decrypt_insurance_id(
         insurance_id_encrypted BYTEA,
         encryption_key TEXT
     )
     RETURNS TEXT AS $$
     BEGIN
         IF insurance_id_encrypted IS NULL THEN
             RETURN NULL;
         END IF;
         
         RETURN pgp_sym_decrypt(
             insurance_id_encrypted,
             encryption_key
         );
     EXCEPTION
         WHEN OTHERS THEN
             RAISE WARNING 'Insurance ID decryption failed: %', SQLERRM;
             RETURN NULL;
     END;
     $$ LANGUAGE plpgsql;
     
     -- Rollback script:
     -- DROP FUNCTION IF EXISTS encrypt_ssn(TEXT, TEXT);
     -- DROP FUNCTION IF EXISTS decrypt_ssn(BYTEA, TEXT);
     -- DROP FUNCTION IF EXISTS encrypt_insurance_id(TEXT, TEXT);
     -- DROP FUNCTION IF EXISTS decrypt_insurance_id(BYTEA, TEXT);
     ```

6. **Update Patient Entity Model**
   - File: `src/backend/PatientAccess.Data/Entities/Patient.cs`
   - Add encrypted properties:
     ```csharp
     using System.ComponentModel.DataAnnotations;
     using System.ComponentModel.DataAnnotations.Schema;
     
     namespace PatientAccess.Data.Entities
     {
         public class Patient
         {
             public int Id { get; set; }
             
             // Existing properties...
             public required string FirstName { get; set; }
             public required string LastName { get; set; }
             public required string Email { get; set; }
             public required string Phone { get; set; }
             public DateTime DateOfBirth { get; set; }
             
             /// <summary>
             /// Encrypted Social Security Number (stored as BYTEA, encrypted with pgcrypto).
             /// Use EncryptionService.EncryptSSN/DecryptSSN for encryption/decryption.
             /// NEVER store plaintext SSN in database.
             /// </summary>
             [Column("SSNEncrypted")]
             public byte[]? SSNEncrypted { get; set; }
             
             /// <summary>
             /// Encrypted Insurance ID Number (stored as BYTEA, encrypted with pgcrypto).
             /// Use EncryptionService.EncryptInsuranceID/DecryptInsuranceID for encryption/decryption.
             /// </summary>
             [Column("InsuranceIDEncrypted")]
             public byte[]? InsuranceIDEncrypted { get; set; }
             
             /// <summary>
             /// Encryption key version identifier for key rotation support.
             /// Default: "v1". Updated during key rotation migrations.
             /// </summary>
             [Column("EncryptionKeyVersion")]
             [MaxLength(20)]
             public string EncryptionKeyVersion { get; set; } = "v1";
             
             // Existing navigation properties...
             public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
             public ICollection<ClinicalDocument> ClinicalDocuments { get; set; } = new List<ClinicalDocument>();
         }
     }
     ```

7. **Document Encryption Key Rotation Procedure**
   - File: `docs/ENCRYPTION_KEY_ROTATION.md`
   - Comprehensive key rotation guide:
     ```markdown
     # Encryption Key Rotation Procedure
     
     ## Overview
     This document describes the procedure for rotating the column-level encryption key used for SSN and Insurance ID encryption.
     
     ## Frequency
     - **Recommended**: Rotate encryption keys every 12 months
     - **Mandatory**: Rotate immediately if key compromise suspected
     
     ## Prerequisites
     - Database administrator access with CREATE FUNCTION privileges
     - Access to production environment variables (Railway/Render dashboard)
     - Maintenance window scheduled (estimated duration: 15-30 minutes for 10,000 patients)
     
     ## Rotation Steps
     
     ### Step 1: Generate New Encryption Key
     ```bash
     # Generate new 32-byte AES-256 key
     openssl rand -base64 32
     
     # Example output: 
     # XjK9mP3nQ7wR5tY8uI1oA6sD4fG7hJ2kL
     ```
     
     ### Step 2: Update Environment Variable
     1. Login to Railway/Render dashboard
     2. Navigate to Environment Variables
     3. Add NEW variable: `ENCRYPTION__COLUMNENCRYPTIONKEY_V2=<new-key>`
     4. **DO NOT** delete old key yet (needed for decryption)
     5. Deploy application with new environment variable
     
     ### Step 3: Create Migration Script
     ```sql
     -- Migration: Rotate encryption keys for SSN and Insurance ID
     -- Date: <current-date>
     -- New Key Version: v2
     
     DO $$
     DECLARE
         old_key TEXT := '<old-encryption-key>';
         new_key TEXT := '<new-encryption-key>';
         patient_record RECORD;
     BEGIN
         -- Re-encrypt all SSN values
         FOR patient_record IN 
             SELECT "Id", "SSNEncrypted" 
             FROM "Patients" 
             WHERE "SSNEncrypted" IS NOT NULL 
               AND "EncryptionKeyVersion" = 'v1'
         LOOP
             UPDATE "Patients"
             SET "SSNEncrypted" = pgp_sym_encrypt(
                     pgp_sym_decrypt("SSNEncrypted", old_key),
                     new_key,
                     'cipher-algo=aes256'
                 ),
                 "EncryptionKeyVersion" = 'v2'
             WHERE "Id" = patient_record."Id";
         END LOOP;
         
         -- Re-encrypt all Insurance ID values
         FOR patient_record IN 
             SELECT "Id", "InsuranceIDEncrypted" 
             FROM "Patients" 
             WHERE "InsuranceIDEncrypted" IS NOT NULL 
               AND "EncryptionKeyVersion" = 'v1'
         LOOP
             UPDATE "Patients"
             SET "InsuranceIDEncrypted" = pgp_sym_encrypt(
                     pgp_sym_decrypt("InsuranceIDEncrypted", old_key),
                     new_key,
                     'cipher-algo=aes256'
                 ),
                 "EncryptionKeyVersion" = 'v2'
             WHERE "Id" = patient_record."Id";
         END LOOP;
         
         RAISE NOTICE 'Encryption key rotation completed';
     END $$;
     
     -- Verify all records migrated to v2
     SELECT COUNT(*) FROM "Patients" WHERE "EncryptionKeyVersion" = 'v1';
     -- Expected: 0
     ```
     
     ### Step 4: Verify Key Rotation
     ```sql
     -- Check encryption key version distribution
     SELECT "EncryptionKeyVersion", COUNT(*) 
     FROM "Patients" 
     GROUP BY "EncryptionKeyVersion";
     
     -- Expected output:
     -- v2 | 10000
     ```
     
     ### Step 5: Remove Old Encryption Key
     1. Wait 24-48 hours to ensure no rollback needed
     2. Remove old environment variable from Railway/Render dashboard
     3. Update appsettings.json KeyRotationDate: "2026-XX-XX"
     
     ## Rollback Procedure
     If key rotation fails:
     1. Restore database from point-in-time backup (before rotation)
     2. Revert environment variable to old key
     3. Redeploy application
     4. Investigate rotation failure logs
     
     ## Security Considerations
     - **NEVER** commit encryption keys to source control
     - **NEVER** log plaintext SSN or Insurance ID values
     - **ALWAYS** use encrypted database connections (Supabase enforces TLS by default)
     - **ALWAYS** test rotation procedure in staging environment first
     
     ## Monitoring
     After key rotation, monitor:
     - Application error logs for decryption failures
     - Database query performance (re-encryption adds temporary load)
     - Patient data access functionality (SSN display, insurance verification)
     ```

8. **Update Database Setup Documentation**
   - File: `docs/DATABASE_SETUP.md`
   - Add encryption configuration section:
     ```markdown
     ## Encryption Configuration (AC1 - US_056)
     
     ### Transparent Data Encryption (Supabase)
     All data at rest is encrypted using AES-256 encryption by default.
     
     **Verification**: See "Verify Supabase Transparent Encryption" section above.
     
     ### Column-Level Encryption (pgcrypto)
     Sensitive PII fields (SSN, Insurance ID) have additional column-level encryption.
     
     **Setup**:
     1. Enable pgcrypto extension:
        ```bash
        psql -f src/backend/scripts/migrations/20260323_enable_pgcrypto.sql
        ```
     
     2. Add encrypted columns:
        ```bash
        psql -f src/backend/scripts/migrations/20260323_add_encrypted_columns.sql
        ```
     
     3. Create encryption functions:
        ```bash
        psql -f src/backend/scripts/migrations/20260323_encryption_functions.sql
        ```
     
     4. Set encryption key environment variable:
        ```bash
        export ENCRYPTION__COLUMNENCRYPTIONKEY="$(openssl rand -base64 32)"
        ```
     
     **Key Management**:
     - Development: Key stored in appsettings.Development.json (dev-only key)
     - Production: Key stored in Railway/Render environment variables
     - Rotation: Follow docs/ENCRYPTION_KEY_ROTATION.md procedure
     
     **Compliance**:
     - FR-041: AES-256 encryption at rest ✅
     - NFR-003: Encrypt all PHI at rest ✅
     - AC1 (US_056): Column-level encryption for SSN/Insurance ID ✅
     ```

## Current Project State

```
src/backend/
├── scripts/
│   └── migrations/
│       ├── (existing migration files)
├── PatientAccess.Data/
│   └── Entities/
│       └── Patient.cs (existing entity)
└── PatientAccess.Web/
    ├── appsettings.json
    └── appsettings.Development.json

docs/
├── DATABASE_SETUP.md (existing)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | src/backend/scripts/migrations/20260323_enable_pgcrypto.sql | Enable pgcrypto extension |
| CREATE | src/backend/scripts/migrations/20260323_add_encrypted_columns.sql | Add SSNEncrypted, InsuranceIDEncrypted |
| CREATE | src/backend/scripts/migrations/20260323_encryption_functions.sql | Encryption helper functions |
| CREATE | docs/ENCRYPTION_KEY_ROTATION.md | Key rotation procedure |
| MODIFY | src/backend/PatientAccess.Data/Entities/Patient.cs | Add encrypted properties |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add encryption key placeholder |
| MODIFY | docs/DATABASE_SETUP.md | Document encryption configuration |

> Only list concrete, verifiable file operations. No speculative directory trees.

## External References

### PostgreSQL pgcrypto
- **pgcrypto Documentation**: https://www.postgresql.org/docs/16/pgcrypto.html
- **pgp_sym_encrypt Function**: https://www.postgresql.org/docs/16/pgcrypto.html#PGCRYPTO-PGP-ENC-FUNCS
- **AES-256 Cipher Configuration**: https://www.postgresql.org/docs/16/pgcrypto.html#PGCRYPTO-PGP-ENC-FUNCS-GEN

### Supabase Documentation
- **Encryption at Rest**: https://supabase.com/docs/guides/platform/security#encryption-at-rest
- **Database Security**: https://supabase.com/docs/guides/database/overview#security

### HIPAA Compliance
- **FR-041**: AES-256 encryption at rest (spec.md)
- **NFR-003**: Encrypt all PHI at rest using AES-256 (design.md)
- **AG-002**: 100% HIPAA compliance for data handling (design.md)

## Build Commands
```powershell
# Run migrations locally (requires DATABASE_URL environment variable)
cd src/backend/scripts/migrations

# 1. Enable pgcrypto
psql $env:DATABASE_URL -f 20260323_enable_pgcrypto.sql

# 2. Add encrypted columns
psql $env:DATABASE_URL -f 20260323_add_encrypted_columns.sql

# 3. Create encryption functions
psql $env:DATABASE_URL -f 20260323_encryption_functions.sql

# 4. Verify migrations
psql $env:DATABASE_URL -c "SELECT extname FROM pg_extension WHERE extname = 'pgcrypto';"
psql $env:DATABASE_URL -c "SELECT column_name FROM information_schema.columns WHERE table_name = 'Patients' AND column_name LIKE '%Encrypted';"

# Generate encryption key for development
openssl rand -base64 32
```

## Validation Strategy

### Database Tests
- File: `src/backend/PatientAccess.Tests/Database/EncryptionTests.cs`
- Test cases:
  1. **Test_Pgcrypto_Extension_Loaded**
     - Query: `SELECT * FROM pg_extension WHERE extname = 'pgcrypto';`
     - Assert: Extension exists and is enabled
  2. **Test_EncryptSSN_ReturnsEncryptedBytea**
     - Input: SSN = "123-45-6789", Key = "test-key"
     - Call: `SELECT encrypt_ssn('123-45-6789', 'test-key');`
     - Assert: Returns BYTEA (not NULL), length > 0
  3. **Test_DecryptSSN_ReturnsPlaintextSSN**
     - Setup: Encrypt SSN "123-45-6789" with "test-key"
     - Call: `SELECT decrypt_ssn(<encrypted>, 'test-key');`
     - Assert: Returns "123-45-6789"
  4. **Test_EncryptSSN_InvalidFormat_ThrowsException**
     - Input: SSN = "invalid"
     - Call: `SELECT encrypt_ssn('invalid', 'test-key');`
     - Assert: Exception raised with message "Invalid SSN format"
  5. **Test_DecryptSSN_WrongKey_ReturnsNull**
     - Setup: Encrypt with "key1", decrypt with "key2"
     - Call: `SELECT decrypt_ssn(<encrypted-with-key1>, 'key2');`
     - Assert: Returns NULL (graceful failure)
  6. **Test_Patient_EncryptedColumns_Exist**
     - Query: `SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'Patients' AND column_name IN ('SSNEncrypted', 'InsuranceIDEncrypted');`
     - Assert: Both columns exist with data_type = 'bytea'
  7. **Test_Patient_EncryptionKeyVersion_HasIndex**
     - Query: `SELECT indexname FROM pg_indexes WHERE tablename = 'Patients' AND indexname = 'IX_Patients_EncryptionKeyVersion';`
     - Assert: Index exists

### Integration Tests
- File: `src/backend/PatientAccess.Tests/Integration/PatientEncryptionIntegrationTests.cs`
- Test cases:
  1. **Test_CreatePatient_WithSSN_EncryptsToDatabase**
     - Create patient with SSN = "123-45-6789"
     - Query database directly: `SELECT SSNEncrypted FROM Patients WHERE Id = <id>;`
     - Assert: SSNEncrypted is BYTEA (not plaintext)
  2. **Test_GetPatient_WithSSN_DecryptsCorrectly**
     - Setup: Patient with encrypted SSN
     - Call: PatientService.GetPatientById(id)
     - Assert: Returned Patient.SSN = "123-45-6789" (decrypted)

### Acceptance Criteria Validation
- **AC1**: ✅ Supabase transparent encryption verified + column-level encryption implemented for SSN/InsuranceID

## Success Criteria Checklist
- [MANDATORY] Supabase transparent data encryption (AES-256) verified and documented
- [MANDATORY] pgcrypto extension enabled in PostgreSQL
- [MANDATORY] SSNEncrypted column added to Patients table (BYTEA type)
- [MANDATORY] InsuranceIDEncrypted column added to Patients table (BYTEA type)
- [MANDATORY] EncryptionKeyVersion column added for key rotation support
- [MANDATORY] encrypt_ssn() function created with AES-256 cipher
- [MANDATORY] decrypt_ssn() function created with error handling
- [MANDATORY] encrypt_insurance_id() function created
- [MANDATORY] decrypt_insurance_id() function created
- [MANDATORY] Patient entity updated with encrypted properties
- [MANDATORY] Encryption key configuration in appsettings.json
- [MANDATORY] Development encryption key in appsettings.Development.json
- [MANDATORY] Encryption key rotation procedure documented
- [MANDATORY] DATABASE_SETUP.md updated with encryption configuration
- [MANDATORY] Migration scripts with rollback support
- [MANDATORY] Database test: pgcrypto extension loaded
- [MANDATORY] Database test: Encrypt/decrypt SSN roundtrip successful
- [MANDATORY] Database test: Invalid SSN format raises exception
- [RECOMMENDED] Integration test: Patient creation encrypts SSN to database
- [RECOMMENDED] Performance test: Encryption overhead < 50ms per operation

## Estimated Effort
**3 hours** (Supabase verification + pgcrypto setup + migrations + entity updates + key rotation docs + tests)
