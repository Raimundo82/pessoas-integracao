using System.Security.Claims;

using FluentAssertions;

using Moq;

using Pessoas.Integracao.Admin.Authorization;
using Pessoas.Integracao.Core.Domain.Constants;

namespace Pessoas.Integracao.Tests.Unit.Authorization;

public sealed class IdentityProviderRolesClaimsTransformationTests
{
    private readonly Mock<ILogger<IdentityProviderRolesClaimsTransformation>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly IdentityProviderRolesClaimsTransformation _transformation;

    public IdentityProviderRolesClaimsTransformationTests()
    {
        _mockLogger = new Mock<ILogger<IdentityProviderRolesClaimsTransformation>>();

        // Create in-memory configuration
        var configurationData = new Dictionary<string, string?>
        {
            ["Keycloak:Resource"] = "test-client"
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        _transformation = new IdentityProviderRolesClaimsTransformation(_mockLogger.Object, _configuration);
    }

    [Fact]
    public async Task TransformAsync_WhenUnauthenticated_ReturnsUnchangedPrincipal()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.Should().BeSameAs(principal);
        result.Identity!.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task TransformAsync_WithValidAdminRole_AddsAdminRoleClaim()
    {
        // Arrange
        var resourceAccessJson = """
        {
            "test-client": {
                "roles": ["admin"]
            }
        }
        """;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new("resource_access", resourceAccessJson)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.Identities.Should().HaveCount(2);  // Original + new identity with roles
        result.IsInRole(Roles.Admin).Should().BeTrue();
        result.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == Roles.Admin);
    }

    [Fact]
    public async Task TransformAsync_WithValidViewerRole_AddsViewerRoleClaim()
    {
        // Arrange
        var resourceAccessJson = """
        {
            "test-client": {
                "roles": ["viewer"]
            }
        }
        """;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new("resource_access", resourceAccessJson)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.IsInRole(Roles.Viewer).Should().BeTrue();
    }

    [Fact]
    public async Task TransformAsync_WithMultipleValidRoles_AddsAllRoleClaims()
    {
        // Arrange
        var resourceAccessJson = """
        {
            "test-client": {
                "roles": ["admin", "viewer"]
            }
        }
        """;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new("resource_access", resourceAccessJson)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.IsInRole(Roles.Admin).Should().BeTrue();
        result.IsInRole(Roles.Viewer).Should().BeTrue();
    }

    [Fact]
    public async Task TransformAsync_WithCaseInsensitiveRole_AddsCorrectRoleClaim()
    {
        // Arrange
        var resourceAccessJson = """
        {
            "test-client": {
                "roles": ["ADMIN", "Viewer"]
            }
        }
        """;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new("resource_access", resourceAccessJson)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.IsInRole(Roles.Admin).Should().BeTrue();
        result.IsInRole(Roles.Viewer).Should().BeTrue();
    }

    [Fact]
    public async Task TransformAsync_WithUnknownRole_DoesNotAddRoleClaim()
    {
        // Arrange
        var resourceAccessJson = """
        {
            "test-client": {
                "roles": ["unknown-role", "custom-role"]
            }
        }
        """;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new("resource_access", resourceAccessJson)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.Claims.Where(c => c.Type == ClaimTypes.Role).Should().BeEmpty();
    }

    [Fact]
    public async Task TransformAsync_WithMixedValidAndInvalidRoles_AddsOnlyValidRoles()
    {
        // Arrange
        var resourceAccessJson = """
        {
            "test-client": {
                "roles": ["admin", "unknown-role", "viewer"]
            }
        }
        """;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new("resource_access", resourceAccessJson)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.IsInRole(Roles.Admin).Should().BeTrue();
        result.IsInRole(Roles.Viewer).Should().BeTrue();
        result.Claims.Where(c => c.Type == ClaimTypes.Role).Should().HaveCount(2);
    }

    [Fact]
    public async Task TransformAsync_WithoutResourceAccessClaim_DoesNotAddRoles()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.Claims.Where(c => c.Type == ClaimTypes.Role).Should().BeEmpty();
    }

    [Fact]
    public async Task TransformAsync_WithMalformedJson_DoesNotAddRoles()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new("resource_access", "{ invalid json }")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.Claims.Where(c => c.Type == ClaimTypes.Role).Should().BeEmpty();
    }

    [Fact]
    public async Task TransformAsync_WithMissingClientId_DoesNotAddRoles()
    {
        // Arrange
        var resourceAccessJson = """
        {
            "different-client": {
                "roles": ["admin"]
            }
        }
        """;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new("resource_access", resourceAccessJson)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.Claims.Where(c => c.Type == ClaimTypes.Role).Should().BeEmpty();
    }

    [Fact]
    public async Task TransformAsync_WithEmptyRolesArray_DoesNotAddRoles()
    {
        // Arrange
        var resourceAccessJson = """
        {
            "test-client": {
                "roles": []
            }
        }
        """;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new("resource_access", resourceAccessJson)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.Claims.Where(c => c.Type == ClaimTypes.Role).Should().BeEmpty();
    }

    [Fact]
    public async Task TransformAsync_PreservesOriginalClaims()
    {
        // Arrange
        var resourceAccessJson = """
        {
            "test-client": {
                "roles": ["admin"]
            }
        }
        """;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@example.com"),
            new("resource_access", resourceAccessJson)
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.FindFirst(ClaimTypes.Name)!.Value.Should().Be("Test User");
        result.FindFirst(ClaimTypes.Email)!.Value.Should().Be("test@example.com");
        result.FindFirst("resource_access")!.Value.Should().Be(resourceAccessJson);
    }
}