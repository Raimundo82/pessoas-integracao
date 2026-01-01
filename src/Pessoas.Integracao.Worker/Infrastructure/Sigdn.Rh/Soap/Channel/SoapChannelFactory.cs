using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;

public static class SoapChannelFactory
{
    public static CustomBinding CreateDefaultBinding()
    {
        return new CustomBinding(
            new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8),
            new HttpTransportBindingElement
            {
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                DecompressionEnabled = false,
                UseDefaultWebProxy = false
            });
    }

    public static ChannelFactory<T> CreateChannelFactory<T>(string endpointUrl, params IEndpointBehavior[] behaviors)
    {
        var factory = new ChannelFactory<T>(CreateDefaultBinding(), new EndpointAddress(endpointUrl));
        foreach (var behavior in behaviors ?? [])
            factory.Endpoint.EndpointBehaviors.Add(behavior);

        return factory;
    }
}