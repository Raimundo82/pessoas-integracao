namespace Pessoas.Integracao.Sync.Tests.Unit.ZhrOutputComposition;

using FluentAssertions;

using Moq;

using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Contracts;
using Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

public sealed class ZhrPersonalDataEnricherTests
{

    [Fact]
    public async Task ShouldPopulateAllPersonalDataLists_WhenFetcherReturnsMatchingRecords()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "0001", ExternalId = "30001" } };
        var pessoaisFetcher = new List<ZhrSPessoais> { new() { Ni = "0001", Nome = "Test User" } };
        var familiaFetcher = new List<ZhrSFamilia> { new() { Ni = "0001", Fanam = "Test Familia" } };
        var outrosDadosFetcher = new List<ZhrSOutrosdados> { new() { Ni = "0001", Aus10 = "Test" } };
        var deficienciasFetcher = new List<ZhrSDeficiencias> { new() { Ni = "0001", Descricao = "Test Deficiencia" } };


        var zhrFetcherByBiMock = new Mock<IZhrFetcherByNi>();
        zhrFetcherByBiMock
            .Setup(x => x.ExecuteAsync<ZhrSPessoais>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(pessoaisFetcher);

        zhrFetcherByBiMock
            .Setup(x => x.ExecuteAsync<ZhrSFamilia>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(familiaFetcher);

        zhrFetcherByBiMock
            .Setup(x => x.ExecuteAsync<ZhrSOutrosdados>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(outrosDadosFetcher);

        zhrFetcherByBiMock
            .Setup(x => x.ExecuteAsync<ZhrSDeficiencias>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(deficienciasFetcher);


        var results = new List<ZhrOutput> { new() { Ni = "0001", ExternalId = "30001" } };
        var uut = new ZhrPersonalDataEnricher(zhrFetcherByBiMock.Object);

        // Act
        await uut.EnrichAsync(pessoaSyncRefs, results, TestContext.Current.CancellationToken);

        // Assert
        zhrFetcherByBiMock.Verify(x => x.ExecuteAsync<ZhrSPessoais>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        zhrFetcherByBiMock.Verify(x => x.ExecuteAsync<ZhrSFamilia>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        zhrFetcherByBiMock.Verify(x => x.ExecuteAsync<ZhrSOutrosdados>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        zhrFetcherByBiMock.Verify(x => x.ExecuteAsync<ZhrSDeficiencias>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        results.Should().ContainSingle();
        results[0].Pessoais.Should().ContainSingle().Which.Should().BeEquivalentTo(pessoaisFetcher[0]);
        results[0].Familias.Should().ContainSingle().Which.Should().BeEquivalentTo(familiaFetcher[0]);
        results[0].OutrosDados.Should().ContainSingle().Which.Should().BeEquivalentTo(outrosDadosFetcher[0]);
        results[0].Deficiencias.Should().ContainSingle().Which.Should().BeEquivalentTo(deficienciasFetcher[0]);
    }

}
