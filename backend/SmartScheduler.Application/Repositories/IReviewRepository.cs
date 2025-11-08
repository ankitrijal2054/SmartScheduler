using SmartScheduler.Domain.Entities;

namespace SmartScheduler.Application.Repositories;

/// <summary>
/// Repository interface for Review entity data access.
/// Provides methods for creating, retrieving, and querying reviews.
/// </summary>
public interface IReviewRepository
{
    /// <summary>
    /// Gets all reviews for a specific contractor.
    /// </summary>
    /// <param name="contractorId">The contractor ID.</param>
    /// <returns>List of reviews ordered by creation date descending, empty list if no reviews found.</returns>
    Task<List<Review>> GetByContractorIdAsync(int contractorId);

    /// <summary>
    /// Gets all reviews for a specific job.
    /// </summary>
    /// <param name="jobId">The job ID.</param>
    /// <returns>List of reviews for the job, empty list if none found.</returns>
    Task<List<Review>> GetByJobIdAsync(int jobId);

    /// <summary>
    /// Adds a new review to the database.
    /// </summary>
    /// <param name="review">The review entity to add.</param>
    /// <returns>The added review.</returns>
    Task<Review> AddAsync(Review review);

    /// <summary>
    /// Checks if a review already exists for a job+customer combination (prevents duplicates).
    /// </summary>
    /// <param name="jobId">The job ID.</param>
    /// <param name="customerId">The customer ID.</param>
    /// <returns>True if review exists, false otherwise.</returns>
    Task<bool> ExistsAsync(int jobId, int customerId);

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    /// <returns>A completed task.</returns>
    Task SaveChangesAsync();
}

