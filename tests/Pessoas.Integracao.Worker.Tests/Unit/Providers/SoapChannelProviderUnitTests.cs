using System.ServiceModel;

using FluentAssertions;

using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.Unit.Providers;

public class SoapChannelProviderUnitTests
{
    [Fact]
    public void CreateChannel_ReturnsChannelInstance()
    {
        // Arrange
        var factory = SoapChannelFactory.CreateChannelFactory<zhr_wsChannel>("http://fake/service");
        var provider = new SoapChannelProvider<zhr_wsChannel>(factory);

        // Act
        var channel = provider.CreateChannel();

        // Assert
        channel.Should().NotBeNull();
        channel.Should().BeAssignableTo<zhr_wsChannel>();
        channel.State.Should().Be(CommunicationState.Created);
    }
}