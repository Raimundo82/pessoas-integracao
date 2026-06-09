using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients.Aptidao;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Correlation;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.Unit.Clients;

public sealed class AptidaoClientUnitTests : IDisposable
{
    private IOptions<DataSourceSettings> _settings;
    private Mock<zhr_wsChannel> _soapChannel;
    private Mock<ISoapChannelProvider<zhr_wsChannel>> _soapChannelProvider;
    private Mock<ISoapResultCorrelator> _soapResultCorrelator;
    private readonly CancellationToken _ct = TestContext.Current.CancellationToken;

    public AptidaoClientUnitTests()
    {
        _settings = Options.Create(new DataSourceSettings { Empresa = "1000" });
        _soapChannel = new Mock<zhr_wsChannel>();
        _soapChannelProvider = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
        _soapResultCorrelator = new Mock<ISoapResultCorrelator>();
    }

    [Fact]
    public async Task ShouldReturnEmptyDict_WhenImportKeyListIsEmpty()
    {
        // Arrange
        var client = new AptidaoClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetAptidaoAsync([], _ct);

        // Assert
        result.Should().BeEmpty();
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Never);
        _soapChannel.Verify(c => c.ZhrWsAptidaoAsync(It.IsAny<ZhrWsAptidaoRequest>()), Times.Never);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<IReadOnlyList<PessoaImportKey>>(),
            It.IsAny<ZhrSAptidaoOutput[]>(),
            It.IsAny<Func<ZhrSAptidaoOutput, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task ShouldReturnExpectedOutput_WhenImportKeyListHasOneItem()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("22600", "30002696") };
        var soapOutput = new[]
        {
            new ZhrSAptidaoOutput { Ni = "22600", Numsap = "30002696" }
        };

        var correlated = new Dictionary<PessoaImportKey, ZhrSAptidaoOutput?>
        {
            { importKeys[0], soapOutput[0] }
        };

        _soapChannel
            .Setup(c => c.ZhrWsAptidaoAsync(It.IsAny<ZhrWsAptidaoRequest>()))
            .ReturnsAsync(new ZhrWsAptidaoResponse1
            {
                ZhrWsAptidaoResponse = new ZhrWsAptidaoResponse { Output = soapOutput }
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
            It.IsAny<IReadOnlyList<PessoaImportKey>>(),
            It.IsAny<ZhrSAptidaoOutput[]>(),
            It.IsAny<Func<ZhrSAptidaoOutput, string>>())).Returns(correlated);

        var client = new AptidaoClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetAptidaoAsync(importKeys, _ct);

        // Assert
        result.Should().BeEquivalentTo(correlated);
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapChannel.Verify(c => c.ZhrWsAptidaoAsync(It.IsAny<ZhrWsAptidaoRequest>()), Times.Once);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.Is<IReadOnlyList<PessoaImportKey>>(keys => keys.SequenceEqual(importKeys)),
            It.Is<ZhrSAptidaoOutput[]>(o => o.SequenceEqual(soapOutput)),
            It.IsAny<Func<ZhrSAptidaoOutput, string>>()),
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
            new ZhrSAptidaoOutput { Ni = "22600", Numsap = "30002696" },
            new ZhrSAptidaoOutput { Ni = "22700", Numsap = "30002697" }
        };

        var correlated = new Dictionary<PessoaImportKey, ZhrSAptidaoOutput?>
        {
            { importKeys[0], soapOutput[0] },
            { importKeys[1], soapOutput[1] }
        };

        _soapChannel
            .Setup(c => c.ZhrWsAptidaoAsync(It.IsAny<ZhrWsAptidaoRequest>()))
            .ReturnsAsync(new ZhrWsAptidaoResponse1
            {
                ZhrWsAptidaoResponse = new ZhrWsAptidaoResponse { Output = soapOutput }
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
            It.IsAny<IReadOnlyList<PessoaImportKey>>(),
            It.IsAny<ZhrSAptidaoOutput[]>(),
            It.IsAny<Func<ZhrSAptidaoOutput, string>>())).Returns(correlated);

        var client = new AptidaoClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetAptidaoAsync(importKeys, _ct);

        // Assert
        result.Should().BeEquivalentTo(correlated);
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapChannel.Verify(c => c.ZhrWsAptidaoAsync(It.IsAny<ZhrWsAptidaoRequest>()), Times.Once);
    }

    [Fact]
    public async Task ShouldBuildRequestWithCorrectFields_WhenCallingGetDataByKeyAsync()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("22600", "30002696") };

        ZhrWsAptidaoRequest? captured = null;

        _soapChannel
            .Setup(c => c.ZhrWsAptidaoAsync(It.IsAny<ZhrWsAptidaoRequest>()))
            .Callback<ZhrWsAptidaoRequest>(r => captured = r)
            .ReturnsAsync(new ZhrWsAptidaoResponse1
            {
                ZhrWsAptidaoResponse = new ZhrWsAptidaoResponse { Output = [] }
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);
        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
            It.IsAny<IReadOnlyList<PessoaImportKey>>(),
            It.IsAny<ZhrSAptidaoOutput[]>(),
            It.IsAny<Func<ZhrSAptidaoOutput, string>>())).Returns([]);

        var client = new AptidaoClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        await client.GetAptidaoAsync(importKeys, _ct);

        // Assert
        captured.Should().NotBeNull();
        captured!.ZhrWsAptidao.Input.Should().ContainSingle();

        var input = captured.ZhrWsAptidao.Input[0];
        input.Ni.Should().Be("22600");
        input.Numsap.Should().Be("30002696");
        input.Empresa.Should().Be("1000");
    }

    [Fact]
    public async Task ShouldReturnEmptyDict_WhenSoapResponseOutputIsNull()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannel
            .Setup(c => c.ZhrWsAptidaoAsync(It.IsAny<ZhrWsAptidaoRequest>()))
            .ReturnsAsync(new ZhrWsAptidaoResponse1
            {
                ZhrWsAptidaoResponse = new ZhrWsAptidaoResponse { Output = null }
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
            It.IsAny<IReadOnlyList<PessoaImportKey>>(),
            null!,
            It.IsAny<Func<ZhrSAptidaoOutput, string>>())).Returns([]);

        var client = new AptidaoClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetAptidaoAsync(importKeys, _ct);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldPropagateException_WhenSoapClientThrows()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannel
            .Setup(c => c.ZhrWsAptidaoAsync(It.IsAny<ZhrWsAptidaoRequest>()))
            .ThrowsAsync(new Exception("SOAP error"));

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new AptidaoClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => client.GetAptidaoAsync(importKeys, _ct));
    }

    [Fact]
    public async Task ShouldPropagateException_WhenChannelCreationFails()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannelProvider.Setup(f => f.CreateChannel()).Throws(new Exception("Channel creation error"));

        var client = new AptidaoClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => client.GetAptidaoAsync(importKeys, _ct));
    }

    [Fact]
    public async Task ShouldPropagateCancellationToken_WhenExecutingGetDataByKeyAsync()
    {
        // Arrange
        var importKeys = new[] { new PessoaImportKey("00001", "00000001") };
        using var cts = new CancellationTokenSource();

        var soapCallStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        _soapChannel
            .Setup(c => c.ZhrWsAptidaoAsync(It.IsAny<ZhrWsAptidaoRequest>()))
            .Returns(() =>
            {
                soapCallStarted.TrySetResult();
                return new TaskCompletionSource<ZhrWsAptidaoResponse1>(TaskCreationOptions.RunContinuationsAsynchronously).Task;
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new AptidaoClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var task = client.GetAptidaoAsync(importKeys, cts.Token);

        await soapCallStarted.Task;
        await cts.CancelAsync();

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);

        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
            It.IsAny<IReadOnlyList<PessoaImportKey>>(),
            It.IsAny<ZhrSAptidaoOutput[]>(),
            It.IsAny<Func<ZhrSAptidaoOutput, string>>()),
            Times.Never);
    }

    public void Dispose()
    {
        _settings = null!;
        _soapChannel = null!;
        _soapChannelProvider = null!;
        _soapResultCorrelator = null!;
        GC.SuppressFinalize(this);
    }
}
