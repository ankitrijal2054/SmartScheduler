namespace SmartScheduler.Domain.Events;

/// <summary>
/// Domain event raised when a job is completed by a contractor.
/// Used to trigger email notification to customer with completion confirmation and rating link.
/// </summary>
public class JobCompletedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the job ID that was completed.
    /// </summary>
    public int JobId { get; }

    /// <summary>
    /// Gets the assignment ID for the completed job.
    /// </summary>
    public int AssignmentId { get; }

    /// <summary>
    /// Gets the contractor ID who completed the job.
    /// </summary>
    public int ContractorId { get; }

    /// <summary>
    /// Gets the customer ID who will receive the notification.
    /// </summary>
    public int CustomerId { get; }

    public JobCompletedEvent(int jobId, int assignmentId, int contractorId, int customerId)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        JobId = jobId;
        AssignmentId = assignmentId;
        ContractorId = contractorId;
        CustomerId = customerId;
    }
}

