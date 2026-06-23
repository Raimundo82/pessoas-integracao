using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Infrastructure.Data;
using Pessoas.Integracao.Sync.Infrastructure.Data.Persistance;
using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;
using Pessoas.Integracao.Testing;

namespace Pessoas.Integracao.Sync.Tests.Integration.Infrastructure;


[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class NiGraphReplacerDbIntegrationTests(PostgresTestContainerDb db) : IAsyncLifetime
{
    private readonly DbContextOptions<ZhrSDbContext> _options = new DbContextOptionsBuilder<ZhrSDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    private readonly PostgresTestContainerDb _db = db;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    [Fact]
    public async Task ShouldInsertRootAndChildren_WhenDbIsEmpty()
    {

        // Arrange
        var ni = "22600";
        var aptidaoOutput = new ZhrSAptidaoOutput
        {
            Ni = ni,
            Numsap = "30002697",
            Aptidao = [new() { Ni = ni, AreaExame = "Aptidao1" }, new() { Ni = ni, AreaExame = "Aptidao2" }]
        };

        var uut = new NiGraphReplacer(new ZhrSDbContext(_options));

        // Act
        await uut.ExecuteAsync([aptidaoOutput], [aptidaoOutput.Aptidao], _ct);

        // Assert
        var assertContext = new ZhrSDbContext(_options);
        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        var childrenResult = assertContext.Set<ZhrSAptidao>();
        rootResult.Should().HaveCount(1);
        childrenResult.Should().HaveCount(2);

    }


    [Fact]
    public async Task ShouldInsertMultipleRootsAndChildren_WhenDbIsEmpty()
    {

        // Arrange
        var ni1 = "22600";
        var ni2 = "22601";
        var aptidoes = new[]
        {
            new ZhrSAptidaoOutput
            {
                Ni = ni1,
                Numsap = "30002697",
                Aptidao = [new() { Ni = ni1, AreaExame = "Aptidao1" }, new() { Ni = ni1, AreaExame = "Aptidao2" }]
            },
            new ZhrSAptidaoOutput
            {
                Ni = ni2,
                Numsap = "30002698",
                Aptidao = [new() { Ni = ni2, AreaExame = "Aptidao3" }, new() { Ni = ni2, AreaExame = "Aptidao4" }]
            }
        };
        var flatChildren = aptidoes.SelectMany(a => a.Aptidao).ToArray();


        var uut = new NiGraphReplacer(new ZhrSDbContext(_options));

        // Act
        await uut.ExecuteAsync(aptidoes, [flatChildren], _ct);

        // Assert
        var assertContext = new ZhrSDbContext(_options);
        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        var childrenResult = assertContext.Set<ZhrSAptidao>();
        rootResult.Should().HaveCount(2);
        rootResult.Should().ContainSingle(r => r.Ni == ni1 && r.Numsap == "30002697");
        rootResult.Should().ContainSingle(r => r.Ni == ni2 && r.Numsap == "30002698");
        childrenResult.Should().HaveCount(4);
        childrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.AreaExame == "Aptidao1");
        childrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.AreaExame == "Aptidao2");
        childrenResult.Should().ContainSingle(c => c.Ni == ni2 && c.AreaExame == "Aptidao3");
        childrenResult.Should().ContainSingle(c => c.Ni == ni2 && c.AreaExame == "Aptidao4");

    }
}
