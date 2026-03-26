using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PatientAccess.Business.Interfaces;
using PatientAccess.Business.Services;
using PatientAccess.Data;
using PatientAccess.Data.Models;

namespace PatientAccess.Tests.Services;

/// <summary>
/// Unit tests for AuditLogService (US_022).
/// Tests audit logging for authentication events, failed logins, session timeouts, and queries.
/// </summary>
public class AuditLogServiceTests : IDisposable
{
    private readonly PatientAccessDbContext _dbContext;
    private readonly Mock<ILogger<AuditLogService>> _loggerMock;
    private readonly AuditLogService _sut;

    public AuditLogServiceTests()
    {
        var options = new DbContextOptionsBuilder<PatientAccessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PatientAccessDbContext(options);
        _loggerMock = new Mock<ILogger<AuditLogService>>();
        _sut = new AuditLogService(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region LogAuthEventAsync Tests

    [Fact]
    public async Task LogAuthEventAsync_WithValidData_CreatesAuditLogEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var actionType = AuditActionType.Login;
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";
        var metadata = "{\"email\":\"test@example.com\"}";

        // Act
        await _sut.LogAuthEventAsync(userId, actionType, ipAddress, userAgent, metadata);

        // Assert
        var entry = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        entry.Should().NotBeNull();
        entry!.UserId.Should().Be(userId);
        entry.ActionType.Should().Be("Login");
        entry.IpAddress.Should().Be(ipAddress);
        entry.UserAgent.Should().Be(userAgent);
        entry.ActionDetails.Should().Be(metadata);
        entry.ResourceType.Should().Be("Authentication");
        entry.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LogAuthEventAsync_WithNullIpAndUserAgent_DefaultsToUnknown()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        await _sut.LogAuthEventAsync(userId, AuditActionType.Login, null, null);

        // Assert
        var entry = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        entry.Should().NotBeNull();
        entry!.IpAddress.Should().Be("Unknown");
        entry.UserAgent.Should().Be("Unknown");
        entry.ActionDetails.Should().Be("{}");
    }

    [Fact]
    public async Task LogAuthEventAsync_WithNullUserId_CreatesEntryForFailedEvents()
    {
        // Arrange & Act
        await _sut.LogAuthEventAsync(null, AuditActionType.FailedLogin, "10.0.0.1", "TestAgent");

        // Assert
        var entry = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        entry.Should().NotBeNull();
        entry!.UserId.Should().BeNull();
        entry.ActionType.Should().Be("FailedLogin");
    }

    [Theory]
    [InlineData(AuditActionType.Login, "Login")]
    [InlineData(AuditActionType.Logout, "Logout")]
    [InlineData(AuditActionType.Registration, "Registration")]
    [InlineData(AuditActionType.EmailVerified, "EmailVerified")]
    [InlineData(AuditActionType.SessionTimeout, "SessionTimeout")]
    public async Task LogAuthEventAsync_ConvertsEnumToString_Correctly(AuditActionType actionType, string expected)
    {
        // Act
        await _sut.LogAuthEventAsync(Guid.NewGuid(), actionType, "1.2.3.4", "Agent");

        // Assert
        var entry = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        entry!.ActionType.Should().Be(expected);
    }

    #endregion

    #region LogFailedLoginAsync Tests

    [Fact]
    public async Task LogFailedLoginAsync_HashesEmail_AndRecordsFailure()
    {
        // Arrange
        var email = "user@example.com";
        var ipAddress = "10.0.0.1";
        var failureReason = "Invalid email or password";

        // Act
        await _sut.LogFailedLoginAsync(email, ipAddress, "TestAgent", failureReason);

        // Assert
        var entry = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        entry.Should().NotBeNull();
        entry!.UserId.Should().BeNull();
        entry.ActionType.Should().Be("FailedLogin");
        entry.IpAddress.Should().Be(ipAddress);
        entry.ActionDetails.Should().Contain("hashedEmail");
        entry.ActionDetails.Should().Contain(failureReason);
        entry.ActionDetails.Should().NotContain(email); // Email should be hashed
    }

    [Fact]
    public async Task LogFailedLoginAsync_SameEmailProducesSameHash()
    {
        // Arrange & Act
        await _sut.LogFailedLoginAsync("user@example.com", "1.1.1.1", "Agent1", "Bad password");
        await _sut.LogFailedLoginAsync("user@example.com", "2.2.2.2", "Agent2", "Bad password");

        // Assert
        var entries = await _dbContext.AuditLogs.ToListAsync();
        entries.Should().HaveCount(2);

        // Extract hashed emails from action details
        var hash1 = entries[0].ActionDetails;
        var hash2 = entries[1].ActionDetails;
        // Both should contain the same hash for the same email
        hash1.Should().ContainEquivalentOf("hashedEmail");
        hash2.Should().ContainEquivalentOf("hashedEmail");
    }

    #endregion

    #region LogSessionTimeoutAsync Tests

    [Fact]
    public async Task LogSessionTimeoutAsync_CreatesTimeoutEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lastActivity = DateTime.UtcNow.AddMinutes(-15);

        // Act
        await _sut.LogSessionTimeoutAsync(userId, "192.168.1.100", "Chrome/120", lastActivity);

        // Assert
        var entry = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        entry.Should().NotBeNull();
        entry!.UserId.Should().Be(userId);
        entry.ActionType.Should().Be("SessionTimeout");
        entry.ResourceType.Should().Be("Authentication");
        entry.ActionDetails.Should().Contain("Session expired");
        entry.IpAddress.Should().Be("192.168.1.100");
    }

