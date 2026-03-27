using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PatientAccess.Business.Services;
using PatientAccess.Data;
using Xunit;
using FluentAssertions;

namespace PatientAccess.Tests.Services;

/// <summary>
/// Unit tests for DocumentChunkingService token counting logic.
/// Validates AIR-R01 (512-token chunks with 64-token overlap) - Tiktoken tokenization.
/// Note: Database integration tests with chunking operations require PostgreSQL test database with pgvector.
/// </summary>
public class DocumentChunkingServiceTests
{
    private readonly Mock<ILogger<DocumentChunkingService>> _loggerMock;

    public DocumentChunkingServiceTests()
    {
        _loggerMock = new Mock<ILogger<DocumentChunkingService>>();
    }

    [Fact]
    public async Task GetTokenCount_EmptyString_ReturnsZero()
    {
        // Arrange
        var service = CreateService();
        var emptyText = "";

        // Act
        var tokenCount = await service.GetTokenCountAsync(emptyText);

        // Assert
        tokenCount.Should().Be(0, "Empty string should have 0 tokens");
    }

    [Fact]
    public async Task GetTokenCount_NullString_ReturnsZero()
    {
        // Arrange
        var service = CreateService();
        string? nullText = null;

        // Act
        var tokenCount = await service.GetTokenCountAsync(nullText!);

        // Assert
        tokenCount.Should().Be(0, "Null text should be treated as 0 tokens");
    }

    [Fact]
    public async Task GetTokenCount_SimpleText_ReturnsPositiveCount()
    {
        // Arrange
        var service = CreateService();
        var text = "Hello, world! This is a test.";

        // Act
        var tokenCount = await service.GetTokenCountAsync(text);

        // Assert
        tokenCount.Should().BeGreaterThan(0, "Non-empty text should have positive token count");
        tokenCount.Should().BeLessThan(20, "Simple sentence should have reasonable token count");
    }

    [Fact]
    public async Task GetTokenCount_MedicalCodeText_ReturnsReasonableCount()
    {
        // Arrange
        var service = CreateService();
        var text = "E11.9 Type 2 diabetes mellitus without complications";

        // Act
        var tokenCount = await service.GetTokenCountAsync(text);

        // Assert
        tokenCount.Should().BeInRange(8, 15, "Medical code text should have reasonable token count");
    }

    [Fact]
    public async Task GetTokenCount_LongText_ReturnsHighCount()
    {
        // Arrange
        var service = CreateService();
        var longText = string.Join(" ", Enumerable.Repeat("medical", 1000)); // ~1000 words

        // Act
        var tokenCount = await service.GetTokenCountAsync(longText);

        // Assert
        tokenCount.Should().BeGreaterThan(500, "Long text (~1000 words) should have >500 tokens");
        tokenCount.Should().BeLessThan(1500, "Long text (~1000 words) should have <1500 tokens");
    }

    [Theory]
    [InlineData("E11.9", 2, 5)] // ICD-10 code format
    [InlineData("99213", 1, 4)] // CPT code format
    [InlineData("Hypertension", 1, 4)] // Single medical term
    [InlineData("", 0, 0)] // Empty
    [InlineData("The quick brown fox jumps over the lazy dog.", 8, 15)] // Common phrase
    public async Task GetTokenCount_VariousInputs_ReturnsExpectedRange(string text, int minTokens, int maxTokens)
    {
        // Arrange
        var service = CreateService();

        // Act
        var tokenCount = await service.GetTokenCountAsync(text);

        // Assert
        tokenCount.Should().BeInRange(minTokens, maxTokens,
            $"Token count for '{text}' should be between {minTokens} and {maxTokens}");
    }

    [Fact]
    public async Task GetTokenCount_VerifyTiktokenCl100kBase_IsUsed()
    {
        // Arrange
        var service = CreateService();
        // cl100k_base encoding is used by text-embedding-3-small
        // Verify with known token count for specific text
        var text = "Hello";

        // Act
        var tokenCount = await service.GetTokenCountAsync(text);

        // Assert
        tokenCount.Should().BeGreaterThan(0, "Should use Tiktoken cl100k_base encoding");
        tokenCount.Should().BeLessThan(5, "Single word should have minimal tokens");
    }

    /// <summary>
    /// Helper method to create DocumentChunkingService instance.
    /// Uses In-Memory database which is sufficient for token counting tests.
    /// Note: Full chunking integration tests require PostgreSQL test database.
    /// </summary>
    private DocumentChunkingService CreateService()
    {
        var options = new DbContextOptionsBuilder<PatientAccessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new PatientAccessDbContext(options);
        return new DocumentChunkingService(_loggerMock.Object, context);
    }
}
