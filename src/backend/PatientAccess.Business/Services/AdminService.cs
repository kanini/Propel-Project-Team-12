using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using System.Text.Json;

namespace PatientAccess.Business.Services;

/// <summary>
/// Admin service for user management operations (US_021).
/// </summary>
public class AdminService : IAdminService
{
    private readonly PatientAccessDbContext _dbContext;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IEmailService _emailService;
    private readonly ISessionCacheService? _sessionCacheService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        PatientAccessDbContext dbContext,
        IPasswordHashingService passwordHashingService,
        IEmailService emailService,
        IAuditLogService auditLogService,
        ILogger<AdminService> logger,
        ISessionCacheService? sessionCacheService = null)
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
        _emailService = emailService;
        _auditLogService = auditLogService;
        _sessionCacheService = sessionCacheService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new user account (US_021, AC1).
    /// Validates email uniqueness, creates user with Pending status, sends activation email.
    /// </summary>
    public async Task<UserDto> CreateUserAsync(
        CreateUserRequestDto request,
        string? ipAddress = null,
        string? userAgent = null)
    {
        // Check for duplicate email (Edge case)
        var emailExists = await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (emailExists)
        {
            throw new InvalidOperationException($"Email address {request.Email} is already registered.");
        }

        // Hash password
        var passwordHash = _passwordHashingService.HashPassword(request.Password);

        // Generate verification token
        var verificationToken = GenerateVerificationToken();

        // Create user entity
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = request.Email.ToLower().Trim(),
            Name = request.Name.Trim(),
            Phone = request.Phone?.Trim(),
            PasswordHash = passwordHash,
            Role = request.Role,
            Status = UserStatus.Pending, // Requires email verification
            VerificationToken = verificationToken,
            VerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Admin created user: {Email}, Role: {Role}, UserId: {UserId}",
            user.Email, user.Role, user.UserId);

        // Log user creation to audit log (US_021, AC2)
        await _auditLogService.LogAuthEventAsync(
            userId: user.UserId,
            actionType: AuditActionType.Registration,
            ipAddress: ipAddress,
            userAgent: userAgent,
            metadata: JsonSerializer.Serialize(new
            {
                email = user.Email,
                role = user.Role.ToString(),
                createdBy = "Admin",
                status = user.Status.ToString()
            }));

        // Send activation email (AC1)
        await _emailService.SendVerificationEmailAsync(user.Email, user.Name, verificationToken);

        return MapToUserDto(user);
    }

    /// <summary>
    /// Updates user details (US_021, AC2).
    /// Creates audit log for changes.
    /// </summary>
    public async Task<UserDto> UpdateUserAsync(
        Guid userId,
        UpdateUserRequestDto request,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var user = await _dbContext.Users.FindAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        // Track changes for audit log
        var changes = new Dictionary<string, object?>();

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            changes["name"] = new { old = user.Name, @new = request.Name };
            user.Name = request.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            // Check for duplicate email
            var emailExists = await _dbContext.Users
                .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.UserId != userId);

            if (emailExists)
            {
                throw new InvalidOperationException($"Email address {request.Email} is already in use.");
            }

            changes["email"] = new { old = user.Email, @new = request.Email };
            user.Email = request.Email.ToLower().Trim();
        }

        if (request.Role.HasValue)
        {
            changes["role"] = new { old = user.Role.ToString(), @new = request.Role.ToString() };
            user.Role = request.Role.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            changes["phone"] = new { old = user.Phone, @new = request.Phone };
            user.Phone = request.Phone.Trim();
        }

        if (request.Status.HasValue)
        {
            changes["status"] = new { old = user.Status.ToString(), @new = request.Status.ToString() };
            user.Status = request.Status.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User updated: UserId: {UserId}, Changes: {Changes}", userId, changes.Count);

        // Log user update to audit log (US_021, AC2)
        await _auditLogService.LogAuthEventAsync(
            userId: userId,
            actionType: AuditActionType.PasswordChanged, // Reusing for user changes
            ipAddress: ipAddress,
            userAgent: userAgent,
            metadata: JsonSerializer.Serialize(new
            {
                changes,
                updatedBy = "Admin",
                timestamp = DateTime.UtcNow
            }));

        return MapToUserDto(user);
    }

    /// <summary>
    /// Deactivates user account (US_021, AC3).
    /// Prevents self-deactivation (AC5) and last admin deactivation (Edge case).
    /// </summary>
    public async Task DeactivateUserAsync(
        Guid userId,
        Guid currentUserId,
        string? ipAddress = null,
        string? userAgent = null)
    {
        // Prevent self-deactivation (US_021, AC5)
        if (userId == currentUserId)
        {
            throw new InvalidOperationException("Cannot deactivate your own account.");
        }

        var user = await _dbContext.Users.FindAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        // Prevent deactivation of last Admin (Edge case)
        if (user.Role == UserRole.Admin)
        {
            var activeAdminCount = await _dbContext.Users
                .CountAsync(u => u.Role == UserRole.Admin && u.Status == UserStatus.Active);

            if (activeAdminCount <= 1)
            {
                throw new InvalidOperationException("Cannot deactivate the last active Admin account.");
            }
        }

        // Set status to Inactive (AC3)
        user.Status = UserStatus.Inactive;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User deactivated: UserId: {UserId}, Role: {Role}", userId, user.Role);

        // Terminate active sessions (AC3)
        if (_sessionCacheService != null)
        {
            try
            {
                await _sessionCacheService.RemoveSessionAsync(userId.ToString());
                _logger.LogInformation("Terminated session for deactivated user: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to terminate session for user {UserId}", userId);
            }
        }

        // Log account deactivation to audit log (US_021, AC3)
        await _auditLogService.LogAuthEventAsync(
            userId: userId,
            actionType: AuditActionType.AccountDeactivated,
            ipAddress: ipAddress,
            userAgent: userAgent,
            metadata: JsonSerializer.Serialize(new
            {
                deactivatedBy = currentUserId,
                previousStatus = UserStatus.Active.ToString(),
                reason = "Admin deactivation",
                timestamp = DateTime.UtcNow
            }));
    }

    /// <summary>
    /// Gets all users with optional filtering.
    /// </summary>
    public async Task<List<UserDto>> GetAllUsersAsync(
        string? searchTerm = null,
        string? role = null,
        string? status = null)
    {
        var query = _dbContext.Users.AsQueryable();

        // Filter by search term (name or email)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(searchLower) ||
                u.Email.ToLower().Contains(searchLower));
        }

        // Filter by role
        if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse<UserRole>(role, true, out var roleEnum))
        {
            query = query.Where(u => u.Role == roleEnum);
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<UserStatus>(status, true, out var statusEnum))
        {
            query = query.Where(u => u.Status == statusEnum);
        }

        // Sort by name
        var users = await query
            .OrderBy(u => u.Name)
            .ToListAsync();

        return users.Select(MapToUserDto).ToList();
    }

    /// <summary>
    /// Gets a single user by ID.
    /// </summary>
    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found.");
        }

        return MapToUserDto(user);
    }

    /// <summary>
    /// Maps User entity to UserDto.
    /// </summary>
    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    /// <summary>
    /// Generates cryptographically secure random verification token.
    /// </summary>
    private static string GenerateVerificationToken()
    {
        var tokenBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }
}
