using SmartScheduler.Application.DTOs;
using SmartScheduler.Domain.Entities;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Service interface for calculating contractor scores and generating recommendations.
/// Implements the intelligent ranking algorithm based on availability, rating, and distance.
/// </summary>
public interface IScoringService
{
    /// <summary>
    /// Gets top 5 recommended contractors for a job, ranked by calculated score.
    /// </summary>
    /// <param name="jobId">The ID of the job to get recommendations for.</param>
    /// <param name="dispatcherId">The ID of the dispatcher making the request.</param>
    /// <param name="contractorListOnly">If true, only consider contractors in the dispatcher's personal list.</param>
    /// <returns>RecommendationResponseDto with top 5 ranked contractors (or empty list if none available).</returns>
    /// <exception cref="ArgumentException">If jobId or dispatcherId is invalid.</exception>
    /// <exception cref="SmartScheduler.Domain.Exceptions.NotFoundException">If job not found.</exception>
    Task<RecommendationResponseDto> GetRecommendationsAsync(int jobId, int dispatcherId, bool contractorListOnly = false);

    /// <summary>
    /// Calculates the recommendation score for a single contractor based on availability, rating, and distance.
    /// </summary>
    /// <param name="availabilityScore">Availability score (1.0 if available, 0.0 if not).</param>
    /// <param name="ratingScore">Rating score normalized to 0.0-1.0 range (null rating = 0.5).</param>
    /// <param name="distanceScore">Distance score normalized to 0.0-1.0 range (0 miles = 1.0, >50 miles = 0.0).</param>
    /// <returns>Final score (0.0 - 1.0) using weighted formula: (0.4 × availability) + (0.3 × rating) + (0.3 × distance).</returns>
    decimal CalculateScore(decimal availabilityScore, decimal ratingScore, decimal distanceScore);

    /// <summary>
    /// Calculates the available time slots for a contractor on a specific date.
    /// </summary>
    /// <param name="contractorId">The ID of the contractor.</param>
    /// <param name="desiredDate">The date to check availability for.</param>
    /// <returns>List of available 1-hour time slots (in UTC) when the contractor can work.</returns>
    /// <exception cref="ArgumentException">If contractorId is invalid or desiredDate is invalid.</exception>
    /// <exception cref="SmartScheduler.Domain.Exceptions.NotFoundException">If contractor not found.</exception>
    Task<List<DateTime>> GetAvailableTimeSlotsAsync(int contractorId, DateTime desiredDate);

    /// <summary>
    /// Normalizes a rating (0-5 stars) to a 0.0-1.0 score range.
    /// </summary>
    /// <param name="rating">The rating value (typically 0-5, null if no reviews).</param>
    /// <returns>Normalized rating score (0.0-1.0), or 0.5 if rating is null.</returns>
    decimal NormalizeRatingScore(decimal? rating);

    /// <summary>
    /// Normalizes distance (in miles) to a 0.0-1.0 score range.
    /// Closer distances receive higher scores (0 miles = 1.0, 50+ miles = 0.0).
    /// </summary>
    /// <param name="distanceMiles">Distance in miles.</param>
    /// <returns>Normalized distance score (0.0-1.0).</returns>
    decimal NormalizeDistanceScore(decimal distanceMiles);
}

