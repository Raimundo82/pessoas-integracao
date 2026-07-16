
using FluentAssertions;

using Moq;

using Pessoas.Integracao.Sync.Application.Contracts;
using Pessoas.Integracao.Sync.Domain.Entities;
using Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition;
using Pessoas.Integracao.Sync.Infrastructure.Services.ZhrOutputComposition.Enrichers;

namespace Pessoas.Integracao.Sync.Tests.Unit.ZhrOutputComposition;

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
            .Returns((IReadOnlyList<PessoaSyncRef> refs, IReadOnlyList<ZhrOutput> zhrOutputs, CancellationToken ct) =>
            {
                zhrOutputs[0].Pessoais = [new() { Ni = "0001", Nome = "Test User" }];
                return Task.FromResult(zhrOutputs);
            });

        var secondEnricher = new Mock<IZhrOutputsEnricher>();
        secondEnricher
            .Setup(x => x.EnrichAsync(
                pessoaSyncRefs,
               It.IsAny<IReadOnlyList<ZhrOutput>>(),
                It.IsAny<CancellationToken>()))
            .Returns((IReadOnlyList<PessoaSyncRef> refs, IReadOnlyList<ZhrOutput> zhrOutputs, CancellationToken ct) =>
            {
                zhrOutputs[0].Familias = [new() { Ni = "0001", Fanam = "Test Family" }];
                return Task.FromResult(zhrOutputs);
            });

        var thirdEnricher = new Mock<IZhrOutputsEnricher>();
        thirdEnricher
            .Setup(x => x.EnrichAsync(
                pessoaSyncRefs,
               It.IsAny<IReadOnlyList<ZhrOutput>>(),
                It.IsAny<CancellationToken>()))
            .Returns((IReadOnlyList<PessoaSyncRef> refs, IReadOnlyList<ZhrOutput> zhrOutputs, CancellationToken ct) =>
            {
                zhrOutputs[0].Aptidoes = [new() { Ni = "0001", ArexamesDesc = "Test Aptidao" }];
                return Task.FromResult(zhrOutputs);
            });

        var composer = new ZhrOutputComposer([firstEnricher.Object, secondEnricher.Object, thirdEnricher.Object]);

        // Act
        var results = await composer.ComposeAsync(pessoaSyncRefs, TestContext.Current.CancellationToken);

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

    [Fact]
    public async Task ShouldPassTheSameListInstanceThroughAllEnrichers()
    {
        // Arrange
        var pessoaSyncRefs = new List<PessoaSyncRef> { new() { Ni = "0001", ExternalId = "30001" } };
        IReadOnlyList<ZhrOutput> capturedList = null!;

        var firstEnricher = new Mock<IZhrOutputsEnricher>();
        firstEnricher
            .Setup(x => x.EnrichAsync(
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<IReadOnlyList<ZhrOutput>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<PessoaSyncRef>, IReadOnlyList<ZhrOutput>, CancellationToken>((refs, list, ct) => capturedList = list)
            .ReturnsAsync((IReadOnlyList<PessoaSyncRef> refs, IReadOnlyList<ZhrOutput> list, CancellationToken ct) => list);

        var secondEnricher = new Mock<IZhrOutputsEnricher>();
        secondEnricher
            .Setup(x => x.EnrichAsync(
                It.IsAny<IReadOnlyList<PessoaSyncRef>>(),
                It.IsAny<IReadOnlyList<ZhrOutput>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<PessoaSyncRef> refs, IReadOnlyList<ZhrOutput> list, CancellationToken ct) => list);

        var composer = new ZhrOutputComposer([firstEnricher.Object, secondEnricher.Object]);

        // Act
        var results = await composer.ComposeAsync(pessoaSyncRefs, TestContext.Current.CancellationToken);

        // Assert
        secondEnricher.Verify(x => x.EnrichAsync(
            pessoaSyncRefs,
            capturedList,
            It.IsAny<CancellationToken>()),
            Times.Once);

        results.Should().BeSameAs(capturedList);
    }
}
