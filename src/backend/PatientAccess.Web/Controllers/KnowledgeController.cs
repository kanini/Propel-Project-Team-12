using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Knowledge base API controller for EP-008/US_050 - Hybrid retrieval (AIR-R02, AIR-R03, AIR-R04).
/// Provides REST endpoints for semantic + keyword search across ICD-10, CPT, and clinical terminology.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all knowledge base endpoints
public class KnowledgeController : ControllerBase
{
    private readonly IHybridRetrievalService _retrievalService;
    private readonly ILogger<KnowledgeController> _logger;

    public KnowledgeController(
        IHybridRetrievalService retrievalService,
        ILogger<KnowledgeController> logger)
    {
        _retrievalService = retrievalService ?? throw new ArgumentNullException(nameof(retrievalService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Searches knowledge base using hybrid retrieval (semantic + keyword).
    /// Returns top-5 results with cosine similarity above 0.75 threshold (AIR-R02).
    /// Falls back to keyword-only if no confident semantic matches (edge case handling).
    /// </summary>
    /// <param name="query">Search query (e.g., "Type 2 Diabetes Mellitus")</param>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="topK">Number of results (default 5, max 20)</param>
    /// <param name="minSimilarityThreshold">Minimum cosine similarity (default 0.75)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>200 OK with search results</returns>
    /// <response code="200">Search completed successfully (may include fallback message)</response>
    /// <response code="400">Invalid parameters (empty query or invalid code system)</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(CodeSearchResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] string codeSystem,
        [FromQuery] int topK = 5,
        [FromQuery] double minSimilarityThreshold = 0.75,
        CancellationToken cancellationToken = default)
    {
        // Validate query
        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.LogWarning("Search request rejected: empty query");
            return BadRequest(new { error = "Query cannot be empty" });
        }

        // Validate code system
        if (!new[] { "ICD10", "CPT", "ClinicalTerminology" }.Contains(codeSystem))
        {
            _logger.LogWarning("Search request rejected: invalid code system {CodeSystem}", codeSystem);
            return BadRequest(new { error = "Invalid code system. Must be ICD10, CPT, or ClinicalTerminology" });
        }

        // Validate topK range
        if (topK < 1 || topK > 20)
        {
            _logger.LogWarning("Search request rejected: topK out of range {TopK}", topK);
            return BadRequest(new { error = "TopK must be between 1 and 20" });
        }

        // Validate threshold range
        if (minSimilarityThreshold < 0.0 || minSimilarityThreshold > 1.0)
        {
            _logger.LogWarning("Search request rejected: threshold out of range {Threshold}", minSimilarityThreshold);
            return BadRequest(new { error = "MinSimilarityThreshold must be between 0.0 and 1.0" });
        }

        var request = new CodeSearchRequestDto
        {
            Query = query,
            CodeSystem = codeSystem,
            TopK = topK,
            MinSimilarityThreshold = minSimilarityThreshold
        };

        try
        {
            var response = await _retrievalService.SearchAsync(request, cancellationToken);

            _logger.LogInformation("Search request completed: Query={Query}, CodeSystem={CodeSystem}, Results={ResultCount}", 
                query, codeSystem, response.ResultCount);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument in search request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during search");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Retrieves direct code details by exact code match (optional endpoint for known code lookups).
    /// Example: GET /api/knowledge/ICD10/E11.9
    /// </summary>
    /// <param name="codeSystem">Code system: ICD10, CPT, or ClinicalTerminology</param>
    /// <param name="code">Exact code (e.g., "E11.9", "99213")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>200 OK with code details, or 404 Not Found</returns>
    /// <response code="200">Code found and returned</response>
    /// <response code="400">Invalid code system</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Code not found</response>
    [HttpGet("{codeSystem}/{code}")]
    [ProducesResponseType(typeof(CodeRetrievalResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCodeDetails(
        string codeSystem, 
        string code,
        CancellationToken cancellationToken = default)
    {
        // Validate code system
        if (!new[] { "ICD10", "CPT", "ClinicalTerminology" }.Contains(codeSystem))
        {
            _logger.LogWarning("GetCodeDetails request rejected: invalid code system {CodeSystem}", codeSystem);
            return BadRequest(new { error = "Invalid code system. Must be ICD10, CPT, or ClinicalTerminology" });
        }

        // Use keyword search with exact code match
        var request = new CodeSearchRequestDto
        {
            Query = code,
            CodeSystem = codeSystem,
            TopK = 1,
            MinSimilarityThreshold = 0.0
        };

        try
        {
            var response = await _retrievalService.KeywordSearchAsync(code, codeSystem, 1, cancellationToken);

            if (response.Count == 0 || !response[0].Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Code not found: CodeSystem={CodeSystem}, Code={Code}", codeSystem, code);
                return NotFound(new { error = $"Code '{code}' not found in {codeSystem} knowledge base" });
            }

            _logger.LogInformation("Code details retrieved: CodeSystem={CodeSystem}, Code={Code}", codeSystem, code);
            return Ok(response[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving code details");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An error occurred while processing your request" });
        }
    }
}
