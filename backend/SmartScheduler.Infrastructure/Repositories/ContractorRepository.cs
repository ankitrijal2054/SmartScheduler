using Microsoft.EntityFrameworkCore;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Contractor entity data access.
/// Provides CRUD operations and querying capabilities.
/// </summary>
public class ContractorRepository : IContractorRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ContractorRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Creates a new contractor and returns it.
    /// </summary>
    public async Task<Contractor> CreateAsync(Contractor contractor)
    {
        ArgumentNullException.ThrowIfNull(contractor);
        
        _dbContext.Contractors.Add(contractor);
        await SaveChangesAsync();
        
        return contractor;
    }

    /// <summary>
    /// Retrieves a contractor by ID (includes inactive contractors).
    /// Note: Returns tracked entity for update scenarios.
    /// </summary>
    public async Task<Contractor?> GetByIdAsync(int id)
    {
        return await _dbContext.Contractors
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Retrieves all active contractors with pagination.
    /// </summary>
    public async Task<(List<Contractor> contractors, int totalCount)> GetAllActiveAsync(int pageNumber, int pageSize)
    {
        // Validate pagination parameters
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        // Get total count of active contractors
        var totalCount = await _dbContext.Contractors
            .Where(c => c.IsActive)
            .CountAsync();

        // Get paginated active contractors ordered by name
        var contractors = await _dbContext.Contractors
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (contractors, totalCount);
    }

    /// <summary>
    /// Updates an existing contractor.
    /// </summary>
    public async Task<Contractor> UpdateAsync(Contractor contractor)
    {
        ArgumentNullException.ThrowIfNull(contractor);

        _dbContext.Contractors.Update(contractor);
        await SaveChangesAsync();

        return contractor;
    }

    /// <summary>
    /// Soft deletes a contractor by ID (sets IsActive = false).
    /// </summary>
    public async Task DeactivateAsync(int id)
    {
        var contractor = await _dbContext.Contractors.FirstOrDefaultAsync(c => c.Id == id);
        
        if (contractor != null)
        {
            contractor.IsActive = false;
            _dbContext.Contractors.Update(contractor);
            await SaveChangesAsync();
        }
    }

    /// <summary>
    /// Checks if a phone number is already registered by another contractor.
    /// </summary>
    public async Task<bool> ExistsByPhoneAsync(string phone, int? excludeContractorId = null)
    {
        ArgumentNullException.ThrowIfNull(phone);

        var query = _dbContext.Contractors.Where(c => c.PhoneNumber == phone);
        
        // If we're checking for update, exclude the current contractor
        if (excludeContractorId.HasValue)
        {
            query = query.Where(c => c.Id != excludeContractorId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets a contractor by ID with all necessary details for scoring.
    /// </summary>
    public async Task<Contractor?> GetContractorByIdAsync(int id)
    {
        return await _dbContext.Contractors
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Gets all active contractor IDs for efficient filtering.
    /// </summary>
    public async Task<List<int>> GetActiveContractorIdsAsync()
    {
        return await _dbContext.Contractors
            .Where(c => c.IsActive)
            .Select(c => c.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all contractor IDs in a dispatcher's personal list.
    /// </summary>
    public async Task<List<int>> GetDispatcherContractorListAsync(int dispatcherId)
    {
        return await _dbContext.DispatcherContractorLists
            .Where(dcl => dcl.DispatcherId == dispatcherId)
            .Select(dcl => dcl.ContractorId)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a job by ID with all necessary details for scoring.
    /// </summary>
    public async Task<Job?> GetJobByIdAsync(int jobId)
    {
        return await _dbContext.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == jobId);
    }
}

