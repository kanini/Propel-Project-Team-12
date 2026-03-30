using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

[ApiController]
[Route("api/health-dashboard")]
[Authorize]
public class HealthDashboardController : ControllerBase
{
    private readonly IHealthDashboardService _service;
    private readonly ILogger<HealthDashboardController> _logger;

    public HealthDashboardController(IHealthDashboardService service, ILogger<HealthDashboardController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get 360° patient health dashboard with all extracted clinical data and medical codes (SCR-016).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(typeof(HealthDashboard360Dto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthDashboard360Dto>> GetMyHealthDashboard()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { error = "Invalid token" });

        var dashboard = await _service.GetPatientHealthDashboardAsync(userId);
        return Ok(dashboard);
    }

    /// <summary>
    /// Get 360° health dashboard for a specific patient (staff view, SCR-017).
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Roles = "Staff,Admin")]
    [ProducesResponseType(typeof(HealthDashboard360Dto), StatusCodes.Status200OK)]
    public async Task<ActionResult<HealthDashboard360Dto>> GetPatientHealthDashboard(Guid patientId)
    {
        var dashboard = await _service.GetPatientHealthDashboardAsync(patientId);
        return Ok(dashboard);
    }
}
