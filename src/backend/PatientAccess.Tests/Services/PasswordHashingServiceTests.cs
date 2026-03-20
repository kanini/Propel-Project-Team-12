using FluentAssertions;
using PatientAccess.Business.Services;

namespace PatientAccess.Tests.Services;

/// <summary>
/// Unit tests for PasswordHashingService using AAA (Arrange-Act-Assert) pattern.
/// Tests BCrypt hashing functionality with work factor 12 per TR-013 requirement.
/// </summary>
public class PasswordHashingServiceTests
{
    private readonly PasswordHashingService _sut; // System Under Test

    public PasswordHashingServiceTests()
    {
        _sut = new PasswordHashingService();
    }

    #region HashPassword Tests

    [Fact]
    public void HashPassword_WithValidPlaintext_ReturnsValidBCryptHash()
    {
        // Arrange
        const string plaintext = "SecurePassword123!";

        // Act
        var hash = _sut.HashPassword(plaintext);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace("hash should be generated");
        hash.Should().StartWith("$2a$12$", "BCrypt hash should start with version and cost factor");
        hash.Length.Should().Be(60, "BCrypt hash should be 60 characters");
    }

    [Fact]
    public void HashPassword_CalledTwiceWithSamePlaintext_ReturnsDifferentHashes()
    {
        // Arrange
        const string plaintext = "SecurePassword123!";

        // Act
        var hash1 = _sut.HashPassword(plaintext);
        var hash2 = _sut.HashPassword(plaintext);

        // Assert
        hash1.Should().NotBe(hash2, "BCrypt should generate unique salt for each hash");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HashPassword_WithNullOrEmptyPlaintext_ThrowsArgumentNullException(string? invalidPlaintext)
    {
        // Act
        Action act = () => _sut.HashPassword(invalidPlaintext!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("plaintext")
            .WithMessage("*Password cannot be null or empty*");
    }

    [Theory]
    [InlineData("a")] // Minimum length
    [InlineData("ThisIsAVeryLongPasswordThatExceedsTypicalLimitsButShouldStillBeHashedCorrectly123!@#")]
    public void HashPassword_WithVariousPasswordLengths_ReturnsValidHash(string plaintext)
    {
        // Act
        var hash = _sut.HashPassword(plaintext);

        // Assert
        hash.Should().StartWith("$2a$12$");
        hash.Length.Should().Be(60);
    }

    [Theory]
    [InlineData("SimplePassword")]
    [InlineData("Password123!")]
    [InlineData("Пароль123")] // Cyrillic characters
    [InlineData("密码123")] // Chinese characters
    [InlineData("emoji🔒password")]
    public void HashPassword_WithSpecialCharacters_ReturnsValidHash(string plaintext)
    {
        // Act
        var hash = _sut.HashPassword(plaintext);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().StartWith("$2a$12$");
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_WithCorrectPlaintext_ReturnsTrue()
    {
        // Arrange
        const string plaintext = "SecurePassword123!";
        var hash = _sut.HashPassword(plaintext);

        // Act
        var result = _sut.VerifyPassword(plaintext, hash);

        // Assert
        result.Should().BeTrue("correct password should verify successfully");
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPlaintext_ReturnsFalse()
    {
        // Arrange
        const string correctPassword = "SecurePassword123!";
        const string incorrectPassword = "WrongPassword456!";
        var hash = _sut.HashPassword(correctPassword);

        // Act
        var result = _sut.VerifyPassword(incorrectPassword, hash);

        // Assert
        result.Should().BeFalse("incorrect password should not verify");
    }

    [Theory]
    [InlineData(null, "validhash")]
    [InlineData("", "validhash")]
    [InlineData("   ", "validhash")]
    [InlineData("validplaintext", null)]
    [InlineData("validplaintext", "")]
    [InlineData("validplaintext", "   ")]
    public void VerifyPassword_WithNullOrEmptyInputs_ReturnsFalse(string? plaintext, string? hash)
    {
        // Act
        var result = _sut.VerifyPassword(plaintext!, hash!);

        // Assert
        result.Should().BeFalse("null or empty inputs should return false");
    }

    [Fact]
    public void VerifyPassword_WithInvalidHashFormat_ReturnsFalse()
    {
        // Arrange
        const string plaintext = "SecurePassword123!";
        const string invalidHash = "not-a-valid-bcrypt-hash";

        // Act
        var result = _sut.VerifyPassword(plaintext, invalidHash);

        // Assert
        result.Should().BeFalse("invalid hash format should return false without throwing");
    }

    [Fact]
    public void VerifyPassword_IsCaseSensitive()
    {
        // Arrange
        const string plaintext = "SecurePassword123!";
        var hash = _sut.HashPassword(plaintext);

        // Act
        var lowercaseResult = _sut.VerifyPassword("securepassword123!", hash);
        var uppercaseResult = _sut.VerifyPassword("SECUREPASSWORD123!", hash);

        // Assert
        lowercaseResult.Should().BeFalse("password verification should be case-sensitive");
        uppercaseResult.Should().BeFalse("password verification should be case-sensitive");
    }

    [Fact]
    public void VerifyPassword_WithHashFromDifferentPassword_ReturnsFalse()
    {
        // Arrange
        const string password1 = "Password123!";
        const string password2 = "DifferentPassword456!";
        var hash1 = _sut.HashPassword(password1);

        // Act
        var result = _sut.VerifyPassword(password2, hash1);

        // Assert
        result.Should().BeFalse("hash from different password should not verify");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void HashAndVerify_RoundTrip_WorksCorrectly()
    {
        // Arrange
        const string plaintext = "SecurePassword123!";

        // Act
        var hash = _sut.HashPassword(plaintext);
        var verifyCorrect = _sut.VerifyPassword(plaintext, hash);
        var verifyIncorrect = _sut.VerifyPassword("WrongPassword", hash);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        verifyCorrect.Should().BeTrue("correct password should verify");
        verifyIncorrect.Should().BeFalse("incorrect password should not verify");
    }

    [Fact]
    public void PasswordHashingService_ImplementsInterface()
    {
        // Assert
        _sut.Should().BeAssignableTo<IPasswordHashingService>("service should implement interface");
    }

    #endregion
}
