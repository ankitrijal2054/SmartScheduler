using Microsoft.Extensions.Logging;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Service for calculating contractor scores and generating ranked recommendations.
/// Implements weighted scoring formula: (0.4 × availability) + (0.3 × rating) + (0.3 × distance)
/// </summary>
public class ScoringService : IScoringService
{
    private const decimal AvailabilityWeight = 0.4m;
    private const decimal RatingWeight = 0.3m;
    private const decimal DistanceWeight = 0.3m;
    private const decimal MaxDistanceMiles = 50m;
    private const decimal NullRatingBaseline = 0.5m;
    private const decimal MaxRating = 5.0m;
    private const int MaxRecommendations = 5;
    private const decimal JobDurationHours = 8m;

    private readonly IContractorRepository _contractorRepository;
    private readonly IAvailabilityService _availabilityService;
    private readonly IDistanceService _distanceService;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ILogger<ScoringService> _logger;

    public ScoringService(
        IContractorRepository contractorRepository,
        IAvailabilityService availabilityService,
        IDistanceService distanceService,
        IAssignmentRepository assignmentRepository,
        ILogger<ScoringService> logger)
    {
        _contractorRepository = contractorRepository;
        _availabilityService = availabilityService;
        _distanceService = distanceService;
        _assignmentRepository = assignmentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets top 5 recommended contractors for a job, ranked by calculated score.
    /// </summary>
    public async Task<RecommendationResponseDto> GetRecommendationsAsync(int jobId, int dispatcherId, bool contractorListOnly = false)
    {
        _logger.LogInformation("Fetching recommendations for Job {JobId} by Dispatcher {DispatcherId}, ContractorListOnly={ContractorListOnly}", 
            jobId, dispatcherId, contractorListOnly);

        // Get job details
        var job = await _contractorRepository.GetJobByIdAsync(jobId);
        if (job == null)
        {
            throw new NotFoundException($"Job with ID {jobId} not found");
        }

        // Validate desired date/time is not in the past
        if (job.DesiredDateTime < DateTime.UtcNow)
        {
            throw new ArgumentException($"Desired date/time cannot be in the past: {job.DesiredDateTime}");
        }

        // Load contractor list based on filter
        List<int> contractorIds = new();
        if (contractorListOnly)
        {
            contractorIds = await _contractorRepository.GetDispatcherContractorListAsync(dispatcherId);
        }
        else
        {
            contractorIds = await _contractorRepository.GetActiveContractorIdsAsync();
        }

        if (contractorIds.Count == 0)
        {
            _logger.LogWarning("No contractors found for recommendation (ContractorListOnly={ContractorListOnly})", contractorListOnly);
            return new RecommendationResponseDto
            {
                Recommendations = new List<RecommendationDto>(),
                Message = "No available contractors"
            };
        }

        // Process each contractor in parallel for performance
        var tasks = contractorIds.Select(contractorId => ProcessContractorAsync(contractorId, job));
        var results = await Task.WhenAll(tasks);

        // Filter out null results and sort by score (descending)
        var validRecommendations = results
            .Where(r => r.HasValue)
            .OrderByDescending(r => r!.Value.Score)
            .Take(MaxRecommendations)
            .Select(r => r!.Value)
            .ToList();

        if (validRecommendations.Count == 0)
        {
            _logger.LogWarning("No available contractors for Job {JobId} after filtering", jobId);
            return new RecommendationResponseDto
            {
                Recommendations = new List<RecommendationDto>(),
                Message = "No available contractors"
            };
        }

        var dtos = validRecommendations.Select(r => r.Dto).ToList();

        _logger.LogInformation("Successfully retrieved {Count} recommendations for Job {JobId}", dtos.Count, jobId);

        return new RecommendationResponseDto
        {
            Recommendations = dtos,
            Message = "Success"
        };
    }

    /// <summary>
    /// Process a single contractor recommendation in parallel.
    /// </summary>
    private async Task<(decimal Score, RecommendationDto Dto)?> ProcessContractorAsync(int contractorId, Domain.Entities.Job job)
    {
        try
        {
            // Get contractor details
            var contractor = await _contractorRepository.GetContractorByIdAsync(contractorId);
            if (contractor == null || !contractor.IsActive)
            {
                return null;
            }

            // Check availability
            var isAvailable = await _availabilityService.CalculateAvailabilityAsync(
                contractorId, job.DesiredDateTime, JobDurationHours);

            var availabilityScore = isAvailable ? 1.0m : 0.0m;

            // Get distance and travel time
            var distance = await _distanceService.GetDistance(
                job.Latitude, job.Longitude,
                contractor.Latitude, contractor.Longitude);

            var travelTime = await _distanceService.GetTravelTime(
                job.Latitude, job.Longitude,
                contractor.Latitude, contractor.Longitude);

            // Calculate rating and distance scores
            var ratingScore = NormalizeRatingScore(contractor.AverageRating);
            var distanceScore = NormalizeDistanceScore(distance);

            // Calculate final score
            var finalScore = CalculateScore(availabilityScore, ratingScore, distanceScore);

            // Get available time slots
            var timeSlots = await GetAvailableTimeSlotsAsync(contractorId, job.DesiredDateTime.Date);

            var recommendationDto = new RecommendationDto
            {
                ContractorId = contractorId,
                Name = contractor.Name,
                Score = finalScore,
                Rating = contractor.AverageRating,
                ReviewCount = contractor.ReviewCount,
                Distance = distance,
                TravelTime = travelTime,
                AvailableTimeSlots = timeSlots
            };

            return (finalScore, recommendationDto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating recommendation score for Contractor {ContractorId}", contractorId);
            return null;
        }
    }

    /// <summary>
    /// Calculates the recommendation score for a contractor using weighted formula.
    /// </summary>
    public decimal CalculateScore(decimal availabilityScore, decimal ratingScore, decimal distanceScore)
    {
        // Validate inputs are in 0-1 range
        if (availabilityScore < 0 || availabilityScore > 1 ||
            ratingScore < 0 || ratingScore > 1 ||
            distanceScore < 0 || distanceScore > 1)
        {
            throw new ArgumentException("All scores must be between 0.0 and 1.0");
        }

        var score = (AvailabilityWeight * availabilityScore) +
                    (RatingWeight * ratingScore) +
                    (DistanceWeight * distanceScore);

        // Round to 2 decimal places for consistency
        return Math.Round(score, 2);
    }

    /// <summary>
    /// Gets available time slots for a contractor on a specific date.
    /// </summary>
    public async Task<List<DateTime>> GetAvailableTimeSlotsAsync(int contractorId, DateTime desiredDate)
    {
        try
        {
            // Get contractor details for working hours
            var contractor = await _contractorRepository.GetContractorByIdAsync(contractorId);
            if (contractor == null)
            {
                throw new NotFoundException($"Contractor with ID {contractorId} not found");
            }

            // Get all assignments for that contractor on the desired date
            var assignments = await _assignmentRepository.GetContractorAssignmentsByDateAsync(contractorId, desiredDate);

            var availableSlots = new List<DateTime>();

            // Generate 1-hour time slots within working hours, excluding occupied slots
            var workingStart = contractor.WorkingHoursStart;
            var workingEnd = contractor.WorkingHoursEnd;

            var currentTime = desiredDate.Add(workingStart);
            var endTime = desiredDate.Add(workingEnd);

            while (currentTime < endTime)
            {
                var slotEnd = currentTime.AddHours(1);

                // Check if this slot conflicts with any assignment
                var hasConflict = assignments.Any(a =>
                {
                    // Use Job.DesiredDateTime for assignment time
                    var assignmentStart = a.Job?.DesiredDateTime ?? DateTime.MinValue;
                    var duration = a.Job?.EstimatedDurationHours ?? 0m;
                    var assignmentEnd = assignmentStart.AddHours((double)duration);
                    return currentTime < assignmentEnd && slotEnd > assignmentStart;
                });

                if (!hasConflict)
                {
                    availableSlots.Add(currentTime);
                }

                currentTime = slotEnd;
            }

            return availableSlots;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting available time slots for Contractor {ContractorId}", contractorId);
            return new List<DateTime>();
        }
    }

    /// <summary>
    /// Normalizes a rating (0-5) to a 0.0-1.0 score range.
    /// </summary>
    public decimal NormalizeRatingScore(decimal? rating)
    {
        if (rating == null)
        {
            return NullRatingBaseline;
        }

        // Divide by 5.0 to normalize to 0-1 range
        var normalized = rating.Value / MaxRating;

        // Clamp to 0-1 range
        return Math.Min(1.0m, Math.Max(0.0m, normalized));
    }

    /// <summary>
    /// Normalizes distance (in miles) to a 0.0-1.0 score range.
    /// </summary>
    public decimal NormalizeDistanceScore(decimal distanceMiles)
    {
        if (distanceMiles <= 0)
        {
            return 1.0m;
        }

        if (distanceMiles >= MaxDistanceMiles)
        {
            return 0.0m;
        }

        // Formula: 1.0 - (distance / 50.0)
        var normalized = 1.0m - (distanceMiles / MaxDistanceMiles);

        // Clamp to 0-1 range
        return Math.Max(0.0m, normalized);
    }
}
