using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Business.Interfaces;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Business.Services;

/// <summary>
/// Authentication service implementation for user registration and verification (FR-001).
/// Handles account creation with email verification workflow per TR-021.
/// Implements login with JWT generation and session management (FR-002).
/// </summary>
public class AuthService : IAuthService
{
    private readonly PatientAccessDbContext _dbContext;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ISessionCacheService? _sessionCacheService; // Optional: graceful degradation when Redis unavailable
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuthService> _logger;
    private const int MaxFailedLoginAttempts = 5;
    private const string FailedLoginKeyPrefix = "failed_login:";

    public AuthService(
        PatientAccessDbContext dbContext,
        IPasswordHashingService passwordHashingService,
        IEmailService emailService,
        IJwtTokenService jwtTokenService,
        IAuditLogService auditLogService,
        ILogger<AuthService> logger,
        ISessionCacheService? sessionCacheService = null) // Optional dependency
    {
        _dbContext = dbContext;
        _passwordHashingService = passwordHashingService;
        _emailService = emailService;
        _jwtTokenService = jwtTokenService;
        _sessionCacheService = sessionCacheService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Registers new user with email verification workflow (FR-001, AC1).
    /// </summary>
    public async Task<RegisterUserResponseDto> RegisterUserAsync(RegisterUserRequestDto request, string? ipAddress = null, string? userAgent = null)
    {
        // Check for duplicate email (FR-001, AC3)
        var emailExists = await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (emailExists)
        {
            throw new InvalidOperationException($"Email address {request.Email} is already registered. Please use password recovery if you forgot your credentials.");
        }

        // Hash password using BCrypt with cost factor 12 (TR-013)
        var passwordHash = _passwordHashingService.HashPassword(request.Password);

        // Generate cryptographically secure verification token (32 bytes, TR-021)
        var verificationToken = GenerateVerificationToken();

        // Create user entity with 'Pending' status (DR-001)
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Email = request.Email.ToLower().Trim(),
            Name = request.Name.Trim(),
            DateOfBirth = request.DateOfBirth,
            Phone = request.Phone?.Trim(),
            PasswordHash = passwordHash,
            Role = UserRole.Patient, // Default role for self-registered users
            Status = UserStatus.Pending, // Account pending email verification
            VerificationToken = verificationToken,
            VerificationTokenExpiry = DateTime.UtcNow.AddHours(24), // 24-hour token expiry
            CreatedAt = DateTime.UtcNow
        };

        // Persist user to database
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User registered with email {Email}, UserId {UserId}, Status: Pending",
            user.Email, user.UserId);

