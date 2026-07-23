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
    public void ShouldSetTimestamp_WhenSetUpdatedAtIsCalled()
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
    public void ShouldOverwriteExistingTimestamp_WhenSetUpdatedAtIsCalledWithNewValue()
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
    public void ShouldPopulateNi_WhenChildNiIsEmpty()
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
    public void ShouldPopulateNi_WhenChildNiIsWhitespace()
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
    public void ShouldPreserveExistingNi_WhenChildNiIsAlreadySet()
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
    public void ShouldPopulateNiForAllChildren_WhenMultipleChildrenArePresent()
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
    public void ShouldNotThrow_WhenChildrenCollectionIsEmpty()
    {
        // Arrange
        var output = new TestOutput();

        // Act
        var action = () => output.SetNi("ROOT123");

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void ShouldModifyOriginalChildInstances_WhenSetNiIsCalled()
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
