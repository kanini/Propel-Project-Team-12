using PatientAccess.Web.Extensions;
using PatientAccess.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() 
    { 
        Title = "Patient Access API", 
        Version = "v1",
        Description = "Unified Patient Access & Clinical Intelligence Platform API"
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Register Business Layer Services (DI)
// Example: builder.Services.AddScoped<IAuthService, AuthService>();

// Register Data Layer Repositories (DI)
// Example: builder.Services.AddScoped<IUserRepository, UserRepository>();

// Configure Database Context with Entity Framework Core
builder.Services.AddDataAccessServices(builder.Configuration);

var app = builder.Build();

// Verify database connectivity on startup (fail-fast behavior)
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PatientAccess.Data.PatientAccessDbContext>();
    
    // Test database connection
    var canConnect = await dbContext.Database.CanConnectAsync();
    
    if (!canConnect)
    {
        app.Logger.LogCritical("Database connection failed. Unable to connect to PostgreSQL database. Check connection string in appsettings.json or environment variables.");
        throw new InvalidOperationException("Database connection failed during startup validation.");
    }
    
    app.Logger.LogInformation("Database connection verified successfully.");
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Failed to connect to database during startup. Application cannot proceed. Error: {ErrorMessage}", ex.Message);
    throw new InvalidOperationException(
        $"Database connection failed. Please verify your connection string and ensure the database is running. " +
        $"See docs/DATABASE_SETUP.md for troubleshooting. Error: {ex.Message}", 
        ex);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Patient Access API v1");
        options.RoutePrefix = "swagger";
    });
}

// Use global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Use CORS
app.UseCors("AllowFrontend");

// Use authentication & authorization (will be configured in future tasks)
// app.UseAuthentication();
// app.UseAuthorization();

// Map health checks endpoint
app.MapHealthChecks("/health");

// Map controllers
app.MapControllers();

app.Run();
