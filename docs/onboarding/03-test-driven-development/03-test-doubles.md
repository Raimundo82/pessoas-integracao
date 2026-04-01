# 03 — Test Doubles — Fake, Stub, Spy, Mock

## 📖 Concept

A **test double** is any object that replaces a real dependency in a test.
There are four kinds, and this project uses all of them.

```
tests/Pessoas.Integracao.Tests/Unit/TestDoubles/
├── FakePessoaRepository.cs          ← Fake
├── StubPessoasDataProvider.cs       ← Stub
├── StubPessoasImportKeyProvider.cs  ← Stub
├── SpyUnitOfWork.cs                 ← Spy
├── ThrowingFakePessoasRepository.cs ← Fake (error path)
├── ThrowingPessoasDataProvider.cs   ← Throwing Stub
└── ThrowingUnitOfWork.cs            ← Throwing Spy
```

### Fake

A **Fake** is a working implementation — simpler than the real one, but real logic.
It doesn't need a database; it stores data in memory.

```csharp
// FakePessoaRepository.cs
public sealed class FakePessoaRepository(IReadOnlyList<PessoaImportKey> existingKeys)
    : IPessoaRepository
{
    // Records what was called — useful for assertions
    public IReadOnlyList<Pessoa>? LastUpsertedPessoas { get; private set; }
    public CancellationToken? LastUpsertToken { get; private set; }

    public Task<UpsertPessoasResult> UpsertAllAsync(
        IReadOnlyList<Pessoa> pessoas, CancellationToken ct)
    {
        LastUpsertedPessoas = pessoas;   // record the call
        LastUpsertToken = ct;
        return Task.FromResult(new UpsertPessoasResult(pessoas.Count, 0));
    }
}
```

**Use a Fake when** the real implementation has side effects (writes to a database,
calls an API) and you want a simple in-memory substitute.

### Stub

A **Stub** returns canned responses. It doesn't record calls or have real logic.

```csharp
// StubPessoasDataProvider.cs
public sealed class StubPessoasDataProvider(IReadOnlyList<Pessoa> pessoasToReturn)
    : IPessoasDataProvider
{
    public IReadOnlyList<PessoaImportKey>? LastRequestedKeys { get; private set; }
    public CancellationToken? LastToken { get; private set; }

    public Task<IReadOnlyList<Pessoa>> GetPessoasByImportKeysAsync(
        IReadOnlyList<PessoaImportKey> keys, CancellationToken ct)
    {
        LastRequestedKeys = keys;  // also records — this stub doubles as a spy
        LastToken = ct;
        return Task.FromResult(_pessoasToReturn);  // always returns the same data
    }
}
```

**Use a Stub when** you need a dependency to return specific data for your test,
but you don't care about the details of how it was called.

### Spy

A **Spy** records how it was called, so you can assert on the interactions.

```csharp
// SpyUnitOfWork.cs
public sealed class SpyUnitOfWork : IUnitOfWork
{
    public int CommitCalls { get; private set; }    // records call count
    public CancellationToken? LastToken { get; private set; }

    public Task CommitAsync(CancellationToken ct)
    {
        CommitCalls++;      // increment every time it's called
        LastToken = ct;     // record which token was passed
        return Task.CompletedTask;
    }
}
```

**Usage in a test:**

```csharp
var uowSpy = new SpyUnitOfWork();
// ... run the use case ...
uowSpy.CommitCalls.Should().Be(1);     // was Commit called exactly once?
uowSpy.LastToken.Should().Be(ct);      // was the right token passed?
```

**Use a Spy when** you want to verify that a dependency was called correctly —
not just that the output is right, but that the right methods were invoked.

### Mock (via Moq)

A **Mock** is a spy that you configure with expectations. Moq generates the
implementation at runtime.

```csharp
// From ImportPessoasUnitTests.cs — verifying call order
var sequence = new MockSequence();

_repo.InSequence(sequence)
    .Setup(r => r.GetExistingImportKeysAsync(ct))
    .ReturnsAsync(importKeys);

_dataProvider.InSequence(sequence)
    .Setup(s => s.GetPessoasByImportKeysAsync(importKeys, ct))
    .ReturnsAsync(pessoas);

// ... run ...

_repo.VerifyAll();          // assert that all setups were called
_dataProvider.VerifyAll();
```

**Use a Mock when** you want to verify exact call sequences, or when the real
implementation has complex behaviour to configure inline.

### Summary

| Double | Stores state   | Returns data    | Records calls | Asserts calls |
| ------ | -------------- | --------------- | ------------- | ------------- |
| Fake   | ✅ (in memory) | ✅              | Optional      | ❌            |
| Stub   | ❌             | ✅ (canned)     | Optional      | ❌            |
| Spy    | ❌             | ✅              | ✅            | ✅ (in test)  |
| Mock   | ❌             | ✅ (configured) | ✅            | ✅ (built-in) |

> **Prefer hand-written test doubles over Moq when the double is reused across many
> tests.** The `TestDoubles/` folder exists for this reason. A hand-written Fake is
> easier to read and debug than a Mock with complex setups.

---

## 💻 Try it — trace a test double through a test

1. Open `FakePessoaRepository.cs` in `TestDoubles/`.
   - Which interface does it implement?
   - Which methods record their calls?

2. Open `ImportPessoasUnitTests.cs`.
   - Find `ShouldUpsertEmptyListAndCommit_WhenNoImportKeysExist`.
   - Which test doubles does it use?
   - What does each assertion verify about each double?

3. Find `ShouldNotUpsertOrCommit_WhenDataProviderThrows`.
   - What does `ThrowingPessoasDataProvider` do?
   - What does the test verify about the `SpyUnitOfWork` after the exception?

---

## ✅ Done when

You can explain the difference between a Stub and a Spy, and identify where each
one is used in the project's `TestDoubles/` folder.
