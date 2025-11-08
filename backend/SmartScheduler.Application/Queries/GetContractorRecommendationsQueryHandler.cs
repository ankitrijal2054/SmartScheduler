using MediatR;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;

namespace SmartScheduler.Application.Queries;

/// <summary>
/// Handler for GetContractorRecommendationsQuery.
/// Orchestrates the scoring service to generate ranked contractor recommendations.
/// </summary>
public class GetContractorRecommendationsQueryHandler : IRequestHandler<GetContractorRecommendationsQuery, RecommendationResponseDto>
{
    private readonly IScoringService _scoringService;

    public GetContractorRecommendationsQueryHandler(IScoringService scoringService)
    {
        _scoringService = scoringService ?? throw new ArgumentNullException(nameof(scoringService));
    }

    /// <summary>
    /// Handles the query by delegating to the scoring service.
    /// </summary>
    public async Task<RecommendationResponseDto> Handle(GetContractorRecommendationsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await _scoringService.GetRecommendationsAsync(request.JobId, request.DispatcherId, request.ContractorListOnly);
    }
}

