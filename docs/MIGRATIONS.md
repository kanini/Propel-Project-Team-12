# Entity Framework Core Migrations Guide

## Overview

This guide provides instructions for managing database schema migrations using Entity Framework Core with PostgreSQL (Supabase). Migrations allow you to version-control your database schema and apply changes consistently across development, staging, and production environments.

## Prerequisites

- .NET 8 SDK installed
- EF Core CLI tools installed globally
- Valid database connection string configured
- PostgreSQL database instance running (Supabase)

## Initial Setup

### Install EF Core CLI Tools (One-time)

```powershell
dotnet tool install --global dotnet-ef
```

### Verify Installation

```powershell
dotnet ef --version
```

Expected output: `Entity Framework Core .NET Command-line Tools 8.x.x`

## Migration Workflows

### Creating a Migration

When you add or modify entity classes in the `PatientAccess.Data` project, you need to create a migration to reflect those changes in the database schema.

#### Command

```powershell
# Navigate to backend directory
cd src\backend

# Add new migration
dotnet ef migrations add <MigrationName> `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

#### Example

```powershell
dotnet ef migrations add AddUserTable `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

#### Naming Conventions

- Use **PascalCase** for migration names
- Use descriptive, action-oriented names:
  - `AddUserTable`
  - `UpdatePatientColumns`
  - `CreateIndexOnEmail`
  - `RemoveObsoleteFields`
- Include the entity or feature being changed

### Applying Migrations to Database

After creating a migration, apply it to update the database schema.

#### Command

```powershell
# Apply all pending migrations
dotnet ef database update `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

#### Apply Specific Migration

```powershell
# Upda to a specific migration (downgrade or upgrade)
dotnet ef database update <MigrationName> `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

#### Example: Rollback to Previous Migration

```powershell
dotnet ef database update AddUserTable `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

### Listing Migrations

View all migrations and their application status.

#### Command

```powershell
dotnet ef migrations list `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

#### Output Example

```
InitialCreate (Applied)
AddUserTable (Applied)
UpdatePatientColumns (Pending)
```

### Removing Last Migration

If you created a migration by mistake and **haven't applied it** yet, you can remove it.

#### Command

```powershell
dotnet ef migrations remove `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

⚠️ **Warning**: This only works if the migration hasn't been applied to the database.

### Generating SQL Scripts

Generate SQL scripts from migrations without applying them to the database. Useful for review or manual deployment.

#### Command

```powershell
# Generate SQL for all migrations
dotnet ef migrations script `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web `
    --output migrations.sql
```

#### Generate SQL for Specific Range

```powershell
# From one migration to another
dotnet ef migrations script <FromMigration> <ToMigration> `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web `
    --output migration-range.sql
```

#### Idempotent Scripts (Production-Safe)

```powershell
# Generate script with IF NOT EXISTS checks
dotnet ef migrations script `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web `
    --idempotent `
    --output idempotent-migrations.sql
```

### Reverting a Migration

To undo a migration that's already been applied to the database, update to a previous migration.

#### Command

```powershell
# Revert to specific migration
dotnet ef database update <PreviousMigrationName> `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

#### Example: Revert to InitialCreate

```powershell
dotnet ef database update InitialCreate `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

Then remove the unwanted migration files:

```powershell
dotnet ef migrations remove `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

### Resetting Database (Development Only)

⚠️ **CAUTION**: This deletes all data!

```powershell
# Drop entire database
dotnet ef database drop `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web `
    --force

