namespace SmartScheduler.Domain.Events;

/// <summary>
/// Domain event raised when a job is assigned to a contractor.
/// Used to trigger email notification to customer with contractor details.
/// </summary>
public class JobAssignedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the job ID that was assigned.
    /// </summary>
    public int JobId { get; }

    /// <summary>
    /// Gets the assignment ID created for this job assignment.
    /// </summary>
    public int AssignmentId { get; }

    /// <summary>
    /// Gets the contractor ID assigned to the job.
    /// </summary>
    public int ContractorId { get; }

    /// <summary>
    /// Gets the customer ID who will receive the notification.
    /// </summary>
    public int CustomerId { get; }

    public JobAssignedEvent(int jobId, int assignmentId, int contractorId, int customerId)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        JobId = jobId;
        AssignmentId = assignmentId;
        ContractorId = contractorId;
        CustomerId = customerId;
    }
}

