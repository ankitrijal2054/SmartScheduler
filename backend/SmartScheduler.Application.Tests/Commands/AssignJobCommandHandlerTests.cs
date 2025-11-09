using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using SmartScheduler.Application.Commands;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Events;
using SmartScheduler.Domain.Exceptions;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Application.Tests.Commands;

/// <summary>
/// Unit tests for AssignJobCommandHandler.
/// </summary>
public class AssignJobCommandHandlerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AssignJobCommandHandler _handler;

    public AssignJobCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _mediatorMock = new Mock<IMediator>();
        var loggerMock = new Mock<ILogger<AssignJobCommandHandler>>();
        _handler = new AssignJobCommandHandler(_dbContext, _mediatorMock.Object, loggerMock.Object);
    }

    private void SeedTestData()
    {
        // Create test user
        var user = new User
        {
            Id = 1,
            Email = "customer@test.com",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create test customer
        var customer = new Customer
        {
            Id = 1,
            UserId = user.Id,
            Name = "John Customer",
            PhoneNumber = "555-0001",
            Location = "Denver, CO",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create test contractor
        var contractor = new Contractor
        {
            Id = 1,
            UserId = 2,
            Name = "Jane Plumber",
            PhoneNumber = "555-0002",
            Location = "Denver, CO",
            Latitude = 39.7392m,
            Longitude = -104.9903m,
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            AverageRating = 4.5m,
            ReviewCount = 10,
            TotalJobsCompleted = 50
        };

        // Create test job
        var job = new Job
        {
            Id = 1,
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "123 Main St",
            Latitude = 39.7392m,
            Longitude = -104.9903m,
            DesiredDateTime = DateTime.UtcNow.AddHours(2),
            EstimatedDurationHours = 2m,
            Description = "Fix leaky faucet",
            Status = JobStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        _dbContext.Customers.Add(customer);
        _dbContext.Contractors.Add(contractor);
        _dbContext.Jobs.Add(job);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidJobAndContractor_CreatesAssignmentAndPublishesEvent()
    {
        // Arrange
        SeedTestData();
        var command = new AssignJobCommand(jobId: 1, contractorId: 1, dispatcherId: 3);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);

        // Verify assignment was created
        var assignment = await _dbContext.Assignments
            .FirstOrDefaultAsync(a => a.JobId == 1 && a.ContractorId == 1);
        assignment.Should().NotBeNull();
        assignment!.Status.Should().Be(AssignmentStatus.Pending);

        // Verify job status updated
        var job = await _dbContext.Jobs.FindAsync(1);
        job!.Status.Should().Be(JobStatus.Assigned);
        job.AssignedContractorId.Should().Be(1);

        // Verify event was published
        _mediatorMock.Verify(
            m => m.Publish(It.IsAny<JobAssignedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentJob_ThrowsNotFoundException()
    {
        // Arrange
        SeedTestData();
        var command = new AssignJobCommand(jobId: 999, contractorId: 1, dispatcherId: 3);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonExistentContractor_ThrowsNotFoundException()
    {
        // Arrange
        SeedTestData();
        var command = new AssignJobCommand(jobId: 1, contractorId: 999, dispatcherId: 3);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInactiveContractor_ThrowsValidationException()
    {
        // Arrange
        SeedTestData();

        // Deactivate contractor
        var contractor = await _dbContext.Contractors.FindAsync(1);
        contractor!.IsActive = false;
        _dbContext.Contractors.Update(contractor);
        await _dbContext.SaveChangesAsync();

        var command = new AssignJobCommand(jobId: 1, contractorId: 1, dispatcherId: 3);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithAlreadyAssignedJob_ThrowsValidationException()
    {
        // Arrange
        SeedTestData();

        // First assignment
        var command1 = new AssignJobCommand(jobId: 1, contractorId: 1, dispatcherId: 3);
        await _handler.Handle(command1, CancellationToken.None);

        // Try to assign same job again
        var command2 = new AssignJobCommand(jobId: 1, contractorId: 1, dispatcherId: 3);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command2, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNonPendingJob_ThrowsValidationException()
    {
        // Arrange
        SeedTestData();

        // Change job status to assigned
        var job = await _dbContext.Jobs.FindAsync(1);
        job!.Status = JobStatus.Assigned;
        job.AssignedContractorId = 1;
        _dbContext.Jobs.Update(job);
        await _dbContext.SaveChangesAsync();

        var command = new AssignJobCommand(jobId: 1, contractorId: 1, dispatcherId: 3);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}

