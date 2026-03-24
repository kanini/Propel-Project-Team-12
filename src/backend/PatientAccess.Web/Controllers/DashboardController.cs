using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Dashboard controller for patient dashboard statistics (US_067).
/// Provides aggregated metrics and summary data for dashboard display.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Patient")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get dashboard statistics for authenticated patient (US_067, AC2).
    /// Returns appointment counts and trend indicators.
    /// </summary>
    /// <returns>Dashboard statistics with trends</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardStatsDto>> GetStatsAsync()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("User ID not found or invalid in token");
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            _logger.LogInformation("Retrieving dashboard stats for user {UserId}", userId);

            var stats = await _dashboardService.GetDashboardStatsAsync(userId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return StatusCode(500, new { error = "Unable to retrieve dashboard statistics" });
        }
    }
}
