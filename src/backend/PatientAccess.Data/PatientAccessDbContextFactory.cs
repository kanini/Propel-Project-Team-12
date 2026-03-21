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
        
        // Use a design-time connection string for migration generation
        // This is only used by 'dotnet ef migrations' commands
        optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=postgres");

        return new PatientAccessDbContext(optionsBuilder.Options);
    }
}
