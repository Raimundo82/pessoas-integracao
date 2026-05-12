using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Core.Domain.ValueObjects;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Repositories;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.Repositories;


[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class ReplaceAllAsyncDbIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PessoaRepository _repository;
    private readonly DbContextOptions<AppDbContext> _options;

    public ReplaceAllAsyncDbIntegrationTests(PostgresTestContainerDb db)
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AppDbContext(_options);
        _repository = new PessoaRepository(_context);
        _context.Database.EnsureCreated();
    }

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

        var colocacoes = new Colocacao[] { AddColocacao("colo-1") };

        var input = new[]
        {
            new Pessoa { NII = "11111", ExternalId = "NEW1", Colocacoes = colocacoes },
            new Pessoa { NII = "22222", ExternalId = "NEW2" }
        };

        // Act
        await _repository.ReplaceAllAsync(input, CancellationToken.None);

        // Assert
        var result = await ReadAllPessoasAsync();
        result.Should().HaveCount(2);

        var p1 = result.Should().ContainSingle(p => p.NII == "11111").Which;
        p1.Id.Should().NotBe(oldId1);
        p1.ExternalId.Should().Be("NEW1");
        p1.Colocacoes.Should().HaveCount(1);
        p1.Colocacoes.Should().BeEquivalentTo(colocacoes, opts => opts.Excluding(c => c.Id));

        var p2 = result.Should().ContainSingle(p => p.NII == "22222").Which;
        p2.Id.Should().NotBe(oldId2);
        p2.ExternalId.Should().Be("NEW2");
        p2.Colocacoes.Should().HaveCount(0);
    }

    private static Colocacao AddColocacao(string unidadeExternaRef)
    {
        return new Colocacao { UnidadeExternaRef = new UnidadeExternaRef(unidadeExternaRef) };
    }

    [Fact]
    public async Task ShouldHandleBothNewAndExisting_WhenReplacingAll()
    {
        // Arrange
        await SeedAsync(new Pessoa { NII = "11111", ExternalId = "OLD1", Colocacoes = [AddColocacao("colo-1")] });

        var colocacoes = new Colocacao[] { AddColocacao("colo-1"), AddColocacao("colo-2") };
        var pessoas = new[]
        {
            new Pessoa { NII = "11111", ExternalId = "UPDATED1",Colocacoes = colocacoes },
            new Pessoa { NII = "22222", ExternalId = "NEW2" },
            new Pessoa { NII = "33333", ExternalId = "NEW3" }
        };

        // Act
        await _repository.ReplaceAllAsync(pessoas, CancellationToken.None);

        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().HaveCount(3);

        var p1 = savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which;
        p1.ExternalId.Should().Be("UPDATED1");
        p1.Colocacoes.Should().BeEquivalentTo(colocacoes, opts => opts.Excluding(c => c.Id));
        p1.Colocacoes.Should().ContainSingle(c => c.UnidadeExternaRef.ExternalReference == "1111");

        var p2 = savedPessoas.Should().ContainSingle(p => p.NII == "22222").Which;
        p2.ExternalId.Should().Be("NEW2");
        p2.Colocacoes.Should().HaveCount(0);

        var p3 = savedPessoas.Should().ContainSingle(p => p.NII == "33333").Which;
        p3.ExternalId.Should().Be("UPDATED1");
        p3.Colocacoes.Should().HaveCount(0);
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
        await _repository.ReplaceAllAsync(pessoas, CancellationToken.None);

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
        await _repository.ReplaceAllAsync([], CancellationToken.None);


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
        await _repository.ReplaceAllAsync(pessoas, CancellationToken.None);

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
        await _repository.ReplaceAllAsync(pessoas, CancellationToken.None);

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
        await _repository.ReplaceAllAsync(duplicatedPessoas, CancellationToken.None);

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
        Func<Task> act = async () => await _repository.ReplaceAllAsync(invalidPessoas, CancellationToken.None);

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
        return await readContext.Pessoas.Include(p => p.Colocacoes).ToListAsync();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
