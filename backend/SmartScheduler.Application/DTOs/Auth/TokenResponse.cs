namespace SmartScheduler.Application.DTOs.Auth;

/// <summary>
/// DTO for token responses (login, refresh).
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// Gets or sets the JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token (only in login response).
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the token expiration time in seconds (e.g., 3600 for 1 hour).
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the token type (always "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
}

