using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

[ApiController]
[Route("api/clinical-verification")]
[Authorize(Roles = "Staff,Admin")]
public class ClinicalVerificationController : ControllerBase
{
    private readonly IClinicalVerificationService _service;
    private readonly ILogger<ClinicalVerificationController> _logger;

    public ClinicalVerificationController(IClinicalVerificationService service, ILogger<ClinicalVerificationController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get verification queue — patients with pending verifications (SCR-023A).
    /// </summary>
    [HttpGet("queue")]
    [ProducesResponseType(typeof(VerificationQueueResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<VerificationQueueResponseDto>> GetVerificationQueue(
        [FromQuery] int limit = 10,
        [FromQuery] string? search = null)
    {
        var queue = await _service.GetVerificationQueueAsync(limit, search);
        return Ok(queue);
    }

    /// <summary>
    /// Get clinical verification dashboard for a patient (SCR-023).
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(ClinicalVerificationDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClinicalVerificationDashboardDto>> GetVerificationDashboard(Guid patientId)
    {
        var dashboard = await _service.GetVerificationDashboardAsync(patientId);
        return Ok(dashboard);
    }

    /// <summary>
    /// Verify an extracted clinical data point.
    /// </summary>
    [HttpPost("data/{id:guid}/verify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> VerifyDataPoint(Guid id)
    {
        var staffId = GetStaffUserId();
        await _service.VerifyDataPointAsync(id, staffId);
        return NoContent();
    }

    /// <summary>
    /// Reject an extracted clinical data point.
    /// </summary>
    [HttpPost("data/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RejectDataPoint(Guid id)
    {
        var staffId = GetStaffUserId();
        await _service.RejectDataPointAsync(id, staffId);
        return NoContent();
    }

    /// <summary>
    /// Accept/verify a medical code.
    /// </summary>
    [HttpPost("codes/{id:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AcceptMedicalCode(Guid id)
    {
        var staffId = GetStaffUserId();
        await _service.VerifyMedicalCodeAsync(id, staffId);
        return NoContent();
    }

    /// <summary>
    /// Reject a medical code with reason.
    /// </summary>
    [HttpPost("codes/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RejectMedicalCode(Guid id, [FromBody] RejectActionDto dto)
    {
        var staffId = GetStaffUserId();
        await _service.RejectMedicalCodeAsync(id, staffId, dto.Reason ?? "");
        return NoContent();
    }

    /// <summary>
    /// Modify a medical code (change code value/description).
    /// </summary>
    [HttpPost("codes/{id:guid}/modify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ModifyMedicalCode(Guid id, [FromBody] ModifyCodeDto dto)
    {
        var staffId = GetStaffUserId();
        await _service.ModifyMedicalCodeAsync(id, dto.CodeValue, dto.CodeDescription, staffId);
        return NoContent();
    }

    private Guid GetStaffUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("Invalid token");
        return userId;
    }
}
