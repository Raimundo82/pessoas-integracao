using System.Collections.ObjectModel;

using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Tests.Unit.TestDoubles;

namespace Pessoas.Integracao.Tests.Unit.Application;

public class ImportPessoasByImporKeyUnitTests : IDisposable
{
    // Test dependencies
    private Mock<IPessoasDataProvider> _dataProvider;
    private Mock<IPessoaRepository> _repo;
    private Mock<IUnitOfWork> _uow;

    public ImportPessoasByImporKeyUnitTests()
    {
        // Setup runs before each test
        _dataProvider = new Mock<IPessoasDataProvider>();
        _repo = new Mock<IPessoaRepository>();
        _uow = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task ShouldReturnImportResult_WhenGivenMultipleImportKeysAndEmptyDb()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;

        var fakeRepo = new FakePessoaRepository([]);
        var inputKeys = new ReadOnlyCollection<PessoaImportKey>([new("22600", "30001000"), new("21200", "30002000")]);
        var pessoas = inputKeys.Select(k => new Pessoa { NII = k.Nii, ExternalId = k.ExternalId }).ToArray();

        var dataProviderStub = new StubPessoasDataProvider(pessoas);
        var uowSpy = new SpyUnitOfWork();
        var uut = new ImportPessoasByImportKey(fakeRepo, dataProviderStub, uowSpy);

        // Act (When)
        var result = await uut.ExecuteAsync(inputKeys, ct);

        // Assert (Then)
        result.Should().NotBeNull();
        result.TotalProcessed.Should().Be(2);
        result.TotalAdded.Should().Be(2);
        fakeRepo.LastUpsertToken.Should().Be(ct);
        dataProviderStub.LastRequestedKeys.Should().NotBeNull();
        dataProviderStub.LastRequestedKeys.Should().HaveCount(2);
        dataProviderStub.LastRequestedKeys.Should().Equal(inputKeys);
        dataProviderStub.LastToken.Should().Be(ct);
        uowSpy.CommitCalls.Should().Be(1);
        uowSpy.LastToken.Should().Be(ct);
    }

    [Fact]
    public async Task ShouldReturnImportResult_WhenGivenSignleImportKeyAndEmptyDb()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;

        var fakeRepo = new FakePessoaRepository([]);
        var inputKeys = new ReadOnlyCollection<PessoaImportKey>([new("22600", "30001000")]);
        var pessoas = inputKeys.Select(k => new Pessoa { NII = k.Nii, ExternalId = k.ExternalId }).ToArray();

        var dataProviderStub = new StubPessoasDataProvider(pessoas);
        var uowSpy = new SpyUnitOfWork();
        var uut = new ImportPessoasByImportKey(fakeRepo, dataProviderStub, uowSpy);

        // Act (When)
        var result = await uut.ExecuteAsync(inputKeys, ct);

