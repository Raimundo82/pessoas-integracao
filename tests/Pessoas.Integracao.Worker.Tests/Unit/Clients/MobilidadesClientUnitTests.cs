using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients.Mobilidades;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Correlation;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.Unit.Clients;

public sealed class MobilidadesClientUnitTests : IDisposable
{
    private IOptions<DataSourceSettings> _settings;
    private Mock<zhr_wsChannel> _soapChannel;
    private Mock<ISoapChannelProvider<zhr_wsChannel>> _soapChannelProvider;
    private readonly Mock<ISoapResultCorrelator> _soapResultCorrelator;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public MobilidadesClientUnitTests()
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
        var importKeys = Array.Empty<PessoaImportKey>();

        var client = new MobilidadesClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetMobilidadesAsync(importKeys, _ct);

        // Assert
        result.Should().BeEmpty();
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Never);
        _soapChannel.Verify(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()), Times.Never);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<PessoaImportKey[]>(),
            It.IsAny<ZhrSMobilidadesOutput[]?>(),
            It.IsAny<Func<ZhrSMobilidadesOutput, string>>()),
        Times.Never);
    }

    [Fact]
    public async Task ShouldReturnExpectedOutput_WhenImportKeyListHasOneItem()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("22600", "30002696") };
        var soapOutput = new[] { new ZhrSMobilidadesOutput { Ni = "22600", Numsap = "30002696" } };
        var correlatedOutput = new Dictionary<PessoaImportKey, ZhrSMobilidadesOutput?> { { importKeys[0], soapOutput[0] } };

        _soapChannel
            .Setup(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()))
            .ReturnsAsync(new ZhrWsMobilidadesResponse1 { ZhrWsMobilidadesResponse = new ZhrWsMobilidadesResponse { Output = soapOutput } });
        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);
        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSMobilidadesOutput[]>(),
                It.IsAny<Func<ZhrSMobilidadesOutput, string>>()))
            .Returns(correlatedOutput);

        var client = new MobilidadesClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetMobilidadesAsync(importKeys, _ct);

        // Assert
        result.Should().BeEquivalentTo(correlatedOutput);
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapChannel.Verify(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()), Times.Once);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<PessoaImportKey[]>(),
            It.IsAny<ZhrSMobilidadesOutput[]>(),
            It.IsAny<Func<ZhrSMobilidadesOutput, string>>()),
        Times.Once);
    }

    [Fact]
    public async Task ShouldReturnMultipleItems_WhenImportKeyListHasMultipleItems()
    {
        // Arrange
        var importKeys = new[]
        {
            new PessoaImportKey("22600", "30002696"),
            new PessoaImportKey("22700", "30002697")
        };
        var soapOutput = new[]
        {
            new ZhrSMobilidadesOutput { Ni = "22600", Numsap = "30002696" },
            new ZhrSMobilidadesOutput { Ni = "22700", Numsap = "30002697" }
        };
        var correlatedOutput = new Dictionary<PessoaImportKey, ZhrSMobilidadesOutput?>
        {
            { importKeys[0], soapOutput[0] },
            { importKeys[1], soapOutput[1] }
        };

        _soapChannel
            .Setup(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()))
            .ReturnsAsync(new ZhrWsMobilidadesResponse1 { ZhrWsMobilidadesResponse = new ZhrWsMobilidadesResponse { Output = soapOutput } });
        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);
        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSMobilidadesOutput[]>(),
                It.IsAny<Func<ZhrSMobilidadesOutput, string>>()))
            .Returns(correlatedOutput);

        var client = new MobilidadesClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetMobilidadesAsync(importKeys, _ct);

        // Assert
        result.Should().BeEquivalentTo(correlatedOutput);
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapChannel.Verify(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()), Times.Once);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<PessoaImportKey[]>(),
            It.IsAny<ZhrSMobilidadesOutput[]>(),
            It.IsAny<Func<ZhrSMobilidadesOutput, string>>()),
        Times.Once);
    }

    [Fact]
    public async Task ShouldReturnEmptyDict_WhenSoapResponseOutputIsNull()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannel
            .Setup(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()))
            .ReturnsAsync(new ZhrWsMobilidadesResponse1 { ZhrWsMobilidadesResponse = new ZhrWsMobilidadesResponse { Output = null } });
        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);
        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSMobilidadesOutput[]>(),
                It.IsAny<Func<ZhrSMobilidadesOutput, string>>()))
            .Returns([]);

        var client = new MobilidadesClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetMobilidadesAsync(importKeys, _ct);

        // Assert
        result.Should().BeEmpty();
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapChannel.Verify(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()), Times.Once);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<PessoaImportKey[]>(),
            It.IsAny<ZhrSMobilidadesOutput[]>(),
            It.IsAny<Func<ZhrSMobilidadesOutput, string>>()),
        Times.Once);
    }

    [Fact]
    public async Task ShouldReturnEmptyDict_WhenSoapResponseBodyIsNull()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannel
            .Setup(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()))
            .ReturnsAsync(new ZhrWsMobilidadesResponse1 { ZhrWsMobilidadesResponse = null! });

        _soapChannelProvider
            .Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        _soapResultCorrelator
            .Setup(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSMobilidadesOutput[]?>(),
                It.IsAny<Func<ZhrSMobilidadesOutput, string>>()))
            .Returns([]);

        var client = new MobilidadesClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetMobilidadesAsync(importKeys, _ct);

        // Assert
        result.Should().BeEmpty();
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapChannel.Verify(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()), Times.Once);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<PessoaImportKey[]>(),
            It.IsAny<ZhrSMobilidadesOutput[]?>(),
            It.IsAny<Func<ZhrSMobilidadesOutput, string>>()),
        Times.Once);
    }

    [Fact]
    public async Task ShouldBuildRequestWithCorrectFields_WhenCallingGetMobilidadesAsync()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("22600", "30002696") };
        _settings = Options.Create(new DataSourceSettings { Empresa = "1000" });

        ZhrWsMobilidadesRequest? capturedRequest = null;
        _soapChannel
            .Setup(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()))
            .Callback<ZhrWsMobilidadesRequest>(r => capturedRequest = r)
            .ReturnsAsync(new ZhrWsMobilidadesResponse1 { ZhrWsMobilidadesResponse = new ZhrWsMobilidadesResponse { Output = [] } });
        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);
        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSMobilidadesOutput[]>(),
                It.IsAny<Func<ZhrSMobilidadesOutput, string>>()))
            .Returns([]);

        var client = new MobilidadesClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        await client.GetMobilidadesAsync(importKeys, _ct);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.ZhrWsMobilidades.Input.Should().ContainSingle();
        capturedRequest.ZhrWsMobilidades.Input[0].Ni.Should().Be("22600");
        capturedRequest.ZhrWsMobilidades.Input[0].Numsap.Should().Be("30002696");
        capturedRequest.ZhrWsMobilidades.Input[0].Empresa.Should().Be("1000");
    }

    [Fact]
    public async Task ShouldPropagateException_WhenSoapClientThrows()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannel
            .Setup(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()))
            .ThrowsAsync(new Exception("SOAP client error"));
        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new MobilidadesClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => client.GetMobilidadesAsync(importKeys, _ct));
    }

    [Fact]
    public async Task ShouldPropagateException_WhenChannelCreationFails()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannelProvider.Setup(f => f.CreateChannel()).Throws(new Exception("Channel creation error"));

        var client = new MobilidadesClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => client.GetMobilidadesAsync(importKeys, _ct));
    }

    [Fact]
    public async Task ShouldPropagateCancellationToken_WhenExecutingGetMobilidadesAsync()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("00001", "00000001") };
        using var cancellationTokenSource = new CancellationTokenSource();
        var soapCallStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _soapChannel
            .Setup(c => c.ZhrWsMobilidadesAsync(It.IsAny<ZhrWsMobilidadesRequest>()))
            .Returns(() =>
            {
                soapCallStarted.TrySetResult();
                return new TaskCompletionSource<ZhrWsMobilidadesResponse1>(TaskCreationOptions.RunContinuationsAsynchronously).Task;
            });
        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new MobilidadesClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var getMobilidadesTask = client.GetMobilidadesAsync(importKeys, cancellationTokenSource.Token);
        await soapCallStarted.Task;
        await cancellationTokenSource.CancelAsync();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => getMobilidadesTask);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<PessoaImportKey[]>(),
            It.IsAny<ZhrSMobilidadesOutput[]?>(),
            It.IsAny<Func<ZhrSMobilidadesOutput, string>>()),
        Times.Never);
    }

    public void Dispose()
    {
        _settings = null!;
        _soapChannel = null!;
        _soapChannelProvider = null!;
        GC.SuppressFinalize(this);
    }
}
