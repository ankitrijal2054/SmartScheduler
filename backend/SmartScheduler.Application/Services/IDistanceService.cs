using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Service for calculating distance and travel time between two locations.
/// Abstracts Google Maps API and caching logic.
/// </summary>
public interface IDistanceService
{
    /// <summary>
    /// Gets the distance between two geographic coordinates.
    /// </summary>
    /// <param name="originLat">Origin latitude (-90 to 90)</param>
    /// <param name="originLng">Origin longitude (-180 to 180)</param>
    /// <param name="destLat">Destination latitude (-90 to 90)</param>
    /// <param name="destLng">Destination longitude (-180 to 180)</param>
    /// <returns>Distance in miles</returns>
    Task<decimal> GetDistance(decimal originLat, decimal originLng, decimal destLat, decimal destLng);

    /// <summary>
    /// Gets the travel time between two geographic coordinates.
    /// </summary>
    /// <param name="originLat">Origin latitude (-90 to 90)</param>
    /// <param name="originLng">Origin longitude (-180 to 180)</param>
    /// <param name="destLat">Destination latitude (-90 to 90)</param>
    /// <param name="destLng">Destination longitude (-180 to 180)</param>
    /// <returns>Travel time in minutes</returns>
    Task<int> GetTravelTime(decimal originLat, decimal originLng, decimal destLat, decimal destLng);

    /// <summary>
    /// Gets distances for multiple origin-destination pairs in a single batch request.
    /// </summary>
    /// <param name="origins">List of origin coordinates</param>
    /// <param name="destinations">List of destination coordinates</param>
    /// <returns>Matrix of distance results [origin index][destination index]</returns>
    Task<List<List<DistanceResultDto>>> GetDistanceBatch(List<(decimal lat, decimal lng)> origins, List<(decimal lat, decimal lng)> destinations);
}

