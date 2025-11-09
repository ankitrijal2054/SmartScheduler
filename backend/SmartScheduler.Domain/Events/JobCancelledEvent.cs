namespace SmartScheduler.Domain.Events;

/// <summary>
/// Domain event raised when a job is cancelled.
/// Used to notify the assigned contractor that the job has been cancelled.
/// </summary>
public class JobCancelledEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the job ID that was cancelled.
    /// </summary>
    public int JobId { get; }

    /// <summary>
    /// Gets the contractor ID assigned to the cancelled job.
    /// </summary>
    public int ContractorId { get; }

    /// <summary>
    /// Gets the reason for cancellation (optional).
    /// </summary>
    public string? Reason { get; }

    public JobCancelledEvent(int jobId, int contractorId, string? reason = null)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        JobId = jobId;
        ContractorId = contractorId;
        Reason = reason;
    }
}

