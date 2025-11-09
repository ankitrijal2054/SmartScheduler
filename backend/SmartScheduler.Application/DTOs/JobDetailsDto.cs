namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for complete job details shown in contractor modal.
/// Includes assignment, job, customer, and review history.
/// </summary>
public class JobDetailsDto
{
    // Assignment info
    public string AssignmentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Accepted, InProgress, Completed, Declined, Cancelled
    public string AssignedAt { get; set; } = string.Empty; // ISO 8601 datetime
    public string? AcceptedAt { get; set; }

    // Job info
    public string JobId { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string DesiredDateTime { get; set; } = string.Empty; // ISO 8601 datetime
    public string Description { get; set; } = string.Empty;
    public int? EstimatedDuration { get; set; } // Minutes
    public decimal? EstimatedPay { get; set; } // Dollar amount

    // Customer info
    public JobDetailCustomerDto Customer { get; set; } = new();

    // Customer's past jobs with this contractor
    public List<JobDetailReviewDto> PastReviews { get; set; } = new();
}

/// <summary>
/// Customer info as shown in job details modal.
/// </summary>
public class JobDetailCustomerDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? Rating { get; set; } // Average rating (null if no reviews)
    public int ReviewCount { get; set; }
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Review record showing past job history with this customer.
/// </summary>
public class JobDetailReviewDto
{
    public string Id { get; set; } = string.Empty;
    public string JobId { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public decimal Rating { get; set; } // 1-5 stars
    public string? Comment { get; set; }
    public string CreatedAt { get; set; } = string.Empty; // ISO 8601 datetime
}

