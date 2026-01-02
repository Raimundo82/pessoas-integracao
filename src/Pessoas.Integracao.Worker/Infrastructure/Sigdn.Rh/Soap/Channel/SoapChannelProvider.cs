namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;

public class SoapChannelProvider<TChannel> : ISoapChannelProvider<TChannel>
{
    public TChannel CreateChannel(string endpointUrl)
    {
        return SoapChannelFactory.CreateChannelFactory<TChannel>(endpointUrl).CreateChannel();
    }
}