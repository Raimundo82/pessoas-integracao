using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Sync.Infrastructure.Data;
using Pessoas.Integracao.Sync.Infrastructure.Models.Dados;
using Pessoas.Integracao.Testing;

namespace Pessoas.Integracao.Sync.Tests.Integration.Infrastructure;

public abstract class TableReplacerTestsBase(PostgresTestContainerDb db) : IAsyncLifetime
{
    protected readonly DbContextOptions<ZhrSDbContext> _options = new DbContextOptionsBuilder<ZhrSDbContext>()
            .UseNpgsql(db.ConnectionString)
            .Options;

    protected readonly CancellationToken _ct = TestContext.Current.CancellationToken;
    protected readonly PostgresTestContainerDb _db = db;

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    protected ZhrSDbContext NewContext() => new(_options);

    protected async Task SeedAsync<TOutput>(TOutput root, params ZhrSBaseModel[][] childrenSets)
        where TOutput : ZhrSBaseModelOutput, IOutputModel, new()
    {
        await using var context = NewContext();
        await context.Set<TOutput>().AddAsync(root, _ct);
        foreach (var children in childrenSets)
            context.AddRange(children);
        await context.SaveChangesAsync(_ct);
    }

    protected static ZhrSAptidaoOutput AptidaoOutput(string ni, string numsap, DateTimeOffset? updatedAt = null) =>
        new() { Ni = ni, Numsap = numsap, UpdatedAt = updatedAt };

    protected static ZhrSAptidao AptidaoChild(string ni, string areaExame) =>
        new() { Ni = ni, AreaExame = areaExame };
}
