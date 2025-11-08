using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartScheduler.Application.DTOs.Auth;
using SmartScheduler.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Implementation of JWT token generation and validation.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeSpan _jwtExpiry;
    private readonly TimeSpan _refreshTokenExpiry;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _secretKey = _configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT secret key is not configured");
        _issuer = _configuration["Jwt:Issuer"] 
            ?? throw new InvalidOperationException("JWT issuer is not configured");
        _audience = _configuration["Jwt:Audience"] 
            ?? throw new InvalidOperationException("JWT audience is not configured");

        // Parse JWT expiry (format: "01:00:00" for 1 hour)
        var jwtExpiryStr = _configuration["Jwt:JwtExpiry"] ?? "01:00:00";
        _jwtExpiry = TimeSpan.Parse(jwtExpiryStr);

        // Parse refresh token expiry (format: "7.00:00:00" for 7 days)
        var refreshTokenExpiryStr = _configuration["Jwt:RefreshTokenExpiry"] ?? "7.00:00:00";
        _refreshTokenExpiry = TimeSpan.Parse(refreshTokenExpiryStr);
    }

    /// <summary>
    /// Generates a JWT access token and refresh token for a user.
    /// </summary>
    /// <param name="user">The user to generate tokens for.</param>
    /// <returns>A TokenResponse containing access token and refresh token.</returns>
    public TokenResponse GenerateToken(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = (int)_jwtExpiry.TotalSeconds,
            TokenType = "Bearer"
        };
    }

    /// <summary>
    /// Generates a new JWT token from a valid refresh token.
    /// </summary>
    /// <param name="user">The user to generate a new token for.</param>
    /// <returns>A TokenResponse containing the new access token.</returns>
    public TokenResponse RefreshToken(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var accessToken = GenerateJwtToken(user);

        return new TokenResponse
        {
            AccessToken = accessToken,
            ExpiresIn = (int)_jwtExpiry.TotalSeconds,
            TokenType = "Bearer"
        };
    }

    /// <summary>
    /// Validates a JWT token and returns the claims principal.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>ClaimsPrincipal with user claims if valid.</returns>
    /// <exception cref="SecurityTokenException">Thrown if token is invalid, expired, or malformed.</exception>
    public ClaimsPrincipal ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty", nameof(token));

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch (Exception ex) when (ex is SecurityTokenException)
        {
            // Re-throw SecurityTokenException and its subclasses as-is
            throw;
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException("Token validation failed", ex);
        }
    }

    /// <summary>
    /// Generates a JWT token for a user.
    /// </summary>
    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("jti", Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(_jwtExpiry),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generates a random refresh token (256 bits / 32 bytes).
    /// </summary>
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}

