using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.Exceptions;
using PatientAccess.Business.Interfaces;
using System.Security.Claims;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Multi-provider calendar integration API controller (US_039/US_040 - FR-024).
/// Handles OAuth2 authorization flow and connection management for Google Calendar and Outlook Calendar.
/// </summary>
[ApiController]
[Route("api/calendar")]
public class CalendarController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CalendarController> _logger;

    public CalendarController(
        IServiceProvider serviceProvider,
        ILogger<CalendarController> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Generates OAuth2 authorization URL for calendar connection (AC-4).
    /// </summary>
    /// <param name="provider">Calendar provider ("google" or "outlook")</param>
    /// <returns>OAuth2 authorization URL</returns>
    [HttpGet("{provider}/connect")]
    [Authorize]
    public async Task<IActionResult> Connect([FromRoute] string provider)
    {
        try
        {
            var providerKey = ValidateAndNormalizeProvider(provider);
            var userId = GetUserIdFromClaims();
            var redirectUri = $"{Request.Scheme}://{Request.Host}/api/calendar/{provider.ToLower()}/callback";

            var calendarService = _serviceProvider.GetRequiredKeyedService<ICalendarService>(providerKey);
            var authorizationUrl = await calendarService.GetAuthorizationUrlAsync(userId, redirectUri);

            _logger.LogInformation("Generated {Provider} OAuth2 authorization URL for User {UserId}",
                providerKey, userId);

            return Ok(new { authorizationUrl, provider = providerKey });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating {Provider} OAuth2 authorization URL", provider);
            return StatusCode(500, new { error = "Failed to generate authorization URL" });
        }
    }

    /// <summary>
    /// Handles OAuth2  callback from calendar provider with authorization code (AC-4).
    /// Exchanges code for tokens and stores in CalendarIntegration entity.
    /// </summary>
    /// <param name="provider">Calendar provider ("google" or "outlook")</param>
    /// <param name="code">Authorization code from OAuth provider</param>
    /// <param name="state">User ID for CSRF protection</param>
    /// <returns>Redirect to frontend with success/error status</returns>
    [HttpGet("{provider}/callback")]
    [AllowAnonymous] // Callback from provider, validated via state parameter
    public async Task<IActionResult> Callback(
        [FromRoute] string provider,
        [FromQuery] string code,
        [FromQuery] string state)
    {
        try
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                _logger.LogWarning("OAuth2 callback missing code or state parameter");
                return Redirect($"/calendar/error?message=Invalid callback parameters&provider={provider}");
            }

            if (!Guid.TryParse(state, out var userId))
            {
                _logger.LogWarning("OAuth2 callback with invalid state parameter: {State}", state);
                return Redirect($"/calendar/error?message=Invalid state parameter&provider={provider}");
            }

            var providerKey = ValidateAndNormalizeProvider(provider);
            var calendarService = _serviceProvider.GetRequiredKeyedService<ICalendarService>(providerKey);
            var redirectUri = $"{Request.Scheme}://{Request.Host}/api/calendar/{provider.ToLower()}/callback";

            await calendarService.HandleCallbackAsync(userId, code, redirectUri);

            _logger.LogInformation("Successfully processed {Provider} OAuth2 callback for User {UserId}",
                providerKey, userId);

            // Redirect to frontend success page
            return Redirect($"/calendar/success?provider={providerKey}");
        }
        catch (ArgumentException ex)
        {
            return Redirect($"/calendar/error?message={Uri.EscapeDataString(ex.Message)}&provider={provider}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing {Provider} OAuth2 callback", provider);
            return Redirect($"/calendar/error?message=Failed to connect calendar&provider={provider}");
        }
    }

    /// <summary>
    /// Retrieves calendar connection status for all providers.
    /// </summary>
    /// <returns>Connection status for Google and Outlook</returns>
    [HttpGet("status")]
    [Authorize]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var userId = GetUserIdFromClaims();

            var googleService = _serviceProvider.GetRequiredKeyedService<ICalendarService>("Google");
            var outlookService = _serviceProvider.GetRequiredKeyedService<ICalendarService>("Outlook");

            var googleConnected = await googleService.IsConnectedAsync(userId);
            var outlookConnected = await outlookService.IsConnectedAsync(userId);

            return Ok(new
            {
                google = new { isConnected = googleConnected },
                outlook = new { isConnected = outlookConnected }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving calendar status");
            return StatusCode(500, new { error = "Failed to retrieve calendar status" });
        }
    }

    /// <summary>
    /// Disconnects calendar integration for specified provider.
    /// </summary>
    /// <param name="provider">Calendar provider ("google" or "outlook")</param>
    /// <returns>Success confirmation</returns>
    [HttpPost("{provider}/disconnect")]
    [Authorize]
    public async Task<IActionResult> Disconnect([FromRoute] string provider)
    {
        try
        {
            var providerKey = ValidateAndNormalizeProvider(provider);
            var userId = GetUserIdFromClaims();

            var calendarService = _serviceProvider.GetRequiredKeyedService<ICalendarService>(providerKey);
            await calendarService.DisconnectAsync(userId);

            _logger.LogInformation("Disconnected {Provider} Calendar for User {UserId}", providerKey, userId);

            return Ok(new { message = $"{providerKey} Calendar disconnected successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting {Provider} calendar", provider);
            return StatusCode(500, new { error = "Failed to disconnect calendar" });
        }
    }

    /// <summary>
    /// Validates and normalizes provider name.
    /// </summary>
    /// <param name="provider">Provider name from route (case-insensitive)</param>
    /// <returns>Normalized provider key ("Google" or "Outlook")</returns>
    /// <exception cref="ArgumentException">Thrown if provider is invalid</exception>
    private static string ValidateAndNormalizeProvider(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentException("Provider parameter is required");
        }

        return provider.ToLowerInvariant() switch
        {
            "google" => "Google",
            "outlook" => "Outlook",
            _ => throw new ArgumentException($"Unsupported calendar provider: {provider}. Valid options: google, outlook")
        };
    }

    /// <summary>
    /// Extracts user ID from JWT claims.
    /// </summary>
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User ID not found in token claims");

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID format in token claims");
        }

        return userId;
    }
}
