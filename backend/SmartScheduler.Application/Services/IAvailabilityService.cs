namespace SmartScheduler.Application.Services;

/// <summary>
/// Service interface for calculating contractor availability.
/// Determines whether a contractor is available for a job at a given time,
/// considering working hours, existing assignments, and buffer time between jobs.
/// </summary>
public interface IAvailabilityService
{
    /// <summary>
    /// Calculates whether a contractor is available for a job at the desired date and time.
    /// Considers contractor working hours, existing assignments, travel time, and buffer time.
    /// </summary>
    /// <param name="contractorId">The ID of the contractor to check availability for.</param>
    /// <param name="desiredDateTime">The desired start date and time for the job.</param>
    /// <param name="jobDurationHours">The estimated duration of the job in hours.</param>
    /// <param name="travelTimeMinutes">The travel time in minutes (default 0 for now, will be provided by Story 2.3).</param>
    /// <returns>True if the contractor is available, false otherwise.</returns>
    /// <exception cref="ArgumentException">If contractorId is invalid or jobDurationHours is non-positive.</exception>
    /// <exception cref="SmartScheduler.Domain.Exceptions.NotFoundException">If contractor not found.</exception>
    Task<bool> CalculateAvailabilityAsync(int contractorId, DateTime desiredDateTime, decimal jobDurationHours, int travelTimeMinutes = 0);
}

