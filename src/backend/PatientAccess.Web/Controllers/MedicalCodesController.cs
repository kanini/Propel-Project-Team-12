using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.Validators;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// API endpoints for medical code mapping (ICD-10, CPT) with staff verification workflow.
/// US_051 Task 2 - Code Mapping API.
/// Implements FR-034 (ICD-10 suggestions), FR-035 (CPT suggestions), AIR-Q01 (staff verification for quality metrics).
/// </summary>
[ApiController]
[Route("api/medical-codes")]
[Authorize(Roles = "Staff,Admin")]
public class MedicalCodesController : ControllerBase
{
    private readonly ILogger<MedicalCodesController> _logger;
    private readonly ICodeMappingService _codeMappingService;
    private readonly PatientAccessDbContext _context;

    public MedicalCodesController(
        ILogger<MedicalCodesController> logger,
        ICodeMappingService codeMappingService,
        PatientAccessDbContext context)
    {
        _logger = logger;
        _codeMappingService = codeMappingService;
        _context = context;
    }

    /// <summary>
    /// Maps extracted clinical text to ICD-10 diagnosis codes using RAG pattern with Azure OpenAI GPT-4o.
    /// Returns top-N suggestions ranked by confidence score (0-100%).
    /// </summary>
    /// <param name="request">Request containing clinical text and ExtractedClinicalDataId</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ICD-10 code suggestions with confidence scores and rationale</returns>
    [HttpPost("map-icd10")]
    [ProducesResponseType(typeof(CodeMappingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> MapToICD10(
        [FromBody] CodeMappingRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Verify ExtractedClinicalData exists
        var extractedData = await _context.ExtractedClinicalData
            .FindAsync(new object[] { request.ExtractedClinicalDataId }, cancellationToken);

        if (extractedData == null)
        {
            return NotFound(new
            {
                Error = $"Extracted clinical data with ID {request.ExtractedClinicalDataId} not found"
            });
        }

        _logger.LogInformation(
            "Mapping ICD-10 codes for ExtractedDataId: {Id} by user: {User}",
            request.ExtractedClinicalDataId,
            User.Identity?.Name);

        var response = await _codeMappingService.MapToICD10Async(request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Maps extracted clinical text to CPT procedure codes using RAG pattern with Azure OpenAI GPT-4o.
    /// Returns top-N suggestions ranked by confidence score (0-100%).
    /// </summary>
    /// <param name="request">Request containing clinical text and ExtractedClinicalDataId</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CPT code suggestions with confidence scores and rationale</returns>
    [HttpPost("map-cpt")]
    [ProducesResponseType(typeof(CodeMappingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> MapToCPT(
        [FromBody] CodeMappingRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Verify ExtractedClinicalData exists
        var extractedData = await _context.ExtractedClinicalData
            .FindAsync(new object[] { request.ExtractedClinicalDataId }, cancellationToken);

        if (extractedData == null)
        {
            return NotFound(new
            {
                Error = $"Extracted clinical data with ID {request.ExtractedClinicalDataId} not found"
            });
        }

        _logger.LogInformation(
            "Mapping CPT codes for ExtractedDataId: {Id} by user: {User}",
            request.ExtractedClinicalDataId,
            User.Identity?.Name);

        var response = await _codeMappingService.MapToCPTAsync(request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Retrieves existing medical code suggestions for extracted clinical data.
    /// Supports filtering by CodeSystem (ICD10, CPT).
    /// </summary>
    /// <param name="extractedDataId">ExtractedClinicalData ID (GUID)</param>
    /// <param name="codeSystem">Optional filter: "ICD10" or "CPT" (case-sensitive name of CodeSystem enum)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of medical code suggestions ordered by rank</returns>
    [HttpGet("{extractedDataId}/suggestions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSuggestions(
        Guid extractedDataId,
        [FromQuery] string? codeSystem = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.MedicalCodes
                .Include(mc => mc.ExtractedData)
                .Include(mc => mc.Verifier)
                .Where(mc => mc.ExtractedDataId == extractedDataId);

            // Filter by CodeSystem enum if provided
            if (!string.IsNullOrEmpty(codeSystem))
            {
                if (Enum.TryParse<CodeSystem>(codeSystem, out var codeSystemEnum))
                {
                    query = query.Where(mc => mc.CodeSystem == codeSystemEnum);
                }
                else
                {
                    return BadRequest(new { Error = $"Invalid CodeSystem: {codeSystem}. Must be 'ICD10' or 'CPT'" });
                }
            }

            var suggestions = await query
                .OrderBy(mc => mc.Rank)
                .Select(mc => new
                {
                    Id = mc.MedicalCodeId.ToString(),
                    Code = mc.CodeValue,
                    Description = mc.CodeDescription,
                    CodeSystem = mc.CodeSystem.ToString(),
                    ConfidenceScore = mc.ConfidenceScore,
                    Rationale = mc.Rationale ?? string.Empty,
                    Rank = mc.Rank,
                    IsTopSuggestion = mc.IsTopSuggestion,
                    VerificationStatus = mc.VerificationStatus.ToString(),
                    VerifiedBy = mc.VerifiedBy != null ? mc.VerifiedBy.ToString() : null,
                    VerifiedAt = mc.VerifiedAt,
                    ExtractedClinicalDataId = mc.ExtractedDataId.ToString(),
                    SourceClinicalText = mc.ExtractedData != null ? (mc.ExtractedData.SourceTextExcerpt ?? string.Empty) : string.Empty,
                    RetrievedContext = mc.RetrievedContext
                })
                .ToListAsync(cancellationToken);

            if (!suggestions.Any())
            {
                return NotFound(new
                {
                    Error = $"No code suggestions found for ExtractedDataId: {extractedDataId}"
                });
            }

            _logger.LogInformation(
                "Retrieved {Count} code suggestions for ExtractedDataId: {Id}",
                suggestions.Count, extractedDataId);

            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error retrieving medical code suggestions for ExtractedDataId: {Id}", 
                extractedDataId);
            throw;
        }
    }

    /// <summary>
    /// Retrieves only the top-ranked code suggestion (Rank = 1, IsTopSuggestion = true).
    /// CodeSystem query parameter is required to disambiguate ICD10 vs CPT top suggestions.
    /// </summary>
    /// <param name="extractedDataId">ExtractedClinicalData ID (GUID)</param>
    /// <param name="codeSystem">Required: "ICD10" or "CPT" (case-sensitive name of CodeSystem enum)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Single top-ranked medical code suggestion</returns>
    [HttpGet("{extractedDataId}/top-suggestion")]
    [ProducesResponseType(typeof(MedicalCodeSuggestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTopSuggestion(
        Guid extractedDataId,
        [FromQuery] string codeSystem,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(codeSystem))
        {
            return BadRequest(new { Error = "CodeSystem query parameter is required" });
        }

        if (!Enum.TryParse<CodeSystem>(codeSystem, out var codeSystemEnum))
        {
            return BadRequest(new { Error = $"Invalid CodeSystem: {codeSystem}. Must be 'ICD10' or 'CPT'" });
        }

        var topSuggestion = await _context.MedicalCodes
            .Where(mc => mc.ExtractedDataId == extractedDataId &&
                        mc.CodeSystem == codeSystemEnum &&
                        mc.IsTopSuggestion)
            .Select(mc => new MedicalCodeSuggestionDto
            {
                Code = mc.CodeValue,
                Description = mc.CodeDescription,
                ConfidenceScore = mc.ConfidenceScore,
                Rationale = mc.Rationale ?? string.Empty,
                Rank = mc.Rank,
                IsTopSuggestion = mc.IsTopSuggestion
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (topSuggestion == null)
        {
            return NotFound(new
            {
                Error = $"No top suggestion found for ExtractedDataId: {extractedDataId}, CodeSystem: {codeSystem}"
            });
        }

        return Ok(topSuggestion);
    }

    /// <summary>
    /// Staff verification workflow: approve, reject, or modify AI-suggested medical code.
    /// Updates MedicalCode.VerificationStatus, VerifiedBy (current user), and VerifiedAt timestamp.
    /// Used for quality metrics tracking (AIR-Q01: AI-Human Agreement Rate >98%).
    /// </summary>
    /// <param name="codeId">MedicalCode ID (GUID)</param>
    /// <param name="request">Verification request with status and optional notes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation message</returns>
    [HttpPatch("{codeId}/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyCode(
        Guid codeId,
        [FromBody] VerifyCodeRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Parse verification status string to enum
        if (!Enum.TryParse<MedicalCodeVerificationStatus>(request.VerificationStatus, out var verificationStatusEnum))
        {
            return BadRequest(new
            {
                Error = $"Invalid VerificationStatus: {request.VerificationStatus}. Must be 'Accepted', 'Rejected', or 'Modified'"
            });
        }

        var medicalCode = await _context.MedicalCodes
            .FindAsync(new object[] { codeId }, cancellationToken);

        if (medicalCode == null)
        {
            return NotFound(new { Error = $"Medical code with ID {codeId} not found" });
        }

        // Get current user ID from JWT claims (NameIdentifier = UserId)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Error = "User ID not found in authentication token" });
        }

        // Update verification fields
        medicalCode.VerificationStatus = verificationStatusEnum;
        medicalCode.VerifiedBy = userId;
        medicalCode.VerifiedAt = DateTime.UtcNow;
        medicalCode.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Medical code {CodeId} (Code: {Code}) verified as {Status} by user {UserId}",
            codeId, medicalCode.CodeValue, request.VerificationStatus, userId);

        return Ok(new
        {
            Message = $"Medical code verified as {request.VerificationStatus}",
            MedicalCodeId = codeId,
            Code = medicalCode.CodeValue,
            VerificationStatus = request.VerificationStatus,
            VerifiedAt = medicalCode.VerifiedAt
        });
    }

    /// <summary>
    /// Modifies AI-suggested medical code with alternative code selected by staff (US_052 Task 2, AC3).
    /// Updates code value, description, sets VerificationStatus to StaffVerified.
    /// Creates audit trail entry with previous/new values and rationale for compliance.
    /// Edge case: Most recent modification takes precedence (allows modifying previously rejected codes).
    /// </summary>
    /// <param name="codeId">MedicalCode ID (GUID)</param>
    /// <param name="dto">Modification request with new code, description, and rationale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Confirmation with updated code details</returns>
    [HttpPatch("{codeId}/modify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ModifyCode(
        Guid codeId,
        [FromBody] ModifyCodeDto dto,
        CancellationToken cancellationToken)
    {
        // 1. Validate DTO using FluentValidation
        var validator = new ModifyCodeDtoValidator();
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                Message = "Validation failed",
                Errors = validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                })
            });
        }

        // 2. Find existing medical code
        var medicalCode = await _context.MedicalCodes
            .FindAsync(new object[] { codeId }, cancellationToken);

        if (medicalCode == null)
        {
            return NotFound(new { Error = $"Medical code with ID {codeId} not found" });
        }

        // 3. Get current user
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Error = "User ID not found in authentication token" });
        }

        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

        // 4. Capture previous state for audit trail
        var previousState = new
        {
            Code = medicalCode.CodeValue,
            Description = medicalCode.CodeDescription,
            VerificationStatus = medicalCode.VerificationStatus.ToString(),
            ConfidenceScore = medicalCode.ConfidenceScore
        };

        var newState = new
        {
            Code = dto.NewCode,
            Description = dto.NewDescription,
            VerificationStatus = MedicalCodeVerificationStatus.Modified.ToString(), // Staff modification uses "Modified" status
            ConfidenceScore = medicalCode.ConfidenceScore // Confidence unchanged for modified codes
        };

        // 5. Create audit log entry (before modification)
        var auditLog = new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            ActionType = "Modified",
            ResourceType = "MedicalCode",
            ResourceId = codeId,
            ActionDetails = JsonSerializer.Serialize(new
            {
                PreviousValue = previousState,
                NewValue = newState,
                Rationale = dto.Rationale,
                ModifiedBy = userName
            }),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
        };

        // 6. Update medical code (most recent action takes precedence)
        medicalCode.CodeValue = dto.NewCode;
        medicalCode.CodeDescription = dto.NewDescription;
        medicalCode.VerificationStatus = MedicalCodeVerificationStatus.Modified; // Staff modification = "Modified" status
        medicalCode.VerifiedBy = userId;
        medicalCode.VerifiedAt = DateTime.UtcNow;
        medicalCode.UpdatedAt = DateTime.UtcNow;

        // 7. Save changes
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Medical code {CodeId} modified by user {UserId} ({UserName}): {OldCode} → {NewCode}. Rationale: {Rationale}",
            codeId, userId, userName, previousState.Code, dto.NewCode, dto.Rationale);

        return Ok(new
        {
            Message = "Medical code modified successfully",
            MedicalCodeId = codeId,
            Code = medicalCode.CodeValue,
            Description = medicalCode.CodeDescription,
            VerificationStatus = medicalCode.VerificationStatus.ToString(),
            VerifiedAt = medicalCode.VerifiedAt
        });
    }

    /// <summary>
    /// Retrieves audit trail for a specific medical code (US_052 Task 2, Edge Case).
    /// Shows modification history ordered by most recent first.
    /// Supports edge case: displays previous rejections when code was later modified/verified.
    /// </summary>
    /// <param name="codeId">MedicalCode ID (GUID)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of audit log entries ordered by timestamp descending</returns>
    [HttpGet("{codeId}/audit-trail")]
    [ProducesResponseType(typeof(List<MedicalCodeAuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditTrail(
        Guid codeId,
        CancellationToken cancellationToken)
    {
        var auditLogs = await _context.AuditLogs
            .Where(al => al.ResourceType == "MedicalCode" && al.ResourceId == codeId)
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync(cancellationToken);

        if (!auditLogs.Any())
        {
            return NotFound(new { Error = $"No audit trail found for medical code {codeId}" });
        }

        // Map AuditLog entities to DTOs
        var auditDtos = auditLogs.Select(al =>
        {
            // Parse ActionDetails JSON to extract structured data
            Dictionary<string, object>? actionDetails = null;
            string? previousValue = null;
            string? newValue = null;
            string? rationale = null;

            try
            {
                actionDetails = JsonSerializer.Deserialize<Dictionary<string, object>>(al.ActionDetails ?? "{}");

                if (actionDetails != null)
                {
                    if (actionDetails.ContainsKey("PreviousValue"))
                        previousValue = actionDetails["PreviousValue"].ToString();
                    if (actionDetails.ContainsKey("NewValue"))
                        newValue = actionDetails["NewValue"].ToString();
                    if (actionDetails.ContainsKey("Rationale"))
                        rationale = actionDetails["Rationale"].ToString();
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, use raw ActionDetails
                previousValue = al.ActionDetails;
            }

            return new MedicalCodeAuditLogDto
            {
                Action = al.ActionType,
                UserName = al.User?.Name ?? "Unknown",
                UserId = al.UserId,
                PreviousValue = previousValue,
                NewValue = newValue,
                Rationale = rationale,
                CreatedAt = al.Timestamp
            };
        }).ToList();

        _logger.LogInformation(
            "Retrieved {Count} audit trail entries for MedicalCode {CodeId}",
            auditDtos.Count, codeId);

        return Ok(auditDtos);
    }
}

