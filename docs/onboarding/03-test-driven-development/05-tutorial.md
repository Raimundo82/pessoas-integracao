# 05 — TDD Tutorial — Build TimePeriod from Scratch

## 🎯 Goal

Build `TimePeriod` using strict TDD. Write each test first, see it fail, then write
the minimum code to make it pass. You will end up with the same class that already
exists in the project — but you'll have built it test-first.

## Setup

Create your working files:

```
tests/Pessoas.Integracao.Tests/Unit/Models/TimePeriodTddTests.cs
src/Pessoas.Integracao.Core/Application/Models/TimePeriodTdd.cs
```

> Use `TimePeriodTdd` as the class name to avoid conflicts with the existing
> `TimePeriod`. Delete the existing tests file if you want to use the exact name.

Start with an empty class:

```csharp
// TimePeriodTdd.cs
namespace Pessoas.Integracao.Core.Application.Models;

public sealed class TimePeriodTdd
{
    // Empty — let the tests drive the implementation
}
```

And an empty test class:

```csharp
// TimePeriodTddTests.cs
using System.Globalization;
using FluentAssertions;
using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Tests.Unit.Models;

public sealed class TimePeriodTddTests
{
    // Add tests here one at a time
}
```

Run `dotnet test --filter "TimePeriodTddTests"` after each step.

---

## Step 1 — Start and End are stored

**Write this test first:**

```csharp
[Fact]
public void ShouldSetStartAndEndCorrectly_WhenRangeIsValid()
{
    var start = DateTime.Parse("2020-11-24 10:00:00", CultureInfo.InvariantCulture);
    var end   = DateTime.Parse("2020-11-24 12:00:00", CultureInfo.InvariantCulture);

    var timePeriod = new TimePeriodTdd(start, end);

    timePeriod.Start.Should().Be(start);
    timePeriod.End.Should().Be(end);
}
```

Run → **RED** (class has no constructor or properties).

Now make it pass with the minimum code:

```csharp
public sealed class TimePeriodTdd
{
    public DateTime Start { get; init; }
    public DateTime End   { get; init; }

    public TimePeriodTdd(DateTime start, DateTime end)
    {
        Start = start;
        End   = end;
    }
}
```

Run → **GREEN** ✅

---

## Step 2 — Equal start and end is allowed

**Write this test:**

```csharp
[Fact]
public void ShouldAllowEqualStartAndEnd_WhenStartEqualsEnd()
{
    var timestamp = DateTime.Parse("2020-11-24 10:00:00", CultureInfo.InvariantCulture);

    var timePeriod = new TimePeriodTdd(timestamp, timestamp);

    timePeriod.Start.Should().Be(timestamp);
    timePeriod.End.Should().Be(timestamp);
}
```

Run → **GREEN** immediately ✅ (already handled by Step 1).

> When a test passes without any code change, it documents an edge case that is
> already covered. That's fine — it's still valuable as a specification.

---

## Step 3 — End before start throws ArgumentException

**Write this test:**

```csharp
[Fact]
public void ShouldThrow_WhenEndIsBeforeStart()
{
    Action act = () => new TimePeriodTdd(
        DateTime.Parse("2020-11-25 10:00:00", CultureInfo.InvariantCulture),
        DateTime.Parse("2020-11-24 10:00:00", CultureInfo.InvariantCulture));

    act.Should().Throw<ArgumentException>()
       .WithMessage("*End timestamp cannot be earlier than start timestamp*");
}
```

Run → **RED** (no validation yet).

Make it pass:

```csharp
public TimePeriodTdd(DateTime start, DateTime end)
{
    if (end < start)
        throw new ArgumentException(
            "End timestamp cannot be earlier than start timestamp.");

    Start = start;
    End   = end;
}
```

Run → **GREEN** ✅

---

## Step 4 — StartAsString formats correctly

**Write this test:**

```csharp
[Fact]
public void ShouldReturnExpectedFormattedString_WhenStartAsStringCalledWithValidTimestamp()
{
    var start = DateTime.Parse("2020-11-24 10:05:30", CultureInfo.InvariantCulture);
    var timePeriod = new TimePeriodTdd(start, start);

    timePeriod.StartAsString().Should().Be("2020-11-24 10:05:30");
}
```

Run → **RED** (method doesn't exist).

Make it pass:

```csharp
public string StartAsString() => Start.ToString("yyyy-MM-dd HH:mm:ss");
```

Run → **GREEN** ✅

---

## Step 5 — EndAsString formats correctly

**Write this test:**

```csharp
[Fact]
public void ShouldReturnExpectedFormattedString_WhenEndAsStringCalledWithValidTimestamp()
{
    var end = DateTime.Parse("2020-11-24 18:45:10", CultureInfo.InvariantCulture);
    var timePeriod = new TimePeriodTdd(end, end);

    timePeriod.EndAsString().Should().Be("2020-11-24 18:45:10");
}
```

Run → **RED**.

Make it pass:

```csharp
public string EndAsString() => End.ToString("yyyy-MM-dd HH:mm:ss");
```

Run → **GREEN** ✅

---

## Step 6 — Format is culture-independent

**Write this test:**

```csharp
[Fact]
public void ShouldKeepFormatting_WhenAsStringMethodsAreCultureIndependent()
{
    var start = new DateTime(2020, 11, 24, 10, 0, 0, DateTimeKind.Unspecified);
    var end   = new DateTime(2020, 11, 24, 11, 0, 0, DateTimeKind.Unspecified);

    var timePeriod = new TimePeriodTdd(start, end);

    timePeriod.StartAsString().Should().Be("2020-11-24 10:00:00");
    timePeriod.EndAsString().Should().Be("2020-11-24 11:00:00");
}
```

Run → check: does it pass? If it does, great. If it doesn't, investigate why
`ToString("yyyy-MM-dd HH:mm:ss")` might produce a different result in some cultures.

> **This is the kind of subtle bug TDD surfaces.** If you had written the code
> without this test, a culture-sensitive machine (different locale) might have
> broken this silently.

---

## Bonus — add [Theory] for the exception test

Replace your single exception test with a `[Theory]` covering multiple invalid inputs:

```csharp
[Theory]
[InlineData("2020-11-25 10:00:00", "2020-11-24 10:00:00")]  // end 1 day before
[InlineData("2020-11-24 11:00:00", "2020-11-24 10:00:00")]  // end 1 hour before
[InlineData("2020-11-24 10:00:01", "2020-11-24 10:00:00")]  // end 1 second before
public void ShouldThrow_WhenEndIsBeforeStart_MultipleScenarios(
    string startStr, string endStr)
{
    Action act = () => new TimePeriodTdd(
        DateTime.Parse(startStr, CultureInfo.InvariantCulture),
        DateTime.Parse(endStr,   CultureInfo.InvariantCulture));

    act.Should().Throw<ArgumentException>();
}
```

---

## Final check

```bash
dotnet test --filter "TimePeriodTddTests"
```

All 6+ tests should be green. Your `TimePeriodTdd` class should now be functionally
identical to `TimePeriod` — built entirely test-first.

Compare your implementation with the original in
`src/Pessoas.Integracao.Core/Application/Models/TimePeriod.cs`.

---

## ✅ Done when

- All tests pass: `dotnet test --filter "TimePeriodTddTests"`
- You made each test go red before writing the code to make it green
- Your commit history shows the Red → Green pattern
  (at least one commit per test added)
