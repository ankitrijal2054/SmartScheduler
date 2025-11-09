using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Application.Queries;

/// <summary>
/// Handler for GetContractorJobHistoryQuery.
/// Retrieves contractor's job history with optional filtering and pagination.
/// Includes customer review data if available.
/// </summary>
public class GetContractorJobHistoryQueryHandler : IRequestHandler<GetContractorJobHistoryQuery, JobHistoryResponseDto>
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ApplicationDbContext _dbContext;

    public GetContractorJobHistoryQueryHandler(
        IAssignmentRepository assignmentRepository,
        ApplicationDbContext dbContext)
    {
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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
            // Get review for this job if it exists
            var review = await _dbContext.Reviews
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.JobId == assignment.JobId, cancellationToken);

            jobHistoryDtos.Add(new JobHistoryItemDto
            {
                Id = assignment.Id,
                JobId = assignment.JobId,
                JobType = assignment.Job?.Type.ToString() ?? "Unknown",
                Location = assignment.Job?.Location ?? "Unknown",
                ScheduledDateTime = assignment.Job?.DesiredDateTime ?? DateTime.MinValue,
                Status = assignment.Status.ToString(),
                CustomerName = assignment.Job?.Customer?.Name ?? "Unknown Customer",
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

