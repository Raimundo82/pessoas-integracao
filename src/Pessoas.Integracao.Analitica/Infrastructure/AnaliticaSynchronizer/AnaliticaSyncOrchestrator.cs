using Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;
using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer;

public sealed class AnaliticaSyncOrchestrator(IEnumerable<IAnaliticaSynchronizer> syncronizers)
{
    public async Task ExecuteAsync(IZhrOutput input, CancellationToken ct)
    {
        var tasks = syncronizers.Select(synchronizer => synchronizer.SyncAsync(input, ct));
        await Task.WhenAll(tasks);
    }
}