        // Assert (Then)
        result.Should().NotBeNull();
        result.TotalProcessed.Should().Be(1);
        result.TotalAdded.Should().Be(1);
        fakeRepo.LastUpsertToken.Should().Be(ct);
        dataProviderStub.LastRequestedKeys.Should().NotBeNull();
        dataProviderStub.LastRequestedKeys.Should().HaveCount(1);
        dataProviderStub.LastRequestedKeys.Should().Equal(inputKeys);
        dataProviderStub.LastToken.Should().Be(ct);
        uowSpy.CommitCalls.Should().Be(1);
        uowSpy.LastToken.Should().Be(ct);
    }

    [Fact]
    public async Task ShouldReturnImportResult_WhenNoImportKeysArePassedAndWithEmptyDb()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;

        var fakeRepo = new FakePessoaRepository([]);
        var inputKeys = new ReadOnlyCollection<PessoaImportKey>([]);
        var pessoas = inputKeys.Select(k => new Pessoa { NII = k.Nii, ExternalId = k.ExternalId }).ToArray();

        var dataProviderStub = new StubPessoasDataProvider(pessoas);
        var uowSpy = new SpyUnitOfWork();
        var uut = new ImportPessoasByImportKey(fakeRepo, dataProviderStub, uowSpy);

        // Act (When)
        var result = await uut.ExecuteAsync(inputKeys, ct);

        // Assert (Then)
        result.Should().NotBeNull();
        result.TotalProcessed.Should().Be(0);
        result.TotalAdded.Should().Be(0);
        fakeRepo.LastUpsertToken.Should().BeNull();
        fakeRepo.LastUpsertedPessoas.Should().BeNull();
        dataProviderStub.LastRequestedKeys.Should().NotBeNull();
        dataProviderStub.LastRequestedKeys.Should().HaveCount(0);
        dataProviderStub.LastToken.Should().Be(ct);
        uowSpy.CommitCalls.Should().Be(0);
        uowSpy.LastToken.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotUpsertOrCommit_WhenPessoaDataProviderThrows()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var fakeRepo = new FakePessoaRepository([]);
        var inputKeys = new ReadOnlyCollection<PessoaImportKey>([new("22600", "30001000")]);
        var dataProviderException = new Exception("source error");
        var dataProviderStub = new ThrowingPessoasDataProvider(dataProviderException);
        var uowSpy = new SpyUnitOfWork();

        var uut = new ImportPessoasByImportKey(fakeRepo, dataProviderStub, uowSpy);

        // Act
        await Assert.ThrowsAsync<Exception>(() => uut.ExecuteAsync(inputKeys, ct));

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
        var inputKeys = new ReadOnlyCollection<PessoaImportKey>([new("22600", "30001000")]);
        var dataProviderStub = new StubPessoasDataProvider([.. inputKeys.Select(k => new Pessoa { NII = k.Nii, ExternalId = k.ExternalId })]);
        var uowSpy = new SpyUnitOfWork();

        var uut = new ImportPessoasByImportKey(throwingRepo, dataProviderStub, uowSpy);

        // Act
        await Assert.ThrowsAsync<Exception>(() => uut.ExecuteAsync(inputKeys, ct));

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
        var inputKeys = new ReadOnlyCollection<PessoaImportKey>([new("22600", "30001000")]);
        var dataProviderStub = new StubPessoasDataProvider([.. inputKeys.Select(k => new Pessoa { NII = k.Nii, ExternalId = k.ExternalId })]);
        var throwingUow = new ThrowingUnitOfWork(new Exception("commit error"));
        var uut = new ImportPessoasByImportKey(fakeRepo, dataProviderStub, throwingUow);

        // Act
        var act = () => uut.ExecuteAsync(inputKeys, ct);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("commit error");
        throwingUow.CommitCalls.Should().Be(1);
        throwingUow.LastToken.Should().Be(ct);
    }


    [Fact]
    public async Task ShouldExecuteDependenciesInOrder_WhenImportKeysInputIsEmpty()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var inputKeys = new ReadOnlyCollection<PessoaImportKey>([]);
        var pessoas = inputKeys.Select(k => new Pessoa { NII = k.Nii, ExternalId = k.ExternalId }).ToArray();
        var sequence = new MockSequence();

        _dataProvider.InSequence(sequence)
            .Setup(s => s.GetPessoasByImportKeysAsync(inputKeys, ct))
            .ReturnsAsync(pessoas);

        var uut = new ImportPessoasByImportKey(_repo.Object, _dataProvider.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(inputKeys, ct);

        // Assert
        _repo.VerifyAll();
        _dataProvider.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task ShouldExecuteDependenciesInOrder_WhenImportKeysInputIsNotEmpty()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var inputKeys = new ReadOnlyCollection<PessoaImportKey>([new("22600", "30001000")]);
        var pessoas = inputKeys.Select(k => new Pessoa { NII = k.Nii, ExternalId = k.ExternalId }).ToArray();
        var sequence = new MockSequence();

        _dataProvider.InSequence(sequence)
            .Setup(s => s.GetPessoasByImportKeysAsync(inputKeys, ct))
            .ReturnsAsync(pessoas);

        _repo.InSequence(sequence)
            .Setup(r => r.UpsertAllAsync(pessoas, ct))
            .Returns(Task.FromResult(new UpsertPessoasResult(0, 0)));

        _uow.InSequence(sequence)
            .Setup(u => u.CommitAsync(ct))
            .Returns(Task.CompletedTask);

        var uut = new ImportPessoasByImportKey(_repo.Object, _dataProvider.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(inputKeys, ct);

        // Assert
        _repo.VerifyAll();
        _dataProvider.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task ShouldPropagateCancellationToken_WhenExecutingUseCase()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;


        var fakeRepo = new FakePessoaRepository([]);
        var inputKeys = new ReadOnlyCollection<PessoaImportKey>([new("22600", "30001000")]);
        var dataProviderStub = new StubPessoasDataProvider([.. inputKeys.Select(k => new Pessoa { NII = k.Nii, ExternalId = k.ExternalId })]);
        var uowSpy = new SpyUnitOfWork();

        var uut = new ImportPessoasByImportKey(fakeRepo, dataProviderStub, uowSpy);

        // Act
        await uut.ExecuteAsync(inputKeys, ct);

        // Assert
        fakeRepo.LastUpsertToken.Should().Be(ct);
        dataProviderStub.LastToken.Should().Be(ct);
        uowSpy.LastToken.Should().Be(ct);
    }
    public void Dispose()
    {
        _dataProvider = null!;
        _repo = null!;
        _uow = null!;
        GC.SuppressFinalize(this);
    }

}