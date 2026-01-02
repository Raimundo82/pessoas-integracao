namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;


public interface ISoapChannelProvider<out TChannel>
{
    TChannel CreateChannel();
}