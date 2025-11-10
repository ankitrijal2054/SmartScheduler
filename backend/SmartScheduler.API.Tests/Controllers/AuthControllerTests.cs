using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using SmartScheduler.API.Controllers;
using SmartScheduler.Application.DTOs.Auth;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Infrastructure.Persistence;
using SmartScheduler.Infrastructure.Services;

namespace SmartScheduler.API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly AuthController _controller;
    private readonly ApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly Mock<IGeocodingService> _geocodingServiceMock;
    private readonly User _testUser;

    public AuthControllerTests()
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
        _loggerMock = new Mock<ILogger<AuthController>>();
        _geocodingServiceMock = new Mock<IGeocodingService>();

        _controller = new AuthController(
            _jwtTokenService,
            _passwordHashingService,
            _dbContext,
            _loggerMock.Object,
            _geocodingServiceMock.Object);

        // Initialize controller context with HttpContext
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Create test user
        _testUser = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = _passwordHashingService.HashPassword("SecurePassword123!"),
            Role = UserRole.Dispatcher,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(_testUser);
        _dbContext.SaveChanges();
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<TokenResponse>().Subject;
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task Login_WithValidCredentials_SavesRefreshTokenToDatabase()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        await _controller.Login(request);

        // Assert
        var refreshTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == _testUser.Id)
            .ToListAsync();

        refreshTokens.Should().HaveCount(1);
        refreshTokens[0].IsRevoked.Should().BeFalse();
        refreshTokens[0].ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        _testUser.IsActive = false;
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithMissingEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "",
            Password = "SecurePassword123!"
        };

        _controller.ModelState.Clear();
        _controller.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_UpdatesLastLoginTimestamp()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };
        var timeBefore = DateTime.UtcNow;

        // Act
        await _controller.Login(request);

        // Assert
        var updatedUser = await _dbContext.Users.FirstAsync(u => u.Id == _testUser.Id);
        updatedUser.LastLoginAt.Should().BeCloseTo(timeBefore, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Refresh Tests

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsOkWithNewAccessToken()
    {
        // Arrange
        // First login to get a refresh token
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        var loginResult = await _controller.Login(loginRequest) as OkObjectResult;
        var tokenResponse = loginResult!.Value as TokenResponse;
        var refreshToken = tokenResponse!.RefreshToken;

        // Now refresh
        var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshToken! };

        // Act
        var result = await _controller.Refresh(refreshRequest);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<TokenResponse>().Subject;
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().BeNull(); // Refresh endpoint doesn't return refresh token
        response.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest { RefreshToken = "invalid-token" };

        // Act
        var result = await _controller.Refresh(refreshRequest);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Refresh_WithRevokedRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        var loginResult = await _controller.Login(loginRequest) as OkObjectResult;
        var tokenResponse = loginResult!.Value as TokenResponse;
        var refreshTokenValue = tokenResponse!.RefreshToken;

        // Revoke the token
        var refreshToken = await _dbContext.RefreshTokens
            .FirstAsync(rt => rt.Token == refreshTokenValue);
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshTokenValue! };

        // Act
        var result = await _controller.Refresh(refreshRequest);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Refresh_WithExpiredRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        var loginResult = await _controller.Login(loginRequest) as OkObjectResult;
        var tokenResponse = loginResult!.Value as TokenResponse;
        var refreshTokenValue = tokenResponse!.RefreshToken;

        // Expire the token
        var refreshToken = await _dbContext.RefreshTokens
            .FirstAsync(rt => rt.Token == refreshTokenValue);
        refreshToken.ExpiresAt = DateTime.UtcNow.AddSeconds(-1);
        await _dbContext.SaveChangesAsync();

        var refreshRequest = new RefreshTokenRequest { RefreshToken = refreshTokenValue! };

        // Act
        var result = await _controller.Refresh(refreshRequest);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidRefreshToken_ReturnsOkAndRevokesToken()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        var loginResult = await _controller.Login(loginRequest) as OkObjectResult;
        var tokenResponse = loginResult!.Value as TokenResponse;
        var refreshTokenValue = tokenResponse!.RefreshToken;

        // Setup authentication context
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, _testUser.Id.ToString())
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestScheme");
        _controller.ControllerContext.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(identity);

        var logoutRequest = new RefreshTokenRequest { RefreshToken = refreshTokenValue! };

        // Act
        var result = await _controller.Logout(logoutRequest);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var refreshToken = await _dbContext.RefreshTokens
            .FirstAsync(rt => rt.Token == refreshTokenValue);
        refreshToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Logout_WithNonExistentToken_ReturnsOkIdempotent()
    {
        // Arrange
        var logoutRequest = new RefreshTokenRequest { RefreshToken = "non-existent-token" };

        // Act
        var result = await _controller.Logout(logoutRequest);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}

