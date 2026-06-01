using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Repositories;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.PessoasRepository;


[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class ReplaceAllAsyncDbIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly AppDbContext _context;
    private readonly PessoaRepository _repository;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly PostgresTestContainerDb _db;


    public ReplaceAllAsyncDbIntegrationTests(PostgresTestContainerDb db)
    {
        _db = db;
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AppDbContext(_options);
        _repository = new PessoaRepository(_context);
    }
    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    [Fact]
    public async Task ShouldReplaceAllPessoas_WhenReplacingAll()
    {
        // Arrange
        var seeded = await SeedAsync(
            new Pessoa { NII = "11111", ExternalId = "OLD1" },
            new Pessoa { NII = "22222", ExternalId = "OLD2" }
        );

        var oldId1 = seeded.First(p => p.NII == "11111").Id;
        var oldId2 = seeded.First(p => p.NII == "22222").Id;

        var input = new[]
        {
            new Pessoa { NII = "11111", ExternalId = "NEW1" },
            new Pessoa { NII = "22222", ExternalId = "NEW2" }
        };

        // Act
        await _repository.ReplaceAllAsync(input, _ct);

        // Assert
        var result = await ReadAllPessoasAsync();
        result.Should().HaveCount(2);

        var p1 = result.Should().ContainSingle(p => p.NII == "11111").Which;
        p1.ExternalId.Should().Be("NEW1");
        p1.Id.Should().NotBe(oldId1);

        var p2 = result.Should().ContainSingle(p => p.NII == "22222").Which;
        p2.ExternalId.Should().Be("NEW2");
        p2.Id.Should().NotBe(oldId2);
    }

    [Fact]
    public async Task ShouldHandleBothNewAndExisting_WhenReplacingAll()
    {
        // Arrange
        await SeedAsync(new Pessoa { NII = "11111", ExternalId = "OLD1" });

        var pessoas = new[]
        {
            new Pessoa { NII = "11111", ExternalId = "UPDATED1" },
            new Pessoa { NII = "22222", ExternalId = "NEW2" },
            new Pessoa { NII = "33333", ExternalId = "NEW3" }
        };

        // Act
        await _repository.ReplaceAllAsync(pessoas, _ct);

        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().HaveCount(3);
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("UPDATED1");
        savedPessoas.Should().ContainSingle(p => p.NII == "22222").Which.ExternalId.Should().Be("NEW2");
        savedPessoas.Should().ContainSingle(p => p.NII == "33333").Which.ExternalId.Should().Be("NEW3");
    }

    [Fact]
    public async Task ShouldUpdateAllPropertiesOfExistingPessoa_WhenReplacingAll()
    {
        // Arrange
        var existingPessoa = new Pessoa
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
        await SeedAsync(existingPessoa);
        var existingId = existingPessoa.Id;

        var pessoas = new[]
        {
            new Pessoa
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
            }
        };

        // Act
        await _repository.ReplaceAllAsync(pessoas, _ct);

        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().ContainSingle(p => p.NII == "55555");
        var savedPessoa = savedPessoas.Single(p => p.NII == "55555");
        savedPessoa.Id.Should().NotBe(existingId);
        savedPessoa.ExternalId.Should().Be(pessoas[0].ExternalId);
        savedPessoa.DadosPessoais.NomeCompleto.Should().Be(pessoas[0].DadosPessoais.NomeCompleto);
        savedPessoa.DadosPessoais.Sobrenome.Should().Be(pessoas[0].DadosPessoais.Sobrenome);
        savedPessoa.DadosPessoais.DataNascimento.Should().Be(pessoas[0].DadosPessoais.DataNascimento);
        savedPessoa.DadosBiometricos.CorDosOlhos.Should().Be(pessoas[0].DadosBiometricos.CorDosOlhos);
        savedPessoa.DadosBiometricos.AlturaEmCm.Should().Be(pessoas[0].DadosBiometricos.AlturaEmCm);
    }

    [Fact]
    public async Task ShouldClearDatabase_WhenReplacingAllByAnEmptyList()
    {
        // Arrange
        await SeedAsync(new Pessoa { NII = "11111" });

        // Act
        await _repository.ReplaceAllAsync([], _ct);


        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldInsertAllPessoas_WhenNoneExist()
    {
        // Arrange
        var pessoas = new[]
        {
        new Pessoa { NII = "11111", ExternalId = "EXT1" },
        new Pessoa { NII = "22222", ExternalId = "EXT2" }
    };

        // Act
        await _repository.ReplaceAllAsync(pessoas, _ct);

        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("EXT1");
        savedPessoas.Should().ContainSingle(p => p.NII == "22222").Which.ExternalId.Should().Be("EXT2");
    }

    [Fact]
    public async Task ShouldDeleteUnrelatedPessoas_WhenReplacingAll()
    {
        // Arrange
        await SeedAsync(
            new Pessoa { NII = "11111", ExternalId = "ORIGINAL1" },
            new Pessoa { NII = "99999", ExternalId = "UNTOUCHED" }
        );

        var pessoas = new[] { new Pessoa { NII = "11111", ExternalId = "UPDATED1" } };

        // Act
        await _repository.ReplaceAllAsync(pessoas, _ct);

        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().HaveCount(1);
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("UPDATED1");
    }

    [Fact]
    public async Task ShouldDeduplicatedAndReplaceAll_WhenInputContainsDuplicateNIIs()
    {
        // Arrange
        await SeedAsync(new Pessoa { NII = "99999", ExternalId = "EXISTING" });

        var duplicatedPessoas = new[]
        {
            new Pessoa { NII = "11111", ExternalId = "FIRST" },
            new Pessoa { NII = "11111", ExternalId = "LAST" },
            new Pessoa { NII = "99999", ExternalId = "UPDATED" }
        };

        // Act
        await _repository.ReplaceAllAsync(duplicatedPessoas, _ct);

        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("FIRST");
        savedPessoas.Should().ContainSingle(p => p.NII == "99999").Which.ExternalId.Should().Be("UPDATED");

    }

    [Fact]
    public async Task ShouldRollbackClearAll_WhenDatabaseErrorOccursDuringInsert()
    {
        // Arrange
        var initialPessoa = new Pessoa { NII = "123", ExternalId = "ORIGINAL" };
        await SeedAsync(initialPessoa);

        var invalidPessoas = new[]
        {
            new Pessoa { NII = null!, ExternalId = "INVALID" }
        };

        // Act
        Func<Task> act = async () => await _repository.ReplaceAllAsync(invalidPessoas, _ct);

        await act.Should().ThrowAsync<PostgresException>();

        var result = await ReadAllPessoasAsync();
        result.Should().ContainSingle(p => p.NII == "123", "The ClearAllAsync operation should have been rolled back because the subsequent insert failed.");
    }


    private async Task<List<Pessoa>> SeedAsync(params Pessoa[] pessoas)
    {
        await using var seedContext = new AppDbContext(_options);
        await seedContext.Pessoas.AddRangeAsync(pessoas);
        await seedContext.SaveChangesAsync();
        return [.. pessoas];
    }
    private async Task<List<Pessoa>> ReadAllPessoasAsync()
    {
        await using var readContext = new AppDbContext(_options);
        return await readContext.Pessoas.ToListAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
