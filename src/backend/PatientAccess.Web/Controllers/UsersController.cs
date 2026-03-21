using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Services;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Admin-only controller for user management operations (US_021, FR-004).
/// Provides CRUD operations for Staff and Admin user accounts with RBAC enforcement.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")] // US_021: Only Admins can manage users
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all Staff and Admin users with optional filtering and sorting (US_021 AC4).
    /// </summary>
    /// <param name="search">Optional search term for name or email</param>
    /// <param name="sortBy">Optional column name for sorting (Name, Email, Role, Status, CreatedAt)</param>
    /// <param name="ascending">Sort direction (default: true)</param>
    /// <returns>List of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers(
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = true)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(search, sortBy, ascending);

            _logger.LogInformation("Admin {AdminId} retrieved user list. Search: {Search}, SortBy: {SortBy}, Count: {Count}",
                GetCurrentUserId(), search ?? "none", sortBy ?? "default", users.Count());

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user list");
            return StatusCode(500, new { message = "An error occurred while retrieving users." });
        }
    }

    /// <summary>
    /// Gets count of active Admin users (US_021 Edge Case validation).
    /// Used by frontend to validate last Admin deletion prevention.
    /// </summary>
    /// <returns>Number of active Admins</returns>
    [HttpGet("admin-count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<int>> GetActiveAdminCount()
    {
        try
        {
            var count = await _userService.GetActiveAdminCountAsync();

            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active Admin count");
            return StatusCode(500, new { message = "An error occurred while retrieving Admin count." });
        }
    }

    /// <summary>
    /// Creates a new Staff or Admin user account (US_021 AC1).
    /// Generates random password and sends activation email.
    /// </summary>
    /// <param name="request">Create user request with name, email, and role</param>
    /// <returns>Created user DTO</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var adminUserId = GetCurrentUserId();
            var createdUser = await _userService.CreateUserAsync(request, adminUserId);

            _logger.LogInformation("Admin {AdminId} created {Role} user: {UserId}, Email: {Email}",
                adminUserId, request.Role, createdUser.UserId, request.Email);

            return CreatedAtAction(nameof(GetAllUsers), new { id = createdUser.UserId }, createdUser);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
        {
            _logger.LogWarning("Admin {AdminId} attempted to create user with duplicate email: {Email}",
                GetCurrentUserId(), request.Email);
            return Conflict(new { message = "Email address is already registered." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, new { message = "An error occurred while creating the user." });
        }
    }

    /// <summary>
    /// Updates existing user's name and/or role (US_021 AC2).
    /// Creates audit log entry for the modification.
    /// </summary>
    /// <param name="id">User ID to update</param>
    /// <param name="request">Update request with new name and role</param>
    /// <returns>Updated user DTO</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var adminUserId = GetCurrentUserId();
            var updatedUser = await _userService.UpdateUserAsync(id, request, adminUserId);

            _logger.LogInformation("Admin {AdminId} updated user {UserId}: Name={Name}, Role={Role}",
                adminUserId, id, request.Name, request.Role);

            return Ok(updatedUser);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = $"User with ID {id} not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the user." });
        }
    }

    /// <summary>
    /// Deactivates user account (soft delete) (US_021 AC3).
    /// Prevents self-deactivation and last Admin deletion.
    /// Terminates all active sessions for the user.
    /// </summary>
    /// <param name="id">User ID to deactivate</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await _userService.DeactivateUserAsync(id, currentUserId);

            _logger.LogInformation("Admin {AdminId} deactivated user {UserId}", currentUserId, id);

            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("own account"))
        {
            _logger.LogWarning("Admin {AdminId} attempted to deactivate their own account", GetCurrentUserId());
            return BadRequest(new { message = "Cannot deactivate your own account." });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("last active Admin"))
        {
            _logger.LogWarning("Admin {AdminId} attempted to deactivate last Admin account: {UserId}",
                GetCurrentUserId(), id);
            return BadRequest(new { message = "Cannot deactivate the last active Admin account." });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = $"User with ID {id} not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while deactivating the user." });
        }
    }

    /// <summary>
    /// Extracts current user ID from JWT claims.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogError("Failed to extract user ID from JWT claims");
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }
}
