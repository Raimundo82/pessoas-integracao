using System.Globalization;

using FluentAssertions;

using Pessoas.Integracao.Core.Application.Models;

namespace Pessoas.Integracao.Tests.Unit.Models;

public sealed class TimePeriodTests
{
    [Fact]
    public void ShouldThrow_WhenEndIsBeforeStart()
    {
        Action act = () =>
        {
            var _ = new TimePeriod(
            DateTime.Parse("2020-11-25 10:00:00", CultureInfo.InvariantCulture),
            DateTime.Parse("2020-11-24 10:00:00", CultureInfo.InvariantCulture));
        };

        act.Should().Throw<ArgumentException>()
           .WithMessage("*End timestamp cannot be earlier than start timestamp*");
    }

    [Fact]
    public void ShouldAllowEqualStartAndEnd_WhenStartEqualsEnd()
    {
        var timestamp = DateTime.Parse("2020-11-24 10:00:00", CultureInfo.InvariantCulture);

        var timePeriod = new TimePeriod(timestamp, timestamp);

        timePeriod.Start.Should().Be(timestamp);
        timePeriod.End.Should().Be(timestamp);
    }

    [Fact]
    public void ShouldSetStartAndEndCorrectly_WhenRangeIsValid()
    {
        var start = DateTime.Parse("2020-11-24 10:00:00", CultureInfo.InvariantCulture);
        var end = DateTime.Parse("2020-11-24 12:00:00", CultureInfo.InvariantCulture);

        var timePeriod = new TimePeriod(start, end);

        timePeriod.Start.Should().Be(start);
        timePeriod.End.Should().Be(end);
    }

    [Fact]
    public void ShouldReturnExpectedFormattedString_WhenStartAsStringCalledWithValidTimestamp()
    {
        var start = DateTime.Parse("2020-11-24 10:05:30", CultureInfo.InvariantCulture);
        var timePeriod = new TimePeriod(start, start);

        timePeriod.StartAsString().Should().Be("2020-11-24 10:05:30");
    }

    [Fact]
    public void ShouldReturnExpectedFormattedString_WhenEndAsStringCalledWithValidTimestamp()
    {
        var end = DateTime.Parse("2020-11-24 18:45:10", CultureInfo.InvariantCulture);
        var timePeriod = new TimePeriod(end, end);

        timePeriod.EndAsString().Should().Be("2020-11-24 18:45:10");
    }

    [Fact]
    public void ShouldKeepFormatting_WhenAsStringMethodsAreCultureIndependent()
    {
        var start = new DateTime(2020, 11, 24, 10, 0, 0, DateTimeKind.Unspecified);
        var end = new DateTime(2020, 11, 24, 11, 0, 0, DateTimeKind.Unspecified);

        var timePeriod = new TimePeriod(start, end);

        var formattedStart = timePeriod.StartAsString();
        var formattedEnd = timePeriod.EndAsString();

        formattedStart.Should().Be("2020-11-24 10:00:00");
        formattedEnd.Should().Be("2020-11-24 11:00:00");
    }
}
