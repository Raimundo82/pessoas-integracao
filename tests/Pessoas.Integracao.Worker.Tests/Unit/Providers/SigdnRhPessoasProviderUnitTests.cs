using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.FragmentProviders;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Fragments;

namespace Pessoas.Integracao.Worker.Tests.Unit.Providers;

public sealed class SigdnRhPessoasProviderUnitTests
{
    private readonly Mock<IPessoaCoreDataProvider> _coreDataProvider = new();

    [Fact]
    public async Task ShouldCallCoreDataProviderWithSameImportKeys_WhenFetchingPessoas()
    {
        // Arrange
        var importKeys = new[]
        {
            new PessoaImportKey("22600", "30002697"),
            new PessoaImportKey("22700", "30002797")
        };

        _coreDataProvider.Setup(p => p.GetPessoaCoreDataAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, PessoaCoreDataFragment>
            {
                [importKeys[0]] = new PessoaCoreDataFragment(new DadosPessoais()),
                [importKeys[1]] = new PessoaCoreDataFragment(new DadosPessoais())
            });

        var provider = new SigdnRhPessoasProvider(_coreDataProvider.Object);

        // Act
        await provider.GetPessoasByImportKeysAsync(importKeys, default);

        // Assert
        _coreDataProvider.Verify(
            p => p.GetPessoaCoreDataAsync(importKeys, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldMapNiiAndExternalIdFromImportKeys_WhenBuildingPessoa()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("22600", "30002697") };

        _coreDataProvider.Setup(p => p.GetPessoaCoreDataAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, PessoaCoreDataFragment>
            {
                [importKeys[0]] = new PessoaCoreDataFragment(new DadosPessoais())
            });

        var provider = new SigdnRhPessoasProvider(_coreDataProvider.Object);

        // Act
        var pessoas = await provider.GetPessoasByImportKeysAsync(importKeys, default);

        // Assert
        pessoas.Should().ContainSingle();
        pessoas[0].NII.Should().Be("22600");
        pessoas[0].ExternalId.Should().Be("30002697");
    }

    [Fact]
    public async Task ShouldMapDadosPessoaisFromCoreDataFragment_WhenBuildingPessoa()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("22600", "30002697") };
        var expectedDadosPessoais = new DadosPessoais { NomeCompleto = "Nome Completo 1" };

        _coreDataProvider.Setup(p => p.GetPessoaCoreDataAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, PessoaCoreDataFragment>
            {
                [importKeys[0]] = new PessoaCoreDataFragment(expectedDadosPessoais)
            });

        var provider = new SigdnRhPessoasProvider(_coreDataProvider.Object);

        // Act
        var pessoas = await provider.GetPessoasByImportKeysAsync(importKeys, default);

        // Assert
        pessoas.Should().ContainSingle();
        pessoas[0].DadosPessoais.Should().BeSameAs(expectedDadosPessoais);
    }

    [Fact]
    public async Task ShouldReturnReadOnlyList_WhenPessoasAreMapped()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("22600", "30002697") };

        _coreDataProvider.Setup(p => p.GetPessoaCoreDataAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, PessoaCoreDataFragment>
            {
                [importKeys[0]] = new PessoaCoreDataFragment(new DadosPessoais())
            });

        var provider = new SigdnRhPessoasProvider(_coreDataProvider.Object);

        // Act
        var pessoas = await provider.GetPessoasByImportKeysAsync(importKeys, default);

        // Assert
        var mutableListView = (IList<Pessoa>)pessoas;
        Action addPessoa = () => mutableListView.Add(new Pessoa { NII = "1", ExternalId = "1" });
        addPessoa.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public async Task ShouldReturnEmptyCollectionAndDoNotCallCoreDataProvider_WhenImportKeysCollectionIsEmpty()
    {
        // Arrange
        var importKeys = Array.Empty<PessoaImportKey>();

        var provider = new SigdnRhPessoasProvider(_coreDataProvider.Object);

        // Act
        var pessoas = await provider.GetPessoasByImportKeysAsync(importKeys, default);

        // Assert
        pessoas.Should().NotBeNull();
        pessoas.Should().BeEmpty();
        _coreDataProvider.Verify(p => p.GetPessoaCoreDataAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}