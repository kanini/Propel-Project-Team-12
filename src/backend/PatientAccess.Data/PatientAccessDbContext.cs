using Microsoft.EntityFrameworkCore;
using PatientAccess.Data.Configurations;
using PatientAccess.Data.Models;
using Pgvector.EntityFrameworkCore;

namespace PatientAccess.Data;

/// <summary>
/// Entity Framework Core database context for Patient Access Platform.
/// Manages database connections and entity configurations for PostgreSQL with pgvector support.
/// </summary>
public class PatientAccessDbContext : DbContext
{
    public PatientAccessDbContext(DbContextOptions<PatientAccessDbContext> options)
        : base(options)
    {
    }

    // EP-DATA-I Core Entity DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<ClinicalDocument> ClinicalDocuments => Set<ClinicalDocument>();
    public DbSet<ExtractedClinicalData> ExtractedClinicalData => Set<ExtractedClinicalData>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // EP-DATA-II Extended Entity DbSets
    public DbSet<WaitlistEntry> WaitlistEntries => Set<WaitlistEntry>();
    public DbSet<IntakeRecord> IntakeRecords => Set<IntakeRecord>();
    public DbSet<MedicalCode> MedicalCodes => Set<MedicalCode>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<InsuranceRecord> InsuranceRecords => Set<InsuranceRecord>();
    public DbSet<NoShowHistory> NoShowHistory => Set<NoShowHistory>();

    // RAG Pipeline
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

    // EP-005 US_037 - System configuration
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable pgvector extension for vector similarity search (DR-010)
        modelBuilder.HasPostgresExtension("vector");

        // EP-DATA-I configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new TimeSlotConfiguration());
        modelBuilder.ApplyConfiguration(new ClinicalDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new ExtractedClinicalDataConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

        // EP-DATA-II configurations
        modelBuilder.ApplyConfiguration(new WaitlistEntryConfiguration());
        modelBuilder.ApplyConfiguration(new IntakeRecordConfiguration());
        modelBuilder.ApplyConfiguration(new MedicalCodeConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        modelBuilder.ApplyConfiguration(new InsuranceRecordConfiguration());
        modelBuilder.ApplyConfiguration(new NoShowHistoryConfiguration());

        // RAG Pipeline
        modelBuilder.ApplyConfiguration(new DocumentChunkConfiguration());

        // InMemory provider cannot map pgvector Vector type; ignore it for testing
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            modelBuilder.Entity<DocumentChunk>().Ignore(e => e.Embedding);
        }

        // EP-005 US_037 - System settings configuration
        modelBuilder.ApplyConfiguration(new SystemSettingConfiguration());

        // US_037 - Seed default reminder settings
        SeedSystemSettings(modelBuilder);
    }

    /// <summary>
    /// Seeds default system settings for reminder configuration (US_037).
    /// </summary>
    private void SeedSystemSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>().HasData(
            new SystemSetting
            {
                SystemSettingId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Key = "Reminder.Intervals",
                Value = "[48, 24, 2]",
                Description = "Reminder intervals in hours before appointment (e.g., 48h, 24h, 2h)",
                CreatedAt = new DateTime(2026, 3, 26, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemSetting
            {
                SystemSettingId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Key = "Reminder.SmsEnabled",
                Value = "true",
                Description = "Enable SMS reminders via Twilio",
                CreatedAt = new DateTime(2026, 3, 26, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemSetting
            {
                SystemSettingId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Key = "Reminder.EmailEnabled",
                Value = "true",
                Description = "Enable email reminders via SendGrid",
                CreatedAt = new DateTime(2026, 3, 26, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
