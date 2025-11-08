using System.Security.Claims;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Implementation of IAuthorizationService for role-based authorization and data filtering.
/// Handles role-specific filtering logic and user claim extraction.
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    /// <summary>
    /// Filters data collection based on user role.
    /// - Dispatcher: Can see all data (orchestrator view)
    /// - Customer: Can only see resources they own (CustomerId match)
    /// - Contractor: Can only see their assigned jobs (via Assignment relationship)
    /// </summary>
    public IQueryable<T> FilterDataByRole<T>(int userId, string? role, IQueryable<T> data) where T : class
    {
        // Handle null or invalid role
        if (string.IsNullOrWhiteSpace(role))
        {
            return data.Where(_ => false); // Return empty for invalid role
        }

        // Dispatcher sees all data
        if (role.Equals("Dispatcher", StringComparison.OrdinalIgnoreCase))
        {
            return data;
        }

        // Customer filtering: Only their own jobs
        if (role.Equals("Customer", StringComparison.OrdinalIgnoreCase))
        {
            if (typeof(T) == typeof(Job))
            {
                return data.Where(j => ((Job)(object)j).CustomerId == userId);
            }
        }

        // Contractor filtering: Only their assigned jobs
        if (role.Equals("Contractor", StringComparison.OrdinalIgnoreCase))
        {
            if (typeof(T) == typeof(Job))
            {
                // Filter jobs where this contractor has an assignment
                return data.Where(j => ((Job)(object)j).Assignment != null && 
                                      ((Job)(object)j).Assignment!.ContractorId == userId);
            }
            
            if (typeof(T) == typeof(Assignment))
            {
                return data.Where(a => ((Assignment)(object)a).ContractorId == userId);
            }
        }

        // Default: Return empty collection for unknown roles (safety)
        return data.Where(_ => false);
    }

    /// <summary>
    /// Extracts the current user's ID from JWT claims.
    /// Looks for the standard NameIdentifier claim which contains the user ID.
    /// </summary>
    public int GetCurrentUserIdFromContext(ClaimsPrincipal claims)
    {
        if (claims == null)
        {
            throw new ArgumentNullException(nameof(claims), "Claims principal cannot be null");
        }

        var userIdClaim = claims.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
        {
            throw new ArgumentException("NameIdentifier claim is missing from user context", nameof(claims));
        }

        if (int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        throw new ArgumentException($"NameIdentifier claim value '{userIdClaim.Value}' is not a valid integer", nameof(claims));
    }

    /// <summary>
    /// Validates that a user owns a specific resource.
    /// </summary>
    public bool ValidateUserOwnsResource(int userId, int resourceOwnerId)
    {
        return userId == resourceOwnerId;
    }

    /// <summary>
    /// Validates that a user has a specific role.
    /// </summary>
    public bool ValidateUserRole(string requiredRole, string? userRole)
    {
        if (string.IsNullOrWhiteSpace(requiredRole) || string.IsNullOrWhiteSpace(userRole))
        {
            return false;
        }

        return requiredRole.Equals(userRole, StringComparison.OrdinalIgnoreCase);
    }
}

