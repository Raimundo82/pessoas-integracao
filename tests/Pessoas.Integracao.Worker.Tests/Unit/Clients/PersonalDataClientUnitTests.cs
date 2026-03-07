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

public sealed class PersonalDataClientUnitTests : IDisposable
{
    private IOptions<DataSourceSettings> _settings;
    private Mock<zhr_wsChannel> _soapChannel;
    private Mock<ISoapChannelProvider<zhr_wsChannel>> _soapChannelProvider;
    private readonly Mock<ISoapResultCorrelator> _soapResultCorrelator;

    public PersonalDataClientUnitTests()
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

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetPersonalDataAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Never);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSPessoaisOutput[]>(),
                It.IsAny<Func<ZhrSPessoaisOutput, string>>())
                , Times.Never);
    }


    [Fact]
    public async Task ShouldReturnExpectedOutput_WhenImportKeysHasOneItem()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("22600", "30002696") };
        var soapOutput = new[]
        {
            new ZhrSPessoaisOutput { Ni = "22600", Numsap = "30002696" }
        };

        var correlatedOutput = new Dictionary<PessoaImportKey, ZhrSPessoaisOutput?> { { personImportKeys[0], soapOutput[0] } };

        _soapChannel
            .Setup(c => c.ZhrWsPersonalDataAsync(It.IsAny<ZhrWsPersonalDataRequest>()))
            .ReturnsAsync(new ZhrWsPersonalDataResponse1 { ZhrWsPersonalDataResponse = new ZhrWsPersonalDataResponse { Output = soapOutput } });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);
        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSPessoaisOutput[]>(),
                It.IsAny<Func<ZhrSPessoaisOutput, string>>()))
            .Returns(correlatedOutput);

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetPersonalDataAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(correlatedOutput);
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
                It.Is<PessoaImportKey[]>(keys => keys.SequenceEqual(personImportKeys)),
                It.Is<ZhrSPessoaisOutput[]>(output => output.SequenceEqual(soapOutput)),
                It.IsAny<Func<ZhrSPessoaisOutput, string>>()), Times.Once);
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
            new ZhrSPessoaisOutput { Ni = "22600", Numsap = "30002696" },
            new ZhrSPessoaisOutput { Ni = "22700", Numsap = "30002697" }
        };

        var correlatedOutput = new Dictionary<PessoaImportKey, ZhrSPessoaisOutput?>
        {
            { personImportKeys[0], soapOutput[0] },
            { personImportKeys[1], soapOutput[1] }
        };

        _soapChannel
            .Setup(c => c.ZhrWsPersonalDataAsync(It.IsAny<ZhrWsPersonalDataRequest>()))
            .ReturnsAsync(new ZhrWsPersonalDataResponse1 { ZhrWsPersonalDataResponse = new ZhrWsPersonalDataResponse { Output = soapOutput } });

        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSPessoaisOutput[]>(),
                It.IsAny<Func<ZhrSPessoaisOutput, string>>()))
            .Returns(correlatedOutput);

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetPersonalDataAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(correlatedOutput);
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
                It.Is<PessoaImportKey[]>(keys => keys.SequenceEqual(personImportKeys)),
                It.Is<ZhrSPessoaisOutput[]>(output => output.SequenceEqual(soapOutput)),
                It.IsAny<Func<ZhrSPessoaisOutput, string>>()), Times.Once);
    }

    [Fact]
    public async Task ShouldReturnEmptyDict_WhenSoapResponseOutputIsNul()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannel
            .Setup(c => c.ZhrWsPersonalDataAsync(It.IsAny<ZhrWsPersonalDataRequest>()))
            .ReturnsAsync(new ZhrWsPersonalDataResponse1
            {
                ZhrWsPersonalDataResponse = new ZhrWsPersonalDataResponse
                {
                    Output = null
                }
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        _soapResultCorrelator.Setup(c => c.CorrelateByKey(
                It.IsAny<PessoaImportKey[]>(),
                It.IsAny<ZhrSPessoaisOutput[]>(),
                It.IsAny<Func<ZhrSPessoaisOutput, string>>()))
            .Returns([]);

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var result = await client.GetPersonalDataAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _soapChannelProvider.Verify(f => f.CreateChannel(), Times.Once);
        _soapResultCorrelator.Verify(c => c.CorrelateByKey(
                It.Is<PessoaImportKey[]>(keys => keys.SequenceEqual(personImportKeys)),
                It.Is<ZhrSPessoaisOutput[]>(output => output == null),
                It.IsAny<Func<ZhrSPessoaisOutput, string>>()), Times.Once);
    }

    [Fact]
    public async Task ShouldPropagateException_WhenSoapClientThrows()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannel
            .Setup(c => c.ZhrWsPersonalDataAsync(It.IsAny<ZhrWsPersonalDataRequest>()))
            .ThrowsAsync(new Exception("SOAP client error"));

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => client.GetPersonalDataAsync(personImportKeys, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldPropagateException_WhenChannelCreationFails()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannelProvider.Setup(f => f.CreateChannel()).Throws(new Exception("Channel creation error"));

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => client.GetPersonalDataAsync(personImportKeys, CancellationToken.None));
    }


    [Fact]
    public async Task ShouldPropagateCancellationToken_WhenExecutingGetPersonalDataAsync()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("00001", "00000001") };
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _soapChannel
            .Setup(c => c.ZhrWsPersonalDataAsync(It.IsAny<ZhrWsPersonalDataRequest>()))
            .Returns(async () =>
            {
                await Task.Delay(100); // Simulate some delay
                cancellationToken.ThrowIfCancellationRequested();
                return new ZhrWsPersonalDataResponse1
                {
                    ZhrWsPersonalDataResponse = new ZhrWsPersonalDataResponse
                    {
                        Output = []
                    }
                };
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object, _soapResultCorrelator.Object);

        // Act
        var getPersonalDataTask = client.GetPersonalDataAsync(personImportKeys, cancellationTokenSource.Token);
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