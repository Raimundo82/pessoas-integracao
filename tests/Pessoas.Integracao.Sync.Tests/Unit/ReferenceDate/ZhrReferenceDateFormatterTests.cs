using FluentAssertions;

using Microsoft.Extensions.Options;

using Microsoft.Extensions.Time.Testing;

using Pessoas.Integracao.Sync.Infrastructure.Configuration;

using Pessoas.Integracao.Sync.Infrastructure.Services.ReferenceDate;

namespace Pessoas.Integracao.Sync.Tests.Unit.ReferenceDate;

public class ZhrReferenceDateFormatterTests
{
    private readonly IOptions<ZhrWsSettings> _settings = Options.Create(new ZhrWsSettings { DateFormat = "yyyy-MM-dd" });
    [Fact]
    public void ShouldReturnFormattedDate_WhenDateIsPast()
    {
        // Arrange
        var timeProviderMock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var date = new DateOnly(2023, 1, 1);
        var uut = GetUut(timeProviderMock);

        // Act
        var result = uut.Format(date);

        // Assert
        result.Should().Be("2023-01-01");

    }


    [Fact]
    public void ShouldReturnFormattedDate_WhenDateIsToday()
    {
        // Arrange
        var timeProviderMock = new FakeTimeProvider(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var date = new DateOnly(2023, 1, 1);
        var uut = GetUut(timeProviderMock);

        // Act
        var result = uut.Format(date);

        // Assert
        result.Should().Be("2023-01-01");
    }

    [Fact]
    public void ShouldReturnTodayDate_WhenDateIsFuture()
    {
        // Arrange
        var timeProviderMock = new FakeTimeProvider(new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var date = new DateOnly(2024, 1, 1);
        var uut = GetUut(timeProviderMock);

        // Act
        var result = uut.Format(date);

        // Assert
        result.Should().Be("2023-01-01");
    }

    [Fact]
    public void ShouldReturnIsoFormat_WhenDateIsValid()
    {
        // Arrange
        var timeProviderMock = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var date = new DateOnly(2023, 12, 25);
        var uut = GetUut(timeProviderMock);

        // Act
        var result = uut.Format(date);

        // Assert
        result.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}$");
    }

    private ZhrReferenceDateFormatter GetUut(FakeTimeProvider timeProviderMock)
    {
        return new ZhrReferenceDateFormatter(_settings, timeProviderMock);
    }

}
