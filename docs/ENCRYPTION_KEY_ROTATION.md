# Encryption Key Rotation Procedure

**Epic:** EP-010 - HIPAA Compliance & Security Hardening  
**User Story:** US_056 - PHI Encryption at Rest & In Transit  
**Requirement:** FR-041, NFR-003 (AES-256 encryption at rest)  
**Last Updated:** 2026-03-23

## Overview

This document describes the procedure for rotating the column-level encryption key used for SSN and Insurance ID encryption in the Patient Access Platform. Encryption keys protect sensitive PHI data encrypted using PostgreSQL pgcrypto extension with AES-256 symmetric encryption.

## Key Management Philosophy

- **Defense-in-Depth:** Database-level transparent encryption (Supabase) + column-level encryption (pgcrypto)
- **Key Separation:** Database encryption keys managed by Supabase, application column encryption keys managed by operations team
- **Zero-Knowledge Principle:** Encryption keys never committed to source control, only stored in secure environment variables

## Rotation Frequency

| Scenario | Action | Timeline |
|----------|--------|----------|
| **Scheduled Rotation** | Rotate encryption keys every 12 months | Annual (recommended) |
| **Key Compromise** | Rotate immediately if key exposure suspected | Immediate (mandatory) |
| **Compliance Audit** | Rotate before external security audits | As needed |
| **Personnel Change** | Rotate if DevOps personnel with key access leaves | Within 48 hours |

## Prerequisites

Before starting key rotation:

1. **Access Requirements:**
   - Database administrator access with `CREATE FUNCTION` and `UPDATE` privileges
   - Production environment variable access (Railway/Render dashboard)
   - Backup restoration access (in case rollback needed)

2. **Timing Considerations:**
   - Schedule during maintenance window (estimated duration: 15-30 minutes for 10,000 users)
   - Notify users of potential brief downtime
   - Ensure backup retention policy allows 7-day point-in-time recovery

3. **Testing:**
   - Test rotation procedure in staging environment first
   - Validate encryption/decryption with both old and new keys
   - Verify application still functions with new key

## Rotation Steps

### Step 1: Generate New Encryption Key

Generate a cryptographically secure 32-byte key using OpenSSL:

```bash
# Generate new AES-256 encryption key (256 bits = 32 bytes, base64-encoded = 44 characters)
openssl rand -base64 32

# Example output:
# XjK9mP3nQ7wR5tY8uI1oA6sD4fG7hJ2kL5pN8qR3tU6vW
```

**Security Note:** Never reuse old keys. Each rotation must generate a fresh key.

### Step 2: Update Environment Variables

1. Login to deployment platform (Railway/Render):
   - Railway: https://railway.app/dashboard
   - Render: https://dashboard.render.com/

2. Navigate to **Environment Variables** section

3. Add NEW key version (do NOT delete old key yet):
   ```bash
   # Add new key as version 2 (keep old key as ENCRYPTION__COLUMNENCRYPTIONKEY for now)
   ENCRYPTION__COLUMNENCRYPTIONKEY_V2=<new-key-from-step-1>
   ```

4. Deploy application with new environment variable:
   - Railway: Automatic deployment triggered by environment variable change
   - Render: Manual deploy via dashboard

5. **Critical:** Old key must remain active during rotation for decryption

### Step 3: Create Migration Script

Create SQL migration script to re-encrypt all data with new key:

