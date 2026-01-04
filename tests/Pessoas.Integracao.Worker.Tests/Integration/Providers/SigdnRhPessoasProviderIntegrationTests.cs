using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.Integration.Providers;

public class SigdnRhPessoasProviderIntegrationTests : IDisposable
{

    private IOptions<DataSourceSettings> _settings;
    private Mock<zhr_wsChannel> _soapChannel;
    private Mock<ISoapChannelProvider<zhr_wsChannel>> _soapChannelFactory;
    public SigdnRhPessoasProviderIntegrationTests()
    {
        _settings = Options.Create(new DataSourceSettings { Empresa = "3000" });
        _soapChannel = new Mock<zhr_wsChannel>();
        _soapChannelFactory = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
    }


    [Fact]
    public async Task GetPessoasAsync_UsesMockedSoapChannel_ReturnsExpectedPessoas()
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

        _soapChannelFactory.Setup(f => f.CreateChannel(_settings.Value.OutputUrl)).Returns(_soapChannel.Object);
        var client = new ExternalPersonnelNumberClient(_settings, _soapChannelFactory.Object);
        var provider = new SigdnRhPessoasProvider(client);

        // Act
        var pessoas = await provider.GetPessoasAsync(default);

        // Assert
        pessoas.Should().NotBeNull();
        pessoas.Should().HaveCount(2);
        pessoas.Should().BeEquivalentTo(
        [
            new Pessoa { NII = "22600", ExternalId = "30002697" },
            new Pessoa { NII = "22700", ExternalId = "30002797" }
        ], options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetPessoasAsync_ReturnsEmptyCollection_WhenSoapChannelReturnsEmptyOutput()
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

        _soapChannelFactory.Setup(f => f.CreateChannel(_settings.Value.OutputUrl)).Returns(_soapChannel.Object);
        var client = new ExternalPersonnelNumberClient(_settings, _soapChannelFactory.Object);
        var provider = new SigdnRhPessoasProvider(client);

        // Act
        var pessoas = await provider.GetPessoasAsync(default);

        // Assert
        pessoas.Should().NotBeNull();
        pessoas.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPessoasAsync_ThrowsException_WhenSoapChannelThrows()
    {
        // Arrange
        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ThrowsAsync(new InvalidOperationException("SOAP error"));

        _soapChannelFactory.Setup(f => f.CreateChannel(_settings.Value.OutputUrl)).Returns(_soapChannel.Object);
        var client = new ExternalPersonnelNumberClient(_settings, _soapChannelFactory.Object);
        var provider = new SigdnRhPessoasProvider(client);

        // Act
        Func<Task> act = async () => await provider.GetPessoasAsync(default);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*SOAP error*");
    }

    [Fact]
    public async Task GetPessoasAsync_PassesCorrectRequestToSoapChannel()
    {
        // Arrange
        ZhrWsGetPernrRequest? receivedRequest = null;
        _soapChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .Callback<ZhrWsGetPernrRequest>(req => receivedRequest = req)
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = []
                }
            });

        _soapChannelFactory.Setup(f => f.CreateChannel(_settings.Value.OutputUrl)).Returns(_soapChannel.Object);
        var client = new ExternalPersonnelNumberClient(_settings, _soapChannelFactory.Object);
        var provider = new SigdnRhPessoasProvider(client);

        // Act
        await provider.GetPessoasAsync(default);

        // Assert
        receivedRequest.Should().NotBeNull();
        receivedRequest!.ZhrWsGetPernr.Input.Should().NotBeNull();
        receivedRequest.ZhrWsGetPernr.Input.Should().ContainSingle();
        receivedRequest.ZhrWsGetPernr.Input[0].Empresa.Should().Be("3000");
        receivedRequest.ZhrWsGetPernr.Input[0].Dtreferencia.Should().NotBeNullOrEmpty();
    }

    public void Dispose()
    {
        _settings = null!;
        _soapChannel = null!;
        _soapChannelFactory = null!;
        GC.SuppressFinalize(this);
    }
}