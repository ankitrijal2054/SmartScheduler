using Microsoft.EntityFrameworkCore;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Infrastructure.Repositories;

/// <summary>
/// Repository for managing dispatcher's contractor lists.
/// Implements idempotent operations for add/remove.
/// </summary>
public class DispatcherContractorListRepository : IDispatcherContractorListRepository
{
    private readonly ApplicationDbContext _dbContext;

    public DispatcherContractorListRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds a contractor to dispatcher's list. Idempotent - returns existing if already present.
    /// </summary>
    public async Task<DispatcherContractorList> AddAsync(int dispatcherId, int contractorId)
    {
        var existing = await _dbContext.DispatcherContractorLists
            .FirstOrDefaultAsync(dcl => dcl.DispatcherId == dispatcherId && dcl.ContractorId == contractorId);

        if (existing != null)
        {
            return existing; // Idempotent: return existing record
        }

        var dispatcherContractorList = new DispatcherContractorList
        {
            DispatcherId = dispatcherId,
            ContractorId = contractorId,
            AddedAt = DateTime.UtcNow
        };

        _dbContext.DispatcherContractorLists.Add(dispatcherContractorList);
        await _dbContext.SaveChangesAsync();

        return dispatcherContractorList;
    }

    /// <summary>
    /// Removes a contractor from dispatcher's list. Idempotent - no error if not found.
    /// </summary>
    public async Task RemoveAsync(int dispatcherId, int contractorId)
    {
        var entry = await _dbContext.DispatcherContractorLists
            .FirstOrDefaultAsync(dcl => dcl.DispatcherId == dispatcherId && dcl.ContractorId == contractorId);

        if (entry != null)
        {
            _dbContext.DispatcherContractorLists.Remove(entry);
            await _dbContext.SaveChangesAsync();
        }
        // Idempotent: no error if not found
    }

    /// <summary>
    /// Gets dispatcher's contractor list with pagination. Ordered by AddedAt descending (most recent first).
    /// </summary>
    public async Task<IEnumerable<DispatcherContractorList>> GetByDispatcherIdAsync(int dispatcherId, int page, int limit)
    {
        return await _dbContext.DispatcherContractorLists
            .Where(dcl => dcl.DispatcherId == dispatcherId)
            .Include(dcl => dcl.Contractor)
            .OrderByDescending(dcl => dcl.AddedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if contractor exists in dispatcher's list.
    /// </summary>
    public async Task<bool> ExistsAsync(int dispatcherId, int contractorId)
    {
        return await _dbContext.DispatcherContractorLists
            .AnyAsync(dcl => dcl.DispatcherId == dispatcherId && dcl.ContractorId == contractorId);
    }

    /// <summary>
    /// Gets total count of contractors in dispatcher's list.
    /// </summary>
    public async Task<int> CountByDispatcherIdAsync(int dispatcherId)
    {
        return await _dbContext.DispatcherContractorLists
            .CountAsync(dcl => dcl.DispatcherId == dispatcherId);
    }
}

