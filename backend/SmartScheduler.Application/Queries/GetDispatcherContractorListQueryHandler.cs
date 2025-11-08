using MediatR;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;

namespace SmartScheduler.Application.Queries;

/// <summary>
/// Handler for GetDispatcherContractorListQuery.
/// Retrieves dispatcher's curated contractor list with filtering and pagination.
/// </summary>
public class GetDispatcherContractorListQueryHandler : IRequestHandler<GetDispatcherContractorListQuery, DispatcherContractorListResponseDto>
{
    private readonly IDispatcherContractorListRepository _repository;

    public GetDispatcherContractorListQueryHandler(IDispatcherContractorListRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Handles the query to retrieve dispatcher's contractor list.
    /// </summary>
    public async Task<DispatcherContractorListResponseDto> Handle(GetDispatcherContractorListQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Get contractors from repository
        var contractorListItems = await _repository.GetByDispatcherIdAsync(request.DispatcherId, request.Page, request.Limit);

        // Apply search filter if provided
        var filteredItems = contractorListItems.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            filteredItems = filteredItems
                .Where(dcl => dcl.Contractor?.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        // Map to DTOs
        var contractorDtos = filteredItems
            .Select(dcl => new ContractorListItemDto
            {
                Id = dcl.Contractor?.Id ?? 0,
                Name = dcl.Contractor?.Name ?? string.Empty,
                PhoneNumber = dcl.Contractor?.PhoneNumber ?? string.Empty,
                Location = dcl.Contractor?.Location ?? string.Empty,
                TradeType = dcl.Contractor?.TradeType.ToString() ?? string.Empty,
                AverageRating = dcl.Contractor?.AverageRating,
                ReviewCount = dcl.Contractor?.ReviewCount ?? 0,
                TotalJobsCompleted = dcl.Contractor?.TotalJobsCompleted ?? 0,
                IsActive = dcl.Contractor?.IsActive ?? false,
                AddedAt = dcl.AddedAt
            })
            .ToList();

        // Get total count for pagination metadata
        var totalCount = await _repository.CountByDispatcherIdAsync(request.DispatcherId);

        // Calculate total pages
        var totalPages = (int)Math.Ceiling((double)totalCount / request.Limit);

        return new DispatcherContractorListResponseDto
        {
            Contractors = contractorDtos,
            Pagination = new PaginationMetadataDto
            {
                Page = request.Page,
                Limit = request.Limit,
                Total = totalCount,
                TotalPages = totalPages
            }
        };
    }
}

