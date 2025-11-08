using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Review entity data access.
/// Provides CRUD operations and querying capabilities for reviews.
/// </summary>
public class ReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ReviewRepository> _logger;

    public ReviewRepository(ApplicationDbContext dbContext, ILogger<ReviewRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all reviews for a specific contractor ordered by creation date descending.
    /// </summary>
    public async Task<List<Review>> GetByContractorIdAsync(int contractorId)
    {
        _logger.LogDebug("Fetching reviews for contractor {ContractorId}", contractorId);
        
        return await _dbContext.Reviews
            .Where(r => r.ContractorId == contractorId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all reviews for a specific job.
    /// </summary>
    public async Task<List<Review>> GetByJobIdAsync(int jobId)
    {
        _logger.LogDebug("Fetching review for job {JobId}", jobId);
        
        return await _dbContext.Reviews
            .Where(r => r.JobId == jobId)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new review to the database.
    /// </summary>
    public async Task<Review> AddAsync(Review review)
    {
        ArgumentNullException.ThrowIfNull(review);
        
        _logger.LogDebug("Adding review for contractor {ContractorId} on job {JobId}", 
            review.ContractorId, review.JobId);
        
        _dbContext.Reviews.Add(review);
        await SaveChangesAsync();
        
        return review;
    }

    /// <summary>
    /// Checks if a review already exists for a job+customer combination.
    /// </summary>
    public async Task<bool> ExistsAsync(int jobId, int customerId)
    {
        _logger.LogDebug("Checking if review exists for job {JobId} and customer {CustomerId}", 
            jobId, customerId);
        
        return await _dbContext.Reviews
            .AnyAsync(r => r.JobId == jobId && r.CustomerId == customerId);
    }

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}

