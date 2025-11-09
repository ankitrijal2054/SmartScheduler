using System.ComponentModel.DataAnnotations;
using SmartScheduler.Domain.Enums;

namespace SmartScheduler.Application.DTOs.Auth;

/// <summary>
/// DTO for signup requests.
/// </summary>
public class SignupRequest
{
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's role.
    /// Must be one of: Dispatcher, Customer, or Contractor.
    /// </summary>
    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; set; }
}


