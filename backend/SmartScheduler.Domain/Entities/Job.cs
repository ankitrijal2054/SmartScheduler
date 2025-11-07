using SmartScheduler.Domain.Enums;

namespace SmartScheduler.Domain.Entities;

/// <summary>
/// Represents a job (work request) in the SmartScheduler system.
/// A job is created by a customer and assigned to a contractor.
/// </summary>
public class Job : BaseEntity
{
    /// <summary>
    /// Gets or sets the foreign key to the Customer entity.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the type of job (trade type).
    /// </summary>
    public TradeType JobType { get; set; }

    /// <summary>
    /// Gets or sets the job location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latitude of the job location.
    /// </summary>
    public decimal Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude of the job location.
    /// </summary>
    public decimal Longitude { get; set; }

    /// <summary>
    /// Gets or sets the desired date and time for the job.
    /// </summary>
    public DateTime DesiredDateTime { get; set; }

    /// <summary>
    /// Gets or sets the estimated duration of the job in hours.
    /// </summary>
    public decimal EstimatedDurationHours { get; set; }

    /// <summary>
    /// Gets or sets the job description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the job.
    /// </summary>
    public JobStatus Status { get; set; } = JobStatus.Pending;

    /// <summary>
    /// Gets or sets the foreign key to the assigned Contractor (nullable).
    /// </summary>
    public int? AssignedContractorId { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public Assignment? Assignment { get; set; }
    public Review? Review { get; set; }
}

