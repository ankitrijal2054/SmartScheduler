using SmartScheduler.Domain.Enums;

namespace SmartScheduler.Domain.Entities;

/// <summary>
/// Represents a user in the SmartScheduler system.
/// Users can have roles: Dispatcher, Customer, or Contractor.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Gets or sets the user's email address (unique).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password hash.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's role.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Gets or sets whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the last login timestamp.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public Contractor? Contractor { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<DispatcherContractorList> DispatcherContractorLists { get; set; } = new List<DispatcherContractorList>();
}

