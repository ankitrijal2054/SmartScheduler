using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Responses;
using SmartScheduler.Domain.Enums;

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
    private readonly ILogger<ContractorController> _logger;

    public ContractorController(
        IAssignmentRepository assignmentRepository,
        IContractorRepository contractorRepository,
        ILogger<ContractorController> logger)
    {
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
        _contractorRepository = contractorRepository ?? throw new ArgumentNullException(nameof(contractorRepository));
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

