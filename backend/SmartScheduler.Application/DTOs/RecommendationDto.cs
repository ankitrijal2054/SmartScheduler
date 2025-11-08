namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for a single contractor recommendation with scoring information.
/// </summary>
public class RecommendationDto
{
    /// <summary>
    /// Contractor's unique ID.
    /// </summary>
    public int ContractorId { get; set; }

    /// <summary>
    /// Contractor's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Calculated recommendation score (0.0 - 1.0, higher is better).
    /// Formula: (0.4 × availabilityScore) + (0.3 × ratingScore) + (0.3 × distanceScore)
    /// </summary>
    public decimal Score { get; set; }

    /// <summary>
    /// Contractor's average rating (null if no reviews).
    /// </summary>
    public decimal? Rating { get; set; }

    /// <summary>
    /// Number of reviews received by the contractor.
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Distance from job location to contractor's location (in miles).
    /// </summary>
    public decimal Distance { get; set; }

    /// <summary>
    /// Estimated travel time from contractor's location to job location (in minutes).
    /// </summary>
    public int TravelTime { get; set; }

    /// <summary>
    /// List of available time slots for the contractor on the desired date (ISO 8601 format).
    /// Each slot represents a 1-hour window when the contractor is available.
    /// </summary>
    public List<DateTime> AvailableTimeSlots { get; set; } = new();
}

