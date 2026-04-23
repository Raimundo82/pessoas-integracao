using System.Collections.ObjectModel;

using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;

using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Tests.Unit.TestDoubles;

namespace Pessoas.Integracao.Tests.Unit.Application;

public sealed class ImportPessoasUnitTests : IDisposable
{
    // Test dependencies
    private Mock<IPessoasDataProvider> _dataProvider;
    private Mock<IPessoasImportKeyProvider> _keysProvider;
    private Mock<IPessoaRepository> _repo;
    private Mock<IUnitOfWork> _uow;

    public ImportPessoasUnitTests()
    {
        // Setup runs before each test
        _dataProvider = new Mock<IPessoasDataProvider>();
        _keysProvider = new Mock<IPessoasImportKeyProvider>();
        _repo = new Mock<IPessoaRepository>();
        _uow = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task ShouldUpsertMergedDistinctPessoasAndCommit_WhenRepositoryAndSourceHaveDifferentKeys()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;


        var fakeRepo = new FakePessoaRepository([new("22600", "30001000"), new("21200", "30002000")]);
        var keysProviderStub = new StubPessoasImportKeyProvider([new("22601", "30001001"), new("21201", "30002001")]);
        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { NII = "22600", ExternalId = "30001000" },
            new() { NII = "21200", ExternalId = "30002000" },
            new() { NII = "22601", ExternalId = "30001001" },
            new() { NII = "21201", ExternalId = "30002001" }
        ]);
        var dataProviderStub = new StubPessoasDataProvider(pessoas);
        var uowSpy = new SpyUnitOfWork();
        var uut = new ImportPessoas(fakeRepo, dataProviderStub, keysProviderStub, uowSpy);

        // Act (When)
        var result = await uut.ExecuteAsync(ct);

        // Assert (Then)
        result.Should().NotBeNull();
        result.Should().BeOfType<ImportPessoasResult>();
        result.TotalProcessed.Should().Be(4);
        result.TotalAdded.Should().Be(4);
        result.TotalUpdated.Should().Be(0);

        fakeRepo.LastGetKeysToken.Should().Be(ct);
        fakeRepo.LastUpsertToken.Should().Be(ct);
        fakeRepo.LastUpsertedPessoas.Should().NotBeNull();
        fakeRepo.LastUpsertedPessoas.Should().HaveCount(4);
        fakeRepo.LastUpsertedPessoas.Should().Equal(pessoas);
        dataProviderStub.LastRequestedKeys.Should().NotBeNull();
        dataProviderStub.LastRequestedKeys.Should().HaveCount(4);
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "22600" && k.ExternalId == "30001000");
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "21200" && k.ExternalId == "30002000");
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "22601" && k.ExternalId == "30001001");
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "21201" && k.ExternalId == "30002001");
        dataProviderStub.LastToken.Should().Be(ct);
        uowSpy.CommitCalls.Should().Be(1);
        uowSpy.LastToken.Should().Be(ct);
    }

    [Fact]
    public async Task ShouldUpsertSourcePessoasAndCommit_WhenRepositoryHasNoKey()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;

        var fakeRepo = new FakePessoaRepository([]);

        var sourceKeys = new ReadOnlyCollection<PessoaImportKey>(
        [
            new("22601", "30001001"),
            new("21201", "30002001")
        ]);
        var keysProviderStub = new StubPessoasImportKeyProvider(sourceKeys);

        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 3, NII = "22601", ExternalId = "30001001" },
            new() { Id = 4, NII = "21201", ExternalId = "30002001" }
        ]);

        var dataProviderStub = new StubPessoasDataProvider(pessoas);

        var uowSpy = new SpyUnitOfWork();
        var uut = new ImportPessoas(fakeRepo, dataProviderStub, keysProviderStub, uowSpy);

        // Act (When)
        var result = await uut.ExecuteAsync(ct);

        // Assert (Then)
        result.Should().NotBeNull();
        result.Should().BeOfType<ImportPessoasResult>();
        result.TotalProcessed.Should().Be(2);
        result.TotalAdded.Should().Be(2);
        result.TotalUpdated.Should().Be(0);

        fakeRepo.LastGetKeysToken.Should().Be(ct);
        fakeRepo.LastUpsertToken.Should().Be(ct);
        fakeRepo.LastUpsertedPessoas.Should().NotBeNull();
        fakeRepo.LastUpsertedPessoas.Should().HaveCount(2);
        fakeRepo.LastUpsertedPessoas.Should().Equal(pessoas);
        dataProviderStub.LastRequestedKeys.Should().NotBeNull();
        dataProviderStub.LastRequestedKeys.Should().HaveCount(2);
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "22601" && k.ExternalId == "30001001");
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "21201" && k.ExternalId == "30002001");
        dataProviderStub.LastToken.Should().Be(ct);

        uowSpy.CommitCalls.Should().Be(1);
        uowSpy.LastToken.Should().Be(ct);
    }

    [Fact]
    public async Task ShouldUpsertRepositoryPessoasAndCommit_WhenSourceHasNoKeys()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;
        var fakeRepo = new FakePessoaRepository([new("22601", "30001001"), new("21201", "30002001")]);
        var keysProviderStub = new StubPessoasImportKeyProvider([]);
        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 3, NII = "22601", ExternalId = "30001001" },
            new() { Id = 4, NII = "21201", ExternalId = "30002001" }
        ]);

        var dataProviderStub = new StubPessoasDataProvider(pessoas);
        var uowSpy = new SpyUnitOfWork();

        var uut = new ImportPessoas(fakeRepo, dataProviderStub, keysProviderStub, uowSpy);

        // Act (When)
        var result = await uut.ExecuteAsync(ct);
        result.Should().NotBeNull();
        result.Should().BeOfType<ImportPessoasResult>();
        result.TotalProcessed.Should().Be(2);
        result.TotalAdded.Should().Be(2);
        result.TotalUpdated.Should().Be(0);

        // Assert (Then)
        fakeRepo.LastGetKeysToken.Should().Be(ct);
        fakeRepo.LastUpsertToken.Should().Be(ct);
        fakeRepo.LastUpsertedPessoas.Should().NotBeNull();
        fakeRepo.LastUpsertedPessoas.Should().HaveCount(2);
        fakeRepo.LastUpsertedPessoas.Should().Equal(pessoas);
        dataProviderStub.LastRequestedKeys.Should().NotBeNull();
        dataProviderStub.LastRequestedKeys.Should().HaveCount(2);
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "22601" && k.ExternalId == "30001001");
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "21201" && k.ExternalId == "30002001");
        dataProviderStub.LastToken.Should().Be(ct);
        uowSpy.CommitCalls.Should().Be(1);
        uowSpy.LastToken.Should().Be(ct);
    }

    [Fact]
    public async Task ShouldRequestDistinctKeysOnly_WhenRepositoryAndSourceContainDuplicateNiis()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;
        var fakeRepo = new FakePessoaRepository([new("22601", "30001001"), new("21201", "30002001")]);
        var keysProviderStub = new StubPessoasImportKeyProvider([new("22601", "30001001"), new("21201", "30002001")]);

        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { NII = "22601", ExternalId = "30001001" },
            new() { NII = "21201", ExternalId = "30002001" }
        ]);
        var dataProviderStub = new StubPessoasDataProvider(pessoas);
        var uowSpy = new SpyUnitOfWork();

        var uut = new ImportPessoas(fakeRepo, dataProviderStub, keysProviderStub, uowSpy);

        // Act (When)
        var result = await uut.ExecuteAsync(ct);

        // Assert (Then)
        result.Should().NotBeNull();
        result.Should().BeOfType<ImportPessoasResult>();
        result.TotalProcessed.Should().Be(2);
        result.TotalAdded.Should().Be(2);
        result.TotalUpdated.Should().Be(0);

        fakeRepo.LastUpsertToken.Should().Be(ct);
        fakeRepo.LastGetKeysToken.Should().Be(ct);
        fakeRepo.LastUpsertedPessoas.Should().NotBeNull();
        fakeRepo.LastUpsertedPessoas.Should().HaveCount(2);
        fakeRepo.LastUpsertedPessoas.Should().Equal(pessoas);
        dataProviderStub.LastRequestedKeys.Should().NotBeNull();
        dataProviderStub.LastRequestedKeys.Should().HaveCount(2);
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "22601" && k.ExternalId == "30001001");
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "21201" && k.ExternalId == "30002001");
        dataProviderStub.LastToken.Should().Be(ct);
        uowSpy.CommitCalls.Should().Be(1);
        uowSpy.LastToken.Should().Be(ct);
    }

    [Fact]
    public async Task ShouldMergePartialOverlapWithoutDuplicates_WhenRepositoryAndSourceShareSomeNiis()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;
        var fakeRepo = new FakePessoaRepository([new("22600", "30001000"), new("21200", "30002000")]);
        var keysProviderStub = new StubPessoasImportKeyProvider([new("22601", "30001001"), new("21200", "30002000")]);

        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { NII = "21200", ExternalId = "30002000" },
            new() { NII = "22601", ExternalId = "30001001" },
            new() { NII = "21201", ExternalId = "30002001" }

        ]);
        var dataProviderStub = new StubPessoasDataProvider(pessoas);
        var uowSpy = new SpyUnitOfWork();

        var uut = new ImportPessoas(fakeRepo, dataProviderStub, keysProviderStub, uowSpy);

        // Act (When)
        var result = await uut.ExecuteAsync(ct);

        // Assert (Then)
        result.Should().NotBeNull();
        result.Should().BeOfType<ImportPessoasResult>();
        result.TotalProcessed.Should().Be(3);
        result.TotalAdded.Should().Be(3);
        result.TotalUpdated.Should().Be(0);

        fakeRepo.LastGetKeysToken.Should().Be(ct);
        fakeRepo.LastUpsertToken.Should().Be(ct);
        fakeRepo.LastUpsertedPessoas.Should().NotBeNull();
        fakeRepo.LastUpsertedPessoas.Should().HaveCount(3);
        fakeRepo.LastUpsertedPessoas.Should().Equal(pessoas);
        dataProviderStub.LastRequestedKeys.Should().NotBeNull();
        dataProviderStub.LastRequestedKeys.Should().HaveCount(3);
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "22600" && k.ExternalId == "30001000");
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "22601" && k.ExternalId == "30001001");
        dataProviderStub.LastRequestedKeys.Should().Contain(k => k.Nii == "21200" && k.ExternalId == "30002000");
        dataProviderStub.LastToken.Should().Be(ct);
        uowSpy.CommitCalls.Should().Be(1);
        uowSpy.LastToken.Should().Be(ct);
    }

    [Fact]
    public async Task ShouldUpsertEmptyListAndCommit_WhenNoImportKeysExist()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;
        var fakeRepo = new FakePessoaRepository([]);
        var keysProviderStub = new StubPessoasImportKeyProvider([]);
        var dataProviderStub = new StubPessoasDataProvider([]);
        var uowSpy = new SpyUnitOfWork();

        var uut = new ImportPessoas(fakeRepo, dataProviderStub, keysProviderStub, uowSpy);

        // Act (When)
        var result = await uut.ExecuteAsync(ct);

        // Assert (Then)
        result.Should().NotBeNull();
        result.Should().BeOfType<ImportPessoasResult>();
        result.TotalProcessed.Should().Be(0);
        result.TotalAdded.Should().Be(0);
        result.TotalUpdated.Should().Be(0);

        fakeRepo.LastGetKeysToken.Should().Be(ct);
        fakeRepo.LastUpsertToken.Should().Be(ct);
        fakeRepo.LastUpsertedPessoas.Should().NotBeNull();
        fakeRepo.LastUpsertedPessoas.Should().HaveCount(0);
        dataProviderStub.LastRequestedKeys.Should().NotBeNull();
        dataProviderStub.LastRequestedKeys.Should().HaveCount(0);
        dataProviderStub.LastToken.Should().Be(ct);
        uowSpy.CommitCalls.Should().Be(1);
        uowSpy.LastToken.Should().Be(ct);
    }

    [Fact]
    public async Task ShouldUpdateExistingPessoaField_WhenSourceProvidesNewValueForSameNii()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var fakeRepo = new FakePessoaRepository([new("22601", "30001001")]);
        var keysProviderStub = new StubPessoasImportKeyProvider([new("22601", "30001001")]);
        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 1, NII = "22601", ExternalId = "30001001", DadosPessoais = new DadosPessoais { NomeCompleto = "Updated Name" } }
        ]);
        var dataProviderStub = new StubPessoasDataProvider(pessoas);
        var uowSpy = new SpyUnitOfWork();

        var uut = new ImportPessoas(fakeRepo, dataProviderStub, keysProviderStub, uowSpy);

        // Act
        var result = await uut.ExecuteAsync(ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ImportPessoasResult>();
        result.TotalProcessed.Should().Be(1);
        result.TotalAdded.Should().Be(1);
        result.TotalUpdated.Should().Be(0);

        fakeRepo.LastUpsertedPessoas.Should().NotBeNull();
        fakeRepo.LastUpsertedPessoas.Should().HaveCount(1);
        fakeRepo.LastUpsertedPessoas.Should().ContainSingle(p => p.NII == "22601" && p.ExternalId == "30001001" && p.DadosPessoais.NomeCompleto == "Updated Name");
    }

    [Fact]
    public async Task ShouldNotUpsertOrCommit_WhenDataProviderThrows()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var fakeRepo = new FakePessoaRepository([]);
        var keysProviderStub = new StubPessoasImportKeyProvider([]);
        var dataProviderException = new Exception("source error");
        var dataProviderStub = new ThrowingPessoasDataProvider(dataProviderException);
        var uowSpy = new SpyUnitOfWork();

        var uut = new ImportPessoas(fakeRepo, dataProviderStub, keysProviderStub, uowSpy);

        // Act
        await Assert.ThrowsAsync<Exception>(() => uut.ExecuteAsync(ct));

        // Assert
        dataProviderStub.WasCalled.Should().BeTrue();
        fakeRepo.LastUpsertedPessoas.Should().BeNull();
        uowSpy.CommitCalls.Should().Be(0);
    }

    [Fact]
    public async Task ShouldNotCommit_WhenRepositoryUpsertThrows()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var throwingRepo = new ThrowingFakePessoasRepository([], new Exception("repository error"));
        var keysProviderStub = new StubPessoasImportKeyProvider([]);
        var dataProviderStub = new StubPessoasDataProvider([]);
        var uowSpy = new SpyUnitOfWork();

        var uut = new ImportPessoas(throwingRepo, dataProviderStub, keysProviderStub, uowSpy);

        // Act
        await Assert.ThrowsAsync<Exception>(() => uut.ExecuteAsync(ct));

        // Assert
        throwingRepo.WasCalled.Should().BeTrue();
        uowSpy.CommitCalls.Should().Be(0);
    }

    [Fact]
    public async Task ShouldPropagateException_WhenCommitThrows()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var fakeRepo = new FakePessoaRepository([]);
        var keysProviderStub = new StubPessoasImportKeyProvider([new("22601", "30001001")]);
        var dataProviderStub = new StubPessoasDataProvider([new Pessoa { NII = "22601", ExternalId = "30001001" }]);
        var throwingUow = new ThrowingUnitOfWork(new Exception("commit error"));

        var uut = new ImportPessoas(fakeRepo, dataProviderStub, keysProviderStub, throwingUow);

        // Act
        var act = () => uut.ExecuteAsync(ct);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("commit error");
        throwingUow.CommitCalls.Should().Be(1);
        throwingUow.LastToken.Should().Be(ct);
    }


    [Fact]
    public async Task ShouldExecuteDependenciesInOrder_WhenUseCaseRuns()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var importKeys = new ReadOnlyCollection<PessoaImportKey>([]);
        var pessoas = new ReadOnlyCollection<Pessoa>([]);

        var sequence = new MockSequence();

        _repo.InSequence(sequence)
            .Setup(r => r.GetExistingImportKeysAsync(ct))
            .ReturnsAsync(importKeys);

        _keysProvider.InSequence(sequence)
            .Setup(s => s.GetSourceImportKeysAsync(ct))
            .ReturnsAsync(importKeys);

        _dataProvider.InSequence(sequence)
            .Setup(s => s.GetPessoasByImportKeysAsync(importKeys, ct))
            .ReturnsAsync(pessoas);

        _repo.InSequence(sequence)
            .Setup(r => r.UpsertAllAsync(pessoas, ct))
            .Returns(Task.FromResult(new UpsertPessoasResult(0, 0)));

        _uow.InSequence(sequence)
            .Setup(u => u.CommitAsync(ct))
            .Returns(Task.CompletedTask);

        var uut = new ImportPessoas(_repo.Object, _dataProvider.Object, _keysProvider.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(ct);

        // Assert
        _repo.VerifyAll();
        _dataProvider.VerifyAll();
        _keysProvider.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task ShouldPropagateCancellationToken_WhenExecutingUseCase()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;


        var fakeRepo = new FakePessoaRepository([new("22601", "30001001")]);
        var keysProviderStub = new StubPessoasImportKeyProvider([new("21201", "30002001")]);
        var dataProviderStub = new StubPessoasDataProvider([new Pessoa { NII = "21201", ExternalId = "30002001" }]);
        var uowSpy = new SpyUnitOfWork();

        var uut = new ImportPessoas(fakeRepo, dataProviderStub, keysProviderStub, uowSpy);

        // Act
        await uut.ExecuteAsync(ct);

        // Assert
        fakeRepo.LastGetKeysToken.Should().Be(ct);
        fakeRepo.LastUpsertToken.Should().Be(ct);
        dataProviderStub.LastToken.Should().Be(ct);
        uowSpy.LastToken.Should().Be(ct);
    }
    public void Dispose()
    {
        _dataProvider = null!;
        _repo = null!;
        _keysProvider = null!;
        _uow = null!;
        GC.SuppressFinalize(this);
    }

}
