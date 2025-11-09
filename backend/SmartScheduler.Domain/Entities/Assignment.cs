using SmartScheduler.Domain.Enums;

namespace SmartScheduler.Domain.Entities;

/// <summary>
/// Represents an assignment of a job to a contractor.
/// Tracks the assignment lifecycle: pending, accepted, in-progress, completed, or declined.
/// </summary>
public class Assignment : BaseEntity
{
    /// <summary>
    /// Gets or sets the foreign key to the Job entity.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the Contractor entity.
    /// </summary>
    public int ContractorId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the assignment was created.
    /// </summary>
    public DateTime AssignedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the contractor accepted the assignment (nullable).
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the contractor declined the assignment (nullable).
    /// </summary>
    public DateTime? DeclinedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when work started (nullable).
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when work was completed (nullable).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the current status of the assignment.
    /// </summary>
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Pending;

    // Navigation properties
    public Job? Job { get; set; }
    public Contractor? Contractor { get; set; }

    /// <summary>
    /// Marks the assignment as in-progress. Validates that the assignment is in 'Accepted' state.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if assignment is not in 'Accepted' state.</exception>
    public void MarkInProgress()
    {
        if (Status != AssignmentStatus.Accepted)
        {
            throw new InvalidOperationException(
                $"Cannot mark assignment as in-progress. Current status is {Status}. " +
                "Only 'Accepted' assignments can transition to 'InProgress'."
            );
        }

        Status = AssignmentStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the assignment as completed. Validates that the assignment is in 'InProgress' state.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if assignment is not in 'InProgress' state.</exception>
    public void MarkComplete()
    {
        if (Status != AssignmentStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Cannot mark assignment as completed. Current status is {Status}. " +
                "Only 'InProgress' assignments can transition to 'Completed'."
            );
        }

        Status = AssignmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}

