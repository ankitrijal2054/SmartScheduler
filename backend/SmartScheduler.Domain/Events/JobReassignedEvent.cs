namespace SmartScheduler.Domain.Events;

/// <summary>
/// Domain event raised when a job assignment is reassigned from one contractor to another.
/// Used to notify the original contractor that their assignment has been reassigned.
/// </summary>
public class JobReassignedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the job ID that was reassigned.
    /// </summary>
    public int JobId { get; }

    /// <summary>
    /// Gets the contractor ID who originally had the assignment.
    /// </summary>
    public int OldContractorId { get; }

    /// <summary>
    /// Gets the contractor ID who now has the assignment.
    /// </summary>
    public int NewContractorId { get; }

    /// <summary>
    /// Gets the reason for reassignment (optional).
    /// </summary>
    public string? Reason { get; }

    public JobReassignedEvent(int jobId, int oldContractorId, int newContractorId, string? reason = null)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        JobId = jobId;
        OldContractorId = oldContractorId;
        NewContractorId = newContractorId;
        Reason = reason;
    }
}

