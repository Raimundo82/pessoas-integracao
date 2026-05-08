using FluentAssertions;

using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

namespace Pessoas.Integracao.Worker.Tests.Unit.Translators;

public sealed class ColocacoesTranslatorUnitTests
{
    private readonly ColocacoesTranslator _sut = new();

    [Fact]
    public void ShouldMapTempoServicoToColocacao()
    {
        // Arrange
        var output = new ZhrSTemposervOutput
        {
            TempoServico = [
                new ZhrSTemposerv
                {
                    Zzunid = "UNIT1",
                    Datainicio = "2020-01-01",
                    Datafim = "2021-01-01"
                }
            ]
        };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().HaveCount(1);
        result[0].ExternalReference.Should().Be(new UnidadeExternaRef("UNIT1"));
        result[0].Inicio.Should().Be(new DateTime(2020, 1, 1));
        result[0].Fim.Should().Be(new DateTime(2021, 1, 1));
    }

    [Fact]
    public void ShouldMapNullFim_WhenDatafimIsEmptyOrNull()
    {
        // Arrange
        var output = new ZhrSTemposervOutput
        {
            TempoServico = [
                new ZhrSTemposerv
                {
                    Zzunid = "UNIT1",
                    Datainicio = "2020-01-01",
                    Datafim = null
                },
                new ZhrSTemposerv
                {
                    Zzunid = "UNIT2",
                    Datainicio = "2020-01-01",
                    Datafim = ""
                },
                new ZhrSTemposerv
                {
                    Zzunid = "UNIT3",
                    Datainicio = "2020-01-01",
                    Datafim = "   "
                }
            ]
        };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().HaveCount(3);
        result[0].Fim.Should().BeNull();
        result[1].Fim.Should().BeNull();
        result[2].Fim.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnEmptyList_WhenOutputIsNull()
    {
        // Arrange
        ZhrSTemposervOutput? output = null;

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnEmptyList_WhenTempoServicoIsNull()
    {
        // Arrange
        var output = new ZhrSTemposervOutput { TempoServico = null };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnEmptyList_WhenTempoServicoIsEmpty()
    {
        // Arrange
        var output = new ZhrSTemposervOutput { TempoServico = [] };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldMapMultipleItems_WhenTempoServicoHasMultipleElements()
    {
        // Arrange
        var output = new ZhrSTemposervOutput
        {
            TempoServico = [
                new ZhrSTemposerv { Zzunid = "UNIT1", Datainicio = "2010-01-01", Datafim = "2011-01-01" },
                new ZhrSTemposerv { Zzunid = "UNIT2", Datainicio = "2012-01-01", Datafim = "2013-01-01" }
            ]
        };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().HaveCount(2);
        result[0].ExternalReference.Should().Be(new UnidadeExternaRef("UNIT1"));
        result[1].ExternalReference.Should().Be(new UnidadeExternaRef("UNIT2"));
    }

    [Fact]
    public void ShouldThrowException_WhenDateIsInvalid()
    {
        // Arrange
        var output = new ZhrSTemposervOutput
        {
            TempoServico = [
                new ZhrSTemposerv { Zzunid = "UNIT1", Datainicio = "invalid-date", Datafim = "2021-01-01" }
            ]
        };

        // Act & Assert
        Assert.Throws<FormatException>(() => _sut.Translate(output));
    }
}
