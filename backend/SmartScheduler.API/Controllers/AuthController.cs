using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartScheduler.Application.Services;
using SmartScheduler.Application.DTOs.Auth;
using SmartScheduler.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SmartScheduler.Domain.Entities;

namespace SmartScheduler.API.Controllers;

/// <summary>
/// Authentication controller for user login, token refresh, and logout.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IJwtTokenService jwtTokenService,
        IPasswordHashingService passwordHashingService,
        ApplicationDbContext dbContext,
        ILogger<AuthController> logger)
    {
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _passwordHashingService = passwordHashingService ?? throw new ArgumentNullException(nameof(passwordHashingService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticates a user with email and password, returns JWT and refresh token.
    /// </summary>
    /// <param name="request">Login credentials (email, password).</param>
    /// <returns>200 OK with JWT and refresh token, or 401 Unauthorized if credentials invalid.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Find user by email (case-insensitive)
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
            return Unauthorized(new { error = new { code = "INVALID_CREDENTIALS", message = "Invalid email or password", statusCode = 401 } });
        }

        // Verify password
        if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login attempt with invalid password for email: {Email}", request.Email);
            return Unauthorized(new { error = new { code = "INVALID_CREDENTIALS", message = "Invalid email or password", statusCode = 401 } });
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
            return Unauthorized(new { error = new { code = "ACCOUNT_INACTIVE", message = "Account is inactive", statusCode = 401 } });
        }

        // Generate JWT and refresh token
        var tokenResponse = _jwtTokenService.GenerateToken(user);

        // Save refresh token to database
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = tokenResponse.RefreshToken!,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

        return Ok(tokenResponse);
    }

    /// <summary>
    /// Refreshes the JWT token using a valid refresh token.
    /// </summary>
    /// <param name="request">Refresh token request.</param>
    /// <returns>200 OK with new JWT token, or 401 Unauthorized if refresh token invalid/revoked.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Find refresh token in database
        var refreshToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh attempt with invalid token");
            return Unauthorized(new { error = new { code = "INVALID_TOKEN", message = "Invalid refresh token", statusCode = 401 } });
        }

        if (refreshToken.IsRevoked)
        {
            _logger.LogWarning("Refresh attempt with revoked token for user: {UserId}", refreshToken.UserId);
            return Unauthorized(new { error = new { code = "TOKEN_REVOKED", message = "Refresh token has been revoked", statusCode = 401 } });
        }

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh attempt with expired token for user: {UserId}", refreshToken.UserId);
            return Unauthorized(new { error = new { code = "TOKEN_EXPIRED", message = "Refresh token expired", statusCode = 401 } });
        }

        // Generate new JWT
        var tokenResponse = _jwtTokenService.RefreshToken(refreshToken.User);

        _logger.LogInformation("Token refreshed successfully for user: {UserId}", refreshToken.UserId);

        return Ok(tokenResponse);
    }

    /// <summary>
    /// Logs out a user by revoking their refresh token.
    /// </summary>
    /// <param name="request">Refresh token to revoke.</param>
    /// <returns>200 OK if logout successful.</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Find refresh token
        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (refreshToken == null)
        {
            // Idempotent response: treat missing token as already logged out
            _logger.LogWarning("Logout attempt with non-existent token");
            return Ok(new { message = "Logged out successfully" });
        }

        // Revoke token
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User logged out successfully: {UserId}", refreshToken.UserId);

        return Ok(new { message = "Logged out successfully" });
    }
}