# Recreate and apply all migrations
dotnet ef database update `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

## Best Practices

### Migration Development

1. **Test migrations locally** before committing
2. **Review generated SQL** using `migrations script` command
3. **One migration per logical change** (e.g., add table, modify column, create index)
4. **Never edit applied migrations** - create new migrations instead
5. **Include migration files in version control** (`Migrations/` folder)

### Naming and Organization

- Use clear, descriptive migration names
- Group related changes in a single migration
- Avoid mixing schema changes with data changes
- Document complex migrations with code comments

### Databconnectivity

- **Verify connection** before running migrations
- **Test rollback** procedures in development
- **Use transactions** (EF Core does this by default)
- **Backup production data** before applying migrations

### Team Collaboration

- **Pull latest migrations** before creating new ones
- **Resolve migration conflicts** immediately
- **Communicate breaking changes** with the team
- **Document manual steps** required alongside migrations

## Troubleshooting

### Common Errors

#### "Unable to connect to database"

**Cause**: Database connection string is incorrect or database is unreachable.

**Solution**:
1. Verify connection string in `appsettings.json`
2. Test database connectivity: See [docs/DATABASE_SETUP.md](DATABASE_SETUP.md)
3. Ensure Supabase project is running
4. Check firewall settings

#### "Migrations assembly not found"

**Cause**: EF Core Design package missing or startup project doesn't reference data project.

**Solution**:
1. Install `Microsoft.EntityFrameworkCore.Design` in startup project
2. Add project reference: `PatientAccess.Web` → `PatientAccess.Data`

#### "Build failed during migration"

**Cause**: Compilation errors in code.

**Solution**:
1. Fix all compilation errors first
2. Run `dotnet build` to verify
3. Retry migration command

#### "Migration already applied"

**Cause**: Trying to apply a migration that's already in the database.

**Solution**:
- Check migration status: `dotnet ef migrations list`
- Create a new migration if changes are needed

### Permission Issues

If you encounter permission errors:

1. Verify database user has schema modification privileges
2. Check Supabase user permissions
3. Ensure connection string uses correct credentials

### Schema Conflicts

If migration fails due to existing objects:

1. Review migration SQL: `dotnet ef migrations script`
2. Manually resolve conflicts in database
3. Consider creating a cleanup migration

## Production Deployment

### Recommended Approach

1. **Generate idempotent script**:
   ```powershell
   dotnet ef migrations script --idempotent --output production-migration.sql
   ```

2. **Review SQL carefully** for correctness

3. **Test on staging environment** first

4. **Backup production database**

5. **Apply during maintenance window**

6. **Monitor application** after deployment

### Alternative: Automatic Migration on Startup

⚠️ **Not recommended for production** due to potential data loss and downtime.

Add to `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PatientAccessDbContext>();
    dbContext.Database.Migrate(); // Automatically apply pending migrations
}
```

## Advanced Scenarios

### Multi-Environment Migrations

Use different connection strings per environment:

```json
{
  "ConnectionStrings": {
    "Development": "Host=localhost;...",
    "Staging": "Host=staging.supabase.co;...",
    "Production": "Host=production.supabase.co;..."
  }
}
```

Override at runtime:

```powershell
dotnet ef database update `
    --connection "Host=production.supabase.co;..." `
    --project PatientAccess.Data `
    --startup-project PatientAccess.Web
```

### Data Seeding

Add seed data in migration:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.InsertData(
        table: "Users",
        columns: new[] { "Id", "Email", "Name" },
        values: new object[] { 1, "admin@example.com", "Admin" }
    );
}
```

### Custom SQL in Migrations

Execute raw SQL:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
        CREATE INDEX CONCURRENTLY idx_user_email 
        ON users (email);
    ");
}
```

## Migration File Structure

Migrations are stored in `PatientAccess.Data/Migrations/`:

```
Migrations/
├── 20260320120000_InitialCreate.cs
├── 20260320120000_InitialCreate.Designer.cs
├── PatientAccessDbContextModelSnapshot.cs
```

- `*.cs` - Migration class with `Up()` and `Down()` methods
- `*.Designer.cs` - Migration metadata
- `*ModelSnapshot.cs` - Current model state (do not edit manually)

## Additional Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core CLI Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [Npgsql EF Core Provider](https://www.npgsql.org/efcore/)
- [Database Setup Guide](DATABASE_SETUP.md)

## Quick Reference

| Task | Command |
|------|---------|
| Create migration | `dotnet ef migrations add <Name>` |
| Apply migrations | `dotnet ef database update` |
| List migrations | `dotnet ef migrations list` |
| Remove last migration | `dotnet ef migrations remove` |
| Generate SQL script | `dotnet ef migrations script` |
| Rollback to migration | `dotnet ef database update <Name>` |
| Drop database | `dotnet ef database drop --force` |

**Always run commands from `src/backend` directory with `--project PatientAccess.Data --startup-project PatientAccess.Web`.**
