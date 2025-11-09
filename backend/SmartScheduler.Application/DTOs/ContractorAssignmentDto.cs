namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for contractor assignment with job and customer details.
/// Used for displaying assignments in contractor dashboard.
/// </summary>
public class ContractorAssignmentDto
{
    /// <summary>
    /// Assignment ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Job ID.
    /// </summary>
    public int JobId { get; set; }

    /// <summary>
    /// Contractor ID.
    /// </summary>
    public int ContractorId { get; set; }

    /// <summary>
    /// Assignment status (Pending, Accepted, InProgress, Completed).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// ISO 8601 formatted creation timestamp.
    /// </summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Job type (Plumbing, HVAC, Electrical, etc.).
    /// </summary>
    public string JobType { get; set; } = string.Empty;

    /// <summary>
    /// Job location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// ISO 8601 formatted scheduled time.
    /// </summary>
    public string? ScheduledTime { get; set; }

    /// <summary>
    /// Customer name.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Job description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

