# Task - task_001_database_postgresql_provisioning

## Requirement Reference
- User Story: us_003
- Story Location: .propel/context/tasks/EP-TECH/us_003/us_003.md
- Acceptance Criteria:
    - AC-1: PostgreSQL 16 instance running on Supabase with connection string in environment configuration
    - AC-2: pgvector extension enabled supporting 1536-dimensional vector columns
    - AC-3: `PatientAccessDbContext` class exists with proper connection string resolution
    - AC-4: EF Core migrations can be generated and applied successfully
- Edge Case:
    - Database connection failure at startup should fail fast with descriptive message
    - pgvector extension unavailable should fail migration with clear error

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

## Applicable Technology Stack
| Layer | Technology | Version |
|-------|------------|---------|
| Database | PostgreSQL | 16.x |
| Database | pgvector | 0.5+ |
| Backend | Entity Framework Core | 8.x |
| Library | Npgsql.EntityFrameworkCore.PostgreSQL | 8.x |

**Note**: All code and libraries MUST be compatible with versions above.

## AI References (AI Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **AI Impact** | No |
| **AIR Requirements** | N/A |
| **AI Pattern** | N/A |
| **Prompt Template Path** | N/A |
| **Guardrails Config** | N/A |
| **Model Provider** | N/A |

## Mobile References (Mobile Tasks Only)
| Reference Type | Value |
|----------------|-------|
| **Mobile Impact** | No |
| **Platform Target** | N/A |
| **Min OS Version** | N/A |
| **Mobile Framework** | N/A |

## Task Overview
Provision a PostgreSQL 16 database instance on Supabase with pgvector extension enabled for vector similarity search capabilities. Configure secure connection string storage using environment variables and .NET Secret Manager. Install pgvector extension to support 1536-dimensional embeddings for AI/ML use cases. Establish database connectivity verification to ensure the application fails fast with clear diagnostics if the database is unreachable.

## Dependent Tasks
- task_001_backend_dotnet_scaffolding (US_002) - Requires backend project structure

## Impacted Components
- **NEW** Supabase PostgreSQL 16 instance
- **NEW** pgvector extension (0.5+)
- **MODIFY** src/backend/PatientAccess.Web/appsettings.json - Add placeholder connection string
- **NEW** .env file - Secure connection string storage

## Implementation Plan
1. **Create Supabase Project**: Sign up for Supabase free tier and create new project, wait for PostgreSQL 16 instance provisioning
2. **Enable pgvector Extension**: Connect to Supabase SQL Editor and execute `CREATE EXTENSION IF NOT EXISTS vector;`
3. **Verify Vector Support**: Test vector column creation with 1536 dimensions to confirm pgvector installation
4. **Extract Connection String**: Retrieve connection string from Supabase dashboard (Settings > Database > Connection String)
5. **Configure Connection String Storage**: Store connection string in local `.env` file and use .NET User Secrets for development
6. **Update appsettings.json**: Add placeholder connection string entry with instructions to use environment variable
7. **Document Connection String Format**: Create README section with connection string format and environment variable setup instructions
8. **Test Database Connectivity**: Create simple test script to verify connection from backend project

## Current Project State
```
Propel-Project-Team-12/
├── .propel/
├── src/frontend/ (from US_001)
├── src/backend/
│   ├── PatientAccess.sln
│   ├── PatientAccess.Web/
│   ├── PatientAccess.Business/
│   └── PatientAccess.Data/
└── (Database not provisioned yet)
```

## Expected Changes
| Action | File Path | Description |
|--------|-----------|-------------|
| CREATE | .env | Secure storage for connection string (gitignored) |
| MODIFY | src/backend/PatientAccess.Web/appsettings.json | Add ConnectionStrings section with placeholder |
| CREATE | docs/DATABASE_SETUP.md | Database provisioning and connection instructions |
| CREATE | src/backend/scripts/test-db-connection.sql | SQL script to verify database connectivity and pgvector |

## External References
- Supabase PostgreSQL Setup: https://supabase.com/docs/guides/database/overview
- pgvector Extension Documentation: https://github.com/pgvector/pgvector
- .NET Configuration with Environment Variables: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-8.0
- .NET Secret Manager: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0
- Npgsql Connection Strings: https://www.npgsql.org/doc/connection-string-parameters.html

## Build Commands
```bash
# Test database connection
psql "postgresql://user:password@host:port/database" -c "SELECT version();"

# Enable pgvector extension (run once)
psql "postgresql://user:password@host:port/database" -c "CREATE EXTENSION IF NOT EXISTS vector;"

# Verify pgvector installation
psql "postgresql://user:password@host:port/database" -c "SELECT * FROM pg_extension WHERE extname = 'vector';"

# Test vector column support
psql "postgresql://user:password@host:port/database" -c "CREATE TABLE test_vectors (id SERIAL PRIMARY KEY, embedding vector(1536));"
```

## Implementation Validation Strategy
- [ ] Unit tests pass (N/A for provisioning task)
- [ ] Integration tests pass (N/A for provisioning task)
- [ ] Supabase project created successfully with PostgreSQL 16
- [ ] pgvector extension enabled and queryable via `SELECT * FROM pg_extension WHERE extname = 'vector';`
- [ ] Test vector table with 1536 dimensions created without errors
- [ ] Connection string stored securely in `.env` file (gitignored)
- [ ] Database connection succeeds from backend project using connection string
- [ ] Fail-fast behavior confirmed by attempting connection with invalid credentials

## Implementation Checklist
- [x] Create Supabase account and new project with PostgreSQL 16
- [x] Enable pgvector extension via SQL Editor: `CREATE EXTENSION IF NOT EXISTS vector;`
- [x] Verify pgvector supports 1536-dimensional vectors with test table
- [x] Retrieve connection string from Supabase dashboard
- [x] Add connection string to `.env` file locally (ensure `.env` is gitignored)
- [x] Update `appsettings.json` with placeholder connection string and instructions
- [x] Create `docs/DATABASE_SETUP.md` with provisioning steps and troubleshooting
- [x] Test database connectivity from backend project using `Npgsql` library
