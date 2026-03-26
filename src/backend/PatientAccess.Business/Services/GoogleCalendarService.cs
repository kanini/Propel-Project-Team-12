using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Exceptions;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Google Calendar API v3 integration service (US_039 - FR-024).
/// Implements OAuth2 authorization flow, token refresh, and calendar event CRUD.
/// Complies with OWASP cryptographic storage requirements (encrypted tokens).
/// </summary>
public class GoogleCalendarService : ICalendarService
{
    private readonly PatientAccessDbContext _context;
    private readonly ILogger<GoogleCalendarService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string Provider = "Google";
    private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";

    public GoogleCalendarService(
        PatientAccessDbContext context,
        ILogger<GoogleCalendarService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    /// <summary>
    /// Generates OAuth2 authorization URL for Google consent (AC-4).
    /// Includes access_type=offline and prompt=consent to ensure refresh token.
    /// </summary>
    public async Task<string> GetAuthorizationUrlAsync(Guid userId, string redirectUri)
    {
        _logger.LogInformation("Generating Google OAuth2 authorization URL for User {UserId}", userId);

        var clientId = _configuration["GoogleCalendarSettings:ClientId"]
            ?? throw new InvalidOperationException("GoogleCalendarSettings:ClientId not configured");
        var scopes = _configuration.GetSection("GoogleCalendarSettings:Scopes").Get<string[]>()
            ?? new[] { "https://www.googleapis.com/auth/calendar.events" };

        // Build authorization URL with required parameters
        var authUrl = $"{AuthorizationEndpoint}?" +
            $"client_id={Uri.EscapeDataString(clientId)}&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"response_type=code&" +
            $"scope={Uri.EscapeDataString(string.Join(" ", scopes))}&" +
            $"access_type=offline&" +  // Required for refresh token
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

        var clientId = _configuration["GoogleCalendarSettings:ClientId"]
            ?? throw new InvalidOperationException("GoogleCalendarSettings:ClientId not configured");
        var clientSecret = _configuration["GoogleCalendarSettings:ClientSecret"]
            ?? throw new InvalidOperationException("GoogleCalendarSettings:ClientSecret not configured");

        // Exchange authorization code for tokens
        var httpClient = _httpClientFactory.CreateClient("GoogleOAuth");
        var tokenRequest = new Dictionary<string, string>
        {
            { "code", authorizationCode },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "redirect_uri", redirectUri },
            { "grant_type", "authorization_code" }
        };

        var response = await httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>()
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
                CalendarId = "primary",
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
    /// Creates calendar event via Google Calendar API (AC-1).
    /// Returns Google event ID for storage in Appointment.GoogleCalendarEventId.
    /// </summary>
    public async Task<string?> CreateEventAsync(Guid userId, CalendarEventDto eventData)
    {
        _logger.LogInformation("Creating Google Calendar event for User {UserId}", userId);

        try
        {
            var calendarService = await GetCalendarServiceForUserAsync(userId);
            var calendarId = await GetCalendarIdAsync(userId);

            var calendarEvent = new Event
            {
                Summary = eventData.Title,
                Description = eventData.Description,
                Location = eventData.Location,
                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = eventData.StartTime,
                    TimeZone = "UTC"
                },
                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = eventData.EndTime,
                    TimeZone = "UTC"
                }
            };

            var request = calendarService.Events.Insert(calendarEvent, calendarId);
            var createdEvent = await request.ExecuteAsync();

            _logger.LogInformation("Created Google Calendar event {EventId} for User {UserId}",
                createdEvent.Id, userId);

            return createdEvent.Id;
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, "Google Calendar API error creating event for User {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Updates existing calendar event with new date and time (AC-2).
    /// </summary>
    public async Task UpdateEventAsync(Guid userId, string eventId, CalendarEventDto eventData)
    {
        _logger.LogInformation("Updating Google Calendar event {EventId} for User {UserId}",
            eventId, userId);

        try
        {
            var calendarService = await GetCalendarServiceForUserAsync(userId);
            var calendarId = await GetCalendarIdAsync(userId);

            // Fetch existing event
            var existingEvent = await calendarService.Events.Get(calendarId, eventId).ExecuteAsync();

            // Update event properties
            existingEvent.Summary = eventData.Title;
            existingEvent.Description = eventData.Description;
            existingEvent.Location = eventData.Location;
            existingEvent.Start = new EventDateTime
            {
                DateTimeDateTimeOffset = eventData.StartTime,
                TimeZone = "UTC"
            };
            existingEvent.End = new EventDateTime
            {
                DateTimeDateTimeOffset = eventData.EndTime,
                TimeZone = "UTC"
            };

            var request = calendarService.Events.Update(existingEvent, calendarId, eventId);
            await request.ExecuteAsync();

            _logger.LogInformation("Updated Google Calendar event {EventId} for User {UserId}",
                eventId, userId);
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, "Google Calendar API error updating event {EventId} for User {UserId}",
                eventId, userId);
            throw;
        }
    }

