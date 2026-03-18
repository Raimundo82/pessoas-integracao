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

    private void SetupDeltaKeys(PessoaDeltasKey deltaKey) =>
        _deltaKeysProvider
            .Setup(k => k.GetPessoasDeltasKeysAsync(It.IsAny<TimePeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([deltaKey]);

    private void SetupChangedPessoa(PessoaImportKey importKey, Pessoa changedPessoa) =>
        _dataProvider
            .Setup(d => d.GetPessoasByImportKeysAsync(
                It.Is<IReadOnlyList<PessoaImportKey>>(keys =>
                    keys.Count == 1 &&
                    keys[0].Nii == importKey.Nii &&
                    keys[0].ExternalId == importKey.ExternalId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([changedPessoa]);

    private void SetupExistingPessoa(PessoaImportKey importKey, Pessoa existingPessoa) =>
        _repo
            .Setup(r => r.GetPessoaByImportKeyAsync(importKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Pessoa> { existingPessoa }.AsReadOnly());

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
            .ReturnsAsync(true);

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

        SetupDeltaKeys(deltaKey);
        SetupChangedPessoa(importKey, changedPessoa);
        SetupExistingPessoa(importKey, existingPessoa);
        SetupUpsert(upsertResult);
        SetupDeltaDetectorDetectsChanges();

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteAsync(startTimestamp, endTimestamp, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new DeltaPessoasResult(1, 1));
        _repo.Verify(r => r.UpsertAllAsync(It.Is<IReadOnlyList<Pessoa>>(list => list.Count == 1 && list[0].NII == "123456789"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_UpsertAllChangedPessoas_When_ExecuteAsyncWithMultipleChanges()
    {
        // Arrange


        // Act


        // Assert
    }

    [Fact]
    public async Task Should_NotUpsert_When_ExecuteAsyncWithNoChanges()
    {
        // Arrange


        // Act


        // Assert

    }

    [Fact]
    public async Task Should_ReturnEmptyResult_When_ExecuteAsyncWithEmptyDeltaKeys()
    {
        // Arrange


        // Act


        // Assert
    }

    [Fact]
    public async Task Should_ThrowArgumentException_When_EndTimestampBeforeStart()
    {
        // Arrange


        // Act


        // Assert

    }

    [Fact]
    public async Task Should_HandleNullRepositoryReturn_When_ExecuteAsyncWithNullPessoa()
    {
        // Arrange


        // Act


        // Assert
    }

    [Fact]
    public async Task Should_ThrowInvalidOperationException_When_ProviderThrowsSoapError()
    {
        // Arrange


        // Act


        // Assert
    }

    public void Dispose()
    {
        _dataProvider = null!;
        _repo = null!;
        _uow = null!;
        GC.SuppressFinalize(this);
    }

}