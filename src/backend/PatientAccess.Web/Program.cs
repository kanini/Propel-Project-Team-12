using PatientAccess.Web.Extensions;
using PatientAccess.Web.Middleware;
using PatientAccess.Web.Filters;
using PatientAccess.Web.HealthChecks;
using PatientAccess.Business.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography;

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
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: 'Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...'",
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

// Configure JWT Authentication (TR-012)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var publicKeyPath = jwtSettings["PublicKeyPath"] ?? throw new InvalidOperationException("JWT PublicKeyPath not configured.");

if (!File.Exists(publicKeyPath))
{
    throw new FileNotFoundException(
        $"RSA public key file not found at {publicKeyPath}. " +
        "Please generate RS256 key pair. See docs/AUTHENTICATION.md for instructions.",
        publicKeyPath);
}

var rsaPublic = RSA.Create();
rsaPublic.FromXmlString(File.ReadAllText(publicKeyPath));

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
        IssuerSigningKey = new RsaSecurityKey(rsaPublic),
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

builder.Services.AddAuthorization();

// Register Business Layer Services (DI)
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHashingService, PasswordHashingService>();

// Register Data Layer Repositories (DI)
// Example: builder.Services.AddScoped<IUserRepository, UserRepository>();

// Configure Database Context with Entity Framework Core
builder.Services.AddDataAccessServices(builder.Configuration);

// Configure Health Checks (TR-018, NFR-008)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString ?? throw new InvalidOperationException("DefaultConnection not configured"),
        name: "database",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "db", "postgresql" },
        timeout: TimeSpan.FromSeconds(5)) // AC-4: 5-second timeout
                                          // Redis health check will be added when Redis is configured (US_006)
    ;

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

// Use authentication & authorization (TR-012)
app.UseAuthentication();
app.UseAuthorization();

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
