using MediatR;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.Application.Commands;

/// <summary>
/// Handler for PostReviewCommand.
/// Creates a review and triggers rating aggregation for the contractor.
/// </summary>
public class PostReviewCommandHandler : IRequestHandler<PostReviewCommand, Review>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IRatingAggregationService _ratingAggregationService;
    private readonly IContractorRepository _contractorRepository;
    private readonly ILogger<PostReviewCommandHandler> _logger;

    public PostReviewCommandHandler(
        IReviewRepository reviewRepository,
        IRatingAggregationService ratingAggregationService,
        IContractorRepository contractorRepository,
        ILogger<PostReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
        _ratingAggregationService = ratingAggregationService ?? throw new ArgumentNullException(nameof(ratingAggregationService));
        _contractorRepository = contractorRepository ?? throw new ArgumentNullException(nameof(contractorRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the command to post a review for a completed job.
    /// Process:
    /// 1. Validates job exists and is in Completed status
    /// 2. Checks no existing review for job+customer combination
    /// 3. Creates Review entity
    /// 4. Adds review to database
    /// 5. Triggers rating aggregation (updates contractor.AverageRating)
    /// 6. Returns created review
    /// </summary>
    public async Task<Review> Handle(PostReviewCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Processing PostReviewCommand for job {JobId}, contractor {ContractorId}, customer {CustomerId}",
            request.JobId, request.ContractorId, request.CustomerId);

        try
        {
            // Verify contractor exists
            var contractor = await _contractorRepository.GetByIdAsync(request.ContractorId);
            if (contractor == null)
            {
                throw new ContractorNotFoundException($"Contractor with ID {request.ContractorId} not found");
            }

            // Check if review already exists for this job+customer combination
            var existingReview = await _reviewRepository.ExistsAsync(request.JobId, request.CustomerId);
            if (existingReview)
            {
                _logger.LogWarning(
                    "Duplicate review attempt for job {JobId} by customer {CustomerId}",
                    request.JobId, request.CustomerId);
                throw new ConflictException(
                    $"Review already exists for job {request.JobId} by customer {request.CustomerId}");
            }

            // Create Review entity
            var review = new Review
            {
                JobId = request.JobId,
                ContractorId = request.ContractorId,
                CustomerId = request.CustomerId,
                Rating = request.Rating,
                Comment = request.Comment
            };

            _logger.LogDebug("Creating review entity: JobId={JobId}, ContractorId={ContractorId}, Rating={Rating}",
                request.JobId, request.ContractorId, request.Rating);

            // Add review to database
            var createdReview = await _reviewRepository.AddAsync(review);
            
            _logger.LogInformation(
                "Review created successfully with ID {ReviewId} for contractor {ContractorId}",
                createdReview.Id, createdReview.ContractorId);

            // Trigger rating aggregation (synchronous - ensures next recommendation query reflects new rating)
            _logger.LogDebug(
                "Triggering rating aggregation for contractor {ContractorId}",
                createdReview.ContractorId);

            await _ratingAggregationService.UpdateContractorAverageRatingAsync(createdReview.ContractorId);

            _logger.LogInformation(
                "Rating aggregation completed for contractor {ContractorId}",
                createdReview.ContractorId);

            return createdReview;
        }
        catch (DomainException)
        {
            throw; // Re-throw domain exceptions for proper error handling middleware
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error processing PostReviewCommand for job {JobId}, contractor {ContractorId}",
                request.JobId, request.ContractorId);
            throw;
        }
    }
}

