using SmartScheduler.Domain.Enums;

namespace SmartScheduler.Domain.Entities;

/// <summary>
/// Represents a contractor (service professional) in the SmartScheduler system.
/// A contractor is linked to a User with Role = Contractor.
/// </summary>
public class Contractor : BaseEntity
{
    /// <summary>
    /// Gets or sets the foreign key to the User entity.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the contractor's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contractor's phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contractor's location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latitude of the contractor's location.
    /// </summary>
    public decimal Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude of the contractor's location.
    /// </summary>
    public decimal Longitude { get; set; }

    /// <summary>
    /// Gets or sets the trade type the contractor specializes in.
    /// </summary>
    public TradeType TradeType { get; set; }

    /// <summary>
    /// Gets or sets the start time of working hours (TimeSpan).
    /// </summary>
    public TimeSpan WorkingHoursStart { get; set; }

    /// <summary>
    /// Gets or sets the end time of working hours (TimeSpan).
    /// </summary>
    public TimeSpan WorkingHoursEnd { get; set; }

    /// <summary>
    /// Gets or sets the contractor's average rating (nullable).
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Gets or sets the number of reviews received.
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of completed jobs.
    /// </summary>
    public int TotalJobsCompleted { get; set; }

    /// <summary>
    /// Gets or sets whether the contractor is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public User? User { get; set; }
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<DispatcherContractorList> DispatcherContractorLists { get; set; } = new List<DispatcherContractorList>();
}

