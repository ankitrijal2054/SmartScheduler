namespace SmartScheduler.Application.DTOs;

/// <summary>
/// Result of a distance/travel time calculation between two locations.
/// </summary>
public class DistanceResultDto
{
    /// <summary>
    /// Distance in miles. Null if unavailable.
    /// </summary>
    public decimal? Distance { get; set; }

    /// <summary>
    /// Travel time in minutes. Null if unavailable.
    /// </summary>
    public int? TravelTime { get; set; }

    /// <summary>
    /// Status of the distance calculation.
    /// Values: "OK", "ZERO_RESULTS", "NOT_FOUND", "REQUEST_DENIED", "FALLBACK_USED"
    /// </summary>
    public string Status { get; set; } = "OK";

    /// <summary>
    /// Error message if the calculation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

