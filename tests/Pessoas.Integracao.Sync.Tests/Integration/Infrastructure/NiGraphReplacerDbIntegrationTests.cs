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
        var newOutput = AptidaoOutput("22600", "30002697")
            .WithChildren("Aptidao1", "Aptidao2")
            .Build();

        // Act
        await ExecuteAsync([newOutput], newOutput.Aptidao);

        // Assert
        await using var assertContext = NewContext();
        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        var childrenResult = assertContext.Set<ZhrSAptidao>();
        rootResult.Should().HaveCount(1);
        rootResult.Should().ContainSingle(r => r.Ni == "22600" && r.Numsap == "30002697" && r.UpdatedAt == null);
        childrenResult.Should().HaveCount(2);
    }

    [Fact]
    public async Task ShouldReplaceMatchingRootAndPreserveOthers_WhenDbIsPopulated()
    {

        // Arrange
        var (ni1, ni2) = ("22600", "226001");
        var updatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);

        await SeedAsync(
            AptidaoOutput(ni1, "30002697").Build(),
            ChildrenFor(ni1, "Aptidao1", "Aptidao2"));

        await SeedAsync(
            AptidaoOutput(ni2, "30002698").Build(),
            ChildrenFor(ni2, "Aptidao1", "Aptidao2"));

        var replacement = AptidaoOutput(ni1, "30002697", updatedAt)
            .WithChildren("Aptidao3", "Aptidao4")
            .Build();


        // Act
        await ExecuteAsync([replacement], replacement.Aptidao);

        // Assert
        await using var assertContext = NewContext();

        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        rootResult.Should().HaveCount(2);
        rootResult.Should().ContainSingle(r => r.Ni == ni1 && r.Numsap == "30002697" && r.UpdatedAt == updatedAt);
        rootResult.Should().ContainSingle(r => r.Ni == ni2 && r.Numsap == "30002698");

        var childrenResult = assertContext.Set<ZhrSAptidao>();
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
        var (ni1, ni2) = ("22600", "22601");

        var outputs = new[]
        {
            AptidaoOutput(ni1, "30002697").WithChildren("Aptidao1", "Aptidao2").Build(),
            AptidaoOutput(ni2, "30002698").WithChildren("Aptidao3", "Aptidao4").Build(),
        };

        // Act
        await ExecuteAsync(outputs, [.. outputs.SelectMany(o => o.Aptidao)]);

        // Assert
        var assertContext = NewContext();
        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        rootResult.Should().HaveCount(2);
        rootResult.Should().ContainSingle(r => r.Ni == ni1 && r.Numsap == "30002697");
        rootResult.Should().ContainSingle(r => r.Ni == ni2 && r.Numsap == "30002698");

        var childrenResult = assertContext.Set<ZhrSAptidao>();
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

        await SeedAsync(
            AptidaoOutput(ni, "30002697").Build(),
            ChildrenFor(ni, "Aptidao1", "Aptidao2"));

        var replacement = AptidaoOutput(ni, "30002697").WithChildren().Build();

        // Act
        await ExecuteAsync([replacement], replacement.Aptidao);

        // Assert
        await using var assertContext = NewContext();
        var rootResult = assertContext.Set<ZhrSAptidaoOutput>();
        rootResult.Should().HaveCount(1);

        var childrenResult = assertContext.Set<ZhrSAptidao>();
        rootResult.Should().ContainSingle(r => r.Ni == ni && r.Numsap == "30002697" && r.UpdatedAt == null);
        childrenResult.Should().BeEmpty();
    }


    private ZhrSDbContext NewContext() => new(_options);
    private async Task ExecuteAsync(ZhrSAptidaoOutput[] outputs, IEnumerable<ZhrSAptidao> children)
    {
        await using var context = NewContext();
        await new NiGraphReplacer(context).ExecuteAsync(outputs, [[.. children]], _ct);
    }
    private async Task SeedAsync(ZhrSAptidaoOutput root, ZhrSAptidao[] children)
    {
        await using var context = NewContext();
        await context.Set<ZhrSAptidaoOutput>().AddAsync(root, _ct);
        await context.Set<ZhrSAptidao>().AddRangeAsync(children, _ct);
        await context.SaveChangesAsync(_ct);
    }

    private static AptidaoOutputBuilder AptidaoOutput(
        string ni,
        string numsap,
        DateTimeOffset? updatedAt = null
    ) => new(ni, numsap, updatedAt);

    private static ZhrSAptidao[] ChildrenFor(string ni, params string[] areaExames) =>
        [.. areaExames.Select(a => new ZhrSAptidao { Ni = ni, AreaExame = a })];

    private sealed class AptidaoOutputBuilder(string ni, string numsap, DateTimeOffset? updatedAt)
    {
        private ZhrSAptidao[] _children = [];

        public AptidaoOutputBuilder WithChildren(params string[] areaExames)
        {
            _children = ChildrenFor(ni, areaExames);
            return this;
        }

        public ZhrSAptidaoOutput Build() => new()
        {
            Ni = ni,
            Numsap = numsap,
            UpdatedAt = updatedAt,
            Aptidao = _children,
        };
    }
}

