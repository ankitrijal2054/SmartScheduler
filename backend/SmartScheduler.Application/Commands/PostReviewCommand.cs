using MediatR;
using SmartScheduler.Domain.Entities;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Command to post a review for a completed job.
/// Returns the created Review entity.
/// </summary>
public class PostReviewCommand : IRequest<Review>
{
    /// <summary>
    /// The ID of the job being reviewed (must be Completed status).
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// The ID of the contractor being reviewed.
    /// </summary>
    public int ContractorId { get; set; }

    /// <summary>
    /// The ID of the customer submitting the review (from JWT token).
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Star rating (1-5 inclusive).
    /// Must be validated to be between 1 and 5.
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Optional review comment (max 500 characters if provided).
    /// </summary>
    public string? Comment { get; set; }

    public PostReviewCommand()
    {
    }

    public PostReviewCommand(int jobId, int contractorId, int customerId, int rating, string? comment = null)
    {
        JobId = jobId;
        ContractorId = contractorId;
        CustomerId = customerId;
        Rating = rating;
        Comment = comment;
    }
}

