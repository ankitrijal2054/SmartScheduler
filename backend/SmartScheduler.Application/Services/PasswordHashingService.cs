using BCrypt.Net;

namespace SmartScheduler.Application.Services;

/// <summary>
/// Implementation of password hashing and verification using BCrypt.Net-Next.
/// </summary>
public class PasswordHashingService : IPasswordHashingService
{
    /// <summary>
    /// Number of salt rounds for BCrypt (higher = more secure but slower).
    /// 12 rounds provides good balance between security and performance.
    /// </summary>
    private const int SaltRounds = 12;

    /// <summary>
    /// Hashes a plaintext password using BCrypt with 12 salt rounds.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>A BCrypt hash (60 characters).</returns>
    /// <exception cref="ArgumentException">Thrown if password is null or empty.</exception>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, SaltRounds);
    }

    /// <summary>
    /// Verifies a plaintext password against a BCrypt hash.
    /// </summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="hash">The BCrypt hash to verify against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            // If hash format is invalid, return false
            return false;
        }
    }
}

