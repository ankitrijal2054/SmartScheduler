using Microsoft.AspNetCore.Mvc;
using SmartScheduler.API.Extensions;
using SmartScheduler.Application.Responses;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Health check controller to verify API is running and clean architecture is working.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly ILogger<HealthCheckController> _logger;

    public HealthCheckController(ILogger<HealthCheckController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simple health check endpoint.
    /// </summary>
    /// <returns>Health status with timestamp</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> Get()
    {
        _logger.LogInformation("Health check endpoint called");

        var response = new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        return Ok(response.Ok());
    }
}

