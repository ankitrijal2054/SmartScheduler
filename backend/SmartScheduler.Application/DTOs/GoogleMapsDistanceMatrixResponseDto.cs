using System.Text.Json.Serialization;

namespace SmartScheduler.Application.DTOs;

/// <summary>
/// Parses response from Google Maps Distance Matrix API.
/// </summary>
public class GoogleMapsDistanceMatrixResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("rows")]
    public List<GoogleMapsDistanceMatrixRowDto> Rows { get; set; } = [];

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents a row in the distance matrix response (one origin).
/// </summary>
public class GoogleMapsDistanceMatrixRowDto
{
    [JsonPropertyName("elements")]
    public List<GoogleMapsDistanceMatrixElementDto> Elements { get; set; } = [];
}

/// <summary>
/// Represents a single distance/duration result (origin to destination).
/// </summary>
public class GoogleMapsDistanceMatrixElementDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("distance")]
    public GoogleMapsValueDto? Distance { get; set; }

    [JsonPropertyName("duration")]
    public GoogleMapsValueDto? Duration { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Represents a value object in Google Maps API response.
/// </summary>
public class GoogleMapsValueDto
{
    [JsonPropertyName("value")]
    public long Value { get; set; } // Distance in meters or duration in seconds

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty; // Human-readable text
}

