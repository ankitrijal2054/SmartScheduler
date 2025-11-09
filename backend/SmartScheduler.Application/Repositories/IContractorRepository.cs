using SmartScheduler.Domain.Entities;

namespace SmartScheduler.Application.Repositories;

/// <summary>
/// Repository interface for Contractor entity data access.
/// Provides methods for CRUD operations and querying contractors.
/// </summary>
public interface IContractorRepository
{
    /// <summary>
    /// Creates a new contractor and returns it.
    /// </summary>
    /// <param name="contractor">The contractor entity to create.</param>
    /// <returns>The created contractor with ID populated.</returns>
    Task<Contractor> CreateAsync(Contractor contractor);

    /// <summary>
    /// Retrieves a contractor by ID (includes deleted contractors).
    /// </summary>
    /// <param name="id">The contractor ID.</param>
    /// <returns>The contractor if found, null otherwise.</returns>
    Task<Contractor?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves all active contractors with pagination.
    /// Ordered by name ascending.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Tuple of contractors list and total count.</returns>
    Task<(List<Contractor> contractors, int totalCount)> GetAllActiveAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Updates an existing contractor.
    /// </summary>
    /// <param name="contractor">The contractor entity with updated values.</param>
    /// <returns>The updated contractor.</returns>
    Task<Contractor> UpdateAsync(Contractor contractor);

    /// <summary>
    /// Soft deletes a contractor by ID (sets IsActive = false).
    /// </summary>
    /// <param name="id">The contractor ID.</param>
    /// <returns>A completed task.</returns>
    Task DeactivateAsync(int id);

    /// <summary>
    /// Checks if a phone number is already registered by another contractor.
    /// </summary>
    /// <param name="phone">The phone number to check.</param>
    /// <param name="excludeContractorId">Optional: exclude this contractor ID from the check (for updates).</param>
    /// <returns>True if phone exists, false otherwise.</returns>
    Task<bool> ExistsByPhoneAsync(string phone, int? excludeContractorId = null);

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    /// <returns>A completed task.</returns>
    Task SaveChangesAsync();

    /// <summary>
    /// Gets a contractor by ID (Story 2.4 - Recommendation Scoring).
    /// </summary>
    /// <param name="id">The contractor ID.</param>
    /// <returns>The contractor if found, null otherwise.</returns>
    Task<Contractor?> GetContractorByIdAsync(int id);

    /// <summary>
    /// Gets all active contractor IDs.
    /// </summary>
    /// <returns>List of active contractor IDs.</returns>
    Task<List<int>> GetActiveContractorIdsAsync();

    /// <summary>
    /// Gets all contractor IDs in a dispatcher's personal list.
    /// </summary>
    /// <param name="dispatcherId">The dispatcher ID.</param>
    /// <returns>List of contractor IDs in the dispatcher's list.</returns>
    Task<List<int>> GetDispatcherContractorListAsync(int dispatcherId);

    /// <summary>
    /// Gets a job by ID (Story 2.4 - Recommendation Scoring).
    /// </summary>
    /// <param name="jobId">The job ID.</param>
    /// <returns>The job if found, null otherwise.</returns>
    Task<Job?> GetJobByIdAsync(int jobId);

    /// <summary>
    /// Gets a contractor by UserId.
    /// Used to resolve contractor ID from authenticated user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The contractor if found, null otherwise.</returns>
    Task<Contractor?> GetByUserIdAsync(int userId);
}

