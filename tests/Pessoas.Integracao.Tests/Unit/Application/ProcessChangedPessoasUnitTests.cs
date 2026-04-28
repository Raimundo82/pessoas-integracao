using FluentAssertions;


using Moq;

using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;

namespace Pessoas.Integracao.Tests.Unit.Application;

public sealed class ProcessChangedPessoasUnitTests : IDisposable
{
    private Mock<IPessoasDataProvider> _dataProvider;
    private readonly Mock<IPessoasChangedImportKeyProvider> _changedImportKeyProvider;
    private Mock<IPessoaRepository> _repo;
    private readonly Mock<IPessoaChangeDetector> _pessoaChangedDetetor;
    private Mock<IUnitOfWork> _uow;

    public ProcessChangedPessoasUnitTests()
    {
        _dataProvider = new Mock<IPessoasDataProvider>();
        _changedImportKeyProvider = new Mock<IPessoasChangedImportKeyProvider>();
        _repo = new Mock<IPessoaRepository>();
        _pessoaChangedDetetor = new Mock<IPessoaChangeDetector>();
        _uow = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task ShouldUpsertOnlyChangedPessoas_WhenExecuteAsyncWithChangedPessoa()
    {
        // Arrange
        var timePeriod = GetTimePeriod();
        var ct = new CancellationToken();

        IReadOnlyList<PessoaImportKey> changedImportKeys = [new PessoaImportKey("123456789", "EXT123")];
        IReadOnlyList<Pessoa> changedPessoas = [new Pessoa { NII = "123456789", ExternalId = "EXT123", DadosPessoais = new DadosPessoais { NomeCompleto = "Changed Name" } }];
        IReadOnlyList<Pessoa> equivalentPessoasInRepo = [new Pessoa { NII = "123456789", ExternalId = "EXT123", DadosPessoais = new DadosPessoais { NomeCompleto = "Old Name" } }];


        _changedImportKeyProvider.Setup(k => k.GetChangedImportKeysAsync(timePeriod, ct)).ReturnsAsync(changedImportKeys);
        _dataProvider.Setup(d => d.GetPessoasByImportKeysAsync(
                It.Is<IReadOnlyList<PessoaImportKey>>(keys => keys.SequenceEqual(changedImportKeys)),
                ct))
            .ReturnsAsync(changedPessoas);

        _repo.Setup(r => r.GetPessoasByNiiAsync(
                It.Is<List<string>>(niis => niis.SequenceEqual(changedImportKeys.Select(k => k.Nii))),
                ct))
            .ReturnsAsync(equivalentPessoasInRepo);

        _pessoaChangedDetetor
               .Setup(d => d.IsPessoaChanged(
                   It.Is<Pessoa>(p => p.NII == "123456789"),
                   It.Is<Pessoa>(p => p.NII == "123456789")))
               .Returns(true);

        _repo.Setup(r => r.UpsertAllAsync(
                It.Is<IReadOnlyList<Pessoa>>(pessoas => pessoas.SequenceEqual(changedPessoas)),
                ct))
            .ReturnsAsync(new UpsertPessoasResult(0, 1));

        _uow.Setup(u => u.CommitAsync(ct)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        // Act
        await sut.ExecuteAsync(timePeriod, ct);

        // Assert
        _changedImportKeyProvider.Verify(k => k.GetChangedImportKeysAsync(timePeriod, ct), Times.Once);

        _dataProvider.Verify(d => d.GetPessoasByImportKeysAsync(changedImportKeys, ct), Times.Once);

        _repo.Verify(r => r.GetPessoasByNiiAsync(
            It.Is<List<string>>(niis => niis.SequenceEqual(changedImportKeys.Select(k => k.Nii))),
            ct),
            Times.Once);

        _pessoaChangedDetetor.Verify(d => d.IsPessoaChanged(
            It.Is<Pessoa>(p => p.NII == "123456789"),
            It.Is<Pessoa>(p => p.NII == "123456789")),
            Times.Once);

        _repo.Verify(r => r.UpsertAllAsync(
            It.Is<IReadOnlyList<Pessoa>>(list => list.Count == 1 && list[0].NII == "123456789"),
            ct),
            Times.Once);

        _uow.Verify(u => u.CommitAsync(ct), Times.Once);
    }

    [Fact]
    public async Task Should_UpsertAllChangedPessoas_When_ExecuteAsyncWithMultipleChanges()
    {
        // Arrange
        var timePeriod = GetTimePeriod();
        var ct = new CancellationToken();

        var importKey1 = new PessoaImportKey("111", "EXT1");
        var importKey2 = new PessoaImportKey("222", "EXT2");

        var changedPessoa1 = new Pessoa { NII = "111", ExternalId = "EXT1", DadosPessoais = new DadosPessoais { NomeCompleto = "Changed Name" } };
        var changedPessoa2 = new Pessoa { NII = "222", ExternalId = "EXT2", DadosPessoais = new DadosPessoais { NomeCompleto = "Changed Name" } };

        var existingPessoa1 = new Pessoa { NII = "111", ExternalId = "EXT1", DadosPessoais = new DadosPessoais { NomeCompleto = "Old Name" } };
        var existingPessoa2 = new Pessoa { NII = "222", ExternalId = "EXT2", DadosPessoais = new DadosPessoais { NomeCompleto = "Old Name" } };

        SetupPessoasChangedKeysProvider([importKey1, importKey2]);
        SetupChangedPessoas([importKey1, importKey2], [changedPessoa1, changedPessoa2]);
        SetupExistingPessoa(importKey1, existingPessoa1);
        SetupExistingPessoa(importKey2, existingPessoa2);
        SetupUpsert(new UpsertPessoasResult(0, 2));
        SetupPessoaChangedDetetorDetectsChange();

        var sut = CreateSut();

        // Act
        await sut.ExecuteAsync(timePeriod, ct);

        // Assert
        _repo.Verify(r => r.UpsertAllAsync(
            It.Is<IReadOnlyList<Pessoa>>(list => list.Count == 2),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task Should_UpsertEmptyPessoa_When_ExecuteAsyncWithNoChanges()
    {
        // Arrange
        var timePeriod = GetTimePeriod();

        var importKey = new PessoaImportKey("123", "EXT");

        var changedPessoa = new Pessoa { NII = "123", ExternalId = "EXT", DadosPessoais = new DadosPessoais { NomeCompleto = "Old Name" } };
        var existingPessoa = new Pessoa { NII = "123", ExternalId = "EXT", DadosPessoais = new DadosPessoais { NomeCompleto = "Old Name" } };

        SetupPessoasChangedKeysProvider([importKey]);
        SetupChangedPessoas([importKey], [changedPessoa]);
        SetupExistingPessoa(importKey, existingPessoa);
        SetupPessoaChangedDetectorDoesNotDetectChange();

        var sut = CreateSut();

        // Act
        await sut.ExecuteAsync(timePeriod, CancellationToken.None);

        // Assert
        _repo.Verify(r => r.UpsertAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_UpsertEmptyPessoa_When_ExecuteAsyncWithEmptyChangedImportKeys()
    {
        // Arrange
        var timePeriod = GetTimePeriod();
        var ct = new CancellationToken();

        SetupPessoasChangedKeysProvider([]);
        SetupChangedPessoas([], []);
        SetupNonExistingPessoa();
        SetupPessoaChangedDetectorDoesNotDetectChange();

        var sut = CreateSut();

        // Act
        await sut.ExecuteAsync(timePeriod, ct);

        // Assert
        _repo.Verify(r => r.UpsertAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_UpsertEmptyPessoa_When_ExecuteAsyncWithEmptyPessoasFromProvider()
    {
        // Arrange
        var timePeriod = GetTimePeriod();

        SetupPessoasChangedKeysProvider([]);
        SetupChangedPessoas([], []);

        var sut = CreateSut();

        // Act
        await sut.ExecuteAsync(timePeriod, CancellationToken.None);

        // Assert
        _repo.Verify(r => r.UpsertAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_UpsertEmptyPessoa_WhenExecuteAsyncWithEmptyPessoasFromRepository()
    {
        // Arrange
        var timePeriod = GetTimePeriod();
        var importKey = new PessoaImportKey("123", "EXT");
        var changedPessoa = new Pessoa { NII = "123", ExternalId = "EXT" };

        SetupPessoasChangedKeysProvider([importKey]);
        SetupChangedPessoas([importKey], [changedPessoa]);
        SetupNonExistingPessoa(importKey);
        SetupPessoaChangedDetetorDetectsChange();
        SetupUpsert(new UpsertPessoasResult(0, 0));

        var sut = CreateSut();

        // Act
        await sut.ExecuteAsync(timePeriod, CancellationToken.None);

        // Assert
        _repo.Verify(r => r.UpsertAllAsync(It.Is<IReadOnlyList<Pessoa>>(list => list.Count == 1 && list[0].NII == "123"), It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_ProviderThrowsSoapError()
    {
        // Arrange
        var timePeriod = GetTimePeriod();

        SetupUnavailableDeltaKeysProvider();

        var sut = CreateSut();

        // Act
        Func<Task> act = () => sut.ExecuteAsync(timePeriod, CancellationToken.None);

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

    private static TimePeriod GetTimePeriod()
    {
        var startTimestamp = DateTime.Now.AddDays(-1);
        var endTimestamp = DateTime.Now;
        return new TimePeriod(startTimestamp, endTimestamp);
    }

    private void SetupPessoasChangedKeysProvider(IReadOnlyList<PessoaImportKey> importKeyList) =>
        _changedImportKeyProvider
            .Setup(k => k.GetChangedImportKeysAsync(It.IsAny<TimePeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(importKeyList);

    private void SetupUnavailableDeltaKeysProvider() =>
        _changedImportKeyProvider
            .Setup(k => k.GetChangedImportKeysAsync(It.IsAny<TimePeriod>(), It.IsAny<CancellationToken>()))
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
        .Setup(r => r.GetPessoasByNiiAsync(
            It.Is<IReadOnlyList<string>>(list =>
                list.Contains(importKey.Nii)),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<Pessoa> { existingPessoa }.AsReadOnly());

    private void SetupNonExistingPessoa(PessoaImportKey importKey) =>
        _repo.Setup(r => r.GetPessoasByNiiAsync(
            It.Is<IReadOnlyList<string>>(list => list.Contains(importKey.Nii)),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<Pessoa>().AsReadOnly());
    private void SetupNonExistingPessoa() =>
        _repo.Setup(r => r.GetPessoasByNiiAsync(
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Pessoa>());


    private void SetupUpsert(UpsertPessoasResult upsertResult) =>
        _repo.Setup(r => r.UpsertAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), It.IsAny<CancellationToken>())).ReturnsAsync(upsertResult);

    private void SetupPessoaChangedDetetorDetectsChange() =>
        _pessoaChangedDetetor.Setup(d => d.IsPessoaChanged(It.IsAny<Pessoa>(), It.IsAny<Pessoa>())).Returns(true);

    private void SetupPessoaChangedDetectorDoesNotDetectChange() =>
        _pessoaChangedDetetor.Setup(d => d.IsPessoaChanged(It.IsAny<Pessoa>(), It.IsAny<Pessoa>())).Returns(false);

    private ProcessChangedPessoas CreateSut() =>
        new(_repo.Object, _dataProvider.Object, _changedImportKeyProvider.Object, _pessoaChangedDetetor.Object, _uow.Object);
}