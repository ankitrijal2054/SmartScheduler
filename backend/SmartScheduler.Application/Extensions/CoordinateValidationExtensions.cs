namespace SmartScheduler.Application.Extensions;

/// <summary>
/// Extension methods for validating geographic coordinates.
/// </summary>
public static class CoordinateValidationExtensions
{
    /// <summary>
    /// Validates if latitude is within valid range: -90 to 90.
    /// </summary>
    public static bool IsValidLatitude(this decimal latitude)
    {
        return latitude >= -90 && latitude <= 90;
    }

    /// <summary>
    /// Validates if longitude is within valid range: -180 to 180.
    /// </summary>
    public static bool IsValidLongitude(this decimal longitude)
    {
        return longitude >= -180 && longitude <= 180;
    }

    /// <summary>
    /// Validates if both latitude and longitude are valid coordinates.
    /// </summary>
    public static bool AreValidCoordinates(this decimal latitude, decimal longitude)
    {
        return latitude.IsValidLatitude() && longitude.IsValidLongitude();
    }

    /// <summary>
    /// Formats coordinates for Google Maps API call (lat,lng format).
    /// </summary>
    public static string ToGoogleMapsFormat(this decimal latitude, decimal longitude)
    {
        return $"{latitude},{longitude}";
    }
}

