using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Authentication controller for user registration and email verification (FR-001).
/// Handles patient registration with email validation and rate limiting.
/// Enhanced with login authentication (FR-002) and session management.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[AllowAnonymous] // Allow public access to registration and login endpoints
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, IAuditService auditService, ILogger<AuthController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a new patient user with email verification.
    /// </summary>
    /// <param name="request">Registration request with user details</param>
    /// <returns>Registration response with user ID and verification instructions</returns>
    /// <response code="201">User created successfully, verification email sent</response>
    /// <response code="400">Invalid request data or validation failed</response>
    /// <response code="409">Email already registered</response>
    /// <response code="429">Rate limit exceeded (max 3 requests per 5 minutes)</response>
    [HttpPost("register")]
    [EnableRateLimiting("registration")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<RegisterUserResponse>> Register([FromBody] RegisterUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Registration request validation failed for email: {Email}", request?.Email);
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _userService.RegisterUserAsync(request);
            
            _logger.LogInformation("User registered successfully: {UserId}", response.UserId);
            
            return CreatedAtAction(
                nameof(VerifyEmail),
                new { token = "pending" },
                response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
        {
            // Return 409 Conflict for duplicate email (FR-001 AC3)
            // Generic message to prevent email enumeration
            _logger.LogWarning(ex, "Registration failed - email already exists");
            
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Registration Failed",
                Detail = "An account with this email address may already exist. If you already have an account, please try logging in or use the password recovery option."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for email: {Email}", request?.Email);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Registration Error",
                Detail = "An unexpected error occurred during registration. Please try again later."
            });
        }
    }

    /// <summary>
    /// Verifies user email using verification token from email link.
    /// </summary>
    /// <param name="token">Verification token (24-hour validity)</param>
    /// <returns>Success message or error if token invalid/expired</returns>
    /// <response code="200">Email verified successfully, account activated</response>
    /// <response code="400">Invalid or expired token</response>
    [HttpGet("verify-email")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Token",
                Detail = "Verification token is required."
            });
        }

        var request = new VerifyEmailRequest { Token = token };
        var success = await _userService.VerifyEmailAsync(request);

        if (!success)
        {
            // Token invalid or expired (FR-001 AC4)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Verification Failed",
                Detail = "The verification link is invalid or has expired. Please request a new verification email."
            });
        }

        _logger.LogInformation("Email verified successfully for token: {Token}", token);

        return Ok(new
        {
            Message = "Email verified successfully! Your account is now active. You can now log in.",
            Success = true
        });
    }

    /// <summary>
    /// Authenticates user with email and password (FR-002).
    /// Generates JWT token and stores session in Redis with 15-minute TTL.
    /// Implements account lockout after 5 failed login attempts.
    /// </summary>
    /// <param name="request">Login request with email and password</param>
    /// <returns>Login response with JWT token and user role</returns>
    /// <response code="200">Authentication successful, JWT token returned</response>
    /// <response code="400">Invalid request data or validation failed</response>
    /// <response code="401">Invalid credentials (generic message)</response>
    /// <response code="403">Account locked or inactive</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Login request validation failed for email: {Email}", request?.Email);
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _userService.AuthenticateUserAsync(request);

            if (response == null)
            {
                // Authentication failed - could be invalid credentials, locked account, or inactive account
                // Return generic error message (FR-002 AC3 - OWASP Email Enumeration Prevention)
                _logger.LogWarning("Login failed for email: {Email}", request.Email);

                // FR-005 AC2: Log failed login attempt with hashed email
                var (ipAddress, userAgent) = GetAuditContext();
                var emailHash = HashEmail(request.Email);
                await _auditService.LogFailedLoginAsync(emailHash, ipAddress, userAgent, "Invalid credentials");
                
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authentication Failed",
                    Detail = "Invalid email or password. Please check your credentials and try again."
                });
            }

            // FR-005 AC1: Log successful login
            var (auditIp, auditUserAgent) = GetAuditContext();
            await _auditService.LogSuccessfulLoginAsync(response.UserId, auditIp, auditUserAgent);

            _logger.LogInformation("User logged in successfully: {UserId}, Role: {Role}", 
                response.UserId, response.Role);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", request?.Email);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Login Error",
                Detail = "An unexpected error occurred during login. Please try again later."
            });
        }
    }

    /// <summary>
    /// Refreshes current user's session by extending Redis TTL to 15 minutes (UXR-604, FR-005).
    /// Called by session timeout warning modal when user clicks "Extend Session" button.
    /// </summary>
    /// <returns>Success message or error if session not found</returns>
    /// <response code="200">Session extended successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Session not found</response>
    [HttpPost("refresh-session")]
    [Authorize] // Requires valid JWT token
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RefreshSession()
    {
        try
        {
            // Extract userId from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("RefreshSession called with invalid or missing user ID claim");
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Unauthorized",
                    Detail = "Invalid authentication token."
                });
            }

            var refreshed = await _userService.RefreshSessionAsync(userId);

            if (!refreshed)
            {
                _logger.LogWarning("RefreshSession failed - session not found for user {UserId}", userId);
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Session Not Found",
                    Detail = "Your session could not be found. Please log in again."
                });
            }

            _logger.LogInformation("Session refreshed successfully for user {UserId}", userId);

            return Ok(new
            {
                Message = "Session extended successfully",
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during session refresh");
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Session Refresh Error",
                Detail = "An unexpected error occurred while refreshing your session. Please try again."
            });
        }
    }

    /// <summary>
    /// Extracts audit context (IP address and User-Agent) from HttpContext.Items.
    /// Set by AuditLoggingMiddleware.
    /// </summary>
    /// <returns>Tuple of (ipAddress, userAgent)</returns>
    private (string? ipAddress, string? userAgent) GetAuditContext()
    {
        var ipAddress = HttpContext.Items["ClientIpAddress"]?.ToString();
        var userAgent = HttpContext.Items["UserAgent"]?.ToString();
        return (ipAddress, userAgent);
    }

    /// <summary>
    /// Hashes email address using SHA256 for privacy-compliant audit logging (FR-005 AC2).
    /// </summary>
    /// <param name="email">Email address to hash</param>
    /// <returns>Base64-encoded SHA256 hash of email</returns>
    private static string HashEmail(string email)
    {
        var bytes = Encoding.UTF8.GetBytes(email.ToLowerInvariant());
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
