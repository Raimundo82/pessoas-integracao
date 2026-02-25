using System.Collections.ObjectModel;

using FluentAssertions;

using Moq;

using Pessoas.Integracao.Core.Application.Abstractions;
using Pessoas.Integracao.Core.Application.Contracts;
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
        var ct = new CancellationTokenSource().Token;
        var pessoas = new ReadOnlyCollection<Pessoa>(
        [
            new() { Id = 1, NII = "22600"},
            new() { Id = 2, NII = "21200" }
        ]);
        _source.Setup(s => s.GetPessoasAsync(ct)).ReturnsAsync(pessoas);

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act (When)
        await uut.ExecuteAsync(ct);

        // Assert (Then)
        _repo.Verify(r => r.AddOrUpdateAllAsync(pessoas, ct), Times.Once);
        _uow.Verify(u => u.CommitAsync(ct), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSourceReturnsEmptyCollection_PreserveExistingPessoas()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var pessoas = new ReadOnlyCollection<Pessoa>([]);

        _source.Setup(s => s.GetPessoasAsync(ct))
              .ReturnsAsync(pessoas);

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(ct);

        // Assert
        _repo.Verify(r => r.AddOrUpdateAllAsync(pessoas, ct), Times.Once);
        _uow.Verify(u => u.CommitAsync(ct), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSourceThrows_DoesNotModifyRepositoryOrCommit()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        _source.Setup(s => s.GetPessoasAsync(ct))
          .ThrowsAsync(new Exception("source error"));

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => uut.ExecuteAsync(ct));

        // Assert
        _repo.Verify(r => r.AddOrUpdateAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), ct), Times.Never);
        _uow.Verify(u => u.CommitAsync(ct), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_AddOrUpdateBeforeCommit()
    {
        // Arrange
        var pessoas = new ReadOnlyCollection<Pessoa>([]);
        var ct = new CancellationTokenSource().Token;

        var sequence = new MockSequence();

        _source.Setup(s => s.GetPessoasAsync(ct)).ReturnsAsync(pessoas);

        _repo.InSequence(sequence)
            .Setup(r => r.AddOrUpdateAllAsync(pessoas, ct))
            .Returns(Task.CompletedTask);

        _uow.Setup(u => u.CommitAsync(ct))
           .Returns(Task.CompletedTask);

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

        // Act
        await uut.ExecuteAsync(ct);

        // Assert
        _repo.VerifyAll();
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCancellationToken()
    {
        // Arrange
        var ct = new CancellationTokenSource().Token;
        var pessoas = new ReadOnlyCollection<Pessoa>([]);

        CancellationToken? capturedSourceToken = null;
        CancellationToken? capturedRepoToken = null;
        CancellationToken? capturedUowToken = null;

        _source
            .Setup(s => s.GetPessoasAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(token => capturedSourceToken = token)
            .ReturnsAsync(pessoas);

        _repo
            .Setup(r => r.AddOrUpdateAllAsync(It.IsAny<IReadOnlyList<Pessoa>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<Pessoa>, CancellationToken>((_, token) => capturedRepoToken = token)
            .Returns(Task.CompletedTask);

        _uow
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(token => capturedUowToken = token)
            .Returns(Task.CompletedTask);

        var uut = new ImportAllPessoas(_repo.Object, _source.Object, _uow.Object);

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