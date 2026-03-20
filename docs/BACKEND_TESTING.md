# Backend Testing Guide

## Overview

This document outlines the backend testing strategy, patterns, and best practices for the Patient Access Platform. We use **xUnit** as our test framework with **Moq** for mocking and **FluentAssertions** for readable test assertions.

## Technology Stack

| Tool               | Version | Purpose                          |
| ------------------ | ------- | -------------------------------- |
| xUnit              | 2.x     | Test framework and runner        |
| Moq                | 4.x     | Mocking framework for interfaces |
| FluentAssertions   | Latest  | Fluent assertion library         |
| coverlet.collector | Latest  | Code coverage collection         |
| ReportGenerator    | Latest  | Coverage report generation       |

## Testing Philosophy

We follow the **AAA (Arrange-Act-Assert)** pattern and adhere to SOLID principles:

> **Tests should be independent, fast, and focused on a single behavior**

### Key Principles

1. **AAA Pattern**: Structure all tests with clear Arrange, Act, Assert sections
2. **Test behavior, not implementation**: Focus on public API contracts
3. **One assertion per test concept**: Keep tests focused and maintainable
4. **Use meaningful test names**: Test name should describe the scenario and expected outcome
5. **Mock external dependencies**: Isolate the system under test

## Project Structure

```
src/backend/
├── PatientAccess.Tests/
│   ├── Services/
│   │   ├── PasswordHashingServiceTests.cs
│   │   └── JwtTokenServiceTests.cs
│   ├── Repositories/
│   │   └── UserRepositoryTests.cs
│   ├── Controllers/
│   │   └── AuthControllerTests.cs
│   ├── Validators/
│   │   └── LoginDtoValidatorTests.cs
│   ├── coverlet.runsettings
│   └── PatientAccess.Tests.csproj
├── PatientAccess.Business/
├── PatientAccess.Data/
└── PatientAccess.Web/
```

## Configuration

### PatientAccess.Tests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <RunSettingsFilePath>$(MSBuildProjectDirectory)\coverlet.runsettings</RunSettingsFilePath>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="8.0.1" />
    <PackageReference Include="FluentAssertions" Version="8.9.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\PatientAccess.Business\PatientAccess.Business.csproj" />
    <ProjectReference Include="..\PatientAccess.Data\PatientAccess.Data.csproj" />
  </ItemGroup>
</Project>
```

### Coverage Thresholds

- **80% minimum** for business logic components:
  - Line coverage: 80%
  - Branch coverage: 80%
  - Method coverage: 80%

**Excluded from coverage:**

- Test assemblies (\*.Tests)
- Migrations (\*.Migrations)
- DTOs and configuration classes
- Program.cs and Startup.cs
- Auto-generated files (_.Designer.cs, _.Generated.cs)
- Middleware (tested via integration tests)

**Included in coverage:**

- Services (PatientAccess.Business.Services)
- Repositories (PatientAccess.Data.Repositories)
- Controllers (PatientAccess.Web.Controllers)
- Validators

## Writing Tests

### AAA Pattern Example

```csharp
using FluentAssertions;
using Moq;
using PatientAccess.Business.Services;

namespace PatientAccess.Tests.Services;

public class UserServiceTests
{
    [Fact]
    public void CreateUser_WithValidData_ReturnsCreatedUser()
    {
        // Arrange - Setup dependencies and test data
        var mockRepository = new Mock<IUserRepository>();
        var mockPasswordService = new Mock<IPasswordHashingService>();
        var sut = new UserService(mockRepository.Object, mockPasswordService.Object);

        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Password = "SecurePass123!"
        };

        mockPasswordService
            .Setup(x => x.HashPassword(userDto.Password))
            .Returns("hashed_password");

        // Act - Execute the method under test
        var result = sut.CreateUser(userDto);

