# 04 — Writing Tests — AAA, Naming, FluentAssertions

## 📖 Concept

### Arrange, Act, Assert (AAA)

Every test in this project follows the same structure:

```csharp
[Fact]
public async Task ShouldUpsertSourcePessoasAndCommit_WhenRepositoryHasNoKey()
{
    // Arrange (Given) — set up state and dependencies
    var ct = new CancellationTokenSource().Token;
    var fakeRepo = new FakePessoaRepository([]);
    var keysProviderStub = new StubPessoasImportKeyProvider([new("22601", "30001001")]);
    var dataProviderStub = new StubPessoasDataProvider([new Pessoa { NII = "22601" }]);
    var uowSpy = new SpyUnitOfWork();
    var uut = new ImportPessoas(fakeRepo, dataProviderStub, keysProviderStub, uowSpy);

    // Act (When) — call the thing under test
    var result = await uut.ExecuteAsync(ct);

    // Assert (Then) — verify outcome
    result.TotalProcessed.Should().Be(1);
    uowSpy.CommitCalls.Should().Be(1);
}
```

The `uut` variable name stands for **unit under test** — a convention used
throughout this codebase to make the tested class obvious at a glance.

### Test naming

Tests in this project follow the pattern:

```
ShouldExpectedBehaviour_WhenCondition
```

| ✅ Good                                                  | ❌ Bad               |
| -------------------------------------------------------- | -------------------- |
| `ShouldThrow_WhenEndIsBeforeStart`                       | `Test1`              |
| `ShouldUpsertEmptyListAndCommit_WhenNoImportKeysExist`   | `TestImport`         |
| `ExecuteAsync_WhenNoPessoasExist_ReturnsEmptyCollection` | `GetAllReturnsEmpty` |
| `ShouldReturnForbidden_WhenImportAsViewer`               | `AuthTest`           |

The name is a specification — someone reading it should understand the scenario
without opening the test body.

### FluentAssertions

We use **FluentAssertions** for readable assertions:

```csharp
// Instead of Assert.Equal(1, result.TotalProcessed)
result.TotalProcessed.Should().Be(1);

// Instead of Assert.NotNull(result)
result.Should().NotBeNull();

// Collection assertions
savedPessoas.Should().HaveCount(2);
savedPessoas.Select(p => p.NII).Should().BeEquivalentTo("22600", "21200");

// Exception assertions
act.Should().Throw<ArgumentException>()
   .WithMessage("*End timestamp cannot be earlier*");

// Async exceptions
await act.Should().ThrowAsync<Exception>().WithMessage("commit error");

// Single item assertions
savedPessoas.Should().ContainSingle(p => p.NII == "22600")
    .Which.ExternalId.Should().Be("30002697");
```

### `[Fact]` vs `[Theory]`

Use `[Fact]` for a single scenario.  
Use `[Theory]` + `[InlineData]` for the same behaviour with multiple inputs:

```csharp
// RolesTests.cs
[Theory]
[InlineData("admin",  Roles.Admin)]
[InlineData("viewer", Roles.Viewer)]
public void FromExternalProvider_WithValidRole_ReturnsCorrectRole(
    string externalRole, string expectedRole)
{
    var result = Roles.FromExternalProvider(externalRole);
    result.Should().Be(expectedRole);
}

[Theory]
[InlineData("ADMIN")]
[InlineData("Admin")]
[InlineData("aDmIn")]
public void FromExternalProvider_WithAdminCaseInsensitive_ReturnsAdmin(string role)
{
    Roles.FromExternalProvider(role).Should().Be(Roles.Admin);
}
```

`[Theory]` avoids copy-pasting the same test body with different values.

### `IDisposable` for cleanup

Test classes that allocate resources implement `IDisposable`:

```csharp
public sealed class GetAllPessoasUnitTests : IDisposable
{
    private Mock<IPessoaRepository> _repo;

    public GetAllPessoasUnitTests()
    {
        _repo = new Mock<IPessoaRepository>(); // runs before each test
    }

    public void Dispose()
    {
        _repo = null!;                         // runs after each test
        GC.SuppressFinalize(this);
    }
}
```

Integration tests use `Dispose` to clean up the database:

```csharp
public void Dispose()
{
    _context.Database.EnsureDeleted(); // wipe the test database
    _context.Dispose();
    GC.SuppressFinalize(this);
}
```

---

## 💻 Try it — read tests as specifications

1. Open `RolesTests.cs`. Without reading `Roles.cs`, write down what you expect
   `Roles.FromExternalProvider` to do, based only on the tests.

2. Now open `Roles.cs`. Does the implementation match your expectations exactly?

3. Open `TimePeriodUnitTests.cs`. How many behaviours does `TimePeriod` have,
   based on the tests alone? List them.

---

## ✅ Done when

You can write a test method that follows AAA, uses a descriptive name, and uses
FluentAssertions for its assertions.
