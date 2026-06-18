using Pessoas.Integracao.Testing;

namespace Pessoas.Integracao.Analitica.Tests.Integration.Infrastructure; // Use the project's namespace

[CollectionDefinition(nameof(PostgresTestDatabaseCollection))]
public sealed class PostgresTestDatabaseCollection : ICollectionFixture<PostgresTestContainerDb> { }
