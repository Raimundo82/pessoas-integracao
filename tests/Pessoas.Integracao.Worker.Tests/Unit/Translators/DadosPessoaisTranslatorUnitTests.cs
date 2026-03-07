using FluentAssertions;

using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

namespace Pessoas.Integracao.Worker.Tests.Unit.Translators;

public sealed class DadosPessoaisTranslatorUnitTests
{
    private readonly DadosPessoaisTranslator _sut = new();

    [Fact]
    public void ShouldMapNomeToNomeCompleto()
    {
        // Arrange
        var output = OutputWithPessoais(new ZhrSPessoais { Nome = "João" });

        // Act
        var result = _sut.Translate(output);

        // Assert
        result!.NomeCompleto.Should().Be("João");
    }

    [Fact]
    public void ShouldMapApelidoToSobrenome()
    {
        // Arrange
        var output = OutputWithPessoais(new ZhrSPessoais { Apelido = "Silva" });

        // Act
        var result = _sut.Translate(output);

        // Assert
        result!.Sobrenome.Should().Be("Silva");
    }

    [Fact]
    public void ShouldMapRufnmToApelidos()
    {
        // Arrange
        var output = OutputWithPessoais(new ZhrSPessoais { Rufnm = "Joãozinho" });

        // Act
        var result = _sut.Translate(output);

        // Assert
        result!.Apelidos.Should().Be("Joãozinho");
    }

    [Fact]
    public void ShouldParseValidDtNasciToDataNascimento()
    {
        // Arrange
        var output = OutputWithPessoais(new ZhrSPessoais { DtNasci = "1985-03-15" });

        // Act
        var result = _sut.Translate(output);

        // Assert
        result!.DataNascimento.Should().Be(new DateOnly(1985, 3, 15));
    }

    [Fact]
    public void ShouldReturnNullDataNascimento_WhenDtNasciIsNull()
    {
        // Arrange
        var output = OutputWithPessoais(new ZhrSPessoais { DtNasci = null });

        // Act
        var result = _sut.Translate(output);

        // Assert
        result!.DataNascimento.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNullDataNascimento_WhenDtNasciIsEmpty()
    {
        // Arrange
        var output = OutputWithPessoais(new ZhrSPessoais { DtNasci = "" });

        // Act
        var result = _sut.Translate(output);

        // Assert
        result!.DataNascimento.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnNullDataNascimento_WhenDtNasciIsNotValidOrInconsistent()
    {
        // Arrange
        var output = OutputWithPessoais(new ZhrSPessoais { DtNasci = "some invalid date" });

        // Act
        var result = _sut.Translate(output);

        // Assert
        result!.DataNascimento.Should().BeNull();
    }

    [Fact]
    public void ShouldReturnEmptyDadosPessoais_WhenPessoaisIsNull()
    {
        // Arrange
        var output = new ZhrSPessoaisOutput { Pessoais = null };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().BeEquivalentTo(new DadosPessoais());
    }

    [Fact]
    public void ShouldReturnEmptyDadosPessoais_WhenPessoaisIsEmpty()
    {
        // Arrange
        var output = new ZhrSPessoaisOutput { Pessoais = [] };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().BeEquivalentTo(new DadosPessoais());
    }

    [Fact]
    public void ShouldReturnTheSingleElement_WhenPessoaisContainsSingleElement()
    {
        // Arrange
        var output = new ZhrSPessoaisOutput { Pessoais = [new ZhrSPessoais { Nome = "Ernesto" }] };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().NotBeNull();
        result.Should().Match<DadosPessoais>(dados => dados.NomeCompleto == "Ernesto");
    }

    [Fact]
    public void ShouldReturnLastElement_WhenPessoaisContainsMultipleElements()
    {
        // Arrange
        var output = new ZhrSPessoaisOutput
        {
            Pessoais = [
                new ZhrSPessoais { Nome = "Ernesto" },
                new ZhrSPessoais { Nome = "Ernestina" },
            ]
        };

        // Act
        var result = _sut.Translate(output);

        // Assert
        result.Should().NotBeNull();
        result.Should().Match<DadosPessoais>(dados => dados.NomeCompleto == "Ernestina");
    }

    private static ZhrSPessoaisOutput OutputWithPessoais(ZhrSPessoais pessoais) =>
        new() { Pessoais = [pessoais] };
}