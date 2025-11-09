using Serilog;
using SmartScheduler.API.Middleware;
using SmartScheduler.Domain.Extensions;
using SmartScheduler.Application.Extensions;
using SmartScheduler.Infrastructure.Extensions;
using SmartScheduler.Infrastructure.Persistence;
using SmartScheduler.Infrastructure.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Allow string enum values to be deserialized (e.g., "Dispatcher" instead of 0)
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Configure CORS for local development and production
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000",      // Local development
            "http://localhost:5173",      // Vite default
            "http://127.0.0.1:3000",
            "http://127.0.0.1:5173"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Load configuration with fallbacks (environment variables take precedence, then appsettings)
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] 
    ?? builder.Configuration["Jwt__SecretKey"]
    ?? throw new InvalidOperationException("JWT secret key is not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? builder.Configuration["Jwt__Issuer"]
    ?? "SmartScheduler"; // Default for development
var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? builder.Configuration["Jwt__Audience"]
    ?? "SmartSchedulerAPI"; // Default for development

// Build database connection string from environment variables or appsettings
string connectionString;
if (builder.Environment.IsDevelopment())
{
    // In development, prefer appsettings.Development.json connection string
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection is not configured in appsettings.Development.json");
}
else
{
    // In production, use environment variables
    var dbHost = builder.Configuration["DATABASE_HOST"] 
        ?? throw new InvalidOperationException("DATABASE_HOST is not configured");
    var dbPort = builder.Configuration["DATABASE_PORT"] ?? "5432";
    var dbName = builder.Configuration["DATABASE_NAME"] ?? "smartscheduler";
    var dbUser = builder.Configuration["DATABASE_USER"] 
        ?? throw new InvalidOperationException("DATABASE_USER is not configured");
    var dbPassword = builder.Configuration["DATABASE_PASSWORD"] 
        ?? throw new InvalidOperationException("DATABASE_PASSWORD is not configured");
    
    // Build connection string - SSL Mode=Prefer (tries SSL, falls back to unencrypted for portfolio app)
    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};SSL Mode=Prefer;Trust Server Certificate=true;";
}

// Update connection string in configuration
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    { "ConnectionStrings:DefaultConnection", connectionString }
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure Authorization Policies for RBAC
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DispatcherOnly", policy =>
        policy.RequireRole("Dispatcher"));
    
    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole("Customer"));
    
    options.AddPolicy("ContractorOnly", policy =>
        policy.RequireRole("Contractor"));
    
    options.AddPolicy("AnyAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

// Register layer services
builder.Services.AddDomainServices();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Run database migrations on startup (all environments)
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    Log.Information("Testing database connectivity...");
    // Test connection first
    var canConnect = await context.Database.CanConnectAsync();
    if (!canConnect)
    {
        Log.Error("Cannot connect to database. Check connection string and database availability.");
        throw new InvalidOperationException("Database connection failed");
    }
    
    Log.Information("Running database migrations...");
    await context.Database.MigrateAsync();
    Log.Information("Database migrations completed successfully");

    // Seed database in development only
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Seeding database with initial data...");
        DatabaseSeeder.Seed(context);
        Log.Information("Database seeding completed successfully");
    }
}
catch (Exception ex)
{
    Log.Error(ex, "Database migration failed - application startup halted. This is a critical error.");
    // Re-throw in production, continue in development for debugging
    if (!app.Environment.IsDevelopment())
    {
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception handling middleware (must be first)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Health check endpoint (before auth middleware so it's publicly accessible)
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithDescription("Health check endpoint")
    .AllowAnonymous();

// Enable CORS (must be before UseAuthentication)
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/notifications");

app.Run();
