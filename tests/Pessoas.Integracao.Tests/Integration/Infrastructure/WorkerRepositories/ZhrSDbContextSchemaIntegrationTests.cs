using DatabaseSchemaReader;

using Npgsql;

using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.WorkerRepositories;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class ZhrSDbContextSchemaIntegrationTests(PostgresTestContainerDb db) : IAsyncLifetime
{
    private readonly PostgresTestContainerDb _db = db;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ShouldMatchExpectedDatabaseSchema_WhenMappingZhrSAptidaoEntities()
    {
        // Arrange
        var connection = new NpgsqlConnection(_db.ConnectionString);
        using var reader = new DatabaseReader(connection);

        // Act
        var rootTable = reader.Table("ZhrSAptidaoOutputs", _ct);
        var leafTable = reader.Table("ZhrSAptidoes", _ct);

        // Assert
        await Verify(new
        {
            RootPK = rootTable.PrimaryKey.Columns,
            RootCols = rootTable.Columns.Select(c => new { c.Name, c.Nullable, c.IsAutoNumber }),
            RootIdxs = rootTable.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) }),

            LeafPK = leafTable.PrimaryKey.Columns,
            LeafCols = leafTable.Columns.Select(c => new { c.Name, c.Nullable, c.IsAutoNumber }),
            LeafIdxs = leafTable.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) })
        });
    }
}
