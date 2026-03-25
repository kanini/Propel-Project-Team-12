using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Insurance precheck controller (US_036)
/// </summary>
[ApiController]
[Route("api/insurance")]
[Authorize]
public class InsurancePrecheckController : ControllerBase
{
    private readonly IInsurancePrecheckService _precheckService;
    private readonly ILogger<InsurancePrecheckController> _logger;

    public InsurancePrecheckController(
        IInsurancePrecheckService precheckService,
        ILogger<InsurancePrecheckController> logger)
    {
        _precheckService = precheckService;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/insurance/precheck - Verify insurance eligibility
    /// </summary>
    [HttpPost("precheck")]
    [ProducesResponseType(typeof(InsurancePrecheckResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InsurancePrecheckResponseDto>> VerifyInsurance(
        [FromBody] InsurancePrecheckRequestDto request,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.ProviderId))
        {
            return BadRequest(new { error = "ProviderId is required" });
        }

        if (string.IsNullOrEmpty(request.MemberId))
        {
            return BadRequest(new { error = "MemberId is required" });
        }

        _logger.LogInformation(
            "Insurance precheck requested for appointment {AppointmentId}",
            request.AppointmentId);

        var result = await _precheckService.VerifyInsuranceAsync(request, ct);

        return Ok(result);
    }

    /// <summary>
    /// GET /api/insurance/precheck/{appointmentId} - Get cached precheck result
    /// </summary>
    [HttpGet("precheck/{appointmentId:int}")]
    [ProducesResponseType(typeof(InsurancePrecheckResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InsurancePrecheckResponseDto>> GetPrecheckResult(
        int appointmentId,
        CancellationToken ct)
    {
        var result = await _precheckService.GetPrecheckResultAsync(appointmentId, ct);

        if (result == null)
        {
            return NotFound(new { error = "No precheck result found for this appointment" });
        }

        return Ok(result);
    }
}
