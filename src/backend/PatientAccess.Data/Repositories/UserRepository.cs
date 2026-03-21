using Microsoft.EntityFrameworkCore;
using PatientAccess.Data.Models;

namespace PatientAccess.Data.Repositories;

/// <summary>
/// Repository implementation for User entity using EF Core (FR-001).
/// Handles data access for user management operations.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly PatientAccessDbContext _context;

    public UserRepository(PatientAccessDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    /// <inheritdoc />
    public async Task<User> CreateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.UserId = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    /// <inheritdoc />
    public async Task<User> UpdateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return user;
    }

    /// <inheritdoc />
    public async Task<User?> GetByVerificationTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return await _context.Users
            .FirstOrDefaultAsync(u => u.VerificationToken == token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<User>> GetAllAsync(string? search = null, string? sortBy = null, bool ascending = true)
    {
        var query = _context.Users.AsNoTracking();

        // Apply search filter to Name or Email if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(searchLower) || u.Email.ToLower().Contains(searchLower));
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => ascending ? query.OrderBy(u => u.Name) : query.OrderByDescending(u => u.Name),
            "email" => ascending ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
            "role" => ascending ? query.OrderBy(u => u.Role) : query.OrderByDescending(u => u.Role),
            "status" => ascending ? query.OrderBy(u => u.Status) : query.OrderByDescending(u => u.Status),
            "createdat" => ascending ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
            _ => query.OrderBy(u => u.CreatedAt) // Default sort by creation date ascending
        };

        return await query.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<int> GetActiveAdminCountAsync()
    {
        return await _context.Users
            .Where(u => u.Role == UserRole.Admin && u.Status == UserStatus.Active)
            .CountAsync();
    }

    /// <inheritdoc />
    public async Task<User> UpdateStatusAsync(Guid userId, UserStatus status)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId)
            ?? throw new InvalidOperationException($"User with ID {userId} not found");

        user.Status = status;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return user;
    }
}
