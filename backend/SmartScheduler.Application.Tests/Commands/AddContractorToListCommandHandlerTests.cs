using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using SmartScheduler.Application.Commands;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;
using SmartScheduler.Infrastructure.Persistence;
using SmartScheduler.Infrastructure.Repositories;

namespace SmartScheduler.Application.Tests.Commands;

/// <summary>
/// Unit tests for AddContractorToListCommandHandler.
/// </summary>
public class AddContractorToListCommandHandlerTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDispatcherContractorListRepository _repository;
    private readonly AddContractorToListCommandHandler _handler;

    public AddContractorToListCommandHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new DispatcherContractorListRepository(_dbContext);
        _handler = new AddContractorToListCommandHandler(_repository, _dbContext);
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

        _dbContext.Contractors.Add(contractor);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithValidContractorId_ReturnsDispatcherContractorListId()
    {
        // Arrange
        SeedTestData();
        var command = new AddContractorToListCommand(dispatcherId: 1, contractorId: 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        
        // Verify it was added to database
        var entry = await _dbContext.DispatcherContractorLists
            .FirstOrDefaultAsync(dcl => dcl.DispatcherId == 1 && dcl.ContractorId == 1);
        entry.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithDuplicateContractor_ReturnsExistingIdIdempotent()
    {
        // Arrange
        SeedTestData();
        var command = new AddContractorToListCommand(dispatcherId: 1, contractorId: 1);

        // Act - Add first time
        var result1 = await _handler.Handle(command, CancellationToken.None);

        // Act - Add same contractor again (idempotent)
        var result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert - Both should return same ID
        result1.Should().Be(result2);
        
        // Verify only one entry in database
        var count = await _dbContext.DispatcherContractorLists
            .CountAsync(dcl => dcl.DispatcherId == 1 && dcl.ContractorId == 1);
        count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNonExistentContractor_ThrowsNotFoundException()
    {
        // Arrange
        SeedTestData();
        var command = new AddContractorToListCommand(dispatcherId: 1, contractorId: 999); // Non-existent

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithMultipleContractors_AddsOnlySpecifiedOne()
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
        _dbContext.Contractors.Add(contractor2);
        _dbContext.SaveChanges();

        var command1 = new AddContractorToListCommand(dispatcherId: 1, contractorId: 1);
        var command2 = new AddContractorToListCommand(dispatcherId: 1, contractorId: 2);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.Should().BeGreaterThan(0);
        result2.Should().BeGreaterThan(0);
        result1.Should().NotBe(result2);

        var count = await _dbContext.DispatcherContractorLists
            .CountAsync(dcl => dcl.DispatcherId == 1);
        count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithNullCommand_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _handler.Handle(null!, CancellationToken.None));
    }
}

