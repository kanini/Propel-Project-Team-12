using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;

namespace PatientAccess.Business.Services;

/// <summary>
/// Provider service implementation for US_023 - Provider Browser (FR-006).
/// Implements efficient database queries with 300ms P95 response time target (NFR-001).
/// </summary>
public class ProviderService : IProviderService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<ProviderService> _logger;

    public ProviderService(PatientAccessDbContext context, ILogger<ProviderService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves paginated list of providers with filtering and search (FR-006, AC1, AC2, AC3).
    /// Performance optimized with AsNoTracking() and selective field projection.
    /// </summary>
    public async Task<ProviderListResponseDto> GetProvidersAsync(
        string? search = null,
        string? specialty = null,
        string? availability = null,
        string? gender = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            _logger.LogInformation(
                "Fetching providers: search={Search}, specialty={Specialty}, availability={Availability}, gender={Gender}, page={Page}, pageSize={PageSize}",
                search, specialty, availability, gender, page, pageSize);

            // Start with base query - only active providers (FR-006, AC1)
            var query = _context.Providers
                .AsNoTracking() // Read-only query performance optimization
                .Where(p => p.IsActive);

            // Apply search filter (FR-006, AC3) - case-insensitive search on Name and Specialty
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    p.Specialty.ToLower().Contains(searchLower));
            }

            // Apply specialty filter (FR-006, AC2)
            if (!string.IsNullOrWhiteSpace(specialty) && specialty != "all")
            {
                // Convert kebab-case to proper case (e.g., "family-medicine" -> "Family Medicine")
                var specialtyFormatted = FormatSpecialty(specialty);
                query = query.Where(p => p.Specialty == specialtyFormatted);
            }

            // Apply availability filter (FR-006, AC2)
            if (!string.IsNullOrWhiteSpace(availability) && availability != "any-time")
            {
                var now = DateTime.UtcNow;
                DateTime startDate = availability switch
                {
                    "today" => now.Date,
                    "this-week" => now.Date,
                    "this-month" => new DateTime(now.Year, now.Month, 1),
                    _ => now
                };

                DateTime endDate = availability switch
                {
                    "today" => now.Date.AddDays(1),
                    "this-week" => now.Date.AddDays(7),
                    "this-month" => new DateTime(now.Year, now.Month, 1).AddMonths(1),
                    _ => now.AddYears(1)
                };

                // Filter providers who have at least one available slot in date range
                query = query.Where(p => p.TimeSlots.Any(ts =>
                    !ts.IsBooked &&
                    ts.StartTime >= startDate &&
                    ts.StartTime < endDate));
            }

            // Apply gender filter (FR-006, AC2)
            // Note: Gender field not in current Provider model - this is placeholder for future enhancement
            // Remove or implement once Gender field is added to Provider entity
            // if (!string.IsNullOrWhiteSpace(gender) && gender != "any")
            // {
            //     query = query.Where(p => p.Gender == gender);
            // }

            // Get total count before pagination (for TotalPages calculation)
            var totalCount = await query.CountAsync();

            // Apply pagination (Edge Case: 20 providers per page)
            var providers = await query
                .OrderBy(p => p.Name) // Consistent ordering
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.ProviderId,
                    p.Name,
                    p.Specialty,
                    p.Email,
                    p.Phone,
                    // Calculate next available slot using subquery
                    NextAvailableSlot = p.TimeSlots
                        .Where(ts => !ts.IsBooked && ts.StartTime > DateTime.UtcNow)
                        .OrderBy(ts => ts.StartTime)
                        .Select(ts => (DateTime?)ts.StartTime)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Map to DTOs
            var providerDtos = providers.Select(p => new ProviderDto
            {
                Id = p.ProviderId,
                Name = p.Name,
                Specialty = p.Specialty,
                Rating = 4.5m, // TODO: Calculate from appointment reviews (future enhancement)
                ReviewCount = 100, // TODO: Count from appointment reviews (future enhancement)
                NextAvailableSlot = p.NextAvailableSlot,
                AvatarUrl = null, // TODO: Implement avatar storage (future enhancement)
                Gender = null, // TODO: Add Gender field to Provider entity
                Location = null // TODO: Add Location field to Provider entity
            }).ToList();

            // Calculate total pages (Edge Case requirement)
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            _logger.LogInformation(
                "Retrieved {Count} providers (page {Page}/{TotalPages})",
                providerDtos.Count, page, totalPages);

            return new ProviderListResponseDto
            {
                Providers = providerDtos,
                Total = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving providers");
            throw; // Re-throw to be handled by controller
        }
    }

    /// <summary>
    /// Convert kebab-case specialty to proper case for database matching.
    /// Example: "family-medicine" -> "Family Medicine"
    /// </summary>
    private static string FormatSpecialty(string specialty)
    {
        return string.Join(" ", specialty.Split('-')
            .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }
}
