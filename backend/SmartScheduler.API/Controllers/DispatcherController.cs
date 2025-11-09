using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartScheduler.Application.Commands;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Queries;
using SmartScheduler.Domain.Exceptions;
using IAuthService = SmartScheduler.Application.Services.IAuthorizationService;
using ValidationException = SmartScheduler.Domain.Exceptions.ValidationException;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Dispatcher controller for managing dispatcher-specific operations.
/// All endpoints require JWT authentication with Dispatcher role.
/// </summary>
[ApiController]
[Route("api/v1/dispatcher")]
[Authorize(Roles = "Dispatcher")]
public class DispatcherController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DispatcherController> _logger;
    private readonly IAuthService _authorizationService;

    public DispatcherController(
        IMediator mediator,
        ILogger<DispatcherController> logger,
        IAuthService authorizationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    /// <summary>
    /// Add a contractor to dispatcher's personal list.
    /// Idempotent: Adding the same contractor twice returns 200 OK without error.
    /// </summary>
    /// <param name="contractorId">The ID of the contractor to add</param>
    /// <returns>200 OK with the dispatcher contractor list ID, or error response</returns>
    /// <response code="200">Success - contractor added to dispatcher's list</response>
    /// <response code="400">Bad Request - invalid contractor ID</response>
    /// <response code="401">Unauthorized - missing or invalid JWT token</response>
    /// <response code="403">Forbidden - user is not a Dispatcher</response>
    /// <response code="404">Not Found - contractor does not exist</response>
    [HttpPost("contractor-list/{contractorId}")]
    public async Task<ActionResult<object>> PostContractorToList(int contractorId)
    {
        _logger.LogInformation("Add contractor to list requested: ContractorId={ContractorId}", contractorId);

        try
        {
            // Extract dispatcher ID from JWT token
            var dispatcherId = _authorizationService.GetCurrentUserIdFromContext(User);

            // Create and handle command via MediatR
            var command = new AddContractorToListCommand(dispatcherId, contractorId);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Successfully added Contractor {ContractorId} to Dispatcher {DispatcherId} list",
                contractorId, dispatcherId);

            return Ok(new
            {
                message = "Contractor added to your list",
                dispatcherContractorListId = result,
                contractorId = contractorId
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error adding contractor {ContractorId}: {Message}", contractorId, ex.Message);
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
            _logger.LogWarning(ex, "Contractor not found: {Message}", ex.Message);
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
            _logger.LogError(ex, "Unexpected error adding contractor {ContractorId} to list", contractorId);
            return StatusCode(500, new
            {
                error = new
                {
                    code = "CONTRACTOR_LIST_ERROR",
                    message = "Unable to add contractor to list",
                    statusCode = 500
                }
            });
        }
    }

    /// <summary>
    /// Remove a contractor from dispatcher's personal list.
    /// Idempotent: Removing a contractor not in the list returns 200 OK without error.
    /// </summary>
    /// <param name="contractorId">The ID of the contractor to remove</param>
    /// <returns>200 OK with success message, or error response</returns>
    /// <response code="200">Success - contractor removed from dispatcher's list (or was not in list)</response>
    /// <response code="400">Bad Request - invalid contractor ID</response>
    /// <response code="401">Unauthorized - missing or invalid JWT token</response>
    /// <response code="403">Forbidden - user is not a Dispatcher</response>
    [HttpDelete("contractor-list/{contractorId}")]
    public async Task<ActionResult<object>> DeleteContractorFromList(int contractorId)
    {
        _logger.LogInformation("Remove contractor from list requested: ContractorId={ContractorId}", contractorId);

        try
        {
            // Extract dispatcher ID from JWT token
            var dispatcherId = _authorizationService.GetCurrentUserIdFromContext(User);

            // Create and handle command via MediatR
            var command = new RemoveContractorFromListCommand(dispatcherId, contractorId);
            await _mediator.Send(command);

            _logger.LogInformation("Successfully removed Contractor {ContractorId} from Dispatcher {DispatcherId} list",
                contractorId, dispatcherId);

            return Ok(new
            {
                message = "Contractor removed from your list",
                contractorId = contractorId
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error removing contractor {ContractorId}: {Message}", contractorId, ex.Message);
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
            _logger.LogError(ex, "Unexpected error removing contractor {ContractorId} from list", contractorId);
            return StatusCode(500, new
            {
                error = new
                {
                    code = "CONTRACTOR_LIST_ERROR",
                    message = "Unable to remove contractor from list",
                    statusCode = 500
                }
            });
        }
    }

    /// <summary>
    /// Get dispatcher's curated contractor list with pagination and optional search filter.
    /// </summary>
    /// <param name="page">Page number (default: 1, must be >= 1)</param>
    /// <param name="limit">Number of items per page (default: 50, max: 100)</param>
    /// <param name="search">Optional search filter to find contractors by name (case-insensitive)</param>
    /// <returns>200 OK with paginated contractor list, or error response</returns>
    /// <response code="200">Success - returns dispatcher's contractor list with pagination</response>
    /// <response code="400">Bad Request - invalid pagination parameters</response>
    /// <response code="401">Unauthorized - missing or invalid JWT token</response>
    /// <response code="403">Forbidden - user is not a Dispatcher</response>
    [HttpGet("contractor-list")]
    public async Task<ActionResult<DispatcherContractorListResponseDto>> GetContractorList(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50,
        [FromQuery] string? search = null)
    {
        _logger.LogInformation("Get contractor list requested: Page={Page}, Limit={Limit}, Search={Search}",
            page, limit, search ?? "none");

        try
        {
            // Extract dispatcher ID from JWT token
            var dispatcherId = _authorizationService.GetCurrentUserIdFromContext(User);

            // Create and handle query via MediatR
            var query = new GetDispatcherContractorListQuery(dispatcherId, page, limit, search);
            var response = await _mediator.Send(query);

            _logger.LogInformation("Successfully retrieved contractor list for Dispatcher {DispatcherId}. Total: {Total}",
                dispatcherId, response.Pagination.Total);

            return Ok(new
            {
                contractors = response.Contractors,
                pagination = response.Pagination
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error retrieving contractor list: {Message}", ex.Message);
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
            _logger.LogError(ex, "Unexpected error retrieving contractor list");
            return StatusCode(500, new
            {
                error = new
                {
                    code = "CONTRACTOR_LIST_ERROR",
                    message = "Unable to retrieve contractor list",
                    statusCode = 500
                }
            });
        }
    }

    /// <summary>
    /// Assign a job to a specific contractor.
    /// Only dispatchers can perform this operation.
    /// </summary>
    /// <param name="jobId">The ID of the job to assign</param>
    /// <param name="contractorId">The ID of the contractor to assign to</param>
    /// <returns>200 OK with assignment ID, or error response</returns>
    /// <response code="200">Success - job assigned to contractor</response>
    /// <response code="400">Bad Request - job already assigned or contractor inactive</response>
    /// <response code="401">Unauthorized - missing or invalid JWT token</response>
    /// <response code="403">Forbidden - user is not a Dispatcher</response>
    /// <response code="404">Not Found - job or contractor does not exist</response>
    /// <response code="409">Conflict - job already assigned to another contractor</response>
    [HttpPost("jobs/{jobId}/assign")]
    public async Task<ActionResult<object>> AssignJob([FromRoute] int jobId, [FromQuery] int contractorId)
    {
        _logger.LogInformation("Job assignment requested: JobId={JobId}, ContractorId={ContractorId}", jobId, contractorId);

        try
        {
            // Extract dispatcher ID from JWT token
            var dispatcherId = _authorizationService.GetCurrentUserIdFromContext(User);

            // Create and handle command via MediatR
            var command = new AssignJobCommand(jobId, contractorId, dispatcherId);
            var assignmentId = await _mediator.Send(command);

            _logger.LogInformation("Successfully assigned Job {JobId} to Contractor {ContractorId}. Assignment ID: {AssignmentId}",
                jobId, contractorId, assignmentId);

            return Ok(new
            {
                message = "Job assigned successfully",
                assignmentId = assignmentId,
                jobId = jobId,
                contractorId = contractorId
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error assigning job {JobId}: {Message}", jobId, ex.Message);
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
            _logger.LogWarning(ex, "Job or contractor not found: {Message}", ex.Message);
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
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            return Conflict(new
            {
                error = new
                {
                    code = "JOB_ALREADY_ASSIGNED",
                    message = ex.Message,
                    statusCode = 409
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
            _logger.LogError(ex, "Unexpected error assigning job {JobId} to contractor {ContractorId}", jobId, contractorId);
            return StatusCode(500, new
            {
                error = new
                {
                    code = "JOB_ASSIGNMENT_ERROR",
                    message = "Unable to assign job to contractor",
                    statusCode = 500
                }
            });
        }
    }
}

