using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using FluentAssertions;

using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;
using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Generated.Output;


namespace Pessoas.Integracao.Worker.Tests.Unit;

public sealed class SoapChannelFactoryTests
{
    [Fact]
    public void CreateDefaultBinding_ReturnsCustomBindingWithExpectedElements()
    {
        // Act
        var binding = SoapChannelFactory.CreateDefaultBinding();

        // Assert
        binding.Should().BeOfType<CustomBinding>();
        binding.Elements.Should().ContainSingle(e => e is TextMessageEncodingBindingElement);
        binding.Elements.Should().ContainSingle(e => e is HttpTransportBindingElement);
    }

    [Fact]
    public void CreateChannelFactory_SetsEndpointAddressAndBehaviors()
    {
        // Arrange
        var endpoint = "http://fake/service";
        var behavior = new MockEndpointBehavior();

        // Act
        var factory = SoapChannelFactory.CreateChannelFactory<zhr_wsChannel>(endpoint, behavior);

        // Assert
        factory.Endpoint.Address.Uri.ToString().Should().Be(endpoint);
        factory.Endpoint.EndpointBehaviors.Should().Contain(behavior);
    }

    // Dummy behavior for testing
    public class MockEndpointBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

        public void Validate(ServiceEndpoint endpoint) { }
    }
}