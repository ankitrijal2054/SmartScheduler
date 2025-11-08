namespace SmartScheduler.Application.DTOs;

/// <summary>
/// Structures a request to Google Maps Distance Matrix API.
/// </summary>
public class GoogleMapsDistanceMatrixRequestDto
{
    /// <summary>
    /// Comma-separated list of origins (lat,lng format).
    /// Example: "40.7128,-74.0060|41.8781,-87.6298"
    /// </summary>
    public string Origins { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of destinations (lat,lng format).
    /// </summary>
    public string Destinations { get; set; } = string.Empty;

    /// <summary>
    /// Google Maps API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Preferred units for distance (metric or imperial).
    /// </summary>
    public string Units { get; set; } = "imperial"; // Returns distance in miles

    /// <summary>
    /// Travel mode (driving, walking, transit, bicycling).
    /// </summary>
    public string Mode { get; set; } = "driving";
}

