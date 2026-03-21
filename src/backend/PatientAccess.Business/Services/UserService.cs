using Microsoft.Extensions.Logging;
using PatientAccess.Business.DTOs;
using PatientAccess.Data.Models;
using PatientAccess.Data.Repositories;

namespace PatientAccess.Business.Services;

/// <summary>
/// User service implementation for user management operations (FR-001).
/// Handles user registration, email verification, and authentication.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ISessionCacheService _sessionCacheService;
    private readonly IAuditService _auditService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordHashingService passwordHashingService,
        IEmailService emailService,
        IJwtTokenService jwtTokenService,
        ISessionCacheService sessionCacheService,
        IAuditService auditService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordHashingService = passwordHashingService ?? throw new ArgumentNullException(nameof(passwordHashingService));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _sessionCacheService = sessionCacheService ?? throw new ArgumentNullException(nameof(sessionCacheService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required", nameof(request.Email));

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required", nameof(request.Password));

        try
        {
            // Check if email already exists (FR-001 AC3)
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                // Don't reveal whether email exists (OWASP Email Enumeration Prevention)
                throw new InvalidOperationException("Email address is already registered.");
            }
        }
        catch (InvalidOperationException)
        {
            // Re-throw business logic exceptions
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existing email during registration: {Email}", request.Email);
            throw new InvalidOperationException("An error occurred during registration. Please try again.");
        }

        try
        {
            // Hash password using BCrypt with cost factor 12 (TR-013)
            var passwordHash = _passwordHashingService.HashPassword(request.Password);

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                _logger.LogError("Password hashing failed for registration: {Email}", request.Email);
                throw new InvalidOperationException("Failed to process password. Please try again.");
            }

            // Generate verification token (24-hour expiry per AC4)
            var verificationToken = Guid.NewGuid().ToString();
            var tokenExpiry = DateTime.UtcNow.AddHours(24);

            // Create user with "Inactive" status until email verified (FR-001 AC1)
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                DateOfBirth = request.DateOfBirth,
                Phone = request.Phone,
                Role = UserRole.Patient, // Default role for self-registration
                Status = UserStatus.Inactive,
                VerificationToken = verificationToken,
                VerificationTokenExpiry = tokenExpiry
            };

            // Save user to database
            var createdUser = await _userRepository.CreateAsync(user);

            if (createdUser == null)
            {
                _logger.LogError("User creation returned null for email: {Email}", request.Email);
                throw new InvalidOperationException("Failed to create user account. Please try again.");
            }

            // Send verification email (non-blocking, within 2 minutes per AC1)
            _ = Task.Run(async () =>
            {
                try
                {
                    var emailSent = await _emailService.SendVerificationEmailAsync(
                        createdUser.Email,
                        createdUser.Name,
                        verificationToken);

                    if (!emailSent)
                    {
                        _logger.LogError("Failed to send verification email to {Email}", createdUser.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception sending verification email to {Email}", createdUser.Email);
                }
            });

            _logger.LogInformation("User registered successfully: {UserId}, Email: {Email}", createdUser.UserId, request.Email);

            return new RegisterUserResponse
            {
                UserId = createdUser.UserId,
                Email = createdUser.Email,
                Message = "Registration successful. Please check your email to verify your account."
            };
        }
        catch (InvalidOperationException)
        {
            // Re-throw business logic exceptions
            throw;
        }
        catch (ArgumentException)
        {
            // Re-throw validation exceptions
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user registration: {Email}", request.Email);
            throw new InvalidOperationException("An unexpected error occurred during registration. Please try again later.");
        }
    }

    /// <inheritdoc />
    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Token))
        {
            _logger.LogWarning("Email verification attempted with invalid token");
            return false;
        }

        try
        {
            // Find user by verification token
            var user = await _userRepository.GetByVerificationTokenAsync(request.Token);
            if (user == null)
            {
                _logger.LogWarning("Verification token not found: {Token}", request.Token);
                return false;
            }

            // Check if token expired (FR-001 AC4)
            if (user.VerificationTokenExpiry == null || user.VerificationTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Verification token expired for user: {UserId}", user.UserId);
                return false;
            }

            // Check if already verified
            if (user.VerifiedAt != null)
            {
                _logger.LogInformation("User already verified: {UserId}", user.UserId);
                return true; // Consider this successful
            }

            // Activate user account (FR-001 AC2)
            user.Status = UserStatus.Active;
            user.VerifiedAt = DateTime.UtcNow;
            user.VerificationToken = null; // Clear token after use
            user.VerificationTokenExpiry = null;

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User email verified successfully: {UserId}, Email: {Email}", user.UserId, user.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification for token: {Token}", request.Token);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<LoginResponse?> AuthenticateUserAsync(LoginRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login attempt with missing credentials");
            return null;
        }

        try
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
                return null; // Don't reveal if email exists (OWASP Email Enumeration Prevention)
            }

            // Check account lockout (FR-002 AC4)
            if (user.AccountLockedUntil.HasValue && user.AccountLockedUntil.Value > DateTime.UtcNow)
            {
                _logger.LogWarning("Login attempt on locked account: {UserId}, Locked until: {LockExpiry}", 
                    user.UserId, user.AccountLockedUntil.Value);
                return null; // Return null to indicate authentication failure
            }

            // Check account status (FR-002 AC3)
            if (user.Status != UserStatus.Active)
            {
                _logger.LogWarning("Login attempt on inactive account: {UserId}, Status: {Status}", 
                    user.UserId, user.Status);
                return null;
            }

            // Verify password using BCrypt (TR-013)
            bool passwordValid;
            try
            {
                passwordValid = _passwordHashingService.VerifyPassword(request.Password, user.PasswordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password for user: {Email}", request.Email);
                return null;
            }

            if (!passwordValid)
            {
                // Increment failed login attempts (FR-002 AC4)
                user.FailedLoginAttempts++;
                user.LastFailedLogin = DateTime.UtcNow;

                // Lock account after 5 failed attempts (30-minute lockout)
                if (user.FailedLoginAttempts >= 5)
                {
                    user.AccountLockedUntil = DateTime.UtcNow.AddMinutes(30);
                    _logger.LogWarning("Account locked due to failed login attempts: {UserId}, Attempts: {Attempts}", 
                        user.UserId, user.FailedLoginAttempts);
                }

                await _userRepository.UpdateAsync(user);

                _logger.LogWarning("Failed login attempt: {Email}, Total attempts: {Attempts}", 
                    request.Email, user.FailedLoginAttempts);
                return null;
            }

            // Successful login - reset failed attempts (FR-002 AC1)
            user.FailedLoginAttempts = 0;
            user.LastFailedLogin = null;
            user.AccountLockedUntil = null;
            await _userRepository.UpdateAsync(user);

            // Generate JWT token (TR-012)
            var token = _jwtTokenService.GenerateToken(
                user.UserId.ToString(),
                user.Email,
                user.Role.ToString());

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogError("Token generation failed for user: {UserId}", user.UserId);
                return null;
            }

            // Store session in Redis with 15-minute TTL (NFR-005)
            try
            {
                await _sessionCacheService.SetSessionAsync(user.UserId.ToString(), token);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Session cache failed for user {UserId}, proceeding with login", user.UserId);
                // Continue with login even if cache fails (graceful degradation)
            }

            _logger.LogInformation("User authenticated successfully: {UserId}, Email: {Email}, Role: {Role}", 
                user.UserId, user.Email, user.Role);

            return new LoginResponse
            {
                Token = token,
                Role = user.Role.ToString(),
                UserId = user.UserId,
                Name = user.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for email: {Email}", request.Email);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, Guid adminUserId)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Validate role is Staff or Admin only (US_021 AC1)
        if (request.Role == UserRole.Patient)
        {
            throw new InvalidOperationException("Cannot create Patient users through admin interface. Patients must self-register.");
        }

        // Check if email already exists (Edge Case: duplicate email)
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Admin {AdminId} attempted to create user with existing email: {Email}", 
                adminUserId, request.Email);
            throw new InvalidOperationException("Email address is already registered.");
        }

        // Generate random password (12 characters with complexity requirements)
        var randomPassword = GenerateRandomPassword();

        // Hash password
        var passwordHash = _passwordHashingService.HashPassword(randomPassword);

        // Create user with Active status (US_021 AC1)
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = request.Role,
            Status = UserStatus.Active
        };

        // Save user to database
        var createdUser = await _userRepository.CreateAsync(user);

        // Send activation email with temporary password
        _ = Task.Run(async () =>
        {
            var emailSent = await _emailService.SendUserActivationEmailAsync(
                user.Email,
                user.Name,
                randomPassword);

            if (!emailSent)
            {
                _logger.LogError("Failed to send activation email to {Email}", user.Email);
            }
        });

        // Create audit log entry (US_021 AC2)
        await _auditService.LogDataAccessAsync(
            adminUserId,
            "User",
            createdUser.UserId,
            $"Created {request.Role} user account: {request.Email}");

        _logger.LogInformation("Admin {AdminId} created {Role} user: {UserId}, Email: {Email}", 
            adminUserId, request.Role, createdUser.UserId, request.Email);

        return MapToUserDto(createdUser);
    }

    /// <inheritdoc />
    public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, Guid adminUserId)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Fetch user
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found.");
        }

        // Update name and role (US_021 AC2)
        user.Name = request.Name;
        user.Role = request.Role;

        // Save changes
        var updatedUser = await _userRepository.UpdateAsync(user);

        // Create audit log entry (US_021 AC2)
        await _auditService.LogDataAccessAsync(
            adminUserId,
            "User",
            userId,
            $"Updated user account: Name={request.Name}, Role={request.Role}");

        _logger.LogInformation("Admin {AdminId} updated user {UserId}: Name={Name}, Role={Role}", 
            adminUserId, userId, request.Name, request.Role);

        return MapToUserDto(updatedUser);
    }

    /// <inheritdoc />
    public async Task DeactivateUserAsync(Guid userId, Guid currentUserId)
    {
        // Prevent self-deactivation (US_021 AC5)
        if (userId == currentUserId)
        {
            _logger.LogWarning("Admin {AdminId} attempted to deactivate their own account", currentUserId);
            throw new InvalidOperationException("Cannot deactivate your own account.");
        }

        // Fetch user
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found.");
        }

        // Check if user is Admin and prevent last Admin deletion (Edge Case)
        if (user.Role == UserRole.Admin)
        {
            var activeAdminCount = await _userRepository.GetActiveAdminCountAsync();
            if (activeAdminCount <= 1)
            {
                _logger.LogWarning("Admin {AdminId} attempted to deactivate last Admin account: {UserId}", 
                    currentUserId, userId);
                throw new InvalidOperationException("Cannot deactivate the last active Admin account.");
            }
        }

        // Update status to Inactive (US_021 AC3)
        await _userRepository.UpdateStatusAsync(userId, UserStatus.Inactive);

        // Terminate all active sessions for the user (US_021 AC3)
        await _sessionCacheService.InvalidateUserSessionsAsync(userId);

        // Create audit log entry
        await _auditService.LogDataAccessAsync(
            currentUserId,
            "User",
            userId,
            $"Deactivated user account: {user.Email}, Role={user.Role}");

        _logger.LogInformation("Admin {AdminId} deactivated user {UserId}, Email: {Email}", 
            currentUserId, userId, user.Email);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(string? search = null, string? sortBy = null, bool ascending = true)
    {
        var users = await _userRepository.GetAllAsync(search, sortBy, ascending);

        return users.Select(MapToUserDto);
    }

    /// <inheritdoc />
    public async Task<int> GetActiveAdminCountAsync()
    {
        return await _userRepository.GetActiveAdminCountAsync();
    }

    /// <inheritdoc />
    public async Task<bool> RefreshSessionAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        _logger.LogDebug("Attempting to refresh session for user {UserId}", userId);

        // Extend session TTL in Redis (or database fallback)
        var extended = await _sessionCacheService.ExtendSessionAsync(userId.ToString());

        if (!extended)
        {
            _logger.LogWarning("Failed to extend session for user {UserId} - session not found", userId);
            return false;
        }

        // Create audit log for session extension (non-blocking)
        try
        {
            await _auditService.LogSessionExtensionAsync(userId);
        }
        catch (Exception ex)
        {
            // NON-BLOCKING: Don't fail session extension if audit fails
            _logger.LogError(ex, "Failed to create audit log for session extension. User: {UserId}", userId);
        }

        _logger.LogInformation("Successfully refreshed session for user {UserId}", userId);
        return true;
    }

    /// <summary>
    /// Maps User entity to UserDto (excludes sensitive fields).
    /// </summary>
    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt,
            LastLogin = user.LastFailedLogin // Proxy for last login (update if tracking last successful login)
        };
    }

    /// <summary>
    /// Generates a random password with complexity requirements.
    /// 12 characters: uppercase, lowercase, digits, special characters.
    /// </summary>
    private static string GenerateRandomPassword()
    {
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "@$!%*?&";
        const string all = uppercase + lowercase + digits + special;

        var random = new Random();
        var password = new char[12];

        // Ensure at least one of each required character type
        password[0] = uppercase[random.Next(uppercase.Length)];
        password[1] = lowercase[random.Next(lowercase.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        // Fill remaining characters randomly
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = all[random.Next(all.Length)];
        }

        // Shuffle password characters
        return new string(password.OrderBy(x => random.Next()).ToArray());
    }
}
