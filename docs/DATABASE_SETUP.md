# Database Setup Guide

## Overview

This guide walks you through provisioning a PostgreSQL 16 database instance on Supabase with pgvector extension for the Patient Access Platform.

## Prerequisites

- A valid email address for Supabase account creation
- Internet connection
- Access to the backend project configuration

## Step 1: Create Supabase Account and Project

### 1.1 Sign Up for Supabase

1. Navigate to [https://supabase.com](https://supabase.com)
2. Click "Start your project" or "Sign up"
3. Create an account using GitHub, Google, or email
4. Verify your email address if required

### 1.2 Create New Project

1. After logging in, click "New Project" from the dashboard
2. Fill in the following details:
   - **Project Name**: `patient-access-platform` (or your preferred name)
   - **Database Password**: Create a strong password (save this securely!)
   - **Region**: Select the region closest to your users
   - **Pricing Plan**: Select "Free" tier
3. Click "Create new project"
4. Wait 2-3 minutes for the database to provision

**Important**: Save your database password immediately - you won't be able to retrieve it later!

## Step 2: Enable pgvector Extension

### 2.1 Access SQL Editor

1. In your Supabase project dashboard, navigate to the **SQL Editor** from the left sidebar
2. Click "New query" to create a new SQL script

### 2.2 Enable pgvector

Execute the following SQL command:

```sql
-- Enable pgvector extension for vector similarity search
CREATE EXTENSION IF NOT EXISTS vector;
```

Click "Run" to execute the query.

### 2.3 Verify pgvector Installation

Run the following verification query:

```sql
-- Verify pgvector extension is enabled
SELECT * FROM pg_extension WHERE extname = 'vector';
```

You should see a row with `extname = 'vector'` in the results.

### 2.4 Test Vector Column Support

Create a test table to confirm 1536-dimensional vector support:

```sql
-- Test vector column creation with 1536 dimensions
CREATE TABLE IF NOT EXISTS test_vectors (
    id SERIAL PRIMARY KEY,
    embedding vector(1536),
    created_at TIMESTAMP DEFAULT NOW()
);

-- Insert a test vector
INSERT INTO test_vectors (embedding) 
VALUES (array_fill(0, ARRAY[1536])::vector);

-- Verify the test
SELECT id, vector_dims(embedding) as dimensions FROM test_vectors;

-- Clean up test table (optional)
DROP TABLE test_vectors;
```

Expected output: `dimensions = 1536`

## Step 3: Retrieve Connection String

### 3.1 Get Connection Details

1. Navigate to **Settings** → **Database** in the left sidebar
2. Scroll down to the **Connection string** section
3. Select the **URI** tab (not the JDBC or .NET tabs)
4. Find the connection string in this format:

```
postgresql://postgres:[YOUR-PASSWORD]@db.[PROJECT-REF].supabase.co:5432/postgres
```

5. Copy the connection string
6. Replace `[YOUR-PASSWORD]` with the database password you created in Step 1.2

### 3.2 Connection String Format

The final connection string should look like:

```
postgresql://postgres:your_actual_password@db.abcdefghijklmnop.supabase.co:5432/postgres
```

**Security Note**: Never commit this connection string directly to version control!

## Step 4: Configure Local Environment

### 4.1 Create .env File

1. Navigate to the project root directory: `Propel-Project-Team-12/`
2. Create a new file named `.env` (note the leading dot)
3. Add the following content:

```env
# Supabase PostgreSQL Connection String
# Replace with your actual connection string from Step 3
DB_CONNECTION_STRING=postgresql://postgres:YOUR_PASSWORD@db.YOUR_PROJECT_REF.supabase.co:5432/postgres

# Example:
# DB_CONNECTION_STRING=postgresql://postgres:mySecureP@ssw0rd@db.abcdefghijklmnop.supabase.co:5432/postgres
```

4. Save the file
5. Verify `.env` is listed in `.gitignore` (it should already be included)

### 4.2 Using .NET User Secrets (Alternative - Recommended for Development)

For enhanced security during development, you can use .NET User Secrets instead of `.env`:

1. Open a terminal in the backend project directory:
   ```powershell
   cd src\backend\PatientAccess.Web
   ```

2. Initialize user secrets:
   ```powershell
   dotnet user-secrets init
   ```

3. Set the connection string:
   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "postgresql://postgres:YOUR_PASSWORD@db.YOUR_PROJECT_REF.supabase.co:5432/postgres"
   ```

4. Verify the secret was set:
   ```powershell
   dotnet user-secrets list
   ```

**Note**: User Secrets are stored outside the project directory and are specific to your local machine.

## Step 5: Verify Database Connectivity

### 5.1 Test Connection Using psql (Optional)

If you have PostgreSQL client tools installed:

```powershell
# Test basic connection
psql "postgresql://postgres:YOUR_PASSWORD@db.YOUR_PROJECT_REF.supabase.co:5432/postgres" -c "SELECT version();"

# Test pgvector extension
psql "postgresql://postgres:YOUR_PASSWORD@db.YOUR_PROJECT_REF.supabase.co:5432/postgres" -c "SELECT * FROM pg_extension WHERE extname = 'vector';"
```

### 5.2 Test Connection from .NET Application

1. Navigate to the backend project:
   ```powershell
   cd src\backend\PatientAccess.Web
   ```

2. Run the application:
   ```powershell
   dotnet run
   ```

3. Check the console output for any database connection errors
4. If the application starts successfully without database errors, the connection is working

## Troubleshooting

### Connection Refused

**Problem**: Cannot connect to the database.

**Solutions**:
- Verify your internet connection
- Check that the connection string is correct
- Ensure the Supabase project is fully provisioned (wait a few minutes if just created)
- Verify your IP is not blocked by firewall rules

### Authentication Failed

**Problem**: Password authentication failed.

**Solutions**:
- Double-check the password in your connection string
- Ensure there are no extra spaces or special characters that need URL encoding
- Try resetting the database password in Supabase Settings → Database → "Reset database password"

### pgvector Extension Not Found

**Problem**: Vector column creation fails.

**Solutions**:
- Re-run the `CREATE EXTENSION IF NOT EXISTS vector;` command
- Verify you're using PostgreSQL 16 (check with `SELECT version();`)
- Contact Supabase support if the extension is not available

### Environment Variable Not Loaded

**Problem**: Application cannot find the connection string.

**Solutions**:
- Verify `.env` file is in the project root directory
- Check that the environment variable name matches exactly: `DB_CONNECTION_STRING`
- Restart your IDE or terminal to reload environment variables
- Consider using .NET User Secrets for development (see Step 4.2)

## Connection String Security Best Practices

1. **Never commit** `.env` files to version control
2. **Use User Secrets** for local development in .NET projects
3. **Use environment variables** for production deployments
4. **Rotate passwords** regularly (every 90 days recommended)
5. **Use connection pooling** to manage database connections efficiently
6. **Enable SSL** for all database connections (Supabase enables this by default)

## Next Steps

After completing this setup:

1. ✅ Supabase project created with PostgreSQL 16
2. ✅ pgvector extension enabled and verified
3. ✅ Connection string securely stored in `.env` or User Secrets
4. ✅ Database connectivity verified

You can now proceed to:
- Set up Entity Framework Core DbContext
- Create database migrations
- Define data models and entities
- Implement repository pattern for data access

## Additional Resources

- [Supabase Documentation](https://supabase.com/docs)
- [pgvector Extension Documentation](https://github.com/pgvector/pgvector)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/16/)
- [.NET Configuration Documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Npgsql Connection Strings](https://www.npgsql.org/doc/connection-string-parameters.html)

## Support

If you encounter issues not covered by this guide:
- Review Supabase documentation: https://supabase.com/docs/guides/database
- Check pgvector GitHub issues: https://github.com/pgvector/pgvector/issues
- Consult the project team or technical lead

---

## Encryption Configuration (US_056 - AC1)

**Epic:** EP-010 - HIPAA Compliance & Security Hardening  
**Requirement:** FR-041, NFR-003 (AES-256 encryption at rest)

The Patient Access Platform implements multi-layer encryption for PHI data protection:
- **Layer 1:** Supabase transparent data encryption (AES-256) for all data at rest
- **Layer 2:** Column-level encryption (pgcrypto) for highly sensitive PII fields (SSN, Insurance ID)

### Verify Supabase Transparent Data Encryption

Supabase provides AES-256 encryption at rest for all PostgreSQL data by default (free tier included).

**Verification Steps:**

1. Login to Supabase dashboard: https://supabase.com/dashboard
2. Navigate to your project → **Settings** → **Database**
3. Confirm **"Encryption at Rest: AES-256"** is displayed in the database settings

**Coverage:**
- All table data files (.dat)
- Write-Ahead Log (WAL) files
- Database backups
- Temporary files

**Security Note:** Supabase manages encryption keys for transparent data encryption. No additional configuration required.

### Enable Column-Level Encryption (pgcrypto)

For defense-in-depth, sensitive PII fields (SSN, Insurance ID) have additional column-level encryption using PostgreSQL pgcrypto extension.

**Setup Steps:**

1. **Enable pgcrypto extension:**
   ```bash
   cd src/backend/scripts/migrations
   psql $DATABASE_URL -f 20260323_enable_pgcrypto.sql
   ```

   Expected output:
   ```
   CREATE EXTENSION
   NOTICE:  pgcrypto extension enabled successfully
   NOTICE:  pgcrypto AES-256 encryption/decryption validated successfully
   ```

2. **Add encrypted columns to Users table:**
   ```bash
   psql $DATABASE_URL -f 20260323_add_encrypted_columns.sql
   ```

   Expected output:
   ```
   ALTER TABLE
   NOTICE:  Encrypted columns added successfully to Users table
   ```

3. **Create encryption helper functions:**
   ```bash
   psql $DATABASE_URL -f 20260323_encryption_functions.sql
   ```

   Expected output:
   ```
   CREATE FUNCTION (encrypt_ssn)
   CREATE FUNCTION (decrypt_ssn)
   CREATE FUNCTION (encrypt_insurance_id)
   CREATE FUNCTION (decrypt_insurance_id)
   NOTICE:  Encryption functions validated successfully
   ```

4. **Set encryption key environment variable:**
   
   **Development:**
   ```powershell
   # The development key is already configured in appsettings.Development.json
   # No additional setup required for local development
   ```

   **Production (Railway/Render):**
   ```bash
   # Generate secure 32-byte encryption key
   openssl rand -base64 32
   
   # Set environment variable in Railway/Render dashboard:
   # Variable Name: ENCRYPTION__COLUMNENCRYPTIONKEY
   # Variable Value: <output-from-openssl-command>
   ```

   **Security Critical:** 
   - ✅ Use OpenSSL to generate production keys (minimum 32 bytes)
   - ❌ NEVER commit encryption keys to source control
   - ❌ NEVER reuse development keys in production
   - ❌ NEVER log plaintext SSN or Insurance ID values

### Verify Column-Level Encryption

**Test encryption functions:**

```sql
-- Test SSN encryption/decryption (replace $KEY with your encryption key)
SELECT 
    encrypt_ssn('123-45-6789', '$KEY') AS encrypted_ssn;
-- Expected: \x<hexadecimal-bytea> (binary encrypted data)

SELECT 
    decrypt_ssn(
        encrypt_ssn('123-45-6789', '$KEY'),
        '$KEY'
    ) AS decrypted_ssn;
-- Expected: 123-45-6789

-- Verify encrypted columns exist
SELECT 
    column_name,
    data_type
FROM information_schema.columns
WHERE table_name = 'Users'
AND column_name IN ('SSNEncrypted', 'InsuranceIDEncrypted', 'EncryptionKeyVersion');
-- Expected: 3 rows with data_type = 'bytea' (SSN/Insurance) and 'character varying' (KeyVersion)
```

**Test encryption from application:**

```bash
# Create user with encrypted SSN (via API)
curl -X POST https://api.patient-access.com/api/users \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <admin-jwt-token>" \
  -d '{
    "email": "test@example.com",
    "name": "Test User",
    "ssn": "123-45-6789",
    "insuranceId": "ABC123456",
    "role": "Patient"
  }'

