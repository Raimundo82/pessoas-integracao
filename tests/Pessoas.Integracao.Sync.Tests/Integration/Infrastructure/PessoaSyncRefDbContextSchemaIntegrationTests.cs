using DatabaseSchemaReader;

using Npgsql;

using Pessoas.Integracao.Testing;

namespace Pessoas.Integracao.Sync.Tests.Integration.Infrastructure;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class PessoaSyncRefDbContextSchemaIntegrationTests(PostgresTestContainerDb db) : IAsyncLifetime
{
    private readonly PostgresTestContainerDb _db = db;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync() => new(_db.ResetDatabaseAsync());

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task ShouldMatchExpectedSchema_WhenVerifyingPessoaSyncRefs()
    {
        // Arrange
        var connection = new NpgsqlConnection(_db.ConnectionString);
        using var reader = new DatabaseReader(connection);

        // Act
        var table = reader.Table("PessoaSyncRefs", TestContext.Current.CancellationToken);

        // Assert
        await Verify(new
        {
            TableName = table.Name,
            PrimaryKey = table.PrimaryKey.Columns,
            ForeignKey = table.ForeignKeys.Select(fk => new { fk.Name, fk.Columns }),
            NonNullableColumns = table.Columns.Where(c => !c.Nullable).Select(c => c.Name),
            NullableColumns = table.Columns.Where(c => c.Nullable).Select(c => c.Name),
            Indexes = table.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) })
        });
    }
}
