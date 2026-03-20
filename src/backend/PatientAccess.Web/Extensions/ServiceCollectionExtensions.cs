using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatientAccess.Data;

namespace PatientAccess.Web.Extensions;

/// <summary>
/// Extension methods for configuring data access services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Entity Framework Core DbContext and related data access services to the service collection.
    /// Configures PostgreSQL connection with Npgsql provider and pgvector support.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration containing connection strings.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get connection string from configuration
        // Priority: appsettings.json > User Secrets > Environment Variables
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Database connection string 'DefaultConnection' not found. " +
                "Please configure it in appsettings.json, User Secrets, or environment variables. " +
                "See docs/DATABASE_SETUP.md for detailed instructions.");

        // Register DbContext with Npgsql provider
        services.AddDbContext<PatientAccessDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Enable retry on failure for transient errors
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);

                // Set command timeout (30 seconds default)
                npgsqlOptions.CommandTimeout(30);

                // Specify migrations assembly (important for multi-project solutions)
                npgsqlOptions.MigrationsAssembly("PatientAccess.Data");
            });

            // Enable detailed errors in development environment
            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            // Configure query behavior
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Note: Database health check is configured in Program.cs with AddNpgSql (includes timeout)
        // See: US_005 Task 002 - Health check endpoint with 5-second timeout

        return services;
    }
}
