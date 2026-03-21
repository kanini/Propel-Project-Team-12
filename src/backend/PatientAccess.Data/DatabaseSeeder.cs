using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Data.Models;

namespace PatientAccess.Data;

/// <summary>
/// Seeds reference data for insurance providers and healthcare providers with time slots.
/// Environment-guarded: only executes in Development/Staging.
/// Idempotent: checks for existing data before inserting.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(PatientAccessDbContext context, ILogger logger)
    {
        // Use execution strategy to handle retry logic with transactions
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await SeedInsuranceRecordsAsync(context, logger);
                await SeedProvidersAndTimeSlotsAsync(context, logger);
                await transaction.CommitAsync();
                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Database seeding failed. Transaction rolled back.");
                throw;
            }
        });
    }

    private static async Task SeedInsuranceRecordsAsync(PatientAccessDbContext context, ILogger logger)
    {
        if (await context.InsuranceRecords.AnyAsync())
        {
            logger.LogInformation("Insurance records already exist. Skipping seed.");
            return;
        }

        var insuranceRecords = new List<InsuranceRecord>
        {
            new() { ProviderName = "Blue Cross Blue Shield", AcceptedIdPattern = @"^[A-Z]{3}\d{9}$", CoverageType = CoverageType.PPO, IsActive = true },
            new() { ProviderName = "Aetna", AcceptedIdPattern = @"^W\d{8,12}$", CoverageType = CoverageType.HMO, IsActive = true },
            new() { ProviderName = "UnitedHealthcare", AcceptedIdPattern = @"^\d{9,11}$", CoverageType = CoverageType.PPO, IsActive = true },
            new() { ProviderName = "Cigna", AcceptedIdPattern = @"^U\d{8}$", CoverageType = CoverageType.EPO, IsActive = true },
            new() { ProviderName = "Humana", AcceptedIdPattern = @"^H\d{8,10}$", CoverageType = CoverageType.HMO, IsActive = true },
            new() { ProviderName = "Kaiser Permanente", AcceptedIdPattern = @"^\d{10}$", CoverageType = CoverageType.HMO, IsActive = true },
            new() { ProviderName = "Anthem", AcceptedIdPattern = @"^[A-Z]{2}\d{9}$", CoverageType = CoverageType.PPO, IsActive = true },
            new() { ProviderName = "Molina Healthcare", AcceptedIdPattern = @"^\d{9}$", CoverageType = CoverageType.Medicaid, IsActive = true },
            new() { ProviderName = "Medicare Part A", AcceptedIdPattern = @"^\d{1}[A-Z]{1,2}\d{1,2}-?\d{3}-?\d{4}$", CoverageType = CoverageType.Medicare, IsActive = true },
            new() { ProviderName = "Centene", AcceptedIdPattern = @"^C\d{8}$", CoverageType = CoverageType.Medicaid, IsActive = true },
        };

        context.InsuranceRecords.AddRange(insuranceRecords);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} insurance provider records.", insuranceRecords.Count);
    }

    private static async Task SeedProvidersAndTimeSlotsAsync(PatientAccessDbContext context, ILogger logger)
    {
        if (await context.Providers.AnyAsync())
        {
            logger.LogInformation("Providers already exist. Skipping seed.");
            return;
        }

        var providers = new List<Provider>
        {
            new() { Name = "Dr. Sarah Johnson", Specialty = "Family Medicine", Email = "s.johnson@clinic.example", Phone = "555-0101", LicenseNumber = "FM-001234", IsActive = true },
            new() { Name = "Dr. Michael Chen", Specialty = "Cardiology", Email = "m.chen@clinic.example", Phone = "555-0102", LicenseNumber = "CD-005678", IsActive = true },
            new() { Name = "Dr. Emily Rodriguez", Specialty = "Dermatology", Email = "e.rodriguez@clinic.example", Phone = "555-0103", LicenseNumber = "DM-009012", IsActive = true },
            new() { Name = "Dr. James Williams", Specialty = "Orthopedics", Email = "j.williams@clinic.example", Phone = "555-0104", LicenseNumber = "OR-003456", IsActive = true },
            new() { Name = "Dr. Priya Patel", Specialty = "Pediatrics", Email = "p.patel@clinic.example", Phone = "555-0105", LicenseNumber = "PD-007890", IsActive = true },
        };

        context.Providers.AddRange(providers);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} providers.", providers.Count);

        // Generate time slots for next 30 weekdays for each provider
        var timeSlots = new List<TimeSlot>();
        var today = DateTime.UtcNow.Date;

        foreach (var provider in providers)
        {
            var slotDate = today;
            var daysGenerated = 0;

            while (daysGenerated < 30)
            {
                slotDate = slotDate.AddDays(1);

                // Skip weekends
                if (slotDate.DayOfWeek == DayOfWeek.Saturday || slotDate.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                daysGenerated++;

                // Generate 30-minute slots from 8:00 to 17:00 (9 hours = 18 slots/day)
                for (var hour = 8; hour < 17; hour++)
                {
                    for (var minute = 0; minute < 60; minute += 30)
                    {
                        // Skip 12:00-13:00 lunch break
                        if (hour == 12)
                            continue;

                        var startTime = DateTime.SpecifyKind(slotDate.AddHours(hour).AddMinutes(minute), DateTimeKind.Utc);
                        timeSlots.Add(new TimeSlot
                        {
                            ProviderId = provider.ProviderId,
                            StartTime = startTime,
                            EndTime = startTime.AddMinutes(30),
                            IsBooked = false
                        });
                    }
                }
            }
        }

        context.TimeSlots.AddRange(timeSlots);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} time slots across {ProviderCount} providers for next 30 weekdays.", timeSlots.Count, providers.Count);
    }
}
