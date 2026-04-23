using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.FragmentProviders;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Contracts;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Translators;

namespace Pessoas.Integracao.Worker.Tests.Unit.Providers;

public sealed class PessoaCoreDataProviderUnitTests
{
    private readonly Mock<IPersonalDataClient> _personalDataClient = new();
    private readonly Mock<IDadosPessoaisTranslator> _dadosPessoaisTranslator = new();
    private readonly Mock<IExamesMedClient> _examesMedClient = new();
    private readonly Mock<IDadosBiometricosTranslator> _dadosBiometricosTranslator = new();

    private PessoaCoreDataProvider CreateSut() =>
        new(_personalDataClient.Object, _dadosPessoaisTranslator.Object, _examesMedClient.Object, _dadosBiometricosTranslator.Object);

    [Fact]
    public async Task ShouldCallClientsWithSameImportKeys_WhenGettingCoreData()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("22600", "30002697"), new PessoaImportKey("22700", "30002797") };

        _personalDataClient.Setup(c => c.GetPersonalDataAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(importKeys.ToDictionary(k => k, _ => (ZhrSPessoaisOutput?)null));

        _examesMedClient.Setup(c => c.GetExamesMedAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(importKeys.ToDictionary(k => k, _ => (ZhrSExamesMedOutput?)null));

        var sut = CreateSut();

        // Act
        await sut.GetPessoaCoreDataAsync(importKeys, default);

        // Assert
        _personalDataClient.Verify(
            c => c.GetPersonalDataAsync(importKeys, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ShouldForwardCancellationTokenToBothClients_WhenGettingCoreData()
    {
        // Arrange
        using var tokenSource = new CancellationTokenSource();
        CancellationToken? personalToken = null;
        CancellationToken? biometricToken = null;

        _personalDataClient.Setup(c => c.GetPersonalDataAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<PessoaImportKey>, CancellationToken>((_, ct) => personalToken = ct)
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSPessoaisOutput?>());

        _examesMedClient.Setup(c => c.GetExamesMedAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<PessoaImportKey>, CancellationToken>((_, ct) => biometricToken = ct)
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSExamesMedOutput?>());

        var sut = CreateSut();

        // Act
        await sut.GetPessoaCoreDataAsync([], tokenSource.Token);

        // Assert
        personalToken.Should().Be(tokenSource.Token);
        biometricToken.Should().Be(tokenSource.Token);
    }

    [Fact]
    public async Task ShouldCallTranslatorsForEachEntry_WhenClientsReturnMultipleResults()
    {
        // Arrange
        var importKeys = new[]
        {
            new PessoaImportKey("22600", "30002697"),
            new PessoaImportKey("22700", "30002797")
        };

        var outputPessoais1 = new ZhrSPessoaisOutput();
        var outputPessoais2 = new ZhrSPessoaisOutput();
        var outputBiometricos1 = new ZhrSExamesMedOutput();
        var outputBiometricos2 = new ZhrSExamesMedOutput();

        _personalDataClient.Setup(c => c.GetPersonalDataAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSPessoaisOutput?> { [importKeys[0]] = outputPessoais1, [importKeys[1]] = outputPessoais2 });

        _examesMedClient.Setup(c => c.GetExamesMedAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSExamesMedOutput?> { [importKeys[0]] = outputBiometricos1, [importKeys[1]] = outputBiometricos2 });

        _dadosPessoaisTranslator.Setup(t => t.Translate(It.IsAny<ZhrSPessoaisOutput?>()))
            .Returns(new DadosPessoais());

        _dadosBiometricosTranslator.Setup(t => t.Translate(It.IsAny<ZhrSExamesMedOutput?>()))
            .Returns(new DadosBiometricos());

        var sut = CreateSut();

        // Act
        await sut.GetPessoaCoreDataAsync(importKeys, default);

        // Assert
        _dadosPessoaisTranslator.Verify(t => t.Translate(outputPessoais1), Times.Once);
        _dadosPessoaisTranslator.Verify(t => t.Translate(outputPessoais2), Times.Once);

        _dadosBiometricosTranslator.Verify(t => t.Translate(outputBiometricos1), Times.Once);
        _dadosBiometricosTranslator.Verify(t => t.Translate(outputBiometricos2), Times.Once);
    }


    [Fact]
    public async Task ShouldReturnDictionaryWithSameKeys_WhenClientsReturnResults()
    {
        // Arrange
        var importKeys = new[]
        {
        new PessoaImportKey("22600", "30002697"),
        new PessoaImportKey("22700", "30002797")
    };

        _personalDataClient.Setup(c => c.GetPersonalDataAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(importKeys.ToDictionary(k => k, _ => (ZhrSPessoaisOutput?)null));

        _examesMedClient.Setup(c => c.GetExamesMedAsync(importKeys, It.IsAny<CancellationToken>()))
            .ReturnsAsync(importKeys.ToDictionary(k => k, _ => (ZhrSExamesMedOutput?)null));

        _dadosPessoaisTranslator.Setup(t => t.Translate(It.IsAny<ZhrSPessoaisOutput?>()))
            .Returns(new DadosPessoais());

        _dadosBiometricosTranslator.Setup(t => t.Translate(It.IsAny<ZhrSExamesMedOutput?>()))
            .Returns(new DadosBiometricos());

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

        _examesMedClient.Setup(c => c.GetExamesMedAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSExamesMedOutput?>
            {
                [importKey] = null
            });

        _dadosPessoaisTranslator.Setup(t => t.Translate(It.IsAny<ZhrSPessoaisOutput?>()))
            .Returns(expectedDadosPessoais);

        _dadosBiometricosTranslator.Setup(t => t.Translate(It.IsAny<ZhrSExamesMedOutput?>()))
            .Returns(new DadosBiometricos());

        var sut = CreateSut();

        // Act
        var result = await sut.GetPessoaCoreDataAsync([importKey], default);

        // Assert
        result.Should().ContainKey(importKey);
        result[importKey].DadosPessoais.Should().BeSameAs(expectedDadosPessoais);
    }


    [Fact]
    public async Task ShouldReturnEmptyDictionary_WhenClientsReturnEmpty()
    {
        // Arrange
        _personalDataClient.Setup(c => c.GetPersonalDataAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSPessoaisOutput?>());

        _examesMedClient.Setup(c => c.GetExamesMedAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<PessoaImportKey, ZhrSExamesMedOutput?>());

        var sut = CreateSut();

        // Act
        var result = await sut.GetPessoaCoreDataAsync([], default);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _dadosPessoaisTranslator.Verify(t => t.Translate(It.IsAny<ZhrSPessoaisOutput?>()), Times.Never);
        _dadosBiometricosTranslator.Verify(t => t.Translate(It.IsAny<ZhrSExamesMedOutput?>()), Times.Never);
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

    [Fact]
    public async Task ShouldThrowException_WhenBiometricallDataClientThrows()
    {
        // Arrange
        _examesMedClient.Setup(c => c.GetExamesMedAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SOAP error"));

        var sut = CreateSut();

        // Act
        Func<Task> action = async () => await sut.GetPessoaCoreDataAsync([], default);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*SOAP error*");
    }
}
