using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Events;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Infrastructure.EventHandlers;
using SmartScheduler.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SmartScheduler.Infrastructure.Tests.EventHandlers;

public class ContractorEmailHandlerTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<JobAssignedContractorEmailHandler>> _mockLoggerAssigned;
    private readonly Mock<ILogger<JobCancelledContractorEmailHandler>> _mockLoggerCancelled;
    private readonly Mock<ILogger<JobScheduleChangedContractorEmailHandler>> _mockLoggerScheduleChanged;
    private readonly Mock<ILogger<RatingPostedContractorEmailHandler>> _mockLoggerRatingPosted;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ApplicationDbContext _dbContext;

    public ContractorEmailHandlerTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockLoggerAssigned = new Mock<ILogger<JobAssignedContractorEmailHandler>>();
        _mockLoggerCancelled = new Mock<ILogger<JobCancelledContractorEmailHandler>>();
        _mockLoggerScheduleChanged = new Mock<ILogger<JobScheduleChangedContractorEmailHandler>>();
        _mockLoggerRatingPosted = new Mock<ILogger<RatingPostedContractorEmailHandler>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration["Frontend:BaseUrl"] = "http://localhost:5173";

        // Create in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
    }

    #region JobAssignedContractorEmailHandler Tests

    [Fact]
    public async Task JobAssignedHandler_SendsEmailToContractorWithCorrectTemplate()
    {
        // Arrange
        _mockEmailService.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<EmailTemplateDataDto>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(true);

        var (contractor, job, customer) = await SetupTestData();
        var @event = new JobAssignedEvent(job.Id, 1, contractor.Id, customer.Id);

        var handler = new JobAssignedContractorEmailHandler(
            _mockEmailService.Object,
            _dbContext,
            _mockLoggerAssigned.Object,
            _mockConfiguration.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _mockEmailService.Verify(
            s => s.SendEmailAsync(
                contractor.User!.Email,
                It.IsAny<string>(),
                "JobAssignedToContractor",
                It.IsAny<EmailTemplateDataDto>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task JobAssignedHandler_PopulatesEmailDataWithCorrectValues()
    {
        // Arrange
        EmailTemplateDataDto? capturedData = null;

        _mockEmailService.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<EmailTemplateDataDto>(),
            It.IsAny<CancellationToken>()
        )).Callback<string, string, string, EmailTemplateDataDto, CancellationToken>(
            (to, subject, template, data, ct) => capturedData = data
        ).ReturnsAsync(true);

        var (contractor, job, customer) = await SetupTestData();
        var @event = new JobAssignedEvent(job.Id, 1, contractor.Id, customer.Id);

        var handler = new JobAssignedContractorEmailHandler(
            _mockEmailService.Object,
            _dbContext,
            _mockLoggerAssigned.Object,
            _mockConfiguration.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        capturedData.Should().NotBeNull();
        capturedData!.ContractorEmail.Should().Be(contractor.User!.Email);
        capturedData.ContractorName.Should().Be(contractor.Name);
        capturedData.CustomerName.Should().Be(customer.Name);
        capturedData.JobType.Should().Be(job.JobType.ToString());
        capturedData.Location.Should().Be(job.Location);
        capturedData.Description.Should().Be(job.Description);
        capturedData.AcceptJobLink.Should().Contain($"/contractor/jobs/{job.Id}/accept");
        capturedData.DeclineJobLink.Should().Contain($"/contractor/jobs/{job.Id}/decline");
    }

    [Fact]
    public async Task JobAssignedHandler_HandlesContractorNotFound_GracefullyAndContinues()
    {
        // Arrange
        var @event = new JobAssignedEvent(jobId: 1, assignmentId: 1, contractorId: 9999, customerId: 1);

        var handler = new JobAssignedContractorEmailHandler(
            _mockEmailService.Object,
            _dbContext,
            _mockLoggerAssigned.Object,
            _mockConfiguration.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert - should not throw, should log warning
        _mockEmailService.Verify(
            s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EmailTemplateDataDto>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task JobAssignedHandler_HandlesEmailServiceException_LogsAndContinues()
    {
        // Arrange
        _mockEmailService.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<EmailTemplateDataDto>(),
            It.IsAny<CancellationToken>()
        )).ThrowsAsync(new Exception("Email service error"));

        var (contractor, job, customer) = await SetupTestData();
        var @event = new JobAssignedEvent(job.Id, 1, contractor.Id, customer.Id);

        var handler = new JobAssignedContractorEmailHandler(
            _mockEmailService.Object,
            _dbContext,
            _mockLoggerAssigned.Object,
            _mockConfiguration.Object
        );

        // Act - should not throw
        Func<Task> act = async () => await handler.Handle(@event, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region JobCancelledContractorEmailHandler Tests

    [Fact]
    public async Task JobCancelledHandler_SendsEmailToContractorWithCorrectTemplate()
    {
        // Arrange
        _mockEmailService.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<EmailTemplateDataDto>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(true);

        var (contractor, job, _) = await SetupTestData();
        var @event = new JobCancelledEvent(job.Id, contractor.Id, "Customer requested cancellation");

        var handler = new JobCancelledContractorEmailHandler(
            _mockEmailService.Object,
            _dbContext,
            _mockLoggerCancelled.Object,
            _mockConfiguration.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _mockEmailService.Verify(
            s => s.SendEmailAsync(
                contractor.User!.Email,
                It.IsAny<string>(),
                "JobCancelledForContractor",
                It.IsAny<EmailTemplateDataDto>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task JobCancelledHandler_IncludesCancellationReason()
    {
        // Arrange
        EmailTemplateDataDto? capturedData = null;
        var reason = "Customer requested cancellation";

        _mockEmailService.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<EmailTemplateDataDto>(),
            It.IsAny<CancellationToken>()
        )).Callback<string, string, string, EmailTemplateDataDto, CancellationToken>(
            (to, subject, template, data, ct) => capturedData = data
        ).ReturnsAsync(true);

        var (contractor, job, _) = await SetupTestData();
        var @event = new JobCancelledEvent(job.Id, contractor.Id, reason);

        var handler = new JobCancelledContractorEmailHandler(
            _mockEmailService.Object,
            _dbContext,
            _mockLoggerCancelled.Object,
            _mockConfiguration.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        capturedData.Should().NotBeNull();
        capturedData!.CancellationReason.Should().Be(reason);
    }

    #endregion

    #region JobScheduleChangedContractorEmailHandler Tests

    [Fact]
    public async Task ScheduleChangedHandler_SendsEmailToContractorWithCorrectTemplate()
    {
        // Arrange
        _mockEmailService.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<EmailTemplateDataDto>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(true);

        var (contractor, job, _) = await SetupTestData();
        var oldTime = DateTime.UtcNow;
        var newTime = oldTime.AddDays(1);
        var @event = new ScheduleUpdatedEvent(job.Id, contractor.Id, newTime, oldTime);

        var handler = new JobScheduleChangedContractorEmailHandler(
            _mockEmailService.Object,
            _dbContext,
            _mockLoggerScheduleChanged.Object,
            _mockConfiguration.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _mockEmailService.Verify(
            s => s.SendEmailAsync(
                contractor.User!.Email,
                It.IsAny<string>(),
                "JobScheduleChangedForContractor",
                It.IsAny<EmailTemplateDataDto>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task ScheduleChangedHandler_IncludesOldAndNewSchedule()
    {
        // Arrange
        EmailTemplateDataDto? capturedData = null;
        var oldTime = DateTime.UtcNow;
        var newTime = oldTime.AddDays(1);

        _mockEmailService.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<EmailTemplateDataDto>(),
            It.IsAny<CancellationToken>()
        )).Callback<string, string, string, EmailTemplateDataDto, CancellationToken>(
            (to, subject, template, data, ct) => capturedData = data
        ).ReturnsAsync(true);

        var (contractor, job, _) = await SetupTestData();
        var @event = new ScheduleUpdatedEvent(job.Id, contractor.Id, newTime, oldTime);

        var handler = new JobScheduleChangedContractorEmailHandler(
            _mockEmailService.Object,
            _dbContext,
            _mockLoggerScheduleChanged.Object,
            _mockConfiguration.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        capturedData.Should().NotBeNull();
        capturedData!.OldScheduledDateTime.Should().Be(oldTime);
        capturedData.NewScheduledDateTime.Should().Be(newTime);
    }

    #endregion

    #region RatingPostedContractorEmailHandler Tests

    [Fact]
    public async Task RatingPostedHandler_SendsEmailToContractorWithCorrectTemplate()
    {
        // Arrange
        _mockEmailService.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<EmailTemplateDataDto>(),
            It.IsAny<CancellationToken>()
        )).ReturnsAsync(true);

        var (contractor, job, customer) = await SetupTestData();
        var @event = new RatingPostedEvent(
            reviewId: 1,
            jobId: job.Id,
            contractorId: contractor.Id,
            customerId: customer.Id,
            rating: 5,
            comment: "Great work!"
        );

        var handler = new RatingPostedContractorEmailHandler(
            _mockEmailService.Object,
            _dbContext,
            _mockLoggerRatingPosted.Object,
            _mockConfiguration.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _mockEmailService.Verify(
            s => s.SendEmailAsync(
                contractor.User!.Email,
                It.IsAny<string>(),
                "RatingReceivedByContractor",
                It.IsAny<EmailTemplateDataDto>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task RatingPostedHandler_IncludesRatingAndComment()
    {
        // Arrange
        EmailTemplateDataDto? capturedData = null;
        var rating = 5;
        var comment = "Excellent work!";

        _mockEmailService.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<EmailTemplateDataDto>(),
            It.IsAny<CancellationToken>()
        )).Callback<string, string, string, EmailTemplateDataDto, CancellationToken>(
            (to, subject, template, data, ct) => capturedData = data
        ).ReturnsAsync(true);

        var (contractor, job, customer) = await SetupTestData();
        var @event = new RatingPostedEvent(
            reviewId: 1,
            jobId: job.Id,
            contractorId: contractor.Id,
            customerId: customer.Id,
            rating: rating,
            comment: comment
        );

        var handler = new RatingPostedContractorEmailHandler(
            _mockEmailService.Object,
            _dbContext,
            _mockLoggerRatingPosted.Object,
            _mockConfiguration.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        capturedData.Should().NotBeNull();
        capturedData!.Rating.Should().Be(rating);
        capturedData.ReviewComment.Should().Be(comment);
        capturedData.CustomerName.Should().Be(customer.Name);
    }

    [Fact]
    public async Task RatingPostedHandler_HandlesMissingComment()
    {
        // Arrange
        EmailTemplateDataDto? capturedData = null;

        _mockEmailService.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<EmailTemplateDataDto>(),
            It.IsAny<CancellationToken>()
        )).Callback<string, string, string, EmailTemplateDataDto, CancellationToken>(
            (to, subject, template, data, ct) => capturedData = data
        ).ReturnsAsync(true);

        var (contractor, job, customer) = await SetupTestData();
        var @event = new RatingPostedEvent(
            reviewId: 1,
            jobId: job.Id,
            contractorId: contractor.Id,
            customerId: customer.Id,
            rating: 4,
            comment: null
        );

        var handler = new RatingPostedContractorEmailHandler(
            _mockEmailService.Object,
            _dbContext,
            _mockLoggerRatingPosted.Object,
            _mockConfiguration.Object
        );

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        capturedData.Should().NotBeNull();
        capturedData!.ReviewComment.Should().Be(string.Empty);
    }

    #endregion

    #region Helper Methods

    private async Task<(Contractor, Job, Customer)> SetupTestData()
    {
        // Create user for contractor
        var contractorUser = new User
        {
            Email = "contractor@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTime.UtcNow
        };

        // Create user for customer
        var customerUser = new User
        {
            Email = "customer@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            CreatedAt = DateTime.UtcNow
        };

        // Create contractor
        var contractor = new Contractor
        {
            User = contractorUser,
            Name = "John Smith",
            PhoneNumber = "555-1234",
            AverageRating = 4.5m
        };

        // Create customer
        var customer = new Customer
        {
            User = customerUser,
            Name = "Jane Doe"
        };

        // Create job
        var job = new Job
        {
            CustomerId = customer.Id,
            JobType = JobType.Plumbing,
            Location = "123 Main St",
            Description = "Fix pipe leak",
            DesiredDateTime = DateTime.UtcNow.AddDays(1)
        };

        _dbContext.Users.Add(contractorUser);
        _dbContext.Users.Add(customerUser);
        _dbContext.Contractors.Add(contractor);
        _dbContext.Customers.Add(customer);
        _dbContext.Jobs.Add(job);

        await _dbContext.SaveChangesAsync();

        return (contractor, job, customer);
    }

    #endregion
}

