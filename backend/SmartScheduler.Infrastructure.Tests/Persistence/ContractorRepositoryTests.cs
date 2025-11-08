using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Infrastructure.Persistence;
using SmartScheduler.Infrastructure.Repositories;

namespace SmartScheduler.Infrastructure.Tests.Persistence;

/// <summary>
/// Unit tests for ContractorRepository.
/// </summary>
public class ContractorRepositoryTests
{
    private readonly ContractorRepository _repository;
    private readonly ApplicationDbContext _dbContext;

    public ContractorRepositoryTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new ContractorRepository(_dbContext);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidContractor_AddsAndReturnsContractor()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "John Plumber",
            Location = "123 Main St",
            PhoneNumber = "555-123-4567",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(contractor);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("John Plumber");
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsContractor()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "John Plumber",
            Location = "123 Main St",
            PhoneNumber = "555-123-4567",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(contractor.Id);

        // Assert
        result.Should().NotBeNull();
        result?.Name.Should().Be("John Plumber");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsInactiveContractors()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Inactive Contractor",
            Location = "123 Main St",
            PhoneNumber = "555-123-4567",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(contractor.Id);

        // Assert
        result.Should().NotBeNull();
        result?.IsActive.Should().BeFalse();
    }

    #endregion

    #region GetAllActiveAsync Tests

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveContractors()
    {
        // Arrange
        var activeContractor = new Contractor
        {
            Name = "Active Contractor",
            Location = "123 Main St",
            PhoneNumber = "555-111-1111",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var inactiveContractor = new Contractor
        {
            Name = "Inactive Contractor",
            Location = "456 Oak Ave",
            PhoneNumber = "555-222-2222",
            TradeType = TradeType.HVAC,
            WorkingHoursStart = TimeSpan.FromHours(8),
            WorkingHoursEnd = TimeSpan.FromHours(16),
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.AddRange(activeContractor, inactiveContractor);
        await _dbContext.SaveChangesAsync();

        // Act
        var (contractors, totalCount) = await _repository.GetAllActiveAsync(1, 50);

        // Assert
        contractors.Should().HaveCount(1);
        contractors[0].Name.Should().Be("Active Contractor");
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllActiveAsync_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        for (int i = 1; i <= 75; i++)
        {
            var contractor = new Contractor
            {
                Name = $"Contractor {i:D3}",
                Location = $"{i} Main St",
                PhoneNumber = $"555-{i:D7}",
                TradeType = TradeType.Plumbing,
                WorkingHoursStart = TimeSpan.FromHours(9),
                WorkingHoursEnd = TimeSpan.FromHours(17),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.Contractors.Add(contractor);
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var (page1Contractors, page1Total) = await _repository.GetAllActiveAsync(1, 50);
        var (page2Contractors, page2Total) = await _repository.GetAllActiveAsync(2, 50);

        // Assert
        page1Contractors.Should().HaveCount(50);
        page1Total.Should().Be(75);
        
        page2Contractors.Should().HaveCount(25);
        page2Total.Should().Be(75);
    }

    [Fact]
    public async Task GetAllActiveAsync_OrdersByName()
    {
        // Arrange
        var contractor1 = new Contractor
        {
            Name = "Zebra Contractor",
            Location = "123 Main St",
            PhoneNumber = "555-111-1111",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var contractor2 = new Contractor
        {
            Name = "Apple Contractor",
            Location = "456 Oak Ave",
            PhoneNumber = "555-222-2222",
            TradeType = TradeType.HVAC,
            WorkingHoursStart = TimeSpan.FromHours(8),
            WorkingHoursEnd = TimeSpan.FromHours(16),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.AddRange(contractor1, contractor2);
        await _dbContext.SaveChangesAsync();

        // Act
        var (contractors, _) = await _repository.GetAllActiveAsync(1, 50);

        // Assert
        contractors[0].Name.Should().Be("Apple Contractor");
        contractors[1].Name.Should().Be("Zebra Contractor");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidContractor_UpdatesAndReturnsContractor()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Original Name",
            Location = "123 Main St",
            PhoneNumber = "555-123-4567",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor);
        await _dbContext.SaveChangesAsync();

        // Modify contractor
        contractor.Name = "Updated Name";

        // Act
        var result = await _repository.UpdateAsync(contractor);

        // Assert
        result.Name.Should().Be("Updated Name");
        
        // Verify persisted
        var updated = await _repository.GetByIdAsync(contractor.Id);
        updated?.Name.Should().Be("Updated Name");
    }

    #endregion

    #region DeactivateAsync Tests

    [Fact]
    public async Task DeactivateAsync_WithValidId_SetsIsActiveFalse()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Contractor",
            Location = "123 Main St",
            PhoneNumber = "555-123-4567",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor);
        await _dbContext.SaveChangesAsync();

        var contractorId = contractor.Id;

        // Act
        await _repository.DeactivateAsync(contractorId);

        // Assert
        var deactivated = await _repository.GetByIdAsync(contractorId);
        deactivated?.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateAsync_WithInvalidId_DoesNotThrow()
    {
        // Act & Assert (should not throw)
        await _repository.DeactivateAsync(999);
    }

    [Fact]
    public async Task DeactivateAsync_DoesNotRemoveFromDatabase()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Contractor",
            Location = "123 Main St",
            PhoneNumber = "555-123-4567",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor);
        await _dbContext.SaveChangesAsync();

        var contractorId = contractor.Id;

        // Act
        await _repository.DeactivateAsync(contractorId);

        // Assert - contractor still exists but is inactive
        var contractor2 = await _repository.GetByIdAsync(contractorId);
        contractor2.Should().NotBeNull();
        contractor2?.IsActive.Should().BeFalse();
    }

    #endregion

    #region ExistsByPhoneAsync Tests

    [Fact]
    public async Task ExistsByPhoneAsync_WithExistingPhone_ReturnsTrue()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Contractor",
            Location = "123 Main St",
            PhoneNumber = "555-123-4567",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor);
        await _dbContext.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsByPhoneAsync("555-123-4567");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByPhoneAsync_WithNonExistingPhone_ReturnsFalse()
    {
        // Act
        var exists = await _repository.ExistsByPhoneAsync("555-999-9999");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsByPhoneAsync_WithExcludeContractorId_IgnoresSpecificContractor()
    {
        // Arrange
        var contractor1 = new Contractor
        {
            Name = "Contractor 1",
            Location = "123 Main St",
            PhoneNumber = "555-123-4567",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor1);
        await _dbContext.SaveChangesAsync();

        // Act - Check if phone exists, but exclude contractor1
        var exists = await _repository.ExistsByPhoneAsync("555-123-4567", contractor1.Id);

        // Assert
        exists.Should().BeFalse();
    }

    #endregion
}

