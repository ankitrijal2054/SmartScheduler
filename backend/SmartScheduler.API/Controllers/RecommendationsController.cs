using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Queries;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Exceptions;
using IAuthService = SmartScheduler.Application.Services.IAuthorizationService;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Recommendations controller for dispatcher-only contractor recommendations.
/// Returns top 5 contractor recommendations ranked by intelligent scoring algorithm.
/// </summary>
[ApiController]
[Route("api/v1/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RecommendationsController> _logger;
    private readonly IAuthService _authorizationService;

    public RecommendationsController(
        IMediator mediator,
        ILogger<RecommendationsController> logger,
        IAuthService authorizationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    /// <summary>
    /// Get top 5 recommended contractors for a specific job.
    /// Dispatcher-only operation. Rankings based on availability (40%), rating (30%), and distance (30%).
    /// Response time: <500ms (even with 10,000 contractors in database).
    /// </summary>
    /// <param name="jobId">The ID of the job to get recommendations for.</param>
    /// <param name="contractorListOnly">Optional: If true, only recommend from dispatcher's personal list. Default: false.</param>
    /// <returns>200 OK with top 5 recommended contractors, or error response.</returns>
    /// <response code="200">Success - returns recommendation response with up to 5 contractors.</response>
    /// <response code="400">Bad Request - invalid job ID or date/time in the past.</response>
    /// <response code="401">Unauthorized - missing or invalid JWT token.</response>
    /// <response code="403">Forbidden - user is not a Dispatcher.</response>
    /// <response code="500">Internal Server Error - unexpected error during recommendation retrieval.</response>
    [HttpGet]
    [Authorize(Roles = "Dispatcher")]
    public async Task<ActionResult<RecommendationResponseDto>> GetRecommendations(
        [FromQuery] int jobId,
        [FromQuery] bool contractorListOnly = false)
    {
        _logger.LogInformation("Recommendations request: JobId={JobId}, ContractorListOnly={ContractorListOnly}", 
            jobId, contractorListOnly);

        try
        {
            // Extract dispatcher ID from JWT token
            var dispatcherId = _authorizationService.GetCurrentUserIdFromContext(User);

            // Create and handle query via MediatR
            var query = new GetContractorRecommendationsQuery(jobId, dispatcherId, contractorListOnly);
            var response = await _mediator.Send(query);

            _logger.LogInformation("Successfully retrieved {Count} recommendations for Job {JobId} by Dispatcher {DispatcherId}",
                response.Recommendations.Count, jobId, dispatcherId);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error for Job {JobId}: {Message}", jobId, ex.Message);
            return BadRequest(new
            {
                error = new
                {
                    code = "INVALID_REQUEST",
                    message = ex.Message,
                    statusCode = 400
                }
            });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Job or resource not found: {Message}", ex.Message);
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = ex.Message,
                    statusCode = 404
                }
            });
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Authorization failed: {Message}", ex.Message);
            return Unauthorized(new
            {
                error = new
                {
                    code = "UNAUTHORIZED",
                    message = ex.Message,
                    statusCode = 401
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving recommendations for Job {JobId}", jobId);
            return StatusCode(500, new
            {
                error = new
                {
                    code = "RECOMMENDATIONS_ERROR",
                    message = "Unable to retrieve contractor recommendations",
                    statusCode = 500
                }
            });
        }
    }
}

