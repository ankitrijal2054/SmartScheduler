using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Contractors controller for managing contractor profiles and listings.
/// </summary>
[ApiController]
[Route("api/v1/contractors")]
public class ContractorsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ContractorsController> _logger;
    private readonly IAuthorizationService _authorizationService;

    public ContractorsController(
        ApplicationDbContext dbContext,
        ILogger<ContractorsController> logger,
        IAuthorizationService authorizationService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    /// <summary>
    /// Get list of all active contractors (paginated).
    /// Requires authentication but no role restriction - all authenticated users can view contractor list.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of results per page (default: 10)</param>
    /// <returns>200 OK with paginated list of contractors</returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetContractors([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            throw new ValidationException("Page and pageSize must be valid. PageSize must be between 1 and 100.");
        }

        try
        {
            var contractors = await _dbContext.Contractors
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.AverageRating)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ContractorDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Location = c.Location,
                    TradeType = c.TradeType.ToString(),
                    AverageRating = c.AverageRating,
                    ReviewCount = c.ReviewCount
                })
                .ToListAsync();

            var totalCount = await _dbContext.Contractors
                .Where(c => c.IsActive)
                .CountAsync();

            _logger.LogInformation("Retrieved {ContractorCount} contractors for user", contractors.Count);

            return Ok(new
            {
                data = contractors,
                pagination = new
                {
                    page,
                    pageSize,
                    total = totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving contractors");
            throw;
        }
    }

    /// <summary>
    /// Create a new contractor profile.
    /// Dispatcher-only operation. Returns 403 Forbidden if user doesn't have Dispatcher role.
    /// </summary>
    /// <param name="createContractorDto">Contractor details to create</param>
    /// <returns>201 Created with new contractor, or 403 Forbidden if not dispatcher</returns>
    [HttpPost]
    [Authorize(Roles = "Dispatcher")]
    public async Task<IActionResult> CreateContractor([FromBody] CreateContractorDto createContractorDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Validate working hours format
            if (!TimeSpan.TryParse(createContractorDto.WorkingHoursStart, out var hoursStart))
                throw new ValidationException("Invalid WorkingHoursStart format. Use HH:mm:ss");

            if (!TimeSpan.TryParse(createContractorDto.WorkingHoursEnd, out var hoursEnd))
                throw new ValidationException("Invalid WorkingHoursEnd format. Use HH:mm:ss");

            // Validate trade type
            if (!Enum.TryParse<TradeType>(createContractorDto.TradeType, true, out var tradeType))
                throw new ValidationException($"Invalid TradeType. Supported types: {string.Join(", ", Enum.GetNames(typeof(TradeType)))}");

            var contractor = new Contractor
            {
                Name = createContractorDto.Name,
                Location = createContractorDto.Location,
                PhoneNumber = createContractorDto.PhoneNumber,
                TradeType = tradeType,
                WorkingHoursStart = hoursStart,
                WorkingHoursEnd = hoursEnd,
                IsActive = true,
                ReviewCount = 0,
                TotalJobsCompleted = 0
            };

            _dbContext.Contractors.Add(contractor);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Contractor created successfully: {ContractorId}", contractor.Id);

            var response = new ContractorDto
            {
                Id = contractor.Id,
                Name = contractor.Name,
                Location = contractor.Location,
                TradeType = contractor.TradeType.ToString(),
                AverageRating = contractor.AverageRating,
                ReviewCount = contractor.ReviewCount
            };

            return CreatedAtAction(nameof(GetContractors), new { id = contractor.Id }, response);
        }
        catch (ValidationException ex)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contractor");
            throw;
        }
    }
}

