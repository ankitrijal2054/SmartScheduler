namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for a contractor item in the dispatcher's curated list.
/// Includes contractor details and the timestamp when added.
/// </summary>
public class ContractorListItemDto
{
    /// <summary>
    /// Contractor's unique ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Contractor's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's phone number (non-masked full number for dispatcher).
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Trade type the contractor specializes in.
    /// </summary>
    public string TradeType { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's average rating (null if no reviews).
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Number of reviews received.
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Total number of jobs completed by contractor.
    /// </summary>
    public int TotalJobsCompleted { get; set; }

    /// <summary>
    /// Whether the contractor is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Timestamp when contractor was added to dispatcher's list (UTC).
    /// </summary>
    public DateTime AddedAt { get; set; }
}