        // Assert - Verify expected outcomes
        result.Should().NotBeNull();
        result.Email.Should().Be(userDto.Email);
        mockRepository.Verify(x => x.Add(It.IsAny<User>()), Times.Once);
    }
}
```

### Test Naming Convention

Use descriptive names that follow the pattern:

```
MethodName_Scenario_ExpectedBehavior
```

**Examples:**

- `CreateUser_WithValidData_ReturnsCreatedUser`
- `CreateUser_WithDuplicateEmail_ThrowsInvalidOperationException`
- `HashPassword_WithNullInput_ThrowsArgumentNullException`

### Fact vs Theory

**Fact**: Single test case with no parameters

```csharp
[Fact]
public void GetUser_WithValidId_ReturnsUser()
{
    // Test implementation
}
```

**Theory**: Parameterized test with multiple inputs

```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public void ValidateEmail_WithInvalidInput_ReturnsFalse(string email)
{
    // Test implementation
}
```

### Using Moq

#### Basic Mocking

```csharp
// Create mock
var mockRepository = new Mock<IUserRepository>();

// Setup method behavior
mockRepository
    .Setup(x => x.GetById(123))
    .Returns(new User { Id = 123, Email = "test@example.com" });

// Verify method was called
mockRepository.Verify(x => x.GetById(123), Times.Once);
```

#### Setup with Parameters

```csharp
// Match specific parameter
mockRepository
    .Setup(x => x.GetByEmail("test@example.com"))
    .Returns(new User());

// Match any parameter
mockRepository
    .Setup(x => x.GetByEmail(It.IsAny<string>()))
    .Returns(new User());

// Match with predicate
mockRepository
    .Setup(x => x.GetByEmail(It.Is<string>(email => email.Contains("@"))))
    .Returns(new User());
```

#### Async Methods

```csharp
mockRepository
    .Setup(x => x.GetByIdAsync(123))
    .ReturnsAsync(new User { Id = 123 });
```

#### Throwing Exceptions

```csharp
mockRepository
    .Setup(x => x.GetById(It.IsAny<int>()))
    .Throws<NotFoundException>();
```

#### Callback Actions

```csharp
User capturedUser = null;
mockRepository
    .Setup(x => x.Add(It.IsAny<User>()))
    .Callback<User>(user => capturedUser = user);
```

### Using FluentAssertions

```csharp
// Basic assertions
result.Should().NotBeNull();
result.Should().BeOfType<User>();
result.Email.Should().Be("test@example.com");

// String assertions
email.Should().NotBeNullOrWhiteSpace();
message.Should().Contain("error");
message.Should().StartWith("Invalid");

// Numeric assertions
count.Should().BeGreaterThan(0);
count.Should().BeLessThanOrEqualTo(100);

// Collection assertions
list.Should().NotBeEmpty();
list.Should().HaveCount(3);
list.Should().Contain(x => x.Id == 123);

// Exception assertions
Action act = () => service.InvalidMethod();
act.Should().Throw<ArgumentNullException>()
    .WithParameterName("userId")
    .WithMessage("*cannot be null*");

// DateTime assertions
createdAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
```

## Common Testing Patterns

### Testing Services

```csharp
public class PasswordHashingServiceTests
{
    private readonly PasswordHashingService _sut; // System Under Test

    public PasswordHashingServiceTests()
    {
        _sut = new PasswordHashingService();
    }

    [Fact]
    public void HashPassword_WithValidPlaintext_ReturnsValidBCryptHash()
    {
        // Arrange
        const string plaintext = "SecurePassword123!";

        // Act
        var hash = _sut.HashPassword(plaintext);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().StartWith("$2a$12$");
        hash.Length.Should().Be(60);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HashPassword_WithInvalidInput_ThrowsArgumentNullException(string? input)
    {
        // Act
        Action act = () => _sut.HashPassword(input!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("plaintext");
    }
}
```

### Testing with Dependency Injection

```csharp
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IPasswordHashingService> _mockPasswordService;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockPasswordService = new Mock<IPasswordHashingService>();
        _sut = new UserService(_mockRepository.Object, _mockPasswordService.Object);
    }

