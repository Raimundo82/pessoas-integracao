using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Repositories;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.Repositories;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class UpsertAllAsyncDbIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PessoaRepository _repository;


    public UpsertAllAsyncDbIntegrationTests(PostgresTestContainerDb db)
    {

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AppDbContext(options);
        _repository = new PessoaRepository(_context);
        _context.Database.EnsureCreated();
    }


    [Fact]
    public async Task ShouldInsertAllPessoas_WhenListContainsOnlyNewPessoas()
    {
        // Arrange
        var pessoas = new[]
        {
            new Pessoa { NII = "11111", ExternalId = "EXT1" },
            new Pessoa { NII = "22222", ExternalId = "EXT2" }
        };


        // Act
        await _repository.UpsertAllAsync(pessoas, CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("11111", "22222");
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("EXT1");
        savedPessoas.Should().ContainSingle(p => p.NII == "22222").Which.ExternalId.Should().Be("EXT2");
    }

    [Fact]
    public async Task ShouldUpdateAllPessoas_WhenListContainsOnlyExistingPessoas()
    {
        // Arrange
        await _context.Pessoas.AddRangeAsync(
            new Pessoa { NII = "11111", ExternalId = "OLD1" },
            new Pessoa { NII = "22222", ExternalId = "OLD2" }
        );
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var updatedPessoas = new[]
        {
            new Pessoa { NII = "11111", ExternalId = "NEW1" },
            new Pessoa { NII = "22222", ExternalId = "NEW2" }
        };

        // Act
        await _repository.UpsertAllAsync(updatedPessoas, CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("NEW1");
        savedPessoas.Should().ContainSingle(p => p.NII == "22222").Which.ExternalId.Should().Be("NEW2");
    }

    [Fact]
    public async Task ShouldHandleBothInsertAndUpdate_WhenListContainsMixOfNewAndExistingPessoas()
    {
        // Arrange
        await _context.Pessoas.AddAsync(new Pessoa { NII = "11111", ExternalId = "OLD1" });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var pessoas = new[]
        {
            new Pessoa { NII = "11111", ExternalId = "UPDATED1" },
            new Pessoa { NII = "22222", ExternalId = "NEW2" },
            new Pessoa { NII = "33333", ExternalId = "NEW3" }
        };

        // Act
        await _repository.UpsertAllAsync(pessoas, CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().HaveCount(3);
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("UPDATED1");
        savedPessoas.Should().ContainSingle(p => p.NII == "22222").Which.ExternalId.Should().Be("NEW2");
        savedPessoas.Should().ContainSingle(p => p.NII == "33333").Which.ExternalId.Should().Be("NEW3");
    }

    [Fact]
    public async Task ShouldUpdateValueObjectsAndPreserveId_WhenPessoaExists()
    {
        // Arrange
        var originalPessoa = new Pessoa
        {
            NII = "55555",
            ExternalId = "OLD_ID",
            DadosPessoais = new DadosPessoais
            {
                NomeCompleto = "Nome Antigo",
                Sobrenome = "Sobrenome Antigo",
                DataNascimento = new DateOnly(1980, 1, 1)
            },
            DadosBiometricos = new DadosBiometricos
            {
                CorDosOlhos = "Azuis",
                AlturaEmCm = 180
            }
        };
        await _context.Pessoas.AddAsync(originalPessoa);
        await _context.SaveChangesAsync();
        var originalId = originalPessoa.Id;
        _context.ChangeTracker.Clear();

        var updatedPessoa = new Pessoa
        {
            NII = "55555",
            ExternalId = "NEW_ID",
            DadosPessoais = new DadosPessoais
            {
                NomeCompleto = "Nome Novo",
                Sobrenome = "Sobrenome Novo",
                DataNascimento = new DateOnly(1990, 5, 15)
            },
            DadosBiometricos = new DadosBiometricos
            {
                CorDosOlhos = "Verdes",
                AlturaEmCm = 175
            }
        };

        // Act
        await _repository.UpsertAllAsync([updatedPessoa], CancellationToken.None);

        // Assert
        var savedPessoa = await _context.Pessoas.AsNoTracking().SingleAsync(p => p.NII == "55555");
        savedPessoa.Id.Should().Be(originalId);
        savedPessoa.ExternalId.Should().Be(updatedPessoa.ExternalId);
        savedPessoa.DadosPessoais.NomeCompleto.Should().Be(updatedPessoa.DadosPessoais.NomeCompleto);
        savedPessoa.DadosPessoais.Sobrenome.Should().Be(updatedPessoa.DadosPessoais.Sobrenome);
        savedPessoa.DadosPessoais.DataNascimento.Should().Be(updatedPessoa.DadosPessoais.DataNascimento);
        savedPessoa.DadosBiometricos.CorDosOlhos.Should().Be(updatedPessoa.DadosBiometricos.CorDosOlhos);
        savedPessoa.DadosBiometricos.AlturaEmCm.Should().Be(updatedPessoa.DadosBiometricos.AlturaEmCm);
    }

    [Fact]
    public async Task ShouldOverwriteExistingValueWithNull_WhenImportedPessoaHasNullField()
    {
        // Arrange
        var nii = "99999";
        var originalPessoa = new Pessoa
        {
            NII = nii,
            ExternalId = "EXT_OLD",
            DadosPessoais = new DadosPessoais { NomeCompleto = "Original Name" }
        };
        await _context.Pessoas.AddAsync(originalPessoa);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var updatedPessoa = new Pessoa
        {
            NII = nii,
            ExternalId = "EXT_OLD",
            DadosPessoais = new DadosPessoais { NomeCompleto = null }
        };

        // Act
        await _repository.UpsertAllAsync([updatedPessoa], CancellationToken.None);

        // Assert
        var savedPessoa = await _context.Pessoas.AsNoTracking().SingleAsync(p => p.NII == nii);
        savedPessoa.DadosPessoais.NomeCompleto.Should().BeNull();
    }

    [Fact]
    public async Task ShouldNotThrowAndMaintainExistingData_WhenListIsEmpty()
    {
        // Arrange
        await _context.Pessoas.AddAsync(new Pessoa { NII = "11111" });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        await _repository.UpsertAllAsync([], CancellationToken.None);

        // Assert
        var savedPessoas = await _context.Pessoas.AsNoTracking().ToListAsync();
        savedPessoas.Should().ContainSingle();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
