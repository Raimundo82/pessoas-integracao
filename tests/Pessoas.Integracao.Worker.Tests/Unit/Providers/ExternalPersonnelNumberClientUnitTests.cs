using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.Unit.Providers;

public sealed class ExternalPersonnelNumberClientUnitTests : IDisposable
{
    private IOptions<DataSourceSettings> _settings;
    private Mock<zhr_wsChannel> _soapChannel;
    private Mock<ISoapChannelProvider<zhr_wsChannel>> _soapChannelProvider;

    public ExternalPersonnelNumberClientUnitTests()
    {
        _settings = Options.Create(new DataSourceSettings { });
        _soapChannel = new Mock<zhr_wsChannel>();
        _soapChannelProvider = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
    }

    [Fact]
    public async Task GetExternalPersonnelNumbersAsync_ReturnsExpectedResult_FromSoapChannel()
    {
        // Arrange

        var expectedOutput = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "22700", Numsap = "30002797", Empresa = "3000" }
        };

        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = expectedOutput }]
                }
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new ExternalPersonnelNumberClient(_settings, _soapChannelProvider.Object);

        // Act
        var result = await client.GetExternalPersonnelNumbersAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedOutput);
    }

    [Fact]
    public async Task GetExternalPersonnelNumbersAsync_ReturnsEmptyCollection_WhenSoapChannelReturnsEmpty()
    {
        // Arrange

        var expectedOutput = Array.Empty<ZhrSListapessoal>();

        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = expectedOutput }]
                }
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new ExternalPersonnelNumberClient(_settings, _soapChannelProvider.Object);

        // Act
        var result = await client.GetExternalPersonnelNumbersAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedOutput);
    }

    public void Dispose()
    {
        _settings = null!;
        _soapChannel = null!;
        _soapChannelProvider = null!;
        GC.SuppressFinalize(this);
    }
}