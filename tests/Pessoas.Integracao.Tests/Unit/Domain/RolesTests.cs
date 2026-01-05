using FluentAssertions;

using Pessoas.Integracao.Core.Domain.Constants;

namespace Pessoas.Integracao.Tests.Unit.Domain;

public sealed class RolesTests
{
    [Theory]
    [InlineData("admin", Roles.Admin)]
    [InlineData("viewer", Roles.Viewer)]
    public void FromExternalProvider_WithValidRole_ReturnsCorrectRole(string externalRole, string expectedRole)
    {
        // Act
        var result = Roles.FromExternalProvider(externalRole);

        // Assert
        result.Should().Be(expectedRole);
    }

    [Theory]
    [InlineData("ADMIN")]
    [InlineData("Admin")]
    [InlineData("aDmIn")]
    public void FromExternalProvider_WithAdminCaseInsensitive_ReturnsAdmin(string externalRole)
    {
        // Act
        var result = Roles.FromExternalProvider(externalRole);

        // Assert
        result.Should().Be(Roles.Admin);
    }

    [Theory]
    [InlineData("VIEWER")]
    [InlineData("Viewer")]
    [InlineData("vIeWeR")]
    public void FromExternalProvider_WithViewerCaseInsensitive_ReturnsViewer(string externalRole)
    {
        // Act
        var result = Roles.FromExternalProvider(externalRole);

        // Assert
        result.Should().Be(Roles.Viewer);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("custom-role")]
    [InlineData("super-admin")]
    [InlineData("")]
    [InlineData(" ")]
    public void FromExternalProvider_WithUnknownRole_ReturnsNull(string externalRole)
    {
        // Act
        var result = Roles.FromExternalProvider(externalRole);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromExternalProvider_WithAdminRole_ReturnsExactConstant()
    {
        // Act
        var result = Roles.FromExternalProvider("admin");

        // Assert
        result.Should().BeSameAs(Roles.Admin);
    }

    [Fact]
    public void FromExternalProvider_WithViewerRole_ReturnsExactConstant()
    {
        // Act
        var result = Roles.FromExternalProvider("viewer");

        // Assert
        result.Should().BeSameAs(Roles.Viewer);
    }
}