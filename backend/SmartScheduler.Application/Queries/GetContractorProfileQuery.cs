using MediatR;
using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Queries;

/// <summary>
/// Query for retrieving a contractor's profile with aggregated statistics and recent reviews.
/// Implements CQRS pattern with MediatR.
/// </summary>
public class GetContractorProfileQuery : IRequest<ContractorProfileDto>
{
    /// <summary>
    /// The ID of the contractor whose profile to retrieve.
    /// </summary>
    public int ContractorId { get; set; }

    public GetContractorProfileQuery(int contractorId)
    {
        ContractorId = contractorId;
    }
}

