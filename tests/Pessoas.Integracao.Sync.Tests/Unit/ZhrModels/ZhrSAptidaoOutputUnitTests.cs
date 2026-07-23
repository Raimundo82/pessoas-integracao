using FluentAssertions;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Tests.Unit.ZhrModels;

public class ZhrSAptidaoOutputTests
{
    [Fact]
    public void GetChildren_ShouldReturnEmpty_WhenAptidaoIsNull()
    {
        // Arrange
        var output = new ZhrSAptidaoOutput();

        // Act
        var result = output.GetChildren();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetChildren_ShouldReturnAllAptidaoChildren()
    {
        // Arrange
        var child1 = new ZhrSAptidao { Ni = "NI1" };
        var child2 = new ZhrSAptidao { Ni = "NI2" };

        var output = new ZhrSAptidaoOutput
        {
            Aptidao =
            [
                child1,
                child2
            ]
        };

        // Act
        var result = output.GetChildren();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(child1);
        result.Should().Contain(child2);
    }

    [Fact]
    public void GetChildren_ShouldReturnOnlyAptidaoChildren()
    {
        // Arrange
        var output = new ZhrSAptidaoOutput
        {
            Aptidao =
            [
                new ZhrSAptidao { Ni = "NI1" },
                new ZhrSAptidao { Ni = "NI2" }
            ]
        };

        // Act
        var result = output.GetChildren();

        // Assert
        result.Should().OnlyContain(x => x is ZhrSAptidao);
    }
}

