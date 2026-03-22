using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Provider browser API controller for US_023 - Provider Browser (FR-006).
/// Provides REST endpoints for searching and filtering healthcare providers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all provider endpoints
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;
    private readonly ILogger<ProvidersController> _logger;

    public ProvidersController(
        IProviderService providerService,
        ILogger<ProvidersController> logger)
    {
        _providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves paginated list of providers with optional filtering and search (FR-006, AC1, AC2, AC3).
    /// </summary>
    /// <param name="search">Search term for provider name or specialty (optional, AC3)</param>
    /// <param name="specialty">Specialty filter: "all", "family-medicine", "cardiology", etc. (optional, AC2)</param>
    /// <param name="availability">Availability filter: "any-time", "today", "this-week", "this-month" (optional, AC2)</param>
    /// <param name="gender">Gender filter: "any", "male", "female" (optional, AC2)</param>
    /// <param name="page">Page number (1-indexed, default: 1)</param>
    /// <param name="pageSize">Items per page (1-100, default: 20)</param>
    /// <returns>200 OK with ProviderListResponseDto</returns>
    /// <response code="200">Providers retrieved successfully (may be empty array if no results, AC-4)</response>
    /// <response code="400">Invalid query parameters (e.g., page < 1, pageSize out of range)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(ProviderListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProviders(
        [FromQuery] string? search = null,
        [FromQuery] string? specialty = null,
        [FromQuery] string? availability = null,
        [FromQuery] string? gender = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Validate query parameters
            if (page < 1)
            {
                _logger.LogWarning("Invalid page number: {Page}", page);
                return BadRequest(new { message = "Page number must be >= 1" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                _logger.LogWarning("Invalid page size: {PageSize}", pageSize);
                return BadRequest(new { message = "Page size must be between 1 and 100" });
            }

            // Call service to retrieve providers
            var result = await _providerService.GetProvidersAsync(
                search,
                specialty,
                availability,
                gender,
                page,
                pageSize);

            // Return 200 OK even if result is empty (AC-4: Empty state handling)
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving providers");
            return StatusCode(500, new { message = "An error occurred while retrieving providers" });
        }
    }

    /// <summary>
    /// Retrieves a single provider by ID (for future provider detail view).
    /// </summary>
    /// <param name="id">Provider GUID</param>
    /// <returns>200 OK with ProviderDto</returns>
    /// <response code="200">Provider found</response>
    /// <response code="404">Provider not found</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProviderById(Guid id)
    {
        try
        {
            // TODO: Implement GetProviderByIdAsync in IProviderService
            // This is a placeholder for future US_024 (Appointment Booking) where detailed provider view is needed

            _logger.LogWarning("GetProviderById not yet implemented");
            return StatusCode(501, new { message = "Provider detail view not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider {ProviderId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving provider" });
        }
    }
}
