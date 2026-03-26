using Microsoft.EntityFrameworkCore;
using PatientAccess.Data.Configurations;
using PatientAccess.Data.Models;

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

    // EP-005 System Configuration DbSets
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    // Calendar Integration DbSets (US_039)
    public DbSet<CalendarIntegration> CalendarIntegrations => Set<CalendarIntegration>();

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

        // EP-005 System Configuration
        modelBuilder.ApplyConfiguration(new SystemSettingConfiguration());

        // Calendar Integration (US_039)
        modelBuilder.ApplyConfiguration(new CalendarIntegrationConfiguration());

        // Seed default reminder system settings (US_037)
        SeedReminderSystemSettings(modelBuilder);
    }

    /// <summary>
    /// Seeds default reminder system configuration (US_037).
    /// Reminder intervals: 48h, 24h, 2h before appointment.
    /// Both SMS and Email channels enabled by default.
    /// </summary>
    private void SeedReminderSystemSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>().HasData(
            new SystemSetting
            {
                SystemSettingId = Guid.Parse("00000000-0000-0000-0001-000000000001"),
                Key = "Reminder.Intervals",
                Value = "[48, 24, 2]",
                Description = "Reminder intervals in hours before appointment. JSON array format.",
                CreatedAt = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemSetting
            {
                SystemSettingId = Guid.Parse("00000000-0000-0000-0001-000000000002"),
                Key = "Reminder.SmsEnabled",
                Value = "true",
                Description = "Enable SMS reminder notifications via Twilio.",
                CreatedAt = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc)
            },
            new SystemSetting
            {
                SystemSettingId = Guid.Parse("00000000-0000-0000-0001-000000000003"),
                Key = "Reminder.EmailEnabled",
                Value = "true",
                Description = "Enable Email reminder notifications via SendGrid.",
                CreatedAt = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
