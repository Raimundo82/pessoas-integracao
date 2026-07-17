using Pessoas.Integracao.Analitica.Infrastructure.SyncHandlers;
using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Application.UseCases;

public sealed class SyncAnaliticaData(IEnumerable<IZhrCollectionSyncHandler> syncHandlers)
{
    public async Task ExecuteAsync(ZhrOutput input, CancellationToken ct)
    {
        foreach (var handler in syncHandlers)
        {
            await handler.SyncAsync(input, ct);
        }
    }
}
