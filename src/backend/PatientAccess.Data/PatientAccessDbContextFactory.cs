using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PatientAccess.Data;

/// <summary>
/// Design-time factory for creating PatientAccessDbContext instances during migrations.
/// Reads DB_PASSWORD from environment variables for secure connection.
/// </summary>
public class PatientAccessDbContextFactory : IDesignTimeDbContextFactory<PatientAccessDbContext>
{
    public PatientAccessDbContext CreateDbContext(string[] args)
    {
        // Load .env file if it exists (for local development)
        LoadEnvironmentFromFile();

        var optionsBuilder = new DbContextOptionsBuilder<PatientAccessDbContext>();

        // Read DB_PASSWORD from environment variable
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "SET_VIA_ENV";
        
        // Build connection string with actual password
        var connectionString = $"Host=aws-1-ap-northeast-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.dhbgcoscqujsfycytvns;Password={dbPassword};SSL Mode=Require;Trust Server Certificate=true;Timeout=140;Command Timeout=300;Keepalive=30;";

        // Use direct connection (Port 5432) instead of pooler (Port 6543) for migrations
        // Pooler can timeout during long-running migration operations
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.CommandTimeout(300)); // 5 minute timeout for migrations
 
        return new PatientAccessDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Loads environment variables from .env or env file in the Web project directory.
    /// This allows migrations to access secrets without hardcoding them.
    /// </summary>
    private void LoadEnvironmentFromFile()
    {
        // Try to find env file (navigate from Data project to Web project)
        var currentDir = Directory.GetCurrentDirectory();
        var webProjectDir = Path.Combine(currentDir, "..", "PatientAccess.Web");
        
        // Try .env first, then env
        var envPaths = new[] 
        { 
            Path.Combine(webProjectDir, ".env"),
            Path.Combine(webProjectDir, "env")
        };

        foreach (var envPath in envPaths)
        {
            if (File.Exists(envPath))
            {
                foreach (var line in File.ReadAllLines(envPath))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
                    var idx = trimmed.IndexOf('=');
                    if (idx <= 0) continue;
                    var key = trimmed[..idx].Trim();
                    var value = trimmed[(idx + 1)..].Trim();
                    Environment.SetEnvironmentVariable(key, value);
                }
                break; // Stop after loading first found file
            }
        }
    }
}
 
