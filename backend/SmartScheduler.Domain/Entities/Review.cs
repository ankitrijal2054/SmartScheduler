namespace SmartScheduler.Domain.Entities;

/// <summary>
/// Represents a review of a completed job.
/// One review per job (unique constraint on JobId).
/// </summary>
public class Review : BaseEntity
{
    /// <summary>
    /// Gets or sets the foreign key to the Job entity (unique).
    /// Only one review per job is allowed.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the Contractor entity.
    /// </summary>
    public int ContractorId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the Customer entity.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the rating (1-5 scale).
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Gets or sets the optional review comment.
    /// </summary>
    public string? Comment { get; set; }

    // Navigation properties
    public Job? Job { get; set; }
    public Contractor? Contractor { get; set; }
    public Customer? Customer { get; set; }
}

