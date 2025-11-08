namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for requesting contractor recommendations for a job.
/// </summary>
public class RecommendationRequestDto
{
    /// <summary>
    /// The ID of the job to get recommendations for.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// Optional: If true, only recommend contractors from the dispatcher's personal list.
    /// If false (default), all active contractors are considered.
    /// </summary>
    public bool ContractorListOnly { get; set; } = false;
}

