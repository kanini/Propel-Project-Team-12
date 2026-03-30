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
