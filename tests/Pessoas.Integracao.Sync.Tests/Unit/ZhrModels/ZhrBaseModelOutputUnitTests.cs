using FluentAssertions;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Tests.Unit.ZhrModels;

public class ZhrSBaseModelOutputTests
{
    private sealed class TestChild : ZhrSBaseModel
    {
    }

    private sealed class TestOutput : ZhrSBaseModelOutput
    {
        public IReadOnlyList<ZhrSBaseModel> Children { get; init; } = [];

        public override IReadOnlyList<ZhrSBaseModel> GetChildren() => Children;
    }

    [Fact]
    public void SetUpdatedAt_ShouldSetTimestamp()
    {
        // Arrange
        var output = new TestOutput();

        var timestamp = new DateTimeOffset(
            2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        output.SetUpdatedAt(timestamp);

        // Assert
        output.UpdatedAt.Should().Be(timestamp);
    }

    [Fact]
    public void SetUpdatedAt_ShouldOverwriteExistingTimestamp()
    {
        // Arrange
        var output = new TestOutput
        {
            UpdatedAt = new DateTimeOffset(
                2024, 1, 1, 12, 0, 0, TimeSpan.Zero)
        };

        var timestamp = new DateTimeOffset(
            2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

        // Act
        output.SetUpdatedAt(timestamp);

        // Assert
        output.UpdatedAt.Should().Be(timestamp);
    }

    [Fact]
    public void SetNi_ShouldPopulateEmptyNi()
    {
        // Arrange
        var child = new TestChild
        {
            Ni = string.Empty
        };

        var output = new TestOutput
        {
            Children = [child]
        };

        // Act
        output.SetNi("ROOT123");

        // Assert
        child.Ni.Should().Be("ROOT123");
    }

    [Fact]
    public void SetNi_ShouldPopulateWhitespaceNi()
    {
        // Arrange
        var child = new TestChild
        {
            Ni = "   "
        };

        var output = new TestOutput
        {
            Children = [child]
        };

        // Act
        output.SetNi("ROOT123");

        // Assert
        child.Ni.Should().Be("ROOT123");
    }

    [Fact]
    public void SetNi_ShouldPreserveExistingNi()
    {
        // Arrange
        var child = new TestChild
        {
            Ni = "CHILD456"
        };

        var output = new TestOutput
        {
            Children = [child]
        };

        // Act
        output.SetNi("ROOT123");

        // Assert
        child.Ni.Should().Be("CHILD456");
    }

    [Fact]
    public void SetNi_ShouldPopulateMultipleChildren()
    {
        // Arrange
        var children = new List<TestChild>
        {
            new() { Ni = "" },
            new() { Ni = "" },
            new() { Ni = "" }
        };

        var output = new TestOutput
        {
            Children = children
        };

        // Act
        output.SetNi("ROOT123");

        // Assert
        children.Should().OnlyContain(x => x.Ni == "ROOT123");
    }

    [Fact]
    public void SetNi_ShouldHandleEmptyChildrenCollection()
    {
        // Arrange
        var output = new TestOutput();

        // Act
        var action = () => output.SetNi("ROOT123");

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void SetNi_ShouldModifyOriginalChildInstances()
    {
        // Arrange
        var child = new TestChild
        {
            Ni = string.Empty
        };

        var output = new TestOutput
        {
            Children = [child]
        };

        // Act
        output.SetNi("ROOT123");

        // Assert
        child.Ni.Should().Be("ROOT123");
    }
}
