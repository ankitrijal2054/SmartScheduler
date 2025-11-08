using FluentAssertions;
using Moq;
using SmartScheduler.Application.Commands;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace SmartScheduler.Application.Tests.Commands;

public class PostReviewCommandHandlerTests
{
    private readonly Mock<IReviewRepository> _mockReviewRepository;
    private readonly Mock<IRatingAggregationService> _mockRatingAggregationService;
    private readonly Mock<IContractorRepository> _mockContractorRepository;
    private readonly Mock<ILogger<PostReviewCommandHandler>> _mockLogger;
    private readonly PostReviewCommandHandler _handler;

    public PostReviewCommandHandlerTests()
    {
        _mockReviewRepository = new Mock<IReviewRepository>();
        _mockRatingAggregationService = new Mock<IRatingAggregationService>();
        _mockContractorRepository = new Mock<IContractorRepository>();
        _mockLogger = new Mock<ILogger<PostReviewCommandHandler>>();

        _handler = new PostReviewCommandHandler(
            _mockReviewRepository.Object,
            _mockRatingAggregationService.Object,
            _mockContractorRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task PostReview_ValidInput_CallsAggregationService()
    {
        // Arrange
        var command = new PostReviewCommand
        {
            JobId = 1,
            ContractorId = 2,
            CustomerId = 3,
            Rating = 5,
            Comment = "Great work!"
        };

        var contractor = new Contractor { Id = 2 };
        var createdReview = new Review
        {
            Id = 1,
            JobId = command.JobId,
            ContractorId = command.ContractorId,
            CustomerId = command.CustomerId,
            Rating = command.Rating,
            Comment = command.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(command.ContractorId))
            .ReturnsAsync(contractor);

        _mockReviewRepository
            .Setup(x => x.ExistsAsync(command.JobId, command.CustomerId))
            .ReturnsAsync(false);

        _mockReviewRepository
            .Setup(x => x.AddAsync(It.IsAny<Review>()))
            .ReturnsAsync(createdReview);

        var updatedContractor = new Contractor { Id = 2, AverageRating = 5.0m, ReviewCount = 1 };
        _mockRatingAggregationService
            .Setup(x => x.UpdateContractorAverageRatingAsync(command.ContractorId))
            .ReturnsAsync(updatedContractor);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().Be(command.JobId);
        result.ContractorId.Should().Be(command.ContractorId);
        result.Rating.Should().Be(command.Rating);

        _mockRatingAggregationService.Verify(
            x => x.UpdateContractorAverageRatingAsync(command.ContractorId),
            Times.Once);
    }

    [Fact]
    public async Task PostReview_DuplicateReview_ThrowsConflictException()
    {
        // Arrange
        var command = new PostReviewCommand
        {
            JobId = 1,
            ContractorId = 2,
            CustomerId = 3,
            Rating = 5
        };

        var contractor = new Contractor { Id = 2 };

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(command.ContractorId))
            .ReturnsAsync(contractor);

        _mockReviewRepository
            .Setup(x => x.ExistsAsync(command.JobId, command.CustomerId))
            .ReturnsAsync(true); // Review already exists

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should()
            .ThrowAsync<ConflictException>()
            .WithMessage($"Review already exists for job {command.JobId} by customer {command.CustomerId}");

        _mockRatingAggregationService.Verify(
            x => x.UpdateContractorAverageRatingAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task PostReview_ContractorNotFound_ThrowsContractorNotFoundException()
    {
        // Arrange
        var command = new PostReviewCommand
        {
            JobId = 1,
            ContractorId = 999,
            CustomerId = 3,
            Rating = 5
        };

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(command.ContractorId))
            .ReturnsAsync((Contractor?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should()
            .ThrowAsync<ContractorNotFoundException>()
            .WithMessage($"Contractor with ID {command.ContractorId} not found");

        _mockReviewRepository.Verify(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _mockRatingAggregationService.Verify(
            x => x.UpdateContractorAverageRatingAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task PostReview_ValidInput_CreatesReviewWithCorrectData()
    {
        // Arrange
        var command = new PostReviewCommand
        {
            JobId = 1,
            ContractorId = 2,
            CustomerId = 3,
            Rating = 4,
            Comment = "Good work"
        };

        var contractor = new Contractor { Id = 2 };
        var createdReview = new Review
        {
            Id = 1,
            JobId = command.JobId,
            ContractorId = command.ContractorId,
            CustomerId = command.CustomerId,
            Rating = command.Rating,
            Comment = command.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(command.ContractorId))
            .ReturnsAsync(contractor);

        _mockReviewRepository
            .Setup(x => x.ExistsAsync(command.JobId, command.CustomerId))
            .ReturnsAsync(false);

        _mockReviewRepository
            .Setup(x => x.AddAsync(It.IsAny<Review>()))
            .ReturnsAsync(createdReview);

        _mockRatingAggregationService
            .Setup(x => x.UpdateContractorAverageRatingAsync(command.ContractorId))
            .ReturnsAsync(contractor);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().Be(1);
        result.ContractorId.Should().Be(2);
        result.CustomerId.Should().Be(3);
        result.Rating.Should().Be(4);
        result.Comment.Should().Be("Good work");

        _mockReviewRepository.Verify(
            x => x.AddAsync(It.Is<Review>(r =>
                r.JobId == command.JobId &&
                r.ContractorId == command.ContractorId &&
                r.CustomerId == command.CustomerId &&
                r.Rating == command.Rating &&
                r.Comment == command.Comment)),
            Times.Once);
    }

    [Fact]
    public async Task PostReview_AggregationServiceCalled_AfterReviewCreated()
    {
        // Arrange
        var command = new PostReviewCommand(jobId: 1, contractorId: 2, customerId: 3, rating: 5);
        var contractor = new Contractor { Id = 2 };
        var createdReview = new Review
        {
            Id = 1,
            JobId = 1,
            ContractorId = 2,
            CustomerId = 3,
            Rating = 5,
            CreatedAt = DateTime.UtcNow
        };

        var callOrder = new List<string>();

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(contractor);

        _mockReviewRepository
            .Setup(x => x.ExistsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        _mockReviewRepository
            .Setup(x => x.AddAsync(It.IsAny<Review>()))
            .Callback(() => callOrder.Add("ReviewAdded"))
            .ReturnsAsync(createdReview);

        _mockRatingAggregationService
            .Setup(x => x.UpdateContractorAverageRatingAsync(It.IsAny<int>()))
            .Callback(() => callOrder.Add("AggregationCalled"))
            .ReturnsAsync(contractor);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert - Review is added before aggregation is called
        callOrder.Should().ContainInOrder("ReviewAdded", "AggregationCalled");
    }
}

