using SmartScheduler.Domain.Entities;

namespace SmartScheduler.Application.Repositories;

/// <summary>
/// Repository interface for managing dispatcher's contractor lists.
/// </summary>
public interface IDispatcherContractorListRepository
{
    /// <summary>
    /// Adds a contractor to dispatcher's list (idempotent).
    /// </summary>
    Task<DispatcherContractorList> AddAsync(int dispatcherId, int contractorId);

    /// <summary>
    /// Removes a contractor from dispatcher's list (idempotent).
    /// </summary>
    Task RemoveAsync(int dispatcherId, int contractorId);

    /// <summary>
    /// Gets dispatcher's contractor list with pagination.
    /// </summary>
    Task<IEnumerable<DispatcherContractorList>> GetByDispatcherIdAsync(int dispatcherId, int page, int limit);

    /// <summary>
    /// Checks if contractor exists in dispatcher's list.
    /// </summary>
    Task<bool> ExistsAsync(int dispatcherId, int contractorId);

    /// <summary>
    /// Gets total count of contractors in dispatcher's list.
    /// </summary>
    Task<int> CountByDispatcherIdAsync(int dispatcherId);
}

