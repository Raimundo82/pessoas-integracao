using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Correlation;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.Unit.Clients;

public sealed class ExamesMedClientUnitTests : IDisposable
{
    private IOptions<DataSourceSettings> _settings;
    private Mock<zhr_wsChannel> _soapChannel;
    private Mock<ISoapChannelProvider<zhr_wsChannel>> _soapChannelProvider;
    private readonly Mock<ISoapResultCorrelator> _soapResultCorrelator;

    public ExamesMedClientUnitTests()
    {
        _settings = Options.Create(new DataSourceSettings { });
        _soapChannel = new Mock<zhr_wsChannel>();
        _soapChannelProvider = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
        _soapResultCorrelator = new Mock<ISoapResultCorrelator>();
    }

    [Fact]
    public async Task ShouldReturnEmptyDict_WhenImportKeyListIsEmpty()
    {
        // Arrange
        var personImportKeys = Array.Empty<PessoaImportKey>();
        var soapOutput = Array.Empty<ZhrSExamesMedOutput>();

        _soapChannel
            .Setup(c => c.ZhrWsExamesMedAsync(It.IsAny<ZhrWsExamesMedRequest>()))
            .ReturnsAsync(new ZhrWsExamesMedResponse1 { ZhrWsExamesMedResponse = new ZhrWsExamesMedResponse { Output = soapOutput } });
        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new ExamesMedClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetExamesMedAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Never);
        _soapChannel.Verify(c => c.ZhrWsExamesMedAsync(It.IsAny<ZhrWsExamesMedRequest>()), Times.Never);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<PessoaImportKey[]>(),
            It.IsAny<ZhrSExamesMedOutput[]?>(),
            It.IsAny<Func<ZhrSExamesMedOutput, string>>()),
        Times.Never);
    }

    [Fact]
    public async Task ShouldReturnExpectedOutput_WhenImportKeyListHasOneItem()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("22600", "30002696") };
        var soapOutput = new[]
        {
            new ZhrSExamesMedOutput { Ni = "22600", Numsap = "30002696" }
        };
        var correlatedOutput = new Dictionary<PessoaImportKey, ZhrSExamesMedOutput?> { { personImportKeys[0], soapOutput[0] } };

        _soapChannel
            .Setup(c => c.ZhrWsExamesMedAsync(It.IsAny<ZhrWsExamesMedRequest>()))
            .ReturnsAsync(new ZhrWsExamesMedResponse1 { ZhrWsExamesMedResponse = new ZhrWsExamesMedResponse { Output = soapOutput } });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);
        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSExamesMedOutput[]>(),
                It.IsAny<Func<ZhrSExamesMedOutput, string>>()))
            .Returns(correlatedOutput);

        var client = new ExamesMedClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetExamesMedAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(correlatedOutput);
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapChannel.Verify(c => c.ZhrWsExamesMedAsync(It.IsAny<ZhrWsExamesMedRequest>()), Times.Once);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<PessoaImportKey[]>(),
            It.IsAny<ZhrSExamesMedOutput[]>(),
            It.IsAny<Func<ZhrSExamesMedOutput, string>>()),
        Times.Once);
    }

    [Fact]
    public async Task ShouldReturnMultipleItems_WhenImportKeyListHasMultipleItems()
    {
        // Arrange
        var personImportKeys = new[] {
            new PessoaImportKey("22600", "30002696"),
            new PessoaImportKey("22700", "30002697")
        };
        var soapOutput = new[]
        {
            new ZhrSExamesMedOutput { Ni = "22600", Numsap = "30002696" },
            new ZhrSExamesMedOutput { Ni = "22700", Numsap = "30002697" }
        };

        var correlatedOutput = new Dictionary<PessoaImportKey, ZhrSExamesMedOutput?>
        {
            { personImportKeys[0], soapOutput[0] },
            { personImportKeys[1], soapOutput[1] }
        };

        _soapChannel
            .Setup(c => c.ZhrWsExamesMedAsync(It.IsAny<ZhrWsExamesMedRequest>()))
            .ReturnsAsync(new ZhrWsExamesMedResponse1 { ZhrWsExamesMedResponse = new ZhrWsExamesMedResponse { Output = soapOutput } });
        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);
        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSExamesMedOutput[]>(),
                It.IsAny<Func<ZhrSExamesMedOutput, string>>()))
            .Returns(correlatedOutput);

        var client = new ExamesMedClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetExamesMedAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(correlatedOutput);
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapChannel.Verify(c => c.ZhrWsExamesMedAsync(It.IsAny<ZhrWsExamesMedRequest>()), Times.Once);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<PessoaImportKey[]>(),
            It.IsAny<ZhrSExamesMedOutput[]>(),
            It.IsAny<Func<ZhrSExamesMedOutput, string>>()),
        Times.Once);
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenSoapResponseOutputIsNul()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannel
            .Setup(c => c.ZhrWsExamesMedAsync(It.IsAny<ZhrWsExamesMedRequest>()))
            .ReturnsAsync(new ZhrWsExamesMedResponse1 { ZhrWsExamesMedResponse = new ZhrWsExamesMedResponse { Output = null } });
        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);
        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSExamesMedOutput[]>(),
                It.IsAny<Func<ZhrSExamesMedOutput, string>>()))
            .Returns([]);

        var client = new ExamesMedClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetExamesMedAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapChannel.Verify(c => c.ZhrWsExamesMedAsync(It.IsAny<ZhrWsExamesMedRequest>()), Times.Once);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<PessoaImportKey[]>(),
            It.IsAny<ZhrSExamesMedOutput[]>(),
            It.IsAny<Func<ZhrSExamesMedOutput, string>>()),
        Times.Once);
    }

    [Fact]
    public async Task ShouldPropagateException_WhenSoapClientThrows()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannel
            .Setup(c => c.ZhrWsExamesMedAsync(It.IsAny<ZhrWsExamesMedRequest>()))
            .ThrowsAsync(new Exception("SOAP client error"));

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new ExamesMedClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => client.GetExamesMedAsync(personImportKeys, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldPropagateException_WhenChannelCreationFails()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannelProvider.Setup(f => f.CreateChannel()).Throws(new Exception("Channel creation error"));

        var client = new ExamesMedClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => client.GetExamesMedAsync(personImportKeys, CancellationToken.None));
    }


    [Fact]
    public async Task ShouldPropagateCancellationToken_WhenExecutingGetExamesMedAsync()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("00001", "00000001") };
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _soapChannel
            .Setup(c => c.ZhrWsExamesMedAsync(It.IsAny<ZhrWsExamesMedRequest>()))
            .Returns(async () =>
            {
                await Task.Delay(100);
                cancellationToken.ThrowIfCancellationRequested();
                return new ZhrWsExamesMedResponse1
                {
                    ZhrWsExamesMedResponse = new ZhrWsExamesMedResponse
                    {
                        Output = []
                    }
                };
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new ExamesMedClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var getPersonalDataTask = client.GetExamesMedAsync(personImportKeys, cancellationTokenSource.Token);
        await cancellationTokenSource.CancelAsync();

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => getPersonalDataTask);
    }


    public void Dispose()
    {
        _settings = null!;
        _soapChannel = null!;
        _soapChannelProvider = null!;
        GC.SuppressFinalize(this);
    }
}
