using Microsoft.Extensions.Logging;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Service implementation for calculating and updating contractor average ratings.
/// Implements synchronous aggregation triggered immediately after review creation.
/// </summary>
public class RatingAggregationService : IRatingAggregationService
{
    private readonly IContractorRepository _contractorRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<RatingAggregationService> _logger;

    public RatingAggregationService(
        IContractorRepository contractorRepository,
        IReviewRepository reviewRepository,
        ILogger<RatingAggregationService> logger)
    {
        _contractorRepository = contractorRepository ?? throw new ArgumentNullException(nameof(contractorRepository));
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates and updates the average rating for a contractor.
    /// Implementation:
    /// 1. Fetches contractor; throws ContractorNotFoundException if not found
    /// 2. Fetches all reviews for contractor
    /// 3. If reviews exist: calculates average, updates AverageRating and ReviewCount
    /// 4. If no reviews: sets AverageRating to null, ReviewCount to 0
    /// 5. Persists changes to database
    /// 6. Logs the update for audit trail
    /// </summary>
    public async Task<Contractor> UpdateContractorAverageRatingAsync(int contractorId)
    {
        try
        {
            // Fetch contractor from database (tracked entity for update)
            var contractor = await _contractorRepository.GetByIdAsync(contractorId);
            
            if (contractor == null)
            {
                _logger.LogWarning("Contractor {ContractorId} not found for rating aggregation", contractorId);
                throw new ContractorNotFoundException($"Contractor with ID {contractorId} not found");
            }

            // Fetch all reviews for contractor
            var reviews = await _reviewRepository.GetByContractorIdAsync(contractorId);
            
            _logger.LogDebug("Found {ReviewCount} reviews for contractor {ContractorId}", 
                reviews.Count, contractorId);

            // Calculate average based on review count
            if (reviews.Count > 0)
            {
                // Calculate average: SUM(ratings) / COUNT(reviews)
                var sum = reviews.Sum(r => r.Rating);
                var average = (decimal)sum / reviews.Count;
                
                // Round to 2 decimal places for DECIMAL(3,2) database column
                contractor.AverageRating = Math.Round(average, 2);
                contractor.ReviewCount = reviews.Count;
                
                _logger.LogInformation(
                    "Updated contractor {ContractorId} rating to {AverageRating} from {ReviewCount} reviews",
                    contractorId, contractor.AverageRating, reviews.Count);
            }
            else
            {
                // No reviews: set average to null (not 0) to distinguish "no data" from "poor rating"
                contractor.AverageRating = null;
                contractor.ReviewCount = 0;
                
                _logger.LogInformation(
                    "Contractor {ContractorId} has no reviews; rating set to null",
                    contractorId);
            }

            // Persist changes to database
            await _contractorRepository.SaveChangesAsync();
            
            _logger.LogInformation(
                "Successfully persisted rating update for contractor {ContractorId}",
                contractorId);

            return contractor;
        }
        catch (ContractorNotFoundException)
        {
            throw; // Re-throw domain exceptions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error updating contractor {ContractorId} average rating",
                contractorId);
            throw;
        }
    }
}

