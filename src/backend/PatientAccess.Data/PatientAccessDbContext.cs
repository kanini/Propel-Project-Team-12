using Microsoft.EntityFrameworkCore;

namespace PatientAccess.Data;

/// <summary>
/// Entity Framework Core database context for Patient Access Platform.
/// Manages database connections and entity configurations for PostgreSQL with pgvector support.
/// </summary>
public class PatientAccessDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PatientAccessDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public PatientAccessDbContext(DbContextOptions<PatientAccessDbContext> options) 
        : base(options)
    {
    }

    /// <summary>
    /// Configures the database model and entity relationships.
    /// Override this method to configure conventions, entity mappings, and relationships.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity configurations will be added here as entities are created
        // Example: modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        
        // Configure schema if needed
        // modelBuilder.HasDefaultSchema("public");
        
        // Configure pgvector extension (if needed for vector columns)
        // modelBuilder.HasPostgresExtension("vector");
    }

    /// <summary>
    /// Configures database context options such as logging and query behavior.
    /// </summary>
    /// <param name="optionsBuilder">The builder being used to configure the context options.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Enable detailed errors in development
        // This will be conditionally enabled based on environment in the DI registration
    }
}
