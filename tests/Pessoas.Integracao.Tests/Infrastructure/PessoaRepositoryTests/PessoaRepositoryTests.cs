using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Persistence;
using Pessoas.Integracao.Core.Infrastructure.Repositories;

namespace Pessoas.Integracao.Core.Tests.Infrastructure.PessoaRepositoryTests;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class PessoaRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PessoaRepository _repository;
    private readonly EfUnitOfWork _uow;


    public PessoaRepositoryTests(PostgresTestContainerDb db)
    {

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AppDbContext(options);
        _repository = new PessoaRepository(_context);
        _uow = new EfUnitOfWork(_context);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistPessoaToDb()
    {
        // Arrange
        var pessoa = new Pessoa { NII = "22600" };

        // Act
        var result = await _repository.AddAsync(pessoa, CancellationToken.None);
        await _uow.CommitAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(_context.Pessoas);
        var savedPessoa = await _context.Pessoas.FindAsync(result.Id);
        Assert.NotNull(savedPessoa);
        Assert.Equal(pessoa.NII, savedPessoa.NII);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldPersistMultiplePessoasToDb()
    {
        // Arrange
        var pessoas = new[]
        {
        new Pessoa { NII = "22600" },
        new Pessoa { NII = "22601" }
    };

        // Act
        await _repository.AddRangeAsync(pessoas, CancellationToken.None);
        await _uow.CommitAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, await _context.Pessoas.CountAsync());
    }

    [Fact]
    public async Task AddAsync_WithDuplicateNii_ShouldFailWithUniqueViolation()
    {
        // Arrange
        await _context.AddAsync(new Pessoa { NII = "22600" });
        await _context.SaveChangesAsync();

        // Act
        await _repository.AddAsync(new Pessoa { NII = "22600" }, CancellationToken.None);
        async Task AddAction() => await _uow.CommitAsync(CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(AddAction);
    }

    [Fact]
    public async Task ClearAllAsync_RemovesAllRecords()
    {
        // Arrange
        await _context.AddRangeAsync(new Pessoa { NII = "22600" }, new Pessoa { NII = "22601" });
        await _context.SaveChangesAsync();

        // Act
        await _repository.ClearAllAsync(CancellationToken.None);
        await _uow.CommitAsync(CancellationToken.None);

        // Assert
        Assert.Empty(_context.Pessoas);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        GC.SuppressFinalize(this);
    }

}