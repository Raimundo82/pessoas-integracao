
using System.ServiceModel;

using Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Channel;

namespace Pessoas.Integracao.Worker.Infrastructure.Sigdn.Rh.Soap.Clients;

public abstract class SoapBaseClient<TChannel>(ISoapChannelProvider<TChannel> provider)
    where TChannel : class, ICommunicationObject
{
    private readonly ISoapChannelProvider<TChannel> _provider = provider;

    protected async Task<TResult> ExecuteAsync<TResult>(
        Func<TChannel, Task<TResult>> action)
    {
        var channel = _provider.CreateChannel();
        try
        {
            return await action(channel);
        }
        catch
        {
            channel.Abort();
            throw;
        }
        finally
        {
            if (channel.State != CommunicationState.Faulted)
                channel.Close();
        }
    }
}

