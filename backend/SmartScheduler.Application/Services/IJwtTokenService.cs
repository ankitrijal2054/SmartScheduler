using System.Security.Claims;
using SmartScheduler.Application.DTOs.Auth;
using SmartScheduler.Domain.Entities;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Interface for JWT token generation and validation.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token and refresh token for a user.
    /// </summary>
    /// <param name="user">The user to generate tokens for.</param>
    /// <returns>A TokenResponse containing access token and refresh token.</returns>
    TokenResponse GenerateToken(User user);

    /// <summary>
    /// Validates a JWT token and returns the claims principal.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>ClaimsPrincipal with user claims if valid.</returns>
    /// <exception cref="SecurityTokenException">Thrown if token is invalid, expired, or malformed.</exception>
    ClaimsPrincipal ValidateToken(string token);

    /// <summary>
    /// Generates a new JWT token from a valid refresh token.
    /// </summary>
    /// <param name="user">The user to generate a new token for.</param>
    /// <returns>A TokenResponse containing the new access token.</returns>
    TokenResponse RefreshToken(User user);
}

