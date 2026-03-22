using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Patient service implementation for US_029 - Walk-in Booking.
/// Implements fast patient search (<300ms) and minimal patient creation.
/// </summary>
public class PatientService : IPatientService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<PatientService> _logger;
    private const int MaxSearchResults = 20;

    public PatientService(
        PatientAccessDbContext context,
        ILogger<PatientService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Searches for patients by name, email, or phone (US_029, AC-1 / US_032, AC-1).
    /// Optimized for <300ms response time with indexed queries and relevance sorting.
    /// Supports special characters (accents, hyphens, apostrophes) in search.
    /// </summary>
    public async Task<List<PatientSearchResultDto>> SearchPatientsAsync(string query)
    {
        try
        {
            // Return empty list for invalid queries
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            {
                _logger.LogDebug("Search query too short or empty: {Query}", query);
                return new List<PatientSearchResultDto>();
            }

            _logger.LogInformation("Searching patients with query: {Query}", query);

            // Sanitize input - trim whitespace and normalize
            var searchTerm = query.Trim();
            var searchTermLower = searchTerm.ToLower();

            // Query patients with indexed fields (Name, Email, Phone)
            // Filter by Patient role and Active status
            // Implement relevance-based sorting:
            // 1. Exact email match (highest priority)
            // 2. Name starts with search term (high priority)
            // 3. Contains match in name, email, or phone (medium priority)
            var patients = await _context.Users
                .AsNoTracking() // Read-only performance optimization
                .Where(u => u.Role == UserRole.Patient &&
                           u.Status == UserStatus.Active &&
                           (u.Name.ToLower().Contains(searchTermLower) ||
                            (u.Email != null && u.Email.ToLower().Contains(searchTermLower)) ||
                            (u.Phone != null && u.Phone.Contains(searchTerm))))
                .Select(u => new
                {
                    u.UserId,
                    u.Name,
                    u.DateOfBirth,
                    u.Email,
                    u.Phone,
                    LastAppointment = u.Appointments
                        .Where(a => a.Status != AppointmentStatus.Cancelled)
                        .OrderByDescending(a => a.ScheduledDateTime)
                        .Select(a => a.ScheduledDateTime)
                        .FirstOrDefault(),
                    // Relevance scoring for sorting
                    IsExactEmailMatch = u.Email != null && u.Email.ToLower() == searchTermLower,
                    IsNameStartsWithMatch = u.Name.ToLower().StartsWith(searchTermLower)
                })
                .ToListAsync();

            // Sort by relevance: exact email > name starts-with > last appointment date > name contains
            var sortedPatients = patients
                .OrderByDescending(p => p.IsExactEmailMatch) // Exact email match first
                .ThenByDescending(p => p.IsNameStartsWithMatch) // Then name starts-with
                .ThenByDescending(p => p.LastAppointment) // Then most recent patients
                .ThenBy(p => p.Name) // Finally alphabetical
                .Take(MaxSearchResults) // Limit to 20 results
                .ToList();

            var results = sortedPatients.Select(p => new PatientSearchResultDto
            {
                Id = p.UserId,
                FullName = p.Name,
                DateOfBirth = p.DateOfBirth.HasValue ? p.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty,
                Email = p.Email,
                Phone = p.Phone,
                LastAppointmentDate = p.LastAppointment != default(DateTime)
                    ? p.LastAppointment.ToString("yyyy-MM-dd")
                    : null
            }).ToList();

            _logger.LogInformation("Found {Count} patients matching query: {Query}", results.Count, query);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients with query: {Query}", query);
            throw;
        }
    }

    /// <summary>
    /// Creates a minimal patient record for walk-in registration (US_029, AC-2).
    /// Handles duplicate email scenario by returning existing patient.
    /// </summary>
    public async Task<PatientSearchResultDto> CreateMinimalPatientAsync(CreateMinimalPatientDto dto)
    {
        try
        {
            _logger.LogInformation(
                "Creating minimal patient: {FirstName} {LastName}, DOB: {DateOfBirth}",
                dto.FirstName, dto.LastName, dto.DateOfBirth);

            // Check if email already exists (if email provided)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingUser = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Email.ToLower() == dto.Email.ToLower() &&
                               u.Role == UserRole.Patient)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    _logger.LogInformation(
                        "Patient with email {Email} already exists. Returning existing patient: {UserId}",
                        dto.Email, existingUser.UserId);

                    // Return existing patient as search result
                    return new PatientSearchResultDto
                    {
                        Id = existingUser.UserId,
                        FullName = existingUser.Name,
                        DateOfBirth = existingUser.DateOfBirth.HasValue
                            ? existingUser.DateOfBirth.Value.ToString("yyyy-MM-dd")
                            : string.Empty,
                        Email = existingUser.Email,
                        Phone = existingUser.Phone,
                        LastAppointmentDate = null // Not fetched for existing patient
                    };
                }
            }

            // Parse date of birth
            if (!DateOnly.TryParse(dto.DateOfBirth, out var dateOfBirth))
            {
                throw new ArgumentException("Invalid date of birth format. Expected YYYY-MM-DD.");
            }

            // Create new user entity for walk-in patient
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = dto.Email ?? string.Empty,
                Name = $"{dto.FirstName} {dto.LastName}",
                DateOfBirth = dateOfBirth,
                Phone = dto.Phone,
                PasswordHash = string.Empty, // Patient sets password during full registration later
                Role = UserRole.Patient,
                Status = UserStatus.Active, // Active status allows immediate appointment booking
                CreatedAt = DateTime.UtcNow
            };

            // Save to database using transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Successfully created minimal patient: {UserId}, Name: {Name}",
                    newUser.UserId, newUser.Name);

                return new PatientSearchResultDto
                {
                    Id = newUser.UserId,
                    FullName = newUser.Name,
                    DateOfBirth = newUser.DateOfBirth.HasValue
                        ? newUser.DateOfBirth.Value.ToString("yyyy-MM-dd")
                        : string.Empty,
                    Email = newUser.Email,
                    Phone = newUser.Phone,
                    LastAppointmentDate = null // New patient has no appointments
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating minimal patient: {FirstName} {LastName}",
                dto.FirstName, dto.LastName);
            throw;
        }
    }
}
