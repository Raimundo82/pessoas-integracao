using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Core.Domain.Entities;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Deltas;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.Integration.Providers;

public sealed class SigdnRhPessoasProviderIntegrationTests : IDisposable
{

    private IOptions<DataSourceSettings> _settings;
    private Mock<zhr_wsChannel> _outputChannel;
    private readonly Mock<ZHR_WS_DELTASChannel> _deltasChannel;
    private Mock<ISoapChannelProvider<zhr_wsChannel>> _outputChannelProvider;
    private readonly Mock<ISoapChannelProvider<ZHR_WS_DELTASChannel>> _deltasChannelProvider;
    public SigdnRhPessoasProviderIntegrationTests()
    {
        _settings = Options.Create(new DataSourceSettings { Empresa = "3000" });
        _outputChannel = new Mock<zhr_wsChannel>();
        _deltasChannel = new Mock<ZHR_WS_DELTASChannel>();
        _outputChannelProvider = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
        _deltasChannelProvider = new Mock<ISoapChannelProvider<ZHR_WS_DELTASChannel>>();
    }


    [Fact]
    public async Task GetPessoasByImportKeysAsync_UsesMockedSoapChannel_ReturnsExpectedPessoas()
    {
        // Arrange
        var importKeys = new[]
        {
            new PessoaImportKey("22600", "30002697"),
            new PessoaImportKey("22700", "30002797")
        };

        var expectedOutput = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "22700", Numsap = "30002797", Empresa = "3000" }
        };
        _outputChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = expectedOutput }]
                }
            });

        _outputChannelProvider.Setup(f => f.CreateChannel()).Returns(_outputChannel.Object);
        var client = new ExternalPersonnelNumberClient(_settings, _outputChannelProvider.Object);
        var provider = new SigdnRhPessoasProvider(client);

        // Act
        var pessoas = await provider.GetPessoasByImportKeysAsync(importKeys, default);

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
    public async Task GetPessoasByImportKeysAsync_ReturnsEmptyCollection_WhenSoapChannelReturnsEmptyOutput()
    {
        // Arrange
        var expectedOutput = Array.Empty<ZhrSListapessoal>();
        _outputChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = expectedOutput }]
                }
            });

        _outputChannelProvider.Setup(f => f.CreateChannel()).Returns(_outputChannel.Object);
        var client = new ExternalPersonnelNumberClient(_settings, _outputChannelProvider.Object);
        var provider = new SigdnRhPessoasProvider(client);

        // Act
        var pessoas = await provider.GetPessoasByImportKeysAsync([], default);

        // Assert
        pessoas.Should().NotBeNull();
        pessoas.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSourceImportKeysAsync_UsesMockedSoapChannel_ReturnsExpectedPessoas()
    {
        // Arrange
        var expectedOutput = new[]
        {
            new ZhrSListapessoal { Ni = "22600", Numsap = "30002697", Empresa = "3000" },
            new ZhrSListapessoal { Ni = "22700", Numsap = "30002797", Empresa = "3000" }
        };
        _outputChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = expectedOutput }]
                }
            });

        _outputChannelProvider.Setup(f => f.CreateChannel()).Returns(_outputChannel.Object);
        var client = new ExternalPersonnelNumberClient(_settings, _outputChannelProvider.Object);
        var provider = new SigdnRhPessoasProvider(client);

        // Act
        var importKeys = await provider.GetSourceImportKeysAsync(default);

        // Assert
        importKeys.Should().NotBeNull();
        importKeys.Should().HaveCount(2);
        importKeys.Should().BeEquivalentTo(
        [
            new PessoaImportKey("22600","30002697"),
            new PessoaImportKey("22700","30002797")
        ], options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetSourceImportKeysAsync_ReturnsEmptyCollection_WhenSoapChannelReturnsEmptyOutput()
    {
        // Arrange
        var expectedOutput = Array.Empty<ZhrSListapessoal>();
        _outputChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = [new ZhrSGetListapessoal { Pessoal = expectedOutput }]
                }
            });

        _outputChannelProvider.Setup(f => f.CreateChannel()).Returns(_outputChannel.Object);
        var client = new ExternalPersonnelNumberClient(_settings, _outputChannelProvider.Object);
        var provider = new SigdnRhPessoasProvider(client);

        // Act
        var importKeys = await provider.GetPessoasByImportKeysAsync([], default);

        // Assert
        importKeys.Should().NotBeNull();
        importKeys.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSourceImportKeysAsync_ThrowsException_WhenSoapChannelThrows()
    {
        // Arrange
        _outputChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .ThrowsAsync(new InvalidOperationException("SOAP error"));

        _outputChannelProvider.Setup(f => f.CreateChannel()).Returns(_outputChannel.Object);
        var client = new ExternalPersonnelNumberClient(_settings, _outputChannelProvider.Object);
        var provider = new SigdnRhPessoasProvider(client);

        // Act
        Func<Task> act = async () => await provider.GetSourceImportKeysAsync(default);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*SOAP error*");
    }

    [Fact]
    public async Task GetSourceImportKeysAsync_PassesCorrectRequestToSoapChannel()
    {
        // Arrange
        ZhrWsGetPernrRequest? receivedRequest = null;
        _outputChannel
            .Setup(c => c.ZhrWsGetPernrAsync(It.IsAny<ZhrWsGetPernrRequest>()))
            .Callback<ZhrWsGetPernrRequest>(req => receivedRequest = req)
            .ReturnsAsync(new ZhrWsGetPernrResponse1
            {
                ZhrWsGetPernrResponse = new ZhrWsGetPernrResponse
                {
                    Output = []
                }
            });

        _outputChannelProvider.Setup(f => f.CreateChannel()).Returns(_outputChannel.Object);
        var client = new ExternalPersonnelNumberClient(_settings, _outputChannelProvider.Object);
        var provider = new SigdnRhPessoasProvider(client);

        // Act
        await provider.GetSourceImportKeysAsync(default);

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
        _outputChannel = null!;
        _outputChannelProvider = null!;
        GC.SuppressFinalize(this);
    }
}