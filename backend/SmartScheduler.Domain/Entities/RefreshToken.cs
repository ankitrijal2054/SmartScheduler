namespace SmartScheduler.Domain.Entities;

/// <summary>
/// Represents a refresh token used for obtaining new JWT tokens.
/// Refresh tokens are long-lived and stored in the database for revocation support.
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// Gets or sets the user ID this refresh token belongs to.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the actual refresh token value (randomly generated, 256 bits).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when this refresh token expires (7 days from creation).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets when this refresh token was revoked (null if not revoked).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Gets a value indicating whether this refresh token has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    // Navigation properties
    /// <summary>
    /// Gets or sets the user this refresh token belongs to.
    /// </summary>
    public virtual User User { get; set; } = null!;
}

