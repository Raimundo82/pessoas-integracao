using Microsoft.EntityFrameworkCore;

using Npgsql;

using Pessoas.Integracao.Core.Infrastructure.Data;

using Testcontainers.PostgreSql;

namespace Pessoas.Integracao.Core.Tests.Infrastructure.PessoaRepositoryTests;

public class PostgresTestContainerDb : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public string ConnectionString { get; private set; } = null!;

    public PostgresTestContainerDb()
    {
        _container = new PostgreSqlBuilder()
            .WithAutoRemove(true)
            .Build();
    }

    public async Task InitializeAsync()
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

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(nameof(PostgresTestDatabaseCollection))]
public sealed class PostgresTestDatabaseCollection : ICollectionFixture<PostgresTestContainerDb>
{
}