```sql
-- File: src/backend/scripts/migrations/YYYYMMDD_rotate_encryption_key_v2.sql
-- Description: Rotate encryption keys from v1 to v2 for SSN and Insurance ID fields
-- Epic: EP-010, User Story: US_056, Task: key_rotation
-- Date: <current-date>
-- New Key Version: v2

DO $$
DECLARE
    old_key TEXT := '<old-encryption-key-from-env>'; -- Retrieve from current ENCRYPTION__COLUMNENCRYPTIONKEY
    new_key TEXT := '<new-encryption-key-from-step-1>'; -- From ENCRYPTION__COLUMNENCRYPTIONKEY_V2
    user_record RECORD;
    re_encrypted_count INTEGER := 0;
BEGIN
    RAISE NOTICE 'Starting encryption key rotation from v1 to v2...';
    
    -- Re-encrypt all SSN values
    FOR user_record IN 
        SELECT "UserId", "SSNEncrypted" 
        FROM "Users" 
        WHERE "SSNEncrypted" IS NOT NULL 
          AND "EncryptionKeyVersion" = 'v1'
    LOOP
        BEGIN
            -- Decrypt with old key, re-encrypt with new key
            UPDATE "Users"
            SET "SSNEncrypted" = pgp_sym_encrypt(
                    pgp_sym_decrypt("SSNEncrypted", old_key),
                    new_key,
                    'cipher-algo=aes256'
                ),
                "EncryptionKeyVersion" = 'v2',
                "UpdatedAt" = NOW()
            WHERE "UserId" = user_record."UserId";
            
            re_encrypted_count := re_encrypted_count + 1;
            
            -- Progress indicator every 100 records
            IF re_encrypted_count % 100 = 0 THEN
                RAISE NOTICE 'Re-encrypted % SSN records...', re_encrypted_count;
            END IF;
        EXCEPTION
            WHEN OTHERS THEN
                RAISE WARNING 'Failed to re-encrypt SSN for UserId %: %', user_record."UserId", SQLERRM;
                -- Log failure but continue (individual record corruption should not halt entire rotation)
        END;
    END LOOP;
    
    -- Reset counter for Insurance ID
    re_encrypted_count := 0;
    
    -- Re-encrypt all Insurance ID values
    FOR user_record IN 
        SELECT "UserId", "InsuranceIDEncrypted" 
        FROM "Users" 
        WHERE "InsuranceIDEncrypted" IS NOT NULL 
          AND "EncryptionKeyVersion" = 'v1'
    LOOP
        BEGIN
            UPDATE "Users"
            SET "InsuranceIDEncrypted" = pgp_sym_encrypt(
                    pgp_sym_decrypt("InsuranceIDEncrypted", old_key),
                    new_key,
                    'cipher-algo=aes256'
                ),
                "EncryptionKeyVersion" = 'v2',
                "UpdatedAt" = NOW()
            WHERE "UserId" = user_record."UserId";
            
            re_encrypted_count := re_encrypted_count + 1;
            
            IF re_encrypted_count % 100 = 0 THEN
                RAISE NOTICE 'Re-encrypted % Insurance ID records...', re_encrypted_count;
            END IF;
        EXCEPTION
            WHEN OTHERS THEN
                RAISE WARNING 'Failed to re-encrypt Insurance ID for UserId %: %', user_record."UserId", SQLERRM;
        END;
    END LOOP;
    
    RAISE NOTICE 'Encryption key rotation completed successfully';
END $$;

-- Verify all records migrated to v2
SELECT 
    "EncryptionKeyVersion",
    COUNT(*) AS "Record Count"
FROM "Users" 
GROUP BY "EncryptionKeyVersion"
ORDER BY "EncryptionKeyVersion";

-- Expected output after successful rotation:
-- v2 | 10000
```

### Step 4: Execute Migration

Run migration script against production database:

```bash
# Using psql command-line tool
psql $DATABASE_URL -f src/backend/scripts/migrations/YYYYMMDD_rotate_encryption_key_v2.sql

# Using Railway CLI
railway run psql -f src/backend/scripts/migrations/YYYYMMDD_rotate_encryption_key_v2.sql

# Expected output:
# NOTICE:  Starting encryption key rotation from v1 to v2...
# NOTICE:  Re-encrypted 100 SSN records...
# NOTICE:  Re-encrypted 200 SSN records...
# ...
# NOTICE:  Encryption key rotation completed successfully
#  EncryptionKeyVersion | Record Count
# ----------------------+--------------
#  v2                   |        10000
```

### Step 5: Verify Key Rotation

1. **Check encryption key version distribution:**
   ```sql
   SELECT "EncryptionKeyVersion", COUNT(*) 
   FROM "Users" 
   GROUP BY "EncryptionKeyVersion";
   
   -- Expected: All records show v2
   -- v2 | 10000
   ```

2. **Test decryption with new key:**
   ```sql
   -- Replace $NEW_KEY with actual new encryption key
   SELECT 
       "UserId",
       decrypt_ssn("SSNEncrypted", '$NEW_KEY') AS "DecryptedSSN"
   FROM "Users"
   WHERE "SSNEncrypted" IS NOT NULL
   LIMIT 5;
   
   -- Expected: Valid SSN values (XXX-XX-XXXX format)
   ```

3. **Verify application functionality:**
   ```bash
   # Test user registration with encrypted SSN
   curl -X POST https://api.patient-access.com/api/users \
     -H "Content-Type: application/json" \
     -d '{"email":"test@example.com", "ssn":"123-45-6789", ...}'
   
   # Expected: HTTP 201 Created
   
   # Test user retrieval (verify SSN decryption)
   curl -X GET https://api.patient-access.com/api/users/me \
     -H "Authorization: Bearer <jwt-token>"
   
   # Expected: JSON response with decrypted SSN field
   ```

### Step 6: Update Configuration

1. **Update environment variables:**
   ```bash
   # In Railway/Render dashboard:
   # 1. Rename ENCRYPTION__COLUMNENCRYPTIONKEY to ENCRYPTION__COLUMNENCRYPTIONKEY_V1_ARCHIVED
   # 2. Rename ENCRYPTION__COLUMNENCRYPTIONKEY_V2 to ENCRYPTION__COLUMNENCRYPTIONKEY
   # 3. Update appsettings.json KeyRotationDate
   ```

