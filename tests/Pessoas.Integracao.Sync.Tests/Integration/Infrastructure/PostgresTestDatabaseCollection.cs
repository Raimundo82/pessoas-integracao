using Pessoas.Integracao.Testing;

namespace Pessoas.Integracao.Sync.Tests.Integration.Infrastructure;

[CollectionDefinition(nameof(PostgresTestDatabaseCollection))]
public sealed class PostgresTestDatabaseCollection : ICollectionFixture<PostgresTestContainerDb> { }
