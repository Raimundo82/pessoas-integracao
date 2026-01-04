using System.ServiceModel;

using FluentAssertions;

using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.SigdnRhPessoasProviderTests;

public class SoapChannelProviderUnitTests
{
    [Fact]
    public void CreateChannel_ReturnsChannelInstance()
    {
        // Arrange
        var endpoint = "http://fake/service";
        var provider = new SoapChannelProvider<zhr_wsChannel>();

        // Act
        var channel = provider.CreateChannel(endpoint);

        // Assert
        channel.Should().NotBeNull();
        channel.Should().BeAssignableTo<zhr_wsChannel>();
        channel.State.Should().Be(CommunicationState.Created);
    }
}