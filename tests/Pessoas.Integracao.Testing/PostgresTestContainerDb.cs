using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Pessoas.Integracao.Analitica.Infrastructure.Data;
using Pessoas.Integracao.Core.Infrastructure.Data;
using Pessoas.Integracao.Sync.Infrastructure.Data;

using Respawn;

using Testcontainers.PostgreSql;


namespace Pessoas.Integracao.Testing;

public sealed class PostgresTestContainerDb : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private NpgsqlConnection _resetConnection = null!;
    private Respawner _respawner = null!;

    private const string DatabaseName = "testedb";

    public string ConnectionString { get; private set; } = null!;

    public PostgresTestContainerDb()
    {
        _container = new PostgreSqlBuilder("postgres:17")
            .WithDatabase(DatabaseName)
            .WithCleanUp(true)
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = DatabaseName,
            SslMode = SslMode.Disable,

        };
        ConnectionString = builder.ToString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        var analiticaOptions = new DbContextOptionsBuilder<AnaliticaDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        var zhrSOptions = new DbContextOptionsBuilder<ZhrSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        var syncOptions = new DbContextOptionsBuilder<PessoaSyncRefDbContext>()

            .UseNpgsql(ConnectionString)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        await using var analiticaContext = new AnaliticaDbContext(analiticaOptions);
        await analiticaContext.Database.GetInfrastructure().GetRequiredService<IRelationalDatabaseCreator>().CreateTablesAsync();

        await using var zhrSContext = new ZhrSDbContext(zhrSOptions);
        await zhrSContext.Database.GetInfrastructure().GetRequiredService<IRelationalDatabaseCreator>().CreateTablesAsync();

        await using var pessoaSyncRefContext = new PessoaSyncRefDbContext(syncOptions);
        await pessoaSyncRefContext.Database.GetInfrastructure().GetRequiredService<IRelationalDatabaseCreator>().CreateTablesAsync();

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

