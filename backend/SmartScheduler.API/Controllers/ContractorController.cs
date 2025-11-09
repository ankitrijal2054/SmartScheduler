using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartScheduler.Application.Commands;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Responses;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Contractor controller for contractor-specific endpoints.
/// All endpoints require JWT authentication with Contractor role.
/// </summary>
[ApiController]
[Route("api/v1/contractors")]
[Authorize(Roles = "Contractor")]
public class ContractorController : ControllerBase
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IContractorRepository _contractorRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ContractorController> _logger;

    public ContractorController(
        IAssignmentRepository assignmentRepository,
        IContractorRepository contractorRepository,
        IReviewRepository reviewRepository,
        IMediator mediator,
        ApplicationDbContext dbContext,
        ILogger<ContractorController> logger)
    {
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
        _contractorRepository = contractorRepository ?? throw new ArgumentNullException(nameof(contractorRepository));
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get contractor's assignments filtered by status.
    /// Returns assignments with job and customer details.
    /// </summary>
    /// <param name="status">Optional status filter (Pending, Accepted, InProgress, Completed)</param>
    /// <returns>200 OK with list of assignments</returns>
    [HttpGet("assignments")]
    [ProducesResponseType(typeof(ApiResponse<List<ContractorAssignmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<ContractorAssignmentDto>>>> GetAssignments(
        [FromQuery] string? status = null)
    {
        try
        {
            var userId = GetUserId();
            
            // Resolve contractor ID from user ID
            var contractor = await _contractorRepository.GetByUserIdAsync(userId);
            if (contractor == null)
            {
                _logger.LogWarning("Contractor not found for UserId: {UserId}", userId);
                return NotFound(new ApiResponse<List<ContractorAssignmentDto>>(null, 404));
            }

            var contractorId = contractor.Id;
            _logger.LogInformation(
                "Get contractor assignments requested. UserId: {UserId}, ContractorId: {ContractorId}, Status: {Status}",
                userId, contractorId, status);

            // Parse status if provided
            AssignmentStatus? assignmentStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AssignmentStatus>(status, true, out var parsedStatus))
            {
                assignmentStatus = parsedStatus;
            }

            // Get assignments with job and customer details
            var assignments = await _assignmentRepository.GetContractorAssignmentsWithDetailsAsync(
                contractorId, assignmentStatus);

            // Map to DTO
            var assignmentDtos = assignments.Select(a => new ContractorAssignmentDto
            {
                Id = a.Id,
                JobId = a.JobId,
                ContractorId = a.ContractorId,
                Status = a.Status.ToString(),
                CreatedAt = a.AssignedAt.ToString("O"),
                JobType = a.Job?.JobType.ToString() ?? "Unknown",
                Location = a.Job?.Location ?? "",
                ScheduledTime = a.Job?.DesiredDateTime.ToString("O"),
                CustomerName = a.Job?.Customer?.Name ?? "Unknown",
                Description = a.Job?.Description ?? "",
            }).ToList();

            return Ok(new ApiResponse<List<ContractorAssignmentDto>>(assignmentDtos, 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contractor assignments");
            throw;
        }
    }

    /// <summary>
    /// Get detailed information about a specific assignment with job, customer, and review history.
    /// </summary>
    /// <param name="assignmentId">The assignment ID</param>
    /// <returns>200 OK with job details, or 404 Not Found</returns>
    [HttpGet("assignments/{assignmentId}")]
    [ProducesResponseType(typeof(ApiResponse<JobDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<JobDetailsDto>>> GetAssignmentDetails(int assignmentId)
    {
        try
        {
            var userId = GetUserId();
            
            // Resolve contractor ID from user ID
            var contractor = await _contractorRepository.GetByUserIdAsync(userId);
            if (contractor == null)
            {
                _logger.LogWarning("Contractor not found for UserId: {UserId}", userId);
                return NotFound(new ApiResponse<JobDetailsDto>(null, 404));
            }

            var contractorId = contractor.Id;
            _logger.LogInformation(
                "Get assignment details requested. AssignmentId: {AssignmentId}, ContractorId: {ContractorId}",
                assignmentId, contractorId);

            // Get assignment with full details
            var assignment = await _assignmentRepository.GetAssignmentWithDetailsAsync(assignmentId, contractorId);
            if (assignment == null || assignment.Job == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} not found or not authorized for contractor {ContractorId}", 
                    assignmentId, contractorId);
                return NotFound(new ApiResponse<JobDetailsDto>(null, 404));
            }

            var job = assignment.Job;
            var customer = job.Customer;
            if (customer == null)
            {
                _logger.LogWarning("Job {JobId} has no customer", job.Id);
                return NotFound(new ApiResponse<JobDetailsDto>(null, 404));
            }

            // Get customer's average rating and review count
            var customerReviews = await _dbContext.Reviews
                .Where(r => r.CustomerId == customer.Id)
                .ToListAsync();
            
            var customerRating = customerReviews.Any() 
                ? (decimal?)customerReviews.Average(r => (decimal)r.Rating)
                : null;
            var customerReviewCount = customerReviews.Count;

            // Get past reviews for jobs where this contractor worked with this customer
            var pastReviews = await _dbContext.Reviews
                .Include(r => r.Job)
                .Where(r => r.ContractorId == contractorId 
                    && r.CustomerId == customer.Id
                    && r.JobId != job.Id) // Exclude current job
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new JobDetailReviewDto
                {
                    Id = r.Id.ToString(),
                    JobId = r.JobId.ToString(),
                    JobType = r.Job != null ? r.Job.JobType.ToString() : "Unknown",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt.ToString("O")
                })
                .ToListAsync();

            // Map to JobDetailsDto
            var jobDetails = new JobDetailsDto
            {
                AssignmentId = assignment.Id.ToString(),
                Status = assignment.Status.ToString(),
                AssignedAt = assignment.AssignedAt.ToString("O"),
                AcceptedAt = assignment.AcceptedAt?.ToString("O"),
                JobId = job.Id.ToString(),
                JobType = job.JobType.ToString(),
                Location = job.Location,
                DesiredDateTime = job.DesiredDateTime.ToString("O"),
                Description = job.Description ?? "",
                EstimatedDuration = null, // Not stored in Job entity
                EstimatedPay = null, // Not stored in Job entity
                Customer = new JobDetailCustomerDto
                {
                    Id = customer.Id.ToString(),
                    Name = customer.Name,
                    Rating = customerRating,
                    ReviewCount = customerReviewCount,
                    PhoneNumber = customer.PhoneNumber
                },
                PastReviews = pastReviews
            };

            return Ok(new ApiResponse<JobDetailsDto>(jobDetails, 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignment details for AssignmentId: {AssignmentId}", assignmentId);
            throw;
        }
    }

    /// <summary>
    /// Accept a pending job assignment.
    /// </summary>
    /// <param name="assignmentId">The assignment ID to accept</param>
    /// <returns>200 OK with updated assignment, or error response</returns>
    [HttpPost("assignments/{assignmentId}/accept")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> AcceptAssignment(int assignmentId)
    {
        try
        {
            var userId = GetUserId();
            
            // Resolve contractor ID from user ID
            var contractor = await _contractorRepository.GetByUserIdAsync(userId);
            if (contractor == null)
            {
                _logger.LogWarning("Contractor not found for UserId: {UserId}", userId);
                return NotFound(new ApiResponse<AssignmentDto>(null, 404));
            }

            var contractorId = contractor.Id;
            _logger.LogInformation(
                "Accept assignment requested. AssignmentId: {AssignmentId}, ContractorId: {ContractorId}",
                assignmentId, contractorId);

            var command = new AcceptAssignmentCommand(assignmentId, contractorId);
            var result = await _mediator.Send(command);

            _logger.LogInformation(
                "Assignment {AssignmentId} accepted successfully by contractor {ContractorId}",
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
            _logger.LogWarning(ex, "Unauthorized attempt to accept assignment. AssignmentId: {AssignmentId}", assignmentId);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid status transition. AssignmentId: {AssignmentId}", assignmentId);
            return BadRequest(new ApiResponse<AssignmentDto>(null, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting assignment. AssignmentId: {AssignmentId}", assignmentId);
            throw;
        }
    }

    /// <summary>
    /// Decline a pending job assignment.
    /// </summary>
    /// <param name="assignmentId">The assignment ID to decline</param>
    /// <param name="request">Optional request body with reason for declining</param>
    /// <returns>200 OK with updated assignment, or error response</returns>
    [HttpPost("assignments/{assignmentId}/decline")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> DeclineAssignment(
        int assignmentId,
        [FromBody] DeclineAssignmentRequest? request = null)
    {
        try
        {
            var userId = GetUserId();
            
            // Resolve contractor ID from user ID
            var contractor = await _contractorRepository.GetByUserIdAsync(userId);
            if (contractor == null)
            {
                _logger.LogWarning("Contractor not found for UserId: {UserId}", userId);
                return NotFound(new ApiResponse<AssignmentDto>(null, 404));
            }

            var contractorId = contractor.Id;
            var reason = request?.Reason;
            
            _logger.LogInformation(
                "Decline assignment requested. AssignmentId: {AssignmentId}, ContractorId: {ContractorId}, Reason: {Reason}",
                assignmentId, contractorId, reason ?? "No reason provided");

            var command = new DeclineAssignmentCommand(assignmentId, contractorId, reason);
            var result = await _mediator.Send(command);

            _logger.LogInformation(
                "Assignment {AssignmentId} declined successfully by contractor {ContractorId}",
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
            _logger.LogWarning(ex, "Unauthorized attempt to decline assignment. AssignmentId: {AssignmentId}", assignmentId);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid status transition. AssignmentId: {AssignmentId}", assignmentId);
            return BadRequest(new ApiResponse<AssignmentDto>(null, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error declining assignment. AssignmentId: {AssignmentId}", assignmentId);
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

/// <summary>
/// Request body for declining an assignment.
/// </summary>
public class DeclineAssignmentRequest
{
    public string? Reason { get; set; }
}

