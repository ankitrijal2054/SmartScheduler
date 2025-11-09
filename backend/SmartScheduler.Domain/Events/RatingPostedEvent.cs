namespace SmartScheduler.Domain.Events;

/// <summary>
/// Domain event raised when a customer posts a rating/review for a completed job.
/// Used to trigger email notification to the contractor about their rating.
/// </summary>
public class RatingPostedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the review ID for the rating.
    /// </summary>
    public int ReviewId { get; }

    /// <summary>
    /// Gets the job ID that was rated.
    /// </summary>
    public int JobId { get; }

    /// <summary>
    /// Gets the contractor ID who is being rated.
    /// </summary>
    public int ContractorId { get; }

    /// <summary>
    /// Gets the customer ID who submitted the rating.
    /// </summary>
    public int CustomerId { get; }

    /// <summary>
    /// Gets the rating value (1-5 stars).
    /// </summary>
    public int Rating { get; }

    /// <summary>
    /// Gets the optional review comment from the customer.
    /// </summary>
    public string? Comment { get; }

    public RatingPostedEvent(int reviewId, int jobId, int contractorId, int customerId, int rating, string? comment = null)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        ReviewId = reviewId;
        JobId = jobId;
        ContractorId = contractorId;
        CustomerId = customerId;
        Rating = rating;
        Comment = comment;
    }
}

