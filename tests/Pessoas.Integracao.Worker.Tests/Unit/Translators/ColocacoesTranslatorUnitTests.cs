using FluentAssertions;

using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

namespace Pessoas.Integracao.Worker.Tests.Unit.Translators;

public sealed class ColocacoesTranslatorUnitTests
{
    private readonly ColocacoesTranslator _sut = new();

    [Fact]
    public void ShouldMapAtribOrgToColocacao()
    {
        // Arrange
        var output = new ZhrSAtribOrgOutput
        {
            AtribOrg = [
                new ZhrSAtribOrg {
                    Unid = "123456",
                    DescUni = "Unit 1",
                    Abunid  = "U1",
                    Datapresenta = "2020-10-01"
                }
            ],
        };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().HaveCount(1);
        result[0].ExternalReference.Should().Be(new UnidadeExternaRef("123456"));
        result[0].Inicio.Should().Be(new DateTime(2020, 10, 1));
    }

    [Fact]
    public void ShouldMapInicio_WhenDatapresentaIsValid()
    {
        // Arrange
        var output = new ZhrSAtribOrgOutput
        {
            AtribOrg = [
                new ZhrSAtribOrg
                {
                    Unid = "UNIT-0001",
                    DescUni = "Unit 1",
                    Abunid = "U1",
                    Datapresenta = "2020-01-01"
                },
                new ZhrSAtribOrg
                {
                    Unid = "UNIT-0002",
                    DescUni = "Unit 2",
                    Abunid = "U2",
                    Datapresenta = "2020-02-01"
                },
                new ZhrSAtribOrg
                {
                    Unid = "UNIT-0003",
                    DescUni = "Unit 3",
                    Abunid = "U3",
                    Datapresenta = "2020-03-01"
                }
            ]
        };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().HaveCount(3);
        result[0].Inicio.Should().Be(new DateTime(2020, 1, 1));
        result[1].Inicio.Should().Be(new DateTime(2020, 2, 1));
        result[2].Inicio.Should().Be(new DateTime(2020, 3, 1));
    }

    [Fact]
    public void ShouldReturnEmptyList_WhenOutputIsNull()
    {
        // Arrange
        ZhrSAtribOrgOutput? output = null;

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnEmptyList_WhenAtribOrgIsNull()
    {
        // Arrange
        var output = new ZhrSAtribOrgOutput { AtribOrg = null };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnEmptyList_WhenAtribOrgIsEmpty()
    {
        // Arrange
        var output = new ZhrSAtribOrgOutput { AtribOrg = [] };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldMapMultipleItems_WhenAtribOrgHasMultipleElements()
    {
        // Arrange
        var output = new ZhrSAtribOrgOutput
        {
            AtribOrg = [
                new ZhrSAtribOrg
                {
                    Unid = "UNIT-0010",
                    DescUni = "Unit 10",
                    Abunid = "U10",
                    Datapresenta = "2010-01-01"
                },
                new ZhrSAtribOrg
                {
                    Unid = "UNIT-0011",
                    DescUni = "Unit 11",
                    Abunid = "U11",
                    Datapresenta = "2012-01-01"
                }
            ]
        };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().HaveCount(2);
        result[0].ExternalReference.Should().Be(new UnidadeExternaRef("UNIT-0010"));
        result[1].ExternalReference.Should().Be(new UnidadeExternaRef("UNIT-0011"));
    }

    [Fact]
    public void ShouldThrowException_WhenDateIsInvalid()
    {
        // Arrange
        var output = new ZhrSAtribOrgOutput
        {
            AtribOrg = [
                new ZhrSAtribOrg
                {
                    Unid = "UNIT-ERROR",
                    DescUni = "Invalid Unit",
                    Abunid = "INV",
                    Datapresenta = "invalid-date"
                }
            ]
        };

        // Act & Assert
        Assert.Throws<FormatException>(() => _sut.Translate(output));
    }
}
