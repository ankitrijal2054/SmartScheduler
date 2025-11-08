using SmartScheduler.Domain.Entities;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Service interface for calculating and updating contractor average ratings.
/// Aggregates all reviews for a contractor and updates the average rating field.
/// </summary>
public interface IRatingAggregationService
{
    /// <summary>
    /// Calculates and updates the average rating for a contractor based on all submitted reviews.
    /// Sets AverageRating to null if no reviews exist.
    /// Invalidates recommendation cache to ensure fresh scoring.
    /// </summary>
    /// <param name="contractorId">The contractor ID to update</param>
    /// <returns>Updated Contractor entity with new averageRating and reviewCount</returns>
    /// <exception cref="ContractorNotFoundException">Thrown if contractor does not exist</exception>
    Task<Contractor> UpdateContractorAverageRatingAsync(int contractorId);
}

