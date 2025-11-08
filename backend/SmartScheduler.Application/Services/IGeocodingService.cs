namespace SmartScheduler.Application.Services;

/// <summary>
/// Service interface for geocoding addresses to latitude/longitude coordinates.
/// </summary>
public interface IGeocodingService
{
    /// <summary>
    /// Geocodes an address string to latitude and longitude coordinates.
    /// Returns default coordinates (center of US) on error and logs warning.
    /// Results are cached for 24 hours to reduce API calls.
    /// </summary>
    /// <param name="address">The address string to geocode (e.g., "123 Main St, Springfield, IL")</param>
    /// <returns>Tuple of (latitude, longitude). On error: (39.8283, -98.5795)</returns>
    Task<(double latitude, double longitude)> GeocodeAddressAsync(string address);
}

