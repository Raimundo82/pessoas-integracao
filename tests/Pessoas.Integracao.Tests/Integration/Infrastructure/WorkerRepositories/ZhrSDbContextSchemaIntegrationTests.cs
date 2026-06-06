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
        var aptidaoTable = reader.Table("ZhrSAptidoes", _ct);

        // Assert
        await Verify(new
        {
            RootPK = rootTable.PrimaryKey.Columns,
            RootNonNullableCols = rootTable.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            RootNullableCols = rootTable.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            RootIdxs = rootTable.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) }),

            AptidaoPK = aptidaoTable.PrimaryKey.Columns,
            AptidaoNonNullableCols = aptidaoTable.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            AptidaoNullableCols = aptidaoTable.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            AptidaoIdxs = aptidaoTable.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) })
        });
    }

    [Fact]
    public async Task ShouldMatchExpectedDatabaseSchema_WhenMappingZhrSAtribOrgEntities()
    {
        // Arrange
        var connection = new NpgsqlConnection(_db.ConnectionString);
        using var reader = new DatabaseReader(connection);

        // Act
        var rootTable = reader.Table("ZhrSAtribOrgOutputs", _ct);
        var atribOrg = reader.Table("ZhrSAtribOrgs", _ct);
        var classifProf = reader.Table("ZhrSClassifProfs", _ct);
        var dataMedidas = reader.Table("ZhrSDataMedidas", _ct);
        var infoProm = reader.Table("ZhrSInfoProms", _ct);
        var monitPrazos = reader.Table("ZhrSMonitPrazos", _ct);
        var om = reader.Table("ZhrSOms", _ct);


        // Assert
        await Verify(new
        {
            RootPK = rootTable.PrimaryKey.Columns,
            RootNonNullableCols = rootTable.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            RootNullableCols = rootTable.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            RootIdxs = rootTable.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) }),

            AtribOrgPK = atribOrg.PrimaryKey.Columns,
            AtribOrgNonNullableCols = atribOrg.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            AtribOrgNullableCols = atribOrg.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            AtribOrgIdxs = atribOrg.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) }),

            ClassifProfPK = classifProf.PrimaryKey.Columns,
            ClassifProfNonNullableCols = classifProf.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            ClassifProfNullableCols = classifProf.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            ClassifProfIdxs = classifProf.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) }),

            DataMedidasPK = dataMedidas.PrimaryKey.Columns,
            DataMedidasNonNullableCols = dataMedidas.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            DataMedidasNullableCols = dataMedidas.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            DataMedidasIdxs = dataMedidas.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) }),

            infoPromPK = infoProm.PrimaryKey.Columns,
            infoPromNonNullableCols = infoProm.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            infoPromNullableCols = infoProm.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            infoPromIdxs = infoProm.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) }),

            monitPrazosPK = monitPrazos.PrimaryKey.Columns,
            monitPrazosNonNullableCols = monitPrazos.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            monitPrazosNullableCols = monitPrazos.Columns.Where(c => c.Nullable).Select(c => new { c.Name }),
            monitPrazosIdxs = monitPrazos.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) }),

            omPK = om.PrimaryKey.Columns,
            omNonNullableCols = om.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            omNullableCols = om.Columns.Where(c => !c.Nullable).Select(c => new { c.Name }),
            omIdxs = om.Indexes.Select(i => new { i.Name, Columns = i.Columns.Select(c => c.Name) })
        });
    }
}
