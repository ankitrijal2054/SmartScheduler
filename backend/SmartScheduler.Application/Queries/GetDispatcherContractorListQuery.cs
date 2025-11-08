using MediatR;
using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Queries;

/// <summary>
/// Query for retrieving dispatcher's curated contractor list with pagination.
/// Implements CQRS pattern with MediatR.
/// </summary>
public class GetDispatcherContractorListQuery : IRequest<DispatcherContractorListResponseDto>
{
    /// <summary>
    /// The ID of the dispatcher requesting their list.
    /// Extracted from JWT token for security.
    /// </summary>
    public int DispatcherId { get; set; }

    /// <summary>
    /// Page number (1-based, default 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page (default 50, max 100).
    /// </summary>
    public int Limit { get; set; } = 50;

    /// <summary>
    /// Optional search filter to find contractors by name (case-insensitive).
    /// </summary>
    public string? Search { get; set; }

    public GetDispatcherContractorListQuery()
    {
    }

    public GetDispatcherContractorListQuery(int dispatcherId, int page = 1, int limit = 50, string? search = null)
    {
        DispatcherId = dispatcherId;
        Page = page;
        Limit = limit;
        Search = search;
    }
}

