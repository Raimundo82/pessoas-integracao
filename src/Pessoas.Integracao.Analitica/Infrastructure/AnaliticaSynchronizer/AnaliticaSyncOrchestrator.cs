using Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer.Synchronizers;
using Pessoas.Integracao.Sync.Application.Contracts;

namespace Pessoas.Integracao.Analitica.Infrastructure.AnaliticaSynchronizer;

public sealed class AnaliticaSyncOrchestrator(IEnumerable<IAnaliticaSynchronizer> synchronizers)
{
    public async Task ExecuteAsync(IReadOnlyList<IZhrOutput> inputs, CancellationToken ct)
    {
        var tasks = synchronizers.Select(synchronizer => synchronizer.SyncAsync(inputs, ct));
        await Task.WhenAll(tasks);
    }
}
