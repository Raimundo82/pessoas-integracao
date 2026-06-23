using Pessoas.Integracao.Testing;

namespace Pessoas.Integracao.Sync.Tests;

[CollectionDefinition("PostgresTestDatabaseCollection")]
public sealed class PostgresTestDatabaseCollection
    : ICollectionFixture<PostgresTestContainerDb>
{
}