        // Log registration event (US_022, AC1)
        await _auditLogService.LogAuthEventAsync(
            userId: user.UserId,
            actionType: AuditActionType.Registration,
            ipAddress: ipAddress,
            userAgent: userAgent,
            metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                email = user.Email,
                role = user.Role.ToString(),
                status = user.Status.ToString()
            }));

        // Send verification email asynchronously (FR-001, AC1)
        var emailSent = await _emailService.SendVerificationEmailAsync(
            user.Email,
            user.Name,
            verificationToken);

        if (!emailSent)
        {
            _logger.LogWarning("Failed to send verification email to {Email}", user.Email);
        }

        return new RegisterUserResponseDto
        {
            UserId = user.UserId,
            Email = user.Email,
            Status = user.Status.ToString(),
            Message = "Registration successful. Please check your email to verify your account."
        };
    }

    /// <summary>
    /// Verifies user email using verification token (FR-001, AC2).
    /// </summary>
    public async Task<bool> VerifyEmailAsync(string token, string? ipAddress = null, string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Email verification attempted with null or empty token");
            return false;
        }

        // Find user by verification token
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.VerificationToken == token);

        if (user == null)
        {
            _logger.LogWarning("Email verification failed: Invalid token");
            return false;
        }

        // Check token expiry (24 hours)
        if (user.VerificationTokenExpiry == null || user.VerificationTokenExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Email verification failed: Token expired for user {Email}", user.Email);
            return false;
        }

        // Check if already verified
        if (user.Status == UserStatus.Active)
        {
            _logger.LogInformation("Email already verified for user {Email}", user.Email);
            return true; // Idempotent operation
        }

        // Activate account
        user.Status = UserStatus.Active;
        user.VerificationToken = null; // Clear token after use
        user.VerificationTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Email verified successfully for user {Email}, UserId {UserId}",
            user.Email, user.UserId);

        // Log email verification event (US_022, AC1)
        await _auditLogService.LogAuthEventAsync(
            userId: user.UserId,
            actionType: AuditActionType.EmailVerified,
            ipAddress: ipAddress,
            userAgent: userAgent,
            metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                email = user.Email,
                verifiedAt = DateTime.UtcNow
            }));

        return true;
    }

    /// <summary>
    /// Checks if email address is already registered.
    /// </summary>
    public async Task<bool> IsEmailRegisteredAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    /// <summary>
    /// Initiates password reset workflow by generating reset token and sending email.
    /// </summary>
    public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, string? ipAddress = null, string? userAgent = null)
    {
        var email = request.Email.ToLower().Trim();

        // Find user by email
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        // For security, always return success even if email not found (prevent email enumeration)
        if (user == null)
        {
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", email);
            
            // Log audit event for security tracking
            await _auditLogService.LogAuthEventAsync(
                userId: null,
                actionType: AuditActionType.PasswordResetAttempt,
                ipAddress: ipAddress,
                userAgent: userAgent,
                metadata: System.Text.Json.JsonSerializer.Serialize(new { 
                    email, 
                    description = "Password reset attempted for non-existent email" 
                }));

            // Return generic success message to prevent email enumeration
            return new ForgotPasswordResponseDto
            {
                Message = "If an account exists with this email, a password reset link will be sent.",
                Email = request.Email
            };
        }

        // Generate cryptographically secure reset token
        // Reuse VerificationToken field for password reset
        var resetToken = GenerateVerificationToken();

        // Update user with reset token and expiry (1 hour)
        user.VerificationToken = resetToken;
        user.VerificationTokenExpiry = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Password reset token generated for user {UserId}, Email: {Email}", 
            user.UserId, user.Email);

        // Log password reset initiation (audit trail)
        await _auditLogService.LogAuthEventAsync(
            userId: user.UserId,
            actionType: AuditActionType.PasswordResetRequested,
            ipAddress: ipAddress,
            userAgent: userAgent,
            metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                email = user.Email,
                tokenExpiry = user.VerificationTokenExpiry
            }));

        // Send password reset email
        var emailSent = await _emailService.SendPasswordResetEmailAsync(
            user.Email, 
            user.Name, 
            resetToken);

        if (!emailSent)
        {
            _logger.LogError("Failed to send password reset email to {Email}", user.Email);
        }

        return new ForgotPasswordResponseDto
        {
            Message = "If an account exists with this email, a password reset link will be sent.",
            Email = request.Email
        };
    }

    /// <summary>
    /// Resets user password using valid reset token.
    /// </summary>
    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request, string? ipAddress = null, string? userAgent = null)
    {
        // Find user by reset token (reusing VerificationToken field)
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.VerificationToken == request.Token);

        if (user == null)
        {
            _logger.LogWarning("Password reset attempted with invalid token");
            
            await _auditLogService.LogAuthEventAsync(
                userId: null,
                actionType: AuditActionType.PasswordResetFailed,
                ipAddress: ipAddress,
                userAgent: userAgent,
                metadata: System.Text.Json.JsonSerializer.Serialize(new { 
                    description = "Invalid password reset token" 
                }));

            throw new InvalidOperationException("Invalid or expired password reset token.");
        }

        // Check if token is expired
        if (user.VerificationTokenExpiry == null || user.VerificationTokenExpiry < DateTime.UtcNow)
        {
            _logger.LogWarning("Password reset attempted with expired token for user {UserId}", user.UserId);
            
            await _auditLogService.LogAuthEventAsync(
                userId: user.UserId,
                actionType: AuditActionType.PasswordResetFailed,
                ipAddress: ipAddress,
                userAgent: userAgent,
                metadata: System.Text.Json.JsonSerializer.Serialize(new
                {
                    reason = "Token expired",
                    tokenExpiry = user.VerificationTokenExpiry
                }));

            throw new InvalidOperationException("Password reset token has expired. Please request a new one.");
        }

        // Hash new password
        var newPasswordHash = _passwordHashingService.HashPassword(request.NewPassword);

        // Update password and clear reset token
        user.PasswordHash = newPasswordHash;
        user.VerificationToken = null;
        user.VerificationTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Password reset successfully for user {UserId}, Email: {Email}", 
            user.UserId, user.Email);

        // Log successful password reset
        await _auditLogService.LogAuthEventAsync(
            userId: user.UserId,
            actionType: AuditActionType.PasswordResetCompleted,
            ipAddress: ipAddress,
            userAgent: userAgent,
            metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                email = user.Email,
                success = true
            }));

        return true;
    }

    /// <summary>
    /// Authenticates user and generates JWT session token (FR-002, AC1).
    /// Implements account lockout after 5 failed attempts (AC4).
    /// </summary>
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null, string? userAgent = null)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var email = request.Email.ToLower().Trim();

        // Check if account is locked due to failed login attempts (AC4)
        var isLocked = await IsAccountLockedAsync(email);
        if (isLocked)
        {
            _logger.LogWarning("Login attempt for locked account: {Email}", email);

            // Log failed login for locked account (US_022, AC2)
            await _auditLogService.LogFailedLoginAsync(
                email: email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                failureReason: "Account locked due to multiple failed login attempts");

            throw new UnauthorizedAccessException("Account is locked due to multiple failed login attempts. Please try again later or contact support.");
        }

        // Retrieve user by email
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        // Use constant-time comparison for security (generic error message)
        if (user == null || !_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Increment failed login counter
            await IncrementFailedLoginAttemptsAsync(email);

            // Log failed login attempt (US_022, AC2)
            await _auditLogService.LogFailedLoginAsync(
                email: email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                failureReason: "Invalid email or password");

            _logger.LogWarning("Failed login attempt for email: {Email}", email);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Check account status
        if (user.Status == UserStatus.Pending)
        {
            _logger.LogWarning("Login attempt for unverified account: {Email}", email);

            await _auditLogService.LogFailedLoginAsync(
                email: email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                failureReason: "Account not verified");

            throw new UnauthorizedAccessException("Account not verified. Please check your email to verify your account.");
        }

        if (user.Status == UserStatus.Inactive)
        {
            _logger.LogWarning("Login attempt for inactive account: {Email}", email);

            await _auditLogService.LogFailedLoginAsync(
                email: email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                failureReason: "Account is inactive");

            throw new UnauthorizedAccessException("Account is inactive. Please contact support.");
        }

        if (user.Status == UserStatus.Locked)
        {
            _logger.LogWarning("Login attempt for locked account: {Email}", email);

            await _auditLogService.LogFailedLoginAsync(
                email: email,
                ipAddress: ipAddress,
                userAgent: userAgent,
                failureReason: "Account is locked");

            throw new UnauthorizedAccessException("Account is locked. Please contact support.");
        }

        // Reset failed login counter on successful authentication
        await ResetFailedLoginAttemptsAsync(email);

        // Generate JWT token with userId, email, and role claims (AC5)
        var token = _jwtTokenService.GenerateToken(user.UserId.ToString(), user.Email, user.Role.ToString());

        // Store session in Redis with 15-minute TTL (AC1) if Redis is available
        if (_sessionCacheService != null)
        {
            var sessionStored = await _sessionCacheService.SetSessionAsync(user.UserId.ToString(), token);

            if (!sessionStored)
            {
                _logger.LogWarning("Failed to store session in cache for user {UserId}. Continuing with token-only authentication.", user.UserId);
            }
        }
        else
        {
            _logger.LogDebug("Session cache service unavailable. Using token-only authentication for user {UserId}", user.UserId);
        }

        _logger.LogInformation("User logged in successfully: {Email}, UserId: {UserId}, Role: {Role}",
            user.Email, user.UserId, user.Role);

        // Log successful login event (US_022, AC1)
        await _auditLogService.LogAuthEventAsync(
            userId: user.UserId,
            actionType: AuditActionType.Login,
            ipAddress: ipAddress,
            userAgent: userAgent,
            metadata: System.Text.Json.JsonSerializer.Serialize(new
            {
                email = user.Email,
                role = user.Role.ToString(),
                loginTime = DateTime.UtcNow
            }));

        // Calculate token expiration (15 minutes from now per NFR-005)
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.UserId,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            ExpiresAt = expiresAt,
            Message = "Login successful"
        };
    }

    /// <summary>
    /// Checks if account is locked due to failed login attempts.
    /// Account is locked after 5 failed attempts for 15 minutes (AC4).
    /// </summary>
    private async Task<bool> IsAccountLockedAsync(string email)
    {
        try
        {
            // Check User.Status first (permanent lock)
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user?.Status == UserStatus.Locked)
            {
                return true;
            }

            // Check temporary lock in cache (failed login attempts) if cache is available
            if (_sessionCacheService != null)
            {
                var failedAttemptsKey = $"{FailedLoginKeyPrefix}{email}";
                var failedAttemptsStr = await _sessionCacheService.GetSessionAsync(failedAttemptsKey);

                if (int.TryParse(failedAttemptsStr, out var failedAttempts))
                {
                    return failedAttempts >= MaxFailedLoginAttempts;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking account lock status for {Email}", email);
            return false; // Fail open to avoid locking out legitimate users due to errors
        }
    }

    /// <summary>
    /// Increments failed login attempts counter.
    /// Locks account temporarily after 5 attempts (15-minute lockout).
    /// </summary>
    private async Task IncrementFailedLoginAttemptsAsync(string email)
    {
        if (_sessionCacheService == null)
        {
            _logger.LogDebug("Session cache unavailable. Failed login tracking disabled for {Email}", email);
            return;
        }

        try
        {
            var failedAttemptsKey = $"{FailedLoginKeyPrefix}{email}";
            var currentAttemptsStr = await _sessionCacheService.GetSessionAsync(failedAttemptsKey);

            var currentAttempts = int.TryParse(currentAttemptsStr, out var attempts) ? attempts : 0;
            var newAttempts = currentAttempts + 1;

            // Store failed attempts count with 15-minute TTL (lockout duration)
            await _sessionCacheService.SetSessionAsync(failedAttemptsKey, newAttempts.ToString());

            if (newAttempts >= MaxFailedLoginAttempts)
            {
                _logger.LogWarning("Account temporarily locked due to {Count} failed login attempts: {Email}",
                    newAttempts, email);
            }
            else
            {
                _logger.LogDebug("Failed login attempt {Count}/{Max} for {Email}",
                    newAttempts, MaxFailedLoginAttempts, email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing failed login attempts for {Email}", email);
        }
    }

    /// <summary>
    /// Resets failed login attempts counter on successful authentication.
    /// </summary>
    private async Task ResetFailedLoginAttemptsAsync(string email)
    {
        if (_sessionCacheService == null)
        {
            return;
        }

        try
        {
            var failedAttemptsKey = $"{FailedLoginKeyPrefix}{email}";
            await _sessionCacheService.RemoveSessionAsync(failedAttemptsKey);

            _logger.LogDebug("Failed login counter reset for {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting failed login attempts for {Email}", email);
        }
    }

    /// <summary>
    /// Generates cryptographically secure random verification token (32 bytes).
    /// Returns Base64-encoded string for URL-safe transmission.
    /// </summary>
    private static string GenerateVerificationToken()
    {
        var tokenBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }

    /// <summary>
    /// Refreshes session TTL in Redis for an authenticated user (US_022, AC5).
    /// </summary>
    public async Task<SessionRefreshResponseDto> RefreshSessionAsync(string userId, string? ipAddress = null, string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentNullException(nameof(userId));
        }

        if (_sessionCacheService != null)
        {
            var refreshed = await _sessionCacheService.RefreshSessionAsync(userId);

            if (!refreshed)
            {
                _logger.LogWarning("Session refresh failed for user {UserId}: session not found in cache", userId);
            }
            else
            {
                _logger.LogDebug("Session TTL refreshed for user {UserId}", userId);
            }
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        return new SessionRefreshResponseDto
        {
            ExpiresAt = expiresAt,
            Message = "Session refreshed successfully"
        };
    }

    /// <summary>
    /// Logs a session timeout event (US_022, AC3).
    /// Called by the frontend when auto-logout triggers due to inactivity.
    /// </summary>
    public async Task LogSessionTimeoutAsync(string userId, string? ipAddress = null, string? userAgent = null, DateTime? lastActivityTimestamp = null)
    {
        if (!Guid.TryParse(userId, out var userGuid))
        {
            _logger.LogWarning("Invalid userId format for session timeout logging: {UserId}", userId);
            return;
        }

        await _auditLogService.LogSessionTimeoutAsync(
            userGuid,
            ipAddress,
            userAgent,
            lastActivityTimestamp);

        // Clean up session from cache
        if (_sessionCacheService != null)
        {
            await _sessionCacheService.RemoveSessionAsync(userId);
        }
    }
}
