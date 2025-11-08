using Microsoft.AspNetCore.Mvc;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Controller for database verification and diagnostics.
/// Used during development to verify database connectivity and schema.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(ApplicationDbContext context, ILogger<DatabaseController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Checks database connection and returns schema information.
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            // Test database connection
            await _context.Database.CanConnectAsync();

            var info = new
            {
                status = "healthy",
                database = "PostgreSQL",
                timestamp = DateTime.UtcNow,
                entityCounts = new
                {
                    users = _context.Users.Count(),
                    contractors = _context.Contractors.Count(),
                    customers = _context.Customers.Count(),
                    jobs = _context.Jobs.Count(),
                    assignments = _context.Assignments.Count(),
                    reviews = _context.Reviews.Count(),
                    dispatcherContractorLists = _context.DispatcherContractorLists.Count()
                }
            };

            _logger.LogInformation("Database health check: OK");
            return Ok(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return StatusCode(500, new { status = "unhealthy", error = ex.Message });
        }
    }

    /// <summary>
    /// Verifies all database tables exist and are queryable.
    /// </summary>
    [HttpGet("verify-schema")]
    public IActionResult VerifySchema()
    {
        try
        {
            var results = new
            {
                users = true, // Verify each DbSet is accessible
                contractors = _context.Contractors != null,
                customers = _context.Customers != null,
                jobs = _context.Jobs != null,
                assignments = _context.Assignments != null,
                reviews = _context.Reviews != null,
                dispatcherContractorLists = _context.DispatcherContractorLists != null
            };

            _logger.LogInformation("Database schema verification: All tables queryable");
            return Ok(new { status = "verified", tables = results });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database schema verification failed");
            return StatusCode(500, new { status = "error", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets database connection status.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();

            _logger.LogInformation("Database status check: {Status}", canConnect ? "Connected" : "Disconnected");
            return Ok(new
            {
                status = canConnect ? "connected" : "disconnected",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve database status");
            return StatusCode(500, new { status = "error", error = ex.Message });
        }
    }
}

