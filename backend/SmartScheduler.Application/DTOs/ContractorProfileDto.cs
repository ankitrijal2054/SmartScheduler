namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for contractor profile data with aggregated statistics.
/// Used by the GetContractorProfile query.
/// </summary>
public class ContractorProfileDto
{
    /// <summary>
    /// Contractor's unique ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Contractor's full name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Average rating (1-5 scale, nullable if no reviews).
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Total number of reviews received.
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Total number of jobs assigned to this contractor.
    /// </summary>
    public int TotalJobsAssigned { get; set; }

    /// <summary>
    /// Total number of jobs accepted by this contractor.
    /// </summary>
    public int TotalJobsAccepted { get; set; }

    /// <summary>
    /// Total number of jobs completed by this contractor.
    /// </summary>
    public int TotalJobsCompleted { get; set; }

    /// <summary>
    /// Acceptance rate as a percentage (0-100).
    /// Calculated as: (accepted / assigned) * 100, returns 0 if assigned = 0.
    /// </summary>
    public decimal AcceptanceRate { get; set; }

    /// <summary>
    /// Total earnings (optional, may be null in MVP).
    /// </summary>
    public decimal? TotalEarnings { get; set; }

    /// <summary>
    /// When the contractor profile was created (ISO 8601).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// List of recent reviews (last 5, sorted by date descending).
    /// </summary>
    public List<CustomerReviewDto> RecentReviews { get; set; } = new();
}

/// <summary>
/// DTO for a customer review of a contractor.
/// </summary>
public class CustomerReviewDto
{
    /// <summary>
    /// Review's unique ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Rating given (1-5 scale).
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Optional review comment.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Name of the customer who wrote the review.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Type of job this review was for (e.g., "Plumbing", "HVAC").
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// When the review was created (ISO 8601).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for contractor job history item (used in job history list).
/// </summary>
public class JobHistoryItemDto
{
    /// <summary>
    /// Assignment ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Job ID.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// Job type (e.g., "Plumbing", "HVAC").
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Job location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// When the job is scheduled (ISO 8601).
    /// </summary>
    public DateTime ScheduledDateTime { get; set; }

    /// <summary>
    /// Current assignment status: Pending, Accepted, InProgress, Completed, Cancelled.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Name of the customer who posted the job.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Customer's rating for this job (nullable if not yet rated).
    /// </summary>
    public int? CustomerRating { get; set; }

    /// <summary>
    /// Customer's review comment for this job (nullable).
    /// </summary>
    public string? CustomerReviewText { get; set; }

    /// <summary>
    /// When the contractor accepted this job (nullable).
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// When the contractor completed this job (nullable).
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// Response DTO for job history query with pagination metadata.
/// </summary>
public class JobHistoryResponseDto
{
    /// <summary>
    /// List of job history items.
    /// </summary>
    public List<JobHistoryItemDto> Assignments { get; set; } = new();

    /// <summary>
    /// Total count of assignments matching the filter (for pagination calculation).
    /// </summary>
    public int TotalCount { get; set; }
}

