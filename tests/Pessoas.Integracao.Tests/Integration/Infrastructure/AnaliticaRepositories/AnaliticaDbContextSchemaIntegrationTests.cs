using DatabaseSchemaReader;

using Npgsql;

using Pessoas.Integracao.Tests.TestInfrastructure;

namespace Pessoas.Integracao.Tests.Integration.Infrastructure.AnaliticaRepositories;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class AnaliticaDbContextSchemaIntegrationTests(PostgresTestContainerDb db) : IAsyncLifetime
{
    private readonly PostgresTestContainerDb _db = db;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ShouldEnforceSchemaConventions_WhenTableIsZhrWsDerived()
    {
        // Arrange
        var connection = new NpgsqlConnection(_db.ConnectionString);
        using var reader = new DatabaseReader(connection);

        // Act
        var table = reader.Table("ZhrWsAptidao_Aptidao", TestContext.Current.CancellationToken);

        // Assert
        await Verify(new
        {
            table.Name,
            PrimaryKey = table.PrimaryKey.Columns,
            Columns = table.Columns.Select(c => new { c.Name, c.Nullable, c.IsAutoNumber }),
            Indexes = table.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) })
        });
    }
}
