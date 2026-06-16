using System.ServiceModel;

namespace Pessoas.Integracao.Sync.Infrastructure.Factories;

public interface IZhrWsGenericClientFactory<TClient, TChannel>
    where TClient : ClientBase<TChannel>
    where TChannel : class
{
    TClient CreateClient();
}