# Verify SSN is encrypted in database (should NOT see plaintext)
psql $DATABASE_URL -c "SELECT \"UserId\", \"SSNEncrypted\", \"EncryptionKeyVersion\" FROM \"Users\" WHERE \"Email\" = 'test@example.com';"
# Expected: Binary data in SSNEncrypted column, NOT plaintext "123-45-6789"
```

### Encryption Key Management

**Key Storage:**
- **Development:** Key stored in `appsettings.Development.json` (intentionally weak for testing)
- **Production:** Key stored in Railway/Render environment variables (ENCRYPTION__COLUMNENCRYPTIONKEY)
- **Security:** Keys never committed to git, never logged, never transmitted over unencrypted channels

**Key Rotation Procedure:**

Follow the detailed key rotation guide: [docs/ENCRYPTION_KEY_ROTATION.md](ENCRYPTION_KEY_ROTATION.md)

**Key Rotation Schedule:**
- **Scheduled:** Rotate every 12 months (mandatory)
- **Immediate:** Rotate if key compromise suspected
- **Compliance:** Rotate before external security audits

**Quick Rotation Steps:**
1. Generate new key: `openssl rand -base64 32`
2. Add new key as ENCRYPTION__COLUMNENCRYPTIONKEY_V2 in environment variables
3. Run migration script to re-encrypt all data with new key
4. Verify all records migrated to new key version
5. Remove old key after 24-48 hour verification period

### Compliance Verification

**HIPAA Compliance Checklist:**

- [x] **FR-041:** AES-256 encryption at rest ✅ (Supabase + pgcrypto)
- [x] **NFR-003:** Encrypt all PHI at rest ✅ (SSN/Insurance ID encrypted)
- [x] **AC1 (US_056):** Column-level encryption for sensitive fields ✅ (pgcrypto AES-256)
- [x] **AG-002:** 100% HIPAA compliance for data handling ✅ (Defense-in-depth encryption)

**Encryption Coverage:**
- ✅ Database files (Supabase transparent encryption)
- ✅ Database backups (Supabase transparent encryption)
- ✅ SSN fields (pgcrypto column-level encryption)
- ✅ Insurance ID fields (pgcrypto column-level encryption)
- ✅ Database connections (TLS 1.2+ enforced by Supabase)

### Troubleshooting Encryption Issues

**Problem:** pgcrypto extension not found

**Solution:**
```sql
-- Verify extension is enabled
SELECT * FROM pg_extension WHERE extname = 'pgcrypto';

