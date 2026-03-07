using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.FragmentProviders;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

namespace Pessoas.Integracao.Worker.Tests.Unit.Providers;

public sealed class PessoaCoreDataProviderUnitTests
{
    private readonly Mock<IPersonalDataClient> _personalDataClient = new();
    private readonly Mock<IDadosPessoaisTranslator> _dadosPessoaisTranslator = new();

    private PessoaCoreDataProvider CreateSut() =>
        new(_personalDataClient.Object, _dadosPessoaisTranslator.Object);

    [Fact]
    public async Task ShouldCallPersonalDataClientWithSameImportKeys_WhenGettingCoreData()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("22600", "30002697"), new PessoaImportKey("22700", "30002797") };

        _personalDataClient.Setup(c => c.GetPersonalDataAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSPessoaisOutput?>());

        var sut = CreateSut();

        // Act
        await sut.GetPessoaCoreDataAsync(importKeys, default);

        // Assert
        _personalDataClient.Verify(
            c => c.GetPersonalDataAsync(importKeys, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldForwardCancellationToken_WhenGettingCoreData()
    {
        // Arrange
        using var tokenSource = new CancellationTokenSource();
        CancellationToken? receivedToken = null;

        _personalDataClient.Setup(c => c.GetPersonalDataAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<PessoaImportKey>, CancellationToken>((_, ct) => receivedToken = ct)
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSPessoaisOutput?>());

        var sut = CreateSut();

        // Act
        await sut.GetPessoaCoreDataAsync([], tokenSource.Token);

        // Assert
        receivedToken.Should().Be(tokenSource.Token);
    }

    [Fact]
    public async Task ShouldCallTranslatorForEachEntry_WhenClientReturnsMultipleResults()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("22600", "30002697"), new PessoaImportKey("22700", "30002797") };
        var output1 = new ZhrSPessoaisOutput();
        var output2 = new ZhrSPessoaisOutput();

        _personalDataClient.Setup(c => c.GetPersonalDataAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSPessoaisOutput?>
            {
                [importKeys[0]] = output1,
                [importKeys[1]] = output2
            });

        _dadosPessoaisTranslator.Setup(t => t.Translate(It.IsAny<ZhrSPessoaisOutput?>()))
            .Returns(new DadosPessoais());

        var sut = CreateSut();

        // Act
        await sut.GetPessoaCoreDataAsync(importKeys, default);

        // Assert
        _dadosPessoaisTranslator.Verify(t => t.Translate(output1), Times.Once);
        _dadosPessoaisTranslator.Verify(t => t.Translate(output2), Times.Once);
    }

    [Fact]
    public async Task ShouldReturnDictionaryWithSameKeys_WhenClientReturnsResults()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("22600", "30002697"), new PessoaImportKey("22700", "30002797") };

        _personalDataClient.Setup(c => c.GetPersonalDataAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSPessoaisOutput?>
            {
                [importKeys[0]] = new ZhrSPessoaisOutput(),
                [importKeys[1]] = new ZhrSPessoaisOutput()
            });

        _dadosPessoaisTranslator.Setup(t => t.Translate(It.IsAny<ZhrSPessoaisOutput?>()))
            .Returns(new DadosPessoais());

        var sut = CreateSut();

        // Act
        var result = await sut.GetPessoaCoreDataAsync(importKeys, default);

        // Assert
        result.Should().HaveCount(2);
        result.Keys.Should().BeEquivalentTo(importKeys);
    }

    [Fact]
    public async Task ShouldWrapTranslatedDadosPessoaisInFragment_WhenBuildingResult()
    {
        // Arrange
        var importKey = new PessoaImportKey("22600", "30002697");
        var expectedDadosPessoais = new DadosPessoais { NomeCompleto = "João Silva" };

        _personalDataClient.Setup(c => c.GetPersonalDataAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSPessoaisOutput?>
            {
                [importKey] = new ZhrSPessoaisOutput()
            });

        _dadosPessoaisTranslator.Setup(t => t.Translate(It.IsAny<ZhrSPessoaisOutput?>()))
            .Returns(expectedDadosPessoais);

        var sut = CreateSut();

        // Act
        var result = await sut.GetPessoaCoreDataAsync([importKey], default);

        // Assert
        result.Should().ContainKey(importKey);
        result[importKey].DadosPessoais.Should().BeSameAs(expectedDadosPessoais);
    }

    [Fact]
    public async Task ShouldReturnEmptyDictionary_WhenClientReturnsEmptyDictionary()
    {
        // Arrange
        _personalDataClient.Setup(c => c.GetPersonalDataAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSPessoaisOutput?>());

        var sut = CreateSut();

        // Act
        var result = await sut.GetPessoaCoreDataAsync([], default);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _dadosPessoaisTranslator.Verify(t => t.Translate(It.IsAny<ZhrSPessoaisOutput?>()), Times.Never);
    }

    [Fact]
    public async Task ShouldThrowException_WhenPersonalDataClientThrows()
    {
        // Arrange
        _personalDataClient.Setup(c => c.GetPersonalDataAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SOAP error"));

        var sut = CreateSut();

        // Act
        Func<Task> action = async () => await sut.GetPessoaCoreDataAsync([], default);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*SOAP error*");
    }
}