using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Infrastructure.Persistence;
using SmartScheduler.Infrastructure.Repositories;

namespace SmartScheduler.Infrastructure.Tests.Services;

/// <summary>
/// Integration tests for AvailabilityService using real database context.
/// Tests full availability calculation flow with persisted data.
/// </summary>
public class AvailabilityServiceIntegrationTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IContractorRepository _contractorRepository;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<AvailabilityService>> _loggerMock;
    private readonly AvailabilityService _service;

    public AvailabilityServiceIntegrationTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _assignmentRepository = new AssignmentRepository(_dbContext);
        _contractorRepository = new ContractorRepository(_dbContext);

        _configurationMock = new Mock<IConfiguration>();
        _configurationMock
            .Setup(c => c.GetValue<int?>("AvailabilityEngine:BufferTimeMinutes", It.IsAny<int?>()))
            .Returns(15);

        _loggerMock = new Mock<ILogger<AvailabilityService>>();

        _service = new AvailabilityService(
            _assignmentRepository,
            _contractorRepository,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    #region Basic Integration Tests

    [Fact]
    public async Task CalculateAvailabilityAsync_WithSeedDataContractorAndNoAssignments_ReturnsTrue()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "John Plumber",
            Location = "123 Main St",
            PhoneNumber = "555-123-4567",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor);
        await _dbContext.SaveChangesAsync();

        var desiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0);

        // Act
        var result = await _service.CalculateAvailabilityAsync(contractor.Id, desiredDateTime, 2, 0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_WithSeedDataAndActiveAssignment_ChecksOverlap()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Jane HVAC Tech",
            Location = "456 Oak Ave",
            PhoneNumber = "555-987-6543",
            TradeType = TradeType.HVAC,
            WorkingHoursStart = TimeSpan.FromHours(8),
            WorkingHoursEnd = TimeSpan.FromHours(18),
            IsActive = true,
            UserId = 2,
            CreatedAt = DateTime.UtcNow
        };

        var customer = new Customer
        {
            Name = "Bob Customer",
            Location = "789 Elm St",
            PhoneNumber = "555-111-2222",
            UserId = 3,
            CreatedAt = DateTime.UtcNow
        };

        var job = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.HVAC,
            Location = "999 Pine St",
            DesiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0),
            EstimatedDurationHours = 2,
            Description = "AC Repair",
            Status = JobStatus.Assigned
        };

        var assignment = new Assignment
        {
            JobId = job.Id,
            ContractorId = contractor.Id,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = job
        };

        _dbContext.Contractors.Add(contractor);
        _dbContext.Customers.Add(customer);
        _dbContext.Jobs.Add(job);
        _dbContext.Assignments.Add(assignment);
        await _dbContext.SaveChangesAsync();

        var desiredDateTime = new DateTime(2025, 11, 10, 11, 0, 0); // 11 AM (overlaps with 10-12 PM existing)

        // Act
        var result = await _service.CalculateAvailabilityAsync(contractor.Id, desiredDateTime, 1, 0);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public async Task CalculateAvailabilityAsync_MultipleContractorsDoNotInterfere()
    {
        // Arrange: Two contractors, each with own assignments
        var contractor1 = new Contractor
        {
            Name = "Contractor 1",
            Location = "Address 1",
            PhoneNumber = "555-001-0001",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var contractor2 = new Contractor
        {
            Name = "Contractor 2",
            Location = "Address 2",
            PhoneNumber = "555-002-0002",
            TradeType = TradeType.HVAC,
            WorkingHoursStart = TimeSpan.FromHours(8),
            WorkingHoursEnd = TimeSpan.FromHours(16),
            IsActive = true,
            UserId = 2,
            CreatedAt = DateTime.UtcNow
        };

        var customer = new Customer
        {
            Name = "Customer",
            Location = "Customer Location",
            PhoneNumber = "555-999-9999",
            UserId = 3,
            CreatedAt = DateTime.UtcNow
        };

        var job1 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "Job 1 Location",
            DesiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0),
            EstimatedDurationHours = 3,
            Description = "Job 1",
            Status = JobStatus.Assigned
        };

        var assignment1 = new Assignment
        {
            JobId = job1.Id,
            ContractorId = contractor1.Id,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.InProgress,
            Job = job1
        };

        _dbContext.Contractors.AddRange(contractor1, contractor2);
        _dbContext.Customers.Add(customer);
        _dbContext.Jobs.Add(job1);
        _dbContext.Assignments.Add(assignment1);
        await _dbContext.SaveChangesAsync();

        // Act: Check contractor2's availability (should not be affected by contractor1's assignment)
        var result = await _service.CalculateAvailabilityAsync(
            contractor2.Id,
            new DateTime(2025, 11, 10, 10, 0, 0),
            2,
            0);

        // Assert: Contractor 2 should be available
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_IgnoresDeclinedAssignments()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Test Contractor",
            Location = "Test Location",
            PhoneNumber = "555-555-5555",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var customer = new Customer
        {
            Name = "Customer",
            Location = "Customer Location",
            PhoneNumber = "555-999-9999",
            UserId = 2,
            CreatedAt = DateTime.UtcNow
        };

        var job1 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "Job 1",
            DesiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0),
            EstimatedDurationHours = 2,
            Description = "Job 1",
            Status = JobStatus.Cancelled
        };

        var declinedAssignment = new Assignment
        {
            JobId = job1.Id,
            ContractorId = contractor.Id,
            AssignedAt = DateTime.UtcNow,
            DeclinedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Declined,
            Job = job1
        };

        _dbContext.Contractors.Add(contractor);
        _dbContext.Customers.Add(customer);
        _dbContext.Jobs.Add(job1);
        _dbContext.Assignments.Add(declinedAssignment);
        await _dbContext.SaveChangesAsync();

        // Act: Check availability for same time slot (should be available since assignment was declined)
        var result = await _service.CalculateAvailabilityAsync(
            contractor.Id,
            new DateTime(2025, 11, 10, 10, 0, 0),
            2,
            0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateAvailabilityAsync_IgnoresCompletedAssignments()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Test Contractor",
            Location = "Test Location",
            PhoneNumber = "555-444-4444",
            TradeType = TradeType.Electrical,
            WorkingHoursStart = TimeSpan.FromHours(7),
            WorkingHoursEnd = TimeSpan.FromHours(15),
            IsActive = true,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var customer = new Customer
        {
            Name = "Customer",
            Location = "Customer Location",
            PhoneNumber = "555-999-9999",
            UserId = 2,
            CreatedAt = DateTime.UtcNow
        };

        var job1 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Electrical,
            Location = "Job 1",
            DesiredDateTime = new DateTime(2025, 11, 10, 9, 0, 0),
            EstimatedDurationHours = 2,
            Description = "Job 1",
            Status = JobStatus.Completed
        };

        var completedAssignment = new Assignment
        {
            JobId = job1.Id,
            ContractorId = contractor.Id,
            AssignedAt = DateTime.UtcNow.AddHours(-2),
            CompletedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Completed,
            Job = job1
        };

        _dbContext.Contractors.Add(contractor);
        _dbContext.Customers.Add(customer);
        _dbContext.Jobs.Add(job1);
        _dbContext.Assignments.Add(completedAssignment);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CalculateAvailabilityAsync(
            contractor.Id,
            new DateTime(2025, 11, 10, 9, 0, 0),
            2,
            0);

        // Assert: Should be available since this assignment is completed
        result.Should().BeTrue();
    }

    #endregion

    #region Date and Time Filtering

    [Fact]
    public async Task CalculateAvailabilityAsync_QueriesOnlyRelevantDate()
    {
        // Arrange: Assignment on Nov 10, check availability on Nov 11
        var contractor = new Contractor
        {
            Name = "Test Contractor",
            Location = "Test Location",
            PhoneNumber = "555-333-3333",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var customer = new Customer
        {
            Name = "Customer",
            Location = "Customer Location",
            PhoneNumber = "555-999-9999",
            UserId = 2,
            CreatedAt = DateTime.UtcNow
        };

        var job1 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "Job 1",
            DesiredDateTime = new DateTime(2025, 11, 10, 10, 0, 0), // Nov 10
            EstimatedDurationHours = 8,
            Description = "Job 1",
            Status = JobStatus.Assigned
        };

        var assignment = new Assignment
        {
            JobId = job1.Id,
            ContractorId = contractor.Id,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            Job = job1
        };

        _dbContext.Contractors.Add(contractor);
        _dbContext.Customers.Add(customer);
        _dbContext.Jobs.Add(job1);
        _dbContext.Assignments.Add(assignment);
        await _dbContext.SaveChangesAsync();

        // Act: Check availability on Nov 11 at 10 AM
        var result = await _service.CalculateAvailabilityAsync(
            contractor.Id,
            new DateTime(2025, 11, 11, 10, 0, 0),
            2,
            0);

        // Assert: Should be available (different date)
        result.Should().BeTrue();
    }

    #endregion

    #region Complex Edge Cases

    [Fact]
    public async Task CalculateAvailabilityAsync_WithManyAssignments_PerformsEfficientQuery()
    {
        // Arrange: Create contractor with many assignments
        var contractor = new Contractor
        {
            Name = "Busy Contractor",
            Location = "Busy Location",
            PhoneNumber = "555-222-2222",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(6),
            WorkingHoursEnd = TimeSpan.FromHours(22),
            IsActive = true,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var customer = new Customer
        {
            Name = "Customer",
            Location = "Customer Location",
            PhoneNumber = "555-999-9999",
            UserId = 2,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor);
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();

        // Add 10 assignments on same day (different times)
        for (int i = 0; i < 10; i++)
        {
            var job = new Job
            {
                CustomerId = customer.Id,
                JobType = TradeType.Plumbing,
                Location = $"Job {i}",
                DesiredDateTime = new DateTime(2025, 11, 10, 6 + (i * 2), 0, 0),
                EstimatedDurationHours = 1.5m,
                Description = $"Job {i}",
                Status = JobStatus.Assigned
            };

            var assignment = new Assignment
            {
                JobId = job.Id,
                ContractorId = contractor.Id,
                AssignedAt = DateTime.UtcNow,
                Status = AssignmentStatus.Pending,
                Job = job
            };

            _dbContext.Jobs.Add(job);
            _dbContext.Assignments.Add(assignment);
        }
        await _dbContext.SaveChangesAsync();

        // Act: Check availability in a free slot
        var result = await _service.CalculateAvailabilityAsync(
            contractor.Id,
            new DateTime(2025, 11, 10, 21, 0, 0), // 9 PM (after most jobs)
            1,
            0);

        // Assert: Should be available
        result.Should().BeTrue();
    }

    #endregion
}

