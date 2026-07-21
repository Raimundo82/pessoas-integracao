using Pessoas.Integracao.Analitica.Infrastructure.Strategies;
using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Application.UseCases;

public sealed class SyncAnaliticaCollections(IEnumerable<ICollectionSyncStrategy> strategies)
{
    public async Task ExecuteAsync(ZhrOutput input, CancellationToken ct)
    {
        foreach (var strategy in strategies)
        {
            await strategy.SyncAsync(input, ct);
        }
    }
}
