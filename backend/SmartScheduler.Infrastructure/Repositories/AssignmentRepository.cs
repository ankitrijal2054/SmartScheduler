using Microsoft.EntityFrameworkCore;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Assignment entity data access.
/// Provides CRUD operations and querying capabilities for assignments.
/// </summary>
public class AssignmentRepository : IAssignmentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AssignmentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Gets all active (non-completed, non-declined) assignments for a contractor on a specific date.
    /// Active statuses: Pending, Accepted, InProgress.
    /// Filters by job's DesiredDateTime to match the target date.
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetActiveAssignmentsByContractorAndDateAsync(int contractorId, DateTime targetDate)
    {
        var startOfDay = targetDate.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1); // 23:59:59.9999999

        var assignments = await _dbContext.Assignments
            .Include(a => a.Job)
            .Where(a => a.ContractorId == contractorId
                && (a.Status == AssignmentStatus.Pending
                    || a.Status == AssignmentStatus.Accepted
                    || a.Status == AssignmentStatus.InProgress)
                && a.Job != null
                && a.Job.DesiredDateTime >= startOfDay
                && a.Job.DesiredDateTime <= endOfDay)
            .AsNoTracking()
            .ToListAsync();

        return assignments;
    }

    /// <summary>
    /// Gets all active assignments for a contractor.
    /// Active statuses: Pending, Accepted, InProgress.
    /// </summary>
    public async Task<IEnumerable<Assignment>> GetActiveAssignmentsByContractorAsync(int contractorId)
    {
        var assignments = await _dbContext.Assignments
            .Include(a => a.Job)
            .Where(a => a.ContractorId == contractorId
                && (a.Status == AssignmentStatus.Pending
                    || a.Status == AssignmentStatus.Accepted
                    || a.Status == AssignmentStatus.InProgress))
            .AsNoTracking()
            .ToListAsync();

        return assignments;
    }

    /// <summary>
    /// Creates a new assignment.
    /// </summary>
    public async Task<Assignment> CreateAsync(Assignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        _dbContext.Assignments.Add(assignment);
        await SaveChangesAsync();

        return assignment;
    }

    /// <summary>
    /// Updates an existing assignment.
    /// </summary>
    public async Task<Assignment> UpdateAsync(Assignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);

        _dbContext.Assignments.Update(assignment);
        await SaveChangesAsync();

        return assignment;
    }

    /// <summary>
    /// Gets an assignment by ID.
    /// </summary>
    public async Task<Assignment?> GetByIdAsync(int id)
    {
        return await _dbContext.Assignments
            .Include(a => a.Job)
            .Include(a => a.Contractor)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    private async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Gets all assignments for a contractor on a specific date.
    /// Used to calculate available time slots.
    /// </summary>
    public async Task<List<Assignment>> GetContractorAssignmentsByDateAsync(int contractorId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1); // 23:59:59.9999999

        var assignments = await _dbContext.Assignments
            .Include(a => a.Job)
            .Where(a => a.ContractorId == contractorId
                && a.Job != null
                && a.Job.DesiredDateTime >= startOfDay
                && a.Job.DesiredDateTime <= endOfDay)
            .AsNoTracking()
            .ToListAsync();

        return assignments;
    }

    /// <summary>
    /// Gets assignments for a contractor filtered by status with pagination.
    /// Returns results in reverse chronological order (newest first).
    /// Used to display job history (e.g., completed jobs).
    /// </summary>
    public async Task<List<Assignment>> GetAssignmentsByContractorAndStatusAsync(int contractorId, AssignmentStatus status, int limit, int offset)
    {
        var assignments = await _dbContext.Assignments
            .Include(a => a.Job)
            .Where(a => a.ContractorId == contractorId && a.Status == status)
            .OrderByDescending(a => a.CompletedAt ?? a.StartedAt ?? a.AcceptedAt ?? a.AssignedAt)
            .Skip(offset)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return assignments;
    }

    /// <summary>
    /// Gets the total count of assignments for a contractor with a specific status.
    /// Used for pagination calculations.
    /// </summary>
    public async Task<int> GetAssignmentCountByContractorAndStatusAsync(int contractorId, AssignmentStatus status)
    {
        return await _dbContext.Assignments
            .Where(a => a.ContractorId == contractorId && a.Status == status)
            .CountAsync();
    }
}

