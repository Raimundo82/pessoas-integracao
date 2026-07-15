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

        var zhrFetcherByNiMock = new Mock<IZhrFetcherByNi>();
        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSPessoais>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(pessoaisFetcher);

        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSFamilia>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(familiaFetcher);

        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSOutrosdados>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(outrosDadosFetcher);

        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSDeficiencias>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(deficienciasFetcher);


        var zhrOutputs = new List<ZhrOutput> { new() { Ni = "0001", ExternalId = "30001" } };
        var uut = new ZhrPersonalDataEnricher(zhrFetcherByNiMock.Object);

        // Act
        var results = await uut.EnrichAsync(pessoaSyncRefs, zhrOutputs, TestContext.Current.CancellationToken);

        // Assert
        zhrFetcherByNiMock.Verify(x => x.ExecuteAsync<ZhrSPessoais>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        zhrFetcherByNiMock.Verify(x => x.ExecuteAsync<ZhrSFamilia>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        zhrFetcherByNiMock.Verify(x => x.ExecuteAsync<ZhrSOutrosdados>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        zhrFetcherByNiMock.Verify(x => x.ExecuteAsync<ZhrSDeficiencias>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        results.Should().ContainSingle();
        await Verify(results);
    }

    [Fact]
    public async Task ShouldPopulatePersonalDataAndPreserveExistingData_WhenMatchingRecordsExistInDb()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "0001", ExternalId = "30001" } };
        var pessoaisFetcher = new List<ZhrSPessoais> { new() { Ni = "0001", Nome = "Test User" } };
        var familiaFetcher = new List<ZhrSFamilia> { new() { Ni = "0001", Fanam = "Test Familia" } };
        var outrosDadosFetcher = new List<ZhrSOutrosdados> { new() { Ni = "0001", Aus10 = "Test" } };
        var deficienciasFetcher = new List<ZhrSDeficiencias> { new() { Ni = "0001", Descricao = "Test Deficiencia" } };


        var zhrFetcherByNiMock = new Mock<IZhrFetcherByNi>();
        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSPessoais>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(pessoaisFetcher);

        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSFamilia>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(familiaFetcher);

        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSOutrosdados>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(outrosDadosFetcher);

        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSDeficiencias>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(deficienciasFetcher);

        var zhrOutputs = new List<ZhrOutput>
        {
            new()
            {
                Ni = "0001",
                ExternalId = "30001",
                Aptidoes = [new() {  Ni = "0001", AreaExame = "Test Aptidao"}],
            }
        };

        var uut = new ZhrPersonalDataEnricher(zhrFetcherByNiMock.Object);

        // Act
        var results = await uut.EnrichAsync(pessoaSyncRefs, zhrOutputs, TestContext.Current.CancellationToken);

        // Assert
        zhrFetcherByNiMock.Verify(x => x.ExecuteAsync<ZhrSPessoais>(
                    pessoaSyncRefs,
                    ct: TestContext.Current.CancellationToken),
                    Times.Once);

        zhrFetcherByNiMock.Verify(x => x.ExecuteAsync<ZhrSFamilia>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        zhrFetcherByNiMock.Verify(x => x.ExecuteAsync<ZhrSOutrosdados>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        zhrFetcherByNiMock.Verify(x => x.ExecuteAsync<ZhrSDeficiencias>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        results.Should().ContainSingle();
        await Verify(results);
    }
}
