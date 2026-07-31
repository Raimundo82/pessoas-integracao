using Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;
using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer;

public sealed class AnaliticaSyncOrchestrator(IEnumerable<IAnaliticaSynchronizer> synchronizers)
{
    public async Task ExecuteAsync(IReadOnlyList<IZhrOutput> zhrOutputs, CancellationToken ct)
    {
        var tasks = synchronizers.Select(synchronizer => synchronizer.SyncAsync(zhrOutputs, ct));
        await Task.WhenAll(tasks);
    }
}
