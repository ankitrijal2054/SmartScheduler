using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Events;
using SmartScheduler.Infrastructure.EventHandlers;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Application.Tests.EventHandlers;

/// <summary>
/// Unit tests for JobAssignedEventHandler.
/// </summary>
public class JobAssignedEventHandlerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly IConfiguration _configuration;
    private readonly JobAssignedEventHandler _handler;

    public JobAssignedEventHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _emailServiceMock = new Mock<IEmailService>();

        // Setup configuration
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Frontend:BaseUrl", "http://localhost:5173" }
        });
        _configuration = configBuilder.Build();

        var loggerMock = new Mock<ILogger<JobAssignedEventHandler>>();
        _handler = new JobAssignedEventHandler(_emailServiceMock.Object, _dbContext, loggerMock.Object, _configuration);
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
            AverageRating = 4.8m,
            ReviewCount = 25,
            TotalJobsCompleted = 100
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
            Status = JobStatus.Assigned,
            AssignedContractorId = contractor.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create assignment
        var assignment = new Assignment
        {
            Id = 1,
            JobId = job.Id,
            ContractorId = contractor.Id,
            AssignedAt = DateTime.UtcNow,
            Status = AssignmentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        _dbContext.Customers.Add(customer);
        _dbContext.Contractors.Add(contractor);
        _dbContext.Jobs.Add(job);
        _dbContext.Assignments.Add(assignment);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidEvent_SendsEmailWithCorrectData()
    {
        // Arrange
        SeedTestData();
        _emailServiceMock
            .Setup(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<EmailTemplateDataDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var jobEvent = new JobAssignedEvent(jobId: 1, assignmentId: 1, contractorId: 1, customerId: 1);

        // Act
        await _handler.Handle(jobEvent, CancellationToken.None);

        // Assert
        _emailServiceMock.Verify(
            m => m.SendEmailAsync(
                It.Is<string>(x => x == "customer@test.com"),
                It.IsAny<string>(),
                It.Is<string>(x => x == "JobAssignedToCustomer"),
                It.Is<EmailTemplateDataDto>(x => 
                    x.CustomerEmail == "customer@test.com" &&
                    x.CustomerName == "John Customer" &&
                    x.ContractorName == "Jane Plumber" &&
                    x.ContractorPhone == "555-0002" &&
                    x.ContractorRating == 4.8m &&
                    x.JobId == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullRatingContractor_HandlesGracefully()
    {
        // Arrange
        SeedTestData();

        // Set contractor rating to null
        var contractor = await _dbContext.Contractors.FindAsync(1);
        contractor!.AverageRating = null;
        _dbContext.Contractors.Update(contractor);
        await _dbContext.SaveChangesAsync();

        _emailServiceMock
            .Setup(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<EmailTemplateDataDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var jobEvent = new JobAssignedEvent(jobId: 1, assignmentId: 1, contractorId: 1, customerId: 1);

        // Act
        await _handler.Handle(jobEvent, CancellationToken.None);

        // Assert - Should still send email
        _emailServiceMock.Verify(
            m => m.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<EmailTemplateDataDto>(x => x.ContractorRating == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithMissingJob_LogsWarningAndReturns()
    {
        // Arrange
        SeedTestData();
        var jobEvent = new JobAssignedEvent(jobId: 999, assignmentId: 1, contractorId: 1, customerId: 1);

        // Act
        await _handler.Handle(jobEvent, CancellationToken.None);

        // Assert - Should not send email
        _emailServiceMock.Verify(
            m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<EmailTemplateDataDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithMissingCustomer_LogsWarningAndReturns()
    {
        // Arrange
        SeedTestData();
        var jobEvent = new JobAssignedEvent(jobId: 1, assignmentId: 1, contractorId: 1, customerId: 999);

        // Act
        await _handler.Handle(jobEvent, CancellationToken.None);

        // Assert - Should not send email
        _emailServiceMock.Verify(
            m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<EmailTemplateDataDto>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmailServiceFailure_LogsErrorButDoesNotThrow()
    {
        // Arrange
        SeedTestData();
        _emailServiceMock
            .Setup(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<EmailTemplateDataDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var jobEvent = new JobAssignedEvent(jobId: 1, assignmentId: 1, contractorId: 1, customerId: 1);

        // Act
        var action = () => _handler.Handle(jobEvent, CancellationToken.None);

        // Assert - Should not throw
        await action.Should().NotThrowAsync();
    }
}

