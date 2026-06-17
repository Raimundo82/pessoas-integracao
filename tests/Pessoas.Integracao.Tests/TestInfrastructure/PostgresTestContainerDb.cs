using System.Runtime.CompilerServices;

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

    [ModuleInitializer]
    public static void Initialize() =>
         DerivePathInfo((sourceFile, projectDirectory, type, method) =>
            new PathInfo(Path.Combine(projectDirectory, "__snapshots__")));

    public PostgresTestContainerDb()
    {
        _container = new PostgreSqlBuilder("postgres:17")
            .WithDatabase("testedb")
            .WithCleanUp(true)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = "testedb",
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
        if (_resetConnection != null) await _resetConnection.DisposeAsync();
        await _container.DisposeAsync();
    }
}

[CollectionDefinition(nameof(PostgresTestDatabaseCollection))]
public sealed class PostgresTestDatabaseCollection : ICollectionFixture<PostgresTestContainerDb>
{
}
