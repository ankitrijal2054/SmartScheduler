using MediatR;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;

namespace SmartScheduler.Application.Queries;

/// <summary>
/// Handler for GetContractorJobHistoryQuery.
/// Retrieves contractor's job history with optional filtering and pagination.
/// Includes customer review data if available.
/// </summary>
public class GetContractorJobHistoryQueryHandler : IRequestHandler<GetContractorJobHistoryQuery, JobHistoryResponseDto>
{
    private readonly IAssignmentRepository _assignmentRepository;

    public GetContractorJobHistoryQueryHandler(
        IAssignmentRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
    }

    /// <summary>
    /// Handles the query by fetching job history with filters and pagination.
    /// Validates that startDate is before endDate if both are provided.
    /// </summary>
    public async Task<JobHistoryResponseDto> Handle(GetContractorJobHistoryQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate date range
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
        {
            throw new InvalidOperationException("startDate must be before endDate");
        }

        // Get job history from repository
        var (assignments, totalCount) = await _assignmentRepository.GetContractorJobsWithReviewsAsync(
            request.ContractorId,
            request.StartDate,
            request.EndDate,
            request.Skip,
            request.Take);

        // Build DTO list with review data
        var jobHistoryDtos = new List<JobHistoryItemDto>();

        foreach (var assignment in assignments)
        {
            var job = assignment.Job;
            var review = job?.Review;
            var customer = job?.Customer;

            // Map assignment status to frontend format
            string status = assignment.Status.ToString().ToLower();
            if (status == "inprogress")
            {
                status = "in-progress";
            }
            else if (status == "declined")
            {
                status = "cancelled"; // Map declined to cancelled for frontend
            }
            // Ensure status is one of: completed, cancelled, in-progress
            if (status != "completed" && status != "cancelled" && status != "in-progress")
            {
                // Default to completed if status doesn't match expected values
                status = "completed";
            }

            jobHistoryDtos.Add(new JobHistoryItemDto
            {
                Id = assignment.Id,
                JobId = assignment.JobId,
                JobType = job?.JobType.ToString() ?? "Unknown",
                Location = job?.Location ?? "Unknown",
                ScheduledDateTime = job?.DesiredDateTime ?? assignment.CreatedAt,
                Status = status,
                CustomerName = customer?.Name ?? "Unknown Customer",
                CustomerRating = review?.Rating,
                CustomerReviewText = review?.Comment,
                AcceptedAt = assignment.AcceptedAt,
                CompletedAt = assignment.CompletedAt
            });
        }

        return new JobHistoryResponseDto
        {
            Assignments = jobHistoryDtos,
            TotalCount = totalCount
        };
    }
}

