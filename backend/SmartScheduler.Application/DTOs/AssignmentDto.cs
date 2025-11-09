namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for Assignment data.
/// Represents the assignment of a job to a contractor.
/// </summary>
public class AssignmentDto
{
    /// <summary>
    /// Assignment's unique ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the job assigned.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// ID of the contractor assigned.
    /// </summary>
    public int ContractorId { get; set; }

    /// <summary>
    /// Current status of the assignment.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the assignment was created.
    /// </summary>
    public DateTime AssignedAt { get; set; }

    /// <summary>
    /// Timestamp when the contractor accepted the assignment (nullable).
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// Timestamp when the contractor declined the assignment (nullable).
    /// </summary>
    public DateTime? DeclinedAt { get; set; }

    /// <summary>
    /// Timestamp when work started (nullable).
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Timestamp when work was completed (nullable).
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}

