using System.Collections.ObjectModel;

using FluentAssertions;


using Moq;

using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.Models;

using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Tests.Unit.Application;

public sealed class ImportPessoasUnitTests : IDisposable
{
    // Test dependencies
    private Mock<IPessoasProvider> _source;
    private Mock<IPessoaRepository> _repo;
    private Mock<IUnitOfWork> _uow;

    public ImportPessoasUnitTests()
    {
        // Setup runs before each test
        _source = new Mock<IPessoasProvider>();
        _repo = new Mock<IPessoaRepository>();
        _uow = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task ExecuteAsync_GivenPopulatedRepoAndSource_UpsertsMergedPessoasAndCommitsAsync()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;

        _repo.Setup(r => r.GetExistingImportKeysAsync(ct)).ReturnsAsync([new("22600", "30001000"), new("21200", "30002000")]);
        _source.Setup(s => s.GetSourceImportKeysAsync(ct)).ReturnsAsync([new("22601", "30001001"), new("21201", "30002001")]);

        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 1, NII = "22600", ExternalId = "30001000" },
            new() { Id = 2, NII = "21200", ExternalId = "30002000" },
            new() { Id = 3, NII = "22601", ExternalId = "30001001" },
            new() { Id = 4, NII = "21201", ExternalId = "30002001" }
        ]);

        _source.Setup(s => s.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct)).ReturnsAsync(pessoas);

        var uut = new ImportPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act (When)
        await uut.ExecuteAsync(ct);

        // Assert (Then)
        _repo.Verify(r => r.GetExistingImportKeysAsync(ct), Times.Once);
        _source.Verify(s => s.GetSourceImportKeysAsync(ct), Times.Once);
        _source.Verify(s => s.GetPessoasByImportKeysAsync(It.Is<IReadOnlyList<PessoaImportKey>>(keys =>
            keys.Count == 4 &&
            keys.Any(k => k.Nii == "22601" && k.ExternalId == "30001001") &&
            keys.Any(k => k.Nii == "21201" && k.ExternalId == "30002001") &&
            keys.Any(k => k.Nii == "22600" && k.ExternalId == "30001000") &&
            keys.Any(k => k.Nii == "21200" && k.ExternalId == "30002000")
        ), ct), Times.Once);
        _repo.Verify(r => r.AddOrUpdateAllAsync(It.Is<IReadOnlyList<Pessoa>>(
            pessoas => pessoas.Count == 4 &&
            pessoas.Any(p => p.NII == "22600" && p.ExternalId == "30001000") &&
            pessoas.Any(p => p.NII == "21200" && p.ExternalId == "30002000") &&
            pessoas.Any(p => p.NII == "22601" && p.ExternalId == "30001001") &&
            pessoas.Any(p => p.NII == "21201" && p.ExternalId == "30002001")
            ), ct), Times.Once);
        _uow.Verify(u => u.CommitAsync(ct), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_GivenEmptyRepo_UpsertsSourcePessoasAndCommitsAsync()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;

        _repo.Setup(r => r.GetExistingImportKeysAsync(ct)).ReturnsAsync([]);
        _source.Setup(s => s.GetSourceImportKeysAsync(ct)).ReturnsAsync([new("22601", "30001001"), new("21201", "30002001")]);

        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 3, NII = "22601", ExternalId = "30001001" },
            new() { Id = 4, NII = "21201", ExternalId = "30002001" }
        ]);

        _source.Setup(s => s.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct)).ReturnsAsync(pessoas);

        var uut = new ImportPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act (When)
        await uut.ExecuteAsync(ct);

        // Assert (Then)
        _repo.Verify(r => r.GetExistingImportKeysAsync(ct), Times.Once);
        _source.Verify(s => s.GetSourceImportKeysAsync(ct), Times.Once);
        _source.Verify(s => s.GetPessoasByImportKeysAsync(It.Is<IReadOnlyList<PessoaImportKey>>(keys =>
            keys.Count == 2 &&
            keys.Any(k => k.Nii == "22601" && k.ExternalId == "30001001") &&
            keys.Any(k => k.Nii == "21201" && k.ExternalId == "30002001")
        ), ct), Times.Once);
        _repo.Verify(r => r.AddOrUpdateAllAsync(It.Is<IReadOnlyList<Pessoa>>(
            pessoas => pessoas.Count == 2 &&
            pessoas.Any(p => p.NII == "22601" && p.ExternalId == "30001001") &&
            pessoas.Any(p => p.NII == "21201" && p.ExternalId == "30002001")
            ), ct), Times.Once);
        _uow.Verify(u => u.CommitAsync(ct), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_GivenEmptySource_UpsertsRepoPessoasAndCommitsAsync()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;

        _repo.Setup(r => r.GetExistingImportKeysAsync(ct)).ReturnsAsync([new("22601", "30001001"), new("21201", "30002001")]);
        _source.Setup(s => s.GetSourceImportKeysAsync(ct)).ReturnsAsync([]);

        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 3, NII = "22601", ExternalId = "30001001" },
            new() { Id = 4, NII = "21201", ExternalId = "30002001" }
        ]);

        _source.Setup(s => s.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct)).ReturnsAsync(pessoas);

        var uut = new ImportPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act (When)
        await uut.ExecuteAsync(ct);

        // Assert (Then)
        _repo.Verify(r => r.GetExistingImportKeysAsync(ct), Times.Once);
        _source.Verify(s => s.GetSourceImportKeysAsync(ct), Times.Once);
        _source.Verify(s => s.GetPessoasByImportKeysAsync(It.Is<IReadOnlyList<PessoaImportKey>>(keys =>
            keys.Count == 2 &&
            keys.Any(k => k.Nii == "22601" && k.ExternalId == "30001001") &&
            keys.Any(k => k.Nii == "21201" && k.ExternalId == "30002001")
        ), ct), Times.Once);
        _repo.Verify(r => r.AddOrUpdateAllAsync(It.Is<IReadOnlyList<Pessoa>>(
            pessoas => pessoas.Count == 2 &&
            pessoas.Any(p => p.NII == "22601" && p.ExternalId == "30001001") &&
            pessoas.Any(p => p.NII == "21201" && p.ExternalId == "30002001")
            ), ct), Times.Once);
        _uow.Verify(u => u.CommitAsync(ct), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_GivenDuplicatedKeysSourceAndRepo_UpsertsDistinctPessoasAndCommitsAsync()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;

        _repo.Setup(r => r.GetExistingImportKeysAsync(ct)).ReturnsAsync([new("22601", "30001001"), new("21201", "30002001")]);
        _source.Setup(s => s.GetSourceImportKeysAsync(ct)).ReturnsAsync([new("22601", "30001001"), new("21201", "30002001")]);

        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 3, NII = "22601", ExternalId = "30001001" },
            new() { Id = 4, NII = "21201", ExternalId = "30002001" }
        ]);

        _source.Setup(s => s.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct)).ReturnsAsync(pessoas);

        var uut = new ImportPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act (When)
        await uut.ExecuteAsync(ct);

        // Assert (Then)
        _repo.Verify(r => r.GetExistingImportKeysAsync(ct), Times.Once);
        _source.Verify(s => s.GetSourceImportKeysAsync(ct), Times.Once);
        _source.Verify(s => s.GetPessoasByImportKeysAsync(It.Is<IReadOnlyList<PessoaImportKey>>(keys =>
            keys.Count == 2 &&
            keys.Any(k => k.Nii == "22601" && k.ExternalId == "30001001") &&
            keys.Any(k => k.Nii == "21201" && k.ExternalId == "30002001")
        ), ct), Times.Once);
        _repo.Verify(r => r.AddOrUpdateAllAsync(It.Is<IReadOnlyList<Pessoa>>(
            pessoas => pessoas.Count == 2 &&
            pessoas.Any(p => p.NII == "22601" && p.ExternalId == "30001001") &&
            pessoas.Any(p => p.NII == "21201" && p.ExternalId == "30002001")
            ), ct), Times.Once);
        _uow.Verify(u => u.CommitAsync(ct), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_GivenSomeDuplicatedKeysInSourceAndRepo_UpsertsDistinctPessoasAndCommitsAsync()
    {
        // Arrange (Given)
        var ct = new CancellationTokenSource().Token;

        _repo.Setup(r => r.GetExistingImportKeysAsync(ct)).ReturnsAsync([new("22601", "30001001"), new("21200", "30002000")]);
        _source.Setup(s => s.GetSourceImportKeysAsync(ct)).ReturnsAsync([new("22601", "30001001"), new("21201", "30002001")]);

        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 2, NII = "21200", ExternalId = "30002000" },
            new() { Id = 3, NII = "22601", ExternalId = "30001001" },
            new() { Id = 4, NII = "21201", ExternalId = "30002001" }

        ]);

        _source.Setup(s => s.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), ct)).ReturnsAsync(pessoas);

        var uut = new ImportPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act (When)
        await uut.ExecuteAsync(ct);

        // Assert (Then)
        _repo.Verify(r => r.GetExistingImportKeysAsync(ct), Times.Once);
        _source.Verify(s => s.GetSourceImportKeysAsync(ct), Times.Once);
        _source.Verify(s => s.GetPessoasByImportKeysAsync(It.Is<IReadOnlyList<PessoaImportKey>>(keys =>
            keys.Count == 3 &&
            keys.Any(k => k.Nii == "21200" && k.ExternalId == "30002000") &&
            keys.Any(k => k.Nii == "22601" && k.ExternalId == "30001001") &&
            keys.Any(k => k.Nii == "21201" && k.ExternalId == "30002001")
        ), ct), Times.Once);
        _repo.Verify(r => r.AddOrUpdateAllAsync(It.Is<IReadOnlyList<Pessoa>>(
            pessoas => pessoas.Count == 3 &&
            pessoas.Any(p => p.NII == "21200" && p.ExternalId == "30002000") &&
            pessoas.Any(p => p.NII == "22601" && p.ExternalId == "30001001") &&
            pessoas.Any(p => p.NII == "21201" && p.ExternalId == "30002001")
            ), ct), Times.Once);
        _uow.Verify(u => u.CommitAsync(ct), Times.Once);
    }


    [Fact]
    public async Task ExecuteAsync_WhenSourceThrows_DoesNotModifyRepositoryOrCommit()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var importKeys = new ReadOnlyCollection<PessoaImportKey>([]);
        _repo.Setup(r => r.GetExistingImportKeysAsync(ct)).ReturnsAsync(importKeys);
        _source.Setup(s => s.GetSourceImportKeysAsync(ct)).ReturnsAsync(importKeys);

        _source.Setup(s => s.GetPessoasByImportKeysAsync(importKeys, ct))
          .ThrowsAsync(new Exception("source error"));

        var uut = new ImportPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => uut.ExecuteAsync(ct));

        // Assert
        _repo.Verify(r => r.AddOrUpdateAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), ct), Times.Never);
        _uow.Verify(u => u.CommitAsync(ct), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_Always_ExecutesStepsInExpectedOrderAsync()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var importKeys = new ReadOnlyCollection<PessoaImportKey>([]);
        var pessoas = new ReadOnlyCollection<Pessoa>([]);

        var sequence = new MockSequence();

        _repo.InSequence(sequence)
            .Setup(r => r.GetExistingImportKeysAsync(ct))
            .ReturnsAsync(importKeys);

        _source.InSequence(sequence)
            .Setup(s => s.GetSourceImportKeysAsync(ct))
            .ReturnsAsync(importKeys);

        _source.InSequence(sequence)
            .Setup(s => s.GetPessoasByImportKeysAsync(importKeys, ct))
            .ReturnsAsync(pessoas);

        _repo.InSequence(sequence)
            .Setup(r => r.AddOrUpdateAllAsync(pessoas, ct))
            .Returns(Task.CompletedTask);

        _uow.InSequence(sequence)
            .Setup(u => u.CommitAsync(ct))
            .Returns(Task.CompletedTask);

        var uut = new ImportPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(ct);

        // Assert
        _repo.VerifyAll();
        _source.VerifyAll();
        _uow.VerifyAll();
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCancellationToken()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var pessoas = new ReadOnlyCollection<Pessoa>([]);
        var importKeys = new ReadOnlyCollection<PessoaImportKey>([]);

        CancellationToken? capturedSourceToken = null;
        CancellationToken? capturedRepoToken = null;
        CancellationToken? capturedUowToken = null;

        _repo
            .Setup(r => r.GetExistingImportKeysAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(token => capturedRepoToken = token)
            .ReturnsAsync(importKeys);

        _source
            .Setup(s => s.GetSourceImportKeysAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(token => capturedSourceToken = token)
            .ReturnsAsync(importKeys);

        _source
            .Setup(s => s.GetPessoasByImportKeysAsync(It.IsAny<IReadOnlyList<PessoaImportKey>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<PessoaImportKey>, CancellationToken>((_, token) => capturedSourceToken = token)
            .ReturnsAsync(pessoas);

        _repo
            .Setup(r => r.AddOrUpdateAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<Pessoa>, CancellationToken>((_, token) => capturedRepoToken = token)
            .Returns(Task.CompletedTask);

        _uow
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(token => capturedUowToken = token)
            .Returns(Task.CompletedTask);

        var uut = new ImportPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(ct);

        // Assert
        capturedSourceToken.Should().Be(ct);
        capturedRepoToken.Should().Be(ct);
        capturedUowToken.Should().Be(ct);
    }
    public void Dispose()
    {
        _source = null!;
        _repo = null!;
        _uow = null!;
        GC.SuppressFinalize(this);
    }

}