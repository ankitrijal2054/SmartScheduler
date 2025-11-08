using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;
using SmartScheduler.Infrastructure.Persistence;
using SmartScheduler.Infrastructure.Repositories;

namespace SmartScheduler.Application.Tests.Services;

/// <summary>
/// Unit tests for ContractorService.
/// </summary>
public class ContractorServiceTests
{
    private readonly ContractorService _service;
    private readonly IContractorRepository _repository;
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IGeocodingService> _geocodingServiceMock;
    private readonly Mock<ILogger<ContractorService>> _loggerMock;

    public ContractorServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new ContractorRepository(_dbContext);
        _geocodingServiceMock = new Mock<IGeocodingService>();
        _geocodingServiceMock
            .Setup(g => g.GeocodeAddressAsync(It.IsAny<string>()))
            .ReturnsAsync((40.7128, -74.0060)); // Mock coordinates for NYC
        _loggerMock = new Mock<ILogger<ContractorService>>();
        _service = new ContractorService(_repository, _geocodingServiceMock.Object, _loggerMock.Object);
    }

    #region CreateContractorAsync Tests

    [Fact]
    public async Task CreateContractorAsync_WithValidRequest_ReturnsContractorResponse()
    {
        // Arrange
        var request = new CreateContractorRequest
        {
            Name = "John Plumber",
            Location = "123 Main St, Springfield, IL",
            Phone = "555-123-4567",
            TradeType = "Plumbing",
            WorkingHours = new CreateWorkingHoursRequest
            {
                StartTime = "09:00",
                EndTime = "17:00",
                WorkDays = new[] { "Mon", "Tue", "Wed", "Thu", "Fri" }
            }
        };

        // Act
        var result = await _service.CreateContractorAsync(request, 1);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("John Plumber");
        result.Location.Should().Be("123 Main St, Springfield, IL");
        result.TradeType.Should().Be("Plumbing");
        result.IsActive.Should().BeTrue();
        result.ReviewCount.Should().Be(0);
        result.AverageRating.Should().BeNull();
    }

    [Fact]
    public async Task CreateContractorAsync_WithDuplicatePhone_ThrowsValidationException()
    {
        // Arrange
        var existingContractor = new Contractor
        {
            Name = "Existing Contractor",
            Location = "456 Oak Ave",
            PhoneNumber = "555-123-4567",
            TradeType = TradeType.HVAC,
            WorkingHoursStart = TimeSpan.FromHours(8),
            WorkingHoursEnd = TimeSpan.FromHours(16),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(existingContractor);
        await _dbContext.SaveChangesAsync();

        var request = new CreateContractorRequest
        {
            Name = "New Contractor",
            Location = "789 Elm St",
            Phone = "555-123-4567", // Duplicate
            TradeType = "Plumbing",
            WorkingHours = new CreateWorkingHoursRequest
            {
                StartTime = "09:00",
                EndTime = "17:00",
                WorkDays = new[] { "Mon", "Tue", "Wed", "Thu", "Fri" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateContractorAsync(request, 1));
    }

    [Fact]
    public async Task CreateContractorAsync_WithInvalidTradeType_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateContractorRequest
        {
            Name = "John Plumber",
            Location = "123 Main St, Springfield, IL",
            Phone = "555-123-4567",
            TradeType = "InvalidTrade",
            WorkingHours = new CreateWorkingHoursRequest
            {
                StartTime = "09:00",
                EndTime = "17:00",
                WorkDays = new[] { "Mon", "Tue" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateContractorAsync(request, 1));
    }

    [Fact]
    public async Task CreateContractorAsync_WithInvalidWorkingHours_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateContractorRequest
        {
            Name = "John Plumber",
            Location = "123 Main St, Springfield, IL",
            Phone = "555-123-4567",
            TradeType = "Plumbing",
            WorkingHours = new CreateWorkingHoursRequest
            {
                StartTime = "17:00",
                EndTime = "09:00", // End before start
                WorkDays = new[] { "Mon", "Tue" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateContractorAsync(request, 1));
    }

    [Fact]
    public async Task CreateContractorAsync_WithMissingName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateContractorRequest
        {
            Name = "", // Empty
            Location = "123 Main St, Springfield, IL",
            Phone = "555-123-4567",
            TradeType = "Plumbing",
            WorkingHours = new CreateWorkingHoursRequest
            {
                StartTime = "09:00",
                EndTime = "17:00",
                WorkDays = new[] { "Mon", "Tue" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.CreateContractorAsync(request, 1));
    }

    #endregion

    #region GetContractorAsync Tests

    [Fact]
    public async Task GetContractorAsync_WithValidId_ReturnsContractorResponse()
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
            ReviewCount = 5,
            AverageRating = 4.5m,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(contractor);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetContractorAsync(contractor.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(contractor.Id);
        result.Name.Should().Be("John Plumber");
        result.ReviewCount.Should().Be(5);
        result.AverageRating.Should().Be(4.5m);
    }

    [Fact]
    public async Task GetContractorAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetContractorAsync(999));
    }

    #endregion

    #region GetAllContractorsAsync Tests

    [Fact]
    public async Task GetAllContractorsAsync_ReturnsOnlyActiveContractors()
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
        var result = await _service.GetAllContractorsAsync(1, 50);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Active Contractor");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllContractorsAsync_WithPagination_ReturnsPaginatedResults()
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
        var page1 = await _service.GetAllContractorsAsync(1, 50);
        var page2 = await _service.GetAllContractorsAsync(2, 50);

        // Assert
        page1.Items.Should().HaveCount(50);
        page1.PageNumber.Should().Be(1);
        page1.TotalCount.Should().Be(75);
        
        page2.Items.Should().HaveCount(25);
        page2.PageNumber.Should().Be(2);
        page2.TotalCount.Should().Be(75);
    }

    #endregion

    #region UpdateContractorAsync Tests

    [Fact]
    public async Task UpdateContractorAsync_WithValidRequest_UpdatesContractor()
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

        var updateRequest = new UpdateContractorRequest
        {
            Name = "John The Plumber"
        };

        // Act
        var result = await _service.UpdateContractorAsync(contractor.Id, updateRequest, 1);

        // Assert
        result.Name.Should().Be("John The Plumber");
    }

    [Fact]
    public async Task UpdateContractorAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var request = new UpdateContractorRequest { Name = "Updated Name" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateContractorAsync(999, request, 1));
    }

    [Fact]
    public async Task UpdateContractorAsync_WithDuplicatePhone_ThrowsValidationException()
    {
        // Arrange
        var contractor1 = new Contractor
        {
            Name = "Contractor 1",
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
            Name = "Contractor 2",
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

        var updateRequest = new UpdateContractorRequest
        {
            Phone = "555-111-1111" // Phone of contractor1
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpdateContractorAsync(contractor2.Id, updateRequest, 1));
    }

    #endregion

    #region DeactivateContractorAsync Tests

    [Fact]
    public async Task DeactivateContractorAsync_WithValidId_SetsIsActiveFalse()
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

        var contractorId = contractor.Id;

        // Act
        await _service.DeactivateContractorAsync(contractorId, 1);

        // Assert
        var deactivatedContractor = await _repository.GetByIdAsync(contractorId);
        deactivatedContractor?.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateContractorAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.DeactivateContractorAsync(999, 1));
    }

    [Fact]
    public async Task DeactivateContractorAsync_DeactivatedContractorNotInList()
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
        await _service.DeactivateContractorAsync(contractor.Id, 1);

        // Get all active contractors
        var result = await _service.GetAllContractorsAsync(1, 50);

        // Assert
        result.Items.Should().BeEmpty();
    }

    #endregion
}

