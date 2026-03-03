using System.Globalization;

using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.DTOs;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas;

namespace Pessoas.Integracao.Worker.Tests.Integration.Providers;

public sealed class SigdnRhClientsIntegrationTests : IDisposable
{

    private IOptions<DataSourceSettings> _settings;
    private readonly Mock<ZHR_WS_DELTASChannel> _soapChannelDeltas;
    private readonly Mock<ISoapChannelProvider<ZHR_WS_DELTASChannel>> _soapChannelDeltasFactory;
    public SigdnRhClientsIntegrationTests()
    {
        _settings = Options.Create(new DataSourceSettings { Empresa = "3000" });
        _soapChannelDeltas = new Mock<ZHR_WS_DELTASChannel>();
        _soapChannelDeltasFactory = new Mock<ISoapChannelProvider<ZHR_WS_DELTASChannel>>();
    }

    [Fact]
    public async Task GetDeltasAsync_ReturnsExpectedDeltasOutput_WhenMockedSoapChannelReturnsValidDeltas_GivenStartAndEndTimestamp()
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

        _soapChannelDeltasFactory.Setup(f => f.CreateChannel(_settings.Value.DeltasUrl)).Returns(_soapChannelDeltas.Object);
        var client = new DeltasClient(_settings, _soapChannelDeltasFactory.Object);

        var startTimestamp = DateTime.ParseExact("2020-11-24 14:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var endTimestamp = DateTime.ParseExact("2020-11-24 15:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var timePeriod = new TimePeriodDto(startTimestamp, endTimestamp);

        // Act
        var deltas = await client.GetDeltasAsync(timePeriod, default);

        // Assert
        deltas.Should().NotBeNull();
        deltas.Should().HaveCount(1);
        deltas.Should().BeEquivalentTo(
        [
            new ZhrWsGetDeltasPernrOut { Id = "000146252", Pernr = "30005978", Ni = "00024014", Bdate = "2020-11-24", Btime = DateTime.Parse("2020-11-24 14:02:38", CultureInfo.InvariantCulture), Infty = "0015", Actio = "MOD", Begda = "2019-11-01", Endda = "2019-11-01" },
        ], options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetDeltasAsync_ReturnsEmptyArray_WhenSoapResponseHasNullOutput()
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

        _soapChannelDeltasFactory.Setup(f => f.CreateChannel(_settings.Value.DeltasUrl))
            .Returns(_soapChannelDeltas.Object);

        var client = new DeltasClient(_settings, _soapChannelDeltasFactory.Object);

        var timePeriod = new TimePeriodDto(
            DateTime.Parse("2020-11-24 14:00:00", CultureInfo.InvariantCulture),
            DateTime.Parse("2020-11-24 15:00:00", CultureInfo.InvariantCulture)
        );

        // Act
        var deltas = await client.GetDeltasAsync(timePeriod, default);

        // Assert
        deltas.Should().NotBeNull();
        deltas.Should().BeEmpty();
    }
    [Fact]
    public async Task GetDeltasAsync_ThrowsException_WhenSoapChannelThrows()
    {
        // Arrange
        _soapChannelDeltas
            .Setup(c => c.ZhrWsGetDeltasPernrAsync(It.IsAny<ZhrWsGetDeltasPernrRequest>()))
            .ThrowsAsync(new InvalidOperationException("SOAP error"));

        _soapChannelDeltasFactory.Setup(f => f.CreateChannel(_settings.Value.DeltasUrl))
            .Returns(_soapChannelDeltas.Object);

        var client = new DeltasClient(_settings, _soapChannelDeltasFactory.Object);

        var timePeriod = new TimePeriodDto(
            DateTime.Parse("2020-11-24 14:00:00", CultureInfo.InvariantCulture),
            DateTime.Parse("2020-11-24 15:00:00", CultureInfo.InvariantCulture)
        );

        // Act
        Func<Task> act = () => client.GetDeltasAsync(timePeriod, default);

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