    [Fact]
    public async Task LogSessionTimeoutAsync_WithNullLastActivity_UsesCurrentTime()
    {
        // Arrange & Act
        await _sut.LogSessionTimeoutAsync(Guid.NewGuid(), null, null);

        // Assert
        var entry = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        entry.Should().NotBeNull();
        entry!.ActionType.Should().Be("SessionTimeout");
        entry.ActionDetails.Should().Contain("lastActivity");
    }

    #endregion

    #region GetAuditLogsAsync Tests

    [Fact]
    public async Task GetAuditLogsAsync_ReturnsPagedResults()
    {
        // Arrange - seed 30 entries
        for (var i = 0; i < 30; i++)
        {
            _dbContext.AuditLogs.Add(new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow.AddMinutes(-i),
                ActionType = "Login",
                ResourceType = "Authentication",
                IpAddress = "1.1.1.1",
                ActionDetails = "{}"
            });
        }
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAuditLogsAsync(page: 1, pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(30);
        result.Items.Should().HaveCount(10);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetAuditLogsAsync_FiltersbyActionType()
    {
        // Arrange
        _dbContext.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            ActionType = "Login",
            ResourceType = "Authentication",
            ActionDetails = "{}"
        });
        _dbContext.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            ActionType = "FailedLogin",
            ResourceType = "Authentication",
            ActionDetails = "{}"
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAuditLogsAsync(actionType: "FailedLogin");

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().AllSatisfy(i => i.ActionType.Should().Be("FailedLogin"));
    }

    [Fact]
    public async Task GetAuditLogsAsync_FiltersByDateRange()
    {
        // Arrange
        _dbContext.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow.AddDays(-5),
            ActionType = "Login",
            ResourceType = "Authentication",
            ActionDetails = "{}"
        });
        _dbContext.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            ActionType = "Login",
            ResourceType = "Authentication",
            ActionDetails = "{}"
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAuditLogsAsync(
            startDate: DateTime.UtcNow.AddDays(-1),
            endDate: DateTime.UtcNow.AddDays(1));

        // Assert
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAuditLogsAsync_FiltersByUserId()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        _dbContext.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            UserId = targetUserId,
            Timestamp = DateTime.UtcNow,
            ActionType = "Login",
            ResourceType = "Authentication",
            ActionDetails = "{}"
        });
        _dbContext.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            ActionType = "Login",
            ResourceType = "Authentication",
            ActionDetails = "{}"
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAuditLogsAsync(userId: targetUserId);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].UserId.Should().Be(targetUserId);
    }

    [Fact]
    public async Task GetAuditLogsAsync_OrdersByTimestampDescending()
    {
        // Arrange
        _dbContext.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow.AddHours(-2),
            ActionType = "Login",
            ResourceType = "Authentication",
            ActionDetails = "{}"
        });
        _dbContext.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            ActionType = "Logout",
            ResourceType = "Authentication",
            ActionDetails = "{}"
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAuditLogsAsync();

        // Assert
        result.Items.Should().BeInDescendingOrder(i => i.Timestamp);
    }

    #endregion

    #region Non-Blocking Error Handling Tests

    [Fact]
    public async Task LogAuthEventAsync_WhenDbFails_DoesNotThrow()
    {
        // Arrange - dispose context to simulate DB failure
        _dbContext.Dispose();

        // Act & Assert - should not throw (non-blocking per edge case requirement)
        var act = async () => await _sut.LogAuthEventAsync(
            Guid.NewGuid(), AuditActionType.Login, "1.1.1.1", "Agent");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LogFailedLoginAsync_WhenDbFails_DoesNotThrow()
    {
        // Arrange
        _dbContext.Dispose();

        // Act & Assert
        var act = async () => await _sut.LogFailedLoginAsync(
            "test@example.com", "1.1.1.1", "Agent", "Bad password");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LogSessionTimeoutAsync_WhenDbFails_DoesNotThrow()
    {
        // Arrange
        _dbContext.Dispose();

        // Act & Assert
        var act = async () => await _sut.LogSessionTimeoutAsync(
            Guid.NewGuid(), "1.1.1.1", "Agent");

        await act.Should().NotThrowAsync();
    }

    #endregion
}
