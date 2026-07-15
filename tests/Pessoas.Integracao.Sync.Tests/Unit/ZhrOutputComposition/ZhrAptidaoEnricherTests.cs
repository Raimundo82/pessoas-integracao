namespace Pessoas.Integracao.Sync.Tests.Unit.ZhrOutputComposition;

using FluentAssertions;

using Moq;

using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Contracts;
using Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

public sealed class ZhrAptidaoEnricherTests
{
    [Fact]
    public async Task ShouldPopulateAllAptidaoLists_WhenMatchingRecordsExistInDb()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "0001", ExternalId = "30001" } };
        var aptidoesFetcher = new List<ZhrSAptidao> { new() { Ni = "0001", ArexamesDesc = "Test Aptidao" } };

        var zhrFetcherByNiMock = new Mock<IZhrFetcherByNi>();
        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSAptidao>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(aptidoesFetcher);

        var zhrOutputs = new List<ZhrOutput> { new() { Ni = "0001", ExternalId = "30001" } };
        var uut = new ZhrAptidaoEnricher(zhrFetcherByNiMock.Object);

        // Act
        var result = await uut.EnrichAsync(pessoaSyncRefs, zhrOutputs, TestContext.Current.CancellationToken);

        // Assert
        zhrFetcherByNiMock.Verify(x => x.ExecuteAsync<ZhrSAptidao>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        result.Should().ContainSingle();
        result[0].Aptidoes.Should().ContainSingle().Which.Should().BeEquivalentTo(aptidoesFetcher[0]);
    }

    [Fact]
    public async Task ShouldPopulateAptidoesAndPreserveExistingData_WhenMatchingRecordsExistInDb()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "0001", ExternalId = "30001" } };
        var aptidoesFetcher = new List<ZhrSAptidao> { new() { Ni = "0001", ArexamesDesc = "Test Aptidao" } };

        var zhrFetcherByNiMock = new Mock<IZhrFetcherByNi>();
        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSAptidao>(
                pessoaSyncRefs,
                ct: TestContext.Current.CancellationToken))
            .ReturnsAsync(aptidoesFetcher);

        var zhrOutputs = new List<ZhrOutput>
        {
            new()
            {
                Ni = "0001",
                ExternalId = "30001",
                Pessoais = [new() {  Ni = "0001", Nome = "Test User"}],
            }
        };

        var uut = new ZhrAptidaoEnricher(zhrFetcherByNiMock.Object);

        // Act
        var result = await uut.EnrichAsync(pessoaSyncRefs, zhrOutputs, TestContext.Current.CancellationToken);

        // Assert
        zhrFetcherByNiMock.Verify(x => x.ExecuteAsync<ZhrSAptidao>(
            pessoaSyncRefs,
            ct: TestContext.Current.CancellationToken),
            Times.Once);

        result.Should().ContainSingle();
        result[0].Aptidoes.Should().ContainSingle().Which.Should().BeEquivalentTo(aptidoesFetcher[0]);
    }
}
