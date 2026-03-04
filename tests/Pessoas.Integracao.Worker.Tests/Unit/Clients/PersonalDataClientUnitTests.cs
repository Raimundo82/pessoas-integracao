using FluentAssertions;

using Microsoft.Extensions.Options;

using Moq;

using Pessoas.Integracao.Core.Application.Models;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Configuration;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.Unit.Clients;

public sealed class PersonalDataClientUnitTests : IDisposable
{
    private IOptions<DataSourceSettings> _settings;
    private Mock<zhr_wsChannel> _soapChannel;
    private Mock<ISoapChannelProvider<zhr_wsChannel>> _soapChannelProvider;

    public PersonalDataClientUnitTests()
    {
        _settings = Options.Create(new DataSourceSettings { });
        _soapChannel = new Mock<zhr_wsChannel>();
        _soapChannelProvider = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenImportKeyListIsEmpty()
    {
        // Arrange
        var personImportKeys = Array.Empty<PessoaImportKey>();
        var expectedOutput = Array.Empty<ZhrSPessoaisOutput>();

        _soapChannel
            .Setup(c => c.ZhrWsPersonalDataAsync(It.IsAny<ZhrWsPersonalDataRequest>()))
            .ReturnsAsync(new ZhrWsPersonalDataResponse1
            {
                ZhrWsPersonalDataResponse = new ZhrWsPersonalDataResponse
                {
                    Output = expectedOutput
                }
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object);

        // Act
        var result = await client.GetPersonalDataAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }


    [Fact]
    public async Task ShouldReturnOneItem_WhenImportKeyListHasOneItem()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("22600", "30002696") };
        var expectedOutput = new[]
        {
            new ZhrSPessoaisOutput { Ni = "22600", Numsap = "30002696" }
        };

        _soapChannel
            .Setup(c => c.ZhrWsPersonalDataAsync(It.IsAny<ZhrWsPersonalDataRequest>()))
            .ReturnsAsync(new ZhrWsPersonalDataResponse1
            {
                ZhrWsPersonalDataResponse = new ZhrWsPersonalDataResponse
                {
                    Output = expectedOutput
                }
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object);

        // Act
        var result = await client.GetPersonalDataAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedOutput);
    }

    [Fact]
    public async Task ShouldReturnMultipleItems_WhenImportKeyListHasMultipleItems()
    {
        // Arrange
        var personImportKeys = new[] {
            new PessoaImportKey("22600", "30002696"),
            new PessoaImportKey("22700", "30002697")
        };
        var expectedOutput = new[]
        {
            new ZhrSPessoaisOutput { Ni = "22600", Numsap = "30002696" },
            new ZhrSPessoaisOutput { Ni = "22700", Numsap = "30002697" }
        };

        _soapChannel
            .Setup(c => c.ZhrWsPersonalDataAsync(It.IsAny<ZhrWsPersonalDataRequest>()))
            .ReturnsAsync(new ZhrWsPersonalDataResponse1
            {
                ZhrWsPersonalDataResponse = new ZhrWsPersonalDataResponse
                {
                    Output = expectedOutput
                }
            });

        _soapChannelProvider.Setup(f => f.CreateChannel()).Returns(_soapChannel.Object);

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object);

        // Act
        var result = await client.GetPersonalDataAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedOutput);
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenSoapResponseOutputIsNul()
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

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object);

        // Act
        var result = await client.GetPersonalDataAsync(personImportKeys, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
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

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => client.GetPersonalDataAsync(personImportKeys, CancellationToken.None));
    }

    [Fact]
    public async Task ShouldPropagateException_WhenChannelCreationFails()
    {
        // Arrange
        var personImportKeys = new[] { new PessoaImportKey("00001", "00000001") };

        _soapChannelProvider.Setup(f => f.CreateChannel()).Throws(new Exception("Channel creation error"));

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object);

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

        var client = new PersonalDataClient(_settings, _soapChannelProvider.Object);

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