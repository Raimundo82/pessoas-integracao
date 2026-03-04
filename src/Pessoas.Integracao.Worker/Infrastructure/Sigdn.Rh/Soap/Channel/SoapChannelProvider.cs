using System.ServiceModel;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;

public class SoapChannelProvider<TChannel>(ChannelFactory<TChannel> channelFactory) : ISoapChannelProvider<TChannel>
{
    private readonly ChannelFactory<TChannel> _channelFactory = channelFactory;
    public TChannel CreateChannel() => _channelFactory.CreateChannel();
}