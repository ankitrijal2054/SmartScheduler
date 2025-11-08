using System.Text.Json;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Extensions;
using SmartScheduler.Application.Services;

namespace SmartScheduler.Infrastructure.Services;

/// <summary>
/// Implements distance and travel time calculation using Google Maps Distance Matrix API.
/// Includes retry logic, fallback Haversine formula, and error handling.
/// </summary>
public class GoogleMapsDistanceService : IDistanceService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GoogleMapsDistanceService> _logger;

    private const string GoogleMapsApiUrl = "https://maps.googleapis.com/maps/api/distancematrix/json";
    private const int MaxRetries = 3;
    private const int InitialDelayMs = 100;
    private const decimal MetersToMiles = 0.000621371m;
    private const decimal HaversineMultiplier = 1.3m; // Approximate road distance vs straight line

    public GoogleMapsDistanceService(HttpClient httpClient, string apiKey, ILogger<GoogleMapsDistanceService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<decimal> GetDistance(decimal originLat, decimal originLng, decimal destLat, decimal destLng)
    {
        ValidateCoordinates(originLat, originLng, nameof(originLat), nameof(originLng));
        ValidateCoordinates(destLat, destLng, nameof(destLat), nameof(destLng));

        try
        {
            var result = await GetDistanceBatch(
                new List<(decimal, decimal)> { (originLat, originLng) },
                new List<(decimal, decimal)> { (destLat, destLng) }
            );

            return result[0][0].Distance ?? CalculateHaversineDistance(originLat, originLng, destLat, destLng);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distance from ({OriginLat},{OriginLng}) to ({DestLat},{DestLng})", originLat, originLng, destLat, destLng);
            // Return fallback distance using Haversine formula
            return CalculateHaversineDistance(originLat, originLng, destLat, destLng);
        }
    }

    public async Task<int> GetTravelTime(decimal originLat, decimal originLng, decimal destLat, decimal destLng)
    {
        ValidateCoordinates(originLat, originLng, nameof(originLat), nameof(originLng));
        ValidateCoordinates(destLat, destLng, nameof(destLat), nameof(destLng));

        try
        {
            var result = await GetDistanceBatch(
                new List<(decimal, decimal)> { (originLat, originLng) },
                new List<(decimal, decimal)> { (destLat, destLng) }
            );

            return result[0][0].TravelTime ?? EstimateTravelTimeFromDistance(result[0][0].Distance ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting travel time from ({OriginLat},{OriginLng}) to ({DestLat},{DestLng})", originLat, originLng, destLat, destLng);
            // Return estimated travel time (assume 30 mph average)
            var distance = CalculateHaversineDistance(originLat, originLng, destLat, destLng);
            return EstimateTravelTimeFromDistance(distance);
        }
    }

    public async Task<List<List<DistanceResultDto>>> GetDistanceBatch(
        List<(decimal lat, decimal lng)> origins,
        List<(decimal lat, decimal lng)> destinations)
    {
        if (origins == null || origins.Count == 0)
            throw new ArgumentException("Origins cannot be null or empty", nameof(origins));
        if (destinations == null || destinations.Count == 0)
            throw new ArgumentException("Destinations cannot be null or empty", nameof(destinations));

        // Validate all coordinates
        foreach (var (lat, lng) in origins)
            ValidateCoordinates(lat, lng, "origin", "");
        foreach (var (lat, lng) in destinations)
            ValidateCoordinates(lat, lng, "destination", "");

        var originStrings = string.Join("|", origins.Select(c => c.lat.ToGoogleMapsFormat(c.lng)));
        var destStrings = string.Join("|", destinations.Select(c => c.lat.ToGoogleMapsFormat(c.lng)));

        var response = await CallGoogleMapsApiWithRetry(originStrings, destStrings);

        return ParseGoogleMapsResponse(response, origins.Count, destinations.Count);
    }

    private async Task<GoogleMapsDistanceMatrixResponseDto> CallGoogleMapsApiWithRetry(string origins, string destinations)
    {
        int retryCount = 0;
        int delayMs = InitialDelayMs;

        while (retryCount < MaxRetries)
        {
            try
            {
                var requestUrl = $"{GoogleMapsApiUrl}?origins={Uri.EscapeDataString(origins)}&destinations={Uri.EscapeDataString(destinations)}&key={_apiKey}&units=imperial&mode=driving";
                
                var response = await _httpClient.GetAsync(requestUrl);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Google Maps API returned status {StatusCode}", response.StatusCode);
                    retryCount++;
                    if (retryCount < MaxRetries)
                    {
                        await Task.Delay(delayMs);
                        delayMs *= 2; // Exponential backoff
                        continue;
                    }
                    throw new HttpRequestException($"Google Maps API returned {response.StatusCode}");
                }

                var result = JsonSerializer.Deserialize<GoogleMapsDistanceMatrixResponseDto>(content);
                if (result == null)
                    throw new InvalidOperationException("Failed to deserialize Google Maps response");

                return result;
            }
            catch (HttpRequestException ex) when (retryCount < MaxRetries - 1)
            {
                _logger.LogWarning(ex, "Google Maps API request failed, attempt {Attempt}/{MaxRetries}", retryCount + 1, MaxRetries);
                retryCount++;
                await Task.Delay(delayMs);
                delayMs *= 2;
            }
            catch (TimeoutException ex) when (retryCount < MaxRetries - 1)
            {
                _logger.LogWarning(ex, "Google Maps API request timed out, attempt {Attempt}/{MaxRetries}", retryCount + 1, MaxRetries);
                retryCount++;
                await Task.Delay(delayMs);
                delayMs *= 2;
            }
        }

        throw new InvalidOperationException("Google Maps API unreachable after max retries");
    }

    private List<List<DistanceResultDto>> ParseGoogleMapsResponse(
        GoogleMapsDistanceMatrixResponseDto response,
        int originCount,
        int destinationCount)
    {
        var results = new List<List<DistanceResultDto>>();

        if (response.Status != "OK")
        {
            _logger.LogError("Google Maps API error: {ErrorMessage}", response.ErrorMessage ?? response.Status);
            // Return empty matrix with error status
            for (int i = 0; i < originCount; i++)
            {
                var row = new List<DistanceResultDto>();
                for (int j = 0; j < destinationCount; j++)
                {
                    row.Add(new DistanceResultDto
                    {
                        Status = response.Status,
                        ErrorMessage = response.ErrorMessage ?? "API request failed"
                    });
                }
                results.Add(row);
            }
            return results;
        }

        if (response.Rows.Count != originCount)
        {
            _logger.LogWarning("Response row count {RowCount} doesn't match origin count {OriginCount}", response.Rows.Count, originCount);
        }

        foreach (var row in response.Rows)
        {
            var rowResults = new List<DistanceResultDto>();
            
            foreach (var element in row.Elements)
            {
                var result = new DistanceResultDto { Status = element.Status };

                if (element.Status == "OK" && element.Distance != null && element.Duration != null)
                {
                    // Convert meters to miles
                    result.Distance = element.Distance.Value * MetersToMiles;
                    // Convert seconds to minutes
                    result.TravelTime = (int)Math.Ceiling(element.Duration.Value / 60m);
                }
                else if (element.Status != "OK")
                {
                    result.ErrorMessage = element.ErrorMessage ?? $"Status: {element.Status}";
                    _logger.LogWarning("Distance calculation failed with status {Status}: {ErrorMessage}", element.Status, result.ErrorMessage);
                }

                rowResults.Add(result);
            }

            results.Add(rowResults);
        }

        return results;
    }

    /// <summary>
    /// Calculates distance using Haversine formula (great circle distance).
    /// Result is multiplied by 1.3x to approximate road distance.
    /// </summary>
    private decimal CalculateHaversineDistance(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
    {
        const decimal earthRadiusMiles = 3959; // Earth's radius in miles

        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);

        var a = (decimal)Math.Sin((double)dLat / 2) * (decimal)Math.Sin((double)dLat / 2) +
                (decimal)Math.Cos((double)lat1 * Math.PI / 180) * (decimal)Math.Cos((double)lat2 * Math.PI / 180) *
                (decimal)Math.Sin((double)dLng / 2) * (decimal)Math.Sin((double)dLng / 2);

        var c = 2 * (decimal)Math.Atan2(Math.Sqrt((double)a), Math.Sqrt((double)(1 - a)));
        var distance = earthRadiusMiles * c;

        // Approximate road distance by multiplying by 1.3x
        return distance * HaversineMultiplier;
    }

    private decimal ToRadians(decimal degrees)
    {
        return degrees * (decimal)Math.PI / 180;
    }

    private int EstimateTravelTimeFromDistance(decimal distanceMiles)
    {
        // Assume average speed of 30 mph for fallback
        const decimal averageSpeedMph = 30;
        var hours = distanceMiles / averageSpeedMph;
        return (int)Math.Ceiling(hours * 60);
    }

    private void ValidateCoordinates(decimal lat, decimal lng, string latParamName, string lngParamName)
    {
        if (!lat.IsValidLatitude())
            throw new ArgumentException($"Latitude must be between -90 and 90, got {lat}", latParamName);
        if (!lng.IsValidLongitude())
            throw new ArgumentException($"Longitude must be between -180 and 180, got {lng}", lngParamName);
    }
}

