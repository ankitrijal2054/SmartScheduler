namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for Contractor data - excludes sensitive fields like phone and email.
/// </summary>
public class ContractorDto
{
    /// <summary>
    /// Contractor's unique ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Contractor's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Trade type the contractor specializes in.
    /// </summary>
    public string TradeType { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's average rating.
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Number of reviews received.
    /// </summary>
    public int ReviewCount { get; set; }
}

/// <summary>
/// DTO for creating a new contractor - dispatcher-only operation.
/// </summary>
public class CreateContractorDto
{
    /// <summary>
    /// Contractor's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Trade type the contractor specializes in.
    /// </summary>
    public string TradeType { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's working hours start time (e.g., "09:00:00").
    /// </summary>
    public string WorkingHoursStart { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's working hours end time (e.g., "17:00:00").
    /// </summary>
    public string WorkingHoursEnd { get; set; } = string.Empty;
}

