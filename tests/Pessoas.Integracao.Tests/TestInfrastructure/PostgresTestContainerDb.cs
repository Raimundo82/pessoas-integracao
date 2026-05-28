using Microsoft.EntityFrameworkCore;

using Npgsql;

using Pessoas.Integracao.Core.Infrastructure.Data;

using Respawn;

using Testcontainers.PostgreSql;

namespace Pessoas.Integracao.Tests.TestInfrastructure;

public sealed class PostgresTestContainerDb : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private NpgsqlConnection _resetConnection = null!;
    private Respawner _respawner = null!;

    public string ConnectionString { get; private set; } = null!;

    public PostgresTestContainerDb()
    {
        _container = new PostgreSqlBuilder("postgres:17")
            .WithDatabase("pessoastestdb")
            .WithCleanUp(true)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = "pessoastestdb",
            SslMode = SslMode.Disable,

        };
        ConnectionString = builder.ToString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        _resetConnection = new NpgsqlConnection(ConnectionString);
        await _resetConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_resetConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
        });
    }

    public Task ResetDatabaseAsync() => _respawner.ResetAsync(_resetConnection);


    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(nameof(PostgresTestDatabaseCollection))]
public sealed class PostgresTestDatabaseCollection : ICollectionFixture<PostgresTestContainerDb>
{
}
