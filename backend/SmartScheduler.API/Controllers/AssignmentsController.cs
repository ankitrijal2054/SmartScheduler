using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartScheduler.Application.Commands;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Responses;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Assignments controller for managing job assignment status transitions.
/// Endpoints for marking jobs as in-progress and completed.
/// All endpoints require JWT authentication with Contractor role.
/// </summary>
[ApiController]
[Route("api/v1/assignments")]
[Authorize(Roles = "Contractor")]
public class AssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ILogger<AssignmentsController> _logger;

    public AssignmentsController(
        IMediator mediator,
        IAssignmentRepository assignmentRepository,
        ILogger<AssignmentsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Mark a job assignment as in-progress.
    /// Contractor-only operation. Transitions assignment status from 'Accepted' to 'InProgress'.
    /// Publishes JobInProgressEvent for real-time customer notification.
    /// </summary>
    /// <param name="assignmentId">The assignment ID to mark as in-progress</param>
    /// <returns>200 OK with updated assignment, or 404 Not Found if not found, or 403 Forbidden if not assigned contractor</returns>
    [HttpPatch("{assignmentId}/mark-in-progress")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> MarkInProgress(int assignmentId)
    {
        try
        {
            var contractorId = GetUserId();
            _logger.LogInformation(
                "Mark in-progress requested. AssignmentId: {AssignmentId}, ContractorId: {ContractorId}",
                assignmentId, contractorId);

            var command = new UpdateAssignmentStatusCommand(assignmentId, AssignmentStatus.InProgress, contractorId);
            var result = await _mediator.Send(command);

            _logger.LogInformation(
                "Assignment marked as in-progress. AssignmentId: {AssignmentId}, ContractorId: {ContractorId}",
                assignmentId, contractorId);

            return Ok(new ApiResponse<AssignmentDto>(result, 200));
        }
        catch (AssignmentNotFoundException ex)
        {
            _logger.LogWarning(ex, "Assignment not found. AssignmentId: {AssignmentId}", assignmentId);
            return NotFound(new ApiResponse<AssignmentDto>(null, 404));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update assignment. AssignmentId: {AssignmentId}", assignmentId);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid status transition. AssignmentId: {AssignmentId}", assignmentId);
            return BadRequest(new ApiResponse<AssignmentDto>(null, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking assignment as in-progress. AssignmentId: {AssignmentId}", assignmentId);
            throw;
        }
    }

    /// <summary>
    /// Mark a job assignment as completed.
    /// Contractor-only operation. Transitions assignment status from 'InProgress' to 'Completed'.
    /// Publishes JobCompletedEvent which triggers completion email to customer (Story 6.5).
    /// </summary>
    /// <param name="assignmentId">The assignment ID to mark as completed</param>
    /// <returns>200 OK with updated assignment, or 404 Not Found if not found, or 403 Forbidden if not assigned contractor</returns>
    [HttpPatch("{assignmentId}/mark-complete")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> MarkComplete(int assignmentId)
    {
        try
        {
            var contractorId = GetUserId();
            _logger.LogInformation(
                "Mark complete requested. AssignmentId: {AssignmentId}, ContractorId: {ContractorId}",
                assignmentId, contractorId);

            var command = new UpdateAssignmentStatusCommand(assignmentId, AssignmentStatus.Completed, contractorId);
            var result = await _mediator.Send(command);

            _logger.LogInformation(
                "Assignment marked as completed. AssignmentId: {AssignmentId}, ContractorId: {ContractorId}",
                assignmentId, contractorId);

            return Ok(new ApiResponse<AssignmentDto>(result, 200));
        }
        catch (AssignmentNotFoundException ex)
        {
            _logger.LogWarning(ex, "Assignment not found. AssignmentId: {AssignmentId}", assignmentId);
            return NotFound(new ApiResponse<AssignmentDto>(null, 404));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized attempt to update assignment. AssignmentId: {AssignmentId}", assignmentId);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid status transition. AssignmentId: {AssignmentId}", assignmentId);
            return BadRequest(new ApiResponse<AssignmentDto>(null, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking assignment as completed. AssignmentId: {AssignmentId}", assignmentId);
            throw;
        }
    }

    /// <summary>
    /// Get contractor's assignments filtered by status with pagination.
    /// Useful for displaying completed jobs history and other statuses.
    /// </summary>
    /// <param name="contractorId">The contractor ID to retrieve assignments for (must match authenticated user)</param>
    /// <param name="status">Assignment status filter (e.g., 'Completed'). Optional, if provided filters results.</param>
    /// <param name="limit">Maximum number of results to return (default: 50, max: 100)</param>
    /// <param name="offset">Number of results to skip for pagination (default: 0)</param>
    /// <returns>200 OK with paginated list of assignments</returns>
    [HttpGet("{contractorId}/history")]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<AssignmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<AssignmentDto>>>> GetContractorHistory(
        int contractorId,
        [FromQuery] string? status = null,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            var authenticatedContractorId = GetUserId();

            // Authorization: contractors can only view their own history
            if (contractorId != authenticatedContractorId)
            {
                _logger.LogWarning(
                    "Unauthorized attempt to view history. RequestedContractorId: {RequestedContractorId}, AuthenticatedContractorId: {AuthenticatedContractorId}",
                    contractorId, authenticatedContractorId);
                return Forbid();
            }

            // Validate pagination parameters
            limit = Math.Min(limit, 100);
            if (limit <= 0 || offset < 0)
                return BadRequest();

            // Determine status filter
            AssignmentStatus assignmentStatus = AssignmentStatus.Completed; // Default to Completed for history
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AssignmentStatus>(status, true, out var parsedStatus))
            {
                assignmentStatus = parsedStatus;
            }

            _logger.LogInformation(
                "Get contractor history requested. ContractorId: {ContractorId}, Status: {Status}, Limit: {Limit}, Offset: {Offset}",
                contractorId, assignmentStatus, limit, offset);

            // Get assignments and total count
            var assignments = await _assignmentRepository.GetAssignmentsByContractorAndStatusAsync(contractorId, assignmentStatus, limit, offset);
            var total = await _assignmentRepository.GetAssignmentCountByContractorAndStatusAsync(contractorId, assignmentStatus);

            // Convert to DTOs
            var assignmentDtos = assignments.Select(a => new AssignmentDto
            {
                Id = a.Id,
                JobId = a.JobId,
                ContractorId = a.ContractorId,
                Status = a.Status.ToString(),
                AssignedAt = a.AssignedAt,
                AcceptedAt = a.AcceptedAt,
                DeclinedAt = a.DeclinedAt,
                StartedAt = a.StartedAt,
                CompletedAt = a.CompletedAt
            }).ToList();

            // Calculate page number from offset (1-based)
            var pageNumber = (offset / limit) + 1;

            var paginatedResult = new PaginatedResponse<AssignmentDto>
            {
                Items = assignmentDtos,
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = limit
            };

            return Ok(new ApiResponse<PaginatedResponse<AssignmentDto>>(paginatedResult, 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contractor history for ContractorId: {ContractorId}", contractorId);
            throw;
        }
    }

    /// <summary>
    /// Helper method to extract user ID from JWT claims.
    /// </summary>
    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
        return userId;
    }
}

