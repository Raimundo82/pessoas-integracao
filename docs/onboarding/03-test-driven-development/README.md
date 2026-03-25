# Module 03 — Test Driven Development

**Format:** Demo → You try it  
**Duration:** ~3 hours  
**Prerequisites:** Module 02 completed (dev container running, `dotnet test` works)

---

## What this module covers

| #   | Topic                                                                  | Format     | Time   |
| --- | ---------------------------------------------------------------------- | ---------- | ------ |
| 1   | [TDD — Red → Green → Refactor](./01-tdd-cycle.md)                      | Demo → Try | 25 min |
| 2   | [Unit Tests vs Integration Tests](./02-unit-vs-integration.md)         | Demo → Try | 30 min |
| 3   | [Test Doubles — Fake, Stub, Spy, Mock](./03-test-doubles.md)           | Demo → Try | 30 min |
| 4   | [Writing Tests — AAA, Naming, FluentAssertions](./04-writing-tests.md) | Demo → Try | 25 min |
| 5   | [TDD Tutorial — Build TimePeriod from scratch](./05-tutorial.md)       | Hands-on   | 50 min |
| –   | Wrap-up & Q&A                                                          | Discussion | 10 min |

---

## The test suite at a glance

```
tests/
└── Pessoas.Integracao.Tests/
    ├── Unit/
    │   ├── Application/            ← Use case unit tests (ImportPessoas, GetAllPessoas)
    │   ├── Domain/                 ← Domain logic unit tests (Roles)
    │   ├── Models/                 ← Value object unit tests (TimePeriod)
    │   ├── Authorization/          ← Auth logic unit tests
    │   └── TestDoubles/            ← Fake, Stub, Spy implementations
    ├── Integration/
    │   ├── Controllers/            ← HTTP-level tests (WebApplicationFactory)
    │   ├── UseCases/               ← Use case integration tests (real Postgres)
    │   └── Infrastructure/         ← Repository integration tests
    └── TestInfrastructure/
        ├── IntegrationTestWebAppFactory.cs   ← Custom WebApplicationFactory
        ├── PostgresTestContainerDb.cs        ← Testcontainers setup
        └── TestAuthHandler.cs               ← Fake auth for tests
```

The key distinction: **Unit** tests use in-memory test doubles and run in milliseconds.
**Integration** tests use a real PostgreSQL container via Testcontainers and test the
full stack end-to-end.
