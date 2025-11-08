using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Responses;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Contractors controller for managing contractor profiles and listings.
/// All endpoints require JWT authentication.
/// Create/Update/Delete operations require Dispatcher role.
/// </summary>
[ApiController]
[Route("api/v1/contractors")]
[Authorize]
public class ContractorsController : ControllerBase
{
    private readonly IContractorService _contractorService;
    private readonly ILogger<ContractorsController> _logger;

    public ContractorsController(
        IContractorService contractorService,
        ILogger<ContractorsController> logger)
    {
        _contractorService = contractorService ?? throw new ArgumentNullException(nameof(contractorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get list of all active contractors (paginated).
    /// All authenticated users can view contractor list.
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Number of results per page (default: 50, max: 100)</param>
    /// <returns>200 OK with paginated list of contractors</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ContractorResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ContractorResponse>>>> GetContractors(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            _logger.LogInformation("Get contractors list requested. Page: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);

            var result = await _contractorService.GetAllContractorsAsync(pageNumber, pageSize);
            
            return Ok(new ApiResponse<PaginatedResponse<ContractorResponse>>(result, 200));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error retrieving contractors");
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contractors");
            throw;
        }
    }

    /// <summary>
    /// Get a single contractor by ID.
    /// All authenticated users can view contractor details.
    /// </summary>
    /// <param name="id">The contractor ID</param>
    /// <returns>200 OK with contractor details, or 404 Not Found if not found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ContractorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ContractorResponse>>> GetContractor(int id)
    {
        try
        {
            _logger.LogInformation("Get contractor requested. ContractorId: {ContractorId}", id);

            var contractor = await _contractorService.GetContractorAsync(id);
            
            return Ok(new ApiResponse<ContractorResponse>(contractor, 200));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Contractor not found. ContractorId: {ContractorId}", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contractor {ContractorId}", id);
            throw;
        }
    }

    /// <summary>
    /// Create a new contractor profile.
    /// Dispatcher-only operation.
    /// </summary>
    /// <param name="request">Contractor details to create</param>
    /// <returns>201 Created with new contractor, or 403 Forbidden if not dispatcher</returns>
    [HttpPost]
    [Authorize(Roles = "Dispatcher")]
    [ProducesResponseType(typeof(ApiResponse<ContractorResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ContractorResponse>>> CreateContractor(
        [FromBody] CreateContractorRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            var dispatcherId = GetUserId();
            _logger.LogInformation("Create contractor requested by dispatcher {DispatcherId}", dispatcherId);

            var contractor = await _contractorService.CreateContractorAsync(request, dispatcherId);
            
            return CreatedAtAction(nameof(GetContractor), new { id = contractor.Id },
                new ApiResponse<ContractorResponse>(contractor, 201));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating contractor");
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contractor");
            throw;
        }
    }

    /// <summary>
    /// Update an existing contractor.
    /// Dispatcher-only operation. All fields are optional (partial update).
    /// </summary>
    /// <param name="id">The contractor ID</param>
    /// <param name="request">Fields to update</param>
    /// <returns>200 OK with updated contractor, or 404 Not Found if not found</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Dispatcher")]
    [ProducesResponseType(typeof(ApiResponse<ContractorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ContractorResponse>>> UpdateContractor(
        int id,
        [FromBody] UpdateContractorRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        try
        {
            var dispatcherId = GetUserId();
            _logger.LogInformation("Update contractor requested by dispatcher {DispatcherId} for contractor {ContractorId}", dispatcherId, id);

            var contractor = await _contractorService.UpdateContractorAsync(id, request, dispatcherId);
            
            return Ok(new ApiResponse<ContractorResponse>(contractor, 200));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Contractor not found for update. ContractorId: {ContractorId}", id);
            return NotFound();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating contractor");
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contractor {ContractorId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deactivate a contractor (soft delete).
    /// Dispatcher-only operation. The contractor will no longer appear in lists.
    /// </summary>
    /// <param name="id">The contractor ID</param>
    /// <returns>204 No Content if successful, or 404 Not Found if not found</returns>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = "Dispatcher")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateContractor(int id)
    {
        try
        {
            var dispatcherId = GetUserId();
            _logger.LogInformation("Deactivate contractor requested by dispatcher {DispatcherId} for contractor {ContractorId}", dispatcherId, id);

            await _contractorService.DeactivateContractorAsync(id, dispatcherId);
            
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Contractor not found for deactivation. ContractorId: {ContractorId}", id);
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating contractor {ContractorId}", id);
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
