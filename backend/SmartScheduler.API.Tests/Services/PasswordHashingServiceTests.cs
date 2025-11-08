using Xunit;
using FluentAssertions;
using SmartScheduler.Application.Services;

namespace SmartScheduler.API.Tests.Services;

public class PasswordHashingServiceTests
{
    private readonly PasswordHashingService _service;

    public PasswordHashingServiceTests()
    {
        _service = new PasswordHashingService();
    }

    #region HashPassword Tests

    [Fact]
    public void HashPassword_WithValidPassword_ReturnsNonEmptyHash()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _service.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Length.Should().Be(60); // BCrypt hashes are always 60 characters
    }

    [Fact]
    public void HashPassword_WithValidPassword_HashDifferentFromPassword()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _service.HashPassword(password);

        // Assert
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash1 = _service.HashPassword(password);
        var hash2 = _service.HashPassword(password);

        // Assert
        // Different hashes due to random salt generation
        hash1.Should().NotBe(hash2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HashPassword_WithInvalidPassword_ThrowsArgumentException(string password)
    {
        // Act & Assert
        _service.Invoking(s => s.HashPassword(password))
            .Should().Throw<ArgumentException>();
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword("WrongPassword", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ReturnsFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword("", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithNullPassword_ReturnsFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _service.HashPassword(password);

        // Act
        var result = _service.VerifyPassword(null!, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithNullHash_ReturnsFalse()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var result = _service.VerifyPassword(password, null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidHashFormat_ReturnsFalse()
    {
        // Arrange
        var password = "SecurePassword123!";
        var invalidHash = "not-a-valid-bcrypt-hash";

        // Act
        var result = _service.VerifyPassword(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}

