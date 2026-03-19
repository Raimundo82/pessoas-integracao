using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;

namespace Pessoas.Integracao.Tests.Unit.Application;

public sealed class DeltasPessoasUnitTests : IDisposable
{
    private Mock<IPessoasDataProvider> _dataProvider;
    private readonly Mock<IPessoasDeltasKeyProvider> _deltaKeysProvider;
    private Mock<IPessoaRepository> _repo;
    private readonly Mock<IPessoasDeltaDetector> _deltaDetector;
    private Mock<IUnitOfWork> _uow;

    public DeltasPessoasUnitTests()
    {
        _dataProvider = new Mock<IPessoasDataProvider>();
        _deltaKeysProvider = new Mock<IPessoasDeltasKeyProvider>();
        _repo = new Mock<IPessoaRepository>();
        _deltaDetector = new Mock<IPessoasDeltaDetector>();
        _uow = new Mock<IUnitOfWork>();
    }

    private void SetupDeltaKeys(IReadOnlyList<PessoaDeltasKey> deltaKeyList) =>
        _deltaKeysProvider
            .Setup(k => k.GetPessoasDeltasKeysAsync(It.IsAny<TimePeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deltaKeyList);
    private void SetupUnavailableDeltaKeysProvider() =>
        _deltaKeysProvider
            .Setup(k => k.GetPessoasDeltasKeysAsync(It.IsAny<TimePeriod>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SOAP error"));
    private void SetupChangedPessoas(IReadOnlyList<PessoaImportKey> importKeys, IReadOnlyList<Pessoa> changedPessoas)
    {
        _dataProvider
            .Setup(d => d.GetPessoasByImportKeysAsync(
                It.Is<IReadOnlyList<PessoaImportKey>>(keys =>
                    keys.Count == importKeys.Count &&
                    keys.Zip(importKeys).All(pair =>
                        pair.First.Nii == pair.Second.Nii &&
                        pair.First.ExternalId == pair.Second.ExternalId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(changedPessoas);
    }

    private void SetupExistingPessoa(PessoaImportKey importKey, Pessoa existingPessoa) =>
        _repo
            .Setup(r => r.GetPessoaByImportKeyAsync(
                It.Is<PessoaImportKey>(k =>
                    k.Nii == importKey.Nii &&
                    k.ExternalId == importKey.ExternalId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Pessoa> { existingPessoa }.AsReadOnly());
    private void SetupNonExistingPessoa(PessoaImportKey importKey) =>
        _repo
            .Setup(r => r.GetPessoaByImportKeyAsync(importKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

    private void SetupUpsert(UpsertPessoasResult upsertResult) =>
        _repo
            .Setup(r => r.UpsertAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(upsertResult);

    private void SetupDeltaDetectorDetectsChanges() =>
        _deltaDetector
            .Setup(d => d.IsPessoaChangedAsync(It.IsAny<Pessoa>(), It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

    private void SetupDeltaDetectorDoesNotDetectChanges() =>
        _deltaDetector
            .Setup(d => d.IsPessoaChangedAsync(It.IsAny<Pessoa>(), It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

    private DeltasPessoas CreateSut() =>
        new(_repo.Object, _dataProvider.Object, _deltaKeysProvider.Object, _deltaDetector.Object, _uow.Object);


    [Fact]
    public async Task Should_UpsertOnlyChangedPessoas_When_ExecuteAsyncWithChangedPessoa()
    {
        // Arrange
        var startTimestamp = DateTime.Now.AddDays(-1);
        var endTimestamp = DateTime.Now;

        var deltaKey = new PessoaDeltasKey("123456789", "EXT123", "UPDATE");
        var importKey = new PessoaImportKey("123456789", "EXT123");
        var changedPessoa = new Pessoa { NII = "123456789", ExternalId = "EXT123", DadosPessoais = new DadosPessoais { NomeCompleto = "Changed Name" } };
        var existingPessoa = new Pessoa { NII = "123456789", ExternalId = "EXT123", DadosPessoais = new DadosPessoais { NomeCompleto = "Old Name" } };
        var upsertResult = new UpsertPessoasResult(0, 1);

        SetupDeltaKeys([deltaKey]);
        SetupChangedPessoas([importKey], [changedPessoa]);
        SetupExistingPessoa(importKey, existingPessoa);
        SetupUpsert(upsertResult);
        SetupDeltaDetectorDetectsChanges();

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(startTimestamp, endTimestamp, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new DeltaPessoasResult(1));
        _repo.Verify(r => r.UpsertAllAsync(It.Is<IReadOnlyList<Pessoa>>(list => list.Count == 1 && list[0].NII == "123456789"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_UpsertAllChangedPessoas_When_ExecuteAsyncWithMultipleChanges()
    {
        // Arrange
        var startTimestamp = DateTime.Now.AddDays(-1);
        var endTimestamp = DateTime.Now;

        var deltaKey1 = new PessoaDeltasKey("111", "EXT1", "UPDATE");
        var deltaKey2 = new PessoaDeltasKey("222", "EXT2", "UPDATE");

        var importKey1 = new PessoaImportKey("111", "EXT1");
        var importKey2 = new PessoaImportKey("222", "EXT2");

        var changedPessoa1 = new Pessoa { NII = "111", ExternalId = "EXT1", DadosPessoais = new DadosPessoais { NomeCompleto = "Changed Name" } };
        var changedPessoa2 = new Pessoa { NII = "222", ExternalId = "EXT2", DadosPessoais = new DadosPessoais { NomeCompleto = "Changed Name" } };

        var existingPessoa1 = new Pessoa { NII = "111", ExternalId = "EXT1", DadosPessoais = new DadosPessoais { NomeCompleto = "Old Name" } };
        var existingPessoa2 = new Pessoa { NII = "222", ExternalId = "EXT2", DadosPessoais = new DadosPessoais { NomeCompleto = "Old Name" } };

        SetupDeltaKeys([deltaKey1, deltaKey2]);
        SetupChangedPessoas([importKey1, importKey2], [changedPessoa1, changedPessoa2]);
        SetupExistingPessoa(importKey1, existingPessoa1);
        SetupExistingPessoa(importKey2, existingPessoa2);
        SetupUpsert(new UpsertPessoasResult(0, 2));
        SetupDeltaDetectorDetectsChanges();

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(startTimestamp, endTimestamp, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new DeltaPessoasResult(2));
        _repo.Verify(r => r.UpsertAllAsync(
            It.Is<IReadOnlyList<Pessoa>>(list => list.Count == 2),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task Should_NotUpsert_When_ExecuteAsyncWithNoChanges()
    {
        // Arrange
        var startTimestamp = DateTime.Now.AddDays(-1);
        var endTimestamp = DateTime.Now;

        var deltaKey = new PessoaDeltasKey("123", "EXT", "UPDATE");
        var importKey = new PessoaImportKey("123", "EXT");

        var changedPessoa = new Pessoa { NII = "123", ExternalId = "EXT", DadosPessoais = new DadosPessoais { NomeCompleto = "Changed Name" } };
        var existingPessoa = new Pessoa { NII = "123", ExternalId = "EXT", DadosPessoais = new DadosPessoais { NomeCompleto = "Old Name" } };

        SetupDeltaKeys([deltaKey]);
        SetupChangedPessoas([importKey], [changedPessoa]);
        SetupExistingPessoa(importKey, existingPessoa);
        SetupDeltaDetectorDoesNotDetectChanges();

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(startTimestamp, endTimestamp, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new DeltaPessoasResult(0));
        _repo.Verify(r => r.UpsertAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task Should_ReturnEmptyResult_When_ExecuteAsyncWithEmptyDeltaKeys()
    {
        // Arrange
        var startTimestamp = DateTime.Now.AddDays(-1);
        var endTimestamp = DateTime.Now;
        var importKey = new PessoaImportKey("123", "EXT");
        var deltaKey = new PessoaDeltasKey("123", "EXT", "UPDATE");

        SetupDeltaKeys([deltaKey]);
        SetupNonExistingPessoa(importKey);

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(startTimestamp, endTimestamp, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new DeltaPessoasResult(0));
        _repo.Verify(r => r.UpsertAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_ReturnEmptyResult_When_ExecuteAsyncWithEmptyPessoasFromProvider()
    {
        // Arrange
        var startTimestamp = DateTime.Now.AddDays(-1);
        var endTimestamp = DateTime.Now;

        SetupDeltaKeys([]); // <— no delta keys

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(startTimestamp, endTimestamp, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new DeltaPessoasResult(0));
        _repo.Verify(r => r.UpsertAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Fact]
    public async Task Should_ThrowArgumentException_When_EndTimestampBeforeStart()
    {
        // Arrange
        var startTimestamp = DateTime.Now;
        var endTimestamp = startTimestamp.AddHours(-1);

        var sut = CreateSut();

        // Act
        Func<Task> act = () => sut.ExecuteAsync(startTimestamp, endTimestamp, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }


    [Fact]
    public async Task Should_HandleNullRepositoryReturn_When_ExecuteAsyncWithNullPessoa()
    {
        // Arrange
        var startTimestamp = DateTime.Now.AddDays(-1);
        var endTimestamp = DateTime.Now;

        var deltaKey = new PessoaDeltasKey("123", "EXT", "UPDATE");
        var importKey = new PessoaImportKey("123", "EXT");
        var changedPessoa = new Pessoa { NII = "123", ExternalId = "EXT" };

        SetupDeltaKeys([deltaKey]);
        SetupChangedPessoas([importKey], [changedPessoa]);
        SetupNonExistingPessoa(importKey);
        SetupDeltaDetectorDetectsChanges();
        SetupUpsert(new UpsertPessoasResult(0, 0));

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(startTimestamp, endTimestamp, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new DeltaPessoasResult(0));
    }


    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_ProviderThrowsSoapError()
    {
        // Arrange
        var startTimestamp = DateTime.Now.AddDays(-1);
        var endTimestamp = DateTime.Now;

        SetupUnavailableDeltaKeysProvider();

        var sut = CreateSut();

        // Act
        Func<Task> act = () => sut.ExecuteAsync(startTimestamp, endTimestamp, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("SOAP error");
    }


    public void Dispose()
    {
        _dataProvider = null!;
        _repo = null!;
        _uow = null!;
        GC.SuppressFinalize(this);
    }

}