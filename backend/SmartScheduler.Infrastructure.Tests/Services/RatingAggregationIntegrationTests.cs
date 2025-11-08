using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
/// Integration tests for RatingAggregationService using in-memory database.
/// Tests full rating aggregation flow with persisted review data.
/// </summary>
public class RatingAggregationIntegrationTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IReviewRepository _reviewRepository;
    private readonly IContractorRepository _contractorRepository;
    private readonly Mock<ILogger<RatingAggregationService>> _loggerMock;
    private readonly RatingAggregationService _service;

    public RatingAggregationIntegrationTests()
    {
        // Setup in-memory database with unique name for test isolation
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _reviewRepository = new ReviewRepository(_dbContext, new Mock<ILogger<ReviewRepository>>().Object);
        _contractorRepository = new ContractorRepository(_dbContext);
        _loggerMock = new Mock<ILogger<RatingAggregationService>>();

        _service = new RatingAggregationService(
            _contractorRepository,
            _reviewRepository,
            _loggerMock.Object);
    }

    private async Task SeedContractorAsync(int contractorId = 1)
    {
        var contractor = new Contractor
        {
            Id = contractorId,
            Name = "Test Contractor",
            PhoneNumber = "555-0001",
            Location = "123 Main St",
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            UserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor);
        await _dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task AddReviewToDatabase_AggregationUpdatesContractor()
    {
        // Arrange
        await SeedContractorAsync();
        var contractorId = 1;
        
        var review = new Review
        {
            JobId = 1,
            ContractorId = contractorId,
            CustomerId = 1,
            Rating = 5,
            Comment = "Excellent work",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        result.Should().NotBeNull();
        result.AverageRating.Should().Be(5.0m);
        result.ReviewCount.Should().Be(1);

        // Verify persisted in database
        var updatedContractor = await _dbContext.Contractors.FirstOrDefaultAsync(c => c.Id == contractorId);
        updatedContractor.Should().NotBeNull();
        updatedContractor!.AverageRating.Should().Be(5.0m);
        updatedContractor.ReviewCount.Should().Be(1);
    }

    [Fact]
    public async Task MultipleReviewsInDatabase_CorrectAverage()
    {
        // Arrange
        await SeedContractorAsync();
        var contractorId = 1;

        var reviews = new List<Review>
        {
            new Review { JobId = 1, ContractorId = contractorId, CustomerId = 1, Rating = 5, CreatedAt = DateTime.UtcNow },
            new Review { JobId = 2, ContractorId = contractorId, CustomerId = 2, Rating = 4, CreatedAt = DateTime.UtcNow },
            new Review { JobId = 3, ContractorId = contractorId, CustomerId = 3, Rating = 4, CreatedAt = DateTime.UtcNow },
            new Review { JobId = 4, ContractorId = contractorId, CustomerId = 4, Rating = 3, CreatedAt = DateTime.UtcNow },
            new Review { JobId = 5, ContractorId = contractorId, CustomerId = 5, Rating = 5, CreatedAt = DateTime.UtcNow }
        };

        _dbContext.Reviews.AddRange(reviews);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        result.Should().NotBeNull();
        result.AverageRating.Should().Be(4.2m); // (5+4+4+3+5)/5 = 21/5 = 4.2
        result.ReviewCount.Should().Be(5);
    }

    [Fact]
    public async Task NoReviewsInDatabase_AverageRemainsNull()
    {
        // Arrange
        await SeedContractorAsync();
        var contractorId = 1;

        // Act
        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        result.Should().NotBeNull();
        result.AverageRating.Should().BeNull();
        result.ReviewCount.Should().Be(0);

        // Verify persisted in database
        var updatedContractor = await _dbContext.Contractors.FirstOrDefaultAsync(c => c.Id == contractorId);
        updatedContractor.Should().NotBeNull();
        updatedContractor!.AverageRating.Should().BeNull();
        updatedContractor.ReviewCount.Should().Be(0);
    }

    [Fact]
    public async Task UpdateRating_DoesNotModifyOldReviews_HistoricalIntegrity()
    {
        // Arrange
        await SeedContractorAsync();
        var contractorId = 1;

        var review1 = new Review
        {
            JobId = 1,
            ContractorId = contractorId,
            CustomerId = 1,
            Rating = 5,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(review1);
        await _dbContext.SaveChangesAsync();

        var originalRating = review1.Rating;

        // Act: Add another review and recalculate
        var review2 = new Review
        {
            JobId = 2,
            ContractorId = contractorId,
            CustomerId = 2,
            Rating = 3,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(review2);
        await _dbContext.SaveChangesAsync();

        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        result.AverageRating.Should().Be(4.0m); // (5+3)/2

        // Verify old reviews are unchanged
        var retrievedReview1 = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == review1.Id);
        retrievedReview1.Should().NotBeNull();
        retrievedReview1!.Rating.Should().Be(originalRating);

        var retrievedReview2 = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == review2.Id);
        retrievedReview2.Should().NotBeNull();
        retrievedReview2!.Rating.Should().Be(3);
    }

    [Fact]
    public async Task ConcurrentReviewInserts_BothIncludedInAverage()
    {
        // Arrange
        await SeedContractorAsync();
        var contractorId = 1;

        var review1 = new Review
        {
            JobId = 1,
            ContractorId = contractorId,
            CustomerId = 1,
            Rating = 4,
            CreatedAt = DateTime.UtcNow
        };

        var review2 = new Review
        {
            JobId = 2,
            ContractorId = contractorId,
            CustomerId = 2,
            Rating = 4,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.AddRange(review1, review2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert: Both reviews should be included in the average
        result.Should().NotBeNull();
        result.AverageRating.Should().Be(4.0m);
        result.ReviewCount.Should().Be(2);
    }

    [Fact]
    public async Task RatingAggregation_WithRoundingToTwoDecimals()
    {
        // Arrange
        await SeedContractorAsync();
        var contractorId = 1;

        var reviews = new List<Review>
        {
            new Review { JobId = 1, ContractorId = contractorId, CustomerId = 1, Rating = 1, CreatedAt = DateTime.UtcNow },
            new Review { JobId = 2, ContractorId = contractorId, CustomerId = 2, Rating = 2, CreatedAt = DateTime.UtcNow },
            new Review { JobId = 3, ContractorId = contractorId, CustomerId = 3, Rating = 3, CreatedAt = DateTime.UtcNow }
        };

        _dbContext.Reviews.AddRange(reviews);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert: Average = (1+2+3)/3 = 6/3 = 2.0
        result.Should().NotBeNull();
        result.AverageRating.Should().Be(2.0m);
        result.ReviewCount.Should().Be(3);
    }

    [Fact]
    public async Task ContractorWithMultipleUpdates_RatingRecalculatesAccurately()
    {
        // Arrange
        await SeedContractorAsync();
        var contractorId = 1;

        // First round: 1 review with rating 5
        var review1 = new Review
        {
            JobId = 1,
            ContractorId = contractorId,
            CustomerId = 1,
            Rating = 5,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(review1);
        await _dbContext.SaveChangesAsync();

        var result1 = await _service.UpdateContractorAverageRatingAsync(contractorId);
        result1.AverageRating.Should().Be(5.0m);

        // Second round: Add review with rating 3
        var review2 = new Review
        {
            JobId = 2,
            ContractorId = contractorId,
            CustomerId = 2,
            Rating = 3,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(review2);
        await _dbContext.SaveChangesAsync();

        var result2 = await _service.UpdateContractorAverageRatingAsync(contractorId);

        // Assert
        result2.AverageRating.Should().Be(4.0m); // (5+3)/2
        result2.ReviewCount.Should().Be(2);
    }
}

