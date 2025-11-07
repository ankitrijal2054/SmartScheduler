namespace SmartScheduler.Domain.Entities;

/// <summary>
/// Represents a customer (job requester) in the SmartScheduler system.
/// A customer is linked to a User with Role = Customer.
/// </summary>
public class Customer : BaseEntity
{
    /// <summary>
    /// Gets or sets the foreign key to the User entity.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the customer's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    // Navigation properties
    public User? User { get; set; }
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}

