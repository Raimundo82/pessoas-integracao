using Pessoas.Integracao.Testing;

namespace Pessoas.Integracao.Sync.Tests.Integration.Infrastructure;

[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class ZhrFetcherByNiDbIntegrationTests(PostgresTestContainerDb db) : TableReplacerTestsBase(db), IAsyncLifetime
{


    [Fact]
    public async Task ShouldReturnAllMatchingRows_WhenNisExistInTable()
    {
    }

    [Fact]
    public async Task ShouldReturnEmpty_WhenNoNisMatchInTable()
    {
    }

    [Fact]
    public async Task ShouldReturnRowsForMultipleNis_WhenAllExistInTable()
    {
    }

    [Fact]
    public async Task ShouldReturnOnlyMatchingNis_WhenPartialNisExistInTable()
    {
    }

}
