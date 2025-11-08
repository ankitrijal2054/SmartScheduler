namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for Contractor read model response.
/// Includes all public contractor information with phone masked for privacy.
/// </summary>
public class ContractorResponse
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
    /// Contractor's phone number (masked: shows last 4 digits only, e.g., "****4567").
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Trade type the contractor specializes in.
    /// </summary>
    public string TradeType { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's location address.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's average rating (null if no reviews).
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Number of reviews received.
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Whether the contractor is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Contractor's working hours.
    /// </summary>
    public WorkingHoursDto WorkingHours { get; set; } = new();
}

/// <summary>
/// DTO for contractor working hours.
/// </summary>
public class WorkingHoursDto
{
    /// <summary>
    /// Start time of work day (HH:mm format, e.g., "09:00").
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// End time of work day (HH:mm format, e.g., "17:00").
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// Array of working days (Mon, Tue, Wed, Thu, Fri, Sat, Sun).
    /// </summary>
    public string[] WorkDays { get; set; } = Array.Empty<string>();
}

/// <summary>
/// DTO for creating a new contractor - dispatcher-only operation.
/// </summary>
public class CreateContractorRequest
{
    /// <summary>
    /// Contractor's name (required, max 100).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's location address for geocoding (required, 5-200 chars).
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's phone number in E.164 format (required, e.g., "+1234567890" or "123-456-7890").
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Trade type the contractor specializes in (required).
    /// Valid values: Flooring, HVAC, Plumbing, Electrical, Other
    /// </summary>
    public string TradeType { get; set; } = string.Empty;

    /// <summary>
    /// Contractor's working hours (required).
    /// </summary>
    public CreateWorkingHoursRequest WorkingHours { get; set; } = new();
}

/// <summary>
/// DTO for creating/updating contractor working hours.
/// </summary>
public class CreateWorkingHoursRequest
{
    /// <summary>
    /// Start time in HH:mm format (e.g., "09:00").
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// End time in HH:mm format (e.g., "17:00").
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// Array of working days (Mon, Tue, Wed, Thu, Fri, Sat, Sun).
    /// At least one day required.
    /// </summary>
    public string[] WorkDays { get; set; } = Array.Empty<string>();
}

/// <summary>
/// DTO for updating an existing contractor - all fields optional for PATCH.
/// </summary>
public class UpdateContractorRequest
{
    /// <summary>
    /// Contractor's name (optional, max 100 if provided).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Contractor's location address (optional, will be re-geocoded if provided).
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Contractor's phone number (optional, must be unique if provided).
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Trade type (optional, must be valid if provided).
    /// </summary>
    public string? TradeType { get; set; }

    /// <summary>
    /// Working hours (optional).
    /// </summary>
    public CreateWorkingHoursRequest? WorkingHours { get; set; }
}

/// <summary>
/// DTO for paginated contractor list response.
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// List of contractors.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total count of all contractors (not just this page).
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; }
}

/// <summary>
/// DTO for pagination request parameters.
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Page number (default 1, must be >= 1).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (default 50, must be between 1 and 100).
    /// </summary>
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Backward compatibility alias for ContractorResponse.
/// </summary>
public class ContractorDto : ContractorResponse
{
}

