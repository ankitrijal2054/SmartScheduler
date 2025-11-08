namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for the API response containing a list of recommended contractors.
/// </summary>
public class RecommendationResponseDto
{
    /// <summary>
    /// List of recommended contractors, sorted by score (highest first).
    /// Maximum of 5 contractors returned.
    /// </summary>
    public List<RecommendationDto> Recommendations { get; set; } = new();

    /// <summary>
    /// Response message providing context or status information.
    /// Examples: "Success", "No available contractors"
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

