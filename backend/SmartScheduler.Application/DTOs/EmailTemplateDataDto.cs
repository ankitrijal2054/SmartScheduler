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
}

