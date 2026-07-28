using FluentAssertions;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

using Pessoas.Integracao.Sync.Tests.Unit.Helpers;

namespace Pessoas.Integracao.Sync.Tests.Unit.ZhrModels;

public class ZhrSBaseModelOutputTests
{

    [Fact]
    public void ShouldSetTimestamp_WhenSetUpdatedAtIsCalled()
    {
        // Arrange
        ZhrTestOutput output = GetZhrTestOutput();

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
        ZhrTestOutput output = GetZhrTestOutput();


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
        var child = new ZhrChildA
        {
            Ni = string.Empty
        };

        IOutputModel output = GetZhrTestOutput("ROOT123", [child]);

        // Act
        output.SetChildrenNi();

        // Assert
        child.Ni.Should().Be("ROOT123");
    }

    [Fact]
    public void ShouldPopulateNi_WhenChildNiIsWhitespace()
    {
        // Arrange
        var child = new ZhrChildA
        {
            Ni = "   "
        };

        IOutputModel output = GetZhrTestOutput("ROOT123", [child]);

        // Act
        output.SetChildrenNi();

        // Assert
        child.Ni.Should().Be("ROOT123");
    }

    [Fact]
    public void ShouldNotPreserveExistingNi_WhenChildNiIsAlreadySet()
    {
        // Arrange
        var child = new ZhrChildB
        {
            Ni = "CHILD456"
        };

        IOutputModel output = GetZhrTestOutput("ROOT123", [child]);

        // Act
        output.SetChildrenNi();

        // Assert
        child.Ni.Should().Be("ROOT123");
    }

    [Fact]
    public void ShouldPopulateNiForAllChildren_WhenMultipleChildrenArePresent()
    {
        // Arrange
        var children = new ZhrSBaseModel[]
        {
            new ZhrChildA { Ni = "" },
            new ZhrChildA { Ni = "" },
            new ZhrChildB { Ni = "" }
        };

        IOutputModel output = GetZhrTestOutput("ROOT123", children);


        // Act
        output.SetChildrenNi();

        // Assert
        children.Should().OnlyContain(x => x.Ni == "ROOT123");
    }

    [Fact]
    public void ShouldNotThrow_WhenChildrenCollectionIsEmpty()
    {
        // Arrange
        IOutputModel output = GetZhrTestOutput("ROOT123");

        // Act
        var action = () => output.SetChildrenNi();

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void ShouldModifyOriginalChildInstances_WhenSetNiIsCalled()
    {
        // Arrange
        var child = new ZhrChildA
        {
            Ni = string.Empty
        };

        IOutputModel output = GetZhrTestOutput("ROOT123", [child]);

        // Act
        output.SetChildrenNi();

        // Assert
        child.Ni.Should().Be("ROOT123");
    }

    private static ZhrTestOutput GetZhrTestOutput(string ni = "NI", ZhrSBaseModel[]? children = null)
    {
        return new ZhrTestOutput
        {
            Ni = ni,
            Numsap = "NUMSAP",
            Children = children ?? []
        };
    }
}
