using FluentAssertions;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Tests.Unit.ZhrModels;

public class ZhrSPessoaisOutputTests
{
    [Fact]
    public void GetChildren_ShouldReturnEmpty_WhenAllCollectionsAreNull()
    {
        // Arrange
        var output = new ZhrSPessoaisOutput();

        // Act
        var result = output.GetChildren();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetChildren_ShouldReturnChildrenFromAllCollections()
    {
        // Arrange
        var pessoais = new ZhrSPessoais { Ni = "NI1" };
        var familia = new ZhrSFamilia { Ni = "NI2" };
        var outrosDados = new ZhrSOutrosdados { Ni = "NI3" };
        var deficiencias = new ZhrSDeficiencias { Ni = "NI4" };

        var output = new ZhrSPessoaisOutput
        {
            Pessoais = [pessoais],
            Familia = [familia],
            OutrosDados = [outrosDados],
            Deficiencias = [deficiencias]
        };

        // Act
        var result = output.GetChildren();

        // Assert
        result.Should().HaveCount(4);

        result.Should().Contain(pessoais);
        result.Should().Contain(familia);
        result.Should().Contain(outrosDados);
        result.Should().Contain(deficiencias);
    }

    [Fact]
    public void GetChildren_ShouldReturnAllExpectedChildTypes()
    {
        // Arrange
        var output = new ZhrSPessoaisOutput
        {
            Pessoais =
            [
                new ZhrSPessoais { Ni = "P1" }
            ],
            Familia =
            [
                new ZhrSFamilia { Ni = "F1" }
            ],
            OutrosDados =
            [
                new ZhrSOutrosdados { Ni = "O1" }
            ],
            Deficiencias =
            [
                new ZhrSDeficiencias { Ni = "D1" }
            ]
        };

        // Act
        var result = output.GetChildren();

        // Assert
        result.Should().Contain(x => x is ZhrSPessoais);
        result.Should().Contain(x => x is ZhrSFamilia);
        result.Should().Contain(x => x is ZhrSOutrosdados);
        result.Should().Contain(x => x is ZhrSDeficiencias);
    }
}
