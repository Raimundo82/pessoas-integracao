# 02 — Unit Tests vs Integration Tests

## 📖 Concept

Both types of tests live in this project and serve different purposes.

### Unit tests

A unit test tests **one class in isolation**. All its dependencies are replaced with
test doubles (see Module 03-03). No database, no HTTP, no file system.

```
Unit test
    │
    ▼
Class under test ──── fake/stub/spy ────▶ (no real dependencies)
```

**Characteristics:**

- Run in milliseconds
- Deterministic — same input always gives same output
- Tell you exactly which class is broken when they fail
- Can run without a container, CI runner, or network

**In this project:**

```
tests/Pessoas.Integracao.Tests/Unit/
├── Application/ImportPessoasUnitTests.cs    ← tests ImportPessoas use case in isolation
├── Application/GetAllPessoasUnitTests.cs    ← tests GetAllPessoas use case in isolation
├── Domain/RolesTests.cs                     ← tests Roles.FromExternalProvider
└── Models/TimePeriodUnitTests.cs            ← tests TimePeriod value object
```

**Example — testing the use case without a real database:**

```csharp
[Fact]
public async Task ExecuteAsync_WhenNoPessoasExist_ReturnsEmptyCollection()
{
    // Arrange — use a mock repository, no real database
    var ct = new CancellationTokenSource().Token;
    _repo.Setup(r => r.GetAllAsync(ct)).ReturnsAsync([]);
    var uut = new GetAllPessoas(_repo.Object);

    // Act
    var result = await uut.ExecuteAsync(ct);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeEmpty();
}
```

### Integration tests

An integration test tests **multiple components together** — often including
the database, HTTP stack, or external services.

```
Integration test
    │
    ▼
HTTP endpoint → Controller → Use Case → Repository → Real PostgreSQL
                                                      (via Testcontainers)
```

**Characteristics:**

- Slower — spin up a real PostgreSQL container, run migrations
- Test the full flow end-to-end
- Catch issues that unit tests can't: SQL queries, EF Core mappings, HTTP routing
- Use `Testcontainers.PostgreSql` for an isolated, throwaway database

**In this project:**

```
tests/Pessoas.Integracao.Tests/Integration/
├── Controllers/PessoasImportControllerTests.cs   ← HTTP POST /api/pessoas/import
├── Controllers/PessoasControllerTests.cs         ← HTTP GET /api/pessoas
├── UseCases/ImportPessoasIntegrationTests.cs     ← full use case + real Postgres
└── Infrastructure/PessoaRepositoryIntegrationTests.cs  ← repository + real Postgres
```

**Example — testing with a real PostgreSQL container:**

```csharp
[Collection(nameof(PostgresTestDatabaseCollection))]
public sealed class ImportPessoasIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;

    public ImportPessoasIntegrationTests(PostgresTestContainerDb db)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(db.ConnectionString) // real Postgres, real SQL
            .Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ShouldPersistAllPessoas_WhenDatabaseIsEmpty()
    {
        // ... runs against a real PostgreSQL container
    }
}
```

### When to write which

| Situation                                           | Write...         |
| --------------------------------------------------- | ---------------- |
| Testing business rules (e.g. duplicate key merging) | Unit test        |
| Testing a value object's constraints                | Unit test        |
| Testing that SQL queries return the right rows      | Integration test |
| Testing the HTTP response code and body             | Integration test |
| Testing auth/authorization                          | Integration test |
| Testing the full import flow end-to-end             | Integration test |

**Rule of thumb:** Start with unit tests. Add integration tests for the boundaries
where your code meets the outside world (database, HTTP, external APIs).

### The test pyramid

```
        ▲
       / \
      / E2E \         ← few, slow, expensive
     /───────\
    /Integration\     ← some, medium speed
   /─────────────\
  /  Unit Tests   \   ← many, fast, cheap
 /─────────────────\
```

Most of your tests should be unit tests. Integration tests are fewer but cover
the seams between components.

---

## 💻 Try it — compare a unit test and its integration twin

1. Open `tests/.../Unit/Application/ImportPessoasUnitTests.cs`
   - Find `ShouldUpsertSourcePessoasAndCommit_WhenRepositoryHasNoKey`
   - Note: it uses `FakePessoaRepository` — no database
   - How long does it take? Run: `dotnet test --filter "ImportPessoasUnitTests"`

2. Open `tests/.../Integration/UseCases/ImportPessoasIntegrationTests.cs`
   - Find `ShouldPersistAllPessoas_WhenDatabaseIsEmpty`
   - Note: it uses a real PostgreSQL container
   - How long does it take? Run: `dotnet test --filter "ImportPessoasIntegrationTests"`

3. Discuss: what does each test verify that the other can't?

---

## ✅ Done when

You can explain why both unit and integration tests are necessary, and give one
example of something only a unit test catches and one example only an integration
test can verify.
