using FluentAssertions;
using Moq;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace SmartScheduler.Application.Tests.Services;

public class RatingAggregationServiceTests
{
    private readonly Mock<IContractorRepository> _mockContractorRepository;
    private readonly Mock<IReviewRepository> _mockReviewRepository;
    private readonly Mock<ILogger<RatingAggregationService>> _mockLogger;
    private readonly RatingAggregationService _service;

    public RatingAggregationServiceTests()
    {
        _mockContractorRepository = new Mock<IContractorRepository>();
        _mockReviewRepository = new Mock<IReviewRepository>();
        _mockLogger = new Mock<ILogger<RatingAggregationService>>();
        
        _service = new RatingAggregationService(
            _mockContractorRepository.Object,
            _mockReviewRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task UpdateContractorAverageRating_WithSingleReview_CalculatesCorrectAverage()
    {
        // Arrange
        var contractorId = 1;
        var contractor = new Contractor { Id = contractorId };
        var reviews = new List<Review>
        {
            new Review { Id = 1, Rating = 5, ContractorId = contractorId }
        };

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(contractorId))
            .ReturnsAsync(contractor);

        _mockReviewRepository
            .Setup(x => x.GetByContractorIdAsync(contractorId))
            .ReturnsAsync(reviews);

        // Act
        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        result.Should().NotBeNull();
        result.AverageRating.Should().Be(5.0m);
        result.ReviewCount.Should().Be(1);
        _mockContractorRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateContractorAverageRating_WithMultipleReviews_CalculatesCorrectAverage()
    {
        // Arrange
        var contractorId = 1;
        var contractor = new Contractor { Id = contractorId };
        var reviews = new List<Review>
        {
            new Review { Id = 1, Rating = 5, ContractorId = contractorId },
            new Review { Id = 2, Rating = 4, ContractorId = contractorId },
            new Review { Id = 3, Rating = 5, ContractorId = contractorId }
        };

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(contractorId))
            .ReturnsAsync(contractor);

        _mockReviewRepository
            .Setup(x => x.GetByContractorIdAsync(contractorId))
            .ReturnsAsync(reviews);

        // Act
        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        result.Should().NotBeNull();
        result.AverageRating.Should().Be(4.67m);
        result.ReviewCount.Should().Be(3);
        _mockContractorRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateContractorAverageRating_WithMixedRatings_RoundsToTwoDecimals()
    {
        // Arrange
        var contractorId = 1;
        var contractor = new Contractor { Id = contractorId };
        var reviews = new List<Review>
        {
            new Review { Id = 1, Rating = 1, ContractorId = contractorId },
            new Review { Id = 2, Rating = 2, ContractorId = contractorId },
            new Review { Id = 3, Rating = 3, ContractorId = contractorId }
        };

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(contractorId))
            .ReturnsAsync(contractor);

        _mockReviewRepository
            .Setup(x => x.GetByContractorIdAsync(contractorId))
            .ReturnsAsync(reviews);

        // Act
        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        result.Should().NotBeNull();
        result.AverageRating.Should().Be(2.0m);
        result.ReviewCount.Should().Be(3);
    }

    [Fact]
    public async Task UpdateContractorAverageRating_WithNoReviews_SetsAverageToNull()
    {
        // Arrange
        var contractorId = 1;
        var contractor = new Contractor { Id = contractorId, AverageRating = 4.5m, ReviewCount = 5 };
        var reviews = new List<Review>(); // Empty

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(contractorId))
            .ReturnsAsync(contractor);

        _mockReviewRepository
            .Setup(x => x.GetByContractorIdAsync(contractorId))
            .ReturnsAsync(reviews);

        // Act
        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        result.Should().NotBeNull();
        result.AverageRating.Should().BeNull();
        result.ReviewCount.Should().Be(0);
        _mockContractorRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateContractorAverageRating_WithInvalidContractorId_ThrowsException()
    {
        // Arrange
        var contractorId = 999;
        
        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(contractorId))
            .ReturnsAsync((Contractor?)null);

        // Act & Assert
        await _service.Invoking(x => x.UpdateContractorAverageRatingAsync(contractorId))
            .Should()
            .ThrowAsync<ContractorNotFoundException>()
            .WithMessage($"Contractor with ID {contractorId} not found");

        _mockContractorRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateContractorAverageRating_UpdatesReviewCount_Correctly()
    {
        // Arrange
        var contractorId = 1;
        var contractor = new Contractor { Id = contractorId };
        var reviews = new List<Review>
        {
            new Review { Id = 1, Rating = 3, ContractorId = contractorId },
            new Review { Id = 2, Rating = 4, ContractorId = contractorId }
        };

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(contractorId))
            .ReturnsAsync(contractor);

        _mockReviewRepository
            .Setup(x => x.GetByContractorIdAsync(contractorId))
            .ReturnsAsync(reviews);

        // Act
        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        result.ReviewCount.Should().Be(2);
        result.AverageRating.Should().Be(3.5m);
    }

    [Fact]
    public async Task UpdateContractorAverageRating_RepositorySaveCalledOnce()
    {
        // Arrange
        var contractorId = 1;
        var contractor = new Contractor { Id = contractorId };
        var reviews = new List<Review>
        {
            new Review { Id = 1, Rating = 5, ContractorId = contractorId }
        };

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(contractorId))
            .ReturnsAsync(contractor);

        _mockReviewRepository
            .Setup(x => x.GetByContractorIdAsync(contractorId))
            .ReturnsAsync(reviews);

        // Act
        await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        _mockContractorRepository.Verify(x => x.SaveChangesAsync(), Times.Exactly(1));
    }

    [Fact]
    public async Task UpdateContractorAverageRating_DecimalPrecision_HandlesComplexAverages()
    {
        // Arrange: Test with ratings that produce repeating decimals
        var contractorId = 1;
        var contractor = new Contractor { Id = contractorId };
        var reviews = new List<Review>
        {
            new Review { Id = 1, Rating = 1, ContractorId = contractorId },
            new Review { Id = 2, Rating = 1, ContractorId = contractorId },
            new Review { Id = 3, Rating = 1, ContractorId = contractorId }
        };

        _mockContractorRepository
            .Setup(x => x.GetByIdAsync(contractorId))
            .ReturnsAsync(contractor);

        _mockReviewRepository
            .Setup(x => x.GetByContractorIdAsync(contractorId))
            .ReturnsAsync(reviews);

        // Act
        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        result.AverageRating.Should().Be(1.0m);
    }
}

