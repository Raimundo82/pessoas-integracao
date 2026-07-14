namespace Pessoas.Integracao.Sync.Tests.Unit.ZhrOutputComposition;

using FluentAssertions;

using Moq;

using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Application.ZhrModels.Dados;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition;

public sealed class ZhrOutputComposerTests
{
    [Fact]
    public async Task ShouldApplyAllEnrichersToTheSameOutputCollection()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "0001", ExternalId = "30001" } };

        var firstEnricher = new Mock<IZhrOutputsEnricher>();
        firstEnricher
            .Setup(x => x.EnrichAsync(
                pessoaSyncRefs,
                It.IsAny<IReadOnlyList<ZhrOutput>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<PessoaSyncRef>, IReadOnlyList<ZhrOutput>, CancellationToken>(
                (_, outputs, _) =>
                {
                    var mutableOutputs = (List<ZhrOutput>)outputs;
                    mutableOutputs[0].Pessoais.Add(new ZhrSPessoais { Ni = "0001", Nome = "Test User" });
                });

        var secondEnricher = new Mock<IZhrOutputsEnricher>();
        secondEnricher
            .Setup(x => x.EnrichAsync(
                pessoaSyncRefs,
                It.IsAny<IReadOnlyList<ZhrOutput>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<PessoaSyncRef>, IReadOnlyList<ZhrOutput>, CancellationToken>(
                (_, outputs, _) =>
                {
                    var mutableOutputs = (List<ZhrOutput>)outputs;
                    mutableOutputs[0].Familias.Add(new ZhrSFamilia { Ni = "0001", Fanam = "Test Family" });
                    mutableOutputs[0].Pessoais.Should().ContainSingle();
                });

        var thirdEnricher = new Mock<IZhrOutputsEnricher>();
        thirdEnricher
            .Setup(x => x.EnrichAsync(
                pessoaSyncRefs,
                It.IsAny<IReadOnlyList<ZhrOutput>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<PessoaSyncRef>, IReadOnlyList<ZhrOutput>, CancellationToken>(
                (_, outputs, _) =>
                {
                    var mutableOutputs = (List<ZhrOutput>)outputs;
                    mutableOutputs[0].Aptidoes.Add(new ZhrSAptidao { Ni = "0001" });
                    mutableOutputs[0].Familias.Should().ContainSingle();
                });

        var composer = new ZhrOutputComposer([firstEnricher.Object, secondEnricher.Object, thirdEnricher.Object]);

        // Act
        var results = await composer.Compose(pessoaSyncRefs, TestContext.Current.CancellationToken);

        // Assert
        firstEnricher.Verify(x => x.EnrichAsync(
            pessoaSyncRefs,
            It.IsAny<IReadOnlyList<ZhrOutput>>(),
            TestContext.Current.CancellationToken),
            Times.Once);

        secondEnricher.Verify(x => x.EnrichAsync(
            pessoaSyncRefs,
            It.IsAny<IReadOnlyList<ZhrOutput>>(),
            TestContext.Current.CancellationToken),
            Times.Once);

        thirdEnricher.Verify(x => x.EnrichAsync(
            pessoaSyncRefs,
            It.IsAny<IReadOnlyList<ZhrOutput>>(),
            TestContext.Current.CancellationToken),
            Times.Once);

        results.Should().ContainSingle();
        results[0].Pessoais.Should().ContainSingle();
        results[0].Familias.Should().ContainSingle();
        results[0].Aptidoes.Should().ContainSingle();
    }
}
