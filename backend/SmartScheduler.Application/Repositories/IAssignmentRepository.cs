using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;

namespace SmartScheduler.Application.Repositories;

/// <summary>
/// Repository interface for Assignment entity operations.
/// Handles querying and persisting assignment data.
/// </summary>
public interface IAssignmentRepository
{
    /// <summary>
    /// Gets all active (non-completed, non-declined) assignments for a contractor on a specific date.
    /// Active statuses: Pending, Accepted, InProgress.
    /// </summary>
    /// <param name="contractorId">The contractor ID to query for.</param>
    /// <param name="targetDate">The date to filter assignments by (uses job's DesiredDateTime).</param>
    /// <returns>List of active assignments for the contractor on the target date.</returns>
    Task<IEnumerable<Assignment>> GetActiveAssignmentsByContractorAndDateAsync(int contractorId, DateTime targetDate);

    /// <summary>
    /// Gets all active assignments for a contractor.
    /// Active statuses: Pending, Accepted, InProgress.
    /// </summary>
    /// <param name="contractorId">The contractor ID to query for.</param>
    /// <returns>List of all active assignments for the contractor.</returns>
    Task<IEnumerable<Assignment>> GetActiveAssignmentsByContractorAsync(int contractorId);

    /// <summary>
    /// Creates a new assignment.
    /// </summary>
    /// <param name="assignment">The assignment entity to create.</param>
    /// <returns>The created assignment entity.</returns>
    Task<Assignment> CreateAsync(Assignment assignment);

    /// <summary>
    /// Updates an existing assignment.
    /// </summary>
    /// <param name="assignment">The assignment entity to update.</param>
    /// <returns>The updated assignment entity.</returns>
    Task<Assignment> UpdateAsync(Assignment assignment);

    /// <summary>
    /// Gets an assignment by ID.
    /// </summary>
    /// <param name="id">The assignment ID.</param>
    /// <returns>The assignment entity, or null if not found.</returns>
    Task<Assignment?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all assignments for a contractor on a specific date (Story 2.4 - Available Time Slots).
    /// Includes all statuses to calculate busy hours.
    /// </summary>
    /// <param name="contractorId">The contractor ID.</param>
    /// <param name="date">The date to filter assignments by.</param>
    /// <returns>List of assignments for the contractor on the target date.</returns>
    Task<List<Assignment>> GetContractorAssignmentsByDateAsync(int contractorId, DateTime date);

    /// <summary>
    /// Gets assignments for a contractor filtered by status with pagination.
    /// </summary>
    /// <param name="contractorId">The contractor ID to query for.</param>
    /// <param name="status">The assignment status to filter by.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <returns>List of assignments matching the filters in reverse chronological order.</returns>
    Task<List<Assignment>> GetAssignmentsByContractorAndStatusAsync(int contractorId, AssignmentStatus status, int limit, int offset);

    /// <summary>
    /// Gets the total count of assignments for a contractor with a specific status.
    /// </summary>
    /// <param name="contractorId">The contractor ID to query for.</param>
    /// <param name="status">The assignment status to filter by.</param>
    /// <returns>Total count of matching assignments.</returns>
    Task<int> GetAssignmentCountByContractorAndStatusAsync(int contractorId, AssignmentStatus status);
}

