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

        await using var actContext = new ZhrSDbContext(_options);
        var uut = new NiGraphReplacer(actContext);

        // Act
        await uut.ExecuteAsync([aptidaoOutput], [aptidaoOutput.Aptidao], _ct);

        // Assert
        var assertContext = new ZhrSDbContext(_options);
        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        var childrenResult = assertContext.Set<ZhrSAptidao>();
        rootResult.Should().HaveCount(1);
        rootResult.Should().ContainSingle(r => r.Ni == ni && r.Numsap == "30002697" && r.UpdatedAt == null);
        childrenResult.Should().HaveCount(2);
    }

    [Fact]
    public async Task ShouldReplaceRootAndChildren_WhenDbIsPopulated()
    {

        // Arrange
        var ni1 = "22600";
        var ni2 = "226001";
        var currentDateTime = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var existingAptidaoOutput1 = new ZhrSAptidaoOutput
        {
            Ni = ni1,
            Numsap = "30002697",
        };

        var existingChildren1 = new ZhrSAptidao[] { new() { Ni = ni1, AreaExame = "Aptidao1" }, new() { Ni = ni1, AreaExame = "Aptidao2" } };

        await SeedAsync(existingAptidaoOutput1, existingChildren1);
        var existingAptidaoOutput2 = new ZhrSAptidaoOutput
        {
            Ni = ni2,
            Numsap = "30002698",
        };
        var existingChildren2 = new ZhrSAptidao[] { new() { Ni = ni2, AreaExame = "Aptidao1" }, new() { Ni = ni2, AreaExame = "Aptidao2" } };
        await SeedAsync(existingAptidaoOutput2, existingChildren2);

        var newAptidaoOutput = new ZhrSAptidaoOutput
        {
            Ni = ni1,
            Numsap = "30002697",
            UpdatedAt = currentDateTime,
            Aptidao = [new() { Ni = ni1, AreaExame = "Aptidao3" }, new() { Ni = ni1, AreaExame = "Aptidao4" }]
        };

        await using var actContext = new ZhrSDbContext(_options);
        var uut = new NiGraphReplacer(actContext);

        // Act
        await uut.ExecuteAsync([newAptidaoOutput], [newAptidaoOutput.Aptidao], _ct);

        // Assert
        await using var assertContext = new ZhrSDbContext(_options);
        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        var childrenResult = assertContext.Set<ZhrSAptidao>();
        rootResult.Should().HaveCount(2);
        rootResult.Should().ContainSingle(r => r.Ni == ni1 && r.Numsap == "30002697" && r.UpdatedAt == currentDateTime);
        rootResult.Should().ContainSingle(r => r.Ni == ni2 && r.Numsap == "30002698");
        childrenResult.Should().HaveCount(4);
        childrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.AreaExame == "Aptidao3");
        childrenResult.Should().ContainSingle(c => c.Ni == ni1 && c.AreaExame == "Aptidao4");
        childrenResult.Should().ContainSingle(c => c.Ni == ni2 && c.AreaExame == "Aptidao1");
        childrenResult.Should().ContainSingle(c => c.Ni == ni2 && c.AreaExame == "Aptidao2");
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


        await using var actContext = new ZhrSDbContext(_options);
        var uut = new NiGraphReplacer(actContext);

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

    [Fact]
    public async Task ShouldDeleteChildren_WhenRootIsReplacedWithEmptyChildrenData()
    {

        // Arrange
        var ni = "22600";
        var existingAptidaoOutput1 = new ZhrSAptidaoOutput
        {
            Ni = ni,
            Numsap = "30002697",
        };
        var existingChildren1 = new ZhrSAptidao[] { new() { Ni = ni, AreaExame = "Aptidao1" }, new() { Ni = ni, AreaExame = "Aptidao2" } };
        await SeedAsync(existingAptidaoOutput1, existingChildren1);

        var newAptidaoOutput = new ZhrSAptidaoOutput
        {
            Ni = ni,
            Numsap = "30002697",
            Aptidao = []
        };

        await using var actContext = new ZhrSDbContext(_options);
        var uut = new NiGraphReplacer(actContext);

        // Act
        await uut.ExecuteAsync([newAptidaoOutput], [newAptidaoOutput.Aptidao], _ct);

        // Assert
        await using var assertContext = new ZhrSDbContext(_options);
        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        var childrenResult = assertContext.Set<ZhrSAptidao>();
        rootResult.Should().HaveCount(1);
        rootResult.Should().ContainSingle(r => r.Ni == ni && r.Numsap == "30002697" && r.UpdatedAt == null);
        childrenResult.Should().BeEmpty();
    }


    private async Task SeedAsync(ZhrSAptidaoOutput root, ZhrSAptidao[] children)
    {
        await using var context = new ZhrSDbContext(_options);
        await context.Set<ZhrSAptidaoOutput>().AddAsync(root, _ct);
        await context.Set<ZhrSAptidao>().AddRangeAsync(children, _ct);
        await context.SaveChangesAsync(_ct);
    }
}
