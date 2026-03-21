using PatientAccess.Web.Extensions;
using PatientAccess.Web.Middleware;
using PatientAccess.Web.Filters;
using PatientAccess.Web.HealthChecks;
using PatientAccess.Business.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Swagger/OpenAPI (TR-005)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // OpenAPI 3.0 document metadata (AC-1, AC-2)
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Patient Access API",
        Version = "v1",
        Description = "Unified Patient Access & Clinical Intelligence Platform API - RESTful API for patient scheduling, clinical data management, and AI-powered healthcare intelligence.",
        Contact = new OpenApiContact
        {
            Name = "Patient Access Development Team",
            Email = "support@patientaccess.com"
        },
        License = new OpenApiLicense
        {
            Name = "Proprietary",
            Url = new Uri("https://patientaccess.com/license")
        }
    });

    // Add JWT Bearer authentication to Swagger (TR-012)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // Automatically add 401/403 responses to endpoints with [Authorize] attribute (AC-1)
    options.OperationFilter<SwaggerAuthorizationOperationFilter>();
});

// Configure CORS (TR-014)
var corsSettings = builder.Configuration.GetSection("CorsSettings");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .WithHeaders("Authorization", "Content-Type")
              .AllowCredentials();
    });
});

// Configure JWT Authentication (TR-012) - HS256 with symmetric secret key
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured. See docs/AUTHENTICATION.md for setup instructions.");

// Validate secret key length (minimum 256 bits / 32 characters for HS256)
if (secretKey.Length < 32)
{
    throw new InvalidOperationException(
        $"JWT SecretKey must be at least 32 characters (256 bits) for HS256 algorithm. " +
        $"Current length: {secretKey.Length} characters. See docs/AUTHENTICATION.md for key generation.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.FromMinutes(int.Parse(jwtSettings["ClockSkewMinutes"] ?? "5"))
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Configure Authorization Policies (NFR-006 RBAC)
builder.Services.AddAuthorization(options =>
{
    // Role-based policies for endpoint protection
    options.AddPolicy("PatientOnly", policy => policy.RequireRole("Patient"));
    options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Staff", "Admin"));
    
    // Fallback policy: all endpoints require authentication unless [AllowAnonymous]
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Register Business Layer Services (DI)
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHashingService, PasswordHashingService>();

// Register Audit Services (NFR-007, FR-005, NFR-014)
builder.Services.AddScoped<PatientAccess.Data.Repositories.IAuditLogRepository, PatientAccess.Data.Repositories.AuditLogRepository>();
builder.Services.AddScoped<PatientAccess.Business.Services.IAuditService, PatientAccess.Business.Services.AuditService>();

// Register Action Filters (NFR-014 minimum necessary access)
builder.Services.AddScoped<MinimumNecessaryAccessFilter>();

// Configure Rate Limiting (US_018 - Registration Endpoint Protection)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("registration", limiterOptions =>
    {
        limiterOptions.PermitLimit = 3; // 3 requests per window
        limiterOptions.Window = TimeSpan.FromMinutes(5); // 5-minute window
        limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // No queueing - immediate rejection
    });
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Configure Redis Connection (US_006 - Upstash Redis Configuration)
var redisSettings = builder.Configuration.GetSection("RedisSettings");
var redisEnabled = redisSettings.GetValue<bool>("Enabled", true);
var redisConnectionString = redisSettings["ConnectionString"];

if (redisEnabled && !string.IsNullOrWhiteSpace(redisConnectionString))
{
    try
    {
        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
        redisOptions.ConnectTimeout = redisSettings.GetValue<int>("ConnectTimeout", 5000);
        redisOptions.SyncTimeout = redisSettings.GetValue<int>("SyncTimeout", 5000);
        redisOptions.AbortOnConnectFail = redisSettings.GetValue<bool>("AbortOnConnectFail", false); // Graceful degradation per AG-001

        // Register Redis ConnectionMultiplexer as Singleton for connection pooling
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<Program>>();
            try
            {
                var connection = ConnectionMultiplexer.Connect(redisOptions);
                logger.LogInformation("Redis connection established successfully");
                return connection;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to connect to Redis. SessionCacheService will operate in fallback mode.");
                // Return null - SessionCacheService will handle null connection gracefully
                return null!;
            }
        });

        // Register SessionCacheService (US_006)
        builder.Services.AddSingleton<ISessionCacheService, SessionCacheService>();

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
        });

        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Redis session caching enabled. Session tokens will be cached with 15-minute TTL.");
    }
    catch (Exception ex)
    {
        var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Redis initialization failed. SessionCacheService will operate without Redis. Error: {ErrorMessage}", ex.Message);
        
        // Register null ConnectionMultiplexer for graceful degradation
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp => null!);
        builder.Services.AddSingleton<ISessionCacheService, SessionCacheService>();
    }
}
else
{
    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Redis caching disabled in configuration. SessionCacheService will operate without caching.");
    
    // Register null ConnectionMultiplexer when Redis is disabled
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp => null!);
    builder.Services.AddSingleton<ISessionCacheService, SessionCacheService>();
}

