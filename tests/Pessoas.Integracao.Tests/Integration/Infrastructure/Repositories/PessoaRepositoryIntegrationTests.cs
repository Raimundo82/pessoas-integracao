using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.Enums;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Persistence;
using Pessoas.Integracao.Core.Infrastructure.Repositories;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.Repositories;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class PessoaRepositoryIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PessoaRepository _repository;
    private readonly EfUnitOfWork _uow;


    public PessoaRepositoryIntegrationTests(PostgresTestContainerDb db)
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
    public async Task AddAsync_WithRequiredFieldsOnly_ShouldPersistPessoa()
    {
        // Arrange
        var pessoa = new Pessoa { NII = "22600" };
        var result = await _repository.AddAsync(pessoa, CancellationToken.None);

        // Act
        await _uow.CommitAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        _context.Pessoas.Should().ContainSingle();
        var persistedPessoa = await _context.Pessoas.AsNoTracking().SingleAsync(p => p.Id == result.Id);
        persistedPessoa.Should().NotBeNull();
        persistedPessoa.Should().BeEquivalentTo(pessoa, opts => opts.Excluding(p => p.Id));
    }

    [Fact]
    public async Task AddAsync_WithAllFields_ShouldPersistCompletePessoa()
    {
        // Arrange
        var pessoa = new Pessoa
        {
            NII = "22600",
            ExternalId = "30002697",
            DadosPessoais = new DadosPessoais
            {
                NomeCompleto = "João Pacheco Raimundo",
                Apelidos = "Pacheco Raimundo",
                Sobrenome = "Raimundo",
                DataNascimento = new DateOnly(1982, 10, 18)
            },
            DadosBiometricos = new DadosBiometricos
            {
                AlturaEmCm = 176,
                CorDosOlhos = "Castanhos",
                TipoDeSangue = new TipoDeSangue
                {
                    GrupoSanguineo = GrupoSanguineo.O,
                    Rhesus = Rhesus.POSITIVO
                }
            }
        };
        var result = await _repository.AddAsync(pessoa, CancellationToken.None);

        // Act
        await _uow.CommitAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        _context.Pessoas.Should().ContainSingle();
        var persistedPessoa = await _context.Pessoas.AsNoTracking().SingleAsync(p => p.Id == result.Id);
        persistedPessoa.Should().NotBeNull();
        persistedPessoa.Should().BeEquivalentTo(pessoa, opts => opts.Excluding(p => p.Id));
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
        await _repository.AddRangeAsync(pessoas, CancellationToken.None);

        // Act
        await _uow.CommitAsync(CancellationToken.None);

        // Assert
        var persistedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        persistedPessoas.Should().HaveCount(2);
        persistedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "22601");
    }

    [Fact]
    public async Task AddAsync_WithDuplicateNii_ShouldFailWithUniqueViolation()
    {
        // Arrange
        await _context.AddAsync(new Pessoa { NII = "22600" });
        await _context.SaveChangesAsync();
        await _repository.AddAsync(new Pessoa { NII = "22600" }, CancellationToken.None);

        // Act
        Func<Task> action = async () => await _uow.CommitAsync(CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task ClearAllAsync_RemovesAllRecords()
    {
        // Arrange
        await _context.AddRangeAsync(new Pessoa { NII = "22600" }, new Pessoa { NII = "22601" });
        await _context.SaveChangesAsync();
        await _repository.ClearAllAsync(CancellationToken.None);

        // Act
        await _uow.CommitAsync(CancellationToken.None);

        // Assert
        var remainingPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        remainingPessoas.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenPessoasExist_ReturnsAllPessoas()
    {
        // Arrange
        var pessoas = new[]
        {
            new Pessoa { NII = "22600" },
            new Pessoa { NII = "22601" }
        };
        await _context.AddRangeAsync(pessoas);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(p => p.NII).Should().BeEquivalentTo("22600", "22601");
    }

    [Fact]
    public async Task GetAllAsync_WhenNoPessoasExist_ReturnsEmptyCollection()
    {
        // Act
        var result = await _repository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetExistingImportKeysAsync_ReturnsCorrectKeys()
    {
        // Arrange
        var pessoas = new[]
        {
            new Pessoa { NII = "11111", ExternalId = "EXT1" },
            new Pessoa { NII = "22222", ExternalId = "EXT2" }
        };
        await _context.AddRangeAsync(pessoas);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetExistingImportKeysAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().ContainSingle(k => k.Nii == "11111" && k.ExternalId == "EXT1");
        result.Should().ContainSingle(k => k.Nii == "22222" && k.ExternalId == "EXT2");
    }

    [Fact]
    public async Task GetExistingImportKeysAsync_WithNoPessoas_ShouldReturnEmptyList()
    {
        // Arrange
        _context.Pessoas.RemoveRange(_context.Pessoas);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetExistingImportKeysAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetExistingImportKeysAsync_WithNullExternalIds_ShouldHandleGracefully()
    {
        // Arrange
        var pessoas = new[]
        {
            new Pessoa { NII = "11111", ExternalId = null },
            new Pessoa { NII = "22222", ExternalId = "EXT2" }
        };
        await _context.AddRangeAsync(pessoas);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _repository.GetExistingImportKeysAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().ContainSingle(k => k.Nii == "11111" && k.ExternalId == null);
        result.Should().ContainSingle(k => k.Nii == "22222" && k.ExternalId == "EXT2");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
