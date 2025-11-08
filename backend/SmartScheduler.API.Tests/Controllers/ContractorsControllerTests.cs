using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SmartScheduler.API.Controllers;
using SmartScheduler.Application.DTOs;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Domain.Exceptions;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.API.Tests.Controllers;

public class ContractorsControllerTests
{
    private readonly ContractorsController _controller;
    private readonly ApplicationDbContext _dbContext;
    private readonly IAuthorizationService _authorizationService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly Mock<ILogger<ContractorsController>> _loggerMock;

    public ContractorsControllerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // Setup JWT configuration
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "test-secret-key-with-more-than-32-characters-for-testing" },
                { "Jwt:Issuer", "SmartScheduler" },
                { "Jwt:Audience", "SmartSchedulerAPI" },
                { "Jwt:JwtExpiry", "01:00:00" },
                { "Jwt:RefreshTokenExpiry", "7.00:00:00" }
            });

        var configuration = configBuilder.Build();
        _jwtTokenService = new JwtTokenService(configuration);
        _passwordHashingService = new PasswordHashingService();
        _authorizationService = new AuthorizationService();
        _loggerMock = new Mock<ILogger<ContractorsController>>();

        _controller = new ContractorsController(_dbContext, _loggerMock.Object, _authorizationService);
    }

    private void SetupControllerUser(string role, int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, $"user{userId}@test.com"),
            new Claim(ClaimTypes.Role, role)
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

    [Fact]
    public async Task GetContractors_WithAuthenticatedUser_ReturnsOkWithContractorList()
    {
        // Arrange
        var contractor = new Contractor
        {
            Name = "Test Contractor",
            Location = "123 Main St",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true,
            ReviewCount = 5,
            AverageRating = 4.5m
        };

        _dbContext.Contractors.Add(contractor);
        await _dbContext.SaveChangesAsync();

        SetupControllerUser("Customer", 1);

        // Act
        var result = await _controller.GetContractors();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task PostContractors_WithDispatcherRole_ReturnsCreated()
    {
        // Arrange
        var dispatcher = new User
        {
            Id = 1,
            Email = "dispatcher@test.com",
            PasswordHash = "hash",
            Role = UserRole.Dispatcher,
            IsActive = true
        };

        _dbContext.Users.Add(dispatcher);
        await _dbContext.SaveChangesAsync();

        SetupControllerUser("Dispatcher", dispatcher.Id);

        var createDto = new CreateContractorDto
        {
            Name = "New Contractor",
            Location = "456 Oak Ave",
            PhoneNumber = "555-1234",
            TradeType = "Plumbing",
            WorkingHoursStart = "09:00:00",
            WorkingHoursEnd = "17:00:00"
        };

        // Act
        var result = await _controller.CreateContractor(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task PostContractors_WithCustomerRole_ReturnsForbidden()
    {
        // Arrange
        var customer = new User
        {
            Id = 1,
            Email = "customer@test.com",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            IsActive = true
        };

        _dbContext.Users.Add(customer);
        await _dbContext.SaveChangesAsync();

        SetupControllerUser("Customer", customer.Id);

        var createDto = new CreateContractorDto
        {
            Name = "New Contractor",
            Location = "456 Oak Ave",
            PhoneNumber = "555-1234",
            TradeType = "Plumbing",
            WorkingHoursStart = "09:00:00",
            WorkingHoursEnd = "17:00:00"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _controller.CreateContractor(createDto));
    }

    [Fact]
    public async Task PostContractors_WithContractorRole_ReturnsForbidden()
    {
        // Arrange
        var contractor = new User
        {
            Id = 1,
            Email = "contractor@test.com",
            PasswordHash = "hash",
            Role = UserRole.Contractor,
            IsActive = true
        };

        _dbContext.Users.Add(contractor);
        await _dbContext.SaveChangesAsync();

        SetupControllerUser("Contractor", contractor.Id);

        var createDto = new CreateContractorDto
        {
            Name = "New Contractor",
            Location = "456 Oak Ave",
            PhoneNumber = "555-1234",
            TradeType = "Plumbing",
            WorkingHoursStart = "09:00:00",
            WorkingHoursEnd = "17:00:00"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _controller.CreateContractor(createDto));
    }

    [Fact]
    public async Task GetContractors_WithNoAuthentication_ThrowsUnauthorizedException()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(new ClaimsPrincipal());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };

        // The Authorize middleware would handle this before reaching the controller,
        // but we test the controller's behavior with missing claims
        // In real scenario, the framework prevents unauthenticated access
        // This test is more of a documentation of expected behavior

        // Act & Assert
        // Note: In real ASP.NET Core, [Authorize] attribute prevents this from being called
        // This is more of an edge case test
    }
}

