namespace SmartScheduler.Domain.Entities;

/// <summary>
/// Represents a dispatcher's curated list of favorite contractors.
/// A dispatcher can maintain a list of preferred contractors for quick access.
/// </summary>
public class DispatcherContractorList : BaseEntity
{
    /// <summary>
    /// Gets or sets the foreign key to the Dispatcher User entity.
    /// </summary>
    public int DispatcherId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the Contractor entity.
    /// </summary>
    public int ContractorId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the contractor was added to the list.
    /// </summary>
    public DateTime AddedAt { get; set; }

    // Navigation properties
    public User? Dispatcher { get; set; }
    public Contractor? Contractor { get; set; }
}

