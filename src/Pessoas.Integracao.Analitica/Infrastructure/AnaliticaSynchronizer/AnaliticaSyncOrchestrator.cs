using Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;
using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer;

public sealed class AnaliticaSyncOrchestrator(IEnumerable<IAnaliticaSynchronizer> syncronizers)
{
    public async Task ExecuteAsync(ZhrOutput input, CancellationToken ct)
    {
        foreach (var synchronizer in syncronizers)
        {
            await synchronizer.SyncAsync(input, ct);
        }
    }
}
