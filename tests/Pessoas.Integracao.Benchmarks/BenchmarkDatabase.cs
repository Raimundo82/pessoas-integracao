using Microsoft.EntityFrameworkCore;

using Pessoas.Integracao.Core.Infrastructure.Data;

using Testcontainers.PostgreSql;

namespace Pessoas.Integracao.Benchmarks;

public sealed class BenchmarkDatabase : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString { get; private set; } = null!;

    public BenchmarkDatabase()
    {
        _container = new PostgreSqlBuilder("postgres:17")
            .WithCleanUp(true)
            .Build();
    }

    public async Task StartAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
