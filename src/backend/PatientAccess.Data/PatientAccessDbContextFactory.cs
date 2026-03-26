using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PatientAccess.Data;

/// <summary>
/// Design-time factory for creating PatientAccessDbContext instances during migrations.
/// Uses a dummy connection string for migration generation only.
/// </summary>
public class PatientAccessDbContextFactory : IDesignTimeDbContextFactory<PatientAccessDbContext>

{

    public PatientAccessDbContext CreateDbContext(string[] args)

    {

        var optionsBuilder = new DbContextOptionsBuilder<PatientAccessDbContext>();

        // Use direct connection (Port 5432) instead of pooler (Port 6543) for migrations

        // Pooler can timeout during long-running migration operations

        optionsBuilder.UseNpgsql(

            "Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.dhbgcoscqujsfycytvns;Password=Maddy@15092001;SSL Mode=Require;Trust Server Certificate=true;Timeout=140;Command Timeout=300;Keepalive=30;",

            npgsqlOptions => npgsqlOptions.CommandTimeout(300)); // 5 minute timeout for migrations
 
        return new PatientAccessDbContext(optionsBuilder.Options);

    }

}
 
