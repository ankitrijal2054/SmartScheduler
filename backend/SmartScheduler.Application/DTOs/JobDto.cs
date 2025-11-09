namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for Job data.
/// </summary>
public class JobDto
{
    /// <summary>
    /// Job's unique ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Type of job (trade type).
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Job location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Desired date and time for the job.
    /// </summary>
    public DateTime DesiredDateTime { get; set; }

    /// <summary>
    /// Job description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the job.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// ID of the customer who created this job.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// ID of the contractor assigned to this job (nullable).
    /// </summary>
    public int? AssignedContractorId { get; set; }
}

/// <summary>
/// DTO for creating a new job - customer-only operation.
/// </summary>
public class CreateJobDto
{
    /// <summary>
    /// Type of job (trade type).
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Job location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Desired date and time for the job.
    /// </summary>
    public DateTime DesiredDateTime { get; set; }

    /// <summary>
    /// Job description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Estimated duration of the job in hours.
    /// </summary>
    public decimal? EstimatedDurationHours { get; set; }
}

