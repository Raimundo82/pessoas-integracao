# 01 — TDD — Red → Green → Refactor

## 📖 Concept

**Test Driven Development** means writing a failing test before you write production
code. Every piece of behaviour you add starts with a test that currently fails.

### The cycle

```
🔴 RED        Write a failing test that defines the behaviour you want
    │         Run it — it must fail. If it passes already, the test is wrong.
    ▼
🟢 GREEN      Write the minimum code to make the test pass
    │         No more, no less. Resist the urge to write "clean" code yet.
    ▼
🔵 REFACTOR   Clean up — extract, rename, simplify
    │         Tests keep you safe: if they still pass, you haven't broken anything.
    └──────── back to RED for the next behaviour
```

### Why test first?

| If you write tests after           | If you write tests first                 |
| ---------------------------------- | ---------------------------------------- |
| Tests confirm the code that exists | Tests define the behaviour you need      |
| Easy to miss edge cases            | Edge cases become explicit tests         |
| Refactoring feels risky            | Refactoring is safe — tests are your net |
| Tests mirror the implementation    | Tests mirror the requirements            |

### A concrete example from this project

**Requirement:** `TimePeriod` should reject construction when `end` is before `start`.

**Step 1 — RED:**

```csharp
[Fact]
public void ShouldThrow_WhenEndIsBeforeStart()
{
    Action act = () => new TimePeriod(
        start: DateTime.Parse("2020-11-25 10:00:00"),
        end:   DateTime.Parse("2020-11-24 10:00:00")); // end < start

    act.Should().Throw<ArgumentException>()
       .WithMessage("*End timestamp cannot be earlier than start timestamp*");
}
```

Run `dotnet test` → **red** ✅ (`TimePeriod` doesn't exist yet)

**Step 2 — GREEN:**

```csharp
public sealed class TimePeriod
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    public TimePeriod(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException(
                "End timestamp cannot be earlier than start timestamp.");
        Start = start;
        End = end;
    }
}
```

Run `dotnet test` → **green** ✅

**Step 3 — REFACTOR:**
Nothing to improve here yet. Add the next test (equal start/end should be allowed),
go back to red, make it green, repeat.

---

## 💻 Try it — read an existing TDD test

1. Open `tests/Pessoas.Integracao.Tests/Unit/Models/TimePeriodUnitTests.cs`
2. Read all 6 tests. For each one, ask:
   - What behaviour does it define?
   - What would need to be true for this test to have been written first?
3. Open `src/Pessoas.Integracao.Core/Application/Models/TimePeriod.cs`
4. Can you see how the implementation matches the test requirements exactly?

---

## ✅ Done when

You can explain the Red → Green → Refactor cycle in your own words, and identify
at least one test in the project that clearly drove an implementation decision.
