using System.Security.Claims;
using FluentAssertions;
using Xunit;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;

namespace SmartScheduler.Application.Tests.Services;

public class AuthorizationServiceTests
{
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationServiceTests()
    {
        _authorizationService = new AuthorizationService();
    }

    [Fact]
    public void GetCurrentUserIdFromContext_WithValidClaim_ReturnsUserId()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Email, "user@test.com")
        };
        var claimsIdentity = new ClaimsIdentity(claims, "test");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Act
        var userId = _authorizationService.GetCurrentUserIdFromContext(claimsPrincipal);

        // Assert
        userId.Should().Be(123);
    }

    [Fact]
    public void GetCurrentUserIdFromContext_WithMissingClaim_ThrowsArgumentException()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, "user@test.com")
        };
        var claimsIdentity = new ClaimsIdentity(claims, "test");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _authorizationService.GetCurrentUserIdFromContext(claimsPrincipal));
    }

    [Fact]
    public void GetCurrentUserIdFromContext_WithNullClaims_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _authorizationService.GetCurrentUserIdFromContext(null!));
    }

    [Fact]
    public void ValidateUserOwnsResource_WithMatchingIds_ReturnsTrue()
    {
        // Arrange
        int userId = 123;
        int resourceOwnerId = 123;

        // Act
        var result = _authorizationService.ValidateUserOwnsResource(userId, resourceOwnerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateUserOwnsResource_WithDifferentIds_ReturnsFalse()
    {
        // Arrange
        int userId = 123;
        int resourceOwnerId = 456;

        // Act
        var result = _authorizationService.ValidateUserOwnsResource(userId, resourceOwnerId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateUserRole_WithMatchingRole_ReturnsTrue()
    {
        // Arrange
        string requiredRole = "Dispatcher";
        string userRole = "Dispatcher";

        // Act
        var result = _authorizationService.ValidateUserRole(requiredRole, userRole);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateUserRole_WithDifferentRole_ReturnsFalse()
    {
        // Arrange
        string requiredRole = "Dispatcher";
        string userRole = "Customer";

        // Act
        var result = _authorizationService.ValidateUserRole(requiredRole, userRole);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateUserRole_WithNullUserRole_ReturnsFalse()
    {
        // Arrange
        string requiredRole = "Dispatcher";
        string? userRole = null;

        // Act
        var result = _authorizationService.ValidateUserRole(requiredRole, userRole);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateUserRole_CaseInsensitive_ReturnsTrue()
    {
        // Arrange
        string requiredRole = "Dispatcher";
        string userRole = "dispatcher";

        // Act
        var result = _authorizationService.ValidateUserRole(requiredRole, userRole);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void FilterDataByRole_WithDispatcherRole_ReturnsAllData()
    {
        // Arrange
        var jobs = new List<Job>
        {
            new Job { Id = 1, CustomerId = 10, Status = JobStatus.Pending, JobType = TradeType.Plumbing, Location = "", Description = "", Latitude = 0, Longitude = 0, DesiredDateTime = DateTime.Now, EstimatedDurationHours = 2 },
            new Job { Id = 2, CustomerId = 20, Status = JobStatus.Pending, JobType = TradeType.Electrical, Location = "", Description = "", Latitude = 0, Longitude = 0, DesiredDateTime = DateTime.Now, EstimatedDurationHours = 2 }
        }.AsQueryable();

        // Act
        var filtered = _authorizationService.FilterDataByRole(999, "Dispatcher", jobs);

        // Assert
        filtered.Count().Should().Be(2);
    }

    [Fact]
    public void FilterDataByRole_WithCustomerRole_ReturnsOnlyOwnJobs()
    {
        // Arrange
        var jobs = new List<Job>
        {
            new Job { Id = 1, CustomerId = 10, Status = JobStatus.Pending, JobType = TradeType.Plumbing, Location = "", Description = "", Latitude = 0, Longitude = 0, DesiredDateTime = DateTime.Now, EstimatedDurationHours = 2 },
            new Job { Id = 2, CustomerId = 20, Status = JobStatus.Pending, JobType = TradeType.Electrical, Location = "", Description = "", Latitude = 0, Longitude = 0, DesiredDateTime = DateTime.Now, EstimatedDurationHours = 2 }
        }.AsQueryable();

        int userId = 10; // This is CustomerId

        // Act
        var filtered = _authorizationService.FilterDataByRole(userId, "Customer", jobs);

        // Assert
        filtered.Count().Should().Be(1);
        filtered.First().CustomerId.Should().Be(10);
    }

    [Fact]
    public void FilterDataByRole_WithContractorRole_ReturnsEmptyForNoAssignments()
    {
        // Arrange
        var jobs = new List<Job>
        {
            new Job { Id = 1, CustomerId = 10, Status = JobStatus.Pending, JobType = TradeType.Plumbing, Location = "", Description = "", Latitude = 0, Longitude = 0, DesiredDateTime = DateTime.Now, EstimatedDurationHours = 2, Assignment = null },
            new Job { Id = 2, CustomerId = 20, Status = JobStatus.Pending, JobType = TradeType.Electrical, Location = "", Description = "", Latitude = 0, Longitude = 0, DesiredDateTime = DateTime.Now, EstimatedDurationHours = 2, Assignment = null }
        }.AsQueryable();

        int userId = 30; // Contractor not assigned to any job

        // Act
        var filtered = _authorizationService.FilterDataByRole(userId, "Contractor", jobs);

        // Assert
        filtered.Count().Should().Be(0);
    }

    [Fact]
    public void FilterDataByRole_WithNullRole_ReturnsEmptyCollection()
    {
        // Arrange
        var jobs = new List<Job>
        {
            new Job { Id = 1, CustomerId = 10, Status = JobStatus.Pending, JobType = TradeType.Plumbing, Location = "", Description = "", Latitude = 0, Longitude = 0, DesiredDateTime = DateTime.Now, EstimatedDurationHours = 2 }
        }.AsQueryable();

        // Act
        var filtered = _authorizationService.FilterDataByRole(999, null, jobs);

        // Assert
        filtered.Count().Should().Be(0);
    }

    [Fact]
    public void FilterDataByRole_WithInvalidRole_ReturnsEmptyCollection()
    {
        // Arrange
        var jobs = new List<Job>
        {
            new Job { Id = 1, CustomerId = 10, Status = JobStatus.Pending, JobType = TradeType.Plumbing, Location = "", Description = "", Latitude = 0, Longitude = 0, DesiredDateTime = DateTime.Now, EstimatedDurationHours = 2 }
        }.AsQueryable();

        // Act
        var filtered = _authorizationService.FilterDataByRole(999, "InvalidRole", jobs);

        // Assert
        filtered.Count().Should().Be(0);
    }
}

