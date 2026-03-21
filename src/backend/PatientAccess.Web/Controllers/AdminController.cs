using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Admin-only controller for administrative operations (US_020, US_021).
/// All endpoints require Admin role (AC2).
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")] // All endpoints require Admin role
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdminService adminService,
        ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint for admin panel.
    /// </summary>
    /// <returns>Admin panel status</returns>
    /// <response code="200">Admin access confirmed</response>
    /// <response code="403">Insufficient permissions - Admin role required</response>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetAdminHealth()
    {
        _logger.LogInformation("Admin health check accessed");

        return Ok(new
        {
            status = "healthy",
            message = "Admin access confirmed",
            role = User.FindFirst(ClaimTypes.Role)?.Value,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets all users with optional filtering (US_021).
    /// </summary>
    /// <param name="searchTerm">Optional search term for name/email</param>
    /// <param name="role">Optional role filter</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of users</returns>
    /// <response code="200">Users retrieved successfully</response>
    /// <response code="403">Insufficient permissions - Admin role required</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? role = null,
        [FromQuery] string? status = null)
    {
        _logger.LogInformation("Admin fetching users: SearchTerm={SearchTerm}, Role={Role}, Status={Status}",
            searchTerm, role, status);

        var users = await _adminService.GetAllUsersAsync(searchTerm, role, status);

        return Ok(users);
    }

    /// <summary>
    /// Gets a single user by ID (US_021).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User information</returns>
    /// <response code="200">User retrieved successfully</response>
    /// <response code="404">User not found</response>
    /// <response code="403">Insufficient permissions - Admin role required</response>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserById([FromRoute] Guid userId)
    {
        try
        {
            var user = await _adminService.GetUserByIdAsync(userId);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return NotFound(new { error = "User not found", message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new user account (US_021, AC1).
    /// Sends activation email to new user.
    /// </summary>
    /// <param name="request">User creation data</param>
    /// <returns>Created user information</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid request or duplicate email</response>
    /// <response code="403">Insufficient permissions - Admin role required</response>
    [HttpPost("users")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract audit context
        var ipAddress = HttpContext.Items["AuditContext:IpAddress"] as string;
        var userAgent = HttpContext.Items["AuditContext:UserAgent"] as string;

        try
        {
            var user = await _adminService.CreateUserAsync(request, ipAddress, userAgent);

            _logger.LogInformation("Admin created user: {Email}, Role: {Role}", request.Email, request.Role);

            return CreatedAtAction(
                nameof(GetUserById),
                new { userId = user.UserId },
                user);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
        {
            _logger.LogWarning("User creation failed - duplicate email: {Email}", request.Email);
            return BadRequest(new { error = "Duplicate email", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User creation failed for email: {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "User creation failed", message = "An error occurred while creating the user." });
        }
    }

    /// <summary>
    /// Updates user details (US_021, AC2).
    /// </summary>
    /// <param name="userId">User ID to update</param>
    /// <param name="request">Updated user data</param>
    /// <returns>Updated user information</returns>
    /// <response code="200">User updated successfully</response>
    /// <response code="400">Invalid request or duplicate email</response>
    /// <response code="404">User not found</response>
    /// <response code="403">Insufficient permissions - Admin role required</response>
    [HttpPut("users/{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(
        [FromRoute] Guid userId,
        [FromBody] UpdateUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract audit context
        var ipAddress = HttpContext.Items["AuditContext:IpAddress"] as string;
        var userAgent = HttpContext.Items["AuditContext:UserAgent"] as string;

        try
        {
            var user = await _adminService.UpdateUserAsync(userId, request, ipAddress, userAgent);

            _logger.LogInformation("Admin updated user: {UserId}", userId);

            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User update failed - user not found: {UserId}", userId);
            return NotFound(new { error = "User not found", message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already in use"))
        {
            _logger.LogWarning("User update failed - duplicate email: {UserId}", userId);
            return BadRequest(new { error = "Duplicate email", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User update failed for {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "User update failed", message = "An error occurred while updating the user." });
        }
    }

    /// <summary>
    /// Deactivates user account (US_021, AC3).
    /// Prevents self-deactivation (AC5) and last admin deactivation.
    /// </summary>
    /// <param name="userId">User ID to deactivate</param>
    /// <returns>No content</returns>
    /// <response code="204">User deactivated successfully</response>
    /// <response code="400">Invalid request (self-deactivation or last admin)</response>
    /// <response code="404">User not found</response>
    /// <response code="403">Insufficient permissions - Admin role required</response>
    [HttpDelete("users/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateUser([FromRoute] Guid userId)
    {
        // Get current user ID to prevent self-deactivation
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(currentUserIdClaim) || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
        {
            return Unauthorized(new { error = "Invalid user session" });
        }

        // Extract audit context
        var ipAddress = HttpContext.Items["AuditContext:IpAddress"] as string;
        var userAgent = HttpContext.Items["AuditContext:UserAgent"] as string;

        try
        {
            await _adminService.DeactivateUserAsync(userId, currentUserId, ipAddress, userAgent);

            _logger.LogInformation("Admin deactivated user: {UserId}", userId);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("User deactivation failed - user not found: {UserId}", userId);
            return NotFound(new { error = "User not found", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("User deactivation failed: {UserId}, Reason: {Reason}", userId, ex.Message);
            return BadRequest(new { error = "Deactivation not allowed", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User deactivation failed for {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "User deactivation failed", message = "An error occurred while deactivating the user." });
        }
    }
}
