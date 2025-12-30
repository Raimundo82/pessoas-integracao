using System.Collections.ObjectModel;

using Moq;

using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
using Pessoas.Integracao.Core.Application.UseCases;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.Interfaces;

namespace Pessoas.Integracao.Tests.Application.ImportAllPessoasTests;

public sealed class ImportAllPessoasTests : IDisposable
{
    // Test dependencies
    private Mock<IPessoasSource> _source;
    private Mock<IPessoaRepository> _repo;
    private Mock<IUnitOfWork> _uow;

    public ImportAllPessoasTests()
    {
        // Setup runs before each test
        _source = new Mock<IPessoasSource>();
        _repo = new Mock<IPessoaRepository>();
        _uow = new Mock<IUnitOfWork>();
    }

    public void Dispose()
    {
        _source = null!;
        _repo = null!;
        _uow = null!;
        GC.SuppressFinalize(this);
    }
    [Fact]
    public async Task ImportAllAsync_WhenCalled_ReplacesAllPessoas()
    {
        // Arrange (Given)
        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 1, NII = "22600"},
            new() { Id = 2, NII = "21200" }
        ]);
        _source
            .Setup(s => s.GetPessoasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoas);

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act (When)
        await uut.ExecuteAsync(CancellationToken.None);

        // Assert (Then)
        _repo.Verify(r => r.ClearAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddRangeAsync(pessoas, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSourceReturnsEmptyCollection_ClearsExistingPessoas()
    {
        // Arrange
        var pessoas = new ReadOnlyCollection<Pessoa>([]);

        _source.Setup(s => s.GetPessoasAsync(It.IsAny<CancellationToken>()))
              .ReturnsAsync(pessoas);

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(CancellationToken.None);

        // Assert
        _repo.Verify(r => r.ClearAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repo.Verify(r => r.AddRangeAsync(pessoas, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSourceThrows_DoesNotModifyRepositoryOrCommit()
    {
        // Arrange
        _source.Setup(s => s.GetPessoasAsync(It.IsAny<CancellationToken>()))
              .ThrowsAsync(new Exception("source error"));



        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => uut.ExecuteAsync(CancellationToken.None));

        // Assert
        _repo.Verify(r => r.ClearAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.AddRangeAsync(It.IsAny<IReadOnlyCollection<Pessoa>>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ClearsBeforeAddingPessoas()
    {
        // Arrange
        var pessoas = new ReadOnlyCollection<Pessoa>([]);

        var sequence = new MockSequence();

        _source.Setup(s => s.GetPessoasAsync(It.IsAny<CancellationToken>())).ReturnsAsync(pessoas);

        _repo.InSequence(sequence)
            .Setup(r => r.ClearAllAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repo.InSequence(sequence)
            .Setup(r => r.AddRangeAsync(pessoas, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

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

        _source.Setup(s => s.GetPessoasAsync(ct)).ReturnsAsync(pessoas);

        _repo.Setup(r => r.ClearAllAsync(ct)).Returns(Task.CompletedTask);

        _repo.Setup(r => r.AddRangeAsync(pessoas, ct)).Returns(Task.CompletedTask);

        _uow.Setup(u => u.CommitAsync(ct)).Returns(Task.CompletedTask);

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(ct);

        // Assert
        _source.VerifyAll();
        _repo.VerifyAll();
        _uow.VerifyAll();
    }

}