2. **Update appsettings.json:**
   ```json
   {
     "Encryption": {
       "ColumnEncryptionKey": "PLACEHOLDER_SET_IN_ENVIRONMENT_VARIABLE",
       "KeyRotationDate": "2026-03-23", // Update to current date
       "_comment": "..."
     }
   }
   ```

3. **Commit configuration change to git:**
   ```bash
   git add src/backend/PatientAccess.Web/appsettings.json
   git commit -m "chore(security): update encryption key rotation date to 2026-03-23"
   git push origin main
   ```

### Step 7: Remove Old Encryption Key

**Wait 24-48 hours** to ensure:
- No rollback needed
- All application instances use new key
- Monitoring shows no decryption errors

Then remove old key:

1. Login to Railway/Render dashboard
2. Delete environment variable: `ENCRYPTION__COLUMNENCRYPTIONKEY_V1_ARCHIVED`
3. Deploy application (automatic on variable change)

**Security Note:** Do NOT store old keys in password managers or documentation after removal.

## Rollback Procedure

If key rotation fails or causes application errors:

### Immediate Rollback (within 48 hours)

1. **Restore database from point-in-time backup:**
   ```bash
   # Supabase database backup restoration (via dashboard)
   # Navigate to: Supabase Dashboard → Project → Database → Backups
   # Select backup from before rotation start time
   # Click "Restore"
   ```

2. **Revert environment variable:**
   ```bash
   # In Railway/Render dashboard:
   # Delete: ENCRYPTION__COLUMNENCRYPTIONKEY (new key)
   # Rename: ENCRYPTION__COLUMNENCRYPTIONKEY_V1_ARCHIVED back to ENCRYPTION__COLUMNENCRYPTIONKEY
   ```

3. **Redeploy application:**
   ```bash
   # Railway: Automatic deployment triggered
   # Render: Manual deploy via dashboard
   ```

4. **Investigate rotation failure:**
   - Review application logs for decryption errors
   - Check database migration script for SQL errors
   - Verify encryption key length (must be 32+ bytes for AES-256)
   - Test encryption/decryption in staging with same keys

### Delayed Rollback (after 48 hours)

If old key already removed, rollback requires manual re-encryption:

1. Restore database backup (as above)
2. Generate temporary key for migration
3. Re-run rotation with corrected procedure
4. Validate thoroughly before marking complete

## Security Considerations

### Key Storage Security

- ✅ **DO:** Store encryption keys in environment variables (Railway/Render dashboard)
- ✅ **DO:** Restrict environment variable access to DevOps team only
- ✅ **DO:** Use separate keys for development, staging, and production
- ❌ **DO NOT:** Commit encryption keys to source control (even private repositories)
- ❌ **DO NOT:** Log plaintext SSN or Insurance ID values in application logs
- ❌ **DO NOT:** Store encryption keys in password managers (use platform-native secrets management)

### Database Connection Security

- ✅ **ALWAYS:** Use encrypted database connections (Supabase enforces TLS by default)
- ✅ **ALWAYS:** Validate SSL certificates (never use `Trust Server Certificate=true` in production)
- ✅ **ALWAYS:** Run rotation scripts during maintenance windows with user notification

### Audit Trail

Document all key rotations:

| Date | Old Key Version | New Key Version | Reason | Performed By |
|------|----------------|-----------------|--------|--------------|
| 2026-03-23 | N/A | v1 | Initial encryption implementation | DevOps Team |
| 2027-03-23 | v1 | v2 | Scheduled annual rotation | DevOps Team |

## Monitoring Post-Rotation

After key rotation, monitor for 7 days:

1. **Application Error Logs:**
   ```bash
   # Check for decryption failures
   grep -i "decryption failed" /var/log/app/*.log
   ```

2. **Database Query Performance:**
   - Encryption/decryption adds 10-50ms latency per operation
   - Monitor slow query logs for performance degradation

3. **User-Reported Issues:**
   - Monitor support tickets for SSN/Insurance ID display failures
   - Validate patient data access functionality

4. **Compliance Verification:**
   - Verify all PHI remains encrypted at rest (query database directly)
   - Confirm decryption only occurs in application layer

## Support Contacts

| Issue Type | Contact | Response Time |
|------------|---------|---------------|
| Key Rotation Failure | DevOps Team: ops@patient-access.com | 1 hour |
| Decryption Errors | Backend Team: backend@patient-access.com | 2 hours |
| Security Incident | Security Team: security@patient-access.com | Immediate |

## References

- **FR-041:** AES-256 encryption at rest (spec.md)
- **NFR-003:** Encrypt all PHI at rest using AES-256 (design.md)
- **AG-002:** 100% HIPAA compliance for data handling (design.md)
- **PostgreSQL pgcrypto Documentation:** https://www.postgresql.org/docs/16/pgcrypto.html
- **HIPAA Security Rule:** https://www.hhs.gov/hipaa/for-professionals/security/
