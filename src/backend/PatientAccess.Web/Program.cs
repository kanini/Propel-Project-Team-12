using System.Text.Json.Serialization;
using PatientAccess.Web.Extensions;
using PatientAccess.Web.Middleware;
using PatientAccess.Web.Filters;
using PatientAccess.Web.HealthChecks;
using PatientAccess.Web.Authorization;
using PatientAccess.Business.Services;
using PatientAccess.Business.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using Hangfire;
using Hangfire.PostgreSql;
using Npgsql;
using Pgvector.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load .env file for local development (secrets not committed to git)
var envFile = Path.Combine(builder.Environment.ContentRootPath, ".env");
if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
        var idx = trimmed.IndexOf('=');
        if (idx <= 0) continue;
        var key = trimmed[..idx].Trim();
        var value = trimmed[(idx + 1)..].Trim();
        Environment.SetEnvironmentVariable(key, value);
        // Also set in configuration directly (env vars set after builder init aren't picked up)
        var configKey = key.Replace("__", ":");
        builder.Configuration[configKey] = value;
    }

    // Inject DB_PASSWORD into connection string if provided
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    if (!string.IsNullOrEmpty(dbPassword))
    {
        var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
        connStr = connStr.Replace("Password=SET_VIA_ENV", $"Password={dbPassword}");
        builder.Configuration["ConnectionStrings:DefaultConnection"] = connStr;
    }
}

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure camelCase naming for JSON serialization/deserialization (standard for web APIs)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // Enable case-insensitive property matching for deserialization
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        // Allow string-to-enum conversion for JSON payloads
        // Use null naming policy to keep enum values as PascalCase (e.g., "Admin" -> UserRole.Admin)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: true));
    });

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
        Description = "JWT Authorization header using the Bearer scheme. Enter your JWT token in the text input below (without 'Bearer' prefix - it will be added automatically).\n\nExample: eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, // ✅ Changed from ApiKey to Http
        Scheme = "bearer", // ✅ Must be lowercase for HTTP scheme
        BearerFormat = "JWT"
    });

    // Add global security requirement (makes "Authorize" button work in Swagger UI)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
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
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure JWT Authentication (TR-012) - RS256 with RSA asymmetric keys
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var publicKeyPath = jwtSettings["PublicKeyPath"]
    ?? Path.Combine(AppContext.BaseDirectory, "rsa-keys", "public-key.xml");

// Load public key for token validation
if (!File.Exists(publicKeyPath))
{
    throw new InvalidOperationException(
        $"JWT public key not found at {publicKeyPath} (absolute: {Path.GetFullPath(publicKeyPath)}). " +
        "Run 'powershell -ExecutionPolicy Bypass -File scripts/GenerateRsaKeys.ps1' to generate RSA key pair. " +
        "See docs/AUTHENTICATION.md for setup instructions.");
}

var publicKeyXml = File.ReadAllText(publicKeyPath);
var rsa = System.Security.Cryptography.RSA.Create();
rsa.FromXmlString(publicKeyXml);
var validationKey = new RsaSecurityKey(rsa) { KeyId = "patient-access-rsa-key-1" };

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
        IssuerSigningKey = validationKey, // Use RSA public key for validation
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

// Configure Role-Based Access Control (RBAC) - US_020
builder.Services.AddAuthorization(options =>
{
    // Admin-only policy - requires Admin role (AC2)
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    // Staff and Admin policy - staff endpoints accessible by both Staff and Admin
    options.AddPolicy("StaffOnly", policy =>
        policy.RequireRole("Staff", "Admin"));

    // Patient-only policy - requires Patient role
    options.AddPolicy("PatientOnly", policy =>
        policy.RequireRole("Patient"));

    // Same patient data access policy - prevents cross-patient access (AC3)
    options.AddPolicy("SamePatient", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new SamePatientRequirement());
    });

    // Authenticated user policy - any authenticated user
    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// Register authorization handlers for RBAC (US_020)
