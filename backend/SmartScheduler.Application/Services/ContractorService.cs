using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Service implementation for contractor business logic.
/// </summary>
public class ContractorService : IContractorService
{
    private readonly IContractorRepository _repository;
    private readonly IGeocodingService _geocodingService;
    private readonly ILogger<ContractorService> _logger;

    public ContractorService(
        IContractorRepository repository,
        IGeocodingService geocodingService,
        ILogger<ContractorService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _geocodingService = geocodingService ?? throw new ArgumentNullException(nameof(geocodingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new contractor with validation and geocoding.
    /// </summary>
    public async Task<ContractorResponse> CreateContractorAsync(CreateContractorRequest request, int dispatcherId)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate required fields
        ValidateCreateRequest(request);

        // Check phone uniqueness
        var phoneExists = await _repository.ExistsByPhoneAsync(request.Phone);
        if (phoneExists)
        {
            _logger.LogWarning("Attempt to create contractor with duplicate phone: {Phone}", MaskPhone(request.Phone));
            throw new ValidationException("Phone number already registered");
        }

        // Validate trade type
        if (!Enum.TryParse<TradeType>(request.TradeType, true, out var tradeType))
        {
            var validTypes = string.Join(", ", Enum.GetNames(typeof(TradeType)));
            throw new ValidationException($"Invalid TradeType. Supported types: {validTypes}");
        }

        // Parse and validate working hours
        if (!TimeSpan.TryParse(request.WorkingHours.StartTime, out var startTime))
        {
            throw new ValidationException("Invalid StartTime format. Use HH:mm or HH:mm:ss");
        }

        if (!TimeSpan.TryParse(request.WorkingHours.EndTime, out var endTime))
        {
            throw new ValidationException("Invalid EndTime format. Use HH:mm or HH:mm:ss");
        }

        if (endTime <= startTime)
        {
            throw new ValidationException("EndTime must be after StartTime");
        }

        if (request.WorkingHours.WorkDays == null || request.WorkingHours.WorkDays.Length == 0)
        {
            throw new ValidationException("At least one work day is required");
        }

        // Validate work days
        var validDays = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        foreach (var day in request.WorkingHours.WorkDays)
        {
            if (!validDays.Contains(day))
            {
                throw new ValidationException($"Invalid work day: {day}. Valid days: Mon, Tue, Wed, Thu, Fri, Sat, Sun");
            }
        }

        // Geocode the address to get latitude/longitude
        var (latitude, longitude) = await _geocodingService.GeocodeAddressAsync(request.Location);

        // Create contractor entity
        var contractor = new Contractor
        {
            Name = request.Name,
            Location = request.Location,
            PhoneNumber = request.Phone,
            TradeType = tradeType,
            WorkingHoursStart = startTime,
            WorkingHoursEnd = endTime,
            IsActive = true,
            ReviewCount = 0,
            AverageRating = null,
            TotalJobsCompleted = 0,
            Latitude = (decimal)latitude,  // Geocoded coordinates
            Longitude = (decimal)longitude,
            CreatedAt = DateTime.UtcNow,
            UserId = 0 // Will be set when linking with user account (future story)
        };

        // Save contractor
        var createdContractor = await _repository.CreateAsync(contractor);

        _logger.LogInformation(
            "Contractor created successfully by dispatcher {DispatcherId}: ContractorId={ContractorId}, Name={Name}",
            dispatcherId, createdContractor.Id, createdContractor.Name);

        return MapToResponse(createdContractor, request.WorkingHours.WorkDays);
    }

    /// <summary>
    /// Retrieves a single contractor by ID.
    /// </summary>
    public async Task<ContractorResponse> GetContractorAsync(int id)
    {
        var contractor = await _repository.GetByIdAsync(id);
        
        if (contractor == null)
        {
            throw new NotFoundException($"Contractor with ID {id} not found");
        }

        // For now, work days are not stored, so we return empty array
        // In a future iteration, work days should be stored in the database
        return MapToResponse(contractor, Array.Empty<string>());
    }

    /// <summary>
    /// Retrieves all active contractors with pagination.
    /// </summary>
    public async Task<PaginatedResponse<ContractorResponse>> GetAllContractorsAsync(int pageNumber, int pageSize)
    {
        // Validate pagination
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 100) pageSize = 100;

        var (contractors, totalCount) = await _repository.GetAllActiveAsync(pageNumber, pageSize);

        var items = contractors.Select(c => MapToResponse(c, Array.Empty<string>())).ToList();

        return new PaginatedResponse<ContractorResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Updates an existing contractor (partial update).
    /// </summary>
    public async Task<ContractorResponse> UpdateContractorAsync(int id, UpdateContractorRequest request, int dispatcherId)
    {
        ArgumentNullException.ThrowIfNull(request);

        var contractor = await _repository.GetByIdAsync(id);
        if (contractor == null)
        {
            throw new NotFoundException($"Contractor with ID {id} not found");
        }

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            if (request.Name.Length < 2 || request.Name.Length > 100)
            {
                throw new ValidationException("Name must be between 2 and 100 characters");
            }
            contractor.Name = request.Name;
        }

        // Update location if provided
        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            if (request.Location.Length < 5 || request.Location.Length > 200)
            {
                throw new ValidationException("Location must be between 5 and 200 characters");
            }
            contractor.Location = request.Location;
            
            // Re-geocode the new location
            var (latitude, longitude) = await _geocodingService.GeocodeAddressAsync(request.Location);
            contractor.Latitude = (decimal)latitude;
            contractor.Longitude = (decimal)longitude;
            
            _logger.LogInformation("Location updated for contractor {ContractorId}. New coordinates: ({Latitude}, {Longitude})", 
                contractor.Id, latitude, longitude);
        }

        // Update phone if provided
        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            if (request.Phone != contractor.PhoneNumber)
            {
                // Check uniqueness for new phone
                var phoneExists = await _repository.ExistsByPhoneAsync(request.Phone, id);
                if (phoneExists)
                {
                    _logger.LogWarning("Attempt to update contractor with duplicate phone: {Phone}", MaskPhone(request.Phone));
                    throw new ValidationException("Phone number already registered");
                }
            }
            contractor.PhoneNumber = request.Phone;
        }

