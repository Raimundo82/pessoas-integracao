using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.SigdnRhPessoasProviderTests;

public sealed class ExternalPersonnelNumberClientUnitTests : IDisposable
{
    private IOptions<DataSourceSettings> _settings;
    private Mock<zhr_wsChannel> _soapChannel;
    private Mock<ISoapChannelProvider<zhr_wsChannel>> _soapChannelFactory;

    public ExternalPersonnelNumberClientUnitTests()
    {
        _settings = Options.Create(new DataSourceSettings { });
        _soapChannel = new Mock<zhr_wsChannel>();
        _soapChannelFactory = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
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

        _soapChannelFactory.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new ExternalPersonnelNumberClient(_settings, _soapChannelFactory.Object);

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

        _soapChannelFactory.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new ExternalPersonnelNumberClient(_settings, _soapChannelFactory.Object);

        // Act
        var result = await client.GetExternalPersonnelNumbersAsync(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedOutput);
    }

    public void Dispose()
    {
        _settings = null!;
        _soapChannel = null!;
        _soapChannelFactory = null!;
        GC.SuppressFinalize(this);
    }
}