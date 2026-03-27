using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Medical Codes API controller for US_051 - Medical Code Mapping (FR-034, FR-035, FR-036).
/// Provides REST endpoints for ICD-10/CPT code mapping and staff verification workflow.
/// </summary>
[ApiController]
[Route("api/medical-codes")]
[Authorize(Roles = "Staff,Admin")] // Staff and Admin only
public class MedicalCodesController : ControllerBase
{
    private readonly ICodeMappingService _codeMappingService;
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<MedicalCodesController> _logger;

    public MedicalCodesController(
        ICodeMappingService codeMappingService,
        PatientAccessDbContext context,
        ILogger<MedicalCodesController> logger)
    {
        _codeMappingService = codeMappingService ?? throw new ArgumentNullException(nameof(codeMappingService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Maps clinical text to ICD-10 diagnosis codes using RAG + Azure OpenAI GPT-4o (FR-034, AC1).
    /// Returns ranked code suggestions with confidence scores (0-100%).
    /// </summary>
    /// <param name="request">Code mapping request with clinical text and parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>200 OK with ICD-10 code suggestions</returns>
    /// <response code="200">ICD-10 codes mapped successfully</response>
    /// <response code="400">Invalid request (missing required fields, validation errors)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (requires Staff or Admin role)</response>
    /// <response code="404">Extracted clinical data not found</response>
    /// <response code="422">AI response validation failed (schema quality below threshold)</response>
    /// <response code="503">AI service temporarily unavailable</response>
    [HttpPost("map-icd10")]
    [ProducesResponseType(typeof(CodeMappingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> MapToICD10(
        [FromBody] CodeMappingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Validate ModelState
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid ModelState for ICD-10 mapping request");
            return BadRequest(ModelState);
        }

        // Verify ExtractedClinicalData exists
        var extractedData = await _context.ExtractedClinicalData
            .FindAsync(new object[] { request.ExtractedClinicalDataId }, cancellationToken);

        if (extractedData == null)
        {
            _logger.LogWarning("ExtractedClinicalData not found: {Id}", request.ExtractedClinicalDataId);
            return NotFound(new
            {
                error = $"Extracted clinical data with ID {request.ExtractedClinicalDataId} not found"
            });
        }

        _logger.LogInformation("Mapping ICD-10 codes for ExtractedDataId: {Id}", request.ExtractedClinicalDataId);

        var response = await _codeMappingService.MapToICD10Async(request, cancellationToken);

        _logger.LogInformation("ICD-10 mapping completed: ExtractedDataId={Id}, SuggestionCount={Count}",
            request.ExtractedClinicalDataId, response.SuggestionCount);

        return Ok(response);
    }

    /// <summary>
    /// Maps clinical text to CPT procedure codes using RAG + Azure OpenAI GPT-4o (FR-035, AC2).
    /// Returns ranked code suggestions with confidence scores (0-100%).
    /// </summary>
    /// <param name="request">Code mapping request with clinical text and parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>200 OK with CPT code suggestions</returns>
    /// <response code="200">CPT codes mapped successfully</response>
    /// <response code="400">Invalid request (missing required fields, validation errors)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (requires Staff or Admin role)</response>
    /// <response code="404">Extracted clinical data not found</response>
    /// <response code="422">AI response validation failed (schema quality below threshold)</response>
    /// <response code="503">AI service temporarily unavailable</response>
    [HttpPost("map-cpt")]
    [ProducesResponseType(typeof(CodeMappingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> MapToCPT(
        [FromBody] CodeMappingRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Validate ModelState
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid ModelState for CPT mapping request");
            return BadRequest(ModelState);
        }

        // Verify ExtractedClinicalData exists
        var extractedData = await _context.ExtractedClinicalData
            .FindAsync(new object[] { request.ExtractedClinicalDataId }, cancellationToken);

        if (extractedData == null)
        {
            _logger.LogWarning("ExtractedClinicalData not found: {Id}", request.ExtractedClinicalDataId);
            return NotFound(new
            {
                error = $"Extracted clinical data with ID {request.ExtractedClinicalDataId} not found"
            });
        }

        _logger.LogInformation("Mapping CPT codes for ExtractedDataId: {Id}", request.ExtractedClinicalDataId);

        var response = await _codeMappingService.MapToCPTAsync(request, cancellationToken);

        _logger.LogInformation("CPT mapping completed: ExtractedDataId={Id}, SuggestionCount={Count}",
            request.ExtractedClinicalDataId, response.SuggestionCount);

        return Ok(response);
    }

    /// <summary>
    /// Retrieves existing code suggestions for extracted clinical data (FR-034, FR-035).
    /// Supports filtering by code system (ICD10, CPT).
    /// </summary>
    /// <param name="extractedDataId">Extracted clinical data ID</param>
    /// <param name="codeSystem">Optional filter: "ICD10" or "CPT"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>200 OK with list of code suggestions ranked by confidence</returns>
    /// <response code="200">Code suggestions retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (requires Staff or Admin role)</response>
    /// <response code="404">No code suggestions found for extracted data ID</response>
    [HttpGet("{extractedDataId}/suggestions")]
    [ProducesResponseType(typeof(List<MedicalCodeSuggestionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSuggestions(
        Guid extractedDataId,
        [FromQuery] string? codeSystem = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving code suggestions: ExtractedDataId={Id}, CodeSystem={CodeSystem}",
            extractedDataId, codeSystem ?? "All");

        var query = _context.MedicalCodes
            .Where(mc => mc.ExtractedDataId == extractedDataId);

        // Filter by code system if specified
        if (!string.IsNullOrEmpty(codeSystem))
        {
            if (codeSystem.ToUpper() == "ICD10")
            {
                query = query.Where(mc => mc.CodeSystem == CodeSystem.ICD10);
            }
            else if (codeSystem.ToUpper() == "CPT")
            {
                query = query.Where(mc => mc.CodeSystem == CodeSystem.CPT);
            }
            else
            {
                return BadRequest(new { error = "CodeSystem must be 'ICD10' or 'CPT'" });
            }
        }

        var suggestions = await query
            .OrderBy(mc => mc.Rank)
            .Select(mc => new MedicalCodeSuggestionDto
            {
                Code = mc.CodeValue,
                Description = mc.CodeDescription,
                ConfidenceScore = mc.ConfidenceScore,
                Rationale = mc.Rationale,
                Rank = mc.Rank,
                IsTopSuggestion = mc.IsTopSuggestion
            })
            .ToListAsync(cancellationToken);

        if (!suggestions.Any())
        {
            _logger.LogWarning("No code suggestions found: ExtractedDataId={Id}, CodeSystem={CodeSystem}",
                extractedDataId, codeSystem ?? "All");
            return NotFound(new
            {
                error = $"No code suggestions found for ExtractedDataId: {extractedDataId}"
            });
        }

        _logger.LogInformation("Retrieved {Count} code suggestions for ExtractedDataId: {Id}",
            suggestions.Count, extractedDataId);

        return Ok(suggestions);
    }

    /// <summary>
    /// Retrieves the top-ranked code suggestion for extracted clinical data (FR-034, FR-035).
    /// Returns the suggestion with Rank = 1 and IsTopSuggestion = true.
    /// </summary>
    /// <param name="extractedDataId">Extracted clinical data ID</param>
    /// <param name="codeSystem">Required: "ICD10" or "CPT"</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>200 OK with top code suggestion</returns>
    /// <response code="200">Top suggestion retrieved successfully</response>
    /// <response code="400">Invalid request (missing or invalid CodeSystem)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (requires Staff or Admin role)</response>
    /// <response code="404">No top suggestion found for extracted data ID and code system</response>
    [HttpGet("{extractedDataId}/top-suggestion")]
    [ProducesResponseType(typeof(MedicalCodeSuggestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTopSuggestion(
        Guid extractedDataId,
        [FromQuery] string codeSystem,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(codeSystem))
        {
            return BadRequest(new { error = "CodeSystem query parameter is required" });
        }

        CodeSystem codeSystemEnum;
        if (codeSystem.ToUpper() == "ICD10")
        {
            codeSystemEnum = CodeSystem.ICD10;
        }
        else if (codeSystem.ToUpper() == "CPT")
        {
            codeSystemEnum = CodeSystem.CPT;
        }
        else
        {
            return BadRequest(new { error = "CodeSystem must be 'ICD10' or 'CPT'" });
        }

        _logger.LogInformation("Retrieving top suggestion: ExtractedDataId={Id}, CodeSystem={CodeSystem}",
            extractedDataId, codeSystem);

        var topSuggestion = await _context.MedicalCodes
            .Where(mc => mc.ExtractedDataId == extractedDataId &&
                        mc.CodeSystem == codeSystemEnum &&
                        mc.IsTopSuggestion)
            .Select(mc => new MedicalCodeSuggestionDto
            {
                Code = mc.CodeValue,
                Description = mc.CodeDescription,
                ConfidenceScore = mc.ConfidenceScore,
                Rationale = mc.Rationale,
                Rank = mc.Rank,
                IsTopSuggestion = mc.IsTopSuggestion
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (topSuggestion == null)
        {
            _logger.LogWarning("No top suggestion found: ExtractedDataId={Id}, CodeSystem={CodeSystem}",
                extractedDataId, codeSystem);
            return NotFound(new
            {
                error = $"No top suggestion found for ExtractedDataId: {extractedDataId}, CodeSystem: {codeSystem}"
            });
        }

        _logger.LogInformation("Retrieved top suggestion: ExtractedDataId={Id}, Code={Code}",
            extractedDataId, topSuggestion.Code);

        return Ok(topSuggestion);
    }

    /// <summary>
    /// Staff verification workflow: verify or reject AI-suggested medical code (FR-036).
    /// Updates verification status and tracks for AIR-Q01 (AI-Human Agreement Rate).
    /// </summary>
    /// <param name="codeId">Medical code ID</param>
    /// <param name="request">Verification request with status and notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>200 OK with verification confirmation</returns>
    /// <response code="200">Medical code verified successfully</response>
    /// <response code="400">Invalid request (invalid verification status)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized (requires Staff or Admin role)</response>
    /// <response code="404">Medical code not found</response>
    [HttpPatch("{codeId}/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyCode(
        Guid codeId,
        [FromBody] VerifyCodeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Validate ModelState
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid ModelState for code verification request");
            return BadRequest(ModelState);
        }

        var medicalCode = await _context.MedicalCodes
            .FindAsync(new object[] { codeId }, cancellationToken);

        if (medicalCode == null)
        {
            _logger.LogWarning("Medical code not found: {CodeId}", codeId);
            return NotFound(new { error = $"Medical code with ID {codeId} not found" });
        }

        // Get current user ID from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogError("Invalid or missing user ID in JWT claims");
            return BadRequest(new { error = "Invalid user authentication" });
        }

        // Update verification status
        var verificationStatusEnum = request.VerificationStatus == "StaffVerified"
            ? MedicalCodeVerificationStatus.Accepted
            : MedicalCodeVerificationStatus.Rejected;

        medicalCode.VerificationStatus = verificationStatusEnum;
        medicalCode.VerifiedBy = userId;
        medicalCode.VerifiedAt = DateTime.UtcNow;
        medicalCode.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Medical code verified: CodeId={CodeId}, Status={Status}, VerifiedBy={UserId}",
            codeId, request.VerificationStatus, userId);

        return Ok(new
        {
            message = $"Medical code verified as {request.VerificationStatus}",
            codeId = codeId,
            verificationStatus = request.VerificationStatus,
            verifiedAt = medicalCode.VerifiedAt
        });
    }
}