    [Fact]
    public void CreateUser_WithValidData_CallsRepositoryAdd()
    {
        // Arrange
        var userDto = new CreateUserDto
        {
            Email = "test@example.com",
            Password = "Pass123!"
        };

        _mockPasswordService
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        // Act
        _sut.CreateUser(userDto);

        // Assert
        _mockRepository.Verify(
            x => x.Add(It.Is<User>(u => u.Email == userDto.Email)),
            Times.Once
        );
    }
}
```

### Testing Controllers

```csharp
public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _sut = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Pass123!"
        };

        var expectedToken = "jwt_token_here";
        _mockAuthService
            .Setup(x => x.AuthenticateAsync(loginDto.Email, loginDto.Password))
            .ReturnsAsync(expectedToken);

        // Act
        var result = await _sut.Login(loginDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(new { token = expectedToken });
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPass"
        };

        _mockAuthService
            .Setup(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new UnauthorizedException());

        // Act
        var result = await _sut.Login(loginDto);

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }
}
```

### Testing Validators

```csharp
public class CreateUserDtoValidatorTests
{
    private readonly CreateUserDtoValidator _validator;

    public CreateUserDtoValidatorTests()
    {
        _validator = new CreateUserDtoValidator();
    }

    [Fact]
    public void Validate_WithValidData_ReturnsNoErrors()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            Name = "John Doe"
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Password123!", "Name")] // Empty email
    [InlineData("invalid-email", "Password123!", "Name")] // Invalid email format
    [InlineData("test@example.com", "", "Name")] // Empty password
    [InlineData("test@example.com", "weak", "Name")] // Weak password
    public void Validate_WithInvalidData_ReturnsValidationErrors(
        string email, string password, string name)
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Email = email,
            Password = password,
            Name = name
        };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}
```

## Running Tests

### CLI Commands

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests with custom settings
dotnet test --settings PatientAccess.Tests/coverlet.runsettings

# Run specific test class
dotnet test --filter "FullyQualifiedName~PasswordHashingServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~PasswordHashingServiceTests.HashPassword_WithValidPlaintext_ReturnsValidBCryptHash"

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./TestResults/CoverageReport" -reporttypes:"Html;Cobertura"
```

### Coverage Report

After running tests with coverage, reports are generated in:

- `TestResults/CoverageReport/index.html` - Interactive HTML report
- `TestResults/coverage.cobertura.xml` - Cobertura XML format
- `TestResults/coverage.opencover.xml` - OpenCover XML format

## CI/CD Integration

Tests run automatically in GitHub Actions on:

- Every push to `main` or `develop`
- Every pull request

**Coverage enforcement:**

- Build fails if coverage drops below 80% for any metric
- Coverage report is posted as PR comment
- Coverage artifacts uploaded for review

## Best Practices

### ✅ DO

- Follow AAA (Arrange-Act-Assert) pattern consistently
- Use descriptive test names that explain the scenario
- Test one behavior per test method
- Mock external dependencies (databases, APIs, file system)
- Use FluentAssertions for readable assertions
- Test edge cases and error conditions
- Keep tests fast and independent
- Use constructor injection for test dependencies
- Clean up resources if tests create any

### ❌ DON'T

- Test implementation details (private methods, internal state)
- Reference other test projects or share test state
- Use static or global state in tests
- Write tests that depend on execution order
- Mock value types or sealed classes
- Overuse mocking (only mock dependencies, not the SUT)
- Write overly complex test setup
- Ignore failing tests or mark them as Skip without justification

## Troubleshooting

### Test Discovery Issues

If tests are not discovered:

1. Ensure xUnit packages are installed
2. Rebuild the test project: `dotnet build PatientAccess.Tests`
3. Check test class is public and test methods have `[Fact]` or `[Theory]` attributes

### Coverage Not Generating

1. Ensure coverlet.collector is installed
2. Verify coverlet.runsettings path is correct in csproj
3. Run tests with explicit coverage collection:
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

### Moq Setup Not Working

1. Verify interface method matches setup exactly (name, parameters, return type)
2. Use `It.IsAny<T>()` for flexible parameter matching
3. Check if method is virtual (required for mocking concrete classes)

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq GitHub](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [.NET Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
- [AAA Pattern](https://automationpanda.com/2020/07/07/arrange-act-assert-a-pattern-for-writing-good-tests/)

## Support

For questions or issues:

1. Check this documentation
2. Review existing tests for patterns
3. Consult xUnit/Moq/FluentAssertions documentation
4. Ask in team Slack channel
