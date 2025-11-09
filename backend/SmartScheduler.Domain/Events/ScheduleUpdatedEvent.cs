namespace SmartScheduler.Domain.Events;

/// <summary>
/// Domain event raised when a job's scheduled time is updated.
/// Used to notify the assigned contractor that their schedule has changed.
/// </summary>
public class ScheduleUpdatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Gets the job ID whose schedule was updated.
    /// </summary>
    public int JobId { get; }

    /// <summary>
    /// Gets the contractor ID assigned to the job.
    /// </summary>
    public int ContractorId { get; }

    /// <summary>
    /// Gets the new scheduled date and time.
    /// </summary>
    public DateTime NewScheduledDateTime { get; }

    /// <summary>
    /// Gets the old scheduled date and time.
    /// </summary>
    public DateTime OldScheduledDateTime { get; }

    public ScheduleUpdatedEvent(int jobId, int contractorId, DateTime newScheduledDateTime, DateTime oldScheduledDateTime)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        JobId = jobId;
        ContractorId = contractorId;
        NewScheduledDateTime = newScheduledDateTime;
        OldScheduledDateTime = oldScheduledDateTime;
    }
}

