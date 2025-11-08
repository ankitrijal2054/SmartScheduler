using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;
using SmartScheduler.Infrastructure.Persistence;
using IAuthService = SmartScheduler.Application.Services.IAuthorizationService;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Jobs controller for managing job listings and submissions.
/// Implements role-based filtering: Dispatchers see all jobs, Customers see their own, Contractors see assigned jobs.
/// </summary>
[ApiController]
[Route("api/v1/jobs")]
public class JobsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<JobsController> _logger;
    private readonly IAuthService _authorizationService;

    public JobsController(
        ApplicationDbContext dbContext,
        ILogger<JobsController> logger,
        IAuthService authorizationService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    /// <summary>
    /// Get list of jobs (role-filtered).
    /// - Dispatcher: Returns all jobs
    /// - Customer: Returns only jobs created by this customer
    /// - Contractor: Returns only jobs assigned to this contractor
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of results per page (default: 10)</param>
    /// <returns>200 OK with paginated list of jobs visible to the user</returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            throw new ValidationException("Page and pageSize must be valid. PageSize must be between 1 and 100.");
        }

        try
        {
            // Extract user ID and role from claims
            var userId = _authorizationService.GetCurrentUserIdFromContext(User);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Start with all jobs query
            var jobsQuery = _dbContext.Jobs
                .Include(j => j.Customer)
                .Include(j => j.Assignment)
                .AsQueryable();

            // Apply role-based filtering
            jobsQuery = _authorizationService.FilterDataByRole(userId, userRole, jobsQuery);

            var jobs = await jobsQuery
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(j => new JobDto
                {
                    Id = j.Id,
                    JobType = j.JobType.ToString(),
                    Location = j.Location,
                    DesiredDateTime = j.DesiredDateTime,
                    Description = j.Description,
                    Status = j.Status.ToString(),
                    CustomerId = j.CustomerId,
                    AssignedContractorId = j.AssignedContractorId
                })
                .ToListAsync();

            // Get total count with same filtering
            jobsQuery = _dbContext.Jobs
                .Include(j => j.Customer)
                .Include(j => j.Assignment)
                .AsQueryable();
            jobsQuery = _authorizationService.FilterDataByRole(userId, userRole, jobsQuery);
            var totalCount = await jobsQuery.CountAsync();

            _logger.LogInformation("Retrieved {JobCount} jobs for user {UserId} with role {Role}", jobs.Count, userId, userRole);

            return Ok(new
            {
                data = jobs,
                pagination = new
                {
                    page,
                    pageSize,
                    total = totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Missing user claims");
            throw new UnauthorizedException(ex.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving jobs");
            throw;
        }
    }

    /// <summary>
    /// Create a new job (customer-only operation).
    /// Auto-populates CustomerId from JWT claims (not from request).
    /// </summary>
    /// <param name="createJobDto">Job details to create</param>
    /// <returns>201 Created with new job, or 403 Forbidden if not customer</returns>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobDto createJobDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Extract customer ID from claims (don't trust request)
            var userId = _authorizationService.GetCurrentUserIdFromContext(User);

            // Verify user is a customer and get customer profile
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null || user.Role != UserRole.Customer)
                throw new ForbiddenException("Only customers can create jobs");

            // Validate trade type
            if (!Enum.TryParse<TradeType>(createJobDto.JobType, true, out var jobType))
                throw new ValidationException($"Invalid JobType. Supported types: {string.Join(", ", Enum.GetNames(typeof(TradeType)))}");

            // Find or create customer profile
            var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
                throw new NotFoundException("Customer profile not found");

            var job = new Job
            {
                CustomerId = customer.Id,
                JobType = jobType,
                Location = createJobDto.Location,
                DesiredDateTime = createJobDto.DesiredDateTime,
                Description = createJobDto.Description,
                EstimatedDurationHours = createJobDto.EstimatedDurationHours,
                Status = JobStatus.Pending,
                Latitude = 0, // Would be calculated from location in real scenario
                Longitude = 0
            };

            _dbContext.Jobs.Add(job);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Job created successfully: {JobId} by customer {CustomerId}", job.Id, customer.Id);

            var response = new JobDto
            {
                Id = job.Id,
                JobType = job.JobType.ToString(),
                Location = job.Location,
                DesiredDateTime = job.DesiredDateTime,
                Description = job.Description,
                Status = job.Status.ToString(),
                CustomerId = job.CustomerId,
                AssignedContractorId = job.AssignedContractorId
            };

            return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, response);
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ForbiddenException)
        {
            throw;
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Missing user claims");
            throw new UnauthorizedException(exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error creating job");
            throw;
        }
    }

    /// <summary>
    /// Get a single job by ID with authorization check.
    /// - Dispatcher: Can view any job
    /// - Customer: Can view only their own jobs
    /// - Contractor: Can view only assigned jobs
    /// </summary>
    /// <param name="id">Job ID</param>
    /// <returns>200 OK with job details, 403 Forbidden if not authorized, 404 Not Found if job doesn't exist</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetJobById([FromRoute] int id)
    {
        try
        {
            var userId = _authorizationService.GetCurrentUserIdFromContext(User);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var job = await _dbContext.Jobs
                .Include(j => j.Customer)
                .Include(j => j.Assignment)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
                throw new NotFoundException($"Job {id} not found");

            // Check authorization based on role
            bool isAuthorized = userRole switch
            {
                "Dispatcher" => true, // Dispatcher can see all jobs
                "Customer" => _authorizationService.ValidateUserOwnsResource(userId, job.CustomerId),
                "Contractor" => job.Assignment != null && _authorizationService.ValidateUserOwnsResource(userId, job.Assignment.ContractorId),
                _ => false
            };

            if (!isAuthorized)
                throw new ForbiddenException("You do not have permission to view this job");

            _logger.LogInformation("Job {JobId} retrieved by user {UserId}", id, userId);

            var response = new JobDto
            {
                Id = job.Id,
                JobType = job.JobType.ToString(),
                Location = job.Location,
                DesiredDateTime = job.DesiredDateTime,
                Description = job.Description,
                Status = job.Status.ToString(),
                CustomerId = job.CustomerId,
                AssignedContractorId = job.AssignedContractorId
            };

            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Missing user claims");
            throw new UnauthorizedException(exception.Message);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ForbiddenException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error retrieving job {JobId}", id);
            throw;
        }
    }
}