        // Update trade type if provided
        if (!string.IsNullOrWhiteSpace(request.TradeType))
        {
            if (!Enum.TryParse<TradeType>(request.TradeType, true, out var tradeType))
            {
                var validTypes = string.Join(", ", Enum.GetNames(typeof(TradeType)));
                throw new ValidationException($"Invalid TradeType. Supported types: {validTypes}");
            }
            contractor.TradeType = tradeType;
        }

        // Update working hours if provided
        if (request.WorkingHours != null)
        {
            if (!TimeSpan.TryParse(request.WorkingHours.StartTime, out var startTime))
            {
                throw new ValidationException("Invalid StartTime format. Use HH:mm or HH:mm:ss");
            }

            if (!TimeSpan.TryParse(request.WorkingHours.EndTime, out var endTime))
            {
                throw new ValidationException("Invalid EndTime format. Use HH:mm or HH:mm:ss");
            }

            if (endTime <= startTime)
            {
                throw new ValidationException("EndTime must be after StartTime");
            }

            if (request.WorkingHours.WorkDays != null && request.WorkingHours.WorkDays.Length > 0)
            {
                // Validate work days
                var validDays = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                foreach (var day in request.WorkingHours.WorkDays)
                {
                    if (!validDays.Contains(day))
                    {
                        throw new ValidationException($"Invalid work day: {day}. Valid days: Mon, Tue, Wed, Thu, Fri, Sat, Sun");
                    }
                }
            }

            contractor.WorkingHoursStart = startTime;
            contractor.WorkingHoursEnd = endTime;
        }

        contractor.UpdatedAt = DateTime.UtcNow;

        // Save updates
        await _repository.UpdateAsync(contractor);

        _logger.LogInformation(
            "Contractor updated by dispatcher {DispatcherId}: ContractorId={ContractorId}, Name={Name}",
            dispatcherId, contractor.Id, contractor.Name);

        return MapToResponse(contractor, Array.Empty<string>());
    }

    /// <summary>
    /// Deactivates a contractor (soft delete).
    /// </summary>
    public async Task DeactivateContractorAsync(int id, int dispatcherId)
    {
        var contractor = await _repository.GetByIdAsync(id);
        if (contractor == null)
        {
            throw new NotFoundException($"Contractor with ID {id} not found");
        }

        await _repository.DeactivateAsync(id);

        _logger.LogInformation(
            "Contractor deactivated by dispatcher {DispatcherId}: ContractorId={ContractorId}, Name={Name}",
            dispatcherId, contractor.Id, contractor.Name);
    }

    /// <summary>
    /// Validates create contractor request.
    /// </summary>
    private static void ValidateCreateRequest(CreateContractorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Name is required");
        }

        if (request.Name.Length < 2 || request.Name.Length > 100)
        {
            throw new ValidationException("Name must be between 2 and 100 characters");
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            throw new ValidationException("Location is required");
        }

        if (request.Location.Length < 5 || request.Location.Length > 200)
        {
            throw new ValidationException("Location must be between 5 and 200 characters");
        }

        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new ValidationException("Phone is required");
        }

        // Validate phone format (basic E.164 or standard format)
        if (!IsValidPhoneFormat(request.Phone))
        {
            throw new ValidationException("Invalid phone format. Use E.164 format (+1234567890) or standard format (123-456-7890)");
        }

        if (string.IsNullOrWhiteSpace(request.TradeType))
        {
            throw new ValidationException("TradeType is required");
        }

        if (request.WorkingHours == null)
        {
            throw new ValidationException("WorkingHours is required");
        }
    }

    /// <summary>
    /// Validates phone format (basic validation).
    /// </summary>
    private static bool IsValidPhoneFormat(string phone)
    {
        // Accept E.164 format (+1234567890) or standard format (123-456-7890) or (123) 456-7890
        var e164Pattern = @"^\+?[1-9]\d{1,14}$";
        var standardPattern = @"^[0-9]{3}[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}$";
        
        return Regex.IsMatch(phone, e164Pattern) || Regex.IsMatch(phone, standardPattern);
    }

    /// <summary>
    /// Maps Contractor entity to ContractorResponse DTO.
    /// </summary>
    private static ContractorResponse MapToResponse(Contractor contractor, string[] workDays)
    {
        return new ContractorResponse
        {
            Id = contractor.Id,
            Name = contractor.Name,
            Phone = MaskPhone(contractor.PhoneNumber),
            TradeType = contractor.TradeType.ToString(),
            Location = contractor.Location,
            AverageRating = contractor.AverageRating,
            ReviewCount = contractor.ReviewCount,
            IsActive = contractor.IsActive,
            WorkingHours = new WorkingHoursDto
            {
                StartTime = contractor.WorkingHoursStart.ToString(@"hh\:mm"),
                EndTime = contractor.WorkingHoursEnd.ToString(@"hh\:mm"),
                WorkDays = workDays ?? Array.Empty<string>()
            }
        };
    }

    /// <summary>
    /// Masks phone number to show only last 4 digits for privacy.
    /// </summary>
    private static string MaskPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 4)
        {
            return "****";
        }

        var lastFour = phone[^4..];
        return $"****{lastFour}";
    }
}

