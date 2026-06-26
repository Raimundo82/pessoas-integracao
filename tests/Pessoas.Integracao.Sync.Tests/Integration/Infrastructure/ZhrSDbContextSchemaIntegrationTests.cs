using DatabaseSchemaReader;

using Npgsql;

using Pessoas.Integracao.Testing;

namespace Pessoas.Integracao.Sync.Tests.Integration.Infrastructure;

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
        var aptidaoOutputTable = reader.Table("ZhrSAptidaoOutputs", _ct);
        var aptidaoTable = reader.Table("ZhrSAptidoes", _ct);

        // Assert
        await Verify(new
        {
            AptidaoOutputPK = aptidaoOutputTable.PrimaryKey.Columns,
            AptidaoOutputNonNullableCols = aptidaoOutputTable.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            AptidaoOutputNullableCols = aptidaoOutputTable.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            AptidaoOutputIdxs = aptidaoOutputTable.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) }),

            AptidaoPK = aptidaoTable.PrimaryKey.Columns,
            AptidaoFK = aptidaoTable.ForeignKeys.Select(fk => new { fk.Name, fk.Columns }),
            AptidaoNonNullableCols = aptidaoTable.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            AptidaoNullableCols = aptidaoTable.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            AptidaoIdxs = aptidaoTable.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) })
        });
    }

    [Fact]
    public async Task ShouldMatchExpectedDatabaseSchema_WhenMappingZhrSPessoaisEntities()
    {
        // Arrange
        var connection = new NpgsqlConnection(_db.ConnectionString);
        using var reader = new DatabaseReader(connection);

        // Act
        var pessoaisOutputTable = reader.Table("ZhrSPessoaisOutputs", _ct);
        var pessoaisTable = reader.Table("ZhrSPessoais", _ct);
        var familiasTable = reader.Table("ZhrSFamilias", _ct);
        var schema = reader.DatabaseSchema;

        // Assert
        await Verify(new
        {
            PessoaisOutputPK = pessoaisOutputTable.PrimaryKey.Columns,
            PessoaisOutputNonNullableCols = pessoaisOutputTable.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            PessoaisOutputNullableCols = pessoaisOutputTable.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            PessoaisOutputIdxs = pessoaisOutputTable.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) }),

            PessoaisPK = pessoaisTable.PrimaryKey.Columns,
            PessoaisFK = pessoaisTable.ForeignKeys.Select(fk => new { fk.Name, fk.Columns }),
            PessoaisNonNullableCols = pessoaisTable.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            PessoaisNullableCols = pessoaisTable.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            PessoaisIdxs = pessoaisTable.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) }),

            FamiliasPK = familiasTable.PrimaryKey.Columns,
            FamiliasFK = familiasTable.ForeignKeys.Select(fk => new { fk.Name, fk.Columns }),
            FamiliasNonNullableCols = familiasTable.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            FamiliasNullableCols = familiasTable.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            FamiliasIdxs = familiasTable.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) })
        });
    }


}
