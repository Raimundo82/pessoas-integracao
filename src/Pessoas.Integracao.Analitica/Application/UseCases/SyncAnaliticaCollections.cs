using Pessoas.Integracao.Analitica.Infrastructure.Strategies;
using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Application.UseCases;

public sealed class SyncAnaliticaCollections(IEnumerable<ICollectionSyncStrategy> syncHandlers)
{
    public async Task ExecuteAsync(ZhrOutput input, CancellationToken ct)
    {
        foreach (var handler in syncHandlers)
        {
            await handler.SyncAsync(input, ct);
        }
    }
}
