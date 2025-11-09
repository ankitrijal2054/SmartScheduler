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

    /// <summary>
    /// Gets or sets the user's name (required for all roles).
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number (optional for Dispatcher, recommended for Customer/Contractor).
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the location address (required for Contractor, optional for Customer/Dispatcher).
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the trade type (required for Contractor only).
    /// </summary>
    public TradeType? TradeType { get; set; }

    /// <summary>
    /// Gets or sets the working hours start time (required for Contractor only).
    /// Format: HH:mm (e.g., "09:00").
    /// </summary>
    public string? WorkingHoursStart { get; set; }

    /// <summary>
    /// Gets or sets the working hours end time (required for Contractor only).
    /// Format: HH:mm (e.g., "17:00").
    /// </summary>
    public string? WorkingHoursEnd { get; set; }
}


