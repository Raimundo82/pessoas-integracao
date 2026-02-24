using System.Collections.ObjectModel;

using Moq;

using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.DTOs;
using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Entities;

namespace Pessoas.Integracao.Tests.Unit.Application;

public sealed class ImportAllPessoasUnitTests : IDisposable
{
    // Test dependencies
    private Mock<IPessoasProvider> _source;
    private Mock<IPessoaRepository> _repo;
    private Mock<IUnitOfWork> _uow;

    public ImportAllPessoasUnitTests()
    {
        // Setup runs before each test
        _source = new Mock<IPessoasProvider>();
        _repo = new Mock<IPessoaRepository>();
        _uow = new Mock<IUnitOfWork>();
    }

    [Fact]
    public async Task ImportAllAsync_WhenCalled_AddOrUpdateAllPessoas()
    {
        // Arrange (Given)
        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 1, NII = "22600"},
            new() { Id = 2, NII = "21200" }
        ]);

        var importNiis = new ReadOnlyCollection<ImportNiiDto>(
        [
            new("22600"),
            new("21200")
        ]);

        _source.Setup(s => s.GetPessoasByNiiAsync(importNiis, It.IsAny<CancellationToken>())).ReturnsAsync(pessoas);
        _source.Setup(s => s.GetProviderImportNiisAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importNiis);

        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(pessoas);

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act (When)
        await uut.ExecuteAsync(CancellationToken.None);

        // Assert (Then)
        _repo.Verify(r => r.AddOrUpdateAllAsync(pessoas, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSourceReturnsEmptyCollection_PreserveExistingPessoas()
    {
        // Arrange
        var pessoas = new ReadOnlyCollection<Pessoa>([]);
        var pessoasInDb = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 1, NII = "22600"},
            new() { Id = 2, NII = "21200" }
        ]);
        var importNiis = new ReadOnlyCollection<ImportNiiDto>([]);

        _source.Setup(s => s.GetPessoasByNiiAsync(It.IsAny<IReadOnlyList<ImportNiiDto>>(), It.IsAny<CancellationToken>())).ReturnsAsync(pessoas);
        _source.Setup(s => s.GetProviderImportNiisAsync(It.IsAny<CancellationToken>())).ReturnsAsync(importNiis);


        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(pessoasInDb);

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(CancellationToken.None);

        // Assert
        _repo.Verify(r => r.AddOrUpdateAllAsync(pessoas, It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

    }

    [Fact]
    public async Task ExecuteAsync_WhenSourceThrows_DoesNotModifyRepositoryOrCommit()
    {
        // Arrange
        var importNiis = new ReadOnlyCollection<ImportNiiDto>([]);

        _source.Setup(s => s.GetPessoasByNiiAsync(importNiis, It.IsAny<CancellationToken>()))
              .ThrowsAsync(new Exception("source error"));
        _source.Setup(s => s.GetProviderImportNiisAsync(It.IsAny<CancellationToken>()))
              .ThrowsAsync(new Exception("source error"));

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => uut.ExecuteAsync(CancellationToken.None));

        // Assert
        _repo.Verify(r => r.AddOrUpdateAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_AddOrUpdateBeforeCommit()
    {
        // Arrange
        var pessoas = new ReadOnlyCollection<Pessoa>([]);
        var importNiis = new ReadOnlyCollection<ImportNiiDto>([]);
        var sequence = new MockSequence();

        _source.Setup(s => s.GetPessoasByNiiAsync(importNiis, It.IsAny<CancellationToken>())).ReturnsAsync(pessoas);
        _source.Setup(s => s.GetProviderImportNiisAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);


        _repo.InSequence(sequence)
            .Setup(r => r.AddOrUpdateAllAsync(pessoas, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(pessoas);

        _uow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(CancellationToken.None);

        // Assert
        _repo.VerifyAll();
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCancellationToken()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;

        var pessoas = new ReadOnlyCollection<Pessoa>([]);
        var importNiis = new ReadOnlyCollection<ImportNiiDto>([]);

        _source.Setup(s => s.GetPessoasByNiiAsync(importNiis, ct)).ReturnsAsync(pessoas);
        _source.Setup(s => s.GetProviderImportNiisAsync(ct)).ReturnsAsync(importNiis);


        _repo.Setup(r => r.AddOrUpdateAllAsync(pessoas, ct)).Returns(Task.CompletedTask);
        _repo.Setup(r => r.GetAllAsync(ct)).ReturnsAsync(pessoas);

        _uow.Setup(u => u.CommitAsync(ct)).Returns(Task.CompletedTask);

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(ct);

        // Assert
        _source.VerifyAll();
        _repo.VerifyAll();
        _uow.VerifyAll();
    }

    public void Dispose()
    {
        _source = null!;
        _repo = null!;
        _uow = null!;
        GC.SuppressFinalize(this);
    }

}