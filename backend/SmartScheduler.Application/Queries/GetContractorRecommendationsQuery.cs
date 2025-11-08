using MediatR;
using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Queries;

/// <summary>
/// Query for retrieving the top 5 recommended contractors for a job.
/// Implements CQRS pattern with MediatR.
/// </summary>
public class GetContractorRecommendationsQuery : IRequest<RecommendationResponseDto>
{
    /// <summary>
    /// The ID of the job to get recommendations for.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// The ID of the dispatcher requesting recommendations.
    /// </summary>
    public int DispatcherId { get; set; }

    /// <summary>
    /// If true, only recommend contractors from the dispatcher's personal list.
    /// If false (default), all active contractors are considered.
    /// </summary>
    public bool ContractorListOnly { get; set; } = false;

    public GetContractorRecommendationsQuery(int jobId, int dispatcherId, bool contractorListOnly = false)
    {
        JobId = jobId;
        DispatcherId = dispatcherId;
        ContractorListOnly = contractorListOnly;
    }
}

