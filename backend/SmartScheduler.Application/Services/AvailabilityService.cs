using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Service implementation for calculating contractor availability.
/// Determines whether a contractor is available for a job at a given time,
/// considering working hours, existing assignments, travel time, and buffer time between jobs.
/// </summary>
public class AvailabilityService : IAvailabilityService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IContractorRepository _contractorRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AvailabilityService> _logger;

    // Default buffer time in minutes if not configured
    private const int DefaultBufferTimeMinutes = 15;

    public AvailabilityService(
        IAssignmentRepository assignmentRepository,
        IContractorRepository contractorRepository,
        IConfiguration configuration,
        ILogger<AvailabilityService> logger)
    {
        _assignmentRepository = assignmentRepository ?? throw new ArgumentNullException(nameof(assignmentRepository));
        _contractorRepository = contractorRepository ?? throw new ArgumentNullException(nameof(contractorRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculates whether a contractor is available for a job at the desired date and time.
    /// Considers contractor working hours, existing assignments, travel time, and buffer time.
    /// </summary>
    public async Task<bool> CalculateAvailabilityAsync(int contractorId, DateTime desiredDateTime, decimal jobDurationHours, int travelTimeMinutes = 0)
    {
        // Validate inputs
        if (contractorId <= 0)
        {
            throw new ArgumentException("Contractor ID must be a positive number", nameof(contractorId));
        }

        if (jobDurationHours <= 0)
        {
            throw new ArgumentException("Job duration must be greater than zero", nameof(jobDurationHours));
        }

        if (travelTimeMinutes < 0)
        {
            throw new ArgumentException("Travel time cannot be negative", nameof(travelTimeMinutes));
        }

        // Retrieve contractor
        var contractor = await _contractorRepository.GetByIdAsync(contractorId);
        if (contractor == null)
        {
            throw new NotFoundException($"Contractor with ID {contractorId} not found");
        }

        // Get buffer time from configuration
        var bufferTimeMinutes = GetBufferTimeMinutes();

        // Check if job falls within working hours
        if (!IsWithinWorkingHours(desiredDateTime, contractor.WorkingHoursStart, contractor.WorkingHoursEnd))
        {
            _logger.LogInformation(
                "Availability check failed for ContractorId={ContractorId}, DesiredDateTime={DesiredDateTime}: Job outside working hours",
                contractorId, desiredDateTime);
            return false;
        }

        // Calculate the job end time (including travel time)
        var jobEndTime = desiredDateTime.AddHours((double)jobDurationHours).AddMinutes(travelTimeMinutes);

        // Check if job end time is still within working hours
        if (!IsWithinWorkingHours(jobEndTime, contractor.WorkingHoursStart, contractor.WorkingHoursEnd))
        {
            _logger.LogInformation(
                "Availability check failed for ContractorId={ContractorId}, DesiredDateTime={DesiredDateTime}: Job extends beyond working hours",
                contractorId, desiredDateTime);
            return false;
        }

        // Get all active assignments for the contractor on the target date
        var activeAssignments = await _assignmentRepository.GetActiveAssignmentsByContractorAndDateAsync(contractorId, desiredDateTime);

        // Check for overlaps with existing assignments
        foreach (var assignment in activeAssignments)
        {
            if (assignment.Job == null)
            {
                continue;
            }

            // Calculate existing job's time block (without buffer initially)
            var existingJobStart = assignment.Job.DesiredDateTime;
            var existingJobEnd = assignment.Job.DesiredDateTime
                .AddHours((double)assignment.Job.EstimatedDurationHours);

            // Check if there's buffer time between the desired job and existing jobs
            // Case 1: Desired job ends, then existing job starts (need buffer after desired job)
            var desiredJobEndWithBuffer = jobEndTime.AddMinutes(bufferTimeMinutes);
            if (desiredJobEndWithBuffer > existingJobStart && jobEndTime <= existingJobStart)
            {
                _logger.LogInformation(
                    "Availability check failed for ContractorId={ContractorId}, DesiredDateTime={DesiredDateTime}: " +
                    "Insufficient buffer time before existing job (AssignmentId={AssignmentId})",
                    contractorId, desiredDateTime, assignment.Id);
                return false;
            }

            // Case 2: Existing job ends, then desired job starts (need buffer before desired job)
            var existingJobEndWithBuffer = existingJobEnd.AddMinutes(bufferTimeMinutes);
            if (existingJobEndWithBuffer > desiredDateTime && existingJobEnd <= desiredDateTime)
            {
                _logger.LogInformation(
                    "Availability check failed for ContractorId={ContractorId}, DesiredDateTime={DesiredDateTime}: " +
                    "Insufficient buffer time after existing job (AssignmentId={AssignmentId})",
                    contractorId, desiredDateTime, assignment.Id);
                return false;
            }

            // Check for direct overlap (desired job overlaps with existing assignment time)
            if (HasTimeOverlap(desiredDateTime, jobEndTime, existingJobStart, existingJobEnd))
            {
                _logger.LogInformation(
                    "Availability check failed for ContractorId={ContractorId}, DesiredDateTime={DesiredDateTime}: " +
                    "Overlaps with existing AssignmentId={AssignmentId}, ExistingJobStart={ExistingJobStart}, ExistingJobEnd={ExistingJobEnd}",
                    contractorId, desiredDateTime, assignment.Id, existingJobStart, existingJobEnd);
                return false;
            }
        }

        _logger.LogInformation(
            "Availability check passed for ContractorId={ContractorId}, DesiredDateTime={DesiredDateTime}, Duration={DurationHours}h, Travel={TravelMinutes}m",
            contractorId, desiredDateTime, jobDurationHours, travelTimeMinutes);

        return true;
    }

    /// <summary>
    /// Checks if a given time falls within contractor working hours.
    /// Compares only the time portion (not the date).
    /// </summary>
    private static bool IsWithinWorkingHours(DateTime dateTime, TimeSpan workingHoursStart, TimeSpan workingHoursEnd)
    {
        var timeOfDay = dateTime.TimeOfDay;
        return timeOfDay >= workingHoursStart && timeOfDay < workingHoursEnd;
    }

    /// <summary>
    /// Checks if two time periods overlap.
    /// </summary>
    /// <param name="start1">Start of first period</param>
    /// <param name="end1">End of first period</param>
    /// <param name="start2">Start of second period</param>
    /// <param name="end2">End of second period</param>
    /// <returns>True if periods overlap, false otherwise</returns>
    private static bool HasTimeOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
    {
        return start1 < end2 && start2 < end1;
    }

    /// <summary>
    /// Gets buffer time in minutes from configuration.
    /// Falls back to default if not configured.
    /// </summary>
    private int GetBufferTimeMinutes()
    {
        var configured = _configuration.GetValue<int?>("AvailabilityEngine:BufferTimeMinutes");
        return configured ?? DefaultBufferTimeMinutes;
    }
}

