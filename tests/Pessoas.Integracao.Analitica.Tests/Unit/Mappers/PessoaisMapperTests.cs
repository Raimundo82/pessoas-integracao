using FluentAssertions;

using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Tests.Unit.Mappers;

public sealed class PessoaisMapperTests
{
    private readonly PessoaisMapper _pessoaisMapper = new();

    [Fact]
    public Task ShouldMapAllFields_WhenSourceIsFullyPopulated()
    {
        // Arrange
        var source = new ZhrSPessoais
        {
            Id = 999,
            Ni = "20002",
            Nome = "João Silva",
            Apelido = "Silva",
            Sexo = "M",
            SexoDesc = "Masculino",
            DtNasci = "1990-05-15",
            Idade = "36",
            Idade31dezembro = "36",
            Nacionalidade1 = "PT",
            Nacionalidade2 = "PT",
            Nacionalidade3 = "PT",
            PaisNat = "PT",
            PaisnascDesc = "Portugal",
            DistritoNat = "11",
            DistnascDesc = "Lisboa",
            ConcelhoNat = "1101",
            ConcnascDesc = "Lisboa",
            FreguesiaNat = "110101",
            FregnascDesc = "Alcântara",
            EstCivil = "S",
            EstadocivilDesc = "Solteiro",
            DataEstCivil = "1990-05-15",
            Rufnm = "João",
            DtFalec = ""
        };

        // Act
        var result = _pessoaisMapper.Map(source);

        // Assert
        return Verify(result);
    }

    [Fact]
    public void ShouldNotCopyIdFromSource()
    {
        // Arrange
        var source = new ZhrSPessoais { Ni = "20002", Id = 999 };

        // Act
        var result = _pessoaisMapper.Map(source);

        // Assert
        result.Id.Should().Be(0);
    }

    [Fact]
    public void ShouldNotCopyNumsapFromSource()
    {
        // Arrange
        var source = new ZhrSPessoais { Ni = "20002" };

        // Act
        var result = (ZhrWsPersonalDataPessoai)_pessoaisMapper.Map(source);

        // Assert
        result.Numsap.Should().BeNull();
    }

    [Fact]
    public Task ShouldMapFieldsAsNull_WhenSourceFieldsAreNull()
    {
        // Arrange
        var source = new ZhrSPessoais { Ni = "20002" };

        // Act
        var result = _pessoaisMapper.Map(source);

        // Assert
        return Verify(result);
    }
}
