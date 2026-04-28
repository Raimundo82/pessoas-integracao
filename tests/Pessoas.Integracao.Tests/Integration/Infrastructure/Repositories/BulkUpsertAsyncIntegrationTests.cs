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
public sealed class BulkUpsertAsyncIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PessoaRepository _repository;
    private readonly DbContextOptions<AppDbContext> _options;

    public BulkUpsertAsyncIntegrationTests(PostgresTestContainerDb db)
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

        _context = new AppDbContext(_options);
        _repository = new PessoaRepository(_context);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ShouldUpdateAllExistingPessoas_WhenUpserting()
    {
        // Arrange
        await SeedAsync(
            new Pessoa { NII = "11111", ExternalId = "OLD1" },
            new Pessoa { NII = "22222", ExternalId = "OLD2" }
        );

        var pessoas = new[]
        {
            new Pessoa { NII = "11111", ExternalId = "NEW1" },
            new Pessoa { NII = "22222", ExternalId = "NEW2" }
        };


        // Act
        await _repository.BulkUpsertAsync(pessoas, CancellationToken.None);

        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("NEW1");
        savedPessoas.Should().ContainSingle(p => p.NII == "22222").Which.ExternalId.Should().Be("NEW2");
    }

    [Fact]
    public async Task ShouldHandleBothNewAndExisting_WhenUpserting()
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
        await _repository.BulkUpsertAsync(pessoas, CancellationToken.None);

        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().HaveCount(3);
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("UPDATED1");
        savedPessoas.Should().ContainSingle(p => p.NII == "22222").Which.ExternalId.Should().Be("NEW2");
        savedPessoas.Should().ContainSingle(p => p.NII == "33333").Which.ExternalId.Should().Be("NEW3");
    }

    [Fact]
    public async Task ShouldUpdateAllPropertiesAndPreserveId_WhenUpsertingExistingPessoa()
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
        await SeedAsync(originalPessoa);
        var originalId = originalPessoa.Id;

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
        await _repository.BulkUpsertAsync(pessoas, CancellationToken.None);

        // Assert


        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().ContainSingle(p => p.NII == "55555");
        var savedPessoa = savedPessoas.Single(p => p.NII == "55555");
        savedPessoa.Id.Should().Be(originalId);
        savedPessoa.ExternalId.Should().Be(pessoas[0].ExternalId);
        savedPessoa.DadosPessoais.NomeCompleto.Should().Be(pessoas[0].DadosPessoais.NomeCompleto);
        savedPessoa.DadosPessoais.Sobrenome.Should().Be(pessoas[0].DadosPessoais.Sobrenome);
        savedPessoa.DadosPessoais.DataNascimento.Should().Be(pessoas[0].DadosPessoais.DataNascimento);
        savedPessoa.DadosBiometricos.CorDosOlhos.Should().Be(pessoas[0].DadosBiometricos.CorDosOlhos);
        savedPessoa.DadosBiometricos.AlturaEmCm.Should().Be(pessoas[0].DadosBiometricos.AlturaEmCm);
    }

    [Fact]
    public async Task ShouldPreserveExistingData_WhenUpsertingEmptyList()
    {
        // Arrange
        await SeedAsync(new Pessoa { NII = "11111" });

        // Act
        await _repository.BulkUpsertAsync([], CancellationToken.None);


        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().ContainSingle();
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
        await _repository.BulkUpsertAsync(pessoas, CancellationToken.None);

        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("EXT1");
        savedPessoas.Should().ContainSingle(p => p.NII == "22222").Which.ExternalId.Should().Be("EXT2");
    }

    [Fact]
    public async Task ShouldNotAffectUnrelatedPessoas_WhenUpsertingSubset()
    {
        // Arrange
        await SeedAsync(
            new Pessoa { NII = "11111", ExternalId = "ORIGINAL1" },
            new Pessoa { NII = "99999", ExternalId = "UNTOUCHED" }
        );

        var pessoas = new[] { new Pessoa { NII = "11111", ExternalId = "UPDATED1" } };

        // Act
        await _repository.BulkUpsertAsync(pessoas, CancellationToken.None);

        // Assert
        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().HaveCount(2);
        savedPessoas.Should().ContainSingle(p => p.NII == "11111").Which.ExternalId.Should().Be("UPDATED1");
        savedPessoas.Should().ContainSingle(p => p.NII == "99999").Which.ExternalId.Should().Be("UNTOUCHED");
    }

    [Fact]
    public async Task ShouldThrowAndRollbackEverything_WhenInputContainsDuplicateNIIs()
    {
        // Arrange
        await SeedAsync(new Pessoa { NII = "99999", ExternalId = "EXISTING" });

        var pessoas = new[]
        {
        new Pessoa { NII = "11111", ExternalId = "FIRST" },
        new Pessoa { NII = "11111", ExternalId = "LAST" },
        new Pessoa { NII = "88888", ExternalId = "VALID_NEW" }
    };

        // Act
        Func<Task> act = () => _repository.BulkUpsertAsync(pessoas, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<PostgresException>()
            .WithMessage("*ON CONFLICT DO UPDATE command cannot affect row a second time*");

        var savedPessoas = await ReadAllPessoasAsync();
        savedPessoas.Should().ContainSingle()
            .Which.NII.Should().Be("99999");
    }

    private async Task SeedAsync(params Pessoa[] pessoas)
    {
        await using var seedContext = new AppDbContext(_options);
        await seedContext.Pessoas.AddRangeAsync(pessoas);
        await seedContext.SaveChangesAsync();
    }
    private async Task<List<Pessoa>> ReadAllPessoasAsync()
    {
        await using var readContext = new AppDbContext(_options);
        return await readContext.Pessoas.ToListAsync();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
