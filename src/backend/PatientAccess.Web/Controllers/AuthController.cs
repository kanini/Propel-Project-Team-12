using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Services;

namespace PatientAccess.Web.Controllers;

/// <summary>
/// Authentication controller for user registration, login, and email verification (FR-001, FR-002).
/// Handles account creation, authentication, and activation workflows.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account with email verification (FR-001, AC1).
    /// Creates account with 'Pending' status and sends verification email within 2 minutes.
    /// Rate limited: 3 requests per 5 minutes per email address.
    /// </summary>
    /// <param name="request">User registration data</param>
    /// <returns>Registration response with userId and status</returns>
    /// <response code="201">Account created successfully, verification email sent</response>
    /// <response code="400">Invalid input or validation error</response>
    /// <response code="409">Email already registered</response>
    /// <response code="429">Rate limit exceeded</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract IP address and User Agent for audit logging
        var ipAddress = HttpContext.Items["AuditContext:IpAddress"] as string;
        var userAgent = HttpContext.Items["AuditContext:UserAgent"] as string;

        try
        {
            var response = await _authService.RegisterUserAsync(request, ipAddress, userAgent);

            _logger.LogInformation("User registration successful: {Email}", request.Email);

            return CreatedAtAction(
                nameof(Register),
                new { userId = response.UserId },
                response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
        {
            _logger.LogWarning("Registration attempt with duplicate email: {Email}", request.Email);
            
            return Conflict(new
            {
                error = "Email already registered",
                message = ex.Message,
                hint = "Use password recovery if you forgot your credentials"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for email: {Email}", request.Email);
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Registration failed", message = "An error occurred during registration. Please try again." });
        }
    }

    /// <summary>
    /// Authenticates user credentials and generates JWT session token (FR-002, AC1).
    /// Validates credentials, generates JWT with role claims, stores session in Redis with 15-minute TTL.
    /// Implements account lockout after 5 failed login attempts (AC4).
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Login response with JWT token and user information</returns>
    /// <response code="200">Login successful, JWT token generated</response>
    /// <response code="400">Invalid input or validation error</response>
    /// <response code="401">Invalid credentials or account locked</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract IP address and User Agent for audit logging
        var ipAddress = HttpContext.Items["AuditContext:IpAddress"] as string;
        var userAgent = HttpContext.Items["AuditContext:UserAgent"] as string;

        try
        {
            var response = await _authService.LoginAsync(request, ipAddress, userAgent);

            _logger.LogInformation("User login successful: {Email}", request.Email);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for email: {Email}. Reason: {Reason}", 
                request.Email, ex.Message);

            return Unauthorized(new
            {
                error = "Authentication failed",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", request.Email);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Login failed", message = "An error occurred during login. Please try again." });
        }
    }

    /// <summary>
    /// Verifies user email using verification token from email link (FR-001, AC2).
    /// Activates account by changing status from 'Pending' to 'Active'.
    /// </summary>
    /// <param name="token">Verification token from email link</param>
    /// <returns>Verification result</returns>
    /// <response code="200">Email verified successfully</response>
    /// <response code="400">Invalid or expired token</response>
    [HttpGet("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { error = "Invalid token", message = "Verification token is required" });
        }

        // Extract IP address and User Agent for audit logging
        var ipAddress = HttpContext.Items["AuditContext:IpAddress"] as string;
        var userAgent = HttpContext.Items["AuditContext:UserAgent"] as string;

        try
        {
            var result = await _authService.VerifyEmailAsync(token, ipAddress, userAgent);

            if (result)
            {
                _logger.LogInformation("Email verification successful for token");
                
                return Ok(new
                {
                    success = true,
                    message = "Email verified successfully. You can now log in to your account."
                });
            }
            else
            {
                _logger.LogWarning("Email verification failed: Invalid or expired token");
                
                return BadRequest(new
                {
                    error = "Verification failed",
                    message = "Invalid or expired verification token. Please request a new verification email."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Verification failed", message = "An error occurred during verification. Please try again." });
        }
    }

    /// <summary>
    /// Checks if email address is already registered (optional helper endpoint).
    /// </summary>
    /// <param name="email">Email address to check</param>
    /// <returns>True if email exists, false otherwise</returns>
    [HttpGet("check-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { error = "Email is required" });
        }

        var exists = await _authService.IsEmailRegisteredAsync(email);

        return Ok(new { exists });
    }

    /// <summary>
    /// Initiates password reset workflow by sending reset link to email.
    /// Returns success message regardless of email existence to prevent enumeration.
    /// Rate limited: 3 requests per 5 minutes per email address.
    /// </summary>
    /// <param name="request">Forgot password request with email</param>
    /// <returns>Success message</returns>
    /// <response code="200">Password reset email sent (if account exists)</response>
    /// <response code="400">Invalid input or validation error</response>
    /// <response code="429">Rate limit exceeded</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ForgotPasswordResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract IP address and User Agent for audit logging
        var ipAddress = HttpContext.Items["AuditContext:IpAddress"] as string;
        var userAgent = HttpContext.Items["AuditContext:UserAgent"] as string;

        try
        {
            var response = await _authService.ForgotPasswordAsync(request, ipAddress, userAgent);

            _logger.LogInformation("Password reset requested for email: {Email}", request.Email);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing password reset request for email: {Email}", request.Email);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Password reset failed", message = "An error occurred. Please try again." });
        }
    }

    /// <summary>
    /// Resets user password using valid reset token from email link.
    /// Token expires after 1 hour and can only be used once.
    /// </summary>
    /// <param name="request">Reset password request with token and new password</param>
    /// <returns>Success or error message</returns>
    /// <response code="200">Password reset successful</response>
    /// <response code="400">Invalid or expired token, or validation error</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract IP address and User Agent for audit logging
        var ipAddress = HttpContext.Items["AuditContext:IpAddress"] as string;
        var userAgent = HttpContext.Items["AuditContext:UserAgent"] as string;

        try
        {
            var result = await _authService.ResetPasswordAsync(request, ipAddress, userAgent);

            if (result)
            {
                _logger.LogInformation("Password reset successful");

                return Ok(new
                {
                    success = true,
                    message = "Password has been reset successfully. You can now log in with your new password."
                });
            }
            else
            {
                _logger.LogWarning("Password reset failed");

                return BadRequest(new
                {
                    error = "Password reset failed",
                    message = "Unable to reset password. Please try again."
                });
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Password reset failed: {Reason}", ex.Message);

            return BadRequest(new
            {
                error = "Invalid request",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Password reset failed", message = "An error occurred. Please try again." });
        }
    }
}
