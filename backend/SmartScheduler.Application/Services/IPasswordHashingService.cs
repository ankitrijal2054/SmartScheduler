namespace SmartScheduler.Application.Services;

/// <summary>
/// Interface for password hashing and verification using BCrypt.
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Hashes a plaintext password using BCrypt with 12 salt rounds.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>A BCrypt hash (60 characters).</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a plaintext password against a BCrypt hash.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="hash">The BCrypt hash to verify against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    bool VerifyPassword(string password, string hash);
}

