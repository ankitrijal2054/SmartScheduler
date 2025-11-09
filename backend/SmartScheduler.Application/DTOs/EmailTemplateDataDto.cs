namespace SmartScheduler.Application.DTOs;

/// <summary>
/// Data transfer object for email template rendering.
/// Contains all data required to render email templates for customer notifications.
/// </summary>
public class EmailTemplateDataDto
{
    /// <summary>
    /// Gets or sets the customer email address.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the job ID.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// Gets or sets the job type/trade type.
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the job location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the job description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the desired date and time for the job.
    /// </summary>
    public DateTime DesiredDateTime { get; set; }

    /// <summary>
    /// Gets or sets the contractor name.
    /// </summary>
    public string ContractorName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contractor phone number.
    /// </summary>
    public string ContractorPhone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contractor average rating (nullable).
    /// </summary>
    public decimal? ContractorRating { get; set; }

    /// <summary>
    /// Gets or sets the estimated time of arrival (ETA) for the contractor.
    /// </summary>
    public string ETA { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the job tracking URL for the customer to view job status.
    /// </summary>
    public string JobTrackingUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rating form URL for the customer to submit a review.
    /// </summary>
    public string RatingUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contractor email address (for contractor notifications).
    /// </summary>
    public string ContractorEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the old scheduled date and time (for schedule change notifications).
    /// </summary>
    public DateTime? OldScheduledDateTime { get; set; }

    /// <summary>
    /// Gets or sets the new scheduled date and time (for schedule change notifications).
    /// </summary>
    public DateTime? NewScheduledDateTime { get; set; }

    /// <summary>
    /// Gets or sets the cancellation reason (for job cancelled notifications).
    /// </summary>
    public string CancellationReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rating value (1-5 stars) for rating notifications.
    /// </summary>
    public int? Rating { get; set; }

    /// <summary>
    /// Gets or sets the review comment from the customer.
    /// </summary>
    public string ReviewComment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contractor profile URL.
    /// </summary>
    public string ContractorProfileUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the job accept link for contractor notifications.
    /// </summary>
    public string AcceptJobLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the job decline link for contractor notifications.
    /// </summary>
    public string DeclineJobLink { get; set; } = string.Empty;
}

