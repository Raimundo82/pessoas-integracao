using FluentAssertions;

using Moq;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Clients;
using Pessoas.Integracao.Sync.Infrastructure.Providers.FetchResults;
using Pessoas.Integracao.Sync.Infrastructure.Strategies;

namespace Pessoas.Integracao.Sync.Tests.Unit.Strategies;

public class ZhrWsAptidaoFetcherConcreteStrategyUnitTests
{
    private readonly Mock<IZhrWsGenericClient> _clientMock = new();

    private ZhrWsAptidaoFetcherConcreteStrategy CreateSut() =>
        new(_clientMock.Object);

    private static List<PessoaSyncRef> SomeRefs() =>
    [
        new() { Ni = "21412", ExternalId = "30005902" }
    ];

    private void SetupResponse(ZhrWsAptidaoResponse1? response)
    {
        _clientMock
            .Setup(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>>(),
                It.IsAny<IReadOnlyCollection<PessoaSyncRef>>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    public static class ZhrAptidaoTestData
    {
        public static ZhrSAptidaoOutput ValidOutput() =>
            new()
            {
                Ni = "21412",
                Numsap = "30005902",
                Aptidao =
                [
                    new ZhrSAptidao
                    {
                        Ni = "21412",
                        Subty = "0001",
                        Denominacao = "Exame Médico Geral",
                        AreaExame = "01",
                        ArexamesDesc = "Medicina Geral",
                        ServicoMedInt = "S",
                        Valor = 15.50m,
                        DataExame = "2025-01-15",
                        Modalidade = "NORMAL",
                        ModalDesc = "Normal",
                        Resultado = "AP",
                        ResultadoDesc = "Apto",
                        Observacoes = "Sem restrições"
                    }
                ]
            };
    }


    [Fact]
    public async Task ShouldPopulateAptidaoOutputs_WhenResponseContainsData()
    {
        // Arrange
        var aptidao = ZhrAptidaoTestData.ValidOutput();

        SetupResponse(new ZhrWsAptidaoResponse1
        {
            ZhrWsAptidaoResponse = new ZhrWsAptidaoResponse
            {
                Output = [aptidao]
            }
        });

        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(
                        SomeRefs(),
                        null,
                        CancellationToken.None);

        // Assert
        result.Should().BeOfType<AptidaoFetchResult>();
        var aptidaoResult = (AptidaoFetchResult)result;
        await Verify(aptidaoResult.Data);
    }

    [Fact]
    public async Task ShouldPopulateEmptyAptidaoOutputs_WhenResponseIsNull()
    {
        // Arrange
        SetupResponse(null);

        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(
            SomeRefs(),
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<AptidaoFetchResult>();
        var aptidaoResult = (AptidaoFetchResult)result;
        aptidaoResult.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldPopulateEmptyAptidaoOutputs_WhenInnerResponseIsNull()
    {
        // Arrange
        SetupResponse(new ZhrWsAptidaoResponse1
        {
            ZhrWsAptidaoResponse = null
        });

        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(
            SomeRefs(),
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<AptidaoFetchResult>();
        var aptidaoResult = (AptidaoFetchResult)result;
        aptidaoResult.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldPopulateEmptyAptidaoOutputs_WhenOutputIsNull()
    {
        // Arrange
        SetupResponse(new ZhrWsAptidaoResponse1
        {
            ZhrWsAptidaoResponse = new ZhrWsAptidaoResponse
            {
                Output = null
            }
        });

        var sut = CreateSut();

        // Act
        var result = await sut.FetchAsync(
            SomeRefs(),
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeOfType<AptidaoFetchResult>();
        var aptidaoResult = (AptidaoFetchResult)result;
        aptidaoResult.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldPassSameRefsAndNullReferenceDateToClient()
    {
        // Arrange
        var refs = SomeRefs();

        SetupResponse(null);

        var sut = CreateSut();

        // Act
        await sut.FetchAsync(
            refs,
            null,
            CancellationToken.None);

        // Assert
        _clientMock.Verify(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>>(),
                refs,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldPassReferenceDateToClient()
    {
        // Arrange
        var refs = SomeRefs();
        var referenceDate = new DateOnly(2025, 01, 15);

        SetupResponse(null);

        var sut = CreateSut();

        // Act
        await sut.FetchAsync(
            refs,
            referenceDate,
            CancellationToken.None);

        // Assert
        _clientMock.Verify(c => c.CallAsync(
                It.IsAny<Func<zhr_wsClient, ZhrWsInputStruct[], Task<ZhrWsAptidaoResponse1?>>>(),
                refs,
                referenceDate,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
