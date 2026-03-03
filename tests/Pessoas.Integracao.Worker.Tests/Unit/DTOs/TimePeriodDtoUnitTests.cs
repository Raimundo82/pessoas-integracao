using System.Globalization;

using FluentAssertions;

namespace Pessoas.Integracao.Worker.Tests.Unit
{
    public sealed class TimePeriodDtoTests
    {
        [Fact]
        public void Constructor_Throws_WhenEndIsBeforeStart()
        {
            Action act = () => new TimePeriodDto(
                DateTime.Parse("2020-11-25 10:00:00", CultureInfo.InvariantCulture),
                DateTime.Parse("2020-11-24 10:00:00", CultureInfo.InvariantCulture)
            );

            act.Should().Throw<ArgumentException>()
               .WithMessage("*End timestamp cannot be earlier than start timestamp*");
        }

        [Fact]
        public void Constructor_AllowsEqualStartAndEnd()
        {
            var timestamp = DateTime.Parse("2020-11-24 10:00:00", CultureInfo.InvariantCulture);

            var dto = new TimePeriodDto(timestamp, timestamp);

            dto.Start.Should().Be(timestamp);
            dto.End.Should().Be(timestamp);
        }

        [Fact]
        public void Constructor_SetsStartAndEndCorrectly_WhenValidRangeIsProvided()
        {
            var start = DateTime.Parse("2020-11-24 10:00:00", CultureInfo.InvariantCulture);
            var end = DateTime.Parse("2020-11-24 12:00:00", CultureInfo.InvariantCulture);

            var dto = new TimePeriodDto(start, end);

            dto.Start.Should().Be(start);
            dto.End.Should().Be(end);
        }

        [Fact]
        public void StartAsSapString_ReturnsExpectedFormattedString()
        {
            var start = DateTime.Parse("2020-11-24 10:05:30", CultureInfo.InvariantCulture);
            var dto = new TimePeriodDto(start, start);

            dto.StartAsSapString().Should().Be("2020-11-24 10:05:30");
        }

        [Fact]
        public void EndAsSapString_ReturnsExpectedFormattedString()
        {
            var end = DateTime.Parse("2020-11-24 18:45:10", CultureInfo.InvariantCulture);
            var dto = new TimePeriodDto(end, end);

            dto.EndAsSapString().Should().Be("2020-11-24 18:45:10");
        }

        [Fact]
        public void Formatting_IsCultureIndependent()
        {
            var start = new DateTime(2020, 11, 24, 10, 0, 0, DateTimeKind.Unspecified);
            var end = new DateTime(2020, 11, 24, 11, 0, 0, DateTimeKind.Unspecified);

            var dto = new TimePeriodDto(start, end);

            var formattedStart = dto.StartAsSapString();
            var formattedEnd = dto.EndAsSapString();

            formattedStart.Should().Be("2020-11-24 10:00:00");
            formattedEnd.Should().Be("2020-11-24 11:00:00");
        }
    }
}