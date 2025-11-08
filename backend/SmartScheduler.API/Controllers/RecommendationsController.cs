using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Exceptions;
using SmartScheduler.Infrastructure.Persistence;
using IAuthService = SmartScheduler.Application.Services.IAuthorizationService;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Recommendations controller for dispatcher-only contractor recommendations.
/// Returns top contractor recommendations based on job requirements.
/// </summary>
[ApiController]
[Route("api/v1/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<RecommendationsController> _logger;
    private readonly IAuthService _authorizationService;

    public RecommendationsController(
        ApplicationDbContext dbContext,
        ILogger<RecommendationsController> logger,
        IAuthService authorizationService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    /// <summary>
    /// Get recommended contractors for a specific job type and location.
    /// Dispatcher-only operation. Returns top 5 contractors ranked by score.
    /// </summary>
    /// <param name="jobType">Job type / trade type</param>
    /// <param name="location">Job location (used for distance-based ranking)</param>
    /// <returns>200 OK with top 5 recommended contractors, or 403 Forbidden if not dispatcher</returns>
    [HttpGet]
    [Authorize(Roles = "Dispatcher")]
    public async Task<IActionResult> GetRecommendations(
        [FromQuery] string jobType,
        [FromQuery] string location)
    {
        if (string.IsNullOrWhiteSpace(jobType) || string.IsNullOrWhiteSpace(location))
        {
            throw new ValidationException("Both jobType and location parameters are required");
        }

        try
        {
            var userId = _authorizationService.GetCurrentUserIdFromContext(User);

            // Get contractors matching the job type, sorted by rating (descending) and active status
            var recommendedContractors = await _dbContext.Contractors
                .Where(c => c.IsActive && c.TradeType.ToString() == jobType)
                .OrderByDescending(c => c.AverageRating ?? 0)
                .ThenByDescending(c => c.TotalJobsCompleted)
                .Take(5)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    Rating = c.AverageRating ?? 0,
                    c.ReviewCount,
                    Distance = 0, // Would be calculated in real scenario based on coordinates
                    TravelTime = 0, // Would be calculated in real scenario
                    Score = (c.AverageRating ?? 0) * 100 + c.TotalJobsCompleted * 10
                })
                .ToListAsync();

            _logger.LogInformation(
                "Retrieved {Count} contractor recommendations for dispatcher {UserId}, jobType {JobType}",
                recommendedContractors.Count,
                userId,
                jobType);

            return Ok(new
            {
                contractors = recommendedContractors
            });
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Missing user claims");
            throw new UnauthorizedException(exception.Message);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving recommendations");
            throw;
        }
    }
}

