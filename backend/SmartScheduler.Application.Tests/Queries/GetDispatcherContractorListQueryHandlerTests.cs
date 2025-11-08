using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SmartScheduler.Application.Queries;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Infrastructure.Persistence;
using SmartScheduler.Infrastructure.Repositories;

namespace SmartScheduler.Application.Tests.Queries;

/// <summary>
/// Unit tests for GetDispatcherContractorListQueryHandler.
/// </summary>
public class GetDispatcherContractorListQueryHandlerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDispatcherContractorListRepository _repository;
    private readonly GetDispatcherContractorListQueryHandler _handler;

    public GetDispatcherContractorListQueryHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new DispatcherContractorListRepository(_dbContext);
        _handler = new GetDispatcherContractorListQueryHandler(_repository);
    }

    private void SeedMultipleContractors()
    {
        var contractors = new[]
        {
            new Contractor
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
            },
            new Contractor
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
            },
            new Contractor
            {
                Id = 3,
                UserId = 4,
                Name = "Bob HVAC",
                PhoneNumber = "555-3333",
                Location = "Fort Collins, CO",
                Latitude = 40.5853m,
                Longitude = -105.0844m,
                TradeType = TradeType.HVAC,
                WorkingHoursStart = TimeSpan.FromHours(7),
                WorkingHoursEnd = TimeSpan.FromHours(15),
                IsActive = true,
                ReviewCount = 10,
                AverageRating = 4.2m,
                TotalJobsCompleted = 67
            }
        };

        _dbContext.Contractors.AddRange(contractors);

        // Add contractors to dispatcher 1's list
        var lists = new[]
        {
            new DispatcherContractorList { DispatcherId = 1, ContractorId = 1, AddedAt = DateTime.UtcNow.AddDays(-2) },
            new DispatcherContractorList { DispatcherId = 1, ContractorId = 2, AddedAt = DateTime.UtcNow.AddDays(-1) },
            new DispatcherContractorList { DispatcherId = 1, ContractorId = 3, AddedAt = DateTime.UtcNow }
        };

        _dbContext.DispatcherContractorLists.AddRange(lists);

        // Add contractors to dispatcher 2's list (for isolation testing)
        _dbContext.DispatcherContractorLists.Add(
            new DispatcherContractorList { DispatcherId = 2, ContractorId = 1, AddedAt = DateTime.UtcNow }
        );

        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithMultipleContractors_ReturnsAllSortedByAddedAtDescending()
    {
        // Arrange
        SeedMultipleContractors();
        var query = new GetDispatcherContractorListQuery(dispatcherId: 1, page: 1, limit: 50);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Contractors.Should().HaveCount(3);
        result.Pagination.Total.Should().Be(3);
        result.Pagination.TotalPages.Should().Be(1);

        // Verify sorted by AddedAt descending (most recent first)
        result.Contractors[0].Id.Should().Be(3); // Bob HVAC (most recent)
        result.Contractors[1].Id.Should().Be(2); // Jane Electrician
        result.Contractors[2].Id.Should().Be(1); // John Plumber (oldest)
    }

    [Fact]
    public async Task Handle_WithSearchFilter_ReturnsFilteredResults()
    {
        // Arrange
        SeedMultipleContractors();
        var query = new GetDispatcherContractorListQuery(
            dispatcherId: 1,
            page: 1,
            limit: 50,
            search: "electrician");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Contractors.Should().HaveCount(1);
        result.Contractors[0].Name.Should().Be("Jane Electrician");
    }

    [Fact]
    public async Task Handle_WithSearchFilter_CaseInsensitive()
    {
        // Arrange
        SeedMultipleContractors();
        var query = new GetDispatcherContractorListQuery(
            dispatcherId: 1,
            page: 1,
            limit: 50,
            search: "PLUMBER");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Contractors.Should().HaveCount(1);
        result.Contractors[0].Name.Should().Be("John Plumber");
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        SeedMultipleContractors();
        var query = new GetDispatcherContractorListQuery(dispatcherId: 1, page: 2, limit: 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Contractors.Should().HaveCount(1);
        result.Pagination.Page.Should().Be(2);
        result.Pagination.Limit.Should().Be(1);
        result.Pagination.Total.Should().Be(3);
        result.Pagination.TotalPages.Should().Be(3);
        result.Contractors[0].Id.Should().Be(2); // Second in descending order
    }

    [Fact]
    public async Task Handle_WithDataIsolation_OnlyReturnsCurrentDispatcherContractors()
    {
        // Arrange
        SeedMultipleContractors();
        var query1 = new GetDispatcherContractorListQuery(dispatcherId: 1, page: 1, limit: 50);
        var query2 = new GetDispatcherContractorListQuery(dispatcherId: 2, page: 1, limit: 50);

        // Act
        var result1 = await _handler.Handle(query1, CancellationToken.None);
        var result2 = await _handler.Handle(query2, CancellationToken.None);

        // Assert
        result1.Contractors.Should().HaveCount(3); // Dispatcher 1 has 3
        result2.Contractors.Should().HaveCount(1); // Dispatcher 2 has only 1
        result2.Contractors[0].Id.Should().Be(1); // And it's contractor 1
    }

    [Fact]
    public async Task Handle_WithEmptyList_ReturnsEmptyContractorsList()
    {
        // Arrange
        SeedMultipleContractors();
        var query = new GetDispatcherContractorListQuery(dispatcherId: 99, page: 1, limit: 50); // Non-existent dispatcher

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Contractors.Should().BeEmpty();
        result.Pagination.Total.Should().Be(0);
        result.Pagination.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithNullCommand_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ReturnsContractorDetailsCorrectly()
    {
        // Arrange
        SeedMultipleContractors();
        var query = new GetDispatcherContractorListQuery(dispatcherId: 1, page: 1, limit: 50);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var contractor = result.Contractors.First(c => c.Name == "John Plumber");
        contractor.Id.Should().Be(1);
        contractor.PhoneNumber.Should().Be("555-1111");
        contractor.Location.Should().Be("Denver, CO");
        contractor.TradeType.Should().Be("Plumbing");
        contractor.AverageRating.Should().Be(4.8m);
        contractor.ReviewCount.Should().Be(5);
        contractor.TotalJobsCompleted.Should().Be(42);
        contractor.IsActive.Should().BeTrue();
        contractor.AddedAt.Should().NotBe(default);
    }
}

