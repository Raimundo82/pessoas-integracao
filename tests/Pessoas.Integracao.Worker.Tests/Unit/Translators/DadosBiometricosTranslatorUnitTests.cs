using FluentAssertions;

using Microsoft.Extensions.Options;

using Pessoas.Integracao.Core.Domain.Enums;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

namespace Pessoas.Integracao.Worker.Tests.Unit.Translators;

public sealed class DadosBiometricosTranslatorUnitTests
{
    private readonly DadosBiometricosTranslator _sut;

    public DadosBiometricosTranslatorUnitTests()
    {
        var options = Options.Create(_config);
        _sut = new DadosBiometricosTranslator(options);
    }

    [Fact]
    public void ShouldMapAltura_WhenAlturaIsPresentAndCorrectSubty()
    {
        var output = OutputWithExames(
            new ZhrSExames
            {
                Subty = _config.Subty,
                AreaExame = _config.Altura,
                Valor = 180
            }
        );

        var result = _sut.Translate(output);

        result!.AlturaEmCm.Should().Be(180);
    }

    [Fact]
    public void ShouldReturnNullForAltura_WhenAlturaIsPresentAndNoSubty()
    {
        var output = OutputWithExames(
            new ZhrSExames
            {
                AreaExame = _config.Altura,
                Valor = 180
            }
        );

        var result = _sut.Translate(output);

        result!.AlturaEmCm.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNullForAltura_WhenAlturaIsPresentAndNonMatchSubty()
    {
        var output = OutputWithExames(
            new ZhrSExames
            {
                Subty = "1111",
                AreaExame = _config.Altura,
                Valor = 180
            }
        );

        var result = _sut.Translate(output);

        result!.AlturaEmCm.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNullAltura_WhenNotPresent()
    {
        var output = OutputWithExames(
            new ZhrSExames
            {
                Subty = _config.Subty,
                AreaExame = _config.CorOlhos,
                ArexamesDesc = "Castanhos"
            }
        );

        var result = _sut.Translate(output);

        result!.AlturaEmCm.Should().BeNull();
    }

    [Fact]
    public void ShouldMapCorDosOlhos_WhenPresent()
    {
        var output = OutputWithExames(
            new ZhrSExames
            {
                Subty = _config.Subty,
                AreaExame = _config.CorOlhos,
                ModalDesc = "Azuis"
            }
        );

        var result = _sut.Translate(output);

        result!.CorDosOlhos.Should().Be("Azuis");
    }

    [Fact]
    public void ShouldReturnNullCorDosOlhos_WhenNotPresent()
    {
        var output = OutputWithExames(
            new ZhrSExames
            {
                Subty = _config.Subty,
                AreaExame = _config.Altura,
                Valor = 170
            }
        );

        var result = _sut.Translate(output);

        result!.CorDosOlhos.Should().BeNull();
    }

    [Fact]
    public void ShouldMapGrupoSanguineo_WhenValid()
    {
        var output = OutputWithExames(
            new ZhrSExames
            {
                Subty = _config.Subty,
                AreaExame = _config.GrupoSanguineo,
                ModalDesc = "AB"
            }
        );

        var result = _sut.Translate(output);

        result!.TipoDeSangue!.GrupoSanguineo.Should().Be(GrupoSanguineo.AB);
    }

    [Fact]
    public void ShouldReturnNullGrupoSanguineo_WhenInvalid()
    {
        var output = OutputWithExames(
            new ZhrSExames
            {
                Subty = _config.Subty,
                AreaExame = _config.GrupoSanguineo,
                ArexamesDesc = "INVALIDO"
            }
        );

        var result = _sut.Translate(output);

        result!.TipoDeSangue!.GrupoSanguineo.Should().BeNull();
    }

    [Fact]
    public void ShouldMapRhesus_WhenValid()
    {
        var output = OutputWithExames(
            new ZhrSExames
            {
                Subty = _config.Subty,
                AreaExame = _config.Rhesus,
                ModalDesc = "POSITIVO"
            }
        );

        var result = _sut.Translate(output);

        result!.TipoDeSangue!.Rhesus.Should().Be(Rhesus.POSITIVO);
    }

    [Fact]
    public void ShouldReturnNullRhesus_WhenInvalid()
    {
        var output = OutputWithExames(
            new ZhrSExames { AreaExame = _config.Rhesus, ArexamesDesc = "???" }
        );

        var result = _sut.Translate(output);

        result!.TipoDeSangue!.Rhesus.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNull_WhenExamesIsNull()
    {
        var output = new ZhrSExamesMedOutput { Exames = null };

        var result = _sut.Translate(output);

        result.Should().NotBeNull();
        result!.AlturaEmCm.Should().BeNull();
        result.CorDosOlhos.Should().BeNull();
        result.TipoDeSangue.Should().NotBeNull();
        result.TipoDeSangue!.GrupoSanguineo.Should().BeNull();
        result.TipoDeSangue!.Rhesus.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNull_WhenExamesIsEmpty()
    {
        var output = new ZhrSExamesMedOutput { Exames = [] };

        var result = _sut.Translate(output);

        result.Should().NotBeNull();
        result!.AlturaEmCm.Should().BeNull();
        result.CorDosOlhos.Should().BeNull();
        result.TipoDeSangue.Should().NotBeNull();
        result.TipoDeSangue!.GrupoSanguineo.Should().BeNull();
        result.TipoDeSangue!.Rhesus.Should().BeNull();
    }

    [Fact]
    public void ShouldPickCorrectValues_WhenMultipleExamsExist()
    {
        var output = new ZhrSExamesMedOutput
        {
            Exames =
            [
                new ZhrSExames {Subty = _config.Subty, AreaExame = _config.Altura, Valor = 175 },
                new ZhrSExames {Subty = _config.Subty, AreaExame = _config.CorOlhos, ModalDesc = "Verdes" },
                new ZhrSExames {Subty = _config.Subty, AreaExame = _config.GrupoSanguineo, ModalDesc = "O" },
                new ZhrSExames {Subty = _config.Subty, AreaExame = _config.Rhesus, ModalDesc = "Negativo" }
            ]
        };

        var result = _sut.Translate(output);

        result.Should().NotBeNull();
        result!.AlturaEmCm.Should().Be(175);
        result.CorDosOlhos.Should().Be("Verdes");
        result.TipoDeSangue!.GrupoSanguineo.Should().Be(GrupoSanguineo.O);
        result.TipoDeSangue!.Rhesus.Should().Be(Rhesus.NEGATIVO);
    }

    private static ZhrSExamesMedOutput OutputWithExames(ZhrSExames exame) =>
        new() { Exames = [exame] };

    private readonly SigdnRhExamesMedConfig _config = new()
    {
        Altura = "01",
        CorOlhos = "05",
        GrupoSanguineo = "03",
        Rhesus = "04",
        Subty = "9000"
    };
}