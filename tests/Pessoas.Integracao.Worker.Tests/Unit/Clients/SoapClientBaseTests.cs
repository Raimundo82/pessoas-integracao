using System.ServiceModel;

using FluentAssertions;

using Moq;

using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;

namespace Pessoas.Integracao.Worker.Tests.Unit.Clients;

public class SoapClientBaseTests
{
    [Fact]
    public async Task ShouldCloseChannel_WhenSuccessful()
    {
        // Arrange
        var channel = new Mock<zhr_wsChannel>();
        channel.SetupGet(c => c.State).Returns(CommunicationState.Opened);

        var provider = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
        provider.Setup(p => p.CreateChannel()).Returns(channel.Object);

        var client = new TestSoapClient(provider.Object);

        // Act
        var result = await client.RunAsync(_ => Task.FromResult(42));

        // Assert
        result.Should().Be(42);
        provider.Verify(p => p.CreateChannel(), Times.Once);
        channel.Verify(c => c.Close(), Times.Once);
        channel.Verify(c => c.Abort(), Times.Never);
    }

    [Fact]
    public async Task ShouldAbortAndNotCloseChannel_WhenThrows()
    {
        // Arrange
        var channel = new Mock<zhr_wsChannel>();
        channel.SetupGet(c => c.State).Returns(CommunicationState.Faulted);

        var provider = new Mock<ISoapChannelProvider<zhr_wsChannel>>();
        provider.Setup(p => p.CreateChannel()).Returns(channel.Object);

        var client = new TestSoapClient(provider.Object);

        // Act
        var act = () => client.RunAsync(_ => throw new InvalidOperationException("boom"));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        channel.Verify(c => c.Abort(), Times.Once);
        channel.Verify(c => c.Close(), Times.Never);
    }

    private sealed class TestSoapClient(ISoapChannelProvider<zhr_wsChannel> provider)
        : SoapBaseClient<zhr_wsChannel>(provider)
    {
        public Task<int> RunAsync(Func<zhr_wsChannel, Task<int>> action) => ExecuteAsync(action);
    }
}
