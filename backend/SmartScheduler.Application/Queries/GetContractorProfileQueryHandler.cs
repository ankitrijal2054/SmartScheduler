using MediatR;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;

namespace SmartScheduler.Application.Queries;

/// <summary>
/// Handler for GetContractorProfileQuery.
/// Aggregates contractor statistics and recent reviews.
/// </summary>
public class GetContractorProfileQueryHandler : IRequestHandler<GetContractorProfileQuery, ContractorProfileDto>
{
    private readonly IContractorRepository _contractorRepository;
    private readonly IAssignmentRepository _assignmentRepository;

    public GetContractorProfileQueryHandler(
        IContractorRepository contractorRepository,
        IAssignmentRepository assignmentRepository)
    {
        _contractorRepository = contractorRepository ?? throw new ArgumentNullException(nameof(contractorRepository));
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
    }

    /// <summary>
    /// Handles the query by aggregating contractor profile data and recent reviews.
    /// </summary>
    public async Task<ContractorProfileDto> Handle(GetContractorProfileQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get contractor data
        var contractor = await _contractorRepository.GetByIdAsync(request.ContractorId);

        if (contractor == null)
        {
            throw new InvalidOperationException($"Contractor with ID {request.ContractorId} not found.");
        }

        // Get assignment statistics (placeholder data for MVP)
        var totalAssigned = 0;
        var totalAccepted = 0;
        var totalCompleted = 0;

        // Calculate acceptance rate
        var acceptanceRate = 0m;

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
            RecentReviews = new List<CustomerReviewDto>() // Placeholder for MVP
        };
    }
}

