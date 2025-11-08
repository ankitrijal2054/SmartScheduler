using System.ComponentModel.DataAnnotations;

namespace SmartScheduler.Application.DTOs.Auth;

/// <summary>
/// DTO for refresh token requests.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// Gets or sets the refresh token value.
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}

