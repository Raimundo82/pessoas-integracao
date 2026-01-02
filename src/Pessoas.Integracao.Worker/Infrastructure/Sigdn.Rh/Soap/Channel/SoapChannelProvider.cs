namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;

public class SoapChannelProvider<TChannel>(string endpointUrl) : ISoapChannelProvider<TChannel>
{
    private readonly string _endpointUrl = endpointUrl;
    public TChannel CreateChannel()
    {
        return SoapChannelFactory.CreateChannelFactory<TChannel>(_endpointUrl).CreateChannel();
    }
}