using SmartScheduler.Application.DTOs;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Service interface for contractor business logic.
/// Handles creation, retrieval, update, and deactivation of contractors.
/// </summary>
public interface IContractorService
{
    /// <summary>
    /// Creates a new contractor with validation and geocoding.
    /// </summary>
    /// <param name="request">The create contractor request.</param>
    /// <param name="dispatcherId">The ID of the dispatcher creating the contractor.</param>
    /// <returns>The created contractor response.</returns>
    /// <exception cref="ValidationException">If validation fails.</exception>
    Task<ContractorResponse> CreateContractorAsync(CreateContractorRequest request, int dispatcherId);

    /// <summary>
    /// Retrieves a single contractor by ID.
    /// </summary>
    /// <param name="id">The contractor ID.</param>
    /// <returns>The contractor response.</returns>
    /// <exception cref="NotFoundException">If contractor not found.</exception>
    Task<ContractorResponse> GetContractorAsync(int id);

    /// <summary>
    /// Retrieves all active contractors with pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based, default 1).</param>
    /// <param name="pageSize">Number of items per page (default 50, max 100).</param>
    /// <returns>Paginated contractor response.</returns>
    Task<PaginatedResponse<ContractorResponse>> GetAllContractorsAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Updates an existing contractor (partial update).
    /// Only provided fields are updated.
    /// </summary>
    /// <param name="id">The contractor ID to update.</param>
    /// <param name="request">The update request with fields to change.</param>
    /// <param name="dispatcherId">The ID of the dispatcher performing the update.</param>
    /// <returns>The updated contractor response.</returns>
    /// <exception cref="NotFoundException">If contractor not found.</exception>
    /// <exception cref="ValidationException">If validation fails.</exception>
    Task<ContractorResponse> UpdateContractorAsync(int id, UpdateContractorRequest request, int dispatcherId);

    /// <summary>
    /// Deactivates a contractor (soft delete).
    /// </summary>
    /// <param name="id">The contractor ID to deactivate.</param>
    /// <param name="dispatcherId">The ID of the dispatcher deactivating the contractor.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="NotFoundException">If contractor not found.</exception>
    Task DeactivateContractorAsync(int id, int dispatcherId);
}

