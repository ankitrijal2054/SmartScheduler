using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;

namespace SmartScheduler.API.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly IJwtTokenService _service;
    private readonly IConfiguration _configuration;
    private readonly User _testUser;

    public JwtTokenServiceTests()
    {
        // Create configuration with JWT settings
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "test-secret-key-with-more-than-32-characters-for-testing" },
                { "Jwt:Issuer", "SmartScheduler" },
                { "Jwt:Audience", "SmartSchedulerAPI" },
                { "Jwt:JwtExpiry", "01:00:00" },
                { "Jwt:RefreshTokenExpiry", "7.00:00:00" }
            });

        _configuration = configBuilder.Build();
        _service = new JwtTokenService(_configuration);

        // Create test user
        _testUser = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = UserRole.Dispatcher,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region GenerateToken Tests

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsTokenResponse()
    {
        // Act
        var response = _service.GenerateToken(_testUser);

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
        response.ExpiresIn.Should().Be(3600); // 1 hour
        response.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public void GenerateToken_WithValidUser_JwtContainsCorrectClaims()
    {
        // Act
        var response = _service.GenerateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(response.AccessToken);

        // Assert
        // JWT tokens use short claim type names (e.g., "nameid" instead of the full URI)
        token.Claims.Should().Contain(c => (c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid") && c.Value == _testUser.Id.ToString());
        token.Claims.Should().Contain(c => (c.Type == ClaimTypes.Email || c.Type == "email") && c.Value == _testUser.Email);
        token.Claims.Should().Contain(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == UserRole.Dispatcher.ToString());
        token.Claims.Should().Contain(c => c.Type == "jti");
    }

    [Fact]
    public void GenerateToken_WithValidUser_JwtExpiresInOneHour()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var response = _service.GenerateToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(response.AccessToken);

        // Assert
        var expirationTime = token.ValidTo;
        var timeDifference = expirationTime - now;
        timeDifference.Should().BeCloseTo(TimeSpan.FromHours(1), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateToken_WithValidUser_RefreshTokenIsBase64String()
    {
        // Act
        var response = _service.GenerateToken(_testUser);

        // Assert
        // Refresh token should be a valid base64 string (can be decoded)
        var action = () => Convert.FromBase64String(response.RefreshToken!);
        action.Should().NotThrow();
    }

    [Fact]
    public void GenerateToken_WithNullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        _service.Invoking(s => s.GenerateToken(null!))
            .Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public void RefreshToken_WithValidUser_ReturnsTokenResponse()
    {
        // Act
        var response = _service.RefreshToken(_testUser);

        // Assert
        response.Should().NotBeNull();
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().BeNull(); // Refresh endpoint doesn't return new refresh token
        response.ExpiresIn.Should().Be(3600);
        response.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public void RefreshToken_WithValidUser_JwtContainsCorrectClaims()
    {
        // Act
        var response = _service.RefreshToken(_testUser);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(response.AccessToken);

        // Assert
        // JWT tokens use short claim type names (e.g., "nameid" instead of the full URI)
        token.Claims.Should().Contain(c => (c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid") && c.Value == _testUser.Id.ToString());
        token.Claims.Should().Contain(c => (c.Type == ClaimTypes.Email || c.Type == "email") && c.Value == _testUser.Email);
        token.Claims.Should().Contain(c => (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == UserRole.Dispatcher.ToString());
    }

    [Fact]
    public void RefreshToken_WithNullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        _service.Invoking(s => s.RefreshToken(null!))
            .Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var tokenResponse = _service.GenerateToken(_testUser);

        // Act
        var principal = _service.ValidateToken(tokenResponse.AccessToken);

        // Assert
        principal.Should().NotBeNull();
        principal.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(_testUser.Id.ToString());
        principal.FindFirst(ClaimTypes.Email)?.Value.Should().Be(_testUser.Email);
    }

    [Fact]
    public void ValidateToken_WithValidToken_ExtractsAllClaims()
    {
        // Arrange
        var tokenResponse = _service.GenerateToken(_testUser);

        // Act
        var principal = _service.ValidateToken(tokenResponse.AccessToken);

        // Assert
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Email);
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Role);
        principal.Claims.Should().Contain(c => c.Type == "jti");
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ThrowsSecurityTokenException()
    {
        // Arrange
        // Create an expired token (set expiry to past)
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "test-secret-key-with-more-than-32-characters-for-testing" },
                { "Jwt:Issuer", "SmartScheduler" },
                { "Jwt:Audience", "SmartSchedulerAPI" },
                { "Jwt:JwtExpiry", "00:00:01" }, // 1 second expiry
                { "Jwt:RefreshTokenExpiry", "7.00:00:00" }
            });
        var configuration = configBuilder.Build();
        var service = new JwtTokenService(configuration);
        var tokenResponse = service.GenerateToken(_testUser);

        // Wait for token to expire
        System.Threading.Thread.Sleep(1500);

        // Act & Assert
        service.Invoking(s => s.ValidateToken(tokenResponse.AccessToken))
            .Should().Throw<SecurityTokenException>();
    }

    [Fact]
    public void ValidateToken_WithInvalidSignature_ThrowsSecurityTokenException()
    {
        // Arrange
        var tokenResponse = _service.GenerateToken(_testUser);
        // Tamper with token
        var tamperedToken = tokenResponse.AccessToken[0..^10] + "0000000000";

        // Act & Assert
        _service.Invoking(s => s.ValidateToken(tamperedToken))
            .Should().Throw<SecurityTokenException>();
    }

    [Fact]
    public void ValidateToken_WithMalformedToken_ThrowsSecurityTokenException()
    {
        // Arrange
        var malformedToken = "not-a-valid-jwt";

        // Act & Assert
        // SecurityTokenMalformedException is a subclass of SecurityTokenException
        _service.Invoking(s => s.ValidateToken(malformedToken))
            .Should().Throw<SecurityTokenException>();
    }

    [Fact]
    public void ValidateToken_WithEmptyToken_ThrowsArgumentException()
    {
        // Act & Assert
        _service.Invoking(s => s.ValidateToken(""))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateToken_WithNullToken_ThrowsArgumentException()
    {
        // Act & Assert
        _service.Invoking(s => s.ValidateToken(null!))
            .Should().Throw<ArgumentException>();
    }

    #endregion
}

