using FluentAssertions;

using Moq;

using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Contracts;
using Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition;
using Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

namespace Pessoas.Integracao.Sync.Tests.Integration.Infrastructure.ZhrOutputComposition;

public sealed class ZhrOutputComposerIntegrationTests
{
    [Fact]
    public async Task ShouldComposeFullZhrOutput_WhenUsingConcreteEnrichers()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "0001", ExternalId = "30001" } };
        var zhrFetcherByNiMock = new Mock<IZhrFetcherByNi>();

        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSPessoais>(pessoaSyncRefs, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Ni = "0001", Nome = "Test User" }]);

        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSFamilia>(pessoaSyncRefs, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Ni = "0001", Fanam = "Test Family" }]);

        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSOutrosdados>(pessoaSyncRefs, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Ni = "0001", Aus10 = "Test Data" }]);

        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSDeficiencias>(pessoaSyncRefs, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Ni = "0001", Descricao = "Test Deficiency" }]);

        zhrFetcherByNiMock
            .Setup(x => x.ExecuteAsync<ZhrSAptidao>(pessoaSyncRefs, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new() { Ni = "0001", ArexamesDesc = "Test Aptidao" }]);

        var enrichers = new List<IZhrOutputsEnricher>
        {
            new ZhrPersonalDataEnricher(zhrFetcherByNiMock.Object),
            new ZhrAptidaoEnricher(zhrFetcherByNiMock.Object)
        };

        var composer = new ZhrOutputComposer(enrichers);

        // Act
        var results = await composer.ComposeAsync(pessoaSyncRefs, TestContext.Current.CancellationToken);

        // Assert
        results.Should().ContainSingle();
        var output = results[0];
        output.Ni.Should().Be("0001");
        output.ExternalId.Should().Be("30001");
        output.Pessoais.Should().ContainSingle().Which.Nome.Should().Be("Test User");
        output.Familias.Should().ContainSingle().Which.Fanam.Should().Be("Test Family");
        output.Aptidoes.Should().ContainSingle().Which.ArexamesDesc.Should().Be("Test Aptidao");
    }
}