builder.Services.AddSingleton<IAuthorizationHandler, SamePatientAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuditingAuthorizationHandler>();

// Register HttpClient for external API calls
builder.Services.AddHttpClient();

// Register Business Layer Services (DI)
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<PatientAccess.Business.Interfaces.ISmsService, PatientAccess.Business.Services.SmsService>(); // US_037 - SMS reminder delivery via Twilio
builder.Services.AddScoped<IAuditLogService, AuditLogService>(); // US_022 - Audit logging for authentication events
builder.Services.AddScoped<IAdminService, AdminService>(); // US_021 - User management
builder.Services.AddScoped<IPatientService, PatientService>(); // US_029 - Walk-in booking (patient search and minimal creation)
builder.Services.AddScoped<IProviderService, ProviderService>(); // US_023 - Provider browser
builder.Services.AddScoped<IAppointmentService, AppointmentService>(); // US_024 - Appointment booking
builder.Services.AddScoped<INoShowRiskService, NoShowRiskService>(); // US_038 - No-show risk scoring
builder.Services.AddScoped<IWaitlistService, WaitlistService>(); // US_025 - Waitlist enrollment
builder.Services.AddScoped<IWaitlistNotificationService, WaitlistNotificationService>(); // US_041 - Waitlist slot availability notifications
builder.Services.AddScoped<PatientAccess.Business.BackgroundJobs.WaitlistSlotDetectionJob>(); // US_041 - Slot detection job
builder.Services.AddScoped<PatientAccess.Business.BackgroundJobs.WaitlistTimeoutJob>(); // US_041 - Timeout processing job
builder.Services.AddScoped<ISlotSwapService, SlotSwapService>(); // US_026 - Dynamic preferred slot swap
builder.Services.AddScoped<PatientAccess.Business.BackgroundJobs.SlotAvailabilityMonitor>(); // US_026 - Slot swap monitoring
builder.Services.AddScoped<IPdfGenerationService, PdfGenerationService>(); // US_028 - PDF generation
builder.Services.AddScoped<PatientAccess.Business.BackgroundJobs.ConfirmationEmailJob>(); // US_028 - Confirmation email job
builder.Services.AddSingleton<IPusherService, PusherService>(); // US_030 - Real-time event broadcasting via Pusher Channels
builder.Services.AddScoped<IQueueManagementService, QueueManagementService>(); // US_030 - Queue management and priority flagging
builder.Services.AddScoped<IArrivalManagementService, ArrivalManagementService>(); // US_031 - Arrival status marking and search
builder.Services.AddScoped<IDashboardService, DashboardService>(); // US_067 - Patient dashboard statistics
builder.Services.AddScoped<IStaffDashboardService, StaffDashboardService>(); // US_068 - Staff dashboard metrics and queue preview
builder.Services.AddScoped<INotificationService, NotificationService>(); // US_067 - Notification management for dashboard
builder.Services.AddScoped<IDocumentService, DocumentService>(); // US_067 - Clinical document retrieval for dashboard
builder.Services.AddScoped<IIntakeAppointmentService, IntakeAppointmentService>(); // US_037 - Intake appointment selection
builder.Services.AddScoped<IIntakeService, IntakeService>(); // US_033 - Intake session management
builder.Services.AddScoped<IAiIntakeService, StubAiIntakeService>(); // US_033 - AI intake (stub until task_003)
builder.Services.AddScoped<IInsurancePrecheckService, InsurancePrecheckService>(); // US_036 - Insurance precheck verification

// US_037 - Reminder scheduling and delivery services
builder.Services.AddScoped<IReminderService, ReminderService>(); // US_037 - Reminder scheduling engine
builder.Services.AddScoped<PatientAccess.Business.BackgroundJobs.ReminderSchedulerJob>(); // US_037 - Recurring job scanning due reminders
builder.Services.AddScoped<PatientAccess.Business.BackgroundJobs.ReminderDeliveryJob>(); // US_037 - Delivery job with exponential backoff

