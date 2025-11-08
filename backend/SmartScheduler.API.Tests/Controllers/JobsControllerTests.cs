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

public class JobsControllerTests
{
    private readonly JobsController _controller;
    private readonly ApplicationDbContext _dbContext;
    private readonly IAuthorizationService _authorizationService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly Mock<ILogger<JobsController>> _loggerMock;

    public JobsControllerTests()
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
        _loggerMock = new Mock<ILogger<JobsController>>();

        _controller = new JobsController(_dbContext, _loggerMock.Object, _authorizationService);
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

    private void SeedTestData()
    {
        // Create users
        var dispatcherUser = new User
        {
            Id = 1,
            Email = "dispatcher@test.com",
            PasswordHash = "hash",
            Role = UserRole.Dispatcher,
            IsActive = true
        };

        var customerUser = new User
        {
            Id = 2,
            Email = "customer@test.com",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            IsActive = true
        };

        var contractorUser = new User
        {
            Id = 3,
            Email = "contractor@test.com",
            PasswordHash = "hash",
            Role = UserRole.Contractor,
            IsActive = true
        };

        _dbContext.Users.AddRange(dispatcherUser, customerUser, contractorUser);
        _dbContext.SaveChanges();

        // Create customer profile
        var customer = new Customer
        {
            UserId = customerUser.Id,
            Name = "Test Customer",
            Location = "123 Main St",
            PhoneNumber = "555-0001"
        };

        _dbContext.Customers.Add(customer);
        _dbContext.SaveChanges();

        // Create contractor profile
        var contractor = new Contractor
        {
            UserId = contractorUser.Id,
            Name = "Test Contractor",
            Location = "456 Oak Ave",
            PhoneNumber = "555-0002",
            TradeType = TradeType.Plumbing,
            WorkingHoursStart = TimeSpan.FromHours(9),
            WorkingHoursEnd = TimeSpan.FromHours(17),
            IsActive = true
        };

        _dbContext.Contractors.Add(contractor);
        _dbContext.SaveChanges();

        // Create jobs
        var job1 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Plumbing,
            Location = "789 Main St",
            DesiredDateTime = DateTime.UtcNow.AddDays(1),
            Description = "Fix leaking pipe",
            Status = JobStatus.Pending,
            Latitude = 0,
            Longitude = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var job2 = new Job
        {
            CustomerId = customer.Id,
            JobType = TradeType.Electrical,
            Location = "321 Oak Ave",
            DesiredDateTime = DateTime.UtcNow.AddDays(2),
            Description = "Install new outlet",
            Status = JobStatus.Pending,
            Latitude = 0,
            Longitude = 0,
            AssignedContractorId = contractor.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Jobs.AddRange(job1, job2);
        _dbContext.SaveChanges();

        // Create assignment for job2
        var assignment = new Assignment
        {
            JobId = job2.Id,
            ContractorId = contractor.Id,
            Status = AssignmentStatus.Pending,
            AssignedAt = DateTime.UtcNow
        };

        _dbContext.Assignments.Add(assignment);
        _dbContext.SaveChanges();

        job2.Assignment = assignment;
    }

    [Fact]
    public async Task GetJobs_WithDispatcherRole_ReturnsAllJobs()
    {
        // Arrange
        SeedTestData();
        SetupControllerUser("Dispatcher", 1);

        // Act
        var result = await _controller.GetJobs();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetJobs_WithCustomerRole_ReturnsOnlyOwnJobs()
    {
        // Arrange
        SeedTestData();
        SetupControllerUser("Customer", 2); // Customer user ID

        // Act
        var result = await _controller.GetJobs();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetJobs_WithContractorRole_ReturnsOnlyAssignedJobs()
    {
        // Arrange
        SeedTestData();
        SetupControllerUser("Contractor", 3); // Contractor user ID

        // Act
        var result = await _controller.GetJobs();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CreateJob_WithCustomerRole_ReturnsCreated()
    {
        // Arrange
        SeedTestData();
        SetupControllerUser("Customer", 2);

        var createDto = new CreateJobDto
        {
            JobType = "Plumbing",
            Location = "999 New St",
            DesiredDateTime = DateTime.UtcNow.AddDays(3),
            Description = "Fix toilet",
            EstimatedDurationHours = 2
        };

        // Act
        var result = await _controller.CreateJob(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateJob_WithDispatcherRole_ReturnsForbidden()
    {
        // Arrange
        SeedTestData();
        SetupControllerUser("Dispatcher", 1);

        var createDto = new CreateJobDto
        {
            JobType = "Plumbing",
            Location = "999 New St",
            DesiredDateTime = DateTime.UtcNow.AddDays(3),
            Description = "Fix toilet",
            EstimatedDurationHours = 2
        };

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _controller.CreateJob(createDto));
    }

    [Fact]
    public async Task CreateJob_WithContractorRole_ReturnsForbidden()
    {
        // Arrange
        SeedTestData();
        SetupControllerUser("Contractor", 3);

        var createDto = new CreateJobDto
        {
            JobType = "Plumbing",
            Location = "999 New St",
            DesiredDateTime = DateTime.UtcNow.AddDays(3),
            Description = "Fix toilet",
            EstimatedDurationHours = 2
        };

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _controller.CreateJob(createDto));
    }

    [Fact]
    public async Task GetJobById_WithCustomerOwner_ReturnsOk()
    {
        // Arrange
        SeedTestData();
        var job = await _dbContext.Jobs.FirstAsync();
        SetupControllerUser("Customer", 2);

        // Act
        var result = await _controller.GetJobById(job.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetJobById_WithUnauthorizedCustomer_ReturnsForbidden()
    {
        // Arrange
        SeedTestData();

        // Create another customer
        var otherCustomerUser = new User
        {
            Id = 99,
            Email = "othercustomer@test.com",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            IsActive = true
        };

        _dbContext.Users.Add(otherCustomerUser);

        var otherCustomer = new Customer
        {
            UserId = otherCustomerUser.Id,
            Name = "Other Customer",
            Location = "999 Other St",
            PhoneNumber = "555-9999"
        };

        _dbContext.Customers.Add(otherCustomer);
        await _dbContext.SaveChangesAsync();

        var job = await _dbContext.Jobs.FirstAsync();
        SetupControllerUser("Customer", 99); // Different customer

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _controller.GetJobById(job.Id));
    }

    [Fact]
    public async Task GetJobById_WithDispatcher_ReturnsOk()
    {
        // Arrange
        SeedTestData();
        var job = await _dbContext.Jobs.FirstAsync();
        SetupControllerUser("Dispatcher", 1);

        // Act
        var result = await _controller.GetJobById(job.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetJobById_WithAssignedContractor_ReturnsOk()
    {
        // Arrange
        SeedTestData();
        var assignedJob = await _dbContext.Jobs
            .Where(j => j.AssignedContractorId != null)
            .FirstAsync();
        SetupControllerUser("Contractor", 3);

        // Act
        var result = await _controller.GetJobById(assignedJob.Id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetJobById_WithUnassignedJob_NotFound()
    {
        // Arrange
        SeedTestData();
        SetupControllerUser("Customer", 2);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _controller.GetJobById(99999));
    }
}

