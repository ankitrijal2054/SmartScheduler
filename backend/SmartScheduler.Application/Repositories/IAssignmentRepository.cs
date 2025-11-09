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

    /// <summary>
    /// Gets all assignments for a contractor with job and customer details.
    /// Optionally filters by status. Results ordered by assigned date descending.
    /// </summary>
    /// <param name="contractorId">The contractor ID.</param>
    /// <param name="status">Optional status filter.</param>
    /// <returns>List of assignments with job and customer details.</returns>
    Task<List<Assignment>> GetContractorAssignmentsWithDetailsAsync(int contractorId, AssignmentStatus? status = null);

    /// <summary>
    /// Gets contractor's job history with optional date filtering and pagination.
    /// Includes customer review data (rating and comment) if available.
    /// Results sorted by job scheduled date in descending order.
    /// </summary>
    /// <param name="contractorId">The contractor ID.</param>
    /// <param name="startDate">Optional: Filter jobs scheduled on or after this date.</param>
    /// <param name="endDate">Optional: Filter jobs scheduled on or before this date.</param>
    /// <param name="skip">Number of results to skip for pagination (default 0).</param>
    /// <param name="take">Number of results to take for pagination (max 100).</param>
    /// <returns>Tuple of (assignments list, total count) for the contractor.</returns>
    Task<(List<Assignment> Assignments, int TotalCount)> GetContractorJobsWithReviewsAsync(
        int contractorId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int skip = 0,
        int take = 20);

    /// <summary>
    /// Gets an assignment by ID with full details including job, customer, and reviews.
    /// Used for displaying complete job details in contractor dashboard.
    /// </summary>
    /// <param name="assignmentId">The assignment ID.</param>
    /// <param name="contractorId">The contractor ID for authorization (must match assignment's contractor).</param>
    /// <returns>The assignment with full details, or null if not found or not authorized.</returns>
    Task<Assignment?> GetAssignmentWithDetailsAsync(int assignmentId, int contractorId);
}