// US_042 - Document upload services (chunked upload with real-time progress)
builder.Services.AddMemoryCache(); // Required for upload session tracking
builder.Services.AddSingleton<ChunkedUploadManager>(); // Singleton for session management
builder.Services.AddScoped<DocumentUploadService>(); // Scoped for DB context access
builder.Services.AddScoped<PatientAccess.Business.BackgroundJobs.UploadSessionCleanupJob>(); // Background cleanup

// US_043 - Document processing services (Hangfire background jobs)
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>(); // Processing orchestration
builder.Services.AddScoped<PatientAccess.Business.BackgroundJobs.DocumentProcessingJob>(); // Background processing job

// US_045 - AI Document Intelligence (task_002: Supabase + Tesseract + Gemini pipeline)
builder.Services.AddHttpClient(); // Required for Gemini API calls and Supabase Storage REST API

builder.Services.AddScoped<ISupabaseStorageService, SupabaseStorageService>(); // Supabase Storage REST API

#pragma warning disable CA1416 // Validate platform compatibility - TesseractOcrService is Windows-only
builder.Services.AddScoped<ITesseractOcrService, TesseractOcrService>(); // OCR text extraction (Windows-only)
#pragma warning restore CA1416

builder.Services.AddScoped<IGeminiAiService, GeminiAiService>(); // Gemini AI data extraction
builder.Services.AddScoped<IClinicalDataExtractionService, ClinicalDataExtractionService>(); // Extraction orchestration

// RAG Pipeline services (EP006-EP008: Document → Chunk → Embed → pgvector → Cosine Search → LLM)
builder.Services.AddScoped<IPdfTextExtractionService, PdfTextExtractionService>(); // PDF text extraction via PdfPig
builder.Services.AddSingleton<ITextChunkingService, TextChunkingService>(); // Text chunking (512 tokens, 12.5% overlap)
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>(); // Gemini text-embedding-004
builder.Services.AddScoped<IVectorSearchService, VectorSearchService>(); // pgvector cosine similarity search

// Health Dashboard & Clinical Verification (SCR-016, SCR-023)
builder.Services.AddScoped<IHealthDashboardService, HealthDashboardService>(); // 360° patient health view
builder.Services.AddScoped<IClinicalVerificationService, ClinicalVerificationService>(); // Staff verification actions

// Register IHttpContextAccessor for audit logging context extraction
builder.Services.AddHttpContextAccessor();

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
                logger.LogWarning(ex, "Failed to connect to Redis. Application will use database-only fallback mode.");
                // Return a null object pattern or disconnected multiplexer
                // For now, throw to prevent registration - application will fall back to database
                throw;
            }
        });

        // Register IDistributedCache for dashboard and notification services (US_067)
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = redisSettings.GetValue<string>("InstanceName", "PatientAccess:");
        });

        // Register SessionCacheService (US_006)
        builder.Services.AddSingleton<ISessionCacheService, SessionCacheService>();

        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
        });

        Console.WriteLine("Redis session caching enabled. Session tokens will be cached with 15-minute TTL.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Redis initialization failed. Application will use database-only session management. Error: {ex.Message}");
        // Continue without Redis - application will fall back to database session storage
    }
}
else
{
    Console.WriteLine("Redis caching disabled in configuration. Application will use in-memory distributed cache fallback.");

    // Register in-memory distributed cache as fallback (US_067)
    builder.Services.AddDistributedMemoryCache();
}

// Register Data Layer Repositories (DI)
// Example: builder.Services.AddScoped<IUserRepository, UserRepository>();

// Configure Database Context with Entity Framework Core + pgvector support
var mainConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var pgDataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(mainConnectionString);
pgDataSourceBuilder.UseVector();
var pgDataSource = pgDataSourceBuilder.Build();

