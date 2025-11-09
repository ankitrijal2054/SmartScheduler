using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Application.Queries;

/// <summary>
/// Handler for GetContractorProfileQuery.
/// Aggregates contractor statistics and recent reviews.
/// </summary>
public class GetContractorProfileQueryHandler : IRequestHandler<GetContractorProfileQuery, ContractorProfileDto>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAssignmentRepository _assignmentRepository;

    public GetContractorProfileQueryHandler(
        ApplicationDbContext dbContext,
        IAssignmentRepository assignmentRepository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
    }

    /// <summary>
    /// Handles the query by aggregating contractor profile data and recent reviews.
    /// </summary>
    public async Task<ContractorProfileDto> Handle(GetContractorProfileQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get contractor data
        var contractor = await _dbContext.Contractors
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ContractorId, cancellationToken);

        if (contractor == null)
        {
            throw new InvalidOperationException($"Contractor with ID {request.ContractorId} not found.");
        }

        // Get assignment statistics
        var assignments = await _dbContext.Assignments
            .Where(a => a.ContractorId == request.ContractorId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalAssigned = assignments.Count;
        var totalAccepted = assignments.Count(a => a.AcceptedAt.HasValue);
        var totalCompleted = assignments.Count(a => a.Status == Domain.Enums.AssignmentStatus.Completed);

        // Calculate acceptance rate
        var acceptanceRate = totalAssigned > 0
            ? (decimal)totalAccepted / totalAssigned * 100
            : 0;

        // Get recent reviews (last 5, sorted by date descending)
        var recentReviews = await _dbContext.Reviews
            .Where(r => r.ContractorId == request.ContractorId)
            .Include(r => r.Job)
            .Include(r => r.Customer)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var recentReviewDtos = recentReviews
            .Select(r => new CustomerReviewDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                CustomerName = r.Customer?.Name ?? "Unknown Customer",
                JobType = r.Job?.Type.ToString() ?? "Unknown",
                CreatedAt = r.CreatedAt
            })
            .ToList();

        return new ContractorProfileDto
        {
            Id = contractor.Id,
            Name = contractor.Name,
            AverageRating = contractor.AverageRating,
            ReviewCount = contractor.ReviewCount,
            TotalJobsAssigned = totalAssigned,
            TotalJobsAccepted = totalAccepted,
            TotalJobsCompleted = totalCompleted,
            AcceptanceRate = Math.Round(acceptanceRate, 2),
            TotalEarnings = null, // MVP: not available
            CreatedAt = contractor.CreatedAt,
            RecentReviews = recentReviewDtos
        };
    }
}

