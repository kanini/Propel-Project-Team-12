using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Documents controller for clinical document management (US_067).
/// Provides endpoints for retrieving recent documents and metadata.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Patient")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IAuditLogService _auditService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IDocumentService documentService,
        IAuditLogService auditService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get recent clinical documents for dashboard display (US_067, AC6).
    /// Returns most recent documents ordered by upload date.
    /// </summary>
    /// <param name="limit">Maximum number of documents to return (default: 3, max: 10)</param>
    /// <returns>List of recent documents with processing status</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(List<RecentDocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<RecentDocumentDto>>> GetRecentAsync(
        [FromQuery] int limit = 3)
    {
        try
        {
            if (limit < 1 || limit > 10)
            {
                return BadRequest(new { error = "Limit must be between 1 and 10" });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("User ID not found or invalid in token");
                return Unauthorized(new { error = "Invalid authentication token" });
            }

            _logger.LogInformation("Retrieving {Limit} recent documents for user {UserId}", limit, userId);

            // TODO: Implement PHI access logging when audit service supports it (NFR-007)
            // await _auditService.LogAccessAsync(userId, "ClinicalDocument", "ViewRecent");

            var documents = await _documentService.GetRecentDocumentsAsync(userId, limit);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent documents");
            return StatusCode(500, new { error = "Unable to retrieve documents" });
        }
    }
}