builder.Services.AddDbContext<PatientAccess.Data.PatientAccessDbContext>(options =>
{
    options.UseNpgsql(
        pgDataSource,
        npgsqlOptions =>
        {
            npgsqlOptions.UseVector();

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

// Configure Hangfire for background job processing (US_028 - FR-012)
// Use a separate connection string with a small pool to avoid exhausting Supabase's session-mode connection limit
var hangfireConnectionString = new NpgsqlConnectionStringBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection"))
{
    MaxPoolSize = 5,
    MinPoolSize = 1
}.ConnectionString;

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(hangfireConnectionString);
    }));

// Add Hangfire server with reduced worker count to stay within Supabase connection limits
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 2;
    options.Queues = new[] { "default", "document-processing" };
    options.SchedulePollingInterval = TimeSpan.FromSeconds(30);
});

// Configure Health Checks (TR-018, NFR-008)
var healthChecksBuilder = builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not configured"),
        name: "database",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "db", "postgresql" },
        timeout: TimeSpan.FromSeconds(5)) // AC-4: 5-second timeout
    .AddCheck<HangfireHealthCheck>(
        "hangfire",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "hangfire", "backgroundjobs" },
        timeout: TimeSpan.FromSeconds(5)) // US_043: Hangfire server health
    .AddCheck<DocumentProcessingHealthCheck>(
        "document-processing",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
        tags: new[] { "documents", "processing" },
        timeout: TimeSpan.FromSeconds(5)); // US_043: Document processing backlog health

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

    // Hangfire Dashboard (US_028, US_043 - Development only for monitoring background jobs)
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireDashboardAuthorizationFilter(app.Environment) }, // US_043: Dev=open, Prod=Admin-only
        StatsPollingInterval = 2000, // Poll every 2 seconds
        DisplayStorageConnectionString = false // Hide connection string for security
    });

    // Schedule recurring background jobs
    using (var scope = app.Services.CreateScope())
    {
        // Schedule upload session cleanup job (US_042) - runs every 30 minutes
        PatientAccess.Business.BackgroundJobs.UploadSessionCleanupJob.Schedule();
        app.Logger.LogInformation("Scheduled upload session cleanup job to run every 30 minutes");

        // US_037 - Schedule reminder scheduler job - runs every 30 seconds (NFR-017: 30-second delivery window)
        RecurringJob.AddOrUpdate<PatientAccess.Business.BackgroundJobs.ReminderSchedulerJob>(
            "reminder-scheduler",
            job => job.RunAsync(),
            "*/30 * * * * *", // Every 30 seconds (cron with seconds: sec min hour day month dow)
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
        app.Logger.LogInformation("Scheduled reminder scheduler job to run every 30 seconds");

        // US_041 - Schedule waitlist slot detection job - runs every 2 minutes
        RecurringJob.AddOrUpdate<PatientAccess.Business.BackgroundJobs.WaitlistSlotDetectionJob>(
            "waitlist-slot-detection",
            job => job.RunAsync(),
            "*/2 * * * *", // Every 2 minutes
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
        app.Logger.LogInformation("Scheduled waitlist slot detection job to run every 2 minutes");

        // US_041 - Schedule waitlist timeout processing job - runs every 1 minute
        RecurringJob.AddOrUpdate<PatientAccess.Business.BackgroundJobs.WaitlistTimeoutJob>(
            "waitlist-timeout-processing",
            job => job.RunAsync(),
            "* * * * *", // Every 1 minute
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
        app.Logger.LogInformation("Scheduled waitlist timeout processing job to run every 1 minute");
    }
}

// Use audit logging middleware early to capture IP and User Agent for all requests (US_022)
app.UseMiddleware<AuditLoggingMiddleware>();

// Use rate limiting middleware for registration endpoint (FR-001)
app.UseMiddleware<RegistrationRateLimitingMiddleware>();

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