// Register Data Layer Repositories (DI)
builder.Services.AddScoped<PatientAccess.Data.Repositories.IUserRepository, PatientAccess.Data.Repositories.UserRepository>();

// Register User Management Services (US_018)
builder.Services.AddScoped<PatientAccess.Business.Services.IUserService, PatientAccess.Business.Services.UserService>();
builder.Services.AddScoped<PatientAccess.Business.Services.IEmailService, PatientAccess.Business.Services.EmailService>();

// Configure Database Context with Entity Framework Core
builder.Services.AddDbContext<PatientAccess.Data.PatientAccessDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            // Enable connection resiliency for transient failures
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
            
            // Set command timeout to match connection string
            npgsqlOptions.CommandTimeout(30);
        });
    
    // Enable detailed errors in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Configure Health Checks (TR-018, NFR-008)
var healthChecksBuilder = builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not configured"),
        name: "database",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "db", "postgresql" },
        timeout: TimeSpan.FromSeconds(5)); // AC-4: 5-second timeout

// Add Redis health check if enabled (US_006)
if (redisEnabled && !string.IsNullOrWhiteSpace(redisConnectionString))
{
    healthChecksBuilder.AddRedis(
        redisConnectionString,
        name: "redis",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded, // Degraded, not Unhealthy (graceful degradation)
        tags: new[] { "cache", "redis" },
        timeout: TimeSpan.FromSeconds(5));
}

var app = builder.Build();

// Verify database connectivity on startup (environment-aware)
// In Development: Log warning but continue; In Production: Fail-fast
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PatientAccess.Data.PatientAccessDbContext>();

    // Test database connection
    var canConnect = await dbContext.Database.CanConnectAsync();

    if (!canConnect)
    {
        if (app.Environment.IsDevelopment())
        {
            app.Logger.LogWarning("Database connection failed in Development environment. Application will continue but database features will not work. Check connection string in appsettings.Development.json.");
        }
        else
        {
            app.Logger.LogCritical("Database connection failed. Unable to connect to PostgreSQL database. Check connection string in appsettings.json or environment variables.");
            throw new InvalidOperationException("Database connection failed during startup validation.");
        }
    }
    else
    {
        app.Logger.LogInformation("Database connection verified successfully.");

        // Seed reference data in Development/Staging only (US_017)
        if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
        {
            try
            {
                await PatientAccess.Data.DatabaseSeeder.SeedAsync(dbContext, app.Logger);
            }
            catch (Exception seedEx)
            {
                app.Logger.LogWarning(seedEx, "Database seeding failed. Application will continue. Error: {ErrorMessage}", seedEx.Message);
            }
        }
    }
}
catch (Exception ex)
{
    if (app.Environment.IsDevelopment())
    {
        app.Logger.LogWarning(ex, "Database connection check failed in Development environment. Error: {ErrorMessage}. Application will continue but database features will not work.", ex.Message);
    }
    else
    {
        app.Logger.LogCritical(ex, "Failed to connect to database during startup. Application cannot proceed. Error: {ErrorMessage}", ex.Message);
        throw new InvalidOperationException(
            $"Database connection failed. Please verify your connection string and ensure the database is running. " +
            $"See docs/DATABASE_SETUP.md for troubleshooting. Error: {ex.Message}",
            ex);
    }
}

// Configure the HTTP request pipeline
// Swagger UI enabled in Development and Staging only (Edge Case: disabled in Production)
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Patient Access API v1");
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration(); // Show request duration
        options.EnableDeepLinking(); // Enable deep linking for sharing specific endpoints
        options.DisplayOperationId(); // Display operation IDs
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None); // Collapse all by default
    });
}

// Use global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Use JWT validation middleware for detailed error responses
app.UseJwtValidation();

// Use CORS (must be before authentication)
app.UseCors("DefaultCorsPolicy");

// Use rate limiting (US_018 - must be before authentication)
app.UseRateLimiter();

// Use authentication & authorization (TR-012)
app.UseAuthentication();
app.UseAuthorization();

// Use audit logging middleware (FR-005, NFR-007)
// MUST be after UseAuthentication to access authenticated user context
app.UseAuditLogging();

// Map health checks endpoint (TR-018, AC-3, AC-4)
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = CustomHealthCheckResponseWriter.WriteResponse,
    ResultStatusCodes =
    {
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = StatusCodes.Status200OK,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = StatusCodes.Status200OK,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

// Map controllers
app.MapControllers();

app.Run();
