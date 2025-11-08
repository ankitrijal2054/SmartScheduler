namespace SmartScheduler.Domain.Enums;

/// <summary>
/// Enum representing the status of a job in the SmartScheduler system.
/// </summary>
public enum JobStatus
{
    Pending = 0,
    Assigned = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

