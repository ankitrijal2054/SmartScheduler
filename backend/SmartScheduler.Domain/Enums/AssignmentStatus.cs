namespace SmartScheduler.Domain.Enums;

/// <summary>
/// Enum representing the status of a job assignment.
/// </summary>
public enum AssignmentStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    InProgress = 3,
    Completed = 4
}

