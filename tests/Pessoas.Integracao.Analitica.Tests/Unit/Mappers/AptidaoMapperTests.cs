using FluentAssertions;

using Pessoas.Integracao.Analitica.Infrastructure.Mappers;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;

namespace Pessoas.Integracao.Analitica.Tests.Unit.Mappers;

public sealed class AptidaoMapperTests
{
    private readonly AptidaoMapper _aptidaoMapper = new();

    [Fact]
    public Task ShouldMapAllFields_WhenSourceIsFullyPopulated()
    {
        // Arrange
        var source = new ZhrSAptidao
        {
            Id = 999,
            Ni = "20002",
            Subty = "0001",
            Denominacao = "Aptidão Física",
            AreaExame = "Cardiologia",
            ArexamesDesc = "Exame Cardiológico",
            ServicoMedInt = "Serviço Médico Interno",
            Valor = 18.5m,
            DataExame = "2026-01-15",
            Modalidade = "Presencial",
            ModalDesc = "Exame presencial",
            Resultado = "Apto",
            ResultadoDesc = "Apto para o serviço",
            Observacoes = "Sem observações relevantes"
        };

        // Act
        var result = _aptidaoMapper.Map(source);

        // Assert
        return Verify(result);
    }

    [Fact]
    public void ShouldNotCopyIdFromSource()
    {
        // Arrange
        var source = new ZhrSAptidao { Ni = "20002", Id = 999 };

        // Act
        var result = _aptidaoMapper.Map(source);

        // Assert
        result.Id.Should().Be(0);
    }

    [Fact]
    public void ShouldConvertValorFromDecimalToString_RegardlessOfServerCulture()
    {
        // Arrange
        var source = new ZhrSAptidao { Ni = "20002", Valor = 18.5m };

        var result = _aptidaoMapper.Map(source);

        result.Valor.Should().Be("18.5");
    }

    [Fact]
    public Task ShouldMapFieldsAsNull_WhenSourceFieldsAreNull()
    {
        // Arrange
        var source = new ZhrSAptidao { Ni = "20002" };

        // Act
        var result = _aptidaoMapper.Map(source);

        // Assert
        return Verify(result);
    }
}
