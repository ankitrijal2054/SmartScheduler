using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Infrastructure.Persistence;
using SmartScheduler.Infrastructure.Repositories;

namespace SmartScheduler.Infrastructure.Tests.Persistence;

/// <summary>
/// Integration tests for DispatcherContractorListRepository.
/// </summary>
public class DispatcherContractorListRepositoryTests
{
    private readonly DispatcherContractorListRepository _repository;
    private readonly ApplicationDbContext _dbContext;

    public DispatcherContractorListRepositoryTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new DispatcherContractorListRepository(_dbContext);
    }

    private void SeedTestData()
    {
        var contractor1 = new Contractor
        {
            Id = 1,
            UserId = 2,
            Name = "John Plumber",
            PhoneNumber = "555-1111",
            Location = "Denver, CO",
            Latitude = 39.7392m,
            Longitude = -104.9903m,
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            ReviewCount = 5,
            AverageRating = 4.8m,
            TotalJobsCompleted = 42
        };

        var contractor2 = new Contractor
        {
            Id = 2,
            UserId = 3,
            Name = "Jane Electrician",
            PhoneNumber = "555-2222",
            Location = "Boulder, CO",
            Latitude = 40.0150m,
            Longitude = -105.2705m,
            TradeType = TradeType.Electrical,
            WorkingHoursStart = TimeSpan.FromHours(8),
            WorkingHoursEnd = TimeSpan.FromHours(16),
            IsActive = true,
            ReviewCount = 3,
            AverageRating = 4.5m,
            TotalJobsCompleted = 35
        };

        _dbContext.Contractors.AddRange(contractor1, contractor2);
        _dbContext.SaveChanges();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidContractorId_AddsToDatabase()
    {
        // Arrange
        SeedTestData();

        // Act
        var result = await _repository.AddAsync(dispatcherId: 1, contractorId: 1);

        // Assert
        result.Should().NotBeNull();
        result.DispatcherId.Should().Be(1);
        result.ContractorId.Should().Be(1);
        result.Id.Should().BeGreaterThan(0);

        // Verify in database
        var entry = await _dbContext.DispatcherContractorLists
            .FirstOrDefaultAsync(dcl => dcl.DispatcherId == 1 && dcl.ContractorId == 1);
        entry.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_WithDuplicateContractor_ReturnsExistingIdempotent()
    {
        // Arrange
        SeedTestData();

        // Act - Add first time
        var result1 = await _repository.AddAsync(dispatcherId: 1, contractorId: 1);

        // Act - Add same contractor again
        var result2 = await _repository.AddAsync(dispatcherId: 1, contractorId: 1);

        // Assert - Same ID returned
        result1.Id.Should().Be(result2.Id);

        // Verify only one entry in database
        var count = await _dbContext.DispatcherContractorLists
            .CountAsync(dcl => dcl.DispatcherId == 1 && dcl.ContractorId == 1);
        count.Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_WithMultipleContractors_AddsAllCorrectly()
    {
        // Arrange
        SeedTestData();

        // Act
        var result1 = await _repository.AddAsync(dispatcherId: 1, contractorId: 1);
        var result2 = await _repository.AddAsync(dispatcherId: 1, contractorId: 2);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Id.Should().NotBe(result2.Id);

        var count = await _dbContext.DispatcherContractorLists
            .CountAsync(dcl => dcl.DispatcherId == 1);
        count.Should().Be(2);
    }

    #endregion

    #region RemoveAsync Tests

    [Fact]
    public async Task RemoveAsync_WithValidContractorId_RemovesFromDatabase()
    {
        // Arrange
        SeedTestData();
        await _repository.AddAsync(dispatcherId: 1, contractorId: 1);

        // Verify exists before removal
        var existsBefore = await _repository.ExistsAsync(1, 1);
        existsBefore.Should().BeTrue();

        // Act
        await _repository.RemoveAsync(dispatcherId: 1, contractorId: 1);

        // Assert
        var existsAfter = await _repository.ExistsAsync(1, 1);
        existsAfter.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveAsync_WithNonExistentContractor_ReturnsSuccessIdempotent()
    {
        // Arrange
        SeedTestData();

        // Act - Should not throw exception
        var exception = await Record.ExceptionAsync(() =>
            _repository.RemoveAsync(dispatcherId: 1, contractorId: 999));

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_RemovesOnlySpecifiedContractor()
    {
        // Arrange
        SeedTestData();
        await _repository.AddAsync(dispatcherId: 1, contractorId: 1);
        await _repository.AddAsync(dispatcherId: 1, contractorId: 2);

        // Act
        await _repository.RemoveAsync(dispatcherId: 1, contractorId: 1);

        // Assert
        var exists1 = await _repository.ExistsAsync(1, 1);
        var exists2 = await _repository.ExistsAsync(1, 2);

        exists1.Should().BeFalse();
        exists2.Should().BeTrue();
    }

    #endregion

    #region GetByDispatcherIdAsync Tests

    [Fact]
    public async Task GetByDispatcherIdAsync_WithMultipleContractors_ReturnsSortedByAddedAt()
    {
        // Arrange
        SeedTestData();

        // Add with delays to test ordering
        await _repository.AddAsync(dispatcherId: 1, contractorId: 1);
        await Task.Delay(10);
        await _repository.AddAsync(dispatcherId: 1, contractorId: 2);

        // Act
        var result = await _repository.GetByDispatcherIdAsync(dispatcherId: 1, page: 1, limit: 50);

        // Assert
        result.Should().HaveCount(2);
        var resultList = result.ToList();
        resultList[0].ContractorId.Should().Be(2); // Most recent first
        resultList[1].ContractorId.Should().Be(1); // Oldest last
    }

    [Fact]
    public async Task GetByDispatcherIdAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        SeedTestData();
        await _repository.AddAsync(dispatcherId: 1, contractorId: 1);
        await _repository.AddAsync(dispatcherId: 1, contractorId: 2);

        // Act - Get page 1 with limit 1
        var page1 = await _repository.GetByDispatcherIdAsync(dispatcherId: 1, page: 1, limit: 1);

        // Act - Get page 2 with limit 1
        var page2 = await _repository.GetByDispatcherIdAsync(dispatcherId: 1, page: 2, limit: 1);

        // Assert
        page1.Should().HaveCount(1);
        page2.Should().HaveCount(1);
        page1.First().ContractorId.Should().NotBe(page2.First().ContractorId);
    }

    [Fact]
    public async Task GetByDispatcherIdAsync_WithDataIsolation_OnlyReturnsCurrentDispatcherContractors()
    {
        // Arrange
        SeedTestData();
        await _repository.AddAsync(dispatcherId: 1, contractorId: 1);
        await _repository.AddAsync(dispatcherId: 2, contractorId: 2);

        // Act
        var dispatcher1List = await _repository.GetByDispatcherIdAsync(dispatcherId: 1, page: 1, limit: 50);
        var dispatcher2List = await _repository.GetByDispatcherIdAsync(dispatcherId: 2, page: 1, limit: 50);

        // Assert
        dispatcher1List.Should().HaveCount(1);
        dispatcher1List.First().ContractorId.Should().Be(1);

        dispatcher2List.Should().HaveCount(1);
        dispatcher2List.First().ContractorId.Should().Be(2);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithExistingEntry_ReturnsTrue()
    {
        // Arrange
        SeedTestData();
        await _repository.AddAsync(dispatcherId: 1, contractorId: 1);

        // Act
        var exists = await _repository.ExistsAsync(dispatcherId: 1, contractorId: 1);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentEntry_ReturnsFalse()
    {
        // Arrange
        SeedTestData();

        // Act
        var exists = await _repository.ExistsAsync(dispatcherId: 1, contractorId: 1);

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region CountByDispatcherIdAsync Tests

    [Fact]
    public async Task CountByDispatcherIdAsync_WithMultipleContractors_ReturnsCorrectCount()
    {
        // Arrange
        SeedTestData();
        await _repository.AddAsync(dispatcherId: 1, contractorId: 1);
        await _repository.AddAsync(dispatcherId: 1, contractorId: 2);

        // Act
        var count = await _repository.CountByDispatcherIdAsync(dispatcherId: 1);

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task CountByDispatcherIdAsync_WithNoContractors_ReturnsZero()
    {
        // Arrange
        SeedTestData();

        // Act
        var count = await _repository.CountByDispatcherIdAsync(dispatcherId: 1);

        // Assert
        count.Should().Be(0);
    }

    #endregion
}

