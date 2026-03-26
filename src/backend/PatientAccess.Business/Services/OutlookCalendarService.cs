using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Exceptions;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions;

namespace PatientAccess.Business.Services;

/// <summary>
/// Microsoft Outlook Calendar integration service (US_040 - FR-024).
/// Implements OAuth2 authorization flow, token refresh, and calendar event CRUD via Microsoft Graph API.
/// Complies with OWASP cryptographic storage requirements (encrypted tokens).
/// </summary>
public class OutlookCalendarService : ICalendarService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<OutlookCalendarService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string Provider = "Outlook";
    private const string AuthorizationEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
    private const string TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token";

    public OutlookCalendarService(
        PatientAccessDbContext context,
        ILogger<OutlookCalendarService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    /// <summary>
    /// Generates OAuth2 authorization URL for Microsoft consent (AC-4).
    /// Includes offline_access scope to ensure refresh token.
    /// </summary>
    public async Task<string> GetAuthorizationUrlAsync(Guid userId, string redirectUri)
    {
        _logger.LogInformation("Generating Microsoft OAuth2 authorization URL for User {UserId}", userId);

        var clientId = _configuration["OutlookCalendarSettings:ClientId"]
            ?? throw new InvalidOperationException("OutlookCalendarSettings:ClientId not configured");
        var scopes = _configuration.GetSection("OutlookCalendarSettings:Scopes").Get<string[]>()
            ?? new[] { "Calendars.ReadWrite", "offline_access" };

        // Build authorization URL with required parameters
        var authUrl = $"{AuthorizationEndpoint}?" +
            $"client_id={Uri.EscapeDataString(clientId)}&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"response_type=code&" +
            $"scope={Uri.EscapeDataString(string.Join(" ", scopes))}&" +
            $"prompt=consent&" +        // Force consent to always get refresh token
            $"state={userId}";          // CSRF protection and user identification

        _logger.LogInformation("Generated authorization URL for User {UserId}", userId);
        return await Task.FromResult(authUrl);
    }

    /// <summary>
    /// Handles OAuth2 callback to exchange authorization code for tokens (AC-4).
    /// Stores encrypted tokens in CalendarIntegration entity.
    /// </summary>
    public async Task HandleCallbackAsync(Guid userId, string authorizationCode, string redirectUri)
    {
        _logger.LogInformation("Processing OAuth2 callback for User {UserId}", userId);

        var clientId = _configuration["OutlookCalendarSettings:ClientId"]
            ?? throw new InvalidOperationException("OutlookCalendarSettings:ClientId not configured");
        var clientSecret = _configuration["OutlookCalendarSettings:ClientSecret"]
            ?? throw new InvalidOperationException("OutlookCalendarSettings:ClientSecret not configured");
        var scopes = _configuration.GetSection("OutlookCalendarSettings:Scopes").Get<string[]>()
            ?? new[] { "Calendars.ReadWrite", "offline_access" };

        // Exchange authorization code for tokens
        var httpClient = _httpClientFactory.CreateClient("MicrosoftOAuth");
        var tokenRequest = new Dictionary<string, string>
        {
            { "code", authorizationCode },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" },
            { "scope", string.Join(" ", scopes) }
        };

        var response = await httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>()
            ?? throw new InvalidOperationException("Failed to parse token response");

        // Upsert CalendarIntegration record
        var integration = await _context.CalendarIntegrations
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.Provider == Provider);

        if (integration == null)
        {
            integration = new CalendarIntegration
            {
                CalendarIntegrationId = Guid.NewGuid(),
                UserId = userId,
                Provider = Provider,
                CalendarId = null, // Will be populated on first event creation if needed
                CreatedAt = DateTime.UtcNow
            };
            _context.CalendarIntegrations.Add(integration);
        }

        // Store tokens (EF Core will encrypt via value converter)
        integration.AccessToken = tokenResponse.AccessToken;
        integration.RefreshToken = tokenResponse.RefreshToken ?? integration.RefreshToken; // Preserve existing if not returned
        integration.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
        integration.IsConnected = true;
        integration.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully stored OAuth2 tokens for User {UserId}", userId);
    }

    /// <summary>
    /// Creates calendar event via Microsoft Graph API (AC-1).
    /// Returns Microsoft Graph event ID for storage in Appointment.OutlookCalendarEventId.
    /// </summary>
    public async Task<string?> CreateEventAsync(Guid userId, CalendarEventDto eventData)
    {
        _logger.LogInformation("Creating Outlook Calendar event for User {UserId}", userId);

        try
        {
            var graphClient = await GetGraphClientForUserAsync(userId);

            var outlookEvent = new Event
            {
                Subject = eventData.Title,
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = eventData.Description
                },
                Start = new DateTimeTimeZone
                {
                    DateTime = eventData.StartTime.ToString("o"),
                    TimeZone = "UTC"
                },
                End = new DateTimeTimeZone
                {
                    DateTime = eventData.EndTime.ToString("o"),
                    TimeZone = "UTC"
                },
                Location = new Location
                {
                    DisplayName = eventData.Location
                }
            };

            var createdEvent = await graphClient.Me.Events.PostAsync(outlookEvent);

            _logger.LogInformation("Created Outlook Calendar event {EventId} for User {UserId}",
                createdEvent?.Id, userId);

            return createdEvent?.Id;
        }
        catch (ServiceException ex) when (ex.ResponseStatusCode == 429)
        {
            // Rate limit handling (EC-1)
            var retryAfter = ex.ResponseHeaders?.TryGetValues("Retry-After", out var values) == true
                ? int.Parse(values.First())
                : 60; // Default 60s if no header

            _logger.LogWarning(
                "Microsoft Graph rate limit hit for user {UserId}. Retry after {Seconds}s",
                userId, retryAfter);

            throw; // Rethrow for Hangfire retry mechanism to handle
        }
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Microsoft Graph API error creating event for User {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Updates existing calendar event with new date and time (AC-2).
    /// </summary>
    public async Task UpdateEventAsync(Guid userId, string eventId, CalendarEventDto eventData)
    {
        _logger.LogInformation("Updating Outlook Calendar event {EventId} for User {UserId}",
            eventId, userId);

        try
        {
            var graphClient = await GetGraphClientForUserAsync(userId);

            var patchEvent = new Event
            {
                Subject = eventData.Title,
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = eventData.Description
                },
                Start = new DateTimeTimeZone
                {
                    DateTime = eventData.StartTime.ToString("o"),
                    TimeZone = "UTC"
                },
                End = new DateTimeTimeZone
                {
                    DateTime = eventData.EndTime.ToString("o"),
                    TimeZone = "UTC"
                },
                Location = new Location
                {
                    DisplayName = eventData.Location
                }
            };

            await graphClient.Me.Events[eventId].PatchAsync(patchEvent);

            _logger.LogInformation("Updated Outlook Calendar event {EventId} for User {UserId}",
                eventId, userId);
        }
        catch (ServiceException ex) when (ex.ResponseStatusCode == 429)
        {
            // Rate limit handling (EC-1)
            var retryAfter = ex.ResponseHeaders?.TryGetValues("Retry-After", out var values) == true
                ? int.Parse(values.First())
                : 60;

            _logger.LogWarning(
                "Microsoft Graph rate limit hit for user {UserId}. Retry after {Seconds}s",
                userId, retryAfter);

            throw;
        }
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Microsoft Graph API error updating event {EventId} for User {UserId}",
                eventId, userId);
            throw;
        }
    }

    /// <summary>
    /// Deletes calendar event from user's Outlook Calendar (AC-3).
    /// </summary>
    public async Task DeleteEventAsync(Guid userId, string eventId)
    {
        _logger.LogInformation("Deleting Outlook Calendar event {EventId} for User {UserId}",
            eventId, userId);

        try
        {
            var graphClient = await GetGraphClientForUserAsync(userId);

            await graphClient.Me.Events[eventId].DeleteAsync();

            _logger.LogInformation("Deleted Outlook Calendar event {EventId} for User {UserId}",
                eventId, userId);
        }
        catch (ServiceException ex) when (ex.ResponseStatusCode == 429)
        {
            // Rate limit handling (EC-1)
            var retryAfter = ex.ResponseHeaders?.TryGetValues("Retry-After", out var values) == true
                ? int.Parse(values.First())
                : 60;

            _logger.LogWarning(
                "Microsoft Graph rate limit hit for user {UserId}. Retry after {Seconds}s",
                userId, retryAfter);

            throw;
        }
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Microsoft Graph API error deleting event {EventId} for User {UserId}",
                eventId, userId);
            throw;
        }
    }

    /// <summary>
    /// Checks if user has an active Outlook Calendar connection.
    /// </summary>
    public async Task<bool> IsConnectedAsync(Guid userId)
    {
        var integration = await _context.CalendarIntegrations
            .AsNoTracking()
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.Provider == Provider);

        return integration?.IsConnected == true;
    }

    /// <summary>
    /// Disconnects user's Outlook Calendar integration.
    /// Marks connection inactive and removes tokens.
    /// </summary>
    public async Task DisconnectAsync(Guid userId)
    {
        _logger.LogInformation("Disconnecting Outlook Calendar for User {UserId}", userId);

        var integration = await _context.CalendarIntegrations
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.Provider == Provider);

        if (integration != null)
        {
            integration.IsConnected = false;
            integration.AccessToken = string.Empty;
            integration.RefreshToken = string.Empty;
            integration.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Disconnected Outlook Calendar for User {UserId}", userId);
        }
    }

    /// <summary>
    /// Builds GraphServiceClient instance with user's OAuth2 credentials.
    /// Automatically refreshes expired tokens (EC-2).
    /// </summary>
    private async Task<GraphServiceClient> GetGraphClientForUserAsync(Guid userId)
    {
        var integration = await _context.CalendarIntegrations
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.Provider == Provider)
            ?? throw new InvalidOperationException($"No Outlook Calendar integration found for User {userId}");

        if (!integration.IsConnected)
        {
            throw new CalendarTokenExpiredException($"Outlook Calendar not connected for User {userId}");
        }

        // Check if token is expired and refresh if needed
        if (integration.TokenExpiry <= DateTime.UtcNow.AddMinutes(5)) // 5-minute buffer
        {
            _logger.LogInformation("Access token expired for User {UserId}, refreshing...", userId);
            await RefreshAccessTokenAsync(integration);
        }

        // Build GraphServiceClient with OAuth2 credentials
        var authProvider = new BaseBearerTokenAuthenticationProvider(
            new TokenProvider(integration.AccessToken));

        var graphClient = new GraphServiceClient(authProvider);

        return graphClient;
    }

    /// <summary>
    /// Refreshes access token using refresh token (EC-2).
    /// Sets IsConnected=false on refresh failure to prompt re-authorization.
    /// </summary>
    private async Task RefreshAccessTokenAsync(CalendarIntegration integration)
    {
        _logger.LogInformation("Refreshing access token for User {UserId}", integration.UserId);

        var clientId = _configuration["OutlookCalendarSettings:ClientId"]
            ?? throw new InvalidOperationException("OutlookCalendarSettings:ClientId not configured");
        var clientSecret = _configuration["OutlookCalendarSettings:ClientSecret"]
            ?? throw new InvalidOperationException("OutlookCalendarSettings:ClientSecret not configured");
        var scopes = _configuration.GetSection("OutlookCalendarSettings:Scopes").Get<string[]>()
            ?? new[] { "Calendars.ReadWrite", "offline_access" };

        try
        {
            var httpClient = _httpClientFactory.CreateClient("MicrosoftOAuth");
            var tokenRequest = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "refresh_token", integration.RefreshToken },
                { "grant_type", "refresh_token" },
                { "scope", string.Join(" ", scopes) }
            };

            var response = await httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                // Token refresh failed - mark connection inactive
                _logger.LogWarning("Token refresh failed for User {UserId}, marking disconnected",
                    integration.UserId);
                integration.IsConnected = false;
                integration.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                throw new CalendarTokenExpiredException(
                    $"Failed to refresh token for User {integration.UserId}. Re-authorization required.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>()
                ?? throw new InvalidOperationException("Failed to parse refresh token response");

            // Update tokens in database
            integration.AccessToken = tokenResponse.AccessToken;
            if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                integration.RefreshToken = tokenResponse.RefreshToken;
            }
            integration.TokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            integration.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully refreshed access token for User {UserId}",
                integration.UserId);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error refreshing token for User {UserId}",
                integration.UserId);
            integration.IsConnected = false;
            integration.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            throw new CalendarTokenExpiredException(
                $"Failed to refresh token for User {integration.UserId}. Re-authorization required.", ex);
        }
    }

    /// <summary>
    /// Microsoft OAuth2 token response model.
    /// </summary>
    private class MicrosoftTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
    }

    /// <summary>
    /// Simple token provider for BaseBearerTokenAuthenticationProvider.
    /// </summary>
    private class TokenProvider : IAccessTokenProvider
    {
        private readonly string _accessToken;

        public TokenProvider(string accessToken)
        {
            _accessToken = accessToken;
        }

        public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_accessToken);
        }

        public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator();
    }
}
