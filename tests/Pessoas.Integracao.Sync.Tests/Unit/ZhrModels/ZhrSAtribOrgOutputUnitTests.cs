using FluentAssertions;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Sync.Tests.Unit.ZhrModels;

public class ZhrSAtribOrgOutputTests
{
    [Fact]
    public void GetChildren_ShouldReturnEmpty_WhenAllCollectionsAreNull()
    {
        // Arrange
        var output = new ZhrSAtribOrgOutput();

        // Act
        var result = output.GetChildrenFlattened();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnChildrenFromAllCollections_WhenAllCollectionsArePopulated()
    {
        // Arrange
        var atribOrg = new ZhrSAtribOrg { Ni = "NI1" };
        var monitPrazos = new ZhrSMonitPrazos { Ni = "NI2" };
        var dataMedida = new ZhrSDataMedida { Ni = "NI3" };
        var om = new ZhrSOm { Ni = "NI4" };
        var classifProf = new ZhrSClassifProf { Ni = "NI5" };

        var output = new ZhrSAtribOrgOutput
        {
            AtribOrg = [atribOrg],
            MonitPrazos = [monitPrazos],
            DataMedida = [dataMedida],
            Om = [om],
            ClassifProf = [classifProf]
        };

        // Act
        var result = output.GetChildrenFlattened();

        // Assert
        result.Should().HaveCount(5);

        result.Should().Contain(atribOrg);
        result.Should().Contain(monitPrazos);
        result.Should().Contain(dataMedida);
        result.Should().Contain(om);
        result.Should().Contain(classifProf);
    }

    [Fact]
    public void ShouldFlattenAllChildCollections_WhenMultipleCollectionsContainChildren()
    {
        // Arrange
        var output = new ZhrSAtribOrgOutput
        {
            AtribOrg =
            [
                new ZhrSAtribOrg { Ni = "A1" },
                new ZhrSAtribOrg { Ni = "A2" }
            ],
            Om =
            [
                new ZhrSOm { Ni = "O1" }
            ]
        };

        // Act
        var result = output.GetChildrenFlattened();

        // Assert
        result.Should().HaveCount(3);
    }
}
