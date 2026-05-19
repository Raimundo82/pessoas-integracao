using Microsoft.EntityFrameworkCore;

using Npgsql;

using Pessoas.Integracao.Core.Infrastructure.Data;

using Testcontainers.PostgreSql;

namespace Pessoas.Integracao.Tests.TestInfrastructure;

public sealed class PostgresTestContainerDb : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString { get; private set; } = null!;

    public PostgresTestContainerDb()
    {
        _container = new PostgreSqlBuilder("postgres:17")
            .WithCleanUp(true)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = "pessoastestdb"
        };
        ConnectionString = builder.ToString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(nameof(PostgresTestDatabaseCollection))]
public sealed class PostgresTestDatabaseCollection : ICollectionFixture<PostgresTestContainerDb>
{
}
