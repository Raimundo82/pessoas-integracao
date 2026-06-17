using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Pessoas.Integracao.Analitica.Infrastructure.Data;
using Pessoas.Integracao.Analitica.Infrastructure.Repositories;
using Pessoas.Integracao.Analitica.Models;
using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.AnaliticaRepositories;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class AnaliticaRepositoryIntegrationTests(PostgresTestContainerDb db) : IAsyncLifetime
{
    private readonly DbContextOptions<AnaliticaDbContext> _options = new DbContextOptionsBuilder<AnaliticaDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;


    public ValueTask InitializeAsync() => new(db.ResetDatabaseAsync());

    [Fact]
    public async Task ShouldPreserveAllRows_WhenReplacingMacthingByNiAndInputIsEmpty()
    {
        // Arrange
        var existing = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "0001" },
            new ZhrWsAptidaoAptidao { Ni = "20102", Subty = "0001" }
        };
        await Seed(existing);

        // Act
        await using var actContext = CreateContext();
        var repo = new AnaliticaRepository<ZhrWsAptidaoAptidao>(actContext);
        await repo.ReplaceMatchingByNiAsync([], _ct);

        // Assert
        await using var assertContext = CreateContext();
        var result = await assertContext.Set<ZhrWsAptidaoAptidao>().ToListAsync(_ct);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ShouldReplaceMatchingByNi_WhenNiExists()
    {
        // Arrange
        var existing = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "Old" },
            new ZhrWsAptidaoAptidao { Ni = "20102", Subty = "Keep" }
        };
        await Seed(existing);

        var updates = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "20002", Subty = "New" }
        };

        // Act
        await using var actContext = CreateContext();
        var repo = new AnaliticaRepository<ZhrWsAptidaoAptidao>(actContext);
        await repo.ReplaceMatchingByNiAsync(updates, _ct);

        // Assert
        await using var assertContext = CreateContext();
        var result = await assertContext.Set<ZhrWsAptidaoAptidao>().ToListAsync(_ct);
        result.Should().HaveCount(2);
        result.Should().ContainSingle(e => e.Ni == "20002" && e.Subty == "New");
        result.Should().ContainSingle(e => e.Ni == "20102" && e.Subty == "Keep");
    }

    [Fact]
    public async Task ShouldClearTableAndInsertNew_WhenReplacingAll()
    {
        // Arrange
        var existing = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "1", Subty = "Old" },
        };
        await Seed(existing);

        var newData = new[] { new ZhrWsAptidaoAptidao { Ni = "2", Subty = "New" } };

        // Act
        await using var actContext = CreateContext();
        var repo = new AnaliticaRepository<ZhrWsAptidaoAptidao>(actContext);
        await repo.ReplaceAllAsync(newData, _ct);

        // Assert
        await using var assertContext = CreateContext();
        var result = await assertContext.Set<ZhrWsAptidaoAptidao>().ToListAsync(_ct);
        result.Should().ContainSingle().Which.Ni.Should().Be("2");
    }

    [Fact]
    public async Task ShouldRollback_WhenErrorOccursDuringReplaceMatchingByNiAsync()
    {
        // Arrange
        var existing = new[]
        {
            new ZhrWsAptidaoAptidao { Ni = "999", Subty = "Safe" },
        };
        await Seed(existing);

        var invalidData = new[] { new ZhrWsAptidaoAptidao { Ni = null!, Subty = "Error" } };

        // Act
        await using var actContext = CreateContext();
        var repo = new AnaliticaRepository<ZhrWsAptidaoAptidao>(actContext);
        Func<Task> act = async () => await repo.ReplaceMatchingByNiAsync(invalidData, _ct);

        // Assert
        await act.Should().ThrowAsync<PostgresException>();
        await using var assertContext = CreateContext();
        var result = await assertContext.Set<ZhrWsAptidaoAptidao>().ToListAsync(_ct);
        result.Should().ContainSingle(e => e.Ni == "999");
    }

    [Fact]
    public async Task ShouldPersistData_WhenUsingDifferentModelTypeDuringReplaceAll()
    {
        // Arrange
        var newData = new[] { new ZhrWsAtribOrgAtribOrg { Ni = "100", Unid = "Test" } };

        // Act
        await using var actContext = CreateContext();
        var repo = new AnaliticaRepository<ZhrWsAtribOrgAtribOrg>(actContext);
        await repo.ReplaceAllAsync(newData, _ct);

        // Assert
        await using var assertContext = CreateContext();
        var result = await assertContext.Set<ZhrWsAtribOrgAtribOrg>().ToListAsync(_ct);
        result.Should().ContainSingle(e => e.Ni == "100" && e.Unid == "Test");
    }

    [Fact]
    public async Task ShouldReplaceDataCorrectly_WhenUsingMultipleAnaliticaRepositories()
    {
        // Arrange
        var aptidaoData = new[] { new ZhrWsAptidaoAptidao { Ni = "2", Subty = "New", Numsap = "3002" } };
        var atribOrgData = new[] { new ZhrWsAtribOrgAtribOrg { Ni = "2", DescCarg = "asfd", Posicao = "asdfasdf", Numsap = "3002" } };

        // Act
        await using var actContext = CreateContext();
        var aptidaoRepo = new AnaliticaRepository<ZhrWsAptidaoAptidao>(actContext);
        await aptidaoRepo.ReplaceAllAsync(aptidaoData, _ct);

        var atribOrgRepo = new AnaliticaRepository<ZhrWsAtribOrgAtribOrg>(actContext);
        await atribOrgRepo.ReplaceMatchingByNiAsync(atribOrgData, _ct);


        // Assert
        await using var assertContext = CreateContext();
        var aptidaoResult = await assertContext.Set<ZhrWsAptidaoAptidao>().ToListAsync(_ct);
        var atribOrgResult = await assertContext.Set<ZhrWsAtribOrgAtribOrg>().ToListAsync(_ct);

        aptidaoResult.Should().HaveCount(1);
        aptidaoResult.Should().ContainSingle(e => e.Ni == "2" && e.Subty == "New" && e.Numsap == "3002");

        atribOrgResult.Should().HaveCount(1);
        atribOrgResult.Should().ContainSingle(e => e.Ni == "2" && e.DescCarg == "asfd" && e.Posicao == "asdfasdf" && e.Numsap == "3002");
    }


    private async Task Seed<T>(IEnumerable<T> entities) where T : ZhrWsBaseModel
    {
        await using var context = CreateContext();
        await context.Set<T>().AddRangeAsync(entities, _ct);
        await context.SaveChangesAsync(_ct);
    }

    private AnaliticaDbContext CreateContext() => new(_options);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

}

