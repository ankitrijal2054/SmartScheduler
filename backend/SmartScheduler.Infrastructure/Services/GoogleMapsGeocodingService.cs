using SmartScheduler.Application.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;

namespace SmartScheduler.Infrastructure.Services;

/// <summary>
/// Service for geocoding addresses using Google Maps Geocoding API.
/// Caches results in memory for 24 hours to reduce API calls.
/// </summary>
public class GoogleMapsGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly string? _googleMapsApiKey;
    private readonly ILogger<GoogleMapsGeocodingService> _logger;
    private readonly Dictionary<string, (double latitude, double longitude, DateTime expiry)> _cache;

    // Default coordinates: Center of United States
    private const double DefaultLatitude = 39.8283;
    private const double DefaultLongitude = -98.5795;
    private const int CacheExpiryHours = 24;

    public GoogleMapsGeocodingService(
        HttpClient httpClient,
        ILogger<GoogleMapsGeocodingService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _googleMapsApiKey = Environment.GetEnvironmentVariable("GOOGLE_MAPS_API_KEY");
        _cache = new Dictionary<string, (double, double, DateTime)>();
    }

    /// <summary>
    /// Geocodes an address string to latitude and longitude coordinates.
    /// Returns cached result if available and not expired.
    /// On error, returns default coordinates and logs warning.
    /// </summary>
    public async Task<(double latitude, double longitude)> GeocodeAddressAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            _logger.LogWarning("Geocoding called with empty address");
            return (DefaultLatitude, DefaultLongitude);
        }

        // Normalize address for caching (remove extra spaces, lowercase)
        var cacheKey = address.Trim().ToLowerInvariant();

        // Check cache first
        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            if (DateTime.UtcNow < cachedResult.expiry)
            {
                _logger.LogInformation("Geocoding cache hit for address: {Address}", address);
                return (cachedResult.latitude, cachedResult.longitude);
            }
            else
            {
                // Remove expired cache entry
                _cache.Remove(cacheKey);
            }
        }

        try
        {
            // If API key not configured, use mock/default coordinates
            if (string.IsNullOrEmpty(_googleMapsApiKey))
            {
                _logger.LogInformation("Google Maps API key not configured. Using mock geocoding for address: {Address}", address);
                return await MockGeocodeAddressAsync(address);
            }

            // Call actual Google Maps API
            return await CallGoogleMapsApiAsync(address, cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Geocoding failed for address: {Address}. Using default coordinates.", address);
            return (DefaultLatitude, DefaultLongitude);
        }
    }

    /// <summary>
    /// Mock geocoding service for development/testing when API key is not configured.
    /// Returns consistent coordinates based on address keywords.
    /// </summary>
    private async Task<(double latitude, double longitude)> MockGeocodeAddressAsync(string address)
    {
        await Task.Delay(10); // Simulate API latency

        var (lat, lng) = address.ToLowerInvariant() switch
        {
            // Sample US locations
            var a when a.Contains("new york") || a.Contains("nyc") => (40.7128, -74.0060),
            var a when a.Contains("los angeles") || a.Contains("la") => (34.0522, -118.2437),
            var a when a.Contains("chicago") => (41.8781, -87.6298),
            var a when a.Contains("houston") => (29.7604, -95.3698),
            var a when a.Contains("phoenix") => (33.4484, -112.0742),
            var a when a.Contains("philadelphia") || a.Contains("philly") => (39.9526, -75.1652),
            var a when a.Contains("san antonio") => (29.4241, -98.4936),
            var a when a.Contains("san diego") => (32.7157, -117.1611),
            var a when a.Contains("dallas") => (32.7767, -96.7970),
            var a when a.Contains("san jose") => (37.3382, -121.8863),
            var a when a.Contains("springfield") && a.Contains("illinois") => (39.7817, -89.6501),
            var a when a.Contains("springfield") && a.Contains("missouri") => (37.2090, -93.2923),
            var a when a.Contains("denver") => (39.7392, -104.9903),
            var a when a.Contains("seattle") => (47.6062, -122.3321),
            var a when a.Contains("boston") => (42.3601, -71.0589),
            var a when a.Contains("miami") => (25.7617, -80.1918),
            var a when a.Contains("atlanta") => (33.7490, -84.3880),
            // Default to US center
            _ => (DefaultLatitude, DefaultLongitude)
        };

        _logger.LogInformation("Mock geocoding for address: {Address} -> ({Latitude}, {Longitude})", address, lat, lng);

        // Cache the result
        var cacheKey = address.Trim().ToLowerInvariant();
        _cache[cacheKey] = (lat, lng, DateTime.UtcNow.AddHours(CacheExpiryHours));

        return (lat, lng);
    }

    /// <summary>
    /// Calls the actual Google Maps Geocoding API.
    /// </summary>
    private async Task<(double latitude, double longitude)> CallGoogleMapsApiAsync(string address, string cacheKey)
    {
        const string googleMapsApiUrl = "https://maps.googleapis.com/maps/api/geocode/json";

        var requestUrl = $"{googleMapsApiUrl}?address={Uri.EscapeDataString(address)}&key={_googleMapsApiKey}";

        using var response = await _httpClient.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonContent);

        var root = doc.RootElement;

        // Check for API errors
        if (root.TryGetProperty("status", out var statusElement))
        {
            var status = statusElement.GetString();
            if (status != "OK")
            {
                _logger.LogWarning("Google Maps API returned status: {Status} for address: {Address}", status, address);
                return (DefaultLatitude, DefaultLongitude);
            }
        }

        // Extract first result's coordinates
        if (root.TryGetProperty("results", out var resultsElement) && resultsElement.GetArrayLength() > 0)
        {
            var firstResult = resultsElement[0];

            if (firstResult.TryGetProperty("geometry", out var geometryElement) &&
                geometryElement.TryGetProperty("location", out var locationElement))
            {
                if (locationElement.TryGetProperty("lat", out var latElement) &&
                    locationElement.TryGetProperty("lng", out var lngElement))
                {
                    var latitude = latElement.GetDouble();
                    var longitude = lngElement.GetDouble();

                    // Cache the result
                    _cache[cacheKey] = (latitude, longitude, DateTime.UtcNow.AddHours(CacheExpiryHours));

                    _logger.LogInformation("Geocoded address: {Address} -> ({Latitude}, {Longitude})", address, latitude, longitude);

                    return (latitude, longitude);
                }
            }
        }

        _logger.LogWarning("No results found in Google Maps API response for address: {Address}", address);
        return (DefaultLatitude, DefaultLongitude);
    }
}

