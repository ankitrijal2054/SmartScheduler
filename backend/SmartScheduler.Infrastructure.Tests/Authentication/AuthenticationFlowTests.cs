using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using FluentAssertions;
using SmartScheduler.Application.DTOs.Auth;
using SmartScheduler.Application.Services;
using SmartScheduler.Domain.Entities;
using SmartScheduler.Domain.Enums;
using SmartScheduler.Infrastructure.Persistence;

namespace SmartScheduler.Infrastructure.Tests.Authentication;

public class AuthenticationFlowTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;

    public AuthenticationFlowTests()
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
    }

    [Fact]
    public async Task CompleteAuthenticationFlow_Login_Refresh_Logout()
    {
        // Arrange
        var password = "SecurePassword123!";
        var user = new User
        {
            Id = 1,
            Email = "integration@example.com",
            PasswordHash = _passwordHashingService.HashPassword(password),
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Step 1: Generate token (simulating login)
        var tokenResponse = _jwtTokenService.GenerateToken(user);
        var refreshTokenValue = tokenResponse.RefreshToken;

        // Verify token is valid
        var principal = _jwtTokenService.ValidateToken(tokenResponse.AccessToken);
        principal.Should().NotBeNull();
        principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            .Should().Be(user.Id.ToString());

        // Step 2: Save refresh token to database
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue!,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Step 3: Refresh token using the refresh token
        var foundRefreshToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstAsync(rt => rt.Token == refreshTokenValue);

        foundRefreshToken.Should().NotBeNull();
        foundRefreshToken.IsRevoked.Should().BeFalse();
        foundRefreshToken.ExpiresAt.Should().BeGreaterThan(DateTime.UtcNow);

        var newTokenResponse = _jwtTokenService.RefreshToken(foundRefreshToken.User);
        newTokenResponse.AccessToken.Should().NotBeNullOrEmpty();
        newTokenResponse.RefreshToken.Should().BeNull();

        // Verify new token is valid
        var newPrincipal = _jwtTokenService.ValidateToken(newTokenResponse.AccessToken);
        newPrincipal.Should().NotBeNull();

        // Step 4: Logout by revoking refresh token
        foundRefreshToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Step 5: Verify refresh token is revoked
        var revokedToken = await _dbContext.RefreshTokens
            .FirstAsync(rt => rt.Token == refreshTokenValue);

        revokedToken.IsRevoked.Should().BeTrue();
        revokedToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task PasswordHashing_DifferentSalts_ProducesDifferentHashes()
    {
        // Arrange
        var password = "MyPassword123!";

        // Act
        var hash1 = _passwordHashingService.HashPassword(password);
        var hash2 = _passwordHashingService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2); // Different hashes due to salt
        _passwordHashingService.VerifyPassword(password, hash1).Should().BeTrue();
        _passwordHashingService.VerifyPassword(password, hash2).Should().BeTrue();
    }

    [Fact]
    public async Task MultipleRefreshTokens_ForSingleUser_AllManageable()
    {
        // Arrange
        var user = new User
        {
            Id = 2,
            Email = "multitoken@example.com",
            PasswordHash = _passwordHashingService.HashPassword("password"),
            Role = UserRole.Dispatcher,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Create multiple refresh tokens (e.g., logged in from multiple devices)
        var tokens = new List<RefreshToken>();
        for (int i = 0; i < 3; i++)
        {
            var tokenResponse = _jwtTokenService.GenerateToken(user);
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = tokenResponse.RefreshToken!,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.RefreshTokens.Add(refreshToken);
            tokens.Add(refreshToken);
        }

        await _dbContext.SaveChangesAsync();

        // Act: Revoke only one token (logout from one device)
        tokens[0].RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Assert: First token is revoked, others are active
        var allTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .ToListAsync();

        allTokens.Should().HaveCount(3);
        allTokens[0].IsRevoked.Should().BeTrue();
        allTokens[1].IsRevoked.Should().BeFalse();
        allTokens[2].IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshToken_WithCascadeDelete_DeletesTokensWhenUserDeleted()
    {
        // Arrange
        var user = new User
        {
            Id = 3,
            Email = "cascade@example.com",
            PasswordHash = _passwordHashingService.HashPassword("password"),
            Role = UserRole.Contractor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var tokenResponse = _jwtTokenService.GenerateToken(user);
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = tokenResponse.RefreshToken!,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Act: Delete user
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();

        // Assert: Refresh token is also deleted (cascade delete)
        var remainingTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .ToListAsync();

        remainingTokens.Should().BeEmpty();
    }

    [Fact]
    public async Task JwtToken_IncludesAllRequiredClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 4,
            Email = "claims@example.com",
            PasswordHash = _passwordHashingService.HashPassword("password"),
            Role = UserRole.Dispatcher,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var tokenResponse = _jwtTokenService.GenerateToken(user);
        var principal = _jwtTokenService.ValidateToken(tokenResponse.AccessToken);

        // Assert - Verify all required claims are present
        var claims = principal.Claims.ToList();
        claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Email && c.Value == user.Email);
        claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == UserRole.Dispatcher.ToString());
        claims.Should().Contain(c => c.Type == "jti"); // JWT ID claim
    }
}

