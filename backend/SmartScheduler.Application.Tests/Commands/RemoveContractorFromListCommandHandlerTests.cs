using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SmartScheduler.Application.Commands;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Infrastructure.Persistence;
using SmartScheduler.Infrastructure.Repositories;

namespace SmartScheduler.Application.Tests.Commands;

/// <summary>
/// Unit tests for RemoveContractorFromListCommandHandler.
/// </summary>
public class RemoveContractorFromListCommandHandlerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDispatcherContractorListRepository _repository;
    private readonly RemoveContractorFromListCommandHandler _handler;

    public RemoveContractorFromListCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new DispatcherContractorListRepository(_dbContext);
        _handler = new RemoveContractorFromListCommandHandler(_repository);
    }

    private void SeedTestData()
    {
        // Create test contractor
        var contractor = new Contractor
        {
            Id = 1,
            UserId = 2,
            Name = "John Plumber",
            PhoneNumber = "555-1234",
            Location = "Denver, CO",
            Latitude = 39.7392m,
            Longitude = -104.9903m,
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            ReviewCount = 5,
            AverageRating = 4.5m
        };

        // Add to dispatcher's list
        var dispatcherContractorList = new DispatcherContractorList
        {
            DispatcherId = 1,
            ContractorId = 1,
            AddedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor);
        _dbContext.DispatcherContractorLists.Add(dispatcherContractorList);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidContractorId_RemovesFromDatabase()
    {
        // Arrange
        SeedTestData();
        var command = new RemoveContractorFromListCommand(dispatcherId: 1, contractorId: 1);

        // Verify it exists before removal
        var existsBefore = await _repository.ExistsAsync(1, 1);
        existsBefore.Should().BeTrue();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var existsAfter = await _repository.ExistsAsync(1, 1);
        existsAfter.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNonExistentContractor_ReturnsSuccessIdempotent()
    {
        // Arrange
        SeedTestData();
        var command = new RemoveContractorFromListCommand(dispatcherId: 1, contractorId: 999); // Non-existent

        // Act - Should not throw exception
        var exception = await Record.ExceptionAsync(() => _handler.Handle(command, CancellationToken.None));

        // Assert - No exception thrown (idempotent)
        exception.Should().BeNull();
    }

    [Fact]
    public async Task Handle_RemoveFromEmptyList_ReturnsSuccessIdempotent()
    {
        // Arrange
        SeedTestData();
        var command = new RemoveContractorFromListCommand(dispatcherId: 1, contractorId: 1);

        // Act - Remove once
        await _handler.Handle(command, CancellationToken.None);

        // Act - Remove again from now-empty list
        var exception = await Record.ExceptionAsync(() => _handler.Handle(command, CancellationToken.None));

        // Assert - No exception thrown (idempotent)
        exception.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithMultipleContractors_RemovesOnlySpecifiedOne()
    {
        // Arrange
        SeedTestData();
        var contractor2 = new Contractor
        {
            Id = 2,
            UserId = 3,
            Name = "Jane Electrician",
            PhoneNumber = "555-5678",
            Location = "Boulder, CO",
            Latitude = 40.0150m,
            Longitude = -105.2705m,
            TradeType = TradeType.Electrical,
            WorkingHoursStart = TimeSpan.FromHours(8),
            WorkingHoursEnd = TimeSpan.FromHours(16),
            IsActive = true,
            ReviewCount = 3,
            AverageRating = 4.0m
        };
        var list2 = new DispatcherContractorList
        {
            DispatcherId = 1,
            ContractorId = 2,
            AddedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor2);
        _dbContext.DispatcherContractorLists.Add(list2);
        _dbContext.SaveChanges();

        var command = new RemoveContractorFromListCommand(dispatcherId: 1, contractorId: 1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exists1 = await _repository.ExistsAsync(1, 1);
        var exists2 = await _repository.ExistsAsync(1, 2);

        exists1.Should().BeFalse(); // Should be removed
        exists2.Should().BeTrue();  // Should still exist

        var count = await _dbContext.DispatcherContractorLists
            .CountAsync(dcl => dcl.DispatcherId == 1);
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNullCommand_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
    }
}