-- If not found, re-run enable script
\i src/backend/scripts/migrations/20260323_enable_pgcrypto.sql
```

**Problem:** Decryption returns NULL

**Causes:**
- Wrong encryption key in environment variable
- Data encrypted with different key version
- Corrupted encrypted data

**Solution:**
```sql
-- Check encryption key version distribution
SELECT "EncryptionKeyVersion", COUNT(*) 
FROM "Users" 
GROUP BY "EncryptionKeyVersion";

-- Test decryption with correct key
SELECT decrypt_ssn("SSNEncrypted", '$CORRECT_KEY') FROM "Users" LIMIT 1;
```

**Problem:** Application logs show "Encryption key must be at least 8 characters" error

**Solution:**
- Verify ENCRYPTION__COLUMNENCRYPTIONKEY environment variable is set
- Check key is at least 32 characters for AES-256 strength
- Use OpenSSL to generate proper key: `openssl rand -base64 32`

### Security Best Practices

1. **Key Management:**
   - ✅ Generate keys with OpenSSL (cryptographically secure random number generator)
   - ✅ Store keys in platform-native environment variables (Railway/Render secrets)
   - ✅ Rotate keys annually or immediately upon suspected compromise
   - ❌ Never commit keys to version control (even private repositories)
   - ❌ Never log plaintext SSN/Insurance ID values

2. **Access Control:**
   - ✅ Restrict database admin access to security-cleared personnel only
   - ✅ Use least-privilege principle for application database credentials
   - ✅ Audit all direct database access (use Supabase audit logs)
   - ❌ Never grant SELECT access to SSNEncrypted columns to external tools

3. **Data Transmission:**
   - ✅ Always use TLS-encrypted connections (Supabase enforces this by default)
   - ✅ Validate SSL certificates (never use Trust Server Certificate=true in production)
   - ✅ Decrypt only in application layer, never expose plaintext in logs or responses

### References

- **FR-041:** AES-256 encryption at rest (spec.md)
- **NFR-003:** Encrypt all PHI at rest using AES-256 (design.md)
- **AG-002:** 100% HIPAA compliance for data handling (design.md)
- **PostgreSQL pgcrypto Documentation:** https://www.postgresql.org/docs/16/pgcrypto.html
- **Supabase Encryption:** https://supabase.com/docs/guides/platform/security#encryption-at-rest
- **HIPAA Security Rule:** https://www.hhs.gov/hipaa/for-professionals/security/
- **Encryption Key Rotation Procedure:** [docs/ENCRYPTION_KEY_ROTATION.md](ENCRYPTION_KEY_ROTATION.md)
