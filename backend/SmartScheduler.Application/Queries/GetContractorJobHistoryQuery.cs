using MediatR;
using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Queries;

/// <summary>
/// Query for retrieving a contractor's job history with optional filtering and pagination.
/// Implements CQRS pattern with MediatR.
/// </summary>
public class GetContractorJobHistoryQuery : IRequest<JobHistoryResponseDto>
{
    /// <summary>
    /// The ID of the contractor whose job history to retrieve.
    /// </summary>
    public int ContractorId { get; set; }

    /// <summary>
    /// Start date for filtering (optional). If provided, only jobs scheduled on or after this date.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for filtering (optional). If provided, only jobs scheduled on or before this date.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Number of records to skip for pagination (default 0).
    /// </summary>
    public int Skip { get; set; } = 0;

    /// <summary>
    /// Number of records to take for pagination (default 20, max 100).
    /// </summary>
    public int Take { get; set; } = 20;

    public GetContractorJobHistoryQuery(
        int contractorId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int skip = 0,
        int take = 20)
    {
        ContractorId = contractorId;
        StartDate = startDate;
        EndDate = endDate;
        Skip = skip;
        Take = Math.Min(take, 100); // Cap at 100 for performance
    }
}