    /// <summary>
    /// Deletes calendar event from user's Google Calendar (AC-3).
    /// </summary>
    public async Task DeleteEventAsync(Guid userId, string eventId)
    {
        _logger.LogInformation("Deleting Google Calendar event {EventId} for User {UserId}",
            eventId, userId);

        try
        {
            var calendarService = await GetCalendarServiceForUserAsync(userId);
            var calendarId = await GetCalendarIdAsync(userId);

            var request = calendarService.Events.Delete(calendarId, eventId);
            await request.ExecuteAsync();

            _logger.LogInformation("Deleted Google Calendar event {EventId} for User {UserId}",
                eventId, userId);
        }
        catch (Google.GoogleApiException ex)
        {
            _logger.LogError(ex, "Google Calendar API error deleting event {EventId} for User {UserId}",
                eventId, userId);
            throw;
        }
    }

    /// <summary>
    /// Checks if user has an active Google Calendar connection.
    /// </summary>
    public async Task<bool> IsConnectedAsync(Guid userId)
    {
        var integration = await _context.CalendarIntegrations
            .AsNoTracking()
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.Provider == Provider);

        return integration?.IsConnected == true;
    }

    /// <summary>
    /// Disconnects user's Google Calendar integration.
    /// Marks connection inactive and removes tokens.
    /// </summary>
    public async Task DisconnectAsync(Guid userId)
    {
        _logger.LogInformation("Disconnecting Google Calendar for User {UserId}", userId);

        var integration = await _context.CalendarIntegrations
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.Provider == Provider);

        if (integration != null)
        {
            integration.IsConnected = false;
            integration.AccessToken = string.Empty;
            integration.RefreshToken = string.Empty;
            integration.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Disconnected Google Calendar for User {UserId}", userId);
        }
    }

    /// <summary>
    /// Builds GoogleCalendarService instance with user's OAuth2 credentials.
    /// Automatically refreshes expired tokens (EC-2).
    /// </summary>
    private async Task<CalendarService> GetCalendarServiceForUserAsync(Guid userId)
    {
        var integration = await _context.CalendarIntegrations
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.Provider == Provider)
            ?? throw new InvalidOperationException($"No Google Calendar integration found for User {userId}");

        if (!integration.IsConnected)
        {
            throw new CalendarTokenExpiredException($"Google Calendar not connected for User {userId}");
        }

        // Check if token is expired and refresh if needed
        if (integration.TokenExpiry <= DateTime.UtcNow.AddMinutes(5)) // 5-minute buffer
        {
            _logger.LogInformation("Access token expired for User {UserId}, refreshing...", userId);
            await RefreshAccessTokenAsync(integration);
        }

        // Build CalendarService with OAuth2 credentials
        var credential = GoogleCredential.FromAccessToken(integration.AccessToken);

        var calendarService = new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "PatientAccess Platform"
        });

        return calendarService;
    }

    /// <summary>
    /// Refreshes access token using refresh token (EC-2).
    /// Sets IsConnected=false on refresh failure to prompt re-authorization.
    /// </summary>
    private async Task RefreshAccessTokenAsync(CalendarIntegration integration)
    {
        _logger.LogInformation("Refreshing access token for User {UserId}", integration.UserId);

        var clientId = _configuration["GoogleCalendarSettings:ClientId"]
            ?? throw new InvalidOperationException("GoogleCalendarSettings:ClientId not configured");
        var clientSecret = _configuration["GoogleCalendarSettings:ClientSecret"]
            ?? throw new InvalidOperationException("GoogleCalendarSettings:ClientSecret not configured");

        try
        {
            var httpClient = _httpClientFactory.CreateClient("GoogleOAuth");
            var tokenRequest = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "refresh_token", integration.RefreshToken },
                { "grant_type", "refresh_token" }
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

            var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>()
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
    /// Retrieves calendar ID for user (defaults to "primary").
    /// </summary>
    private async Task<string> GetCalendarIdAsync(Guid userId)
    {
        var integration = await _context.CalendarIntegrations
            .AsNoTracking()
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.Provider == Provider);

        return integration?.CalendarId ?? "primary";
    }

    /// <summary>
    /// Google OAuth2 token response model.
    /// </summary>
    private class GoogleTokenResponse
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
}
