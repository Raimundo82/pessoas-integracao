using System.Globalization;

using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas;

namespace Pessoas.Integracao.Worker.Tests.Unit.Clients;

public sealed class DeltaClientUnitTests : IDisposable
{
    private IOptions<DataSourceSettings> _settings;
    private readonly Mock<ZHR_WS_DELTASChannel> _soapChannelDeltas;
    private readonly Mock<ISoapChannelProvider<ZHR_WS_DELTASChannel>> _soapChannelDeltasProvider;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public DeltaClientUnitTests()
    {
        _settings = Options.Create(new DataSourceSettings { Empresa = "3000" });
        _soapChannelDeltas = new Mock<ZHR_WS_DELTASChannel>();
        _soapChannelDeltasProvider = new Mock<ISoapChannelProvider<ZHR_WS_DELTASChannel>>();
    }

    [Fact]
    public async Task ShouldReturnExpectedDeltasOutput_WhenMockedSoapChannelReturnsSingleValidDelta()
    {
        // Arrange
        var expectedOutput = new[]
        {
            new ZhrWsGetDeltasPernrOut { Id = "000146252", Pernr = "30005978", Ni = "00024014", Bdate = "2020-11-24", Btime = DateTime.Parse("2020-11-24 14:02:38", CultureInfo.InvariantCulture), Infty = "0015", Actio = "MOD", Begda = "2019-11-01", Endda = "2019-11-01" },
        };
        _soapChannelDeltas
            .Setup(c => c.ZhrWsGetDeltasPernrAsync(It.IsAny<ZhrWsGetDeltasPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetDeltasPernrResponse1
            {
                ZhrWsGetDeltasPernrResponse = new ZhrWsGetDeltasPernrResponse
                {
                    Output = expectedOutput
                }
            });

        _soapChannelDeltasProvider.Setup(f => f.CreateChannel()).Returns(_soapChannelDeltas.Object);
        var client = new DeltasClient(_settings, _soapChannelDeltasProvider.Object);

        var startTimestamp = DateTime.ParseExact("2020-11-24 14:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var endTimestamp = DateTime.ParseExact("2020-11-24 15:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var timePeriod = new TimePeriod(startTimestamp, endTimestamp);

        // Act
        var deltas = await client.GetDeltasAsync(timePeriod, _ct);

        // Assert
        deltas.Should().NotBeNull();
        deltas.Should().HaveCount(1);
        deltas.Should().Equal(expectedOutput);
    }

    [Fact]
    public async Task ShouldReturnExpectedDeltasOutput_WhenMockedSoapChannelReturnsMultipleValidDeltas()
    {
        // Arrange
        var expectedOutput = new[]
        {
            new ZhrWsGetDeltasPernrOut { Id = "000146252", Pernr = "30005978", Ni = "00024014", Bdate = "2020-11-24", Btime = DateTime.Parse("2020-11-24 14:02:38", CultureInfo.InvariantCulture), Infty = "0015", Actio = "MOD", Begda = "2019-11-01", Endda = "2019-11-01" },
            new ZhrWsGetDeltasPernrOut { Id = "000146253", Pernr = "30005978", Ni = "00024014", Bdate = "2020-11-24", Btime = DateTime.Parse("2020-11-24 14:02:38", CultureInfo.InvariantCulture), Infty = "0015", Actio = "MOD", Begda = "2019-11-01", Endda = "2019-11-01" },

        };

        _soapChannelDeltas
            .Setup(c => c.ZhrWsGetDeltasPernrAsync(It.IsAny<ZhrWsGetDeltasPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetDeltasPernrResponse1
            {
                ZhrWsGetDeltasPernrResponse = new ZhrWsGetDeltasPernrResponse
                {
                    Output = expectedOutput
                }
            });

        _soapChannelDeltasProvider.Setup(f => f.CreateChannel()).Returns(_soapChannelDeltas.Object);
        var client = new DeltasClient(_settings, _soapChannelDeltasProvider.Object);

        var startTimestamp = DateTime.ParseExact("2020-11-24 14:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var endTimestamp = DateTime.ParseExact("2020-11-24 15:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var timePeriod = new TimePeriod(startTimestamp, endTimestamp);

        // Act
        var deltas = await client.GetDeltasAsync(timePeriod, _ct);

        // Assert
        deltas.Should().NotBeNull();
        deltas.Should().HaveCount(2);
        deltas.Should().Equal(expectedOutput);
    }

    [Fact]
    public async Task ShouldReturnEmptyArray_WhenMockedSoapChannelReturnsNoDeltas()
    {
        // Arrange
        var expectedOutput = Array.Empty<ZhrWsGetDeltasPernrOut>();
        _soapChannelDeltas
            .Setup(c => c.ZhrWsGetDeltasPernrAsync(It.IsAny<ZhrWsGetDeltasPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetDeltasPernrResponse1
            {
                ZhrWsGetDeltasPernrResponse = new ZhrWsGetDeltasPernrResponse
                {
                    Output = expectedOutput
                }
            });

        _soapChannelDeltasProvider.Setup(f => f.CreateChannel()).Returns(_soapChannelDeltas.Object);
        var client = new DeltasClient(_settings, _soapChannelDeltasProvider.Object);

        var startTimestamp = DateTime.ParseExact("2020-11-24 14:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var endTimestamp = DateTime.ParseExact("2020-11-24 15:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var timePeriod = new TimePeriod(startTimestamp, endTimestamp);

        // Act
        var deltas = await client.GetDeltasAsync(timePeriod, _ct);

        // Assert
        deltas.Should().NotBeNull();
        deltas.Should().BeEmpty();
        deltas.Should().Equal(expectedOutput);
    }

    [Fact]
    public async Task ShouldReturnEmptyArray_WhenSoapResponseHasNullOutput()
    {
        // Arrange
        _soapChannelDeltas
            .Setup(c => c.ZhrWsGetDeltasPernrAsync(It.IsAny<ZhrWsGetDeltasPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetDeltasPernrResponse1
            {
                ZhrWsGetDeltasPernrResponse = new ZhrWsGetDeltasPernrResponse
                {
                    Output = null
                }
            });

        _soapChannelDeltasProvider.Setup(f => f.CreateChannel()).Returns(_soapChannelDeltas.Object);

        var client = new DeltasClient(_settings, _soapChannelDeltasProvider.Object);

        var timePeriod = new TimePeriod(
            DateTime.Parse("2020-11-24 14:00:00", CultureInfo.InvariantCulture),
            DateTime.Parse("2020-11-24 15:00:00", CultureInfo.InvariantCulture)
        );

        // Act
        var deltas = await client.GetDeltasAsync(timePeriod, _ct);

        // Assert
        deltas.Should().NotBeNull();
        deltas.Should().BeEmpty();
    }
    [Fact]
    public async Task ShouldThrowException_WhenSoapChannelThrows()
    {
        // Arrange
        _soapChannelDeltas
            .Setup(c => c.ZhrWsGetDeltasPernrAsync(It.IsAny<ZhrWsGetDeltasPernrRequest>()))
            .ThrowsAsync(new InvalidOperationException("SOAP error"));

        _soapChannelDeltasProvider.Setup(f => f.CreateChannel()).Returns(_soapChannelDeltas.Object);

        var client = new DeltasClient(_settings, _soapChannelDeltasProvider.Object);

        var timePeriod = new TimePeriod(
            DateTime.Parse("2020-11-24 14:00:00", CultureInfo.InvariantCulture),
            DateTime.Parse("2020-11-24 15:00:00", CultureInfo.InvariantCulture)
        );

        // Act
        Func<Task> act = () => client.GetDeltasAsync(timePeriod, _ct);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("SOAP error");
    }

    public void Dispose()
    {
        _settings = null!;
        GC.SuppressFinalize(this);
    }
}
