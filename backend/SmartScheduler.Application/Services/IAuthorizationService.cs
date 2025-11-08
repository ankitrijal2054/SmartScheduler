using System.Security.Claims;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Service for handling role-based authorization and data filtering.
/// Provides methods to filter data by user role, validate ownership, and extract user claims.
/// </summary>
public interface IAuthorizationService
{
/// <summary>
/// Filters data collection by user role and ID.
/// Different roles see different subsets of data:
/// - Dispatcher: Returns all data (admin/orchestrator view)
/// - Customer: Filters to only resources owned by this customer
/// - Contractor: Filters to only contractor's own assigned data
/// </summary>
/// <typeparam name="T">The entity type to filter</typeparam>
/// <param name="userId">The ID of the current user</param>
/// <param name="role">The role of the current user (Dispatcher, Customer, Contractor)</param>
/// <param name="data">The queryable data collection to filter</param>
/// <returns>Filtered IQueryable collection based on role and user ID</returns>
IQueryable<T> FilterDataByRole<T>(int userId, string? role, IQueryable<T> data) where T : class;

/// <summary>
/// Extracts the current user's ID from JWT claims.
/// </summary>
/// <param name="claims">The claims principal from the HTTP context</param>
/// <returns>The user's ID as an int</returns>
/// <exception cref="ArgumentException">Thrown if NameIdentifier claim is missing</exception>
int GetCurrentUserIdFromContext(ClaimsPrincipal claims);

/// <summary>
/// Validates that a user owns a specific resource.
/// Used for data access control (e.g., customer can only view their own jobs).
/// </summary>
/// <param name="userId">The ID of the current user</param>
/// <param name="resourceOwnerId">The ID of the user who owns the resource</param>
/// <returns>True if user owns the resource, false otherwise</returns>
bool ValidateUserOwnsResource(int userId, int resourceOwnerId);

    /// <summary>
    /// Validates that a user has a specific role.
    /// </summary>
    /// <param name="requiredRole">The required role</param>
    /// <param name="userRole">The user's current role</param>
    /// <returns>True if user has the required role, false otherwise</returns>
    bool ValidateUserRole(string requiredRole, string? userRole);
}

