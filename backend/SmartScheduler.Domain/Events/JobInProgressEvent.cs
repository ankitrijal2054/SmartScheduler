namespace SmartScheduler.Domain.Events;

/// <summary>
/// Domain event raised when a job is marked as in-progress by a contractor.
/// Used to trigger email notification to customer that contractor is on the way.
/// </summary>
public class JobInProgressEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the job ID that started.
    /// </summary>
    public int JobId { get; }

    /// <summary>
    /// Gets the assignment ID for the job.
    /// </summary>
    public int AssignmentId { get; }

    /// <summary>
    /// Gets the contractor ID working on the job.
    /// </summary>
    public int ContractorId { get; }

    /// <summary>
    /// Gets the customer ID who will receive the notification.
    /// </summary>
    public int CustomerId { get; }

    public JobInProgressEvent(int jobId, int assignmentId, int contractorId, int customerId)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        JobId = jobId;
        AssignmentId = assignmentId;
        ContractorId = contractorId;
        CustomerId = customerId;
    }
}

