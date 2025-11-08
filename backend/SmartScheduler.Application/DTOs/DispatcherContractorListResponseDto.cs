namespace SmartScheduler.Application.DTOs;

/// <summary>
/// DTO for dispatcher's contractor list response with pagination.
/// </summary>
public class DispatcherContractorListResponseDto
{
    /// <summary>
    /// List of contractors in dispatcher's curated list.
    /// </summary>
    public List<ContractorListItemDto> Contractors { get; set; } = new();

    /// <summary>
    /// Pagination metadata.
    /// </summary>
    public PaginationMetadataDto Pagination { get; set; } = new();
}

/// <summary>
/// DTO for pagination metadata in responses.
/// </summary>
public class PaginationMetadataDto
{
    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; set; }
}

