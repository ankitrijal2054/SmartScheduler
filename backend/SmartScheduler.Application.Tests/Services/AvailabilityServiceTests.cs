using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Collections.Generic;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;

namespace SmartScheduler.Application.Tests.Services;

/// <summary>
/// Unit tests for AvailabilityService.
/// Tests availability calculation considering working hours, assignments, travel time, and buffer time.
/// </summary>
public class AvailabilityServiceTests
{
    private readonly Mock<IAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<IContractorRepository> _contractorRepositoryMock;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<AvailabilityService>> _loggerMock;
    private readonly AvailabilityService _service;

    // Test contractor with 9 AM - 5 PM working hours
    private readonly Contractor _testContractor = new()
    {
        Id = 1,
        Name = "Test Contractor",
        PhoneNumber = "555-123-4567",
        Location = "123 Main St",
        TradeType = TradeType.Plumbing,
        WorkingHoursStart = TimeSpan.FromHours(9),
        WorkingHoursEnd = TimeSpan.FromHours(17),
        IsActive = true,
        UserId = 1,
        CreatedAt = DateTime.UtcNow
    };

    public AvailabilityServiceTests()
    {
        _assignmentRepositoryMock = new Mock<IAssignmentRepository>();
        _contractorRepositoryMock = new Mock<IContractorRepository>();
        _loggerMock = new Mock<ILogger<AvailabilityService>>();

        // Build real IConfiguration with test values
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "AvailabilityEngine:BufferTimeMinutes", "15" }
            });
        _configuration = configBuilder.Build();

        _service = new AvailabilityService(
            _assignmentRepositoryMock.Object,
            _contractorRepositoryMock.Object,
            _configuration,
            _loggerMock.Object);
    }

    private AvailabilityService CreateServiceWithBufferTime(int bufferMinutes)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "AvailabilityEngine:BufferTimeMinutes", bufferMinutes.ToString() }
            });
        var config = configBuilder.Build();

        return new AvailabilityService(
            _assignmentRepositoryMock.Object,
            _contractorRepositoryMock.Object,
            config,
            _loggerMock.Object);
    }

    #region Input Validation Tests

    [Fact]
    public async Task CalculateAvailabilityAsync_WithInvalidContractorId_ThrowsArgumentException()
    {
        // Arrange
        var desiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CalculateAvailabilityAsync(0, desiredDateTime, 2, 0));
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_WithNegativeJobDuration_ThrowsArgumentException()
    {
        // Arrange
        var desiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CalculateAvailabilityAsync(1, desiredDateTime, -1, 0));
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_WithNegativeTravelTime_ThrowsArgumentException()
    {
        // Arrange
        var desiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CalculateAvailabilityAsync(1, desiredDateTime, 2, -1));
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_WithNonexistentContractor_ThrowsNotFoundException()
    {
        // Arrange
        var desiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0);
        _contractorRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Contractor?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CalculateAvailabilityAsync(999, desiredDateTime, 2, 0));
    }

    #endregion

    #region AC 1: Working Hours Validation

    [Fact]
    public async Task CalculateAvailabilityAsync_WithJobStartOutsideWorkingHours_ReturnsFalse()
    {
        // Arrange
        var desiredDateTime = new DateTime(2025, 11, 10, 8, 0, 0); // 8 AM (before 9 AM start)
        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment>());

        // Act
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, 1, 0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_WithJobEndOutsideWorkingHours_ReturnsFalse()
    {
        // Arrange
        var desiredDateTime = new DateTime(2025, 11, 10, 16, 30, 0); // 4:30 PM
        var jobDuration = 1.5m; // 1.5 hours -> ends at 6 PM (after 5 PM end)
        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment>());

        // Act
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, jobDuration, 0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_WithJobWithinWorkingHours_ChecksAssignments()
    {
        // Arrange
        var desiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0); // 10 AM
        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment>());

        // Act
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, 2, 0);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region AC 2, 3: Contractor with No Assignments

    [Fact]
    public async Task CalculateAvailabilityAsync_WithNoAssignments_ReturnsTrue()
    {
        // Arrange
        var desiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0);
        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment>());

        // Act
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, 2, 0);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region AC 5, 6: Overlap and Non-Overlap Detection

    [Fact]
    public async Task CalculateAvailabilityAsync_WithOverlappingAssignment_ReturnsFalse()
    {
        // Arrange
        var desiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0); // 10 AM
        var existingJob = new Job
        {
            Id = 1,
            CustomerId = 1,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            DesiredDateTime = new DateTime(2025, 11, 10, 9, 30, 0), // 9:30 AM
            EstimatedDurationHours = 1.5m, // Ends at 11 AM
            Status = JobStatus.Assigned
        };
        var existingAssignment = new Assignment
        {
            Id = 1,
            JobId = 1,
            ContractorId = 1,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = existingJob
        };

        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment> { existingAssignment });

        // Act
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, 1, 0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_WithNonOverlappingAssignmentsSameDay_ReturnsTrue()
    {
        // Arrange
        var desiredDateTime = new DateTime(2025, 11, 10, 14, 0, 0); // 2 PM
        var existingJob = new Job
        {
            Id = 1,
            CustomerId = 1,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            DesiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0), // 10 AM
            EstimatedDurationHours = 1, // Ends at 11 AM
            Status = JobStatus.Assigned
        };
        var existingAssignment = new Assignment
        {
            Id = 1,
            JobId = 1,
            ContractorId = 1,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = existingJob
        };

        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment> { existingAssignment });

        // Act
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, 2, 0);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region AC 7: Buffer Time Enforcement

    [Fact]
    public async Task CalculateAvailabilityAsync_WithInsufficientBufferTimeAfterExistingJob_ReturnsFalse()
    {
        // Arrange: Existing job ends at 11 AM, buffer is 15 minutes, desired job starts at 11:10 AM
        var desiredDateTime = new DateTime(2025, 11, 10, 11, 10, 0); // 11:10 AM (only 10 min buffer)
        var existingJob = new Job
        {
            Id = 1,
            CustomerId = 1,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            DesiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0), // 10 AM
            EstimatedDurationHours = 1, // Ends at 11 AM
            Status = JobStatus.Assigned
        };
        var existingAssignment = new Assignment
        {
            Id = 1,
            JobId = 1,
            ContractorId = 1,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = existingJob
        };

        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment> { existingAssignment });

        // Act
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, 1, 0);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_WithExactBufferTime_ReturnsTrue()
    {
        // Arrange: Existing job ends at 11 AM, buffer is 15 minutes, desired job starts at 11:15 AM
        var desiredDateTime = new DateTime(2025, 11, 10, 11, 15, 0); // 11:15 AM (exactly 15 min buffer)
        var existingJob = new Job
        {
            Id = 1,
            CustomerId = 1,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            DesiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0), // 10 AM
            EstimatedDurationHours = 1, // Ends at 11 AM
            Status = JobStatus.Assigned
        };
        var existingAssignment = new Assignment
        {
            Id = 1,
            JobId = 1,
            ContractorId = 1,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = existingJob
        };

        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment> { existingAssignment });

        // Act
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, 1, 0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_EdgeCase_JobEndsAtNoonNextJobStartsAtNoon_ReturnsFalse()
    {
        // Arrange: First job ends at noon, second job starts at noon (no gap) -> should fail
        var desiredDateTime = new DateTime(2025, 11, 10, 12, 0, 0); // 12 PM
        var existingJob = new Job
        {
            Id = 1,
            CustomerId = 1,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            DesiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0), // 10 AM
            EstimatedDurationHours = 2, // Ends at 12 PM
            Status = JobStatus.Assigned
        };
        var existingAssignment = new Assignment
        {
            Id = 1,
            JobId = 1,
            ContractorId = 1,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = existingJob
        };

        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment> { existingAssignment });

        // Act
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, 1, 0);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AC 4: Travel Time Consideration

    [Fact]
    public async Task CalculateAvailabilityAsync_WithTravelTime_IncludesInDurationCalculation()
    {
        // Arrange: Job duration 1 hour + travel time 30 minutes
        // If desired job ends with travel time in overlap window, should return false
        var desiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0); // 10 AM
        var existingJob = new Job
        {
            Id = 1,
            CustomerId = 1,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            DesiredDateTime = new DateTime(2025, 11, 10, 11, 0, 0), // 11 AM
            EstimatedDurationHours = 1, // Ends at 12 PM
            Status = JobStatus.Assigned
        };
        var existingAssignment = new Assignment
        {
            Id = 1,
            JobId = 1,
            ContractorId = 1,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = existingJob
        };

        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment> { existingAssignment });

        // Act: Desired job 10 AM + 1 hour = 11 AM end + 30 min travel = 11:30 AM
        // Existing job starts at 11 AM, so overlap exists
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, 1, 30);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_WithTravelTimeNoOverlap_ReturnsTrue()
    {
        // Arrange: Desired job ends well before existing job with travel buffer
        var desiredDateTime = new DateTime(2025, 11, 10, 9, 0, 0); // 9 AM
        var existingJob = new Job
        {
            Id = 1,
            CustomerId = 1,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            DesiredDateTime = new DateTime(2025, 11, 10, 12, 0, 0), // 12 PM
            EstimatedDurationHours = 1,
            Status = JobStatus.Assigned
        };
        var existingAssignment = new Assignment
        {
            Id = 1,
            JobId = 1,
            ContractorId = 1,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = existingJob
        };

        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment> { existingAssignment });

        // Act: Desired job 9 AM + 1.5 hours = 10:30 AM end + 30 min travel = 11:00 AM
        // Existing job starts at 12 PM, buffer is 15 min, so 11 AM + 15 min = 11:15 AM (before 12 PM)
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, 1.5m, 30);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Multiple Assignments

    [Fact]
    public async Task CalculateAvailabilityAsync_WithMultipleAssignments_ChecksAllForOverlap()
    {
        // Arrange: Two non-overlapping jobs, but desired job overlaps with second
        var desiredDateTime = new DateTime(2025, 11, 10, 13, 0, 0); // 1 PM
        
        var job1 = new Job
        {
            Id = 1,
            CustomerId = 1,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            DesiredDateTime = new DateTime(2025, 11, 10, 9, 0, 0), // 9 AM
            EstimatedDurationHours = 1.5m, // Ends at 10:30 AM
            Status = JobStatus.Assigned
        };
        var assignment1 = new Assignment
        {
            Id = 1,
            JobId = 1,
            ContractorId = 1,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = job1
        };

        var job2 = new Job
        {
            Id = 2,
            CustomerId = 1,
            JobType = TradeType.Plumbing,
            Location = "789 Elm St",
            DesiredDateTime = new DateTime(2025, 11, 10, 12, 30, 0), // 12:30 PM
            EstimatedDurationHours = 1.5m, // Ends at 2 PM
            Status = JobStatus.Assigned
        };
        var assignment2 = new Assignment
        {
            Id = 2,
            JobId = 2,
            ContractorId = 1,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = job2
        };

        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment> { assignment1, assignment2 });

        // Act: Desired job 1 PM + 1 hour = 2 PM end (overlaps with job2's 12:30-2:00 PM)
        var result = await _service.CalculateAvailabilityAsync(1, desiredDateTime, 1, 0);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public async Task CalculateAvailabilityAsync_WithCustomBufferTime_UsesConfiguredValue()
    {
        // Arrange: Custom buffer time of 30 minutes
        var service = CreateServiceWithBufferTime(30);

        var desiredDateTime = new DateTime(2025, 11, 10, 11, 20, 0); // 11:20 AM (only 20 min buffer)
        var existingJob = new Job
        {
            Id = 1,
            CustomerId = 1,
            JobType = TradeType.Plumbing,
            Location = "456 Oak Ave",
            DesiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0), // 10 AM
            EstimatedDurationHours = 1, // Ends at 11 AM
            Status = JobStatus.Assigned
        };
        var existingAssignment = new Assignment
        {
            Id = 1,
            JobId = 1,
            ContractorId = 1,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = existingJob
        };

        _contractorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_testContractor);
        _assignmentRepositoryMock
            .Setup(r => r.GetActiveAssignmentsByContractorAndDateAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Assignment> { existingAssignment });

        // Act
        var result = await service.CalculateAvailabilityAsync(1, desiredDateTime, 1, 0);

        // Assert: Should fail with 30-minute buffer requirement
        result.Should().BeFalse();
    }

    #endregion
}

