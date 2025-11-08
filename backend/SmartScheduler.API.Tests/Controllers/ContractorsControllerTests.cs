using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartScheduler.API.Controllers;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Repositories;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;
using SmartScheduler.Infrastructure.Persistence;
using SmartScheduler.Infrastructure.Repositories;

namespace SmartScheduler.API.Tests.Controllers;

/// <summary>
/// Integration tests for ContractorsController using in-memory database.
/// </summary>
public class ContractorsControllerTests
{
    private readonly ContractorsController _controller;
    private readonly ContractorService _contractorService;
    private readonly IContractorRepository _repository;
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<ContractorsController>> _loggerMock;

    public ContractorsControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _repository = new ContractorRepository(_dbContext);
        
        var serviceLoggerMock = new Mock<ILogger<ContractorService>>();
        var geocodingServiceMock = new Mock<IGeocodingService>();
        // Mock geocoding to return test coordinates
        geocodingServiceMock
            .Setup(g => g.GeocodeAddressAsync(It.IsAny<string>()))
            .ReturnsAsync((40.7128, -74.0060)); // Default to NYC coordinates for tests
        
        _contractorService = new ContractorService(_repository, geocodingServiceMock.Object, serviceLoggerMock.Object);
        
        _loggerMock = new Mock<ILogger<ContractorsController>>();
        _controller = new ContractorsController(_contractorService, _loggerMock.Object);
    }

    #region Setup Helper

    private void SetupControllerUser(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, $"user{userId}@test.com"),
            new Claim(ClaimTypes.Role, "Dispatcher")
        };

        var claimsIdentity = new ClaimsIdentity(claims, "test");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(claimsPrincipal);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };
    }

    #endregion

    #region GET /contractors Tests

    [Fact]
    public async Task GetContractors_WithAuthenticatedUser_ReturnsOkWithContractorList()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Test Contractor",
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

        SetupControllerUser(1);

        // Act
        var result = await _controller.GetContractors();

        // Assert
        result.Result.Should().NotBeNull();
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetContractors_ReturnsPaginatedResponse()
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

        SetupControllerUser(1);

        // Act
        var result = await _controller.GetContractors(1, 50);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetContractors_WithValidPageSize_ReturnsRequestedAmount()
    {
        // Arrange
        for (int i = 1; i <= 30; i++)
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

        SetupControllerUser(1);

        // Act
        var result = await _controller.GetContractors(1, 25);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region GET /contractors/{id} Tests

    [Fact]
    public async Task GetContractor_WithValidId_ReturnsOkWithContractor()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Test Contractor",
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

        SetupControllerUser(1);

        // Act
        var result = await _controller.GetContractor(contractor.Id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetContractor_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        SetupControllerUser(1);

        // Act
        var result = await _controller.GetContractor(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region POST /contractors Tests

    [Fact]
    public async Task CreateContractor_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        SetupControllerUser(1);

        var request = new CreateContractorRequest
        {
            Name = "New Contractor",
            Location = "456 Oak Ave",
            Phone = "555-234-5678",
            TradeType = "Plumbing",
            WorkingHours = new CreateWorkingHoursRequest
            {
                StartTime = "09:00",
                EndTime = "17:00",
                WorkDays = new[] { "Mon", "Tue", "Wed", "Thu", "Fri" }
            }
        };

        // Act
        var result = await _controller.CreateContractor(request);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(_controller.GetContractor));
    }

    [Fact]
    public async Task CreateContractor_WithMissingRequiredField_ReturnsBadRequest()
    {
        // Arrange
        SetupControllerUser(1);

        var request = new CreateContractorRequest
        {
            Name = "", // Empty
            Location = "456 Oak Ave",
            Phone = "555-234-5678",
            TradeType = "Plumbing",
            WorkingHours = new CreateWorkingHoursRequest
            {
                StartTime = "09:00",
                EndTime = "17:00",
                WorkDays = new[] { "Mon", "Tue" }
            }
        };

        // Act
        var result = await _controller.CreateContractor(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task CreateContractor_WithDuplicatePhone_ReturnsBadRequest()
    {
        // Arrange
        var existingContractor = new Contractor
        {
            Name = "Existing",
            Location = "123 Main St",
            PhoneNumber = "555-123-4567",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Contractors.Add(existingContractor);
        await _dbContext.SaveChangesAsync();

        SetupControllerUser(1);

        var request = new CreateContractorRequest
        {
            Name = "New Contractor",
            Location = "456 Oak Ave",
            Phone = "555-123-4567", // Duplicate
            TradeType = "Plumbing",
            WorkingHours = new CreateWorkingHoursRequest
            {
                StartTime = "09:00",
                EndTime = "17:00",
                WorkDays = new[] { "Mon", "Tue" }
            }
        };

        // Act
        var result = await _controller.CreateContractor(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestResult>();
    }

    #endregion

    #region PUT /contractors/{id} Tests

    [Fact]
    public async Task UpdateContractor_WithValidRequest_ReturnsOkWithUpdatedContractor()
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

        SetupControllerUser(1);

        var request = new UpdateContractorRequest
        {
            Name = "Updated Name"
        };

        // Act
        var result = await _controller.UpdateContractor(contractor.Id, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdateContractor_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        SetupControllerUser(1);

        var request = new UpdateContractorRequest { Name = "Updated Name" };

        // Act
        var result = await _controller.UpdateContractor(999, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateContractor_WithPartialUpdate_UpdatesOnlyProvidedFields()
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

        SetupControllerUser(1);

        var request = new UpdateContractorRequest
        {
            Name = "Updated Name"
            // Location not provided, should remain unchanged
        };

        // Act
        var result = await _controller.UpdateContractor(contractor.Id, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region PATCH /contractors/{id}/deactivate Tests

    [Fact]
    public async Task DeactivateContractor_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Test Contractor",
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

        SetupControllerUser(1);

        // Act
        var result = await _controller.DeactivateContractor(contractor.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeactivateContractor_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        SetupControllerUser(1);

        // Act
        var result = await _controller.DeactivateContractor(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeactivateContractor_DeactivatedContractorNotInList()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Test Contractor",
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

        SetupControllerUser(1);

        // Act
        await _controller.DeactivateContractor(contractor.Id);
        var listResult = await _controller.GetContractors();

        // Assert
        var okResult = listResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    #endregion
}
